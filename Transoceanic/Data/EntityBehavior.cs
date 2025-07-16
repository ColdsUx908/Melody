using System.Collections;
using MonoMod.Utils;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.GameInput;
using Transoceanic.RuntimeEditing;

namespace Transoceanic.Data;

#region Base
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class CriticalBehaviorAttribute : Attribute { }

public abstract class EntityBehavior<TEntity> where TEntity : Entity
{
    public abstract Mod Mod { get; }

    /// <summary>
    /// 优先级，越大越先应用。
    /// </summary>
    public virtual decimal Priority { get; } = 0m;

    /// <summary>
    /// 将指定实体连接到Behavior实例。
    /// </summary>
    public virtual void Connect(TEntity entity) { }

    /// <summary>
    /// Allows you to modify the properties after initial loading has completed.
    /// </summary>
    public virtual void SetStaticDefaults() { }
}

public abstract class GeneralEntityBehavior<TEntity> : EntityBehavior<TEntity> where TEntity : Entity { }

public abstract class GlobalEntityBehavior<TEntity> : GeneralEntityBehavior<TEntity> where TEntity : Entity
{
    /// <summary>
    /// <inheritdoc/><para/>
    /// 此方法不应被调用，调用时抛出异常。
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public sealed override void Connect(TEntity entity) => throw new NotSupportedException();
}

public abstract class SingleEntityBehavior<TEntity> : EntityBehavior<TEntity> where TEntity : Entity
{
    public abstract int ApplyingType { get; }

    public virtual bool ShouldProcess => true;
}

public class GeneralEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : GeneralEntityBehavior<TEntity>
{
    private bool _initialized = false;
    private readonly Dictionary<string, List<TBehavior>> _data = [];

    public void FillSet() => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>());

    public void FillSet<T>() where T : Mod => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>().Where(b => b.Mod is T));

    public void FillSet(Assembly assemblyToSearch) => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch));

    private void Initialize(IEnumerable<TBehavior> behaviors)
    {
        foreach ((TBehavior behavior, HashSet<string> behaviorMethods) in
            behaviors.OrderByDescending(b => b.Priority).Select(b =>
            {
                Type type = b.GetType();
                return (b, (type.HasAttribute<CriticalBehaviorAttribute>() ? type.GetMethodNamesExceptObject(TOReflectionUtils.UniversalBindingFlags) : type.GetOverrideMethodNames(TOReflectionUtils.UniversalBindingFlags)).ToHashSet());
            }))
        {
            foreach (string method in behaviorMethods)
            {
                if (_data.TryGetValue(method, out List<TBehavior> behaviorList))
                    _data[method].Add(behavior);
                else
                    _data[method] = [behavior];
            }
        }
        if (_initialized)
        {
            foreach (string methodName in _data.Keys)
                _data[methodName] = [.. _data[methodName].Distinct().OrderByDescending(b => b.Priority)];
        }
        _initialized = true;
    }

    public List<TBehavior> GetBehaviors([CallerMemberName] string methodName = null!)
    {
        if (_data.TryGetValue(methodName, out List<TBehavior> behaviors))
            return behaviors;
        return [];
    }

    public IEnumerable<T> GetBehaviors<T>([CallerMemberName] string methodName = null!) where T : TBehavior
    {
        if (_data.TryGetValue(methodName, out List<TBehavior> behaviors))
        {
            foreach (TBehavior behavior in behaviors)
            {
                if (behavior is T typedBehavior)
                    yield return typedBehavior;
            }
        }
    }

    public IEnumerable<TBehavior> GetBehaviors(TEntity entity, [CallerMemberName] string methodName = null!)
    {
        if (_data.TryGetValue(methodName, out List<TBehavior> behaviors))
        {
            foreach (TBehavior behavior in behaviors)
            {
                behavior.Connect(entity);
                yield return behavior;
            }
        }
    }

    public IEnumerable<T> GetBehaviors<T>(TEntity entity, [CallerMemberName] string methodName = null!) where T : TBehavior
    {
        if (_data.TryGetValue(methodName, out List<TBehavior> behaviors))
        {
            foreach (TBehavior behavior in behaviors)
            {
                if (behavior is T typedBehavior)
                {
                    typedBehavior.Connect(entity);
                    yield return typedBehavior;
                }
            }
        }
    }

    public void Clear() => _data.Clear();
}

public class GlobalEntityBehaviorSet<TEntity, TBehavior> : GeneralEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : GeneralEntityBehavior<TEntity>
{ }

public class SingleEntityBehaviorSet<TEntity, TBehavior> : IEnumerable<TBehavior>
    where TEntity : Entity
    where TBehavior : SingleEntityBehavior<TEntity>
{
    private bool _initialized = false;
    private readonly Dictionary<int, List<(TBehavior behavior, HashSet<string> behaviorMethods)>> _data = [];

    public IEnumerator<TBehavior> GetEnumerator()
    {
        foreach (List<(TBehavior behavior, HashSet<string> behaviorMethods)> behaviors in _data.Values)
        {
            foreach ((TBehavior behavior, HashSet<string> _) in behaviors)
                yield return behavior;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// 尝试获取指定实体的行为实例。
    /// <br/>按照 <see cref="EntityBehavior{TEntity}.Priority"/> 降序寻找通过 <see cref="SingleEntityBehavior{TEntity}.ShouldProcess"/> 检测的实现了指定方法的Override实例。
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="methodName"></param>
    /// <param name="behavior"></param>
    /// <returns></returns>
    public bool TryGetBehavior(TEntity entity, string methodName, [NotNullWhen(true)] out TBehavior behavior)
    {
        if (_data.TryGetValue(entity.EntityType, out List<(TBehavior behavior, HashSet<string> behaviorMethods)> behaviorList))
        {
            foreach ((TBehavior temp, HashSet<string> behaviorMethods) in behaviorList)
            {
                if (!behaviorMethods.Contains(methodName))
                    continue;

                temp.Connect(entity);
                if (temp.ShouldProcess)
                {
                    behavior = temp;
                    return true;
                }
            }
        }
        behavior = null;
        return false;
    }

    public void FillSet() => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>());

    public void FillSet<T>() where T : Mod => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>().Where(b => b.Mod is T));

    public void FillSet(Assembly assemblyToSearch) => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch));

    private void Initialize(IEnumerable<TBehavior> behaviors)
    {
        _data.AddRange(behaviors.GroupBy(b => b.ApplyingType).ToDictionary(g => g.Key, g =>
            g.OrderByDescending(b => b.Priority).Select(b =>
            {
                Type type = b.GetType();
                return (b, (type.HasAttribute<CriticalBehaviorAttribute>() ? type.GetMethodNamesExceptObject(TOReflectionUtils.UniversalBindingFlags) : type.GetOverrideMethodNames(TOReflectionUtils.UniversalBindingFlags)).ToHashSet());
            }).ToList()));
        if (_initialized)
        {
            foreach (int entityType in _data.Keys)
                _data[entityType] = [.. _data[entityType].Distinct().OrderByDescending(b => b.behavior.Priority)];
        }
        _initialized = true;
    }

    public void Clear()
    {
        foreach ((_, List<(TBehavior behaviorInstance, HashSet<string> behaviorMethods)> behaviorList) in _data)
        {
            foreach ((_, HashSet<string> behaviorMethods) in behaviorList)
                behaviorMethods.Clear();
            behaviorList.Clear();
        }
        _data.Clear();
    }
}
#endregion Base

#region General Behavior
public abstract class PlayerBehavior : GeneralEntityBehavior<Player>
{
    public Player Player { get; private set; } = null;

    public TOPlayer OceanPlayer { get; private set; } = null;

    public override void Connect(Player player)
    {
        Player = player;
        OceanPlayer = player.Ocean();
    }

    #region 虚成员
    /// <summary>
    /// Called whenever the player is loaded (on the player selection screen). This can be used to initialize data structures, etc.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// This is where you reset any fields you add to your ModPlayer subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void ResetEffects() { }

    /// <summary>
    /// This is where you reset any fields related to INFORMATION accessories to their "default" states. This is identical to ResetEffects(); but should ONLY be used to
    /// reset info accessories. It will cause unintended side-effects if used with other fields.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <remarks>
    /// This method is called in tandem with <seealso cref="ResetEffects"/>, but it also called in <seealso cref="Player.RefreshInfoAccs"/> even when the game is paused;
    /// this allows for info accessories to keep properly updating while the game is paused, a feature/fix added in 1.4.4.
    /// </remarks>
    public virtual void ResetInfoAccessories() { }

    /// <summary>
    /// This is where you set any fields related to INFORMATION accessories based on the passed in player argument.
    /// <para/> Called on the local client.
    /// <para/> Note that this hook is only called if all of the requirements
    /// for a "nearby teammate" is met, which is when the other player is on the same team and within a certain distance, determined by the following code:
    /// <code>(Main.player[i].Center - base.Center).Length() &lt; 800f</code>
    /// </summary>
    public virtual void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer) { }

    /// <summary>
    /// Allows you to modify the player's max stats.  This hook runs after vanilla increases from the Life Crystal, Life Fruit and Mana Crystal are applied
    /// <para/> Called on local, server, and remote clients.
    /// <para/> <b>NOTE:</b> You should NOT modify <see cref="Player.statLifeMax"/> nor <see cref="Player.statManaMax"/> here.  Use the <paramref name="health"/> and <paramref name="mana"/> parameters.
    /// <para/> Also note that unlike many other tModLoader hooks, the default implementation of this hook has code that will assign <paramref name="health"/> and <paramref name="mana"/> to <see cref="StatModifier.Default"/>. Take care to place <c>base.ModifyMaxStats(out health, out mana);</c> before any other code you add to this hook to avoid issues, if you use it.
    /// </summary>
    /// <param name="health">The modifier to the player's maximum health</param>
    /// <param name="mana">The modifier to the player's maximum mana</param>
    public virtual void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        mana = StatModifier.Default;
    }

    /// <summary>
    /// Similar to <see cref="ResetEffects"/>, except this is only called when the player is dead. If this is called, then <see cref="ResetEffects"/> will not be called.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateDead() { }

    /// <summary>
    /// Currently never gets called, so this is useless.
    /// </summary>
    public virtual void PreSaveCustomData() { }

    /// <summary>
    /// Allows you to save custom data for this player.
    /// <para/> <b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
    /// <para/> <b>NOTE:</b> Try to only save data that isn't default values.
    /// </summary>
    /// <param name="tag"> The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument only for the sake of convenience and optimization. </param>
    public virtual void SaveData(TagCompound tag) { }

    /// <summary>
    /// Allows you to load custom data that you have saved for this player.
    /// <para/> <b>Try to write defensive loading code that won't crash if something's missing.</b>
    /// </summary>
    /// <param name="tag"> The TagCompound to load data from. </param>
    public virtual void LoadData(TagCompound tag) { }

    /// <summary>
    /// PreSavePlayer and PostSavePlayer wrap the vanilla player saving code (both are before the ModPlayer.Save). Useful for advanced situations where a save might be corrupted or rendered unusable by the values that normally would save.
    /// </summary>
    public virtual void PreSavePlayer() { }

    /// <summary>
    /// PreSavePlayer and PostSavePlayer wrap the vanilla player saving code (both are before the ModPlayer.Save). Useful for advanced situations where a save might be corrupted or rendered unusable by the values that normally would save.
    /// </summary>
    public virtual void PostSavePlayer() { }

    /// <summary>
    /// <br/> Allows you to copy information that you intend to sync between server and client to the <paramref name="targetCopy"/> parameter.
    /// <br/> You would then use the <see cref="SendClientChanges"/> hook to compare against that data and decide what needs synchronizing.
    /// <br/> This hook is called with every call of the <see cref="Player.clientClone"/> method.
    /// <br/>
    /// <br/> <b>NOTE:</b> For performance reasons, avoid deep cloning or copying any excessive information.
    /// <br/> <b>NOTE:</b> Using <see cref="Item.CopyNetStateTo"/> is the recommended way of creating item snapshots.
    /// </summary>
    /// <param name="targetCopy"></param>
    public virtual void CopyClientState(ModPlayer targetCopy) { }

    /// <summary>
    /// Allows you to sync information about this player between server and client. The toWho and fromWho parameters correspond to the remoteClient/toClient and ignoreClient arguments, respectively, of NetMessage.SendData/ModPacket.Send. The newPlayer parameter is whether or not the player is joining the server (it is true on the joining client).
    /// </summary>
    /// <param name="toWho"></param>
    /// <param name="fromWho"></param>
    /// <param name="newPlayer"></param>
    public virtual void SyncPlayer(int toWho, int fromWho, bool newPlayer) { }

    /// <summary>
    /// Allows you to sync any information that has changed between the server and client. Here, you should check the information you have copied in the clientClone parameter; if they differ between this player and the clientPlayer parameter, then you should send that information using NetMessage.SendData or ModPacket.Send.
    /// </summary>
    /// <param name="clientPlayer"></param>
    public virtual void SendClientChanges(ModPlayer clientPlayer) { }

    /// <summary>
    /// Allows you to give the player a negative life regeneration based on its state (for example, the "On Fire!" debuff makes the player take damage-over-time). This is typically done by setting Player.lifeRegen to 0 if it is positive, setting Player.lifeRegenTime to 0, and subtracting a number from Player.lifeRegen. The player will take damage at a rate of half the number you subtract per second.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateBadLifeRegen() { }

    /// <summary>
    /// Allows you to increase the player's life regeneration based on its state. This can be done by incrementing Player.lifeRegen by a certain number. The player will recover life at a rate of half the number you add per second. You can also increment Player.lifeRegenTime to increase the speed at which the player reaches its maximum natural life regeneration.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateLifeRegen() { }

    /// <summary>
    /// Allows you to modify the power of the player's natural life regeneration. This can be done by multiplying the regen parameter by any number. For example, campfires multiply it by 1.1, while walking multiplies it by 0.5.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="regen"></param>
    public virtual void NaturalLifeRegen(ref float regen) { }

    /// <summary>
    /// Allows you to modify the player's stats while the game is paused due to the autopause setting being on.
    /// This is called in single player only, some time before the player's tick update would happen when the game isn't paused.
    /// </summary>
    public virtual void UpdateAutopause() { }

    /// <summary>
    /// This is called at the beginning of every tick update for this player, after checking whether the player exists. <br/>
    /// This can be used to adjust timers and cooldowns.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PreUpdate() { }

    /// <summary>
    /// Use this to check on keybinds you have registered. While SetControls is set even while in text entry mode, this hook is only called during gameplay.
    /// <para/> Called on the local client only.
    /// <para/> Read <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/Players/ExampleKeybindPlayer.cs">ExampleKeybindPlayer.cs</see> for examples and information on using this hook.
    /// </summary>
    /// <param name="triggersSet"></param>
    public virtual void ProcessTriggers(TriggersSet triggersSet) { }

    /// <summary>
    /// This is called when the player activates their armor set bonus by double tapping down (or up if <see cref="Main.ReversedUpDownArmorSetBonuses"/> is true). As an example, the Vortex armor uses this to toggle stealth mode.
    /// <para/> Use this to implement armor set bonuses that need to be activated by the player.
    /// <para/> Don't forget to check if your armor set is active.
    /// <para/> While this technically can be used for other effects, it will likely be frustrating for your players if non-armor set effects are being triggered in tandem with armor set bonus effects. Modders can use <see cref="Player.holdDownCardinalTimer"/> and <see cref="Player.doubleTapCardinalTimer"/> directly in other hooks for similar effects if needed.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void ArmorSetBonusActivated() { }

    /// <summary>
    /// This is called when the player activates their armor set bonus by holding down (or up if <see cref="Main.ReversedUpDownArmorSetBonuses"/> is true) for some amount of time. The <paramref name="holdTime"/> parameter indicates how many ticks the key has been held down for. As an example, the Stardust armor prior to 1.4.4 used to use this to set the location of the Stardust Guardian if <paramref name="holdTime"/> was greater than 60.
    /// <para/> Use this to implement armor set bonuses that need to be activated by the player.
    /// <para/> Don't forget to check if your armor set is active.
    /// <para/> While this technically can be used for other effects, it will likely be frustrating for your players if non-armor set effects are being triggered in tandem with armor set bonus effects. Modders can use <see cref="Player.holdDownCardinalTimer"/> and <see cref="Player.doubleTapCardinalTimer"/> directly in other hooks for similar effects if needed.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void ArmorSetBonusHeld(int holdTime) { }

    /// <summary>
    /// Use this to modify the control inputs that the player receives. For example, the Confused debuff swaps the values of Player.controlLeft and Player.controlRight. This is called sometime after PreUpdate is called.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void SetControls() { }

    /// <summary>
    /// This is called sometime after SetControls is called, and right before all the buffs update on this player. This hook can be used to add buffs to the player based on the player's state (for example, the Campfire buff is added if the player is near a Campfire).
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PreUpdateBuffs() { }

    /// <summary>
    /// This is called right after all of this player's buffs update on the player. This can be used to modify the effects that the buff updates had on this player, and can also be used for general update tasks.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PostUpdateBuffs() { }

    /// <summary>
    /// Called after Update Accessories.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateEquips() { }

    /// <summary>
    /// This is called right after all of this player's equipment and armor sets update on the player, which is sometime after PostUpdateBuffs is called. This can be used to modify the effects that the equipment had on this player, and can also be used for general update tasks.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PostUpdateEquips() { }

    /// <summary>
    /// Is called in Player.Frame() after vanilla functional slots are evaluated, including selection screen to prepare and denote visible accessories. Player Instance sensitive.
    /// </summary>
    public virtual void UpdateVisibleAccessories() { }

    /// <summary>
    /// Is called in Player.Frame() after vanilla vanity slots are evaluated, including selection screen to prepare and denote visible accessories. Player Instance sensitive.
    /// </summary>
    public virtual void UpdateVisibleVanityAccessories() { }

    /// <summary>
    /// Is called in Player.UpdateDyes(), including selection screen. Player Instance sensitive.
    /// </summary>
    public virtual void UpdateDyes() { }

    /// <summary>
    /// This is called after miscellaneous update code is called in Player.Update, which is sometime after PostUpdateEquips is called. This can be used for general update tasks.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PostUpdateMiscEffects() { }

    /// <summary>
    /// This is called after the player's horizontal speeds are modified, which is sometime after PostUpdateMiscEffects is called, and right before the player's horizontal position is updated. Use this to modify maxRunSpeed, accRunSpeed, runAcceleration, and similar variables before the player moves forwards/backwards.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PostUpdateRunSpeeds() { }

    /// <summary>
    /// This is called right before modifying the player's position based on velocity. Use this to make direct changes to the velocity.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PreUpdateMovement() { }

    /// <summary>
    /// This is called at the very end of the Player.Update method. Final general update tasks can be placed here.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PostUpdate() { }

    /// <summary>
    /// Use this hook to modify the jump duration from an extra jump.
    /// <para/> Called on local, server, and remote clients.
    /// <para/> Vanilla's extra jumps use the following values:
    /// <para>
    /// Basilisk mount: 0.75<br/>
    /// Blizzard in a Bottle: 1.5<br/>
    /// Cloud in a Bottle: 0.75<br/>
    /// Fart in a Jar: 2<br/>
    /// Goat mount: 2<br/>
    /// Sandstorm in a Bottle: 3<br/>
    /// Santank mount: 2<br/>
    /// Tsunami in a Bottle: 1.25<br/>
    /// Unicorn mount: 2
    /// </para>
    /// </summary>
    /// <param name="jump">The jump being performed</param>
    /// <param name="duration">A modifier to the player's jump height, which when combined effectively acts as the duration for the extra jump</param>
    public virtual void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration) { }

    /// <summary>
    /// An extra condition for whether an extra jump can be started.  Returns <see langword="true"/> by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="jump">The jump that would be performed</param>
    /// <returns><see langword="true"/> to let the jump be started, <see langword="false"/> otherwise.</returns>
    public virtual bool CanStartExtraJump(ExtraJump jump) => true;

    /// <summary>
    /// Effects that should appear when the extra jump starts should happen here.
    /// <para/> For example, the Cloud in a Bottle's initial puff of smoke is spawned here.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="jump">The jump being performed</param>
    /// <param name="playSound">Whether the poof sound should play.  Set this parameter to <see langword="false"/> if you want to play a different sound.</param>
    public virtual void OnExtraJumpStarted(ExtraJump jump, ref bool playSound) { }

    /// <summary>
    /// This hook runs before the <see cref="ExtraJumpState.Active"/> flag for an extra jump is set from <see langword="true"/> to <see langword="false"/> when the extra jump's duration has expired
    /// <para/> This occurs when a grappling hook is thrown, the player grabs onto a rope, the jump's duration has finished and when the player's frozen, turned to stone or webbed.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="jump">The jump that was performed</param>
    public virtual void OnExtraJumpEnded(ExtraJump jump) { }

    /// <summary>
    /// This hook runs before the <see cref="ExtraJumpState.Available"/> flag for an extra jump is set to <see langword="true"/> in <see cref="Player.RefreshDoubleJumps"/>
    /// <para/> This occurs at the start of the grounded jump and while the player is grounded.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="jump">The jump instance</param>
    public virtual void OnExtraJumpRefreshed(ExtraJump jump) { }

    /// <summary>
    /// Effects that should appear while the player is performing an extra jump should happen here.
    /// <para/> For example, the Sandstorm in a Bottle's dusts are spawned here.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void ExtraJumpVisuals(ExtraJump jump) { }

    /// <summary>
    /// Return <see langword="false"/> to prevent <see cref="ExtraJump.ShowVisuals(Player)"/> from executing on <paramref name="jump"/>.
    /// <para/> By default, this hook returns whether the player is moving upwards with respect to <see cref="Player.gravDir"/>
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="jump">The jump instance</param>
    public virtual bool CanShowExtraJumpVisuals(ExtraJump jump) => true;

    /// <summary>
    /// This hook runs before the <see cref="ExtraJumpState.Available"/> flag for an extra jump is set to <see langword="false"/>  in <see cref="Player.Update(int)"/> due to the jump being unavailable or when calling <see cref="Player.ConsumeAllExtraJumps"/> (vanilla calls it when a mount that blocks jumps is active)
    /// </summary>
    /// <param name="jump">The jump instance</param>
    public virtual void OnExtraJumpCleared(ExtraJump jump) { }

    /// <summary>
    /// Allows you to modify the armor and accessories that visually appear on the player. In addition, you can create special effects around this character, such as creating dust.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void FrameEffects() { }

    /// <summary>
    /// Allows you to make a player immune to damage from a certain source, or at a certain time.
    /// Vanilla examples include shimmer and journey god mode. Runs before dodges are used, or any damage calculations are performed.
    /// <para/> If immunity is determined on the local player, the hit will not be sent across the network.
    /// <para/> In pvp the hit will be sent regardless, and all clients will determine immunity independently, though it only really matters for the receiving player.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="damageSource">The source of the damage (projectile, NPC, etc)</param>
    /// <param name="cooldownCounter">The <see cref="ImmunityCooldownID"/> of the hit</param>
    /// <param name="dodgeable">Whether the hit is dodgeable</param>
    /// <returns>True to completely ignore the hit</returns>
    public virtual bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => false;

    /// <summary>
    /// Allows you to dodge damage for a player. Intended for guaranteed 'free' or random dodges. Vanilla example is black belt.
    /// <para/> For dodges which consume a stack/buff or have a cooldown, use <see cref="ConsumableDodge"/> instead.
    /// <para/> Called on the local client receiving damage.
    /// <para/> If dodge is determined on the local player, the hit will not be sent across the network. If visual indication of the dodge is required on remote clients, you will need to send your own packet.
    /// </summary>
    /// <returns>True to completely ignore the hit</returns>
    public virtual bool FreeDodge(Player.HurtInfo info) => false;

    /// <summary>
    /// Allows you to dodge damage for a player. Vanilla examples include hallowed armor shadow dodge, and brain of confusion.
    /// <para/> For dodges which are 'free' and should be used before triggering consumables, use <see cref="FreeDodge"/> instead.
    /// <para/> Called on the local client receiving damage.
    /// <para/> If dodge is determined on the local player, the hit will not be sent across the network.
    /// <para/> You may need to send your own packet to synchronize the consumption of the effect, or application of the cooldown in multiplayer.
    /// </summary>
    /// <returns>True to completely ignore the hit</returns>
    public virtual bool ConsumableDodge(Player.HurtInfo info) => false;

    /// <summary>
    /// Allows you to adjust an instance of player taking damage.
    /// <para/> Called on the local client taking damage.
    /// <para/> Only use this hook if you need to modify the hurt parameters in some way, eg consuming a buff which reduces the damage of the next hit. Use <see cref="OnHurt"/> or <see cref="PostHurt"/> instead where possible.
    /// <para/> The player will always take at least 1 damage. To prevent damage use <see cref="ImmuneTo"/> or <see cref="FreeDodge"/> <br/>
    /// </summary>
    public virtual void ModifyHurt(ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to make anything happen when the player takes damage.
    /// <para/> Called on the local client taking damage.
    /// <para/> Called right before health is reduced.
    /// </summary>
    public virtual void OnHurt(Player.HurtInfo info) { }

    /// <summary>
    /// Allows you to make anything happen when the player takes damage.
    /// <para/> Called on the local client taking damage
    /// <para/> Only called if the player survives the hit.
    /// </summary>
    public virtual void PostHurt(Player.HurtInfo info) { }

    /// <summary>
    /// This hook is called whenever the player is about to be killed after reaching 0 health.
    /// <para/> Called on local, server, and remote clients.
    /// <para/> Set the <paramref name="playSound"/> parameter to false to stop the death sound from playing. Set the <paramref name="genDust"/> parameter to false to stop the dust from being created. These are useful for creating your own sound or dust to replace the normal death effects, such as how the Frost armor set spawns <see cref="DustID.IceTorch"/> instead of <see cref="DustID.Blood"/>. For mod compatibility, it is recommended to check if these values are true before setting them to true and spawning dust or playing sounds to avoid overlapping sounds and dust effects.
    /// <para/> Return false to stop the player from being killed. Only return false if you know what you are doing! Returns true by default.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="hitDirection"></param>
    /// <param name="pvp"></param>
    /// <param name="playSound"></param>
    /// <param name="genDust"></param>
    /// <param name="damageSource"></param>
    /// <returns></returns>
    public virtual bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) => true;

    /// <summary>
    /// Allows you to make anything happen when the player dies.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="hitDirection"></param>
    /// <param name="pvp"></param>
    /// <param name="damageSource"></param>
    public virtual void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) { }

    /// <summary>
    /// Called before vanilla makes any luck calculations. Return false to prevent vanilla from making their luck calculations. Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="luck"></param>
    public virtual bool PreModifyLuck(ref float luck) => true;

    /// <summary>
    /// Allows you to modify a player's luck amount.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="luck"></param>
    public virtual void ModifyLuck(ref float luck) { }

    /// <summary>
    /// Allows you to do anything before the update code for the player's held item is run. Return false to stop the held item update code from being run (for example, if the player is frozen). Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <returns></returns>
    public virtual bool PreItemCheck() => true;

    /// <summary>
    /// Allows you to do anything after the update code for the player's held item is run. Hooks for the middle of the held item update code have more specific names in ModItem and ModPlayer.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PostItemCheck() { }

    /// <summary>
    /// Allows you to change the effective useTime of an item.
    /// <para/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <returns> The multiplier on the usage time. 1f by default. Values greater than 1 increase the item use's length. </returns>
    public virtual float UseTimeMultiplier(Item item) => 1f;

    /// <summary>
    /// Allows you to change the effective useAnimation of an item.
    /// <para/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <returns>The multiplier on the animation time. 1f by default. Values greater than 1 increase the item animation's length. </returns>
    public virtual float UseAnimationMultiplier(Item item) => 1f;

    /// <summary>
    /// Allows you to safely change both useTime and useAnimation while keeping the values relative to each other.
    /// <para/> Useful for status effects.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <returns> The multiplier on the use speed. 1f by default. Values greater than 1 increase the overall item speed. </returns>
    public virtual float UseSpeedMultiplier(Item item) => 1f;

    /// <summary>
    /// Allows you to temporarily modify the amount of life a life healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healLife"/> value.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
    /// <param name="healValue">The amount of life being healed.</param>
    public virtual void GetHealLife(Item item, bool quickHeal, ref int healValue) { }

    /// <summary>
    /// Allows you to temporarily modify the amount of mana a mana healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healMana"/> value.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
    /// <param name="healValue">The amount of mana being healed.</param>
    public virtual void GetHealMana(Item item, bool quickHeal, ref int healValue) { }

    /// <summary>
    /// Allows you to temporarily modify the amount of mana an item will consume on use, based on player buffs, accessories, etc. This is only called for items with a mana value.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="reduce">Used for decreasingly stacking buffs (most common). Only ever use -= on this field.</param>
    /// <param name="mult">Use to directly multiply the item's effective mana cost. Good for debuffs, or things which should stack separately (eg meteor armor set bonus).</param>
    public virtual void ModifyManaCost(Item item, ref float reduce, ref float mult) { }

    /// <summary>
    /// Allows you to make stuff happen when a player doesn't have enough mana for the item they are trying to use.
    /// If the player has high enough mana after this hook runs, mana consumption will happen normally.
    /// Only runs once per item use.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="neededMana">The mana needed to use the item.</param>
    public virtual void OnMissingMana(Item item, int neededMana) { }

    /// <summary>
    /// Allows you to make stuff happen when a player consumes mana on use of an item.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="manaConsumed">The mana consumed from the player.</param>
    public virtual void OnConsumeMana(Item item, int manaConsumed) { }

    /// <summary>
    /// Allows you to dynamically modify a weapon's damage based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="damage">The StatModifier object representing the totality of the various modifiers to be applied to the item's base damage.</param>
    public virtual void ModifyWeaponDamage(Item item, ref StatModifier damage) { }

    /// <summary>
    /// Allows you to dynamically modify a weapon's knockback based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="knockback">The StatModifier object representing the totality of the various modifiers to be applied to the item's base knockback.</param>
    public virtual void ModifyWeaponKnockback(Item item, ref StatModifier knockback) { }

    /// <summary>
    /// Allows you to dynamically modify a weapon's crit chance based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="crit">The total crit chance of the item after all normal crit chance calculations.</param>
    public virtual void ModifyWeaponCrit(Item item, ref float crit) { }

    /// <summary>
    /// Whether or not the given ammo item will be consumed by this weapon.<br></br>
    /// By default, returns true; return false to prevent ammo consumption. <br></br>
    /// If false is returned, the <see cref="OnConsumeAmmo"/> hook is never called.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="weapon">The weapon that this player is attempting to use.</param>
    /// <param name="ammo">The ammo that the given weapon is attempting to consume.</param>
    /// <returns></returns>
    public virtual bool CanConsumeAmmo(Item weapon, Item ammo) => true;

    /// <summary>
    /// Allows you to make things happen when the given ammo is consumed by the given weapon.<br></br>
    /// Called before the ammo stack is reduced, and is never called if the ammo isn't consumed in the first place.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="weapon">The weapon that is currently using the given ammo.</param>
    /// <param name="ammo">The ammo that the given weapon is currently using.</param>
    public virtual void OnConsumeAmmo(Item weapon, Item ammo) { }

    /// <summary>
    /// Allows you to prevent an item from shooting a projectile on use. Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item"> The item being used. </param>
    /// <returns></returns>
    public virtual bool CanShoot(Item item) => true;

    /// <summary>
    /// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by an item.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item"> The item being used. </param>
    /// <param name="position"> The center position of the projectile. </param>
    /// <param name="velocity"> The velocity of the projectile. </param>
    /// <param name="type"> The ID of the projectile. </param>
    /// <param name="damage"> The damage of the projectile. </param>
    /// <param name="knockback"> The knockback of the projectile. </param>
    public virtual void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <summary>
    /// Allows you to modify an item's shooting mechanism. Return false to prevent vanilla's shooting code from running. Returns true by default.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item"> The item being used. </param>
    /// <param name="source"> The projectile source's information. </param>
    /// <param name="position"> The center position of the projectile. </param>
    /// <param name="velocity"> The velocity of the projectile. </param>
    /// <param name="type"> The ID of the projectile. </param>
    /// <param name="damage"> The damage of the projectile. </param>
    /// <param name="knockback"> The knockback of the projectile. </param>
    public virtual bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <summary>
    /// Allows you to give this player's melee weapon special effects, such as creating light or dust. This is typically used to implement a weapon enchantment, similar to flasks, frost armor, or magma stone effects.
    /// <para/> If implementing a weapon enchantment, also implement <see cref="EmitEnchantmentVisualsAt(Projectile, Vector2, int, int)"/> to support enchantment visuals for projectiles as well.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="hitbox"></param>
    public virtual void MeleeEffects(Item item, Rectangle hitbox) { }

    /// <inheritdoc cref="ModProjectile.EmitEnchantmentVisualsAt"/>
    public virtual void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) { }

    /// <summary>
    /// Allows you to determine whether the given item can catch the given NPC.<br></br>
    /// Return true or false to say the target can or cannot be caught, respectively, regardless of vanilla rules.<br></br>
    /// Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.<br></br>
    /// If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.<br></br>
    /// <para/> Called on the local client only.
    /// <para/> NOTE: this does not classify the given item as a catch tool, which is necessary for catching NPCs in the first place.
    /// To do that, you will need to use the "CatchingTool" set in ItemID.Sets.
    /// </summary>
    /// <param name="target">The NPC the player is trying to catch.</param>
    /// <param name="item">The item with which the player is trying to catch the target NPC.</param>
    public virtual bool? CanCatchNPC(NPC target, Item item) => null;

    /// <summary>
    /// Allows you to make things happen when the given item attempts to catch the given NPC.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc">The NPC which the player attempted to catch.</param>
    /// <param name="item">The item used to catch the given NPC.</param>
    /// <param name="failed">Whether or not the given NPC has been successfully caught.</param>
    public virtual void OnCatchNPC(NPC npc, Item item, bool failed) { }

    /// <summary>
    /// Allows you to dynamically modify the given item's size for this player, similarly to the effect of the Titan Glove.
    /// <para/> Called on local and remote clients
    /// </summary>
    /// <param name="item">The item to modify the scale of.</param>
    /// <param name="scale">
    /// The scale multiplier to be applied to the given item.<br></br>
    /// Will be 1.1 if the Titan Glove is equipped, and 1 otherwise.
    /// </param>
    public virtual void ModifyItemScale(Item item, ref float scale) { }

    /// <summary>
    /// This hook is called when a player damages anything, whether it be an NPC or another player, using anything, whether it be a melee weapon or a projectile. The x and y parameters are the coordinates of the victim parameter's center.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="victim"></param>
    public virtual void OnHitAnything(float x, float y, Entity victim) { }

    /// <summary>
    /// Allows you to determine whether a player can hit the given NPC. Returns true by default.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="target"></param>
    /// <returns>True by default</returns>
    public virtual bool CanHitNPC(NPC target) => true;

    /// <summary>
    /// Allows you to determine whether a player melee attack can collide the given NPC by swinging a melee weapon.
    /// Use <see cref="CanHitNPCWithItem(Item, NPC)"/> instead for Guide Voodoo Doll-type effects.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="item">The weapon item the player is holding.</param>
    /// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
    /// <param name="target">The target npc.</param>
    /// <returns>
    /// Return true to allow colliding the target, return false to block the player weapon from colliding the target, and return null to use the vanilla code for whether the target can be colliding by melee weapon. Returns null by default.
    /// </returns>
    public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc that this player does to an NPC.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when this player hits an NPC.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether a player can hit the given NPC by swinging a melee weapon. Return true to allow hitting the target, return false to block this player from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool? CanHitNPCWithItem(Item item, NPC target) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that this player does to an NPC by swinging a melee weapon.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when this player hits an NPC by swinging a melee weapon (for example how the Pumpkin Sword creates pumpkin heads).
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="target"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether a projectile created by this player can hit the given NPC. Return true to allow hitting the target, return false to block this projectile from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="proj"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool? CanHitNPCWithProj(Projectile proj, NPC target) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that a projectile created by this player does to an NPC.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="proj"></param>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a projectile created by this player hits an NPC (for example, inflicting debuffs).
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="proj"></param>
    /// <param name="target"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether a melee weapon swung by this player can hit the given opponent player. Return false to block this weapon from hitting the target. Returns true by default.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool CanHitPvp(Item item, Player target) => true;

    /// <summary>
    /// Allows you to determine whether a projectile created by this player can hit the given opponent player. Return false to block the projectile from hitting the target. Returns true by default.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="proj"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool CanHitPvpWithProj(Projectile proj, Player target) => true;

    /// <summary>
    /// Allows you to determine whether the given NPC can hit this player. Return false to block this player from being hit by the NPC. Returns true by default. CooldownSlot determines which of the player's cooldown counters (<see cref="ImmunityCooldownID"/>) to use, and defaults to -1 (<see cref="ImmunityCooldownID.General"/>).
    /// <para/> Called on the client taking damage
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="cooldownSlot"></param>
    /// <returns></returns>
    public virtual bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) => true;

    /// <summary>
    /// Allows you to modify the damage, etc., that an NPC does to this player.
    /// <para/> Called on the client taking damage
    /// </summary>
    public virtual void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC hits this player (for example, inflicting debuffs).
    /// <para/> Called on the client taking damage
    /// </summary>
    public virtual void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) { }

    /// <summary>
    /// Allows you to determine whether the given hostile projectile can hit this player. Return false to block this player from being hit. Returns true by default.
    /// <para/> Called on the client taking damage
    /// </summary>
    /// <param name="proj"></param>
    /// <returns></returns>
    public virtual bool CanBeHitByProjectile(Projectile proj) => true;

    /// <summary>
    /// Allows you to modify the damage, etc., that a hostile projectile does to this player.
    /// <para/> Called on the client taking damage
    /// </summary>
    public virtual void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a hostile projectile hits this player.
    /// <para/> Called on the client taking damage
    /// </summary>
    public virtual void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) { }

    /// <summary>
    /// Allows you to change information about the ongoing fishing attempt before caught items/NPCs are decided, after all vanilla information has been gathered.
    /// <para/> Will not be called if various conditions for getting a catch aren't met, meaning you can't modify those.
    /// <para/> Setting <see cref="FishingAttempt.rolledItemDrop"/> or <see cref="FishingAttempt.rolledEnemySpawn"/> is not allowed and will be reset, use <see cref="CatchFish"/> for that.
    /// <para/> Called for the local client only.
    /// </summary>
    /// <param name="attempt">The structure containing most data from the vanilla fishing attempt</param>
    public virtual void ModifyFishingAttempt(ref FishingAttempt attempt) { }

    /// <summary>
    /// Allows you to change the item or enemy the player gets when successfully catching an item or NPC. The Fishing Attempt structure contains most information about the vanilla event, including the Item Rod and Bait used by the player, the liquid it is being fished on, and so on.
    /// The Sonar and Sonar position fields allow you to change the text, color, velocity and position of the catch's name (be it item or NPC) freely
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="attempt">The structure containing most data from the vanilla fishing attempt</param>
    /// <param name="itemDrop">The item that will be created when this fishing attempt succeeds. leave &lt;0 for no item</param>
    /// <param name="npcSpawn">The enemy that will be spawned if there is no item caught. leave &lt;0 for no NPC spawn</param>
    /// <param name="sonar">Fill all of this structure's fields to override the sonar text, or make sonar.Text null to disable custom sonar</param>
    /// <param name="sonarPosition">The position the Sonar text will spawn. Bobber location by default.</param>
    public virtual void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) { }

    /// <summary>
    /// Allows you to modify the item caught by the fishing player, including stack
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="fish">The item (Fish) to modify</param>
    public virtual void ModifyCaughtFish(Item fish) { }

    /// <summary>
    /// Choose if this bait will be consumed or not when used for fishing. return null for vanilla behavior.
    /// Not consuming will always take priority over forced consumption
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="bait">The item (bait) that would be consumed</param>
    public virtual bool? CanConsumeBait(Item bait) => null;

    /// <summary>
    /// Allows you to modify the player's fishing power. As an example of the type of stuff that should go here, the phase of the moon can influence fishing power.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="fishingRod"></param>
    /// <param name="bait"></param>
    /// <param name="fishingLevel"></param>
    public virtual void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel) { }

    /// <summary>
    /// Allows you to add to, change, or remove from the items the player earns when finishing an Angler quest. The rareMultiplier is a number between 0.15 and 1 inclusively; the lower it is the higher chance there should be for the player to earn rare items.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="rareMultiplier"></param>
    /// <param name="rewardItems"></param>
    public virtual void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems) { }

    /// <summary>
    /// Allows you to modify what items are possible for the player to earn when giving a Strange Plant to the Dye Trader.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="rewardPool"></param>
    public virtual void GetDyeTraderReward(List<int> rewardPool) { }

    /// <summary>
    /// Allows you to create special effects when this player is drawn, such as creating dust, modifying the color the player is drawn in, etc. The fullBright parameter makes it so that the drawn player ignores the modified color and lighting. Make sure to add the indexes of any dusts you create to drawInfo.DustCache, and the indexes of any gore you create to drawInfo.GoreCache.
    /// <para/> This will be called multiple times a frame if a player afterimage is being drawn. Check <code>if(drawinfo.shadow == 0f)</code> to do some logic only when drawing the original player image. For example, spawning dust only for the original player image is commonly the desired behavior.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="drawInfo"></param>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    /// <param name="fullBright"></param>
    public virtual void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { }

    /// <summary>
    /// Allows you to modify the drawing parameters of the player before drawing begins.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="drawInfo"></param>
    public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) { }

    /// <summary>
    /// Allows you to reorder the player draw layers.
    /// This is called once at the end of mod loading, not during the game.
    /// Use with extreme caution, or risk breaking other mods.
    /// </summary>
    /// <param name="positions">Add/remove/change the positions applied to each layer here</param>
    public virtual void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) { }

    /// <summary>
    /// Allows you to modify the visibility of layers about to be drawn. Layers can be accessed via <see cref="PlayerDrawLayerLoader.Layers"/>
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="drawInfo"></param>
    public virtual void HideDrawLayers(PlayerDrawSet drawInfo) { }

    /// <summary>
    /// Use this hook to modify <see cref="Main.screenPosition"/> after weapon zoom and camera lerp have taken place.
    /// <para/> Called on the local client only.
    /// <para/> Also consider using <c>Main.instance.CameraModifiers.Add(CameraModifier);</c> as shown in ExampleMods MinionBossBody for screen shakes.
    /// </summary>
    public virtual void ModifyScreenPosition() { }

    /// <summary>
    /// Use this to modify the zoom factor for the player. The zoom correlates to the percentage of half the screen size the zoom can reach. A value of -1 passed in means no vanilla scope is in effect. A value of 1.0 means the scope can zoom half a screen width/height away, putting the player on the edge of the game screen. Vanilla values include .8, .6666, and .5.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="zoom"></param>
    public virtual void ModifyZoom(ref float zoom) { }

    /// <summary>
    /// Called on remote clients when a player connects.
    /// </summary>
    public virtual void PlayerConnect() { }

    /// <summary>
    /// Called on the server and remote clients when a player disconnects.
    /// </summary>
    public virtual void PlayerDisconnect() { }

    /// <summary>
    /// Called when the player enters the world. A possible use is ensuring that UI elements are reset to the configuration specified in data saved to the ModPlayer. Can also be used for informational messages.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void OnEnterWorld() { }

    /// <summary>
    /// Called when a player respawns in the world.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void OnRespawn() { }

    /// <summary>
    /// Called whenever the player shift-clicks an item slot. This can be used to override default clicking behavior (ie. selling, trashing, moving items).
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="inventory">The array of items the slot is part of.</param>
    /// <param name="context">The Terraria.UI.ItemSlot.Context of the inventory.</param>
    /// <param name="slot">The index in the inventory of the clicked slot.</param>
    /// <returns>Whether or not to block the default code (sell, trash, move, etc) from running. Returns false by default.</returns>
    public virtual bool ShiftClickSlot(Item[] inventory, int context, int slot) => false;

    /// <summary>
    /// Called whenever the player hovers over an item slot. This can be used to override <see cref="Main.cursorOverride"/>
    /// <para/> Called on the local client only.
    /// <para/> See <see cref="ID.CursorOverrideID"/> for cursor override style IDs
    /// </summary>
    /// <param name="inventory">The array of items the slot is part of.</param>
    /// <param name="context">The Terraria.UI.ItemSlot.Context of the inventory.</param>
    /// <param name="slot">The index in the inventory of the hover slot.</param>
    /// <returns>Whether or not to block the default code that modifies <see cref="Main.cursorOverride"/> from running. Returns false by default.</returns>
    public virtual bool HoverSlot(Item[] inventory, int context, int slot) => false;

    /// <summary>
    /// Called whenever the player sells an item to an NPC.
    /// <para/> Called on the local client only.
    /// <para/> Note that <paramref name="item"/> might be an item sold by the NPC, not an item to buy back. Check <see cref="Item.buyOnce"/> if relevant to your logic.
    /// </summary>
    /// <param name="vendor">The NPC vendor.</param>
    /// <param name="shopInventory">The current inventory of the NPC shop.</param>
    /// <param name="item">The item the player just sold.</param>
    public virtual void PostSellItem(NPC vendor, Item[] shopInventory, Item item) { }

    /// <summary>
    /// Return false to prevent a transaction. Called before the transaction.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="vendor">The NPC vendor.</param>
    /// <param name="shopInventory">The current inventory of the NPC shop.</param>
    /// <param name="item">The item the player is attempting to sell.</param>
    /// <returns></returns>
    public virtual bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) => true;

    /// <summary>
    /// Called whenever the player buys an item from an NPC.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="vendor">The NPC vendor.</param>
    /// <param name="shopInventory">The current inventory of the NPC shop.</param>
    /// <param name="item">The item the player just purchased.</param>
    public virtual void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) { }

    /// <summary>
    /// Return false to prevent a transaction. Called before the transaction.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="vendor">The NPC vendor.</param>
    /// <param name="shopInventory">The current inventory of the NPC shop.</param>
    /// <param name="item">The item the player is attempting to buy.</param>
    /// <returns></returns>
    public virtual bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item) => true;

    /// <summary>
    /// Return false to prevent an item from being used. By default returns true.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item the player is attempting to use.</param>
    public virtual bool CanUseItem(Item item) => true;

    /// <summary>
    /// Allows you to modify the autoswing (auto-reuse) behavior of any item without having to mess with Item.autoReuse.
    /// <para/> Useful to create effects like the Feral Claws which makes melee weapons and whips auto-reusable.
    /// <para/> Return true to enable autoswing (if not already enabled through autoReuse), return false to prevent autoswing. Returns null by default, which applies vanilla behavior.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item"> The item. </param>
    public virtual bool? CanAutoReuseItem(Item item) => null;

    /// <summary>
    /// Called while the nurse chat is displayed. Return false to prevent the player from healing. If you return false, you need to set chatText so the user knows why they can't heal.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="nurse">The Nurse NPC instance.</param>
    /// <param name="health">How much health the player gains.</param>
    /// <param name="removeDebuffs">If set to false, debuffs will not be healed.</param>
    /// <param name="chatText">Set this to the Nurse chat text that will display if healing is prevented.</param>
    /// <returns>True by default. False to prevent nurse services.</returns>
    public virtual bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText) => true;

    /// <summary>
    /// Called while the nurse chat is displayed and after ModifyNurseHeal. Allows custom pricing for Nurse services. See the <see href="https://terraria.wiki.gg/wiki/Nurse">Nurse wiki page</see> for the default pricing.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="nurse">The Nurse NPC instance.</param>
    /// <param name="health">How much health the player gains.</param>
    /// <param name="removeDebuffs">Whether or not debuffs will be healed.</param>
    /// <param name="price"></param>
    public virtual void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price) { }

    /// <summary>
    /// Called after the player heals themselves with the Nurse NPC.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="nurse">The Nurse npc providing the heal.</param>
    /// <param name="health">How much health the player gained.</param>
    /// /// <param name="removeDebuffs">Whether or not debuffs were healed.</param>
    /// <param name="price">The price the player paid in copper coins.</param>
    public virtual void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price) { }

    /// <summary>
    /// Called when the player is created in the menu.
    /// You can use this method to add items to the player's starting inventory, as well as their inventory when they respawn in mediumcore.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="mediumCoreDeath">Whether you are setting up a mediumcore player's inventory after their death.</param>
    /// <returns>An enumerable of the items you want to add. If you want to add nothing, return Enumerable.Empty&lt;Item&gt;().</returns>
    public virtual IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) => [];

    /// <summary>
    /// Allows you to modify the items that will be added to the player's inventory. Useful if you want to stop vanilla or other mods from adding an item.
    /// You can access a mod's items by using the mod's internal name as the indexer, such as: additions["ModName"]. To access vanilla items you can use "Terraria" as the index.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="itemsByMod">The items that will be added. Each key is the internal mod name of the mod adding the items. Vanilla items use the "Terraria" key.</param>
    /// <param name="mediumCoreDeath">Whether you are setting up a mediumcore player's inventory after their death.</param>
    public virtual void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath) { }

    /// <summary>
    /// Called when Recipe.FindRecipes is called or the player is crafting an item
    /// You can use this method to add items as the materials that may be used for crafting items
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="itemConsumedCallback">The action that gets invoked when the item is consumed</param>
    /// <returns>A list of the items that may be used as crafting materials or null if none are available.</returns>
    public virtual IEnumerable<Item> AddMaterialsForCrafting(out ModPlayer.ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        return null;
    }

    /// <summary>
    /// Allows you to make special things happen when this player picks up an item. Return false to stop the item from being added to the player's inventory; returns true by default.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item being picked up</param>
    /// <returns></returns>
    public virtual bool OnPickup(Item item) => true;

    /// <summary>
    /// Whether or not the player can be teleported to the given coordinates with methods such as Teleportation Potions or the Rod of Discord.
    /// <para/> The coordinates correspond to the top left corner of the player position after teleporting.
    /// <para/> This gets called in <see cref="Player.CheckForGoodTeleportationSpot(ref bool, int, int, int, int, Player.RandomTeleportationAttemptSettings)"/> and <see cref="Player.ItemCheck_UseTeleportRod(Item)"/>. The <paramref name="context"/> will have a value of "CheckForGoodTeleportationSpot" or "TeleportRod" respectively indicating which type of teleport is being attempted.
    /// </summary>
    public virtual bool CanBeTeleportedTo(Vector2 teleportPosition, string context) => true;

    /// <summary>
    /// Allows to execute some code if the equipment loadout was switched.
    /// <br/><br/> Called on the server and on all clients.
    /// </summary>
    /// <param name="oldLoadoutIndex">The old loadout index.</param>
    /// <param name="loadoutIndex">The new loadout index.</param>
    public virtual void OnEquipmentLoadoutSwitched(int oldLoadoutIndex, int loadoutIndex) { }
    #endregion 虚成员
}

public abstract class GlobalNPCBehavior : GlobalEntityBehavior<NPC>
{
    /// <summary>
    /// Allows you to set the properties of any and every instance that gets created.
    /// </summary>
    public virtual void SetDefaults(NPC npc) { }

    /// <summary>
    /// Called after SetDefaults for NPCs with a negative <see cref="NPC.netID"/><br/>
    /// This hook is required because <see cref="NPC.SetDefaultsFromNetId"/> only sets <see cref="NPC.netID"/> after SetDefaults<br/>
    /// Remember that <see cref="NPC.type"/> does not support negative numbers and AppliesToEntity cannot distinguish between NPCs with the same type but different netID<br/>
    /// </summary>
    public virtual void SetDefaultsFromNetId(NPC npc) { }

    /// <summary>
    /// Gets called when any NPC spawns in world
    /// <para/> Called in single player or on the server only.
    /// </summary>
    public virtual void OnSpawn(NPC npc, IEntitySource source) { }

    /// <summary>
    /// Allows you to customize this NPC's stats when the difficulty is expert or higher.<br/>
    /// This runs after <see cref="NPC.value"/>,  <see cref="NPC.lifeMax"/>,  <see cref="NPC.damage"/>,  <see cref="NPC.knockBackResist"/> have been adjusted for the current difficulty, (expert/master/FTW)<br/>
    /// It is common to multiply lifeMax by the balance factor, and sometimes adjust knockbackResist.<br/>
    /// <br/>
    /// Eg:<br/>
    /// <code>lifeMax = (int)(lifeMax * balance * bossAdjustment)</code>
    /// </summary>
    /// <param name="npc">The newly spawned NPC</param>
    /// <param name="numPlayers">The number of active players</param>
    /// <param name="balance">Scaling factor that increases by a fraction for each player</param>
    /// <param name="bossAdjustment">An extra reduction factor to be applied to boss life in high difficulty modes</param>
    public virtual void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment) { }

    /// <summary>
    /// Allows you to set an NPC's information in the Bestiary.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="database"></param>
    /// <param name="bestiaryEntry"></param>
    public virtual void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }

    /// <summary>
    /// Allows you to modify the type name of this NPC dynamically.
    /// </summary>
    public virtual void ModifyTypeName(NPC npc, ref string typeName) { }

    /// <summary>
    /// Allows you to modify the bounding box for hovering over the given NPC (affects things like whether or not its name is displayed).
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc">The NPC in question.</param>
    /// <param name="boundingBox">The bounding box used for determining whether or not the NPC counts as being hovered over.</param>
    public virtual void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox) { }

    /// <summary>
    /// Allows you to set the town NPC profile that a given NPC uses.
    /// </summary>
    /// <param name="npc">The NPC in question.</param>
    /// <returns>The profile that you want the given NPC to use.<br></br>
    /// This will only influence their choice of profile if you do not return null.<br></br>
    /// By default, returns null, which causes no change.</returns>
    public virtual ITownNPCProfile ModifyTownNPCProfile(NPC npc) => null;

    /// <summary>
    /// Allows you to modify the list of names available to the given town NPC.
    /// </summary>
    public virtual void ModifyNPCNameList(NPC npc, List<string> nameList) { }

    /// <summary>
    /// This is where you reset any fields you add to your subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    public virtual void ResetEffects(NPC npc) { }

    /// <summary>
    /// Allows you to determine how any NPC behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
    /// <para/> Called on the server and clients.
    /// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public virtual bool PreAI(NPC npc) => true;

    /// <summary>
    /// Allows you to determine how any NPC behaves. This will only be called if PreAI returns true.
    /// <para/> Called on the server and clients.
    /// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
    /// </summary>
    /// <param name="npc"></param>
    public virtual void AI(NPC npc) { }

    /// <summary>
    /// Allows you to determine how any NPC behaves. This will be called regardless of what PreAI returns.
    /// <para/> Called on the server and clients.
    /// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
    /// </summary>
    /// <param name="npc"></param>
    public virtual void PostAI(NPC npc) { }

    /// <summary>
    /// Use this judiciously to avoid straining the network.
    /// <para/> Checks and methods such as <see cref="GlobalType{TEntity, TGlobal}.AppliesToEntity"/> can reduce how much data must be sent for how many projectiles.
    /// <para/> Called whenever <see cref="MessageID.SyncNPC"/> is successfully sent, for example on NPC creation, on player join, or whenever NPC.netUpdate is set to true in the update loop for that tick.
    /// <para/> Can be called on the server.
    /// </summary>
    /// <param name="npc">The NPC.</param>
    /// <param name="bitWriter">The compressible bit writer. Booleans written via this are compressed across all mods to improve multiplayer performance.</param>
    /// <param name="binaryWriter">The writer.</param>
    public virtual void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) { }

    /// <summary>
    /// Use this to receive information that was sent in <see cref="SendExtraAI"/>.
    /// <para/> Called whenever <see cref="MessageID.SyncNPC"/> is successfully received.
    /// <para/> Can be called on multiplayer clients.
    /// </summary>
    /// <param name="npc">The NPC.</param>
    /// <param name="bitReader">The compressible bit reader.</param>
    /// <param name="binaryReader">The reader.</param>
    public virtual void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) { }

    /// <summary>
    /// Allows you to modify the frame from an NPC's texture that is drawn, which is necessary in order to animate NPCs.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="frameHeight"></param>
    public virtual void FindFrame(NPC npc, int frameHeight) { }

    /// <summary>
    /// Allows you to make things happen whenever an NPC is hit, such as creating dust or gores.
    /// <para/> Called on local, server, and remote clients.
    /// <para/> Usually when something happens when an npc dies such as item spawning, you use NPCLoot, but you can use HitEffect paired with a check for <c>if (npc.life &lt;= 0)</c> to do client-side death effects, such as spawning dust, gore, or death sounds. <br/>
    /// </summary>
    public virtual void HitEffect(NPC npc, NPC.HitInfo hit) { }

    /// <summary>
    /// Allows you to make the NPC either regenerate health or take damage over time by setting <see cref="NPC.lifeRegen"/>. This is useful for implementing damage over time debuffs such as <see cref="BuffID.Poisoned"/> or <see cref="BuffID.OnFire"/>. Regeneration or damage will occur at a rate of half of <see cref="NPC.lifeRegen"/> per second.
    /// <para/> Essentially, modders implementing damage over time debuffs should subtract from <see cref="NPC.lifeRegen"/> a number that is twice as large as the intended damage per second. See <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/GlobalNPCs/DamageOverTimeGlobalNPC.cs#L16">DamageOverTimeGlobalNPC.cs</see> for an example of this.
    /// <para/> The damage parameter is the number that appears above the NPC's head if it takes damage over time.
    /// <para/> Multiple debuffs work together by following some conventions: <see cref="NPC.lifeRegen"/> should not be assigned a number, rather it should be subtracted from. <paramref name="damage"/> should only be assigned if the intended popup text is larger then its current value.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="damage"></param>
    public virtual void UpdateLifeRegen(NPC npc, ref int damage) { }

    /// <summary>
    /// Whether or not to run the code for checking whether an NPC will remain active. Return false to stop the NPC from being despawned and to stop the NPC from counting towards the limit for how many NPCs can exist near a player. Returns true by default.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public virtual bool CheckActive(NPC npc) => true;

    /// <summary>
    /// Whether or not an NPC should be killed when it reaches 0 health. You may program extra effects in this hook (for example, how Golem's head lifts up for the second phase of its fight). Return false to stop the NPC from being killed. Returns true by default.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public virtual bool CheckDead(NPC npc) => true;

    /// <summary>
    /// Allows you to call OnKill on your own when the NPC dies, rather then letting vanilla call it on its own. Returns false by default.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <returns>Return true to stop vanilla from calling OnKill on its own. Do this if you call OnKill yourself.</returns>
    public virtual bool SpecialOnKill(NPC npc) => false;

    /// <inheritdoc cref="ModNPC.PreKill"/>
    public virtual bool PreKill(NPC npc) => true;

    /// <summary>
    /// Allows you to make things happen when an NPC dies (for example, setting ModSystem fields). For client-side effects, such as dust, gore, and sounds, see HitEffect.
    /// <para/> Called in single player or on the server only.
    /// <para/> Most item drops should be done via drop rules registered in <see cref="ModifyNPCLoot(NPC, NPCLoot)"/> or <see cref="ModifyGlobalLoot(GlobalLoot)"/>. Some dynamic NPC drops, such as additional hearts, are more suited for OnKill instead. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/MinionBoss/MinionBossMinion.cs#L101">MinionBossMinion.cs</see> shows an example of an NPC that drops additional hearts. See <see cref="NPC.lastInteraction"/> and <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4#player-who-killed-npc">Player who killed NPC wiki section</see> as well for determining which players attacked or killed this NPC.
    /// </summary>
    /// <param name="npc"></param>
    public virtual void OnKill(NPC npc) { }

    /// <summary>
    /// Allows you to determine how and when an NPC can fall through platforms and similar tiles.
    /// <para/> Return true to allow an NPC to fall through platforms, false to prevent it. Returns null by default, applying vanilla behaviors (based on aiStyle and type).
    /// <para/> Called on the server and clients.
    /// </summary>
    public virtual bool? CanFallThroughPlatforms(NPC npc) => null;

    /// <summary>
    /// Allows you to determine whether the given item can catch the given NPC.<br></br>
    /// Return true or false to say the given NPC can or cannot be caught, respectively, regardless of vanilla rules.
    /// <para/> Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.
    /// <para/> If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.
    /// <para/> NOTE: this does not classify the given item as an NPC-catching tool, which is necessary for catching NPCs in the first place. To do that, you will need to use the "CatchingTool" set in ItemID.Sets.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc">The NPC that can potentially be caught.</param>
    /// <param name="item">The item with which the player is trying to catch the given NPC.</param>
    /// <param name="player">The player attempting to catch the given NPC.</param>
    /// <returns></returns>
    public virtual bool? CanBeCaughtBy(NPC npc, Item item, Player player) => null;

    /// <summary>
    /// Allows you to make things happen when the given item attempts to catch the given NPC.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc">The NPC which the player attempted to catch.</param>
    /// <param name="player">The player attempting to catch the given NPC.</param>
    /// <param name="item">The item used to catch the given NPC.</param>
    /// <param name="failed">Whether or not the given NPC has been successfully caught.</param>
    public virtual void OnCaughtBy(NPC npc, Player player, Item item, bool failed) { }

    /// <summary>
    /// Allows you to add and modify NPC loot tables to drop on death and to appear in the Bestiary.<br/>
    /// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4">Basic NPC Drops and Loot 1.4 Guide</see> explains how to use this hook to modify npc loot.
    /// <br/> This hook only runs once per npc type during mod loading, any dynamic behavior must be contained in the rules themselves.
    /// </summary>
    /// <param name="npc">A default npc of the type being opened, not the actual npc instance.</param>
    /// <param name="npcLoot">A reference to the item drop database for this npc type.</param>
    public virtual void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) { }

    /// <summary>
    /// Allows you to add and modify global loot rules that are conditional, i.e. vanilla's biome keys and souls.<br/>
    /// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4">Basic NPC Drops and Loot 1.4 Guide</see> explains how to use this hook to modify npc loot.
    /// </summary>
    /// <param name="globalLoot"></param>
    public virtual void ModifyGlobalLoot(GlobalLoot globalLoot) { }

    /// <summary>
    /// Allows you to determine whether an NPC can hit the given player. Return false to block the NPC from hitting the target. Returns true by default. CooldownSlot determines which of the player's cooldown counters (<see cref="ImmunityCooldownID"/>) to use, and defaults to -1 (<see cref="ImmunityCooldownID.General"/>).
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="target"></param>
    /// <param name="cooldownSlot"></param>
    /// <returns></returns>
    public virtual bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => true;

    /// <summary>
    /// Allows you to modify the damage, etc., that an NPC does to a player.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC hits a player (for example, inflicting debuffs).
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="target"></param>
    /// <param name="hurtInfo"></param>
    public virtual void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) { }

    /// <summary>
    /// Allows you to determine whether an NPC can hit the given friendly NPC. Return false to block the NPC from hitting the target, and return true to use the vanilla code for whether the target can be hit. Returns true by default.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool CanHitNPC(NPC npc, NPC target) => true;

    /// <summary>
    /// Allows you to determine whether a friendly NPC can be hit by an NPC. Return false to block the attacker from hitting the NPC, and return true to use the vanilla code for whether the target can be hit. Returns true by default.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="attacker"></param>
    /// <returns></returns>
    public virtual bool CanBeHitByNPC(NPC npc, NPC attacker) => true;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that an NPC does to a friendly NPC.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC hits a friendly NPC.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="target"></param>
    /// <param name="hit"></param>
    public virtual void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit) { }

    /// <summary>
    /// Allows you to determine whether an NPC can be hit by the given melee weapon when swung. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual bool? CanBeHitByItem(NPC npc, Player player, Item item) => null;

    /// <summary>
    /// Allows you to determine whether an NPC can be collided with the player melee weapon when swung.
    /// <para/> Use <see cref="CanBeHitByItem(NPC, Player, Item)"/> instead for Guide Voodoo Doll-type effects.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc">The NPC being collided with</param>
    /// <param name="player">The player wielding this item.</param>
    /// <param name="item">The weapon item the player is holding.</param>
    /// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
    /// <returns>
    /// Return true to allow colliding with the melee attack, return false to block the weapon from colliding with the NPC, and return null to use the vanilla code for whether the attack can be colliding. Returns null by default.
    /// </returns>
    public virtual bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that an NPC takes from a melee weapon.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC is hit by a melee weapon.
    /// <para/> Called on the client doing the damage.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether an NPC can be hit by the given projectile. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public virtual bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that an NPC takes from a projectile.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="projectile"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC is hit by a projectile.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="projectile"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to use a custom damage formula for when an NPC takes damage from any source. For example, you can change the way defense works or use a different crit multiplier.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// <para/> Can be called on the local client or server, depending on who is dealing damage.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to customize the boss head texture used by an NPC based on its state. Set index to -1 to stop the texture from being displayed.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="index">The index for NPCID.Sets.BossHeadTextures</param>
    public virtual void BossHeadSlot(NPC npc, ref int index) { }

    /// <summary>
    /// Allows you to customize the rotation of an NPC's boss head icon on the map.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="rotation"></param>
    public virtual void BossHeadRotation(NPC npc, ref float rotation) { }

    /// <summary>
    /// Allows you to flip an NPC's boss head icon on the map.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="spriteEffects"></param>
    public virtual void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects) { }

    /// <summary>
    /// Allows you to determine the color and transparency in which an NPC is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="drawColor"></param>
    /// <returns></returns>
    public virtual Color? GetAlpha(NPC npc, Color drawColor) => null;

    /// <summary>
    /// Allows you to add special visual effects to an NPC (such as creating dust), and modify the color in which the NPC is drawn.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="drawColor"></param>
    public virtual void DrawEffects(NPC npc, ref Color drawColor) { }

    /// <summary>
    /// Allows you to draw things behind an NPC, or to modify the way the NPC is drawn. Substract screenPos from the draw position before drawing. Return false to stop the game from drawing the NPC (useful if you're manually drawing the NPC). Returns true by default.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc">The NPC that is being drawn</param>
    /// <param name="spriteBatch">The spritebatch to draw on</param>
    /// <param name="screenPos">The screen position used to translate world position into screen position</param>
    /// <param name="drawColor">The color the NPC is drawn in</param>
    /// <returns></returns>
    public virtual bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

    /// <summary>
    /// Allows you to draw things in front of this NPC. Substract screenPos from the draw position before drawing. This method is called even if PreDraw returns false.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc">The NPC that is being drawn</param>
    /// <param name="spriteBatch">The spritebatch to draw on</param>
    /// <param name="screenPos">The screen position used to translate world position into screen position</param>
    /// <param name="drawColor">The color the NPC is drawn in</param>
    public virtual void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <summary>
    /// When used in conjunction with "npc.hide = true", allows you to specify that this npc should be drawn behind certain elements. Add the index to one of Main.DrawCacheNPCsMoonMoon, DrawCacheNPCsOverPlayers, DrawCacheNPCProjectiles, or DrawCacheNPCsBehindNonSolidTiles.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="index"></param>
    public virtual void DrawBehind(NPC npc, int index) { }

    /// <summary>
    /// Allows you to control how the health bar for the given NPC is drawn. The hbPosition parameter is the same as Main.hbPosition; it determines whether the health bar gets drawn above or below the NPC by default. The scale parameter is the health bar's size. By default, it will be the normal 1f; most bosses set this to 1.5f. Return null to let the normal vanilla health-bar-drawing code to run. Return false to stop the health bar from being drawn. Return true to draw the health bar in the position specified by the position parameter (note that this is the world position, not screen position).
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="hbPosition"></param>
    /// <param name="scale"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public virtual bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position) => null;

    /// <summary>
    /// Allows you to modify the chance of NPCs spawning around the given player and the maximum number of NPCs that can spawn around the player. Lower spawnRates mean a higher chance for NPCs to spawn.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="spawnRate"></param>
    /// <param name="maxSpawns"></param>
    public virtual void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) { }

    /// <summary>
    /// Allows you to modify the range at which NPCs can spawn around the given player. The spawnRanges determine that maximum distance NPCs can spawn from the player, and the safeRanges determine the minimum distance.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="spawnRangeX"></param>
    /// <param name="spawnRangeY"></param>
    /// <param name="safeRangeX"></param>
    /// <param name="safeRangeY"></param>
    public virtual void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY) { }

    /// <summary>
    /// Allows you to control which NPCs can spawn and how likely each one is to spawn. The pool parameter maps NPC types to their spawning weights (likelihood to spawn compared to other NPCs). A type of 0 in the pool represents the default vanilla NPC spawning.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="spawnInfo"></param>
    public virtual void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) { }

    /// <summary>
    /// Allows you to customize an NPC (for example, its position or ai array) after it naturally spawns and before it is synced between servers and clients. As of right now, this only works for modded NPCs.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="tileX"></param>
    /// <param name="tileY"></param>
    public virtual void SpawnNPC(int npc, int tileX, int tileY) { }

    /// <summary>
    /// Allows you to determine whether this NPC can talk with the player. Return true to allow talking with the player, return false to block this NPC from talking with the player, and return null to use the vanilla code for whether the NPC can talk. Returns null by default.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public virtual bool? CanChat(NPC npc) => null;

    /// <summary>
    /// Allows you to modify the chat message of any NPC that the player can talk to.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="chat"></param>
    public virtual void GetChat(NPC npc, ref string chat) { }

    /// <summary>
    /// Allows you to determine if something can happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked. Return false to prevent the normal code for this button from running. Returns true by default.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="firstButton"></param>
    /// <returns></returns>
    public virtual bool PreChatButtonClicked(NPC npc, bool firstButton) => true;

    /// <summary>
    /// Allows you to make something happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="firstButton"></param>
    public virtual void OnChatButtonClicked(NPC npc, bool firstButton) { }

    /// <summary>
    /// Allows you to modify existing shop. Be aware that this hook is called just one time during loading.
    /// <para/> The traveling merchant shop is handled separately in <see cref="SetupTravelShop(int[], ref int)"/>.
    /// </summary>
    /// <param name="shop">A <seealso cref="NPCShop"/> instance.</param>
    public virtual void ModifyShop(NPCShop shop) { }

    /// <summary>
    /// Allows you to modify the contents of a shop whenever player opens it. <br/>
    /// If possible, use <see cref="ModifyShop(NPCShop)"/> instead, to reduce mod conflicts and improve compatibility.
    /// Note that for special shops like travelling merchant, the <paramref name="shopName"/> may not correspond to a <see cref="NPCShop"/> in the <see cref="NPCShopDatabase"/>
    /// <para/> Also note that unused slots in <paramref name="items"/> are null while <see cref="Item.IsAir"/> entries are entries that have a reserved slot (<see cref="NPCShop.Entry.SlotReserved"/>) but did not have their conditions met. These should not be overwritten.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="npc">An instance of <seealso cref="NPC"/> that currently player talks to.</param>
    /// <param name="shopName">The full name of the shop being opened. See <see cref="NPCShopDatabase.GetShopName"/> for the format. </param>
    /// <param name="items">Items in the shop including 'air' items in empty slots.</param>
    public virtual void ModifyActiveShop(NPC npc, string shopName, Item[] items) { }

    /// <summary>
    /// Allows you to add items to the traveling merchant's shop. Add an item by setting shop[nextSlot] to the ID of the item you are adding then incrementing nextSlot. In the end, nextSlot must have a value of 1 greater than the highest index in shop that represents an item ID. If you want to remove an item, you will have to be familiar with programming.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="shop"></param>
    /// <param name="nextSlot"></param>
    public virtual void SetupTravelShop(int[] shop, ref int nextSlot) { }

    /// <summary>
    /// Whether this NPC can be teleported to a King or Queen statue. Return true to allow the NPC to teleport to the statue, return false to block this NPC from teleporting to the statue, and return null to use the vanilla code for whether the NPC can teleport to the statue. Returns null by default.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="npc">The NPC</param>
    /// <param name="toKingStatue">Whether the NPC is being teleported to a King or Queen statue.</param>
    public virtual bool? CanGoToStatue(NPC npc, bool toKingStatue) => null;

    /// <summary>
    /// Allows you to make things happen when this NPC teleports to a King or Queen statue.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="npc">The NPC</param>
    /// <param name="toKingStatue">Whether the NPC was teleported to a King or Queen statue.</param>
    public virtual void OnGoToStatue(NPC npc, bool toKingStatue) { }

    /// <summary>
    /// Allows you to modify the stats of town NPCs. Useful for buffing town NPCs when certain bosses are defeated, etc.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="damageMult"></param>
    /// <param name="defense"></param>
    public virtual void BuffTownNPC(ref float damageMult, ref int defense) { }

    /// <summary>
    /// Allows you to modify the death message of a town NPC or boss. This also affects what the dropped tombstone will say in the case of a town NPC. The text color can also be modified.
    /// <para/> When modifying the death message, use <see cref="NPC.GetFullNetName"/> to retrieve the NPC name to use in substitutions.
    /// <para/> Return false to skip the vanilla code for sending the message. This is useful if the death message is handled by this method or if the message should be skipped for any other reason, such as if there are multiple bosses. Returns true by default.
    /// </summary>
    public virtual bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color) => true;

    /// <summary>
    /// Allows you to determine the damage and knockback of a town NPC's attack before the damage is scaled. (More information on scaling in GlobalNPC.BuffTownNPCs.)
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="damage"></param>
    /// <param name="knockback"></param>
    public virtual void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback) { }

    /// <summary>
    /// Allows you to determine the cooldown between each of a town NPC's attack. The cooldown will be a number greater than or equal to the first parameter, and less then the sum of the two parameters.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="cooldown"></param>
    /// <param name="randExtraCooldown"></param>
    public virtual void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown) { }

    /// <summary>
    /// Allows you to determine the projectile type of a town NPC's attack, and how long it takes for the projectile to actually appear. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="projType"></param>
    /// <param name="attackDelay"></param>
    public virtual void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay) { }

    /// <summary>
    /// Allows you to determine the speed at which a town NPC throws a projectile when it attacks. Multiplier is the speed of the projectile, gravityCorrection is how much extra the projectile gets thrown upwards, and randomOffset allows you to randomize the projectile's velocity in a square centered around the original velocity. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="multiplier"></param>
    /// <param name="gravityCorrection"></param>
    /// <param name="randomOffset"></param>
    public virtual void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset) { }

    /// <summary>
    /// Allows you to tell the game that a town NPC has already created a projectile and will still create more projectiles as part of a single attack so that the game can animate the NPC's attack properly. Only used when the town NPC has an attack type of 1 (shooting).
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="inBetweenShots"></param>
    public virtual void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots) { }

    /// <summary>
    /// Allows you to control the brightness of the light emitted by a town NPC's aura when it performs a magic attack. Only used when the town NPC has an attack type of 2 (magic)
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="auraLightMultiplier"></param>
    public virtual void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier) { }

    /// <summary>
    /// Allows you to determine the width and height of the item a town NPC swings when it attacks, which controls the range of the NPC's swung weapon. Only used when the town NPC has an attack type of 3 (swinging).
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="itemWidth"></param>
    /// <param name="itemHeight"></param>
    public virtual void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight) { }

    /// <summary>
    /// Allows you to customize how a town NPC's weapon is drawn when the NPC is shooting (the NPC must have an attack type of 1). <paramref name="scale"/> is a multiplier for the item's drawing size, <paramref name="item"/> is the Texture2D instance of the item to be drawn, <paramref name="itemFrame"/> is the section of the texture to draw, and <paramref name="horizontalHoldoutOffset"/> is how far away the item should be drawn from the NPC.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="item"></param>
    /// <param name="itemFrame"></param>
    /// <param name="scale"></param>
    /// <param name="horizontalHoldoutOffset"></param>
    public virtual void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) { }


    /// <inheritdoc cref="ModNPC.DrawTownAttackSwing" />
    public virtual void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) { }

    /// <summary>
    /// Allows you to modify the NPC's <seealso cref="ID.ImmunityCooldownID"/>, damage multiplier, and hitbox. Useful for implementing dynamic damage hitboxes that change in dimensions or deal extra damage. Returns false to prevent vanilla code from running. Returns true by default.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="victimHitbox"></param>
    /// <param name="immunityCooldownSlot"></param>
    /// <param name="damageMultiplier"></param>
    /// <param name="npcHitbox"></param>
    /// <returns></returns>
    public virtual bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => true;

    /// <summary>
    /// Allows you to make a npc be saved even if it's not a townNPC and NPCID.Sets.SavesAndLoads[npc.type] is false.
    /// <br/><b>NOTE:</b> A town NPC will always be saved (except the Travelling Merchant that never will).
    /// <br/><b>NOTE:</b> A NPC that needs saving will not despawn naturally.
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public virtual bool NeedSaving(NPC npc) => false;

    /// <summary>
    /// Allows you to save custom data for the given npc.
    /// <br/>
    /// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
    /// <br/><b>NOTE:</b> Try to only save data that isn't default values.
    /// <br/><b>NOTE:</b> The npc may be saved even if NeedSaving returns false and npc is not a townNPC, if another mod returns true on NeedSaving.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="tag">The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument</param>
    public virtual void SaveData(NPC npc, TagCompound tag) { }

    /// <summary>
    /// Allows you to load custom data that you have saved for the given npc.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="tag"></param>
    public virtual void LoadData(NPC npc, TagCompound tag) { }

    /// <summary>
    /// Allows you to change the emote that the NPC will pick
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="closestPlayer">The <see cref="Player"/> closest to the NPC. You can check the biome the player is in and let the NPC pick the emote that corresponds to the biome.</param>
    /// <param name="emoteList">A list of emote IDs from which the NPC will randomly select one</param>
    /// <param name="otherAnchor">A <see cref="WorldUIAnchor"/> instance that indicates the target of this emote conversation. Use this to get the instance of the <see cref="NPC"/> or <see cref="Player"/> this NPC is talking to.</param>
    /// <returns>Return null to use vanilla mechanic (pick one from the list), otherwise pick the emote by the returned ID. Returning -1 will prevent the emote from being used. Returns null by default</returns>
    public virtual int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => null;

    /// <inheritdoc cref="ModNPC.ChatBubblePosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="ModNPC.PartyHatPosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="ModNPC.EmoteBubblePosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }
}

public abstract class GlobalProjectileBehavior : GlobalEntityBehavior<Projectile>
{
    /// <summary>
    /// Allows you to set the properties of any and every instance that gets created.
    /// </summary>
    public virtual void SetDefaults(Projectile projectile) { }

    /// <summary>
    /// Gets called when any projectiles spawns in world
    /// <para/> Called on the client or server spawning the projectile via Projectile.NewProjectile.
    /// </summary>
    public virtual void OnSpawn(Projectile projectile, IEntitySource source) { }

    /// <summary>
    /// Allows you to determine how any projectile behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
    /// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public virtual bool PreAI(Projectile projectile) => true;

    /// <summary>
    /// Allows you to determine how any projectile behaves. This will only be called if PreAI returns true.
    /// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    public virtual void AI(Projectile projectile) { }

    /// <summary>
    /// Allows you to determine how any projectile behaves. This will be called regardless of what PreAI returns.
    /// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    public virtual void PostAI(Projectile projectile) { }

    /// <summary>
    /// Use this judiciously to avoid straining the network.
    /// <para/> Checks and methods such as <see cref="GlobalType{TEntity, TGlobal}.AppliesToEntity"/> can reduce how much data must be sent for how many projectiles.
    /// <para/> Called whenever <see cref="MessageID.SyncProjectile"/> is successfully sent, for example on projectile creation, or whenever Projectile.netUpdate is set to true in the update loop for that tick.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile">The projectile.</param>
    /// <param name="bitWriter">The compressible bit writer. Booleans written via this are compressed across all mods to improve multiplayer performance.</param>
    /// <param name="binaryWriter">The writer.</param>
    public virtual void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) { }

    /// <summary>
    /// Use this to receive information that was sent in <see cref="SendExtraAI"/>.
    /// <para/> Called whenever <see cref="MessageID.SyncProjectile"/> is successfully received.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile">The projectile.</param>
    /// <param name="bitReader">The compressible bit reader.</param>
    /// <param name="binaryReader">The reader.</param>
    public virtual void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) { }

    /// <summary>
    /// Whether or not the given projectile should update its position based on factors such as its velocity, whether it is in liquid, etc. Return false to make its velocity have no effect on its position. Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public virtual bool ShouldUpdatePosition(Projectile projectile) => true;

    /// <summary>
    /// Allows you to determine how a projectile interacts with tiles. Return false if you completely override or cancel a projectile's tile collision behavior. Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"> The projectile. </param>
    /// <param name="width"> The width of the hitbox the projectile will use for tile collision. If vanilla or a mod don't modify it, defaults to projectile.width. </param>
    /// <param name="height"> The height of the hitbox the projectile will use for tile collision. If vanilla or a mod don't modify it, defaults to projectile.height. </param>
    /// <param name="fallThrough"> Whether or not the projectile falls through platforms and similar tiles. </param>
    /// <param name="hitboxCenterFrac"> Determines by how much the tile collision hitbox's position (top left corner) will be offset from the projectile's real center. If vanilla or a mod don't modify it, defaults to half the hitbox size (new Vector2(0.5f, 0.5f)). </param>
    /// <returns></returns>
    public virtual bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

    /// <summary>
    /// Allows you to determine what happens when a projectile collides with a tile. OldVelocity is the velocity before tile collision. The velocity that takes tile collision into account can be found with projectile.velocity. Return true to allow the vanilla tile collision code to take place (which normally kills the projectile). Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="oldVelocity"></param>
    /// <returns></returns>
    public virtual bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) => true;

    /// <summary>
    /// Allows you to determine whether the vanilla code for Kill and the Kill hook will be called. Return false to stop them from being called. Returns true by default. Note that this does not stop the projectile from dying.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="timeLeft"></param>
    /// <returns></returns>
    public virtual bool PreKill(Projectile projectile, int timeLeft) => true;

    /// <summary>
    /// Allows you to control what happens when a projectile is killed (for example, creating dust or making sounds).
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="timeLeft"></param>
    public virtual void OnKill(Projectile projectile, int timeLeft) { }

    /// <summary>
    /// Return true or false to specify if the projectile can cut tiles like vines, pots, and Queen Bee larva. Return null for vanilla decision.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public virtual bool? CanCutTiles(Projectile projectile) => null;

    /// <summary>
    /// Code ran when the projectile cuts tiles. Only runs if CanCutTiles() returns true. Useful when programming lasers and such.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile"></param>
    public virtual void CutTiles(Projectile projectile) { }

    /// <summary>
    /// Whether or not the given projectile is capable of killing tiles (such as grass) and damaging NPCs/players.
    /// Return false to prevent it from doing any sort of damage.
    /// Return true if you want the projectile to do damage regardless of the default blacklist.
    /// Return null to let the projectile follow vanilla can-damage-anything rules. This is what happens by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public virtual bool? CanDamage(Projectile projectile) => null;

    /// <summary>
    /// Whether or not a minion can damage NPCs by touching them. Returns false by default. Note that this will only be used if the projectile is considered a pet.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public virtual bool MinionContactDamage(Projectile projectile) => false;

    /// <summary>
    /// Allows you to change the hitbox used by a projectile for damaging players and NPCs.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="hitbox"></param>
    public virtual void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) { }

    /// <summary>
    /// Allows you to determine whether a projectile can hit the given NPC. Return true to allow hitting the target, return false to block the projectile from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool? CanHitNPC(Projectile projectile, NPC target) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that a projectile does to an NPC.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a projectile hits an NPC (for example, inflicting debuffs).
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="target"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether a projectile can hit the given opponent player. Return false to block the projectile from hitting the target. Returns true by default.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool CanHitPvp(Projectile projectile, Player target) => true;

    /// <summary>
    /// Allows you to determine whether a hostile projectile can hit the given player. Return false to block the projectile from hitting the target. Returns true by default.
    /// <para/> Called on the server only.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool CanHitPlayer(Projectile projectile, Player target) => true;

    /// <summary>
    /// Allows you to modify the damage, etc., that a hostile projectile does to a player.
    /// <para/> Called on the client taking damage.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a hostile projectile hits a player. <br/>
    /// <para/> Called on the client taking damage.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="target"></param>
    /// <param name="info"></param>
    public virtual void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) { }

    /// <summary>
    /// Allows you to use custom collision detection between a projectile and a player or NPC that the projectile can damage. Useful for things like diagonal lasers, projectiles that leave a trail behind them, etc.
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="projHitbox"></param>
    /// <param name="targetHitbox"></param>
    /// <returns></returns>
    public virtual bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) => null;

    /// <summary>
    /// Allows you to determine the color and transparency in which a projectile is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="lightColor"></param>
    /// <returns></returns>
    public virtual Color? GetAlpha(Projectile projectile, Color lightColor) => null;

    /// <summary>
    /// Allows you to draw things behind a projectile. Use the <c>Main.EntitySpriteDraw</c> method for drawing. Returns false to stop the game from drawing extras textures related to the projectile (for example, the chains for grappling hooks), useful if you're manually drawing the extras. Returns true by default.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="projectile"> The projectile. </param>
    public virtual bool PreDrawExtras(Projectile projectile) => true;

    /// <summary>
    /// Allows you to draw things behind a projectile, or to modify the way the projectile is drawn. Use the <c>Main.EntitySpriteDraw</c> method for drawing. Return false to stop the vanilla projectile drawing code (useful if you're manually drawing the projectile). Returns true by default.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="projectile"> The projectile. </param>
    /// <param name="lightColor"> The color of the light at the projectile's center. </param>
    public virtual bool PreDraw(Projectile projectile, ref Color lightColor) => true;

    /// <summary>
    /// Allows you to draw things in front of a projectile. Use the <c>Main.EntitySpriteDraw</c> method for drawing. This method is called even if PreDraw returns false.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="projectile"> The projectile. </param>
    /// <param name="lightColor"> The color of the light at the projectile's center, after being modified by vanilla and other mods. </param>
    public virtual void PostDraw(Projectile projectile, Color lightColor) { }

    /// <summary>
    /// When used in conjunction with "projectile.hide = true", allows you to specify that this projectile should be drawn behind certain elements. Add the index to one and only one of the lists. For example, the Nebula Arcanum projectile draws behind NPCs and tiles.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="index"></param>
    /// <param name="behindNPCsAndTiles"></param>
    /// <param name="behindNPCs"></param>
    /// <param name="behindProjectiles"></param>
    /// <param name="overPlayers"></param>
    /// <param name="overWiresUI"></param>
    public virtual void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) { }

    /// <summary>
    /// Whether or not a grappling hook that shoots this type of projectile can be used by the given player. Return null to use the default code (whether or not the player is in the middle of firing the grappling hook). Returns null by default.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual bool? CanUseGrapple(int type, Player player) => null;

    /// <summary>
    /// This code is called whenever the player uses a grappling hook that shoots this type of projectile. Use it to change what kind of hook is fired (for example, the Dual Hook does this), to kill old hook projectiles, etc.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void UseGrapple(Player player, ref int type) { }

    /// <summary>
    /// How many of this type of grappling hook the given player can latch onto blocks before the hooks start disappearing. Change the numHooks parameter to determine this; by default it will be 3.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks) { }

    /// <summary>
    /// The speed at which the grapple retreats back to the player after not hitting anything. Defaults to 11, but vanilla hooks go up to 24.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed) { }

    /// <summary>
    /// The speed at which the grapple pulls the player after hitting something. Defaults to 11, but the Bat Hook uses 16.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void GrapplePullSpeed(Projectile projectile, Player player, ref float speed) { }

    /// <summary>
    /// The location that the grappling hook pulls the player to. Defaults to the center of the hook projectile.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY) { }

    /// <summary>
    /// Whether or not the grappling hook can latch onto the given position in tile coordinates.
    /// <para/> This position may be air or an actuated tile!
    /// <para/> Return true to make it latch, false to prevent it, or null to apply vanilla conditions. Returns null by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y) => null;

    /// <inheritdoc cref="ModProjectile.PrepareBombToBlow"/>
    public virtual void PrepareBombToBlow(Projectile projectile) { }

    /// <inheritdoc cref="ModProjectile.EmitEnchantmentVisualsAt"/>
    public virtual void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) { }
}

public abstract class GlobalItemBehavior : GlobalEntityBehavior<Item>
{
    /// <summary>
    /// Allows you to set the properties of any and every instance that gets created.
    /// </summary>
    public virtual void SetDefaults(Item item) { }

    /// <summary>
    /// Called when the <paramref name="item"/> is created. The <paramref name="context"/> parameter indicates the context of the item creation and can be used in logic for the desired effect.
    /// <para/> Called on the local client only.
    /// <para/> Known <see cref="ItemCreationContext"/> include: <see cref="InitializationItemCreationContext"/>, <see cref="BuyItemCreationContext"/>, <see cref="JourneyDuplicationItemCreationContext"/>, and <see cref="RecipeItemCreationContext"/>. Some of these provide additional context such as how <see cref="RecipeItemCreationContext"/> includes the items consumed to craft the <paramref name="item"/>.
    /// </summary>
    public virtual void OnCreated(Item item, ItemCreationContext context) { }

    /// <summary>
    /// Gets called when any item spawns in world
    /// <para/> Called on the local client or the server where Item.NewItem is called.
    /// </summary>
    public virtual void OnSpawn(Item item, IEntitySource source) { }

    /// <summary>
    /// Allows you to manually choose what prefix an item will get.
    /// </summary>
    /// <returns>The ID of the prefix to give the item, -1 to use default vanilla behavior</returns>
    public virtual int ChoosePrefix(Item item, UnifiedRandom rand) => -1;

    /// <summary>
    /// To prevent putting the item in the tinkerer slot, return false when pre is -3.
    /// To prevent rolling of a prefix on spawn, return false when pre is -1.
    /// To force rolling of a prefix on spawn, return true when pre is -1.
    ///
    /// To reduce the probability of a prefix on spawn (pre == -1) to X%, return false 100-4X % of the time.
    /// To increase the probability of a prefix on spawn (pre == -1) to X%, return true (4X-100)/3 % of the time.
    ///
    /// To delete a prefix from an item when the item is loaded, return false when pre is the prefix you want to delete.
    /// Use AllowPrefix to prevent rolling of a certain prefix.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="pre">The prefix being applied to the item, or the roll mode. -1 is when the item is naturally generated in a chest, crafted, purchased from an NPC, looted from a grab bag (excluding presents), or dropped by a slain enemy (if it's spawned with prefixGiven: -1). -2 is when the item is rolled in the tinkerer. -3 determines if the item can be placed in the tinkerer slot.</param>
    /// <param name="rand"></param>
    /// <returns></returns>
    public virtual bool? PrefixChance(Item item, int pre, UnifiedRandom rand) => null;

    /// <summary>
    /// Force a re-roll of a prefix by returning false.
    /// </summary>
    public virtual bool AllowPrefix(Item item, int pre) => true;

    /// <summary>
    /// Returns whether or not any item can be used. Returns true by default. The inability to use a specific item overrides this, so use this to stop an item from being used.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool CanUseItem(Item item, Player player) => true;

    /// <summary>
    /// Allows you to modify the autoswing (auto-reuse) behavior of any item without having to mess with Item.autoReuse.
    /// <para/> Useful to create effects like the Feral Claws which makes melee weapons and whips auto-reusable.
    /// <para/> Return true to enable autoswing (if not already enabled through autoReuse), return false to prevent autoswing. Returns null by default, which applies vanilla behavior.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item"> The item. </param>
    /// <param name="player"> The player. </param>
    public virtual bool? CanAutoReuseItem(Item item, Player player) => null;

    /// <summary>
    /// Allows you to modify the location and rotation of any item in its use animation.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item"> The item. </param>
    /// <param name="player"> The player. </param>
    /// <param name="heldItemFrame"> The source rectangle for the held item's texture. </param>
    public virtual void UseStyle(Item item, Player player, Rectangle heldItemFrame) { }

    /// <summary>
    /// Allows you to modify the location and rotation of the item the player is currently holding.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item"> The item. </param>
    /// <param name="player"> The player. </param>
    /// <param name="heldItemFrame"> The source rectangle for the held item's texture. </param>
    public virtual void HoldStyle(Item item, Player player, Rectangle heldItemFrame) { }

    /// <summary>
    /// Allows you to make things happen when the player is holding an item (for example, torches make light and water candles increase spawn rate).
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void HoldItem(Item item, Player player) { }

    /// <summary>
    /// Allows you to change the effective useTime of an item.
    /// <para/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <returns> The multiplier on the usage time. 1f by default. Values greater than 1 increase the item use's length. </returns>
    public virtual float UseTimeMultiplier(Item item, Player player) => 1f;

    /// <summary>
    /// Allows you to change the effective useAnimation of an item.
    /// <para/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <returns>The multiplier on the animation time. 1f by default. Values greater than 1 increase the item animation's length. </returns>
    public virtual float UseAnimationMultiplier(Item item, Player player) => 1f;

    /// <summary>
    /// Allows you to safely change both useTime and useAnimation while keeping the values relative to each other.
    /// <para/> Useful for status effects.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <returns> The multiplier on the use speed. 1f by default. Values greater than 1 increase the overall item speed. </returns>
    public virtual float UseSpeedMultiplier(Item item, Player player) => 1f;

    /// <summary>
    /// Allows you to temporarily modify the amount of life a life healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healLife"/> value.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
    /// <param name="healValue">The amount of life being healed.</param>
    public virtual void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue) { }

    /// <summary>
    /// Allows you to temporarily modify the amount of mana a mana healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healMana"/> value.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
    /// <param name="healValue">The amount of mana being healed.</param>
    public virtual void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue) { }

    /// <summary>
    /// Allows you to temporarily modify the amount of mana an item will consume on use, based on player buffs, accessories, etc. This is only called for items with a mana value.
    /// <para/> <b>Do not</b> modify <see cref="Item.mana"/>, modify the <paramref name="reduce"/> and <paramref name="mult"/> parameters.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="reduce">Used for decreasingly stacking buffs (most common). Only ever use -= on this field.</param>
    /// <param name="mult">Use to directly multiply the item's effective mana cost. Good for debuffs, or things which should stack separately (eg meteor armor set bonus).</param>
    public virtual void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult) { }

    /// <summary>
    /// Allows you to make stuff happen when a player doesn't have enough mana for an item they are trying to use.
    /// If the player has high enough mana after this hook runs, mana consumption will happen normally.
    /// Only runs once per item use.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="neededMana">The mana needed to use the item.</param>
    public virtual void OnMissingMana(Item item, Player player, int neededMana) { }

    /// <summary>
    /// Allows you to make stuff happen when a player consumes mana on use of an item.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="manaConsumed">The mana consumed from the player.</param>
    public virtual void OnConsumeMana(Item item, Player player, int manaConsumed) { }

    /// <summary>
    /// Allows you to dynamically modify a weapon's damage based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <para/> <b>Do not</b> modify <see cref="Item.damage"/>, modify the <paramref name="damage"/> parameter.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="damage">The StatModifier object representing the totality of the various modifiers to be applied to the item's base damage.</param>
    public virtual void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) { }

    /// <summary>
    /// Allows you to set an item's sorting group in Journey Mode's duplication menu. This is useful for setting custom item types that group well together, or whenever the default vanilla sorting doesn't sort the way you want it.
    /// <para/> Note that this affects the order of the item in the listing, not which filters the item satisfies.
    /// </summary>
    /// <param name="item">The item being used</param>
    /// <param name="itemGroup">The item group this item is being assigned to</param>
    public virtual void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) { }

    /// <summary>
    /// Allows you to choose if a given bait will be consumed by a given player
    /// Not consuming will always take priority over forced consumption
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="bait">The bait being used</param>
    /// <param name="player">The player using the item</param>
    public virtual bool? CanConsumeBait(Player player, Item bait) => null;

    /// <summary>
    /// Allows you to prevent an item from being researched by returning false. True is the default behavior.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item being researched</param>
    public virtual bool CanResearch(Item item) => true;

    /// <summary>
    /// Allows you to create custom behavior when an item is accepted by the Research function
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item being researched</param>
    /// <param name="fullyResearched">True if the item was completely researched, and is ready to be duplicated, false if only partially researched.</param>
    public virtual void OnResearched(Item item, bool fullyResearched) { }

    /// <summary>
    /// Allows you to dynamically modify a weapon's knockback based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <para/> <b>Do not</b> modify <see cref="Item.knockBack"/>, modify the <paramref name="knockback"/> parameter.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="knockback">The StatModifier object representing the totality of the various modifiers to be applied to the item's base knockback.</param>
    public virtual void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { }

    /// <summary>
    /// Allows you to dynamically modify a weapon's crit chance based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <para/> <b>Do not</b> modify <see cref="Item.crit"/>, modify the <paramref name="crit"/> parameter.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="crit">The total crit chance of the item after all normal crit chance calculations.</param>
    public virtual void ModifyWeaponCrit(Item item, Player player, ref float crit) { }

    /// <summary>
    /// Whether or not having no ammo prevents an item that uses ammo from shooting.
    /// Return false to allow shooting with no ammo in the inventory, in which case the item will act as if the default ammo for it is being used.
    /// Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool NeedsAmmo(Item item, Player player) => true;

    /// <summary>
    /// Allows you to modify various properties of the projectile created by a weapon based on the ammo it is using.
    /// <para/> Called on local and remote clients when a player picking ammo but only on the local client when held projectiles are picking ammo.
    /// </summary>
    /// <param name="weapon">The item that is using the given ammo.</param>
    /// <param name="ammo">The ammo item being used by the given weapon.</param>
    /// <param name="player">The player using the item.</param>
    /// <param name="type">The ID of the fired projectile.</param>
    /// <param name="speed">The speed of the fired projectile.</param>
    /// <param name="damage">
    /// The damage modifier for the projectile.<br></br>
    /// Total weapon damage is included as Flat damage.<br></br>
    /// Be careful not to apply flat or base damage bonuses which are already applied to the weapon.
    /// </param>
    /// <param name="knockback">The knockback of the fired projectile.</param>
    public virtual void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) { }

    /// <summary>
    /// Whether or not the given ammo item is valid for the given weapon; called on the weapon. If this, or <see cref="CanBeChosenAsAmmo"/> on the ammo, returns false, then the ammo will not be valid for this weapon.
    /// <para/> By default, returns null and allows <see cref="Item.useAmmo"/> and <see cref="Item.ammo"/> to decide. Return true to make the ammo valid regardless of these fields, and return false to make it invalid.
    /// <para/> If false is returned, the <see cref="CanConsumeAmmo"/>, <see cref="CanBeConsumedAsAmmo"/>, <see cref="OnConsumeAmmo"/>, and <see cref="OnConsumedAsAmmo"/> hooks are never called.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="weapon">The weapon that this hook is being called for.</param>
    /// <param name="ammo">The ammo that the weapon is attempting to select.</param>
    /// <param name="player">The player which this weapon and the potential ammo belong to.</param>
    /// <returns></returns>
    public virtual bool? CanChooseAmmo(Item weapon, Item ammo, Player player) => null;

    /// <summary>
    /// Whether or not the given ammo item is valid for the given weapon; called on the ammo. If this, or <see cref="CanChooseAmmo"/> on the weapon, returns false, then the ammo will not be valid for this weapon.
    /// <para/> By default, returns null and allows <see cref="Item.useAmmo"/> and <see cref="Item.ammo"/> to decide. Return true to make the ammo valid regardless of these fields, and return false to make it invalid.
    /// <para/> If false is returned, the <see cref="CanConsumeAmmo"/>, <see cref="CanBeConsumedAsAmmo"/>, <see cref="OnConsumeAmmo"/>, and <see cref="OnConsumedAsAmmo"/> hooks are never called.
    /// <para/> Called on local and remote clients.
    /// </summary>
    /// <param name="ammo">The ammo that this hook is being called for.</param>
    /// <param name="weapon">The weapon attempting to select the ammo.</param>
    /// <param name="player">The player which the weapon and this potential ammo belong to.</param>
    /// <returns></returns>
    public virtual bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player) => null;

    /// <summary>
    /// Whether or not the given ammo item will be consumed; called on the weapon.
    /// <para/> By default, returns true; return false to prevent ammo consumption.
    /// <para/> If false is returned, the <see cref="OnConsumeAmmo"/> and <see cref="OnConsumedAsAmmo"/> hooks are never called.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="weapon">The weapon that this hook is being called for.</param>
    /// <param name="ammo">The ammo that the weapon is attempting to consume.</param>
    /// <param name="player">The player which this weapon and the ammo belong to.</param>
    /// <returns></returns>
    public virtual bool CanConsumeAmmo(Item weapon, Item ammo, Player player) => true;

    /// <summary>
    /// Whether or not the given ammo item will be consumed; called on the ammo.
    /// <para/> By default, returns true; return false to prevent ammo consumption.
    /// <para/> If false is returned, the <see cref="OnConsumeAmmo"/> and <see cref="OnConsumedAsAmmo"/> hooks are never called.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="ammo">The ammo that this hook is being called for.</param>
    /// <param name="weapon">The weapon attempting to consume the ammo.</param>
    /// <param name="player">The player which the weapon and this ammo belong to.</param>
    /// <returns></returns>
    public virtual bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) => true;

    /// <summary>
    /// Allows you to make things happen when the given ammo is consumed by the given weapon. Called by the weapon.
    /// <para/> Called before the ammo stack is reduced, and is never called if the ammo isn't consumed in the first place.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="weapon">The currently-active weapon.</param>
    /// <param name="ammo">The ammo that the given weapon is currently using.</param>
    /// <param name="player">The player which the given weapon and the given ammo belong to.</param>
    public virtual void OnConsumeAmmo(Item weapon, Item ammo, Player player) { }

    /// <summary>
    /// Allows you to make things happen when the given ammo is consumed by the given weapon. Called by the ammo.
    /// <para/> Called before the ammo stack is reduced, and is never called if the ammo isn't consumed in the first place.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="ammo">The currently-active ammo.</param>
    /// <param name="weapon">The weapon that is currently using the given ammo.</param>
    /// <param name="player">The player which the given weapon and the given ammo belong to.</param>
    public virtual void OnConsumedAsAmmo(Item ammo, Item weapon, Player player) { }

    /// <summary>
    /// Allows you to prevent an item from shooting a projectile on use. Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="item"> The item being used. </param>
    /// <param name="player"> The player using the item. </param>
    /// <returns></returns>
    public virtual bool CanShoot(Item item, Player player) => true;

    /// <summary>
    /// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by an item.
    /// <para/> These parameters will be provided to <see cref="Shoot(Item, Player, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float)"/> where the projectile will actually be spawned.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item"> The item being used. </param>
    /// <param name="player"> The player using the item. </param>
    /// <param name="position"> The center position of the projectile. </param>
    /// <param name="velocity"> The velocity of the projectile. </param>
    /// <param name="type"> The ID of the projectile. </param>
    /// <param name="damage"> The damage of the projectile. </param>
    /// <param name="knockback"> The knockback of the projectile. </param>
    public virtual void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <summary>
    /// Allows you to modify an item's shooting mechanism. Return false to prevent vanilla's shooting code from running. Returns true by default.
    /// <para/> This method is called after the <see cref="ModifyShootStats"/> hook has had a chance to adjust the spawn parameters.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item"> The item being used. </param>
    /// <param name="player"> The player using the item. </param>
    /// <param name="source"> The projectile source's information. </param>
    /// <param name="position"> The center position of the projectile. </param>
    /// <param name="velocity"> The velocity of the projectile. </param>
    /// <param name="type"> The ID of the projectile. </param>
    /// <param name="damage"> The damage of the projectile. </param>
    /// <param name="knockback"> The knockback of the projectile. </param>
    public virtual bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <summary>
    /// Changes the hitbox of a melee weapon when it is used.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) { }

    /// <summary>
    /// Allows you to give melee weapons special effects, such as creating light or dust.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void MeleeEffects(Item item, Player player, Rectangle hitbox) { }

    /// <summary>
    /// Allows you to determine whether the given item can catch the given NPC.
    /// <para/> Return true or false to say the given NPC can or cannot be caught, respectively, regardless of vanilla rules.
    /// <para/> Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.
    /// <para/> If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.
    /// <para/> NOTE: this does not classify the given item as an NPC-catching tool, which is necessary for catching NPCs in the first place. To do that, you will need to use <see cref="ItemID.Sets.CatchingTool"/>.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item with which the player is trying to catch the target NPC.</param>
    /// <param name="target">The NPC the player is trying to catch.</param>
    /// <param name="player">The player attempting to catch the NPC.</param>
    /// <returns></returns>
    public virtual bool? CanCatchNPC(Item item, NPC target, Player player) => null;

    /// <summary>
    /// Allows you to make things happen when the given item attempts to catch the given NPC.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item used to catch the given NPC.</param>
    /// <param name="npc">The NPC which the player attempted to catch.</param>
    /// <param name="player">The player attempting to catch the given NPC.</param>
    /// <param name="failed">Whether or not the given NPC has been successfully caught.</param>
    public virtual void OnCatchNPC(Item item, NPC npc, Player player, bool failed) { }

    /// <summary>
    /// Allows you to dynamically modify the given item's size for the given player, similarly to the effect of the Titan Glove.
    /// <para/> <b>Do not</b> modify <see cref="Item.scale"/>, modify the <paramref name="scale"/> parameter.
    /// <para/> Called on local and remote clients
    /// </summary>
    /// <param name="item">The item to modify the scale of.</param>
    /// <param name="player">The player wielding the given item.</param>
    /// <param name="scale">
    /// The scale multiplier to be applied to the given item.<br></br>
    /// Will be 1.1 if the Titan Glove is equipped, and 1 otherwise.
    /// </param>
    public virtual void ModifyItemScale(Item item, Player player, ref float scale) { }

    /// <summary>
    /// Allows you to determine whether a melee weapon can hit the given NPC when swung. Return true to allow hitting the target, return false to block the weapon from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    public virtual bool? CanHitNPC(Item item, Player player, NPC target) => null;

    /// <summary>
    /// Allows you to determine whether a melee weapon can collide with the given NPC when swung.
    /// <para/> Use <see cref="CanHitNPC(Item, Player, NPC)"/> instead for Flymeal-type effects.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    /// <param name="item">The weapon item the player is holding.</param>
    /// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
    /// <param name="player">The player wielding this item.</param>
    /// <param name="target">The target npc.</param>
    /// <returns>
    /// Return true to allow colliding with target, return false to block the weapon from colliding with target, and return null to use the vanilla code for whether the target can be colliding. Returns null by default.
    /// </returns>
    public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that a melee weapon does to an NPC.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    public virtual void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a melee weapon hits an NPC (for example how the Pumpkin Sword creates pumpkin heads).
    /// <para/> Called on the client hitting the target.
    /// </summary>
    public virtual void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether a melee weapon can hit the given opponent player when swung. Return false to block the weapon from hitting the target. Returns true by default.
    /// <para/> Called on the client hitting the target.
    /// </summary>
    public virtual bool CanHitPvp(Item item, Player player, Player target) => true;

    /// <summary>
    /// Allows you to modify the damage, etc., that a melee weapon does to a player.
    /// <para/> Called on the client taking damage.
    /// </summary>
    public virtual void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a melee weapon hits a player.
    /// <para/> Called on the client taking damage.
    /// </summary>
    public virtual void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo) { }

    /// <summary>
    /// Allows you to make things happen when an item is used. The return value controls whether or not ApplyItemTime will be called for the player.
    /// <para/> Return true if the item actually did something, to force itemTime.
    /// <para/> Return false to keep itemTime at 0.
    /// <para/> Return null for vanilla behavior.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool? UseItem(Item item, Player player) => null;

    /// <summary>
    /// Allows you to make things happen when an item's use animation starts.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UseAnimation(Item item, Player player) { }

    /// <summary>
    /// If the item is consumable and this returns true, then the item will be consumed upon usage. Returns true by default.
    /// If false is returned, the OnConsumeItem hook is never called.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual bool ConsumeItem(Item item, Player player) => true;

    /// <summary>
    /// Allows you to make things happen when this item is consumed.
    /// Called before the item stack is reduced.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void OnConsumeItem(Item item, Player player) { }

    /// <summary>
    /// Allows you to modify the player's animation when an item is being used.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UseItemFrame(Item item, Player player) { }

    /// <summary>
    /// Allows you to modify the player's animation when the player is holding an item.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void HoldItemFrame(Item item, Player player) { }

    /// <inheritdoc cref="ModItem.AltFunctionUse(Player)"/>
    public virtual bool AltFunctionUse(Item item, Player player) => false;

    /// <summary>
    /// Allows you to make things happen when an item is in the player's inventory. This should NOT be used for information accessories;
    /// use <seealso cref="UpdateInfoAccessory"/> for those instead.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateInventory(Item item, Player player) { }

    /// <summary>
    /// Allows you to set information accessory fields with the passed in player argument. This hook should only be used for information
    /// accessory fields such as the Radar, Lifeform Analyzer, and others. Using it for other fields will likely cause weird side-effects.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateInfoAccessory(Item item, Player player) { }

    /// <summary>
    /// Allows you to give effects to armors and accessories, such as increased damage.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateEquip(Item item, Player player) { }

    /// <summary>
    /// Allows you to give effects to accessories. The hideVisual parameter is whether the player has marked the accessory slot to be hidden from being drawn on the player.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateAccessory(Item item, Player player, bool hideVisual) { }

    /// <summary>
    /// Allows you to give effects to this accessory when equipped in a vanity slot. Vanilla uses this for boot effects, wings and merman/werewolf visual flags
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateVanity(Item item, Player player) { }

    /// <inheritdoc cref="ModItem.UpdateVisibleAccessory(Player, bool)"/>
    public virtual void UpdateVisibleAccessory(Item item, Player player, bool hideVisual) { }

    /// <inheritdoc cref="ModItem.UpdateItemDye(Player, int, bool)"/>
    public virtual void UpdateItemDye(Item item, Player player, int dye, bool hideVisual) { }

    /// <summary>
    /// Allows you to determine whether the player is wearing an armor set, and return a name for this set. If there is no armor set, return the empty string. Returns the empty string by default.
    /// <para/> This method is not instanced.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual string IsArmorSet(Item head, Item body, Item legs) => "";

    /// <summary>
    /// Allows you to give set bonuses to your armor set with the given name.
    /// The set name will be the same as returned by IsArmorSet.
    /// <para/> This method is not instanced.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateArmorSet(Player player, string set) { }

    /// <summary>
    /// Returns whether or not the head armor, body armor, and leg armor textures make up a set.
    /// This hook is used for the PreUpdateVanitySet, UpdateVanitySet, and ArmorSetShadows hooks, and will use items in the social slots if they exist.
    /// By default this will return the same value as the IsArmorSet hook, so you will not have to use this hook unless you want vanity effects to be entirely separate from armor sets.
    /// <para/> This method is not instanced.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual string IsVanitySet(int head, int body, int legs)
    {
        int headItemType = 0;
        if (head >= 0)
            headItemType = Item.headType[head];

        Item headItem = ContentSamples.ItemsByType[headItemType];

        int bodyItemType = 0;
        if (body >= 0)
            bodyItemType = Item.bodyType[body];

        Item bodyItem = ContentSamples.ItemsByType[bodyItemType];

        int legsItemType = 0;
        if (legs >= 0)
            legsItemType = Item.legType[legs];

        Item legItem = ContentSamples.ItemsByType[legsItemType];

        return IsArmorSet(headItem, bodyItem, legItem);
    }

    /// <summary>
    /// Allows you to create special effects (such as the necro armor's hurt noise) when the player wears the vanity set with the given name returned by IsVanitySet.
    /// This hook is called regardless of whether the player is frozen in any way.
    /// <para/> This method is not instanced.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void PreUpdateVanitySet(Player player, string set) { }

    /// <summary>
    /// Allows you to create special effects (such as dust) when the player wears the vanity set with the given name returned by IsVanitySet. This hook will only be called if the player is not frozen in any way.
    /// <para/> This method is not instanced.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void UpdateVanitySet(Player player, string set) { }

    /// <summary>
    /// Allows you to determine special visual effects a vanity has on the player without having to code them yourself.
    /// <para/> This method is not instanced.
    /// <para/> Called on local, server, and remote clients.
    /// <example><code>player.armorEffectDrawShadow = true;</code></example>
    /// </summary>
    public virtual void ArmorSetShadows(Player player, string set) { }

    /// <summary>
    /// Allows you to modify the equipment that the player appears to be wearing.
    /// <para/> Note that type and equipSlot are not the same as the item type of the armor the player will appear to be wearing. Worn equipment has a separate set of IDs.
    /// <para/> You can find the vanilla equipment IDs by looking at the headSlot, bodySlot, and legSlot fields for items, and modded equipment IDs by looking at EquipLoader.
    /// <para/> This method is not instanced.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="armorSlot">head armor (0), body armor (1) or leg armor (2).</param>
    /// <param name="type">The equipment texture ID of the item that the player is wearing.</param>
    /// <param name="male">True if the player is male.</param>
    /// <param name="equipSlot">The altered equipment texture ID for the legs (armorSlot 1 and 2) or head (armorSlot 0)</param>
    /// <param name="robes">Set to true if you modify equipSlot when armorSlot == 1 to set Player.wearsRobe, otherwise ignore it</param>
    public virtual void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) { }

    /// <summary>
    /// Returns whether or not an item does something when right-clicked in the inventory. Returns false by default.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual bool CanRightClick(Item item) => false;

    /// <summary>
    /// Allows you to make things happen when this item is right-clicked in the inventory. By default this will consume the item by 1 stack, so return false in <see cref="ConsumeItem(Player)"/> if that behavior is undesired.
    /// <para/> This is only called if the item can be right-clicked, meaning <see cref="ItemID.Sets.OpenableBag"/> is true for the item type or <see cref="GlobalItem.CanRightClick"/> returns true.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void RightClick(Item item, Player player) { }

    /// <summary>
    /// Allows you to add and modify the loot items that spawn from bag items when opened.
    /// <para/> The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4">Basic NPC Drops and Loot 1.4 Guide</see> explains how to use the <see cref="ModNPC.ModifyNPCLoot(NPCLoot)"/> hook to modify NPC loot as well as this hook. A common usage is to use this hook and <see cref="ModNPC.ModifyNPCLoot(NPCLoot)"/> to edit non-expert exclusive drops for bosses.
    /// <para/> This hook only runs once per item type during mod loading, any dynamic behavior must be contained in the rules themselves.
    /// <para/> This hook is not instanced.
    /// </summary>
    /// <param name="item">A default item of the type being opened, not the actual item instance</param>
    /// <param name="itemLoot">A reference to the item drop database for this item type</param>
    public virtual void ModifyItemLoot(Item item, ItemLoot itemLoot) { }

    /// <summary>
    /// Allows you to prevent items from stacking.
    /// <para/>This is only called when two items of the same type attempt to stack.
    /// <para/>This is usually not called for coins and ammo in the inventory/UI.
    /// <para/>This covers all scenarios, if you just need to change in-world stacking behavior, use <see cref="CanStackInWorld"/>.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
    /// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
    /// <returns>Whether or not the items are allowed to stack</returns>
    public virtual bool CanStack(Item destination, Item source) => true;

    /// <summary>
    /// Allows you to prevent items from stacking in the world.
    /// <para/> This is only called when two items of the same type attempt to stack.
    /// <para/> Called on the local client or server, depending on who the item is reserved for.
    /// </summary>
    /// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
    /// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
    /// <returns>Whether or not the items are allowed to stack</returns>
    public virtual bool CanStackInWorld(Item destination, Item source) => true;

    /// <summary>
    /// Allows you to make things happen when items stack together.
    /// <para/> This hook is called before the items are transferred from <paramref name="source"/> to <paramref name="destination"/>
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
    /// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
    /// <param name="numToTransfer">The quantity of <paramref name="source"/> that will be transferred to <paramref name="destination"/></param>
    public virtual void OnStack(Item destination, Item source, int numToTransfer) { }

    /// <summary>
    /// Allows you to make things happen when an item stack is split. This hook is called before the stack values are modified.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="destination">
    /// The item instance that <paramref name="source"/> will transfer items to, and is usually a clone of <paramref name="source"/>.<br/>
    /// This parameter's stack will always be zero.
    /// </param>
    /// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
    /// <param name="numToTransfer">The quantity of <paramref name="source"/> that will be transferred to to <paramref name="destination"/></param>
    public virtual void SplitStack(Item destination, Item source, int numToTransfer) { }

    /// <summary>
    /// Returns if the normal reforge pricing is applied.
    /// If true or false is returned and the price is altered, the price will equal the altered price.
    /// The passed reforge price equals the item.value. Vanilla pricing will apply 20% discount if applicable and then price the reforge at a third of that value.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) => true;

    /// <summary>
    /// This hook gets called when the player clicks on the reforge button and can afford the reforge.
    /// Returns whether the reforge will take place. If false is returned by the ModItem or any GlobalItem, the item will not be reforged, the cost to reforge will not be paid, and PreReforge and PostReforge hooks will not be called.
    /// Reforging preserves modded data on the item.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual bool CanReforge(Item item) => true;

    /// <summary>
    /// This hook gets called immediately before an item gets reforged by the Goblin Tinkerer.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void PreReforge(Item item) { }

    /// <summary>
    /// This hook gets called immediately after an item gets reforged by the Goblin Tinkerer.
    /// Useful for modifying modded data based on the reforge result.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void PostReforge(Item item) { }

    /// <summary>
    /// Allows you to modify the colors in which the player's armor and their surrounding accessories are drawn, in addition to which glow mask and in what color is drawn.
    /// <para/> This method is not instanced.
    /// <para/> Called on local and remote clients.
    /// </summary>
    public virtual void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }

    /// <summary>
    /// Allows you to modify which glow mask and in what color is drawn on the player's arms. Note that this is only called for body armor.
    /// <para/> This method is not instanced.
    /// <para/> Called on local and remote clients.
    /// </summary>
    public virtual void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) { }

    /// <summary>
    /// Allows you to modify the speeds at which you rise and fall when wings are equipped.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) { }

    /// <summary>
    /// Allows you to modify the horizontal flight speed and acceleration of wings.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration) { }

    /// <summary>
    /// Allows for Wings to do various things while in use. "inUse" is whether or not the jump button is currently pressed.
    /// Called when wings visually appear on the player.
    /// Use to animate wings, create dusts, invoke sounds, and create lights. False will keep everything the same.
    /// True, you need to handle all animations in your own code.
    /// <para/> This method is not instanced.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool WingUpdate(int wings, Player player, bool inUse) => false;

    /// <summary>
    /// Allows you to customize an item's movement when lying in the world. Note that this will not be called if the item is currently being grabbed by a player.
    /// <para/> Called on all clients and the server.
    /// </summary>
    public virtual void Update(Item item, ref float gravity, ref float maxFallSpeed) { }

    /// <summary>
    /// Allows you to make things happen when an item is lying in the world. This will always be called, even when the item is being grabbed by a player. This hook should be used for adding light, or for increasing the age of less valuable items.
    /// <para/> Called on all clients and the server.
    /// </summary>
    public virtual void PostUpdate(Item item) { }

    /// <summary>
    /// Allows you to modify how close an item must be to the player in order to move towards the player.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void GrabRange(Item item, Player player, ref int grabRange) { }

    /// <summary>
    /// Allows you to modify the way an item moves towards the player. Return false to allow the vanilla grab style to take place. Returns false by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool GrabStyle(Item item, Player player) => false;

    /// <summary>
    /// Allows you to determine whether or not the item can be picked up
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool CanPickup(Item item, Player player) => true;

    /// <summary>
    /// Allows you to make special things happen when the player picks up an item. Return false to stop the item from being added to the player's inventory; returns true by default.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual bool OnPickup(Item item, Player player) => true;

    /// <summary>
    /// Return true to specify that the item can be picked up despite not having enough room in inventory. Useful for something like hearts or experience items. Use in conjunction with OnPickup to actually consume the item and handle it.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool ItemSpace(Item item, Player player) => false;

    /// <summary>
    /// Allows you to determine the color and transparency in which an item is drawn. Return null to use the default color (normally light color). Returns null by default.
    /// <para/> Called on all clients.
    /// </summary>
    public virtual Color? GetAlpha(Item item, Color lightColor) => null;

    /// <summary>
    /// <inheritdoc cref="ModItem.PreDrawInWorld(SpriteBatch, Color, Color, ref float, ref float, int)"/>
    /// </summary>
    public virtual bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => true;

    /// <summary>
    /// <inheritdoc cref="ModItem.PostDrawInWorld(SpriteBatch, Color, Color, float, float, int)"/>
    /// </summary>
    public virtual void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) { }

    /// <summary>
    /// <inheritdoc cref="ModItem.PreDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/>
    /// </summary>
    public virtual bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

    /// <summary>
    /// <inheritdoc cref="ModItem.PostDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/>
    /// </summary>
    public virtual void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { }

    /// <summary>
    /// Allows you to determine the offset of an item's sprite when used by the player.
    /// This is only used for items with a useStyle of 5 that aren't staves.
    /// Return null to use the item's default holdout offset; returns null by default.
    /// <para/> This method is not instanced.
    /// <para/> Called on local and remote clients.
    /// <example><code>return new Vector2(10, 0);</code></example>
    /// </summary>
    public virtual Vector2? HoldoutOffset(int type) => null;

    /// <summary>
    /// Allows you to determine the point on an item's sprite that the player holds onto when using the item.
    /// The origin is from the bottom left corner of the sprite. This is only used for staves with a useStyle of 5.
    /// Return null to use the item's default holdout origin; returns null by default.
    /// <para/> This method is not instanced.
    /// <para/> Called on local and remote clients.
    /// </summary>
    public virtual Vector2? HoldoutOrigin(int type) => null;

    /// <summary>
    /// Allows you to disallow the player from equipping an accessory. Return false to disallow equipping the accessory. Returns true by default.
    /// </summary>
    /// <param name="item">The item that is attempting to equip.</param>
    /// <param name="player">The player.</param>
    /// <param name="slot">The inventory slot that the item is attempting to occupy.</param>
    /// <param name="modded">If the inventory slot index is for modded slots.</param>
    public virtual bool CanEquipAccessory(Item item, Player player, int slot, bool modded) => true;

    /// <summary>
    /// Allows you to prevent similar accessories from being equipped multiple times. For example, vanilla Wings.
    /// Return false to have the currently equipped item swapped with the incoming item - ie both can't be equipped at same time.
    /// </summary>
    public virtual bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) => true;

    /// <summary>
    /// Allows you to modify what item, and in what quantity, is obtained when an item of the given type is fed into the Extractinator. Use <see cref="ItemID.Sets.ExtractinatorMode"/> to allow an item to be fed into the Extractinator.
    /// <para/> An extractType of 0 represents the default extraction (Silt and Slush). 0, <see cref="ItemID.DesertFossil"/>, <see cref="ItemID.OldShoe"/>, and <see cref="ItemID.LavaMoss"/> are vanilla extraction types. Modded types by convention will correspond to the iconic item of the extraction type. The <see href="https://terraria.wiki.gg/wiki/Extractinator">Extractinator wiki page</see> has more info.
    /// <para/> By default the parameters will be set to the output of feeding Silt/Slush into the Extractinator.
    /// <para/> Use <paramref name="extractinatorBlockType"/> to provide different behavior for <see cref="TileID.ChlorophyteExtractinator"/> if desired.
    /// <para/> If the Chlorophyte Extractinator item swapping behavior is desired, see the example in <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/GlobalItems/TorchExtractinatorGlobalItem.cs">TorchExtractinatorGlobalItem.cs</see>.
    /// <para/> This method is not instanced.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="extractType">The extractinator type corresponding to the items being processed</param>
    /// <param name="extractinatorBlockType">Which Extractinator tile is being used, <see cref="TileID.Extractinator"/> or <see cref="TileID.ChlorophyteExtractinator"/>.</param>
    /// <param name="resultType">Type of the result.</param>
    /// <param name="resultStack">The result stack.</param>
    public virtual void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack) { }

    /// <summary>
    /// Allows you to modify how many of an item a player obtains when the player fishes that item.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void CaughtFishStack(int type, ref int stack) { }

    /// <summary>
    /// Whether or not specific conditions have been satisfied for the Angler to be able to request the given item. (For example, Hardmode.)
    /// Returns true by default.
    /// <para/> This method is not instanced.
    /// <para/> Called on the server only.
    /// </summary>
    public virtual bool IsAnglerQuestAvailable(int type) => true;

    /// <summary>
    /// Allows you to set what the Angler says when the Quest button is clicked in his chat.
    /// The chat parameter is his dialogue, and catchLocation should be set to "Caught at [location]" for the given type.
    /// <para/> This method is not instanced.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void AnglerChat(int type, ref string chat, ref string catchLocation) { }

    /// <summary>
    /// Override this method to add <see cref="Recipe"/>s to the game.<br/>
    /// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Recipes">Basic Recipes Guide</see> teaches how to add new recipes to the game and how to manipulate existing recipes.<br/>
    /// </summary>
    public virtual void AddRecipes() { }

    /// <summary>
    /// Allows you to do things before this item's tooltip is drawn.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item</param>
    /// <param name="lines">The tooltip lines for this item</param>
    /// <param name="x">The top X position for this tooltip. It is where the first line starts drawing</param>
    /// <param name="y">The top Y position for this tooltip. It is where the first line starts drawing</param>
    /// <returns>Whether or not to draw this tooltip</returns>
    public virtual bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => true;

    /// <summary>
    /// Allows you to do things after this item's tooltip is drawn. The lines contain draw information as this is ran after drawing the tooltip.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item</param>
    /// <param name="lines">The tooltip lines for this item</param>
    public virtual void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines) { }

    /// <summary>
    /// Allows you to do things before a tooltip line of this item is drawn. The line contains draw info.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item</param>
    /// <param name="line">The line that would be drawn</param>
    /// <param name="yOffset">The Y offset added for next tooltip lines</param>
    /// <returns>Whether or not to draw this tooltip line</returns>
    public virtual bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) => true;

    /// <summary>
    /// Allows you to do things after a tooltip line of this item is drawn. The line contains draw info.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item</param>
    /// <param name="line">The line that was drawn</param>
    public virtual void PostDrawTooltipLine(Item item, DrawableTooltipLine line) { }

    /// <summary>
    /// Allows you to modify all the tooltips that display for the given item. See here for information about TooltipLine. To hide tooltips, please use <see cref="TooltipLine.Hide"/> and defensive coding.
    /// <para/> Called on a clone of the item, not the original. Modifying instanced fields will have no effect.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void ModifyTooltips(Item item, List<TooltipLine> tooltips) { }

    /// <summary>
    /// Allows you to save custom data for this item.
    /// <br/>
    /// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
    /// <br/><b>NOTE:</b> Try to only save data that isn't default values.
    /// </summary>
    /// <param name="item"> The item. </param>
    /// <param name="tag"> The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument only for the sake of convenience and optimization. </param>
    public virtual void SaveData(Item item, TagCompound tag) { }

    /// <summary>
    /// Allows you to load custom data that you have saved for this item.
    /// <br/><b>Try to write defensive loading code that won't crash if something's missing.</b>
    /// </summary>
    /// <param name="item"> The item. </param>
    /// <param name="tag"> The TagCompound to load data from. </param>
    public virtual void LoadData(Item item, TagCompound tag) { }

    /// <inheritdoc cref="ModItem.NetSend"/>
    public virtual void NetSend(Item item, BinaryWriter writer) { }

    /// <inheritdoc cref="ModItem.NetReceive"/>
    public virtual void NetReceive(Item item, BinaryReader reader) { }
}
#endregion General Behavior

#region Transoceanic General Behavior
public abstract class TOPlayerBehavior : PlayerBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}

public abstract class TOGlobalNPCBehavior : GlobalNPCBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}

public abstract class TOGlobalProjectileBehavior : GlobalProjectileBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}

public abstract class TOGlobalItemBehavior : GlobalItemBehavior
{
    public sealed override TOMain Mod => TOMain.Instance;
}
#endregion Transoceanic General Behavior

#region Single Behavior
public abstract class SingleNPCBehavior : SingleEntityBehavior<NPC>
{
    public NPC NPC { get; private set; } = null;

    public TOGlobalNPC OceanNPC { get; private set; } = null;

    public Player Target => Main.player[NPC.target];

    public override void Connect(NPC npc)
    {
        NPC = npc;
        OceanNPC = npc.Ocean();
    }

    public int Timer1
    {
        get => OceanNPC.Timer1;
        set => OceanNPC.Timer1 = value;
    }

    public int Timer2
    {
        get => OceanNPC.Timer2;
        set => OceanNPC.Timer2 = value;
    }

    public int Timer3
    {
        get => OceanNPC.Timer3;
        set => OceanNPC.Timer3 = value;
    }

    public float Timer4
    {
        get => OceanNPC.Timer4;
        set => OceanNPC.Timer4 = value;
    }

    public float Timer5
    {
        get => OceanNPC.Timer5;
        set => OceanNPC.Timer5 = value;
    }

    #region 虚成员
    #region Defaults
    /// <summary>
    /// 设置基本属性。
    /// </summary>
    public virtual void SetDefaults() { }

    /// <summary>
    /// 设置负数type的NPC的额外属性。
    /// </summary>
    public virtual void SetDefaultsFromNetId() { }

    /// <summary>
    /// Allows you to customize this NPC's stats when the difficulty is expert or higher.<br/>
    /// This runs after <see cref="NPC.value"/>,  <see cref="NPC.lifeMax"/>,  <see cref="NPC.damage"/>,  <see cref="NPC.knockBackResist"/> have been adjusted for the current difficulty, (expert/master/FTW)<br/>
    /// It is common to multiply lifeMax by the balance factor, and sometimes adjust knockbackResist.<br/>
    /// <br/>
    /// Eg:<br/>
    /// <code>lifeMax = (int)(lifeMax * balance * bossAdjustment)</code>
    /// </summary>
    /// <param name="numPlayers">The number of active players</param>
    /// <param name="balance">Scaling factor that increases by a fraction for each player</param>
    /// <param name="bossAdjustment">An extra reduction factor to be applied to boss life in high difficulty modes</param>
    public virtual void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) { }

    /// <summary>
    /// Allows you to set an NPC's information in the Bestiary.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="bestiaryEntry"></param>
    public virtual void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }

    /// <summary>
    /// Allows you to modify the type name of this NPC dynamically.
    /// </summary>
    public virtual void ModifyTypeName(ref string typeName) { }

    /// <summary>
    /// Allows you to modify the bounding box for hovering over the given NPC (affects things like whether or not its name is displayed).
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="boundingBox">The bounding box used for determining whether or not the NPC counts as being hovered over.</param>
    public virtual void ModifyHoverBoundingBox(ref Rectangle boundingBox) { }

    /// <summary>
    /// Allows you to set the town NPC profile that a given NPC uses.
    /// </summary>
    /// <returns>The profile that you want the given NPC to use.<br></br>
    /// This will only influence their choice of profile if you do not return null.<br></br>
    /// By default, returns null, which causes no change.</returns>
    public virtual ITownNPCProfile ModifyTownNPCProfile() => null;

    /// <summary>
    /// Allows you to modify the list of names available to the given town NPC.
    /// </summary>
    public virtual void ModifyNPCNameList(List<string> nameList) { }

    /// <summary>
    /// This is where you reset any fields you add to your subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied.
    /// <para/> Called on the server and clients.
    /// </summary>
    public virtual void ResetEffects() { }
    #endregion Defaults

    #region Lifetime
    /// <summary>
    /// 生成时执行的代码。
    /// </summary>
    /// <param name="source"></param>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <summary>
    /// 是否执行检查NPC是否保持 <see cref="Entity.active"> 的代码。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止NPC消失，并且阻止NPC计入玩家附近可以存在的NPC数量限制。默认返回 <see langword="true"/>。</returns>
    public virtual bool CheckActive() => true;

    /// <summary>
    /// 当NPC到达0生命值时是否应该被杀死。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止NPC被杀死。默认返回 <see langword="true"/>。</returns>
    public virtual bool CheckDead() => true;

    /// <summary>
    /// 在这个方法中，可自主调用 <c>OnKill()</c>。
    /// </summary>
    /// <returns>返回 <see langword="true"/> 以阻止自动调用 <c>OnKill()</c>。默认返回 <see langword="false"/>。</returns>
    public virtual bool SpecialOnKill() => false;

    /// <summary>
    /// 决定NPC死亡时是否会执行一些操作（除了死亡本身）。
    /// <br/>此方法也可用于动态阻止特定物品掉落，使用 <see cref="NPCLoader.blockLoot"/>，但通常编辑掉落规则是更好的方法。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 将跳过战利品掉落、<see cref="NPCLoader.OnKill(NPC)"/> 方法和Boss击败标志设置。</returns>
    public virtual bool PreKill() => true;

    /// <summary>
    /// 当NPC死亡时执行的代码。
    /// <br/>这个钩子仅在单人模式或服务器上运行。对于客户端效果（如灰尘、血迹和声音），使用 <see cref="HitEffect(NPC.HitInfo)"/>。
    /// </summary>
    public virtual void OnKill() { }

    /// <summary>
    /// Allows you to add and modify NPC loot tables to drop on death and to appear in the Bestiary.<br/>
    /// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4">Basic NPC Drops and Loot 1.4 Guide</see> explains how to use this hook to modify npc loot.
    /// <br/> This hook only runs once per npc type during mod loading, any dynamic behavior must be contained in the rules themselves.
    /// </summary>
    /// <param name="npcLoot">A reference to the item drop database for this npc type.</param>
    public virtual void ModifyNPCLoot(NPCLoot npcLoot) { }


    #endregion Lifetime

    #region AI
    /// <summary>
    /// 在 <see cref="NPC.AI"/> 方法之前调用。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止 <see cref="NPC.AI"/> 方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreAI() => true;

    /// <summary>
    /// AI。
    /// </summary>
    public virtual void AI() { }

    /// <summary>
    /// AI。
    /// </summary>
    public virtual void PostAI() { }
    #endregion AI

    #region Draw
    /// <summary>
    /// 设置NPC当前帧。
    /// </summary>
    /// <param name="frameHeight"></param>
    public virtual void FindFrame(int frameHeight) { }

    /// <summary>
    /// 获取绘制颜色。
    /// </summary>
    /// <param name="drawColor">原始绘制颜色。</param>
    /// <returns>返回 <see langword="null"/> 以使用默认颜色。</returns>
    public virtual Color? GetAlpha(Color drawColor) => null;

    /// <summary>
    /// Allows you to add special visual effects to an NPC (such as creating dust), and modify the color in which the NPC is drawn.
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="drawColor"></param>
    public virtual void DrawEffects(ref Color drawColor) { }

    /// <summary>
    /// 在绘制NPC之前调用。
    /// </summary>
    /// <param name="screenPos">在绘制之前，将绘制位置减去之。</param>
    /// <param name="drawColor">绘制颜色。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制NPC方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

    /// <summary>
    /// 在绘制NPC之后调用。
    /// </summary>
    /// <param name="screenPos">在绘制之前，将绘制位置减去之。</param>
    /// <param name="drawColor">绘制颜色。</param>
    public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <summary>
    /// 当 <see cref="NPC.hide"/> 为 <see langword="true"/> 的情况下，允许您指定此NPC应绘制在某些元素之后。
    /// <br/>将索引添加到 <see cref="Main.DrawCacheNPCsMoonMoon"/>, <see cref="Main.DrawCacheNPCsOverPlayers"/>, <see cref="Main.DrawCacheNPCProjectiles"/> 或 <see cref="Main.DrawCacheNPCsBehindNonSolidTiles"/> 中的一个。
    /// </summary>
    /// <param name="index"></param>
    public virtual void DrawBehind(int index) { }

    /// <summary>
    /// 根据NPC的状态，允许您自定义NPC在地图上显示的Boss图标。
    /// <br/>将索引设置为-1以停止绘制。
    /// </summary>
    /// <param name="index">在 <see cref="NPCID.Sets.BossHeadTextures"/> 中的索引。</param>
    public virtual void BossHeadSlot(ref int index) { }

    /// <summary>
    /// 自定义NPC的Boss头图标在地图上的旋转角度。
    /// </summary>
    public virtual void BossHeadRotation(ref float rotation) { }

    /// <summary>
    /// 允许你在地图上翻转NPC的Boss头图标。
    /// </summary>
    public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects) { }

    /// <summary>
    /// Allows you to control how the health bar for the given NPC is drawn. The hbPosition parameter is the same as Main.hbPosition; it determines whether the health bar gets drawn above or below the NPC by default. The scale parameter is the health bar's size. By default, it will be the normal 1f; most bosses set this to 1.5f. Return null to let the normal vanilla health-bar-drawing code to run. Return false to stop the health bar from being drawn. Return true to draw the health bar in the position specified by the position parameter (note that this is the world position, not screen position).
    /// <para/> Called on all clients.
    /// </summary>
    /// <param name="hbPosition"></param>
    /// <param name="scale"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public virtual bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => null;
    #endregion Draw

    #region Hit
    /// <summary>
    /// Allows you to make things happen whenever an NPC is hit, such as creating dust or gores. <br/>
    /// Called on local, server and remote clients. <br/>
    /// Usually when something happens when an npc dies such as item spawning, you use NPCLoot, but you can use HitEffect paired with a check for <c>if (npc.life &lt;= 0)</c> to do client-side death effects, such as spawning dust, gore, or death sounds. <br/>
    /// </summary>
    public virtual void HitEffect(NPC.HitInfo hit) { }

    /// <summary>
    /// Allows you to determine whether an NPC can be hit by the given melee weapon when swung. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual bool? CanBeHitByItem(Player player, Item item) => null;

    /// <summary>
    /// Allows you to determine whether an NPC can be collided with the player melee weapon when swung. <br/>
    /// Use <see cref="CanBeHitByItem(Player, Item)"/> instead for Guide Voodoo Doll-type effects.
    /// </summary>
    /// <param name="player">The player wielding this item.</param>
    /// <param name="item">The weapon item the player is holding.</param>
    /// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
    /// <returns>
    /// Return true to allow colliding with the melee attack, return false to block the weapon from colliding with the NPC, and return null to use the vanilla code for whether the attack can be colliding. Returns null by default.
    /// </returns>
    public virtual bool? CanCollideWithPlayerMeleeAttack(Player player, Item item, Rectangle meleeAttackHitbox) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that an NPC takes from a melee weapon. 
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// <para/> Runs on the local client.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC is hit by a melee weapon.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether an NPC can be hit by the given projectile. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
    /// </summary>
    /// <param name="projectile"></param>
    /// <returns></returns>
    public virtual bool? CanBeHitByProjectile(Projectile projectile) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that an NPC takes from a projectile.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC is hit by a projectile.
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether a friendly NPC can be hit by an NPC. Return false to block the attacker from hitting the NPC, and return true to use the vanilla code for whether the target can be hit. Returns true by default.
    /// </summary>
    /// <param name="attacker"></param>
    /// <returns></returns>
    public virtual bool CanBeHitByNPC(NPC attacker) => true;

    /// <summary>
    /// Allows you to use a custom damage formula for when an NPC takes damage from any source. For example, you can change the way defense works or use a different crit multiplier.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// </summary>
    /// <param name="modifiers"></param>
    public virtual void ModifyIncomingHit(ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to determine whether an NPC can hit the given player. Return false to block the NPC from hitting the target. Returns true by default. CooldownSlot determines which of the player's cooldown counters (<see cref="ImmunityCooldownID"/>) to use, and defaults to -1 (<see cref="ImmunityCooldownID.General"/>).
    /// </summary>
    /// <param name="target"></param>
    /// <param name="cooldownSlot"></param>
    /// <returns></returns>
    public virtual bool CanHitPlayer(Player target, ref int cooldownSlot) => true;

    /// <summary>
    /// Allows you to modify the damage, etc., that an NPC does to a player.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC hits a player (for example, inflicting debuffs). <br/>
    /// Only runs on the local client in multiplayer.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="hurtInfo"></param>
    public virtual void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) { }

    /// <summary>
    /// Allows you to determine whether an NPC can hit the given friendly NPC. Return false to block the NPC from hitting the target, and return true to use the vanilla code for whether the target can be hit. Returns true by default.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool CanHitNPC(NPC target) => true;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that an NPC does to a friendly NPC.
    /// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when an NPC hits a friendly NPC.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="hit"></param>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit) { }

    /// <summary>
    /// Allows you to modify the NPC's <seealso cref="ID.ImmunityCooldownID"/>, damage multiplier, and hitbox. Useful for implementing dynamic damage hitboxes that change in dimensions or deal extra damage. Returns false to prevent vanilla code from running. Returns true by default.
    /// </summary>
    /// <param name="victimHitbox"></param>
    /// <param name="immunityCooldownSlot"></param>
    /// <param name="damageMultiplier"></param>
    /// <param name="npcHitbox"></param>
    /// <returns></returns>
    public virtual bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => true;
    #endregion Hit

    #region SpecialEffects
    /// <summary>
    /// Allows you to make the NPC either regenerate health or take damage over time by setting <see cref="NPC.lifeRegen"/>. This is useful for implementing damage over time debuffs such as <see cref="BuffID.Poisoned"/> or <see cref="BuffID.OnFire"/>. Regeneration or damage will occur at a rate of half of <see cref="NPC.lifeRegen"/> per second.
    /// <para/> Essentially, modders implementing damage over time debuffs should subtract from <see cref="NPC.lifeRegen"/> a number that is twice as large as the intended damage per second. See <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/GlobalNPCs/DamageOverTimeGlobalNPC.cs#L16">DamageOverTimeGlobalNPC.cs</see> for an example of this.
    /// <para/> The damage parameter is the number that appears above the NPC's head if it takes damage over time.
    /// <para/> Multiple debuffs work together by following some conventions: <see cref="NPC.lifeRegen"/> should not be assigned a number, rather it should be subtracted from. <paramref name="damage"/> should only be assigned if the intended popup text is larger then its current value.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="damage"></param>
    public virtual void UpdateLifeRegen(ref int damage) { }

    /// <summary>
    /// Allows you to determine how and when an NPC can fall through platforms and similar tiles.
    /// <para/> Return true to allow an NPC to fall through platforms, false to prevent it. Returns null by default, applying vanilla behaviors (based on aiStyle and type).
    /// <para/> Called on the server and clients.
    /// </summary>
    public virtual bool? CanFallThroughPlatforms() => null;

    /// <summary>
    /// Allows you to determine whether the given item can catch the given NPC.<br></br>
    /// Return true or false to say the given NPC can or cannot be caught, respectively, regardless of vanilla rules.
    /// <para/> Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.
    /// <para/> If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.
    /// <para/> NOTE: this does not classify the given item as an NPC-catching tool, which is necessary for catching NPCs in the first place. To do that, you will need to use the "CatchingTool" set in ItemID.Sets.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="item">The item with which the player is trying to catch the given NPC.</param>
    /// <param name="player">The player attempting to catch the given NPC.</param>
    /// <returns></returns>
    public virtual bool? CanBeCaughtBy(Item item, Player player) => null;

    /// <summary>
    /// Allows you to make things happen when the given item attempts to catch the given NPC.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="player">The player attempting to catch the given NPC.</param>
    /// <param name="item">The item used to catch the given NPC.</param>
    /// <param name="failed">Whether or not the given NPC has been successfully caught.</param>
    public virtual void OnCaughtBy(Player player, Item item, bool failed) { }

    /// <summary>
    /// Allows you to determine whether this NPC can talk with the player. Return true to allow talking with the player, return false to block this NPC from talking with the player, and return null to use the vanilla code for whether the NPC can talk. Returns null by default.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <returns></returns>
    public virtual bool? CanChat() => null;

    /// <summary>
    /// Allows you to modify the chat message of any NPC that the player can talk to.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="chat"></param>
    public virtual void GetChat(ref string chat) { }

    /// <summary>
    /// Allows you to determine if something can happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked. Return false to prevent the normal code for this button from running. Returns true by default.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="firstButton"></param>
    /// <returns></returns>
    public virtual bool PreChatButtonClicked(bool firstButton) => true;

    /// <summary>
    /// Allows you to make something happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="firstButton"></param>
    public virtual void OnChatButtonClicked(bool firstButton) { }

    /// <summary>
    /// Allows you to modify the contents of a shop whenever player opens it. <br/>
    /// If possible, use <see cref="ModifyShop(NPCShop)"/> instead, to reduce mod conflicts and improve compatibility.
    /// Note that for special shops like travelling merchant, the <paramref name="shopName"/> may not correspond to a <see cref="NPCShop"/> in the <see cref="NPCShopDatabase"/>
    /// <para/> Also note that unused slots in <paramref name="items"/> are null while <see cref="Item.IsAir"/> entries are entries that have a reserved slot (<see cref="NPCShop.Entry.SlotReserved"/>) but did not have their conditions met. These should not be overwritten.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="shopName">The full name of the shop being opened. See <see cref="NPCShopDatabase.GetShopName"/> for the format. </param>
    /// <param name="items">Items in the shop including 'air' items in empty slots.</param>
    public virtual void ModifyActiveShop(string shopName, Item[] items) { }

    /// <summary>
    /// Whether this NPC can be teleported to a King or Queen statue. Return true to allow the NPC to teleport to the statue, return false to block this NPC from teleporting to the statue, and return null to use the vanilla code for whether the NPC can teleport to the statue. Returns null by default.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="toKingStatue">Whether the NPC is being teleported to a King or Queen statue.</param>
    public virtual bool? CanGoToStatue(bool toKingStatue) => null;

    /// <summary>
    /// Allows you to make things happen when this NPC teleports to a King or Queen statue.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="toKingStatue">Whether the NPC was teleported to a King or Queen statue.</param>
    public virtual void OnGoToStatue(bool toKingStatue) { }

    /// <summary>
    /// Allows you to modify the death message of a town NPC or boss. This also affects what the dropped tombstone will say in the case of a town NPC. The text color can also be modified.
    /// <para/> When modifying the death message, use <see cref="NPC.GetFullNetName"/> to retrieve the NPC name to use in substitutions.
    /// <para/> Return false to skip the vanilla code for sending the message. This is useful if the death message is handled by this method or if the message should be skipped for any other reason, such as if there are multiple bosses. Returns true by default.
    /// </summary>
    public virtual bool ModifyDeathMessage(ref NetworkText customText, ref Color color) => true;

    /// <summary>
    /// Allows you to determine the damage and knockback of a town NPC's attack before the damage is scaled. (More information on scaling in GlobalNPC.BuffTownNPCs.)
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="knockback"></param>
    public virtual void TownNPCAttackStrength(ref int damage, ref float knockback) { }

    /// <summary>
    /// Allows you to determine the cooldown between each of a town NPC's attack. The cooldown will be a number greater than or equal to the first parameter, and less then the sum of the two parameters.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="cooldown"></param>
    /// <param name="randExtraCooldown"></param>
    public virtual void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) { }

    /// <summary>
    /// Allows you to determine the projectile type of a town NPC's attack, and how long it takes for the projectile to actually appear. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="projType"></param>
    /// <param name="attackDelay"></param>
    public virtual void TownNPCAttackProj(ref int projType, ref int attackDelay) { }

    /// <summary>
    /// Allows you to determine the speed at which a town NPC throws a projectile when it attacks. Multiplier is the speed of the projectile, gravityCorrection is how much extra the projectile gets thrown upwards, and randomOffset allows you to randomize the projectile's velocity in a square centered around the original velocity. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="multiplier"></param>
    /// <param name="gravityCorrection"></param>
    /// <param name="randomOffset"></param>
    public virtual void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) { }

    /// <summary>
    /// Allows you to tell the game that a town NPC has already created a projectile and will still create more projectiles as part of a single attack so that the game can animate the NPC's attack properly. Only used when the town NPC has an attack type of 1 (shooting).
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="inBetweenShots"></param>
    public virtual void TownNPCAttackShoot(ref bool inBetweenShots) { }

    /// <summary>
    /// Allows you to control the brightness of the light emitted by a town NPC's aura when it performs a magic attack. Only used when the town NPC has an attack type of 2 (magic)
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="auraLightMultiplier"></param>
    public virtual void TownNPCAttackMagic(ref float auraLightMultiplier) { }

    /// <summary>
    /// Allows you to determine the width and height of the item a town NPC swings when it attacks, which controls the range of the NPC's swung weapon. Only used when the town NPC has an attack type of 3 (swinging).
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="itemWidth"></param>
    /// <param name="itemHeight"></param>
    public virtual void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) { }

    /// <summary>
    /// Allows you to customize how a town NPC's weapon is drawn when the NPC is shooting (the NPC must have an attack type of 1). <paramref name="scale"/> is a multiplier for the item's drawing size, <paramref name="item"/> is the Texture2D instance of the item to be drawn, <paramref name="itemFrame"/> is the section of the texture to draw, and <paramref name="horizontalHoldoutOffset"/> is how far away the item should be drawn from the NPC.
    /// <para/> Called on the server and clients.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="itemFrame"></param>
    /// <param name="scale"></param>
    /// <param name="horizontalHoldoutOffset"></param>
    public virtual void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) { }


    /// <inheritdoc cref="ModNPC.DrawTownAttackSwing" />
    public virtual void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) { }

    /// <summary>
    /// Allows you to make a npc be saved even if it's not a townNPC and NPCID.Sets.SavesAndLoads[npc.type] is false.
    /// <br/><b>NOTE:</b> A town NPC will always be saved (except the Travelling Merchant that never will).
    /// <br/><b>NOTE:</b> A NPC that needs saving will not despawn naturally.
    /// </summary>
    /// <returns></returns>
    public virtual bool NeedSaving() => false;

    /// <summary>
    /// Allows you to save custom data for the given npc.
    /// <br/>
    /// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
    /// <br/><b>NOTE:</b> Try to only save data that isn't default values.
    /// <br/><b>NOTE:</b> The npc may be saved even if NeedSaving returns false and npc is not a townNPC, if another mod returns true on NeedSaving.
    /// </summary>
    /// <param name="tag">The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument</param>
    public virtual void SaveData(TagCompound tag) { }

    /// <summary>
    /// Allows you to load custom data that you have saved for the given npc.
    /// </summary>
    /// <param name="tag"></param>
    public virtual void LoadData(TagCompound tag) { }

    /// <summary>
    /// Allows you to change the emote that the NPC will pick
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="closestPlayer">The <see cref="Player"/> closest to the NPC. You can check the biome the player is in and let the NPC pick the emote that corresponds to the biome.</param>
    /// <param name="emoteList">A list of emote IDs from which the NPC will randomly select one</param>
    /// <param name="otherAnchor">A <see cref="WorldUIAnchor"/> instance that indicates the target of this emote conversation. Use this to get the instance of the <see cref="NPC"/> or <see cref="Player"/> this NPC is talking to.</param>
    /// <returns>Return null to use vanilla mechanic (pick one from the list), otherwise pick the emote by the returned ID. Returning -1 will prevent the emote from being used. Returns null by default</returns>
    public virtual int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => null;

    /// <inheritdoc cref="ModNPC.ChatBubblePosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="ModNPC.PartyHatPosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="ModNPC.EmoteBubblePosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void EmoteBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }
    #endregion SpecialEffects
    #endregion 虚成员
}

public abstract class SingleProjectileBehavior : SingleEntityBehavior<Projectile>
{
    public Projectile Projectile { get; private set; } = null;

    public TOGlobalProjectile OceanProjectile { get; private set; } = null;

    public Player Owner { get; private set; } = null;

    public override void Connect(Projectile projectile)
    {
        Projectile = projectile;
        Owner = Main.player[Projectile.owner];
        OceanProjectile = projectile.Ocean();
    }

    #region 虚成员
    #region Defaults
    /// <summary>
    /// 设置基本属性。
    /// </summary>
    public virtual void SetDefaults() { }
    #endregion Defaults

    #region Lifetime
    /// <summary>
    /// Gets called when any projectiles spawns in world
    /// <para/> Called on the client or server spawning the projectile via Projectile.NewProjectile.
    /// </summary>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <summary>
    /// Allows you to determine how a projectile interacts with tiles. Return false if you completely override or cancel a projectile's tile collision behavior. Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="width"> The width of the hitbox the projectile will use for tile collision. If vanilla or a mod don't modify it, defaults to projectile.width. </param>
    /// <param name="height"> The height of the hitbox the projectile will use for tile collision. If vanilla or a mod don't modify it, defaults to projectile.height. </param>
    /// <param name="fallThrough"> Whether or not the projectile falls through platforms and similar tiles. </param>
    /// <param name="hitboxCenterFrac"> Determines by how much the tile collision hitbox's position (top left corner) will be offset from the projectile's real center. If vanilla or a mod don't modify it, defaults to half the hitbox size (new Vector2(0.5f, 0.5f)). </param>
    /// <returns></returns>
	public virtual bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

    /// <summary>
    /// Allows you to determine what happens when a projectile collides with a tile. OldVelocity is the velocity before tile collision. The velocity that takes tile collision into account can be found with projectile.velocity. Return true to allow the vanilla tile collision code to take place (which normally kills the projectile). Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="oldVelocity"></param>
    /// <returns></returns>
    public virtual bool OnTileCollide(Vector2 oldVelocity) => true;

    /// <summary>
    /// Allows you to determine whether the vanilla code for Kill and the Kill hook will be called. Return false to stop them from being called. Returns true by default. Note that this does not stop the projectile from dying.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <param name="timeLeft"></param>
    /// <returns></returns>
    public virtual bool PreKill(int timeLeft) => true;

    /// <summary>
    /// Allows you to control what happens when a projectile is killed (for example, creating dust or making sounds).
    /// <para/> Can be called on the local client or server, depending on who owns the projectile.
    /// </summary>
    /// <param name="timeLeft"></param>
    public virtual void OnKill(int timeLeft) { }
    #endregion Lifetime

    #region AI
    /// <summary>
    /// AI。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止 <see cref="Projectile.AI"/> 方法运行。</returns>
    public virtual bool PreAI() => true;

    /// <summary>
    /// AI。
    /// </summary>
    public virtual void AI() { }

    /// <summary>
    /// AI。
    /// </summary>
    public virtual void PostAI() { }

    /// <summary>
    /// Whether or not the given projectile should update its position based on factors such as its velocity, whether it is in liquid, etc. Return false to make its velocity have no effect on its position. Returns true by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    /// <returns></returns>
    public virtual bool ShouldUpdatePosition() => true;
    #endregion AI

    #region Hit
    /// <summary>
    /// Return true or false to specify if the projectile can cut tiles like vines, pots, and Queen Bee larva. Return null for vanilla decision.
    /// </summary>
    /// <returns></returns>
    public virtual bool? CanCutTiles() => null;

    /// <summary>
    /// Code ran when the projectile cuts tiles. Only runs if CanCutTiles() returns true. Useful when programming lasers and such.
    /// </summary>
    public virtual void CutTiles() { }

    /// <summary>
    /// Whether or not the given projectile is capable of killing tiles (such as grass) and damaging NPCs/players.
    /// Return false to prevent it from doing any sort of damage.
    /// Return true if you want the projectile to do damage regardless of the default blacklist.
    /// Return null to let the projectile follow vanilla can-damage-anything rules. This is what happens by default.
    /// </summary>
    /// <returns></returns>
    public virtual bool? CanDamage() => null;

    /// <summary>
    /// Whether or not a minion can damage NPCs by touching them. Returns false by default. Note that this will only be used if the projectile is considered a pet.
    /// </summary>
    /// <returns></returns>
    public virtual bool MinionContactDamage() => false;

    /// <summary>
    /// Allows you to change the hitbox used by a projectile for damaging players and NPCs.
    /// </summary>
    /// <param name="hitbox"></param>
    public virtual void ModifyDamageHitbox(ref Rectangle hitbox) { }

    /// <summary>
    /// Allows you to determine whether a projectile can hit the given NPC. Return true to allow hitting the target, return false to block the projectile from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool? CanHitNPC(NPC target) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that a projectile does to an NPC. This method is only called for the owner of the projectile, meaning that in multi-player, projectiles owned by a player call this method on that client, and projectiles owned by the server such as enemy projectiles call this method on the server.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a projectile hits an NPC (for example, inflicting debuffs). This method is only called for the owner of the projectile, meaning that in multi-player, projectiles owned by a player call this method on that client, and projectiles owned by the server such as enemy projectiles call this method on the server.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="hit"></param>
    /// <param name="damageDone"></param>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether a projectile can hit the given opponent player. Return false to block the projectile from hitting the target. Returns true by default.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool CanHitPvp(Player target) => true;

    /// <summary>
    /// Allows you to determine whether a hostile projectile can hit the given player. Return false to block the projectile from hitting the target. Returns true by default.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual bool CanHitPlayer(Player target) => true;

    /// <summary>
    /// Allows you to modify the damage, etc., that a hostile projectile does to a player.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="modifiers"></param>
    public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a hostile projectile hits a player. <br/>
    /// Only runs on the local client in multiplayer.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="info"></param>
    public virtual void OnHitPlayer(Player target, Player.HurtInfo info) { }

    /// <summary>
    /// Allows you to use custom collision detection between a projectile and a player or NPC that the projectile can damage. Useful for things like diagonal lasers, projectiles that leave a trail behind them, etc.
    /// </summary>
    /// <param name="projHitbox"></param>
    /// <param name="targetHitbox"></param>
    /// <returns></returns>
    public virtual bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => null;
    #endregion Hit

    #region Draw
    /// <summary>
    /// Allows you to determine the color and transparency in which a projectile is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
    /// </summary>
    /// <param name="lightColor"></param>
    /// <returns></returns>
    public virtual Color? GetAlpha(Color lightColor) => null;

    /// <summary>
    /// Allows you to draw things behind a projectile. Use the <c>Main.EntitySpriteDraw</c> method for drawing. Returns false to stop the game from drawing extras textures related to the projectile (for example, the chains for grappling hooks), useful if you're manually drawing the extras. Returns true by default.
    /// </summary>
    public virtual bool PreDrawExtras() => true;

    /// <summary>
    /// Allows you to draw things behind a projectile, or to modify the way the projectile is drawn. Use the <c>Main.EntitySpriteDraw</c> method for drawing. Return false to stop the vanilla projectile drawing code (useful if you're manually drawing the projectile). Returns true by default.
    /// </summary>
    /// <param name="lightColor"> The color of the light at the projectile's center. </param>
    public virtual bool PreDraw(ref Color lightColor) => true;

    /// <summary>
    /// Allows you to draw things in front of a projectile. Use the <c>Main.EntitySpriteDraw</c> method for drawing. This method is called even if PreDraw returns false.
    /// </summary>
    /// <param name="lightColor"> The color of the light at the projectile's center, after being modified by vanilla and other mods. </param>
    public virtual void PostDraw(Color lightColor) { }

    /// <summary>
    /// When used in conjunction with "projectile.hide = true", allows you to specify that this projectile should be drawn behind certain elements. Add the index to one and only one of the lists. For example, the Nebula Arcanum projectile draws behind NPCs and tiles.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="behindNPCsAndTiles"></param>
    /// <param name="behindNPCs"></param>
    /// <param name="behindProjectiles"></param>
    /// <param name="overPlayers"></param>
    /// <param name="overWiresUI"></param>
    public virtual void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) { }
    #endregion Draw

    #region SpecialEffects
    /// <summary>
    /// How many of this type of grappling hook the given player can latch onto blocks before the hooks start disappearing. Change the numHooks parameter to determine this; by default it will be 3.
    /// <para/> Called on the local client only.
    /// </summary>
    public virtual void NumGrappleHooks(Player player, ref int numHooks) { }

    /// <summary>
    /// The speed at which the grapple retreats back to the player after not hitting anything. Defaults to 11, but vanilla hooks go up to 24.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void GrappleRetreatSpeed(Player player, ref float speed) { }

    /// <summary>
    /// The speed at which the grapple pulls the player after hitting something. Defaults to 11, but the Bat Hook uses 16.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void GrapplePullSpeed(Player player, ref float speed) { }

    /// <summary>
    /// The location that the grappling hook pulls the player to. Defaults to the center of the hook projectile.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY) { }

    /// <summary>
    /// Whether or not the grappling hook can latch onto the given position in tile coordinates.
    /// <para/> This position may be air or an actuated tile!
    /// <para/> Return true to make it latch, false to prevent it, or null to apply vanilla conditions. Returns null by default.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool? GrappleCanLatchOnTo(Player player, int x, int y) => null;

    /// <inheritdoc cref="ModProjectile.PrepareBombToBlow"/>
    public virtual void PrepareBombToBlow() { }

    /// <inheritdoc cref="ModProjectile.EmitEnchantmentVisualsAt"/>
    public virtual void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) { }
    #endregion SpecialEffects
    #endregion 虚成员
}

public abstract class SingleItemBehavior : SingleEntityBehavior<Item>
{
    public Item Item { get; private set; } = null;

    public TOGlobalItem OceanItem { get; private set; } = null;

    public override void Connect(Item item)
    {
        Item = item;
        OceanItem = item.Ocean();
    }

    #region 虚成员
    #region Defaults
    /// <summary>
    /// 设置基本属性。
    /// </summary>
    public virtual void SetDefaults() { }

    /// <summary>
    /// Override this method to add <see cref="Recipe"/>s to the game.<br/>
    /// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Recipes">Basic Recipes Guide</see> teaches how to add new recipes to the game and how to manipulate existing recipes.<br/>
    /// </summary>
    public virtual void AddRecipes() { }
    #endregion Defaults

    #region Lifetime
    /// <summary>
    /// Called when the item is created. The <paramref name="context"/> parameter indicates the context of the item creation and can be used in logic for the desired effect.
    /// <para/> Known <see cref="ItemCreationContext"/> include: <see cref="InitializationItemCreationContext"/>, <see cref="BuyItemCreationContext"/>, <see cref="JourneyDuplicationItemCreationContext"/>, and <see cref="RecipeItemCreationContext"/>. Some of these provide additional context such as how <see cref="RecipeItemCreationContext"/> includes the items consumed to craft the <paramref name="item"/>.
    /// </summary>
    public virtual void OnCreated(ItemCreationContext context) { }

    /// <summary>
    /// Gets called when any item spawns in world
    /// </summary>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <summary>
    /// Allows you to customize an item's movement when lying in the world. Note that this will not be called if the item is currently being grabbed by a player.
    /// </summary>
    public virtual void Update(ref float gravity, ref float maxFallSpeed) { }

    /// <summary>
    /// Allows you to make things happen when an item is lying in the world. This will always be called, even when the item is being grabbed by a player. This hook should be used for adding light, or for increasing the age of less valuable items.
    /// </summary>
    public virtual void PostUpdate() { }

    /// <summary>
    /// Allows you to modify how close an item must be to the player in order to move towards the player.
    /// </summary>
    public virtual void GrabRange(Player player, ref int grabRange) { }

    /// <summary>
    /// Allows you to modify the way an item moves towards the player. Return false to allow the vanilla grab style to take place. Returns false by default.
    /// </summary>
    public virtual bool GrabStyle(Player player) => false;

    /// <summary>
    /// Allows you to determine whether or not the item can be picked up
    /// </summary>
    public virtual bool CanPickup(Player player) => true;

    /// <summary>
    /// Allows you to make special things happen when the player picks up an item. Return false to stop the item from being added to the player's inventory; returns true by default.
    /// </summary>
    public virtual bool OnPickup(Player player) => true;

    /// <summary>
    /// Return true to specify that the item can be picked up despite not having enough room in inventory. Useful for something like hearts or experience items. Use in conjunction with OnPickup to actually consume the item and handle it.
    /// </summary>
    public virtual bool ItemSpace(Player player) => false;
    #endregion Lifetime

    #region Update
    /// <summary>
    /// Allows you to make things happen when an item is in the player's inventory. This should NOT be used for information accessories;
    /// use <seealso cref="UpdateInfoAccessory"/> for those instead.
    /// </summary>
    public virtual void UpdateInventory(Player player) { }

    /// <summary>
    /// Allows you to set information accessory fields with the passed in player argument. This hook should only be used for information
    /// accessory fields such as the Radar, Lifeform Analyzer, and others. Using it for other fields will likely cause weird side-effects.
    /// </summary>
    public virtual void UpdateInfoAccessory(Player player) { }

    /// <summary>
    /// Allows you to give effects to armors and accessories, such as increased damage.
    /// </summary>
    public virtual void UpdateEquip(Player player) { }

    /// <summary>
    /// Allows you to give effects to accessories. The hideVisual parameter is whether the player has marked the accessory slot to be hidden from being drawn on the player.
    /// </summary>
    public virtual void UpdateAccessory(Player player, bool hideVisual) { }

    /// <summary>
    /// Allows you to give effects to this accessory when equipped in a vanity slot. Vanilla uses this for boot effects, wings and merman/werewolf visual flags
    /// </summary>
    public virtual void UpdateVanity(Player player) { }

    /// <summary>
    /// Allows you to set custom draw flags for this accessory that can be checked in a <see cref="PlayerDrawLayer"/> or other drawcode. Not required if using pre-existing layers (e.g. face, back).
    /// <para/> <paramref name="hideVisual"/> indicates if the accessory is hidden (in a non-vanity accessory slot that is set to hidden). It sounds counterintuitive for this method to be called on hidden accessories, but this can be used for effects where the visuals of an accessory should be forced despite the player hiding the accessory. For example, wings will always show while in the air and the Shield of Cthulhu will always show while its dash is active even while hidden.
    /// </summary>
    public virtual void UpdateVisibleAccessory(Player player, bool hideVisual) { }

    /// <summary>
    /// Allows tracking custom shader values corresponding to specific items or custom player layers for equipped accessories. <paramref name="dye"/> is the <see cref="Item.dye"/> of the item in the dye slot. <paramref name="hideVisual"/> indicates if this item is in a non-vanity accessory slot that is set to hidden. Most implementations will not assign shaders if the accessory is hidden, but there are rare cases where it is desired to assign the shader regardless of accessory visibility. One example is Hand Of Creation, the player can disable visibility of the accessory to prevent the backpack visuals from showing, but the stool will still be properly dyed by the corresponding dye item when visible.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="dye"></param>
    /// <param name="hideVisual"></param>
    public virtual void UpdateItemDye(Player player, int dye, bool hideVisual) { }
    #endregion Update

    #region Draw
    /// <summary>
    /// Allows you to determine the color and transparency in which an item is drawn. Return null to use the default color (normally light color). Returns null by default.
    /// </summary>
    public virtual Color? GetAlpha(Color lightColor) => null;

    /// <summary>
    /// <inheritdoc cref="ModItem.PreDrawInWorld(SpriteBatch, Color, Color, ref float, ref float, int)"/>
    /// </summary>
    public virtual bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => true;

    /// <summary>
    /// <inheritdoc cref="ModItem.PostDrawInWorld(SpriteBatch, Color, Color, float, float, int)"/>
    /// </summary>
    public virtual void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) { }

    /// <summary>
    /// <inheritdoc cref="ModItem.PreDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/>
    /// </summary>
    public virtual bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

    /// <summary>
    /// <inheritdoc cref="ModItem.PostDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/>
    /// </summary>
    public virtual void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { }
    #endregion Draw

    #region Prefix
    /// <summary>
    /// Allows you to manually choose what prefix an item will get.
    /// </summary>
    /// <returns>The ID of the prefix to give the item, -1 to use default vanilla behavior</returns>
    public virtual int ChoosePrefix(UnifiedRandom rand) => -1;

    /// <summary>
    /// To prevent putting the item in the tinkerer slot, return false when pre is -3.
    /// To prevent rolling of a prefix on spawn, return false when pre is -1.
    /// To force rolling of a prefix on spawn, return true when pre is -1.
    ///
    /// To reduce the probability of a prefix on spawn (pre == -1) to X%, return false 100-4X % of the time.
    /// To increase the probability of a prefix on spawn (pre == -1) to X%, return true (4X-100)/3 % of the time.
    ///
    /// To delete a prefix from an item when the item is loaded, return false when pre is the prefix you want to delete.
    /// Use AllowPrefix to prevent rolling of a certain prefix.
    /// </summary>
    /// <param name="pre">The prefix being applied to the item, or the roll mode. -1 is when the item is naturally generated in a chest, crafted, purchased from an NPC, looted from a grab bag (excluding presents), or dropped by a slain enemy (if it's spawned with prefixGiven: -1). -2 is when the item is rolled in the tinkerer. -3 determines if the item can be placed in the tinkerer slot.</param>
    /// <param name="rand"></param>
    /// <returns></returns>
    public virtual bool? PrefixChance(int pre, UnifiedRandom rand) => null;

    /// <summary>
    /// Force a re-roll of a prefix by returning false.
    /// </summary>
    public virtual bool AllowPrefix(int pre) => true;

    /// <summary>
    /// Returns if the normal reforge pricing is applied.
    /// If true or false is returned and the price is altered, the price will equal the altered price.
    /// The passed reforge price equals the item.value. Vanilla pricing will apply 20% discount if applicable and then price the reforge at a third of that value.
    /// </summary>
    public virtual bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount) => true;

    /// <summary>
    /// This hook gets called when the player clicks on the reforge button and can afford the reforge.
    /// Returns whether the reforge will take place. If false is returned by the ModItem or any GlobalItem, the item will not be reforged, the cost to reforge will not be paid, and PreRefoge and PostReforge hooks will not be called.
    /// Reforging preserves modded data on the item.
    /// </summary>
    public virtual bool CanReforge() => true;

    /// <summary>
    /// This hook gets called immediately before an item gets reforged by the Goblin Tinkerer.
    /// </summary>
    public virtual void PreReforge() { }

    /// <summary>
    /// This hook gets called immediately after an item gets reforged by the Goblin Tinkerer.
    /// Useful for modifying modded data based on the reforge result.
    /// </summary>
    public virtual void PostReforge() { }
    #endregion Prefix

    #region Use
    /// <summary>
    /// Allows you to make this item usable by right-clicking. When this item is used by right-clicking, <see cref="Player.altFunctionUse"/> will be set to 2. Check the value of altFunctionUse in <see cref="UseItem(Player)"/> to apply right-click specific logic. For auto-reusing through right clicking, see also <see cref="ItemID.Sets.ItemsThatAllowRepeatedRightClick"/>.
    /// <para/> Returns false by default.
    /// <para/> Called on the local client only.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns></returns>
    public virtual bool AltFunctionUse(Player player) => false;

    /// <summary>
    /// Returns whether or not any item can be used. Returns true by default. The inability to use a specific item overrides this, so use this to stop an item from being used.
    /// <para/> Called on local, server, and remote clients.
    /// </summary>
    public virtual bool CanUseItem(Player player) => true;

    /// <summary>
    /// Allows you to modify the autoswing (auto-reuse) behavior of any item without having to mess with Item.autoReuse.
    /// <br>Useful to create effects like the Feral Claws which makes melee weapons and whips auto-reusable.</br>
    /// <br>Return true to enable autoswing (if not already enabled through autoReuse), return false to prevent autoswing. Returns null by default, which applies vanilla behavior.</br>
    /// </summary>
    /// <param name="player"> The player. </param>
    public virtual bool? CanAutoReuseItem(Player player) => null;

    /// <summary>
    /// Allows you to modify the location and rotation of any item in its use animation.
    /// </summary>
    /// <param name="player"> The player. </param>
    /// <param name="heldItemFrame"> The source rectangle for the held item's texture. </param>
    public virtual void UseStyle(Player player, Rectangle heldItemFrame) { }

    /// <summary>
    /// Allows you to modify the location and rotation of the item the player is currently holding.
    /// </summary>
    /// <param name="player"> The player. </param>
    /// <param name="heldItemFrame"> The source rectangle for the held item's texture. </param>
    public virtual void HoldStyle(Player player, Rectangle heldItemFrame) { }

    /// <summary>
    /// Allows you to make things happen when the player is holding an item (for example, torches make light and water candles increase spawn rate).
    /// </summary>
    public virtual void HoldItem(Player player) { }

    /// <summary>
    /// Allows you to change the effective useTime of an item.
    /// <br/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
    /// </summary>
    /// <returns> The multiplier on the usage time. 1f by default. Values greater than 1 increase the item use's length. </returns>
    public virtual float UseTimeMultiplier(Player player) => 1f;

    /// <summary>
    /// Allows you to change the effective useAnimation of an item.
    /// <br/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
    /// </summary>
    /// <returns>The multiplier on the animation time. 1f by default. Values greater than 1 increase the item animation's length. </returns>
    public virtual float UseAnimationMultiplier(Player player) => 1f;

    /// <summary>
    /// Allows you to safely change both useTime and useAnimation while keeping the values relative to each other.
    /// <br/> Useful for status effects.
    /// </summary>
    /// <returns> The multiplier on the use speed. 1f by default. Values greater than 1 increase the overall item speed. </returns>
    public virtual float UseSpeedMultiplier(Player player) => 1f;

    /// <summary>
    /// Allows you to make things happen when an item is used. The return value controls whether or not ApplyItemTime will be called for the player.
    /// <br/> Return true if the item actually did something, to force itemTime.
    /// <br/> Return false to keep itemTime at 0.
    /// <br/> Return null for vanilla behavior.
    /// </summary>
    public virtual bool? UseItem(Player player) => null;

    /// <summary>
    /// Allows you to make things happen when an item's use animation starts.
    /// </summary>
    public virtual void UseAnimation(Player player) { }

    /// <summary>
    /// Allows you to prevent an item from shooting a projectile on use. Returns true by default.
    /// </summary>
    /// <param name="player"> The player using the item. </param>
    /// <returns></returns>
    public virtual bool CanShoot(Player player) => true;

    /// <summary>
    /// Allows you to modify an item's shooting mechanism. Return false to prevent vanilla's shooting code from running. Returns true by default.<br/>
    /// This method is called after the <see cref="ModifyShootStats"/> hook has had a chance to adjust the spawn parameters. 
    /// </summary>
    /// <param name="player"> The player using the item. </param>
    /// <param name="source"> The projectile source's information. </param>
    /// <param name="position"> The center position of the projectile. </param>
    /// <param name="velocity"> The velocity of the projectile. </param>
    /// <param name="type"> The ID of the projectile. </param>
    /// <param name="damage"> The damage of the projectile. </param>
    /// <param name="knockback"> The knockback of the projectile. </param>
    public virtual bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <summary>
    /// Returns whether or not an item does something when right-clicked in the inventory. Returns false by default.
    /// </summary>
    public virtual bool CanRightClick() => false;

    /// <summary>
    /// Allows you to make things happen when an item is right-clicked in the inventory. Useful for goodie bags.
    /// </summary>
    public virtual void RightClick(Player player) { }
    #endregion Use

    #region ModifyStats
    /// <summary>
    /// Allows you to dynamically modify a weapon's damage based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <br/><br/> <b>Do not</b> modify <see cref="Item.damage"/>, modify the <paramref name="damage"/> parameter.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="damage">The StatModifier object representing the totality of the various modifiers to be applied to the item's base damage.</param>
    public virtual void ModifyWeaponDamage(Player player, ref StatModifier damage) { }

    /// <summary>
    /// Allows you to dynamically modify a weapon's knockback based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <br/><br/> <b>Do not</b> modify <see cref="Item.knockBack"/>, modify the <paramref name="knockback"/> parameter.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="knockback">The StatModifier object representing the totality of the various modifiers to be applied to the item's base knockback.</param>
    public virtual void ModifyWeaponKnockback(Player player, ref StatModifier knockback) { }

    /// <summary>
    /// Allows you to dynamically modify a weapon's crit chance based on player and item conditions.
    /// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
    /// <br/><br/> <b>Do not</b> modify <see cref="Item.crit"/>, modify the <paramref name="crit"/> parameter.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="crit">The total crit chance of the item after all normal crit chance calculations.</param>
    public virtual void ModifyWeaponCrit(Player player, ref float crit) { }

    /// <summary>
    /// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by an item.<br/>
    /// These parameters will be provided to <see cref="Shoot(Item, Player, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float)"/> where the projectile will actually be spawned.
    /// </summary>
    /// <param name="player"> The player using the item. </param>
    /// <param name="position"> The center position of the projectile. </param>
    /// <param name="velocity"> The velocity of the projectile. </param>
    /// <param name="type"> The ID of the projectile. </param>
    /// <param name="damage"> The damage of the projectile. </param>
    /// <param name="knockback"> The knockback of the projectile. </param>
    public virtual void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <summary>
    /// Allows you to dynamically modify the given item's size for the given player, similarly to the effect of the Titan Glove.
    /// <br/><br/> <b>Do not</b> modify <see cref="Item.scale"/>, modify the <paramref name="scale"/> parameter.
    /// </summary>
    /// <param name="player">The player wielding the given item.</param>
    /// <param name="scale">
    /// The scale multiplier to be applied to the given item.<br></br>
    /// Will be 1.1 if the Titan Glove is equipped, and 1 otherwise.
    /// </param>
    public virtual void ModifyItemScale(Player player, ref float scale) { }
    #endregion ModifyStats

    #region Hit
    /// <summary>
    /// Allows you to determine whether a melee weapon can hit the given NPC when swung. Return true to allow hitting the target, return false to block the weapon from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
    /// </summary>
    public virtual bool? CanHitNPC(Player player, NPC target) => null;

    /// <summary>
    /// Allows you to determine whether a melee weapon can collide with the given NPC when swung. <br/>
    /// Use <see cref="CanHitNPC(Player, NPC)"/> instead for Flymeal-type effects.
    /// </summary>
    /// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
    /// <param name="player">The player wielding this item.</param>
    /// <param name="target">The target npc.</param>
    /// <returns>
    /// Return true to allow colliding with target, return false to block the weapon from colliding with target, and return null to use the vanilla code for whether the target can be colliding. Returns null by default.
    /// </returns>
    public virtual bool? CanMeleeAttackCollideWithNPC(Rectangle meleeAttackHitbox, Player player, NPC target) => null;

    /// <summary>
    /// Allows you to modify the damage, knockback, etc., that a melee weapon does to an NPC. <br/>
    /// This method is only called on the on the client of the player holding the weapon. <br/>
    /// </summary>
    public virtual void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a melee weapon hits an NPC (for example how the Pumpkin Sword creates pumpkin heads).
    /// </summary>
    public virtual void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// Allows you to determine whether a melee weapon can hit the given opponent player when swung. Return false to block the weapon from hitting the target. Returns true by default.
    /// </summary>
    public virtual bool CanHitPvp(Player player, Player target) => true;

    /// <summary>
    /// Allows you to modify the damage, etc., that a melee weapon does to a player.
    /// </summary>
    public virtual void ModifyHitPvp(Player player, Player target, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// Allows you to create special effects when a melee weapon hits a player. <br/>
    /// Called on local, server and remote clients. <br/>
    /// </summary>
    public virtual void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) { }
    #endregion Hit

    #region SpecialEffects
    /// <summary>
    /// Allows you to temporarily modify the amount of life a life healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healLife"/> value.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
    /// <param name="healValue">The amount of life being healed.</param>
    public virtual void GetHealLife(Player player, bool quickHeal, ref int healValue) { }

    /// <summary>
    /// Allows you to temporarily modify the amount of mana a mana healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healMana"/> value.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
    /// <param name="healValue">The amount of mana being healed.</param>
    public virtual void GetHealMana(Player player, bool quickHeal, ref int healValue) { }

    /// <summary>
    /// Allows you to temporarily modify the amount of mana an item will consume on use, based on player buffs, accessories, etc. This is only called for items with a mana value.
    /// <br/><br/> <b>Do not</b> modify <see cref="Item.mana"/>, modify the <paramref name="reduce"/> and <paramref name="mult"/> parameters.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="reduce">Used for decreasingly stacking buffs (most common). Only ever use -= on this field.</param>
    /// <param name="mult">Use to directly multiply the item's effective mana cost. Good for debuffs, or things which should stack separately (eg meteor armor set bonus).</param>
    public virtual void ModifyManaCost(Player player, ref float reduce, ref float mult) { }

    /// <summary>
    /// Allows you to make stuff happen when a player doesn't have enough mana for an item they are trying to use.
    /// If the player has high enough mana after this hook runs, mana consumption will happen normally.
    /// Only runs once per item use.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="neededMana">The mana needed to use the item.</param>
    public virtual void OnMissingMana(Player player, int neededMana) { }

    /// <summary>
    /// Allows you to make stuff happen when a player consumes mana on use of an item.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="manaConsumed">The mana consumed from the player.</param>
    public virtual void OnConsumeMana(Player player, int manaConsumed) { }

    /// <summary>
    /// Allows you to prevent an item from being researched by returning false. True is the default behavior.
    /// </summary>
    public virtual bool CanResearch() => true;

    /// <summary>
    /// Allows you to create custom behavior when an item is accepted by the Research function 
    /// </summary>
    /// <param name="fullyResearched">True if the item was completely researched, and is ready to be duplicated, false if only partially researched.</param>
    public virtual void OnResearched(bool fullyResearched) { }

    /// <summary>
    /// Allows you to set an item's sorting group in Journey Mode's duplication menu. This is useful for setting custom item types that group well together, or whenever the default vanilla sorting doesn't sort the way you want it.
    /// <para/> Note that this affects the order of the item in the listing, not which filters the item satisfies.
    /// </summary>
    /// <param name="itemGroup">The item group this item is being assigned to</param>
    public virtual void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) { }

    /// <summary>
    /// Whether or not having no ammo prevents an item that uses ammo from shooting.
    /// Return false to allow shooting with no ammo in the inventory, in which case the item will act as if the default ammo for it is being used.
    /// Returns true by default.
    /// </summary>
    public virtual bool NeedsAmmo(Player player) => true;

    /// <summary>
    /// Changes the hitbox of a melee weapon when it is used.
    /// </summary>
    public virtual void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) { }

    /// <summary>
    /// Allows you to give melee weapons special effects, such as creating light or dust.
    /// </summary>
    public virtual void MeleeEffects(Player player, Rectangle hitbox) { }

    /// <summary>
    /// Allows you to determine whether the given item can catch the given NPC.<br></br>
    /// Return true or false to say the given NPC can or cannot be caught, respectively, regardless of vanilla rules.<br></br>
    /// Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.<br></br>
    /// If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.<br></br><br></br>
    /// NOTE: this does not classify the given item as an NPC-catching tool, which is necessary for catching NPCs in the first place.<br></br>
    /// To do that, you will need to use the "CatchingTool" set in ItemID.Sets.
    /// </summary>
    /// <param name="target">The NPC the player is trying to catch.</param>
    /// <param name="player">The player attempting to catch the NPC.</param>
    /// <returns></returns>
    public virtual bool? CanCatchNPC(NPC target, Player player) => null;

    /// <summary>
    /// Allows you to make things happen when the given item attempts to catch the given NPC.
    /// </summary>
    /// <param name="npc">The NPC which the player attempted to catch.</param>
    /// <param name="player">The player attempting to catch the given NPC.</param>
    /// <param name="failed">Whether or not the given NPC has been successfully caught.</param>
    public virtual void OnCatchNPC(NPC npc, Player player, bool failed) { }

    /// <summary>
    /// If the item is consumable and this returns true, then the item will be consumed upon usage. Returns true by default.
    /// If false is returned, the OnConsumeItem hook is never called.
    /// </summary>
    public virtual bool ConsumeItem(Player player) => true;

    /// <summary>
    /// Allows you to make things happen when this item is consumed.
    /// Called before the item stack is reduced.
    /// </summary>
    public virtual void OnConsumeItem(Player player) { }

    /// <summary>
    /// Allows you to modify the player's animation when an item is being used.
    /// </summary>
    public virtual void UseItemFrame(Player player) { }

    /// <summary>
    /// Allows you to modify the player's animation when the player is holding an item.
    /// </summary>
    public virtual void HoldItemFrame(Player player) { }

    /// <summary>
    /// Allows you to modify the speeds at which you rise and fall when wings are equipped.
    /// </summary>
    public virtual void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
        ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    { }

    /// <summary>
    /// Allows you to modify the horizontal flight speed and acceleration of wings.
    /// </summary>
    public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) { }

    /// <summary>
    /// Allows you to disallow the player from equipping an accessory. Return false to disallow equipping the accessory. Returns true by default.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="slot">The inventory slot that the item is attempting to occupy.</param>
    /// <param name="modded">If the inventory slot index is for modded slots.</param>
    public virtual bool CanEquipAccessory(Player player, int slot, bool modded) => true;
    #endregion SpecialEffects

    #region Tooltip
    /// <summary>
    /// Allows you to do things before this item's tooltip is drawn.
    /// </summary>
    /// <param name="lines">The tooltip lines for this item</param>
    /// <param name="x">The top X position for this tooltip. It is where the first line starts drawing</param>
    /// <param name="y">The top Y position for this tooltip. It is where the first line starts drawing</param>
    /// <returns>Whether or not to draw this tooltip</returns>
    public virtual bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => true;

    /// <summary>
    /// Allows you to do things after this item's tooltip is drawn. The lines contain draw information as this is ran after drawing the tooltip.
    /// </summary>
    /// <param name="lines">The tooltip lines for this item</param>
    public virtual void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines) { }

    /// <summary>
    /// Allows you to do things before a tooltip line of this item is drawn. The line contains draw info.
    /// </summary>
    /// <param name="line">The line that would be drawn</param>
    /// <param name="yOffset">The Y offset added for next tooltip lines</param>
    /// <returns>Whether or not to draw this tooltip line</returns>
    public virtual bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) => true;

    /// <summary>
    /// Allows you to do things after a tooltip line of this item is drawn. The line contains draw info.
    /// </summary>
    /// <param name="line">The line that was drawn</param>
    public virtual void PostDrawTooltipLine(DrawableTooltipLine line) { }

    /// <summary>
    /// Allows you to modify all the tooltips that display for the given item. See here for information about TooltipLine. To hide tooltips, please use <see cref="TooltipLine.Hide"/> and defensive coding.
    /// </summary>
    public virtual void ModifyTooltips(List<TooltipLine> tooltips) { }
    #endregion Tooltip

    #region WorldSaving
    /// <summary>
    /// Allows you to save custom data for this item.
    /// <br/>
    /// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
    /// <br/><b>NOTE:</b> Try to only save data that isn't default values.
    /// </summary>
    /// <param name="tag"> The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument only for the sake of convenience and optimization. </param>
    public virtual void SaveData(TagCompound tag) { }

    /// <summary>
    /// Allows you to load custom data that you have saved for this item.
    /// <br/><b>Try to write defensive loading code that won't crash if something's missing.</b>
    /// </summary>
    /// <param name="tag"> The TagCompound to load data from. </param>
    public virtual void LoadData(TagCompound tag) { }
    #endregion WorldSaving

    #region Net
    /// <inheritdoc cref="ModItem.NetSend"/>
    public virtual void NetSend(BinaryWriter writer) { }

    /// <inheritdoc cref="ModItem.NetReceive"/>
    public virtual void NetReceive(BinaryReader reader) { }
    #endregion Net
    #endregion 虚成员
}
#endregion Single Behavior

#region Single Behavior Handler
[CriticalBehavior]
public abstract class SingleNPCBehaviorHandler<TNPCBehavior> : GlobalNPCBehavior where TNPCBehavior : SingleNPCBehavior
{
    protected abstract SingleEntityBehaviorSet<NPC, TNPCBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(NPC npc, out TNPCBehavior npcBehavior, [CallerMemberName] string methodName = null!) => BehaviorSet.TryGetBehavior(npc, methodName, out npcBehavior);

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (TNPCBehavior npcBehavior in BehaviorSet)
            npcBehavior.SetStaticDefaults();
    }

    public override void SetDefaults(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.SetDefaults();
    }

    public override void SetDefaultsFromNetId(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.SetDefaultsFromNetId();
    }

    public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ApplyDifficultyAndPlayerScaling(numPlayers, balance, bossAdjustment);
    }

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.SetBestiary(database, bestiaryEntry);
    }

    public override void ModifyTypeName(NPC npc, ref string typeName)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyTypeName(ref typeName);
    }

    public override void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHoverBoundingBox(ref boundingBox);
    }

    public override ITownNPCProfile ModifyTownNPCProfile(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            return npcBehavior.ModifyTownNPCProfile();
        return null;
    }

    public override void ModifyNPCNameList(NPC npc, List<string> nameList)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyNPCNameList(nameList);
    }

    public override void ResetEffects(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ResetEffects();
    }
    #endregion Defaults

    #region Lifetime
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnSpawn(source);
    }

    public override bool CheckActive(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CheckActive())
                return false;
        }
        return true;
    }

    public override bool CheckDead(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CheckDead())
                return false;
        }
        return true;
    }

    public override bool SpecialOnKill(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (npcBehavior.SpecialOnKill())
                return true;
        }
        return false;
    }

    public override bool PreKill(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreKill())
                return false;
        }
        return true;
    }

    public override void OnKill(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnKill();
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyNPCLoot(npcLoot);
    }
    #endregion Lifetime

    #region AI
    public override bool PreAI(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreAI())
                return false;
        }
        return true;
    }

    public override void AI(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.AI();
    }

    public override void PostAI(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.PostAI();
    }
    #endregion AI

    #region Draw
    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.FindFrame(frameHeight);
    }

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            Color? result = npcBehavior.GetAlpha(drawColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.DrawEffects(ref drawColor);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreDraw(spriteBatch, screenPos, drawColor))
                return false;
        }
        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.PostDraw(spriteBatch, screenPos, drawColor);
    }

    public override void DrawBehind(NPC npc, int index)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.DrawBehind(index);
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.BossHeadSlot(ref index);
    }

    public override void BossHeadRotation(NPC npc, ref float rotation)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.BossHeadRotation(ref rotation);
    }

    public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.BossHeadSpriteEffects(ref spriteEffects);
    }

    public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.DrawHealthBar(hbPosition, ref scale, ref position);
            if (result is not null)
                return result;
        }
        return null;
    }
    #endregion Draw

    #region Hit
    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.HitEffect(hit);
    }

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeHitByItem(player, item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHitByItem(player, item, ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnHitByItem(player, item, hit, damageDone);
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeHitByProjectile(projectile);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHitByProjectile(projectile, ref modifiers);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnHitByProjectile(projectile, hit, damageDone);
    }

    public override bool CanBeHitByNPC(NPC npc, NPC attacker)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanBeHitByNPC(attacker))
                return false;
        }
        return true;
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyIncomingHit(ref modifiers);
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanHitPlayer(target, ref cooldownSlot))
                return false;
        }
        return true;
    }

    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnHitPlayer(target, hurtInfo);
    }

    public override bool CanHitNPC(NPC npc, NPC target)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanHitNPC(target))
                return false;
        }
        return true;
    }

    public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnHitNPC(target, hit);
    }

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox))
                return false;
        }
        return true;
    }
    #endregion Hit

    #region SpecialEffects
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.UpdateLifeRegen(ref damage);
    }

    public override bool? CanFallThroughPlatforms(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanFallThroughPlatforms();
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanBeCaughtBy(NPC npc, Item item, Player player)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeCaughtBy(item, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnCaughtBy(player, item, failed);
    }

    public override bool? CanChat(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanChat();
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.GetChat(ref chat);
    }

    public override bool PreChatButtonClicked(NPC npc, bool firstButton)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreChatButtonClicked(firstButton))
                return false;
        }
        return true;
    }

    public override void OnChatButtonClicked(NPC npc, bool firstButton)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnChatButtonClicked(firstButton);
    }

    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ModifyActiveShop(shopName, items);
    }

    public override bool? CanGoToStatue(NPC npc, bool toKingStatue)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanGoToStatue(toKingStatue);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnGoToStatue(NPC npc, bool toKingStatue)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.OnGoToStatue(toKingStatue);
    }

    public override bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (!npcBehavior.ModifyDeathMessage(ref customText, ref color))
                return false;
        }
        return true;
    }

    public override void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackStrength(ref damage, ref knockback);
    }

    public override void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);
    }

    public override void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackProj(ref projType, ref attackDelay);
    }

    public override void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackProjSpeed(ref multiplier, ref gravityCorrection, ref randomOffset);
    }

    public override void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackShoot(ref inBetweenShots);
    }

    public override void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackMagic(ref auraLightMultiplier);
    }

    public override void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackSwing(ref itemWidth, ref itemHeight);
    }

    public override void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.DrawTownAttackGun(ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
    }

    public override void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.DrawTownAttackSwing(ref item, ref itemFrame, ref itemSize, ref scale, ref offset);
    }

    public override int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            int? result = npcBehavior.PickEmote(closestPlayer, emoteList, otherAnchor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.ChatBubblePosition(ref position, ref spriteEffects);
    }

    public override void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.PartyHatPosition(ref position, ref spriteEffects);
    }

    public override void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.EmoteBubblePosition(ref position, ref spriteEffects);
    }
    #endregion SpecialEffects

    #region WorldSaving
    public override bool NeedSaving(NPC npc)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
        {
            if (npcBehavior.NeedSaving())
                return true;
        }
        return false;
    }

    public override void SaveData(NPC npc, TagCompound tag)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.SaveData(tag);
    }

    public override void LoadData(NPC npc, TagCompound tag)
    {
        if (TryGetBehavior(npc, out TNPCBehavior npcBehavior))
            npcBehavior.LoadData(tag);
    }
    #endregion WorldSaving
}

[CriticalBehavior]
public abstract class SingleProjectileBehaviorHandler<TProjectileBehavior> : GlobalProjectileBehavior where TProjectileBehavior : SingleProjectileBehavior
{
    protected abstract SingleEntityBehaviorSet<Projectile, TProjectileBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(Projectile projectile, out TProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null!) => BehaviorSet.TryGetBehavior(projectile, methodName, out projectileBehavior);

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (TProjectileBehavior projectileBehavior in BehaviorSet)
            projectileBehavior.SetStaticDefaults();
    }

    public override void SetDefaults(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.SetDefaults();
    }
    #endregion Defaults

    #region Lifetime
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.OnSpawn(source);
    }

    public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac))
                return false;
        }
        return true;
    }

    public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.OnTileCollide(oldVelocity))
                return false;
        }
        return true;
    }

    public override bool PreKill(Projectile projectile, int timeLeft)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreKill(timeLeft))
                return false;
        }
        return true;
    }

    public override void OnKill(Projectile projectile, int timeLeft)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.OnKill(timeLeft);
    }
    #endregion Lifetime

    #region AI
    public override bool PreAI(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreAI())
                return false;
        }
        return true;
    }

    public override void AI(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.AI();
    }

    public override void PostAI(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.PostAI();
    }

    public override bool ShouldUpdatePosition(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.ShouldUpdatePosition())
                return false;
        }
        return true;
    }
    #endregion AI

    #region Draw
    public override Color? GetAlpha(Projectile projectile, Color lightColor)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            Color? result = projectileBehavior.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreDrawExtras())
                return false;
        }
        return true;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreDraw(ref lightColor))
                return false;
        }
        return true;
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.PostDraw(lightColor);
    }

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
    #endregion Draw

    #region Hit
    public override bool? CanCutTiles(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanCutTiles();
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void CutTiles(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.CutTiles();
    }

    public override bool? CanDamage(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanDamage();
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool MinionContactDamage(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.MinionContactDamage())
                return false;
        }
        return false;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyDamageHitbox(ref hitbox);
    }

    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanHitNPC(target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.OnHitNPC(target, hit, damageDone);
    }

    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.CanHitPvp(target))
                return false;
        }
        return true;
    }

    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.CanHitPlayer(target))
                return false;
        }
        return true;
    }

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.OnHitPlayer(target, info);
    }

    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.Colliding(projHitbox, targetHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }
    #endregion Hit

    #region SpecialEffects
    public override void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.NumGrappleHooks(player, ref numHooks);
    }

    public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.GrappleRetreatSpeed(player, ref speed);
    }

    public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.GrapplePullSpeed(player, ref speed);
    }

    public override void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.GrappleTargetPoint(player, ref grappleX, ref grappleY);
    }

    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.GrappleCanLatchOnTo(player, x, y);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void PrepareBombToBlow(Projectile projectile)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.PrepareBombToBlow();
    }

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
            projectileBehavior.EmitEnchantmentVisualsAt(boxPosition, boxWidth, boxHeight);
    }
    #endregion SpecialEffects
}

[CriticalBehavior]
public abstract class SingleItemBehaviorHandler<TItemBehavior> : GlobalItemBehavior where TItemBehavior : SingleItemBehavior
{
    protected abstract SingleEntityBehaviorSet<Item, TItemBehavior> BehaviorSet { get; }

    public virtual bool TryGetBehavior(Item item, out TItemBehavior itemBehavior, [CallerMemberName] string methodName = null!) => BehaviorSet.TryGetBehavior(item, methodName, out itemBehavior);

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (TItemBehavior itemBehavior in BehaviorSet)
            itemBehavior.SetStaticDefaults();
    }

    public override void SetDefaults(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.SetDefaults();
    }

    public override void AddRecipes()
    {
        foreach (TItemBehavior itemBehavior in BehaviorSet)
            itemBehavior.AddRecipes();
    }
    #endregion Defaults

    #region Lifetime
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnCreated(context);
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnSpawn(source);
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.Update(ref gravity, ref maxFallSpeed);
    }

    public override void PostUpdate(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostUpdate();
    }

    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.GrabRange(player, ref grabRange);
    }

    public override bool GrabStyle(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.GrabStyle(player))
                return false;
        }
        return false;
    }

    public override bool CanPickup(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanPickup(player))
                return false;
        }
        return true;
    }

    public override bool OnPickup(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.OnPickup(player))
                return false;
        }
        return true;
    }

    public override bool ItemSpace(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.ItemSpace(player))
                return false;
        }
        return false;
    }
    #endregion Lifetime

    #region Update
    public override void UpdateInventory(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateInventory(player);
    }

    public override void UpdateInfoAccessory(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateInfoAccessory(player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateEquip(player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateAccessory(player, hideVisual);
    }

    public override void UpdateVanity(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateVanity(player);
    }

    public override void UpdateVisibleAccessory(Item item, Player player, bool hideVisual)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateVisibleAccessory(player, hideVisual);
    }

    public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UpdateItemDye(player, dye, hideVisual);
    }
    #endregion Update

    #region Draw
    public override Color? GetAlpha(Item item, Color lightColor)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            Color? result = itemBehavior.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI))
                return false;
        }
        return true;
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
                return false;
        }
        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
    #endregion Draw

    #region Prefix
    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            int result = itemBehavior.ChoosePrefix(rand);
            if (result != -1)
                return result;
        }
        return -1;
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.PrefixChance(pre, rand);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool AllowPrefix(Item item, int pre)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.AllowPrefix(pre))
                return false;
        }
        return true;
    }

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.ReforgePrice(ref reforgePrice, ref canApplyDiscount))
                return false;
        }
        return true;
    }

    public override bool CanReforge(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanReforge())
                return false;
        }
        return true;
    }

    public override void PreReforge(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PreReforge();
    }

    public override void PostReforge(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostReforge();
    }
    #endregion Prefix

    #region Use
    public override bool AltFunctionUse(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (itemBehavior.AltFunctionUse(player))
                return true;
        }
        return false;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanUseItem(player))
                return false;
        }
        return true;
    }

    public override bool? CanAutoReuseItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanAutoReuseItem(player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UseStyle(player, heldItemFrame);
    }

    public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.HoldStyle(player, heldItemFrame);
    }

    public override void HoldItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.HoldItem(player);
    }

    public override float UseTimeMultiplier(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseTimeMultiplier(player);
            if (result != 1f)
                return result;
        }
        return 1f;
    }

    public override float UseAnimationMultiplier(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseAnimationMultiplier(player);
            if (result != 1f)
                return result;
        }
        return 1f;
    }

    public override float UseSpeedMultiplier(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseSpeedMultiplier(player);
            if (result != 1f)
                return result;
        }
        return 1f;
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.UseItem(player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseAnimation(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UseAnimation(player);
    }

    public override bool CanShoot(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanShoot(player))
                return false;
        }
        return true;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.Shoot(player, source, position, velocity, type, damage, knockback))
                return false;
        }
        return true;
    }

    public override bool CanRightClick(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanRightClick())
                return false;
        }
        return false;
    }

    public override void RightClick(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.RightClick(player);
    }
    #endregion Use

    #region ModifyStats
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponDamage(player, ref damage);
    }

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponKnockback(player, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponCrit(player, ref crit);
    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override void ModifyItemScale(Item item, Player player, ref float scale)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyItemScale(player, ref scale);
    }
    #endregion ModifyStats

    #region Hit
    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanHitNPC(player, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyHitNPC(player, target, ref modifiers);
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnHitNPC(player, target, hit, damageDone);
    }

    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool result = itemBehavior.CanHitPvp(player, target);
            if (!result)
                return false;
        }
        return true;
    }

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyHitPvp(player, target, ref modifiers);
    }

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnHitPvp(player, target, hurtInfo);
    }
    #endregion Hit

    #region SpecialEffects
    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.GetHealLife(player, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.GetHealMana(player, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyManaCost(player, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, Player player, int neededMana)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnMissingMana(player, neededMana);
    }

    public override void OnConsumeMana(Item item, Player player, int manaConsumed)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnConsumeMana(player, manaConsumed);
    }

    public override bool CanResearch(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanResearch())
                return false;
        }
        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnResearched(fullyResearched);
    }

    public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyResearchSorting(ref itemGroup);
    }

    public override bool NeedsAmmo(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.NeedsAmmo(player))
                return false;
        }
        return true;
    }

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UseItemHitbox(player, ref hitbox, ref noHitbox);
    }

    public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.MeleeEffects(player, hitbox);
    }

    public override bool? CanCatchNPC(Item item, NPC target, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanCatchNPC(target, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnCatchNPC(npc, player, failed);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.ConsumeItem(player))
                return false;
        }
        return true;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.OnConsumeItem(player);
    }

    public override void UseItemFrame(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.UseItemFrame(player);
    }

    public override void HoldItemFrame(Item item, Player player)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.HoldItemFrame(player);
    }

    public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
                ref maxAscentMultiplier, ref constantAscend);
    }

    public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.HorizontalWingSpeeds(player, ref speed, ref acceleration);
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanEquipAccessory(player, slot, modded))
                return false;
        }
        return true;
    }
    #endregion SpecialEffects

    #region Tooltip
    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawTooltip(lines, ref x, ref y))
                return false;
        }
        return true;
    }

    public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostDrawTooltip(lines);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawTooltipLine(line, ref yOffset))
                return false;
        }
        return true;
    }

    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.PostDrawTooltipLine(line);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.ModifyTooltips(tooltips);
    }
    #endregion Tooltip

    #region WorldSaving
    public override void SaveData(Item item, TagCompound tag)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.SaveData(tag);
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.LoadData(tag);
    }
    #endregion WorldSaving

    #region Net
    public override void NetSend(Item item, BinaryWriter writer)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.NetSend(writer);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.NetReceive(reader);
    }
    #endregion Net
}
#endregion Single Behavior Handler

#region General Behavior Handler
public sealed class PlayerBehaviorHandler : ModPlayer, IResourceLoader
{
    public static GeneralEntityBehaviorSet<Player, PlayerBehavior> BehaviorSet { get; } = new();

    public override void SetStaticDefaults()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.SetStaticDefaults();
    }

    public override void Initialize()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.Initialize();
    }

    public override void ResetEffects()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ResetEffects();
    }

    public override void ResetInfoAccessories()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ResetInfoAccessories();
    }

    public override void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.RefreshInfoAccessoriesFromTeamPlayers(otherPlayer);
    }

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        mana = StatModifier.Default;

        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            behavior.ModifyMaxStats(out StatModifier newHealth, out StatModifier newMana);
            health = health.CombineWith(newHealth);
            mana = mana.CombineWith(newMana);
        }
    }

    public override void UpdateDead()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.UpdateDead();
    }

    public override void PreSaveCustomData()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PreSaveCustomData();
    }

    public override void SaveData(TagCompound tag)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.LoadData(tag);
    }

    public override void PreSavePlayer()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PreSavePlayer();
    }

    public override void PostSavePlayer()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostSavePlayer();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.CopyClientState(targetCopy);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.SyncPlayer(toWho, fromWho, newPlayer);
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.SendClientChanges(clientPlayer);
    }

    public override void UpdateBadLifeRegen()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.UpdateBadLifeRegen();
    }

    public override void UpdateLifeRegen()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.UpdateLifeRegen();
    }

    public override void NaturalLifeRegen(ref float regen)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.NaturalLifeRegen(ref regen);
    }

    public override void UpdateAutopause()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.UpdateAutopause();
    }

    public override void PreUpdate()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PreUpdate();
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ProcessTriggers(triggersSet);
    }

    public override void ArmorSetBonusActivated()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ArmorSetBonusActivated();
    }

    public override void ArmorSetBonusHeld(int holdTime)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ArmorSetBonusHeld(holdTime);
    }

    public override void SetControls()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.SetControls();
    }

    public override void PreUpdateBuffs()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PreUpdateBuffs();
    }

    public override void PostUpdateBuffs()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostUpdateBuffs();
    }

    public override void UpdateEquips()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.UpdateEquips();
    }

    public override void PostUpdateEquips()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostUpdateEquips();
    }

    public override void UpdateVisibleAccessories()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.UpdateVisibleAccessories();
    }

    public override void UpdateVisibleVanityAccessories()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.UpdateVisibleVanityAccessories();
    }

    public override void UpdateDyes()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.UpdateDyes();
    }

    public override void PostUpdateMiscEffects()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostUpdateMiscEffects();
    }

    public override void PostUpdateRunSpeeds()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostUpdateRunSpeeds();
    }

    public override void PreUpdateMovement()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PreUpdateMovement();
    }

    public override void PostUpdate()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostUpdate();
    }

    public override void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyExtraJumpDurationMultiplier(jump, ref duration);
    }

    public override bool CanStartExtraJump(ExtraJump jump)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanStartExtraJump(jump);
        return result;
    }

    public override void OnExtraJumpStarted(ExtraJump jump, ref bool playSound)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnExtraJumpStarted(jump, ref playSound);
    }

    public override void OnExtraJumpEnded(ExtraJump jump)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnExtraJumpEnded(jump);
    }

    public override void OnExtraJumpRefreshed(ExtraJump jump)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnExtraJumpRefreshed(jump);
    }

    public override void ExtraJumpVisuals(ExtraJump jump)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ExtraJumpVisuals(jump);
    }

    public override bool CanShowExtraJumpVisuals(ExtraJump jump)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanShowExtraJumpVisuals(jump);
        return result;
    }

    public override void OnExtraJumpCleared(ExtraJump jump)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnExtraJumpCleared(jump);
    }

    public override void FrameEffects()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.FrameEffects();
    }

    public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result |= behavior.ImmuneTo(damageSource, cooldownCounter, dodgeable);
        return result;
    }

    public override bool FreeDodge(Player.HurtInfo info)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result |= behavior.FreeDodge(info);
        return result;
    }

    public override bool ConsumableDodge(Player.HurtInfo info)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result |= behavior.ConsumableDodge(info);
        return result;
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyHurt(ref modifiers);
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnHurt(info);
    }

    public override void PostHurt(Player.HurtInfo info)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostHurt(info);
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
        return result;
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.Kill(damage, hitDirection, pvp, damageSource);
    }

    public override bool PreModifyLuck(ref float luck)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.PreModifyLuck(ref luck);
        return result;
    }

    public override void ModifyLuck(ref float luck)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyLuck(ref luck);
    }

    public override bool PreItemCheck()
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.PreItemCheck();
        return result;
    }

    public override void PostItemCheck()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostItemCheck();
    }

    public override float UseTimeMultiplier(Item item)
    {
        float result = 1f;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result *= behavior.UseTimeMultiplier(item);
        return result;
    }

    public override float UseAnimationMultiplier(Item item)
    {
        float result = 1f;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result *= behavior.UseAnimationMultiplier(item);
        return result;
    }

    public override float UseSpeedMultiplier(Item item)
    {
        float result = 1f;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result *= behavior.UseSpeedMultiplier(item);
        return result;
    }

    public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.GetHealLife(item, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.GetHealMana(item, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyManaCost(item, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, int neededMana)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnMissingMana(item, neededMana);
    }

    public override void OnConsumeMana(Item item, int manaConsumed)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnConsumeMana(item, manaConsumed);
    }

    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyWeaponDamage(item, ref damage);
    }

    public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyWeaponKnockback(item, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, ref float crit)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyWeaponCrit(item, ref crit);
    }

    public override bool CanConsumeAmmo(Item weapon, Item ammo)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanConsumeAmmo(weapon, ammo);
        return result;
    }

    public override void OnConsumeAmmo(Item weapon, Item ammo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnConsumeAmmo(weapon, ammo);
    }

    public override bool CanShoot(Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanShoot(item);
        return result;
    }

    public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.Shoot(item, source, position, velocity, type, damage, knockback);
        return result;
    }

    public override void MeleeEffects(Item item, Rectangle hitbox)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.MeleeEffects(item, hitbox);
    }

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
    }

    public override bool? CanCatchNPC(NPC target, Item item)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanCatchNPC(target, item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCatchNPC(NPC npc, Item item, bool failed)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnCatchNPC(npc, item, failed);
    }

    public override void ModifyItemScale(Item item, ref float scale)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyItemScale(item, ref scale);
    }

    public override void OnHitAnything(float x, float y, Entity victim)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnHitAnything(x, y, victim);
    }

    public override bool CanHitNPC(NPC target)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanHitNPC(target);
        return result;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnHitNPC(target, hit, damageDone);
    }

    public override bool? CanHitNPCWithItem(Item item, NPC target)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanHitNPCWithItem(item, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyHitNPCWithItem(item, target, ref modifiers);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnHitNPCWithItem(item, target, hit, damageDone);
    }

    public override bool? CanHitNPCWithProj(Projectile proj, NPC target)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanHitNPCWithProj(proj, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyHitNPCWithProj(proj, target, ref modifiers);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnHitNPCWithProj(proj, target, hit, damageDone);
    }

    public override bool CanHitPvp(Item item, Player target)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanHitPvp(item, target);
        return result;
    }

    public override bool CanHitPvpWithProj(Projectile proj, Player target)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanHitPvpWithProj(proj, target);
        return result;
    }

    public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanBeHitByNPC(npc, ref cooldownSlot);
        return result;
    }

    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyHitByNPC(npc, ref modifiers);
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnHitByNPC(npc, hurtInfo);
    }

    public override bool CanBeHitByProjectile(Projectile proj)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanBeHitByProjectile(proj);
        return result;
    }

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyHitByProjectile(proj, ref modifiers);
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnHitByProjectile(proj, hurtInfo);
    }

    public override void ModifyFishingAttempt(ref FishingAttempt attempt)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyFishingAttempt(ref attempt);
    }

    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.CatchFish(attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);
    }

    public override void ModifyCaughtFish(Item fish)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyCaughtFish(fish);
    }

    public override bool? CanConsumeBait(Item bait)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanConsumeBait(bait);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.GetFishingLevel(fishingRod, bait, ref fishingLevel);
    }

    public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.AnglerQuestReward(rareMultiplier, rewardItems);
    }

    public override void GetDyeTraderReward(List<int> rewardPool)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.GetDyeTraderReward(rewardPool);
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyDrawInfo(ref drawInfo);
    }

    public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyDrawLayerOrdering(positions);
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.HideDrawLayers(drawInfo);
    }

    public override void ModifyScreenPosition()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyScreenPosition();
    }

    public override void ModifyZoom(ref float zoom)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyZoom(ref zoom);
    }

    public override void PlayerConnect()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PlayerConnect();
    }

    public override void PlayerDisconnect()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PlayerDisconnect();
    }

    public override void OnEnterWorld()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnEnterWorld();
    }

    public override void OnRespawn()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnRespawn();
    }

    public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result |= behavior.ShiftClickSlot(inventory, context, slot);
        return result;
    }

    public override bool HoverSlot(Item[] inventory, int context, int slot)
    {
        bool result = false;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result |= behavior.HoverSlot(inventory, context, slot);
        return result;
    }

    public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostSellItem(vendor, shopInventory, item);
    }

    public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanSellItem(vendor, shopInventory, item);
        return result;
    }

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostBuyItem(vendor, shopInventory, item);
    }

    public override bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanBuyItem(vendor, shopInventory, item);
        return result;
    }

    public override bool CanUseItem(Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanUseItem(item);
        return result;
    }

    public override bool? CanAutoReuseItem(Item item)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanAutoReuseItem(item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.ModifyNurseHeal(nurse, ref health, ref removeDebuffs, ref chatText);
        return result;
    }

    public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyNursePrice(nurse, health, removeDebuffs, ref price);
    }

    public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.PostNurseHeal(nurse, health, removeDebuffs, price);
    }

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            IEnumerable<Item> items = behavior.AddStartingItems(mediumCoreDeath);
            if (items is not null)
                allItems.AddRange(items);
        }
        return allItems;
    }

    public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.ModifyStartingInventory(itemsByMod, mediumCoreDeath);
    }

    public override IEnumerable<Item> AddMaterialsForCrafting(out ModPlayer.ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            IEnumerable<Item> items = behavior.AddMaterialsForCrafting(out ModPlayer.ItemConsumedCallback callback);
            if (items is not null)
                allItems.AddRange(items);
            itemConsumedCallback += callback;
        }
        return allItems;
    }

    public override bool OnPickup(Item item)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.OnPickup(item);
        return result;
    }

    public override bool CanBeTeleportedTo(Vector2 teleportPosition, string context)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanBeTeleportedTo(teleportPosition, context);
        return result;
    }

    public override void OnEquipmentLoadoutSwitched(int oldLoadoutIndex, int loadoutIndex)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            behavior.OnEquipmentLoadoutSwitched(oldLoadoutIndex, loadoutIndex);
    }

    void IResourceLoader.PostSetupContent() => BehaviorSet.FillSet();

    void IResourceLoader.OnModUnload() => BehaviorSet.Clear();
}

public sealed class GlobalNPCBehaviorHandler : GlobalNPC, IResourceLoader
{
    public override bool InstancePerEntity => true;

    public static GlobalEntityBehaviorSet<NPC, GlobalNPCBehavior> BehaviorSet { get; } = new();

    public override void SetStaticDefaults()
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetStaticDefaults();
    }

    public override void SetDefaults(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetDefaults(npc);
    }

    public override void SetDefaultsFromNetId(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetDefaultsFromNetId(npc);
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnSpawn(npc, source);
    }

    public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ApplyDifficultyAndPlayerScaling(npc, numPlayers, balance, bossAdjustment);
    }

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetBestiary(npc, database, bestiaryEntry);
    }

    public override void ModifyTypeName(NPC npc, ref string typeName)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyTypeName(npc, ref typeName);
    }

    public override void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHoverBoundingBox(npc, ref boundingBox);
    }

    public override ITownNPCProfile ModifyTownNPCProfile(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            ITownNPCProfile temp = behavior.ModifyTownNPCProfile(npc);
            if (temp is not null)
                return temp;
        }
        return null;
    }

    public override void ModifyNPCNameList(NPC npc, List<string> nameList)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyNPCNameList(npc, nameList);
    }

    public override void ResetEffects(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ResetEffects(npc);
    }

    public override bool PreAI(NPC npc)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreAI(npc);
        return result;
    }

    public override void AI(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.AI(npc);
    }

    public override void PostAI(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostAI(npc);
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SendExtraAI(npc, bitWriter, binaryWriter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ReceiveExtraAI(npc, bitReader, binaryReader);
    }

    public override void FindFrame(NPC npc, int frameHeight)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.FindFrame(npc, frameHeight);
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.HitEffect(npc, hit);
    }

    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateLifeRegen(npc, ref damage);
    }

    public override bool CheckActive(NPC npc)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CheckActive(npc);
        return result;
    }

    public override bool CheckDead(NPC npc)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CheckDead(npc);
        return result;
    }

    public override bool SpecialOnKill(NPC npc)
    {
        bool result = false;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.SpecialOnKill(npc);
        return result;
    }

    public override bool PreKill(NPC npc)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreKill(npc);
        return result;
    }

    public override void OnKill(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnKill(npc);
    }

    public override bool? CanFallThroughPlatforms(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanFallThroughPlatforms(npc);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanBeCaughtBy(NPC npc, Item item, Player player)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanBeCaughtBy(npc, item, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnCaughtBy(npc, player, item, failed);
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyNPCLoot(npc, npcLoot);
    }

    public override void ModifyGlobalLoot(GlobalLoot globalLoot)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyGlobalLoot(globalLoot);
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPlayer(npc, target, ref cooldownSlot);
        return result;
    }

    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHitPlayer(npc, target, ref modifiers);
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnHitPlayer(npc, target, hurtInfo);
    }

    public override bool CanHitNPC(NPC npc, NPC target)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitNPC(npc, target);
        return result;
    }

    public override bool CanBeHitByNPC(NPC npc, NPC attacker)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanBeHitByNPC(npc, attacker);
        return result;
    }

    public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHitNPC(npc, target, ref modifiers);
    }

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnHitNPC(npc, target, hit);
    }

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanBeHitByItem(npc, player, item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanCollideWithPlayerMeleeAttack(npc, player, item, meleeAttackHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHitByItem(npc, player, item, ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnHitByItem(npc, player, item, hit, damageDone);
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanBeHitByProjectile(npc, projectile);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHitByProjectile(npc, projectile, ref modifiers);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnHitByProjectile(npc, projectile, hit, damageDone);
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyIncomingHit(npc, ref modifiers);
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.BossHeadSlot(npc, ref index);
    }

    public override void BossHeadRotation(NPC npc, ref float rotation)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.BossHeadRotation(npc, ref rotation);
    }

    public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.BossHeadSpriteEffects(npc, ref spriteEffects);
    }

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            Color? result = behavior.GetAlpha(npc, drawColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.DrawEffects(npc, ref drawColor);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreDraw(npc, spriteBatch, screenPos, drawColor);
        return result;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostDraw(npc, spriteBatch, screenPos, drawColor);
    }

    public override void DrawBehind(NPC npc, int index)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.DrawBehind(npc, index);
    }

    public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.DrawHealthBar(npc, hbPosition, ref scale, ref position);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
    }

    public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.EditSpawnRange(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.EditSpawnPool(pool, spawnInfo);
    }

    public override void SpawnNPC(int npc, int tileX, int tileY)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SpawnNPC(npc, tileX, tileY);
    }

    public override bool? CanChat(NPC npc)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanChat(npc);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.GetChat(npc, ref chat);
    }

    public override bool PreChatButtonClicked(NPC npc, bool firstButton)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreChatButtonClicked(npc, firstButton);
        return result;
    }

    public override void OnChatButtonClicked(NPC npc, bool firstButton)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnChatButtonClicked(npc, firstButton);
    }

    public override void ModifyShop(NPCShop shop)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyShop(shop);
    }

    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyActiveShop(npc, shopName, items);
    }

    public override void SetupTravelShop(int[] shop, ref int nextSlot)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetupTravelShop(shop, ref nextSlot);
    }

    public override bool? CanGoToStatue(NPC npc, bool toKingStatue)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanGoToStatue(npc, toKingStatue);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnGoToStatue(NPC npc, bool toKingStatue)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnGoToStatue(npc, toKingStatue);
    }

    public override void BuffTownNPC(ref float damageMult, ref int defense)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.BuffTownNPC(ref damageMult, ref defense);
    }

    public override bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.ModifyDeathMessage(npc, ref customText, ref color);
        return result;
    }

    public override void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.TownNPCAttackStrength(npc, ref damage, ref knockback);
    }

    public override void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.TownNPCAttackCooldown(npc, ref cooldown, ref randExtraCooldown);
    }

    public override void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.TownNPCAttackProj(npc, ref projType, ref attackDelay);
    }

    public override void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.TownNPCAttackProjSpeed(npc, ref multiplier, ref gravityCorrection, ref randomOffset);
    }

    public override void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.TownNPCAttackShoot(npc, ref inBetweenShots);
    }

    public override void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.TownNPCAttackMagic(npc, ref auraLightMultiplier);
    }

    public override void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.TownNPCAttackSwing(npc, ref itemWidth, ref itemHeight);
    }

    public override void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.DrawTownAttackGun(npc, ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
    }

    public override void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.DrawTownAttackSwing(npc, ref item, ref itemFrame, ref itemSize, ref scale, ref offset);
    }

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.ModifyCollisionData(npc, victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
        return result;
    }

    public override bool NeedSaving(NPC npc)
    {
        bool result = false;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.NeedSaving(npc);
        return result;
    }

    public override void SaveData(NPC npc, TagCompound tag)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SaveData(npc, tag);
    }

    public override void LoadData(NPC npc, TagCompound tag)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.LoadData(npc, tag);
    }

    public override int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            int? result = behavior.PickEmote(npc, closestPlayer, emoteList, otherAnchor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ChatBubblePosition(npc, ref position, ref spriteEffects);
    }

    public override void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PartyHatPosition(npc, ref position, ref spriteEffects);
    }

    public override void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.EmoteBubblePosition(npc, ref position, ref spriteEffects);
    }

    void IResourceLoader.PostSetupContent() => BehaviorSet.FillSet();

    void IResourceLoader.OnModUnload() => BehaviorSet.Clear();
}

public sealed class GlobalProjectileBehaviorHandler : GlobalProjectile, IResourceLoader
{
    public override bool InstancePerEntity => true;

    public static GlobalEntityBehaviorSet<Projectile, GlobalProjectileBehavior> BehaviorSet { get; } = new();

    public override void SetStaticDefaults()
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetStaticDefaults();
    }

    public override void SetDefaults(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetDefaults(projectile);
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnSpawn(projectile, source);
    }

    public override bool PreAI(Projectile projectile)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreAI(projectile);
        return result;
    }

    public override void AI(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.AI(projectile);
    }

    public override void PostAI(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostAI(projectile);
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SendExtraAI(projectile, bitWriter, binaryWriter);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ReceiveExtraAI(projectile, bitReader, binaryReader);
    }

    public override bool ShouldUpdatePosition(Projectile projectile)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.ShouldUpdatePosition(projectile);
        return result;
    }

    public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.TileCollideStyle(projectile, ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        return result;
    }

    public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.OnTileCollide(projectile, oldVelocity);
        return result;
    }

    public override bool PreKill(Projectile projectile, int timeLeft)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreKill(projectile, timeLeft);
        return result;
    }

    public override void OnKill(Projectile projectile, int timeLeft)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnKill(projectile, timeLeft);
    }

    public override bool? CanCutTiles(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanCutTiles(projectile);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void CutTiles(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.CutTiles(projectile);
    }

    public override bool? CanDamage(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanDamage(projectile);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool MinionContactDamage(Projectile projectile)
    {
        bool result = false;  // 默认值为false，使用或运算
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.MinionContactDamage(projectile);
        return result;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyDamageHitbox(projectile, ref hitbox);
    }

    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanHitNPC(projectile, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHitNPC(projectile, target, ref modifiers);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnHitNPC(projectile, target, hit, damageDone);
    }

    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPvp(projectile, target);
        return result;
    }

    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPlayer(projectile, target);
        return result;
    }

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHitPlayer(projectile, target, ref modifiers);
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnHitPlayer(projectile, target, info);
    }

    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.Colliding(projectile, projHitbox, targetHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override Color? GetAlpha(Projectile projectile, Color lightColor)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            Color? result = behavior.GetAlpha(projectile, lightColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreDrawExtras(projectile);
        return result;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        bool result = true;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreDraw(projectile, ref lightColor);
        return result;
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostDraw(projectile, lightColor);
    }

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.DrawBehind(projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }

    public override bool? CanUseGrapple(int type, Player player)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanUseGrapple(type, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseGrapple(Player player, ref int type)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UseGrapple(player, ref type);
    }

    public override void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.NumGrappleHooks(projectile, player, ref numHooks);
    }

    public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.GrappleRetreatSpeed(projectile, player, ref speed);
    }

    public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.GrapplePullSpeed(projectile, player, ref speed);
    }

    public override void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.GrappleTargetPoint(projectile, player, ref grappleX, ref grappleY);
    }

    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.GrappleCanLatchOnTo(projectile, player, x, y);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void PrepareBombToBlow(Projectile projectile)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PrepareBombToBlow(projectile);
    }

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
    }

    void IResourceLoader.PostSetupContent() => BehaviorSet.FillSet();

    void IResourceLoader.OnModUnload() => BehaviorSet.Clear();
}

public sealed class GlobalItemBehaviorHandler : GlobalItem, IResourceLoader
{
    public override bool InstancePerEntity => true;

    public static GlobalEntityBehaviorSet<Item, GlobalItemBehavior> BehaviorSet { get; } = new();

    public override void SetStaticDefaults()
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetStaticDefaults();
    }

    public override void SetDefaults(Item item)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetDefaults(item);
    }

    public override void OnCreated(Item item, ItemCreationContext context)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnCreated(item, context);
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnSpawn(item, source);
    }

    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            int result = behavior.ChoosePrefix(item, rand);
            if (result != -1)
                return result;
        }
        return -1;
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.PrefixChance(item, pre, rand);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool AllowPrefix(Item item, int pre)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.AllowPrefix(item, pre);
        return result;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanUseItem(item, player);
        return result;
    }

    public override bool? CanAutoReuseItem(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanAutoReuseItem(item, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UseStyle(item, player, heldItemFrame);
    }

    public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.HoldStyle(item, player, heldItemFrame);
    }

    public override void HoldItem(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.HoldItem(item, player);
    }

    public override float UseTimeMultiplier(Item item, Player player)
    {
        float result = 1f;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result *= behavior.UseTimeMultiplier(item, player);
        return result;
    }

    public override float UseAnimationMultiplier(Item item, Player player)
    {
        float result = 1f;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result *= behavior.UseAnimationMultiplier(item, player);
        return result;
    }

    public override float UseSpeedMultiplier(Item item, Player player)
    {
        float result = 1f;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result *= behavior.UseSpeedMultiplier(item, player);
        return result;
    }

    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.GetHealLife(item, player, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.GetHealMana(item, player, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyManaCost(item, player, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, Player player, int neededMana)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnMissingMana(item, player, neededMana);
    }

    public override void OnConsumeMana(Item item, Player player, int manaConsumed)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnConsumeMana(item, player, manaConsumed);
    }

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyWeaponDamage(item, player, ref damage);
    }

    public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyResearchSorting(item, ref itemGroup);
    }

    public override bool? CanConsumeBait(Player player, Item bait)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanConsumeBait(player, bait);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool CanResearch(Item item)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanResearch(item);
        return result;
    }

    public override void OnResearched(Item item, bool fullyResearched)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnResearched(item, fullyResearched);
    }

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyWeaponKnockback(item, player, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyWeaponCrit(item, player, ref crit);
    }

    public override bool NeedsAmmo(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.NeedsAmmo(item, player);
        return result;
    }

    public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);
    }

    public override bool? CanChooseAmmo(Item weapon, Item ammo, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanChooseAmmo(weapon, ammo, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanBeChosenAsAmmo(ammo, weapon, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanConsumeAmmo(weapon, ammo, player);
        return result;
    }

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanBeConsumedAsAmmo(ammo, weapon, player);
        return result;
    }

    public override void OnConsumeAmmo(Item weapon, Item ammo, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnConsumeAmmo(weapon, ammo, player);
    }

    public override void OnConsumedAsAmmo(Item ammo, Item weapon, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnConsumedAsAmmo(ammo, weapon, player);
    }

    public override bool CanShoot(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanShoot(item, player);
        return result;
    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.Shoot(item, player, source, position, velocity, type, damage, knockback);
        return result;
    }

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
    }

    public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.MeleeEffects(item, player, hitbox);
    }

    public override bool? CanCatchNPC(Item item, NPC target, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanCatchNPC(item, target, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnCatchNPC(item, npc, player, failed);
    }

    public override void ModifyItemScale(Item item, Player player, ref float scale)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyItemScale(item, player, ref scale);
    }

    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanHitNPC(item, player, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, player, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHitNPC(item, player, target, ref modifiers);
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnHitNPC(item, player, target, hit, damageDone);
    }

    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPvp(item, player, target);
        return result;
    }

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyHitPvp(item, player, target, ref modifiers);
    }

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnHitPvp(item, player, target, hurtInfo);
    }

    public override bool? UseItem(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.UseItem(item, player);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void UseAnimation(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UseAnimation(item, player);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.ConsumeItem(item, player);
        return result;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnConsumeItem(item, player);
    }

    public override void UseItemFrame(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UseItemFrame(item, player);
    }

    public override void HoldItemFrame(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.HoldItemFrame(item, player);
    }

    public override bool AltFunctionUse(Item item, Player player)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.AltFunctionUse(item, player);
        return result;
    }

    public override void UpdateInventory(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateInventory(item, player);
    }

    public override void UpdateInfoAccessory(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateInfoAccessory(item, player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateEquip(item, player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateAccessory(item, player, hideVisual);
    }

    public override void UpdateVanity(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateVanity(item, player);
    }

    public override void UpdateVisibleAccessory(Item item, Player player, bool hideVisual)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateVisibleAccessory(item, player, hideVisual);
    }

    public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateItemDye(item, player, dye, hideVisual);
    }

    public override string IsArmorSet(Item head, Item body, Item legs)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            string result = behavior.IsArmorSet(head, body, legs);
            if (result != "")
                return result;
        }
        return "";
    }

    public override void UpdateArmorSet(Player player, string set)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateArmorSet(player, set);
    }

    public override string IsVanitySet(int head, int body, int legs)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            string result = behavior.IsVanitySet(head, body, legs);
            if (result != "")
                return result;
        }
        return "";
    }

    public override void PreUpdateVanitySet(Player player, string set)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PreUpdateVanitySet(player, set);
    }

    public override void UpdateVanitySet(Player player, string set)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.UpdateVanitySet(player, set);
    }

    public override void ArmorSetShadows(Player player, string set)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ArmorSetShadows(player, set);
    }

    public override void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SetMatch(armorSlot, type, male, ref equipSlot, ref robes);
    }

    public override bool CanRightClick(Item item)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.CanRightClick(item);
        return result;
    }

    public override void RightClick(Item item, Player player)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.RightClick(item, player);
    }

    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyItemLoot(item, itemLoot);
    }

    public override bool CanStack(Item destination, Item source)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanStack(destination, source);
        return result;
    }

    public override bool CanStackInWorld(Item destination, Item source)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanStackInWorld(destination, source);
        return result;
    }

    public override void OnStack(Item destination, Item source, int numToTransfer)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.OnStack(destination, source, numToTransfer);
    }

    public override void SplitStack(Item destination, Item source, int numToTransfer)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SplitStack(destination, source, numToTransfer);
    }

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
        return result;
    }

    public override bool CanReforge(Item item)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanReforge(item);
        return result;
    }

    public override void PreReforge(Item item)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PreReforge(item);
    }

    public override void PostReforge(Item item)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostReforge(item);
    }

    public override void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.DrawArmorColor(type, slot, drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
    }

    public override void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ArmorArmGlowMask(slot, drawPlayer, shadow, ref glowMask, ref color);
    }

    public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
    }

    public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.HorizontalWingSpeeds(item, player, ref speed, ref acceleration);
    }

    public override bool WingUpdate(int wings, Player player, bool inUse)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.WingUpdate(wings, player, inUse);
        return result;
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.Update(item, ref gravity, ref maxFallSpeed);
    }

    public override void PostUpdate(Item item)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostUpdate(item);
    }

    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.GrabRange(item, player, ref grabRange);
    }

    public override bool GrabStyle(Item item, Player player)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.GrabStyle(item, player);
        return result;
    }

    public override bool CanPickup(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanPickup(item, player);
        return result;
    }

    public override bool OnPickup(Item item, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.OnPickup(item, player);
        return result;
    }

    public override bool ItemSpace(Item item, Player player)
    {
        bool result = false;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.ItemSpace(item, player);
        return result;
    }

    public override Color? GetAlpha(Item item, Color lightColor)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            Color? result = behavior.GetAlpha(item, lightColor);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        return result;
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        return result;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override Vector2? HoldoutOffset(int type)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            Vector2? result = behavior.HoldoutOffset(type);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override Vector2? HoldoutOrigin(int type)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
        {
            Vector2? result = behavior.HoldoutOrigin(type);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanEquipAccessory(item, player, slot, modded);
        return result;
    }

    public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
        return result;
    }

    public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ExtractinatorUse(extractType, extractinatorBlockType, ref resultType, ref resultStack);
    }

    public override void CaughtFishStack(int type, ref int stack)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.CaughtFishStack(type, ref stack);
    }

    public override bool IsAnglerQuestAvailable(int type)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.IsAnglerQuestAvailable(type);
        return result;
    }

    public override void AnglerChat(int type, ref string chat, ref string catchLocation)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.AnglerChat(type, ref chat, ref catchLocation);
    }

    public override void AddRecipes()
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.AddRecipes();
    }

    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreDrawTooltip(item, lines, ref x, ref y);
        return result;
    }

    public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostDrawTooltip(item, lines);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.PreDrawTooltipLine(item, line, ref yOffset);
        return result;
    }

    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.PostDrawTooltipLine(item, line);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyTooltips(item, tooltips);
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.SaveData(item, tag);
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.LoadData(item, tag);
    }

    public override void NetSend(Item item, BinaryWriter writer)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.NetSend(item, writer);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.NetReceive(item, reader);
    }

    void IResourceLoader.PostSetupContent() => BehaviorSet.FillSet();

    void IResourceLoader.OnModUnload() => BehaviorSet.Clear();
}
#endregion General Behavior Handler