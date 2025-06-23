using System.Collections;
using MonoMod.Utils;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.GameInput;

namespace Transoceanic.Data;

#region Core
public abstract class EntityBehaviorBase<TEntity> where TEntity : Entity
{
    /// <summary>
    /// 优先级，越大越先应用。
    /// </summary>
    public virtual decimal Priority { get; } = 0m;

    public abstract void Connect(TEntity entity);

    public virtual void SetStaticDefaults() { }
}

public abstract class NontypedEntityBehavior<TEntity> : EntityBehaviorBase<TEntity> where TEntity : Entity { }

public abstract class TypedEntityBehavior<TEntity> : EntityBehaviorBase<TEntity> where TEntity : Entity
{
    public abstract int ApplyingType { get; }

    public virtual bool ShouldProcess => true;
}

public abstract class EntityBehaviorSetBase<TEntity, TBehavior> : IEnumerable<TBehavior>
    where TEntity : Entity
    where TBehavior : EntityBehaviorBase<TEntity>
{
    public abstract void FillSet(Assembly assemblyToSearch);

    public abstract IEnumerator<TBehavior> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public abstract void Clear();
}

public class NontypedEntityBehaviorSet<TEntity, TBehavior> : EntityBehaviorSetBase<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : NontypedEntityBehavior<TEntity>
{
    private readonly List<TBehavior> _data = [];

    public override void FillSet(Assembly assemblyToSearch)
    {
        _data.Clear();
        _data.AddRange(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch).OrderByDescending(k => k.Priority));
    }

    public override IEnumerator<TBehavior> GetEnumerator() => _data.GetEnumerator();

    /// <summary>
    /// 通过指定实体获取Behavior集合。
    /// </summary>
    /// <remarks>每个Behavior示例返回前均与指定实体相连接。</remarks>
    /// <param name="entity">待连接实体。</param>
    public IEnumerable<TBehavior> this[TEntity entity]
    {
        get
        {
            foreach (TBehavior behavior in this)
            {
                behavior.Connect(entity);
                yield return behavior;
            }
        }
    }

    public override void Clear() => _data.Clear();
}

public class TypedEntityBehaviorSet<TEntity, TBehavior> : EntityBehaviorSetBase<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : TypedEntityBehavior<TEntity>
{
    private readonly Dictionary<int, List<(TBehavior behaviorInstance, HashSet<string> behaviorMethods)>> _data = [];

    public override IEnumerator<TBehavior> GetEnumerator()
    {
        foreach (List<(TBehavior behaviorInstance, HashSet<string> behaviorMethods)> behaviors in _data.Values)
        {
            foreach ((TBehavior behaviorInstance, HashSet<string> _) in behaviors)
                yield return behaviorInstance;
        }
    }

    /// <summary>
    /// 尝试获取指定实体的行为实例。
    /// <br/>按照 <see cref="EntityBehaviorBase{TEntity}.Priority"/> 降序寻找通过 <see cref="TypedEntityBehavior{TEntity}.ShouldProcess"/> 检测的实现了指定方法的Override实例。
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="methodName"></param>
    /// <param name="behaviorInstance"></param>
    /// <returns></returns>
    public bool TryGetBehavior(TEntity entity, string methodName, [NotNullWhen(true)] out TBehavior behaviorInstance)
    {
        if (_data.TryGetValue(entity.EntityType, out List<(TBehavior behaviorInstance, HashSet<string> behaviorMethods)> behaviorList))
        {
            foreach ((TBehavior temp, HashSet<string> behaviorMethods) in behaviorList)
            {
                if (!behaviorMethods.Contains(methodName))
                    continue;

                temp.Connect(entity);
                if (temp.ShouldProcess)
                {
                    behaviorInstance = temp;
                    return true;
                }
            }
        }
        behaviorInstance = null;
        return false;
    }

    public override void FillSet(Assembly assemblyToSearch)
    {
        _data.Clear();
        _data.AddRange(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch)
            .GroupBy(k => k.ApplyingType).ToDictionary(keySelector: k => k.Key, elementSelector: k => (
                from behaviorInstance in k.AsValueEnumerable()
                orderby behaviorInstance.Priority
                select (behaviorInstance, behaviorInstance.GetType().GetOverrideMethodNames(TOReflectionUtils.UniversalBindingFlags).ToHashSet())
            ).ToList()));
    }

    public override void Clear() => _data.Clear();
}
#endregion Core

public abstract class PlayerBehavior : NontypedEntityBehavior<Player>
{
    #region 实成员
    public Player Player { get; private set; } = null;

    public TOPlayer OceanPlayer { get; private set; } = null;

    public override void Connect(Player player)
    {
        Player = player;
        OceanPlayer = player.Ocean();
    }
    #endregion 实成员

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
    #endregion 虚成员
}

public abstract class NPCBehavior : TypedEntityBehavior<NPC>
{
    #region 实成员
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
    #endregion 实成员

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
    public virtual bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        return null;
    }
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
    public virtual bool? CanFallThroughPlatforms()
    {
        return null;
    }

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
    public virtual bool? CanBeCaughtBy(Item item, Player player)
    {
        return null;
    }

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
    public virtual bool? CanChat()
    {
        return null;
    }

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
    public virtual bool PreChatButtonClicked(bool firstButton)
    {
        return true;
    }

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
    public virtual bool? CanGoToStatue(bool toKingStatue)
    {
        return null;
    }

    /// <summary>
    /// Allows you to make things happen when this NPC teleports to a King or Queen statue.
    /// <para/> Called in single player or on the server only.
    /// </summary>
    /// <param name="toKingStatue">Whether the NPC was teleported to a King or Queen statue.</param>
    public virtual void OnGoToStatue(bool toKingStatue) { }

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
    public virtual bool NeedSaving()
    {
        return false;
    }

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
    public virtual int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
    {
        return null;
    }

    /// <inheritdoc cref="ModNPC.ChatBubblePosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="ModNPC.PartyHatPosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="ModNPC.EmoteBubblePosition(ref Vector2, ref SpriteEffects)"/>
    public virtual void EmoteBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }
    #endregion SpecialEffects
    #endregion 虚成员
}

public abstract class ProjectileBehavior : TypedEntityBehavior<Projectile>
{
    #region 实成员
    public Projectile Projectile { get; private set; } = null;

    public TOGlobalProjectile OceanProjectile { get; private set; } = null;

    public Player Owner { get; private set; } = null;

    public override void Connect(Projectile projectile)
    {
        Projectile = projectile;
        Owner = Main.player[Projectile.owner];
        OceanProjectile = projectile.Ocean();
    }

    #endregion 实成员

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

public abstract class ItemBehavior : TypedEntityBehavior<Item>
{
    #region 实成员
    public Item Item { get; private set; } = null;

    public TOGlobalItem OceanItem { get; private set; } = null;

    public override void Connect(Item item)
    {
        Item = item;
        OceanItem = item.Ocean();
    }
    #endregion 实成员

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

    #region Data
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
    #endregion Data
    #endregion 虚成员
}