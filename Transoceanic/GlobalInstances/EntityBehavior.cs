using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.GameInput;

namespace Transoceanic.GlobalInstances;

#region Base
/// <summary>
/// 标识某个继承自 <see cref="EntityBehavior{TEntity}"/> 的类是关键类。
/// <br/>在 <see cref="SimpleEntityBehaviorSet{TEntity, TBehavior}.Initialize(IEnumerable{TBehavior})"/> 逻辑中，具有此特性的类会无条件捕获几乎所有方法。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class CriticalBehaviorAttribute : Attribute;

public abstract class EntityBehavior<TEntity> where TEntity : Entity
{
    protected internal TEntity _entity;

    public abstract Mod Mod { get; }

    /// <summary>
    /// 优先级，越大越先应用。
    /// </summary>
    public virtual decimal Priority => 0m;

    public virtual bool ShouldProcess => true;

    /// <summary>
    /// <inheritdoc cref="ModType.SetStaticDefaults"/>
    /// </summary>
    public virtual void SetStaticDefaults() { }
}

public abstract class GeneralEntityBehavior<TEntity> : EntityBehavior<TEntity> where TEntity : Entity;

public abstract class GlobalEntityBehavior<TEntity> : GeneralEntityBehavior<TEntity> where TEntity : Entity
{
    /// <summary>
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.SetDefaults(TEntity)"/>
    /// </summary>
    public virtual void SetDefaults(TEntity entity) { }
}

public abstract class SingleEntityBehavior<TEntity> : EntityBehavior<TEntity> where TEntity : Entity
{
    public abstract int ApplyingType { get; }

    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.SetDefaults(TEntity)"/>
    public virtual void SetDefaults() { }
}

public class SimpleEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : EntityBehavior<TEntity>
{
    protected internal readonly Dictionary<string, List<TBehavior>> _data = [];

    public void Clear()
    {
        foreach ((string _, List<TBehavior> behaviors) in _data)
            behaviors.Clear();
        _data.Clear();
    }

    public void FillSet() => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>());

    public void FillSet(Assembly assemblyToSearch) => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch));

    public void FillSet<T>() where T : Mod => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>().Where(b => b.Mod is T));

    public IEnumerable<TBehavior> GetBehaviors([CallerMemberName] string methodName = null!)
    {
        if (_data.TryGetValue(methodName, out List<TBehavior> behaviors))
        {
            foreach (TBehavior behavior in behaviors)
            {
                if (behavior.ShouldProcess)
                    yield return behavior;
            }
        }
    }

    public IEnumerable<TBehavior> GetBehaviors(TEntity entity, [CallerMemberName] string methodName = null!)
    {
        if (_data.TryGetValue(methodName, out List<TBehavior> behaviors))
        {
            foreach (TBehavior behavior in behaviors)
            {
                behavior._entity = entity;
                if (behavior.ShouldProcess)
                    yield return behavior;
            }
        }
    }

    public IEnumerable<T> GetBehaviors<T>([CallerMemberName] string methodName = null!) where T : TBehavior
    {
        if (_data.TryGetValue(methodName, out List<TBehavior> behaviors))
        {
            foreach (TBehavior behavior in behaviors)
            {
                if (behavior is T typedBehavior && typedBehavior.ShouldProcess)
                    yield return typedBehavior;
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
                    behavior._entity = entity;
                    if (typedBehavior.ShouldProcess)
                        yield return typedBehavior;
                }
            }
        }
    }

    /// <summary>
    /// 按照 <see cref="EntityBehavior{TEntity}.Priority"/> 降序寻找通过 <see cref="SingleEntityBehavior{TEntity}.ShouldProcess"/> 检测且实现了指定方法的Override实例。
    /// </summary>
    public TBehavior GetFirstBehavior(TEntity entity, [CallerMemberName] string methodName = null!)
    {
        if (_data.TryGetValue(methodName, out List<TBehavior> behaviors))
        {
            foreach (TBehavior behavior in behaviors)
            {
                behavior._entity = entity;
                if (behavior.ShouldProcess)
                    return behavior;
            }
        }
        return null;
    }

    internal void Initialize(IEnumerable<TBehavior> behaviors)
    {
        foreach ((TBehavior behavior, HashSet<string> behaviorMethods) in
            behaviors.Select(b =>
            {
                Type type = b.GetType();
                return (b, (type.HasAttribute<CriticalBehaviorAttribute>() ? type.GetMethodNamesExceptObject(TOReflectionUtils.UniversalBindingFlags) : type.GetOverrideMethodNames(TOReflectionUtils.UniversalBindingFlags)).ToHashSet());
            }))
        {
            foreach (string method in behaviorMethods)
            {
                if (_data.TryGetValue(method, out List<TBehavior> behaviorList))
                    behaviorList.Add(behavior);
                else
                    _data[method] = [behavior];
            }
        }
        foreach (string methodName in _data.Keys)
            _data[methodName] = [.. _data[methodName].Distinct().OrderByDescending(b => b.Priority)];
    }
}

public class GeneralEntityBehaviorSet<TEntity, TBehavior> : SimpleEntityBehaviorSet<TEntity, TBehavior> where TEntity : Entity
    where TBehavior : GeneralEntityBehavior<TEntity>
{ }

public class GlobalEntityBehaviorSet<TEntity, TBehavior> : GeneralEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : GeneralEntityBehavior<TEntity>
{ }

public class SingleEntityBehaviorSet<TEntity, TBehavior>
    where TEntity : Entity
    where TBehavior : SingleEntityBehavior<TEntity>
{
    protected internal readonly Dictionary<int, SimpleEntityBehaviorSet<TEntity, TBehavior>> _data = [];

    /// <summary>
    /// 按照 <see cref="EntityBehavior{TEntity}.Priority"/> 降序寻找通过 <see cref="SingleEntityBehavior{TEntity}.ShouldProcess"/> 检测且实现了指定方法的Override实例。
    /// </summary>
    public bool TryGetBehavior(TEntity entity, string methodName, [NotNullWhen(true)] out TBehavior behavior) =>
        (behavior = _data.TryGetValue(entity.EntityType, out SimpleEntityBehaviorSet<TEntity, TBehavior> set) ? set.GetFirstBehavior(entity, methodName) : null) is not null;

    public void FillSet() => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>());

    public void FillSet<T>() where T : Mod => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>().Where(b => b.Mod is T));

    public void FillSet(Assembly assemblyToSearch) => Initialize(TOReflectionUtils.GetTypeInstancesDerivedFrom<TBehavior>(assemblyToSearch));

    internal void Initialize(IEnumerable<TBehavior> behaviors)
    {
        foreach (IGrouping<int, TBehavior> group in (IEnumerable<IGrouping<int, TBehavior>>)behaviors.GroupBy(b => b.ApplyingType))
        {
            _data[group.Key] = new();
            _data[group.Key].Initialize(group);
        }
    }

    public void Clear()
    {
        foreach (SimpleEntityBehaviorSet<TEntity, TBehavior> set in _data.Values)
            set.Clear();
        _data.Clear();
    }
}
#endregion Base

#region General Behavior
public abstract class PlayerBehavior : GeneralEntityBehavior<Player>
{
    public Player Player => _entity;

    public TOPlayer OceanPlayer => _entity.Ocean();

    #region 虚成员
    /// <summary>
    /// <inheritdoc cref="ModPlayer.Initialize"/>
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ResetEffects"/>
    /// </summary>
    public virtual void ResetEffects() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ResetInfoAccessories"/>
    /// </summary>
    public virtual void ResetInfoAccessories() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.RefreshInfoAccessoriesFromTeamPlayers"/>
    /// </summary>
    public virtual void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyMaxStats"/>
    /// </summary>
    public virtual void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        mana = StatModifier.Default;
    }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UpdateDead"/>
    /// </summary>
    public virtual void UpdateDead() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PreSaveCustomData"/>
    /// </summary>
    public virtual void PreSaveCustomData() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.SaveData"/>
    /// </summary>
    public virtual void SaveData(TagCompound tag) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.LoadData"/>
    /// </summary>
    public virtual void LoadData(TagCompound tag) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PreSavePlayer"/>
    /// </summary>
    public virtual void PreSavePlayer() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostSavePlayer"/>
    /// </summary>
    public virtual void PostSavePlayer() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CopyClientState"/>
    /// </summary>
    public virtual void CopyClientState(ModPlayer targetCopy) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.SyncPlayer"/>
    /// </summary>
    public virtual void SyncPlayer(int toWho, int fromWho, bool newPlayer) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.SendClientChanges"/>
    /// </summary>
    public virtual void SendClientChanges(ModPlayer clientPlayer) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UpdateBadLifeRegen"/>
    /// </summary>
    public virtual void UpdateBadLifeRegen() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UpdateLifeRegen"/>
    /// </summary>
    public virtual void UpdateLifeRegen() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.NaturalLifeRegen"/>
    /// </summary>
    public virtual void NaturalLifeRegen(ref float regen) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UpdateAutopause"/>
    /// </summary>
    public virtual void UpdateAutopause() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PreUpdate"/>
    /// </summary>
    public virtual void PreUpdate() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ProcessTriggers"/>
    /// </summary>
    public virtual void ProcessTriggers(TriggersSet triggersSet) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ArmorSetBonusActivated"/>
    /// </summary>
    public virtual void ArmorSetBonusActivated() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ArmorSetBonusHeld"/>
    /// </summary>
    public virtual void ArmorSetBonusHeld(int holdTime) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.SetControls"/>
    /// </summary>
    public virtual void SetControls() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PreUpdateBuffs"/>
    /// </summary>
    public virtual void PreUpdateBuffs() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostUpdateBuffs"/>
    /// </summary>
    public virtual void PostUpdateBuffs() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UpdateEquips"/>
    /// </summary>
    public virtual void UpdateEquips() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostUpdateEquips"/>
    /// </summary>
    public virtual void PostUpdateEquips() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UpdateVisibleAccessories"/>
    /// </summary>
    public virtual void UpdateVisibleAccessories() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UpdateVisibleVanityAccessories"/>
    /// </summary>
    public virtual void UpdateVisibleVanityAccessories() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UpdateDyes"/>
    /// </summary>
    public virtual void UpdateDyes() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostUpdateMiscEffects"/>
    /// </summary>
    public virtual void PostUpdateMiscEffects() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostUpdateRunSpeeds"/>
    /// </summary>
    public virtual void PostUpdateRunSpeeds() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PreUpdateMovement"/>
    /// </summary>
    public virtual void PreUpdateMovement() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostUpdate"/>
    /// </summary>
    public virtual void PostUpdate() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyExtraJumpDurationMultiplier"/>
    /// </summary>
    public virtual void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanStartExtraJump"/>
    /// </summary>
    public virtual bool CanStartExtraJump(ExtraJump jump) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnExtraJumpStarted"/>
    /// </summary>
    public virtual void OnExtraJumpStarted(ExtraJump jump, ref bool playSound) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnExtraJumpEnded"/>
    /// </summary>
    public virtual void OnExtraJumpEnded(ExtraJump jump) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnExtraJumpRefreshed"/>
    /// </summary>
    public virtual void OnExtraJumpRefreshed(ExtraJump jump) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ExtraJumpVisuals"/>
    /// </summary>
    public virtual void ExtraJumpVisuals(ExtraJump jump) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanShowExtraJumpVisuals"/>
    /// </summary>
    public virtual bool CanShowExtraJumpVisuals(ExtraJump jump) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnExtraJumpCleared"/>
    /// </summary>
    public virtual void OnExtraJumpCleared(ExtraJump jump) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.FrameEffects"/>
    /// </summary>
    public virtual void FrameEffects() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ImmuneTo"/>
    /// </summary>
    public virtual bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => false;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.FreeDodge"/>
    /// </summary>
    public virtual bool FreeDodge(Player.HurtInfo info) => false;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ConsumableDodge"/>
    /// </summary>
    public virtual bool ConsumableDodge(Player.HurtInfo info) => false;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyHurt"/>
    /// </summary>
    public virtual void ModifyHurt(ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnHurt"/>
    /// </summary>
    public virtual void OnHurt(Player.HurtInfo info) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostHurt"/>
    /// </summary>
    public virtual void PostHurt(Player.HurtInfo info) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PreKill"/>
    /// </summary>
    public virtual bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.Kill"/>
    /// </summary>
    public virtual void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PreModifyLuck"/>
    /// </summary>
    public virtual bool PreModifyLuck(ref float luck) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyLuck"/>
    /// </summary>
    public virtual void ModifyLuck(ref float luck) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PreItemCheck"/>
    /// </summary>
    public virtual bool PreItemCheck() => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostItemCheck"/>
    /// </summary>
    public virtual void PostItemCheck() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UseTimeMultiplier"/>
    /// </summary>
    public virtual float UseTimeMultiplier(Item item) => 1f;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UseAnimationMultiplier"/>
    /// </summary>
    public virtual float UseAnimationMultiplier(Item item) => 1f;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.UseSpeedMultiplier"/>
    /// </summary>
    public virtual float UseSpeedMultiplier(Item item) => 1f;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.GetHealLife"/>
    /// </summary>
    public virtual void GetHealLife(Item item, bool quickHeal, ref int healValue) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.GetHealMana"/>
    /// </summary>
    public virtual void GetHealMana(Item item, bool quickHeal, ref int healValue) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyManaCost"/>
    /// </summary>
    public virtual void ModifyManaCost(Item item, ref float reduce, ref float mult) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnMissingMana"/>
    /// </summary>
    public virtual void OnMissingMana(Item item, int neededMana) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnConsumeMana"/>
    /// </summary>
    public virtual void OnConsumeMana(Item item, int manaConsumed) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyWeaponDamage"/>
    /// </summary>
    public virtual void ModifyWeaponDamage(Item item, ref StatModifier damage) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyWeaponKnockback"/>
    /// </summary>
    public virtual void ModifyWeaponKnockback(Item item, ref StatModifier knockback) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyWeaponCrit"/>
    /// </summary>
    public virtual void ModifyWeaponCrit(Item item, ref float crit) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanConsumeAmmo"/>
    /// </summary>
    public virtual bool CanConsumeAmmo(Item weapon, Item ammo) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnConsumeAmmo"/>
    /// </summary>
    public virtual void OnConsumeAmmo(Item weapon, Item ammo) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanShoot"/>
    /// </summary>
    public virtual bool CanShoot(Item item) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyShootStats"/>
    /// </summary>
    public virtual void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.Shoot"/>
    /// </summary>
    public virtual bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.MeleeEffects"/>
    /// </summary>
    public virtual void MeleeEffects(Item item, Rectangle hitbox) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.EmitEnchantmentVisualsAt"/>
    /// </summary>
    public virtual void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanCatchNPC"/>
    /// </summary>
    public virtual bool? CanCatchNPC(NPC target, Item item) => null;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnCatchNPC"/>
    /// </summary>
    public virtual void OnCatchNPC(NPC npc, Item item, bool failed) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyItemScale"/>
    /// </summary>
    public virtual void ModifyItemScale(Item item, ref float scale) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnHitAnything"/>
    /// </summary>
    public virtual void OnHitAnything(float x, float y, Entity victim) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanHitNPC"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitNPC(NPC target) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanMeleeAttackCollideWithNPC"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target) => null;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyHitNPC"/>
    /// </summary>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnHitNPC"/>
    /// </summary>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanHitNPCWithItem"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanHitNPCWithItem(Item item, NPC target) => null;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyHitNPCWithItem"/>
    /// </summary>
    public virtual void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnHitNPCWithItem"/>
    /// </summary>
    public virtual void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanHitNPCWithProj"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanHitNPCWithProj(Projectile proj, NPC target) => null;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyHitNPCWithProj"/>
    /// </summary>
    public virtual void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnHitNPCWithProj"/>
    /// </summary>
    public virtual void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanHitPvp"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPvp(Item item, Player target) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanHitPvpWithProj"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPvpWithProj(Projectile proj, Player target) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanBeHitByNPC"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyHitByNPC"/>
    /// </summary>
    public virtual void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnHitByNPC"/>
    /// </summary>
    public virtual void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanBeHitByProjectile"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanBeHitByProjectile(Projectile proj) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyHitByProjectile"/>
    /// </summary>
    public virtual void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnHitByProjectile"/>
    /// </summary>
    public virtual void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyFishingAttempt"/>
    /// </summary>
    public virtual void ModifyFishingAttempt(ref FishingAttempt attempt) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CatchFish"/>
    /// </summary>
    public virtual void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyCaughtFish"/>
    /// </summary>
    public virtual void ModifyCaughtFish(Item fish) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanConsumeBait"/>
    /// </summary>
    public virtual bool? CanConsumeBait(Item bait) => null;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.GetFishingLevel"/>
    /// </summary>
    public virtual void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.AnglerQuestReward"/>
    /// </summary>
    public virtual void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.GetDyeTraderReward"/>
    /// </summary>
    public virtual void GetDyeTraderReward(List<int> rewardPool) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.DrawEffects"/>
    /// </summary>
    public virtual void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyDrawInfo"/>
    /// </summary>
    public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyDrawLayerOrdering"/>
    /// </summary>
    public virtual void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.HideDrawLayers"/>
    /// </summary>
    public virtual void HideDrawLayers(PlayerDrawSet drawInfo) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyScreenPosition"/>
    /// </summary>
    public virtual void ModifyScreenPosition() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyZoom"/>
    /// </summary>
    public virtual void ModifyZoom(ref float zoom) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PlayerConnect"/>
    /// </summary>
    public virtual void PlayerConnect() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PlayerDisconnect"/>
    /// </summary>
    public virtual void PlayerDisconnect() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnEnterWorld"/>
    /// </summary>
    public virtual void OnEnterWorld() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnRespawn"/>
    /// </summary>
    public virtual void OnRespawn() { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ShiftClickSlot"/>
    /// </summary>
    public virtual bool ShiftClickSlot(Item[] inventory, int context, int slot) => false;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.HoverSlot"/>
    /// </summary>
    public virtual bool HoverSlot(Item[] inventory, int context, int slot) => false;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostSellItem"/>
    /// </summary>
    public virtual void PostSellItem(NPC vendor, Item[] shopInventory, Item item) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanSellItem"/>
    /// </summary>
    public virtual bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostBuyItem"/>
    /// </summary>
    public virtual void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanBuyItem"/>
    /// </summary>
    public virtual bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanUseItem"/>
    /// </summary>
    public virtual bool CanUseItem(Item item) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanAutoReuseItem"/>
    /// </summary>
    public virtual bool? CanAutoReuseItem(Item item) => null;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyNurseHeal"/>
    /// </summary>
    public virtual bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyNursePrice"/>
    /// </summary>
    public virtual void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.PostNurseHeal"/>
    /// </summary>
    public virtual void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.AddStartingItems"/>
    /// </summary>
    public virtual IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) => [];

    /// <summary>
    /// <inheritdoc cref="ModPlayer.ModifyStartingInventory"/>
    /// </summary>
    public virtual void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath) { }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.AddMaterialsForCrafting"/>
    /// </summary>
    public virtual IEnumerable<Item> AddMaterialsForCrafting(out ModPlayer.ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        return null;
    }

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnPickup"/>
    /// </summary>
    public virtual bool OnPickup(Item item) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.CanBeTeleportedTo"/>
    /// </summary>
    public virtual bool CanBeTeleportedTo(Vector2 teleportPosition, string context) => true;

    /// <summary>
    /// <inheritdoc cref="ModPlayer.OnEquipmentLoadoutSwitched"/>
    /// </summary>
    public virtual void OnEquipmentLoadoutSwitched(int oldLoadoutIndex, int loadoutIndex) { }
    #endregion 虚成员
}

public abstract class GlobalNPCBehavior : GlobalEntityBehavior<NPC>
{
    /// <summary>
    /// <inheritdoc cref="GlobalNPC.SetDefaultsFromNetId"/>
    /// </summary>
    public virtual void SetDefaultsFromNetId(NPC npc) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnSpawn"/>
    /// </summary>
    public virtual void OnSpawn(NPC npc, IEntitySource source) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ApplyDifficultyAndPlayerScaling"/>
    /// </summary>
    public virtual void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.SetBestiary"/>
    /// </summary>
    public virtual void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyTypeName"/>
    /// </summary>
    public virtual void ModifyTypeName(NPC npc, ref string typeName) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyHoverBoundingBox"/>
    /// </summary>
    public virtual void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyTownNPCProfile"/>
    /// </summary>
    public virtual ITownNPCProfile ModifyTownNPCProfile(NPC npc) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyNPCNameList"/>
    /// </summary>
    public virtual void ModifyNPCNameList(NPC npc, List<string> nameList) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ResetEffects"/>
    /// </summary>
    public virtual void ResetEffects(NPC npc) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.PreAI"/>
    /// </summary>
    public virtual bool PreAI(NPC npc) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.AI"/>
    /// </summary>
    public virtual void AI(NPC npc) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.PostAI"/>
    /// </summary>
    public virtual void PostAI(NPC npc) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.SendExtraAI"/>
    /// </summary>
    public virtual void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ReceiveExtraAI"/>
    /// </summary>
    public virtual void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.FindFrame"/>
    /// </summary>
    public virtual void FindFrame(NPC npc, int frameHeight) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.HitEffect"/>
    /// </summary>
    public virtual void HitEffect(NPC npc, NPC.HitInfo hit) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.UpdateLifeRegen"/>
    /// </summary>
    public virtual void UpdateLifeRegen(NPC npc, ref int damage) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CheckActive"/>
    /// </summary>
    public virtual bool CheckActive(NPC npc) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CheckDead"/>
    /// </summary>
    public virtual bool CheckDead(NPC npc) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.SpecialOnKill"/>
    /// </summary>
    public virtual bool SpecialOnKill(NPC npc) => false;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.PreKill"/>
    /// </summary>
    public virtual bool PreKill(NPC npc) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnKill"/>
    /// </summary>
    public virtual void OnKill(NPC npc) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanFallThroughPlatforms"/>
    /// </summary>
    public virtual bool? CanFallThroughPlatforms(NPC npc) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanBeCaughtBy"/>
    /// </summary>
    public virtual bool? CanBeCaughtBy(NPC npc, Item item, Player player) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnCaughtBy"/>
    /// </summary>
    public virtual void OnCaughtBy(NPC npc, Player player, Item item, bool failed) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyNPCLoot"/>
    /// </summary>
    public virtual void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyGlobalLoot"/>
    /// </summary>
    public virtual void ModifyGlobalLoot(GlobalLoot globalLoot) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanHitPlayer"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyHitPlayer"/>
    /// </summary>
    public virtual void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnHitPlayer"/>
    /// </summary>
    public virtual void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanHitNPC"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitNPC(NPC npc, NPC target) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanBeHitByNPC"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanBeHitByNPC(NPC npc, NPC attacker) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyHitNPC"/>
    /// </summary>
    public virtual void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnHitNPC"/>
    /// </summary>
    public virtual void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanBeHitByItem"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanBeHitByItem(NPC npc, Player player, Item item) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanCollideWithPlayerMeleeAttack"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyHitByItem"/>
    /// </summary>
    public virtual void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnHitByItem"/>
    /// </summary>
    public virtual void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanBeHitByProjectile"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyHitByProjectile"/>
    /// </summary>
    public virtual void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnHitByProjectile"/>
    /// </summary>
    public virtual void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyIncomingHit"/>
    /// </summary>
    public virtual void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.BossHeadSlot"/>
    /// </summary>
    public virtual void BossHeadSlot(NPC npc, ref int index) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.BossHeadRotation"/>
    /// </summary>
    public virtual void BossHeadRotation(NPC npc, ref float rotation) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.BossHeadSpriteEffects"/>
    /// </summary>
    public virtual void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.GetAlpha"/>
    /// </summary>
    public virtual Color? GetAlpha(NPC npc, Color drawColor) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.DrawEffects"/>
    /// </summary>
    public virtual void DrawEffects(NPC npc, ref Color drawColor) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.PreDraw"/>
    /// </summary>
    public virtual bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.PostDraw"/>
    /// </summary>
    public virtual void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.DrawBehind"/>
    /// </summary>
    public virtual void DrawBehind(NPC npc, int index) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.DrawHealthBar"/>
    /// </summary>
    public virtual bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.EditSpawnRate"/>
    /// </summary>
    public virtual void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.EditSpawnRange"/>
    /// </summary>
    public virtual void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.EditSpawnPool"/>
    /// </summary>
    public virtual void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.SpawnNPC"/>
    /// </summary>
    public virtual void SpawnNPC(int npc, int tileX, int tileY) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanChat"/>
    /// </summary>
    public virtual bool? CanChat(NPC npc) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.GetChat"/>
    /// </summary>
    public virtual void GetChat(NPC npc, ref string chat) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.PreChatButtonClicked"/>
    /// </summary>
    public virtual bool PreChatButtonClicked(NPC npc, bool firstButton) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnChatButtonClicked"/>
    /// </summary>
    public virtual void OnChatButtonClicked(NPC npc, bool firstButton) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyShop"/>
    /// </summary>
    public virtual void ModifyShop(NPCShop shop) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyActiveShop"/>
    /// </summary>
    public virtual void ModifyActiveShop(NPC npc, string shopName, Item[] items) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.SetupTravelShop"/>
    /// </summary>
    public virtual void SetupTravelShop(int[] shop, ref int nextSlot) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.CanGoToStatue"/>
    /// </summary>
    public virtual bool? CanGoToStatue(NPC npc, bool toKingStatue) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.OnGoToStatue"/>
    /// </summary>
    public virtual void OnGoToStatue(NPC npc, bool toKingStatue) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.BuffTownNPC"/>
    /// </summary>
    public virtual void BuffTownNPC(ref float damageMult, ref int defense) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyDeathMessage"/>
    /// </summary>
    public virtual bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.TownNPCAttackStrength"/>
    /// </summary>
    public virtual void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.TownNPCAttackCooldown"/>
    /// </summary>
    public virtual void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.TownNPCAttackProj"/>
    /// </summary>
    public virtual void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.TownNPCAttackProjSpeed"/>
    /// </summary>
    public virtual void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.TownNPCAttackShoot"/>
    /// </summary>
    public virtual void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.TownNPCAttackMagic"/>
    /// </summary>
    public virtual void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.TownNPCAttackSwing"/>
    /// </summary>
    public virtual void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.DrawTownAttackGun"/>
    /// </summary>
    public virtual void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.DrawTownAttackSwing"/>
    /// </summary>
    public virtual void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ModifyCollisionData"/>
    /// </summary>
    public virtual bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.NeedSaving"/>
    /// </summary>
    public virtual bool NeedSaving(NPC npc) => false;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.SaveData"/>
    /// </summary>
    public virtual void SaveData(NPC npc, TagCompound tag) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.LoadData"/>
    /// </summary>
    public virtual void LoadData(NPC npc, TagCompound tag) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.PickEmote"/>
    /// </summary>
    public virtual int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.ChatBubblePosition"/>
    /// </summary>
    public virtual void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.PartyHatPosition"/>
    /// </summary>
    public virtual void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <summary>
    /// <inheritdoc cref="GlobalNPC.EmoteBubblePosition"/>
    /// </summary>
    public virtual void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) { }
}

public abstract class GlobalProjectileBehavior : GlobalEntityBehavior<Projectile>
{
    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.OnSpawn"/>
    /// </summary>
    public virtual void OnSpawn(Projectile projectile, IEntitySource source) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.PreAI"/>
    /// </summary>
    public virtual bool PreAI(Projectile projectile) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.AI"/>
    /// </summary>
    public virtual void AI(Projectile projectile) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.PostAI"/>
    /// </summary>
    public virtual void PostAI(Projectile projectile) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.SendExtraAI"/>
    /// </summary>
    public virtual void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.ReceiveExtraAI"/>
    /// </summary>
    public virtual void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.ShouldUpdatePosition"/>
    /// </summary>
    public virtual bool ShouldUpdatePosition(Projectile projectile) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.TileCollideStyle"/>
    /// </summary>
    public virtual bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.OnTileCollide"/>
    /// </summary>
    public virtual bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.PreKill"/>
    /// </summary>
    public virtual bool PreKill(Projectile projectile, int timeLeft) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.OnKill"/>
    /// </summary>
    public virtual void OnKill(Projectile projectile, int timeLeft) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.CanCutTiles"/>
    /// </summary>
    public virtual bool? CanCutTiles(Projectile projectile) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.CutTiles"/>
    /// </summary>
    public virtual void CutTiles(Projectile projectile) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.CanDamage"/>
    /// </summary>
    public virtual bool? CanDamage(Projectile projectile) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.MinionContactDamage"/>
    /// </summary>
    public virtual bool MinionContactDamage(Projectile projectile) => false;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.ModifyDamageHitbox"/>
    /// </summary>
    public virtual void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.CanHitNPC"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanHitNPC(Projectile projectile, NPC target) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.ModifyHitNPC"/>
    /// </summary>
    public virtual void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.OnHitNPC"/>
    /// </summary>
    public virtual void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.CanHitPvp"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPvp(Projectile projectile, Player target) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.CanHitPlayer"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPlayer(Projectile projectile, Player target) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.ModifyHitPlayer"/>
    /// </summary>
    public virtual void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.OnHitPlayer"/>
    /// </summary>
    public virtual void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.Colliding"/>
    /// </summary>
    [Obsolete("Poor performance.", true)]
    public virtual bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.GetAlpha"/>
    /// </summary>
    public virtual Color? GetAlpha(Projectile projectile, Color lightColor) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.PreDrawExtras"/>
    /// </summary>
    public virtual bool PreDrawExtras(Projectile projectile) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.PreDraw"/>
    /// </summary>
    public virtual bool PreDraw(Projectile projectile, ref Color lightColor) => true;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.PostDraw"/>
    /// </summary>
    public virtual void PostDraw(Projectile projectile, Color lightColor) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.DrawBehind"/>
    /// </summary>
    public virtual void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.CanUseGrapple"/>
    /// </summary>
    public virtual bool? CanUseGrapple(int type, Player player) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.UseGrapple"/>
    /// </summary>
    public virtual void UseGrapple(Player player, ref int type) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.NumGrappleHooks"/>
    /// </summary>
    public virtual void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.GrappleRetreatSpeed"/>
    /// </summary>
    public virtual void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.GrapplePullSpeed"/>
    /// </summary>
    public virtual void GrapplePullSpeed(Projectile projectile, Player player, ref float speed) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.GrappleTargetPoint"/>
    /// </summary>
    public virtual void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.GrappleCanLatchOnTo"/>
    /// </summary>
    public virtual bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y) => null;

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.PrepareBombToBlow"/>
    /// </summary>
    public virtual void PrepareBombToBlow(Projectile projectile) { }

    /// <summary>
    /// <inheritdoc cref="GlobalProjectile.EmitEnchantmentVisualsAt"/>
    /// </summary>
    public virtual void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) { }
}

public abstract class GlobalItemBehavior : GlobalEntityBehavior<Item>
{
    /// <inheritdoc cref="GlobalItem.OnCreated"/>
    public virtual void OnCreated(Item item, ItemCreationContext context) { }

    /// <inheritdoc cref="GlobalItem.OnSpawn"/>
    public virtual void OnSpawn(Item item, IEntitySource source) { }

    /// <inheritdoc cref="GlobalItem.ChoosePrefix"/>
    public virtual int ChoosePrefix(Item item, UnifiedRandom rand) => -1;

    /// <inheritdoc cref="GlobalItem.PrefixChance"/>
    public virtual bool? PrefixChance(Item item, int pre, UnifiedRandom rand) => null;

    /// <inheritdoc cref="GlobalItem.AllowPrefix"/>
    public virtual bool AllowPrefix(Item item, int pre) => true;

    /// <inheritdoc cref="GlobalItem.CanUseItem"/>
    public virtual bool CanUseItem(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanAutoReuseItem"/>
    public virtual bool? CanAutoReuseItem(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalItem.UseStyle"/>
    public virtual void UseStyle(Item item, Player player, Rectangle heldItemFrame) { }

    /// <inheritdoc cref="GlobalItem.HoldStyle"/>
    public virtual void HoldStyle(Item item, Player player, Rectangle heldItemFrame) { }

    /// <inheritdoc cref="GlobalItem.HoldItem"/>
    public virtual void HoldItem(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UseTimeMultiplier"/>
    public virtual float UseTimeMultiplier(Item item, Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseAnimationMultiplier"/>
    public virtual float UseAnimationMultiplier(Item item, Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseSpeedMultiplier"/>
    public virtual float UseSpeedMultiplier(Item item, Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.GetHealLife"/>
    public virtual void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="GlobalItem.GetHealMana"/>
    public virtual void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="GlobalItem.ModifyManaCost"/>
    public virtual void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult) { }

    /// <inheritdoc cref="GlobalItem.OnMissingMana"/>
    public virtual void OnMissingMana(Item item, Player player, int neededMana) { }

    /// <inheritdoc cref="GlobalItem.OnConsumeMana"/>
    public virtual void OnConsumeMana(Item item, Player player, int manaConsumed) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponDamage"/>
    public virtual void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) { }

    /// <inheritdoc cref="GlobalItem.ModifyResearchSorting"/>
    public virtual void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) { }

    /// <inheritdoc cref="GlobalItem.CanConsumeBait"/>
    public virtual bool? CanConsumeBait(Player player, Item bait) => null;

    /// <inheritdoc cref="GlobalItem.CanResearch"/>
    public virtual bool CanResearch(Item item) => true;

    /// <inheritdoc cref="GlobalItem.OnResearched"/>
    public virtual void OnResearched(Item item, bool fullyResearched) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponKnockback"/>
    public virtual void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponCrit"/>
    public virtual void ModifyWeaponCrit(Item item, Player player, ref float crit) { }

    /// <inheritdoc cref="GlobalItem.NeedsAmmo"/>
    public virtual bool NeedsAmmo(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.PickAmmo"/>
    public virtual void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalItem.CanChooseAmmo"/>
    public virtual bool? CanChooseAmmo(Item weapon, Item ammo, Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanBeChosenAsAmmo"/>
    public virtual bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player) => null;

    /// <inheritdoc cref="GlobalItem.CanConsumeAmmo"/>
    public virtual bool CanConsumeAmmo(Item weapon, Item ammo, Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanBeConsumedAsAmmo"/>
    public virtual bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnConsumeAmmo"/>
    public virtual void OnConsumeAmmo(Item weapon, Item ammo, Player player) { }

    /// <inheritdoc cref="GlobalItem.OnConsumedAsAmmo"/>
    public virtual void OnConsumedAsAmmo(Item ammo, Item weapon, Player player) { }

    /// <inheritdoc cref="GlobalItem.CanShoot"/>
    public virtual bool CanShoot(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.ModifyShootStats"/>
    public virtual void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalItem.Shoot"/>
    public virtual bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <inheritdoc cref="GlobalItem.UseItemHitbox"/>
    public virtual void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) { }

    /// <inheritdoc cref="GlobalItem.MeleeEffects"/>
    public virtual void MeleeEffects(Item item, Player player, Rectangle hitbox) { }

    /// <inheritdoc cref="GlobalItem.CanCatchNPC"/>
    public virtual bool? CanCatchNPC(Item item, NPC target, Player player) => null;

    /// <inheritdoc cref="GlobalItem.OnCatchNPC"/>
    public virtual void OnCatchNPC(Item item, NPC npc, Player player, bool failed) { }

    /// <inheritdoc cref="GlobalItem.ModifyItemScale"/>
    public virtual void ModifyItemScale(Item item, Player player, ref float scale) { }

    /// <inheritdoc cref="GlobalItem.CanHitNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanHitNPC(Item item, Player player, NPC target) => null;

    /// <inheritdoc cref="GlobalItem.CanMeleeAttackCollideWithNPC"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target) => null;

    /// <inheritdoc cref="GlobalItem.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalItem.OnHitNPC"/>
    public virtual void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalItem.CanHitPvp"/>
    [Obsolete("Poor performance.", true)]
    public virtual bool CanHitPvp(Item item, Player player, Player target) => true;

    /// <inheritdoc cref="GlobalItem.ModifyHitPvp"/>
    public virtual void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalItem.OnHitPvp"/>
    public virtual void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo) { }

    /// <inheritdoc cref="GlobalItem.UseItem"/>
    public virtual bool? UseItem(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalItem.UseAnimation"/>
    public virtual void UseAnimation(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.ConsumeItem"/>
    public virtual bool ConsumeItem(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnConsumeItem"/>
    public virtual void OnConsumeItem(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UseItemFrame"/>
    public virtual void UseItemFrame(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.HoldItemFrame"/>
    public virtual void HoldItemFrame(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.AltFunctionUse"/>
    public virtual bool AltFunctionUse(Item item, Player player) => false;

    /// <inheritdoc cref="GlobalItem.UpdateInventory"/>
    public virtual void UpdateInventory(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateInfoAccessory"/>
    public virtual void UpdateInfoAccessory(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateEquip"/>
    public virtual void UpdateEquip(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateAccessory"/>
    public virtual void UpdateAccessory(Item item, Player player, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.UpdateVanity"/>
    public virtual void UpdateVanity(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateVisibleAccessory"/>
    public virtual void UpdateVisibleAccessory(Item item, Player player, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.UpdateItemDye"/>
    public virtual void UpdateItemDye(Item item, Player player, int dye, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.IsArmorSet"/>
    public virtual string IsArmorSet(Item head, Item body, Item legs) => "";

    /// <inheritdoc cref="GlobalItem.UpdateArmorSet"/>
    public virtual void UpdateArmorSet(Player player, string set) { }

    /// <inheritdoc cref="GlobalItem.IsVanitySet"/>
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

    /// <inheritdoc cref="GlobalItem.PreUpdateVanitySet"/>
    public virtual void PreUpdateVanitySet(Player player, string set) { }

    /// <inheritdoc cref="GlobalItem.UpdateVanitySet"/>
    public virtual void UpdateVanitySet(Player player, string set) { }

    /// <inheritdoc cref="GlobalItem.ArmorSetShadows"/>
    public virtual void ArmorSetShadows(Player player, string set) { }

    /// <inheritdoc cref="GlobalItem.SetMatch"/>
    public virtual void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) { }

    /// <inheritdoc cref="GlobalItem.CanRightClick"/>
    public virtual bool CanRightClick(Item item) => false;

    /// <inheritdoc cref="GlobalItem.RightClick"/>
    public virtual void RightClick(Item item, Player player) { }

    /// <inheritdoc cref="GlobalItem.ModifyItemLoot"/>
    public virtual void ModifyItemLoot(Item item, ItemLoot itemLoot) { }

    /// <inheritdoc cref="GlobalItem.CanStack"/>
    public virtual bool CanStack(Item destination, Item source) => true;

    /// <inheritdoc cref="GlobalItem.CanStackInWorld"/>
    public virtual bool CanStackInWorld(Item destination, Item source) => true;

    /// <inheritdoc cref="GlobalItem.OnStack"/>
    public virtual void OnStack(Item destination, Item source, int numToTransfer) { }

    /// <inheritdoc cref="GlobalItem.SplitStack"/>
    public virtual void SplitStack(Item destination, Item source, int numToTransfer) { }

    /// <inheritdoc cref="GlobalItem.ReforgePrice"/>
    public virtual bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) => true;

    /// <inheritdoc cref="GlobalItem.CanReforge"/>
    public virtual bool CanReforge(Item item) => true;

    /// <inheritdoc cref="GlobalItem.PreReforge"/>
    public virtual void PreReforge(Item item) { }

    /// <inheritdoc cref="GlobalItem.PostReforge"/>
    public virtual void PostReforge(Item item) { }

    /// <inheritdoc cref="GlobalItem.DrawArmorColor"/>
    public virtual void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }

    /// <inheritdoc cref="GlobalItem.ArmorArmGlowMask"/>
    public virtual void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) { }

    /// <inheritdoc cref="GlobalItem.VerticalWingSpeeds"/>
    public virtual void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) { }

    /// <inheritdoc cref="GlobalItem.HorizontalWingSpeeds"/>
    public virtual void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration) { }

    /// <inheritdoc cref="GlobalItem.WingUpdate"/>
    public virtual bool WingUpdate(int wings, Player player, bool inUse) => false;

    /// <inheritdoc cref="GlobalItem.Update"/>
    public virtual void Update(Item item, ref float gravity, ref float maxFallSpeed) { }

    /// <inheritdoc cref="GlobalItem.PostUpdate"/>
    public virtual void PostUpdate(Item item) { }

    /// <inheritdoc cref="GlobalItem.GrabRange"/>
    public virtual void GrabRange(Item item, Player player, ref int grabRange) { }

    /// <inheritdoc cref="GlobalItem.GrabStyle"/>
    public virtual bool GrabStyle(Item item, Player player) => false;

    /// <inheritdoc cref="GlobalItem.CanPickup"/>
    public virtual bool CanPickup(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnPickup"/>
    public virtual bool OnPickup(Item item, Player player) => true;

    /// <inheritdoc cref="GlobalItem.ItemSpace"/>
    public virtual bool ItemSpace(Item item, Player player) => false;

    /// <inheritdoc cref="GlobalItem.GetAlpha"/>
    public virtual Color? GetAlpha(Item item, Color lightColor) => null;

    /// <inheritdoc cref="GlobalItem.PreDrawInWorld"/>
    public virtual bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawInWorld"/>
    public virtual void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) { }

    /// <inheritdoc cref="GlobalItem.PreDrawInInventory"/>
    public virtual bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawInInventory"/>
    public virtual void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { }

    /// <inheritdoc cref="GlobalItem.HoldoutOffset"/>
    public virtual Vector2? HoldoutOffset(int type) => null;

    /// <inheritdoc cref="GlobalItem.HoldoutOrigin"/>
    public virtual Vector2? HoldoutOrigin(int type) => null;

    /// <inheritdoc cref="GlobalItem.CanEquipAccessory"/>
    public virtual bool CanEquipAccessory(Item item, Player player, int slot, bool modded) => true;

    /// <inheritdoc cref="GlobalItem.CanAccessoryBeEquippedWith"/>
    public virtual bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) => true;

    /// <inheritdoc cref="GlobalItem.ExtractinatorUse"/>
    public virtual void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack) { }

    /// <inheritdoc cref="GlobalItem.CaughtFishStack"/>
    public virtual void CaughtFishStack(int type, ref int stack) { }

    /// <inheritdoc cref="GlobalItem.IsAnglerQuestAvailable"/>
    public virtual bool IsAnglerQuestAvailable(int type) => true;

    /// <inheritdoc cref="GlobalItem.AnglerChat"/>
    public virtual void AnglerChat(int type, ref string chat, ref string catchLocation) { }

    /// <inheritdoc cref="GlobalItem.AddRecipes"/>
    public virtual void AddRecipes() { }

    /// <inheritdoc cref="GlobalItem.PreDrawTooltip"/>
    public virtual bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawTooltip"/>
    public virtual void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines) { }

    /// <inheritdoc cref="GlobalItem.PreDrawTooltipLine"/>
    public virtual bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawTooltipLine"/>
    public virtual void PostDrawTooltipLine(Item item, DrawableTooltipLine line) { }

    /// <inheritdoc cref="GlobalItem.ModifyTooltips"/>
    public virtual void ModifyTooltips(Item item, List<TooltipLine> tooltips) { }

    /// <inheritdoc cref="GlobalItem.SaveData"/>
    public virtual void SaveData(Item item, TagCompound tag) { }

    /// <inheritdoc cref="GlobalItem.LoadData"/>
    public virtual void LoadData(Item item, TagCompound tag) { }

    /// <inheritdoc cref="GlobalItem.NetSend"/>
    public virtual void NetSend(Item item, BinaryWriter writer) { }

    /// <inheritdoc cref="GlobalItem.NetReceive"/>
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
    public NPC NPC => _entity;

    public TOGlobalNPC OceanNPC => _entity.Ocean();

    public Player Target => NPC.PlayerTarget;

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
    /// <inheritdoc cref="GlobalNPC.SetDefaultsFromNetId"/>
    public virtual void SetDefaultsFromNetId() { }

    /// <inheritdoc cref="GlobalNPC.ApplyDifficultyAndPlayerScaling"/>
    public virtual void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) { }

    /// <inheritdoc cref="GlobalNPC.SetBestiary"/>
    public virtual void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }

    /// <inheritdoc cref="GlobalNPC.ModifyTypeName"/>
    public virtual void ModifyTypeName(ref string typeName) { }

    /// <inheritdoc cref="GlobalNPC.ModifyHoverBoundingBox"/>
    public virtual void ModifyHoverBoundingBox(ref Rectangle boundingBox) { }

    /// <inheritdoc cref="GlobalNPC.ModifyTownNPCProfile"/>
    public virtual ITownNPCProfile ModifyTownNPCProfile() => null;

    /// <inheritdoc cref="GlobalNPC.ModifyNPCNameList"/>
    public virtual void ModifyNPCNameList(List<string> nameList) { }

    /// <inheritdoc cref="GlobalNPC.ResetEffects"/>
    public virtual void ResetEffects() { }
    #endregion Defaults

    #region Lifetime
    /// <inheritdoc cref="GlobalNPC.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <inheritdoc cref="GlobalNPC.CheckActive"/>
    public virtual bool CheckActive() => true;

    /// <inheritdoc cref="GlobalNPC.CheckDead"/>
    public virtual bool CheckDead() => true;

    /// <inheritdoc cref="GlobalNPC.SpecialOnKill"/>
    public virtual bool SpecialOnKill() => false;

    /// <inheritdoc cref="GlobalNPC.PreKill"/>
    public virtual bool PreKill() => true;

    /// <inheritdoc cref="GlobalNPC.OnKill"/>
    public virtual void OnKill() { }

    /// <inheritdoc cref="GlobalNPC.ModifyNPCLoot"/>
    public virtual void ModifyNPCLoot(NPCLoot npcLoot) { }
    #endregion Lifetime

    #region AI
    /// <inheritdoc cref="GlobalNPC.PreAI"/>
    public virtual bool PreAI() => true;

    /// <inheritdoc cref="GlobalNPC.AI"/>
    public virtual void AI() { }

    /// <inheritdoc cref="GlobalNPC.PostAI"/>
    public virtual void PostAI() { }
    #endregion AI

    #region Draw
    /// <inheritdoc cref="GlobalNPC.FindFrame"/>
    public virtual void FindFrame(int frameHeight) { }

    /// <inheritdoc cref="GlobalNPC.GetAlpha"/>
    public virtual Color? GetAlpha(Color drawColor) => null;

    /// <inheritdoc cref="GlobalNPC.DrawEffects"/>
    public virtual void DrawEffects(ref Color drawColor) { }

    /// <inheritdoc cref="GlobalNPC.PreDraw"/>
    public virtual bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

    /// <inheritdoc cref="GlobalNPC.PostDraw"/>
    public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <inheritdoc cref="GlobalNPC.DrawBehind"/>
    public virtual void DrawBehind(int index) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadSlot"/>
    public virtual void BossHeadSlot(ref int index) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadRotation"/>
    public virtual void BossHeadRotation(ref float rotation) { }

    /// <inheritdoc cref="GlobalNPC.BossHeadSpriteEffects"/>
    public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.DrawHealthBar"/>
    public virtual bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => null;
    #endregion Draw

    #region Hit
    /// <inheritdoc cref="GlobalNPC.HitEffect"/>
    public virtual void HitEffect(NPC.HitInfo hit) { }

    /// <inheritdoc cref="GlobalNPC.CanBeHitByItem"/>
    public virtual bool? CanBeHitByItem(Player player, Item item) => null;

    /// <inheritdoc cref="GlobalNPC.CanCollideWithPlayerMeleeAttack"/>
    public virtual bool? CanCollideWithPlayerMeleeAttack(Player player, Item item, Rectangle meleeAttackHitbox) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyHitByItem"/>
    public virtual void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitByItem"/>
    public virtual void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalNPC.CanBeHitByProjectile"/>
    public virtual bool? CanBeHitByProjectile(Projectile projectile) => null;

    /// <inheritdoc cref="GlobalNPC.ModifyHitByProjectile"/>
    public virtual void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitByProjectile"/>
    public virtual void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalNPC.CanBeHitByNPC"/>
    public virtual bool CanBeHitByNPC(NPC attacker) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyIncomingHit"/>
    public virtual void ModifyIncomingHit(ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.CanHitPlayer"/>
    public virtual bool CanHitPlayer(Player target, ref int cooldownSlot) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyHitPlayer"/>
    public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitPlayer"/>
    public virtual void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) { }

    /// <inheritdoc cref="GlobalNPC.CanHitNPC"/>
    public virtual bool CanHitNPC(NPC target) => true;

    /// <inheritdoc cref="GlobalNPC.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalNPC.OnHitNPC"/>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit) { }

    /// <inheritdoc cref="GlobalNPC.ModifyCollisionData"/>
    public virtual bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => true;
    #endregion Hit

    #region SpecialEffects
    /// <inheritdoc cref="GlobalNPC.UpdateLifeRegen"/>
    public virtual void UpdateLifeRegen(ref int damage) { }

    /// <inheritdoc cref="GlobalNPC.CanFallThroughPlatforms"/>
    public virtual bool? CanFallThroughPlatforms() => null;

    /// <inheritdoc cref="GlobalNPC.CanBeCaughtBy"/>
    public virtual bool? CanBeCaughtBy(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalNPC.OnCaughtBy"/>
    public virtual void OnCaughtBy(Player player, Item item, bool failed) { }

    /// <inheritdoc cref="GlobalNPC.CanChat"/>
    public virtual bool? CanChat() => null;

    /// <inheritdoc cref="GlobalNPC.GetChat"/>
    public virtual void GetChat(ref string chat) { }

    /// <inheritdoc cref="GlobalNPC.PreChatButtonClicked"/>
    public virtual bool PreChatButtonClicked(bool firstButton) => true;

    /// <inheritdoc cref="GlobalNPC.OnChatButtonClicked"/>
    public virtual void OnChatButtonClicked(bool firstButton) { }

    /// <inheritdoc cref="GlobalNPC.ModifyActiveShop"/>
    public virtual void ModifyActiveShop(string shopName, Item[] items) { }

    /// <inheritdoc cref="GlobalNPC.CanGoToStatue"/>
    public virtual bool? CanGoToStatue(bool toKingStatue) => null;

    /// <inheritdoc cref="GlobalNPC.OnGoToStatue"/>
    public virtual void OnGoToStatue(bool toKingStatue) { }

    /// <inheritdoc cref="GlobalNPC.ModifyDeathMessage"/>
    public virtual bool ModifyDeathMessage(ref NetworkText customText, ref Color color) => true;

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackStrength"/>
    public virtual void TownNPCAttackStrength(ref int damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackCooldown"/>
    public virtual void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackProj"/>
    public virtual void TownNPCAttackProj(ref int projType, ref int attackDelay) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackProjSpeed"/>
    public virtual void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackShoot"/>
    public virtual void TownNPCAttackShoot(ref bool inBetweenShots) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackMagic"/>
    public virtual void TownNPCAttackMagic(ref float auraLightMultiplier) { }

    /// <inheritdoc cref="GlobalNPC.TownNPCAttackSwing"/>
    public virtual void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) { }

    /// <inheritdoc cref="GlobalNPC.DrawTownAttackGun"/>
    public virtual void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) { }

    /// <inheritdoc cref="GlobalNPC.DrawTownAttackSwing"/>
    public virtual void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) { }

    /// <inheritdoc cref="GlobalNPC.NeedSaving"/>
    public virtual bool NeedSaving() => false;

    /// <inheritdoc cref="GlobalNPC.SaveData"/>
    public virtual void SaveData(TagCompound tag) { }

    /// <inheritdoc cref="GlobalNPC.LoadData"/>
    public virtual void LoadData(TagCompound tag) { }

    /// <inheritdoc cref="GlobalNPC.PickEmote"/>
    public virtual int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => null;

    /// <inheritdoc cref="GlobalNPC.ChatBubblePosition"/>
    public virtual void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.PartyHatPosition"/>
    public virtual void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }

    /// <inheritdoc cref="GlobalNPC.EmoteBubblePosition"/>
    public virtual void EmoteBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) { }
    #endregion SpecialEffects
    #endregion 虚成员
}

public abstract class SingleProjectileBehavior : SingleEntityBehavior<Projectile>
{
    public Projectile Projectile => _entity;

    public TOGlobalProjectile OceanProjectile => _entity.Ocean();

    public Player Owner => Projectile.Owner;

    public int Timer1
    {
        get => OceanProjectile.Timer1;
        set => OceanProjectile.Timer1 = value;
    }
    public int Timer2
    {
        get => OceanProjectile.Timer2;
        set => OceanProjectile.Timer2 = value;
    }
    public int Timer3
    {
        get => OceanProjectile.Timer3;
        set => OceanProjectile.Timer3 = value;
    }
    public float Timer4
    {
        get => OceanProjectile.Timer4;
        set => OceanProjectile.Timer4 = value;
    }
    public float Timer5
    {
        get => OceanProjectile.Timer5;
        set => OceanProjectile.Timer5 = value;
    }

    #region 虚成员
    #region Lifetime
    /// <inheritdoc cref="GlobalProjectile.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <inheritdoc cref="GlobalProjectile.TileCollideStyle"/>
	public virtual bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => true;

    /// <inheritdoc cref="GlobalProjectile.OnTileCollide"/>
    public virtual bool OnTileCollide(Vector2 oldVelocity) => true;

    /// <inheritdoc cref="GlobalProjectile.PreKill"/>
    public virtual bool PreKill(int timeLeft) => true;

    /// <inheritdoc cref="GlobalProjectile.OnKill"/>
    public virtual void OnKill(int timeLeft) { }
    #endregion Lifetime

    #region AI
    /// <inheritdoc cref="GlobalProjectile.PreAI"/>
    public virtual bool PreAI() => true;

    /// <inheritdoc cref="GlobalProjectile.AI"/>
    public virtual void AI() { }

    /// <inheritdoc cref="GlobalProjectile.PostAI"/>
    public virtual void PostAI() { }

    /// <inheritdoc cref="GlobalProjectile.ShouldUpdatePosition"/>
    public virtual bool ShouldUpdatePosition() => true;
    #endregion AI

    #region Hit
    /// <inheritdoc cref="GlobalProjectile.CanCutTiles"/>
    public virtual bool? CanCutTiles() => null;

    /// <inheritdoc cref="GlobalProjectile.CutTiles"/>
    public virtual void CutTiles() { }

    /// <inheritdoc cref="GlobalProjectile.CanDamage"/>
    public virtual bool? CanDamage() => null;

    /// <inheritdoc cref="GlobalProjectile.MinionContactDamage"/>
    public virtual bool MinionContactDamage() => false;

    /// <inheritdoc cref="GlobalProjectile.ModifyDamageHitbox"/>
    public virtual void ModifyDamageHitbox(ref Rectangle hitbox) { }

    /// <inheritdoc cref="GlobalProjectile.CanHitNPC"/>
    public virtual bool? CanHitNPC(NPC target) => null;

    /// <inheritdoc cref="GlobalProjectile.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalProjectile.OnHitNPC"/>
    public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalProjectile.CanHitPvp"/>
    public virtual bool CanHitPvp(Player target) => true;

    /// <inheritdoc cref="GlobalProjectile.CanHitPlayer"/>
    public virtual bool CanHitPlayer(Player target) => true;

    /// <inheritdoc cref="GlobalProjectile.ModifyHitPlayer"/>
    public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalProjectile.OnHitPlayer"/>
    public virtual void OnHitPlayer(Player target, Player.HurtInfo info) { }

    /// <inheritdoc cref="GlobalProjectile.Colliding"/>
    public virtual bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => null;
    #endregion Hit

    #region Draw
    /// <inheritdoc cref="GlobalProjectile.GetAlpha"/>
    public virtual Color? GetAlpha(Color lightColor) => null;

    /// <inheritdoc cref="GlobalProjectile.PreDrawExtras"/>
    public virtual bool PreDrawExtras() => true;

    /// <inheritdoc cref="GlobalProjectile.PreDraw"/>
    public virtual bool PreDraw(ref Color lightColor) => true;

    /// <inheritdoc cref="GlobalProjectile.PostDraw"/>
    public virtual void PostDraw(Color lightColor) { }

    /// <inheritdoc cref="GlobalProjectile.DrawBehind"/>
    public virtual void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) { }
    #endregion Draw

    #region SpecialEffects
    /// <inheritdoc cref="GlobalProjectile.NumGrappleHooks"/>
    public virtual void NumGrappleHooks(Player player, ref int numHooks) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleRetreatSpeed"/>
    public virtual void GrappleRetreatSpeed(Player player, ref float speed) { }

    /// <inheritdoc cref="GlobalProjectile.GrapplePullSpeed"/>
    public virtual void GrapplePullSpeed(Player player, ref float speed) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleTargetPoint"/>
    public virtual void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY) { }

    /// <inheritdoc cref="GlobalProjectile.GrappleCanLatchOnTo"/>
    public virtual bool? GrappleCanLatchOnTo(Player player, int x, int y) => null;

    /// <inheritdoc cref="GlobalProjectile.PrepareBombToBlow"/>
    public virtual void PrepareBombToBlow() { }

    /// <inheritdoc cref="GlobalProjectile.EmitEnchantmentVisualsAt"/>
    public virtual void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight) { }
    #endregion SpecialEffects
    #endregion 虚成员
}

public abstract class SingleItemBehavior : SingleEntityBehavior<Item>
{
    public Item Item => _entity;

    public TOGlobalItem OceanItem => _entity.Ocean();

    #region 虚成员
    #region Defaults
    /// <inheritdoc cref="GlobalItem.AddRecipes"/>
    public virtual void AddRecipes() { }
    #endregion Defaults

    #region Lifetime
    /// <inheritdoc cref="GlobalItem.OnCreated"/>
    public virtual void OnCreated(ItemCreationContext context) { }

    /// <inheritdoc cref="GlobalItem.OnSpawn"/>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <inheritdoc cref="GlobalItem.Update"/>
    public virtual void Update(ref float gravity, ref float maxFallSpeed) { }

    /// <inheritdoc cref="GlobalItem.PostUpdate"/>
    public virtual void PostUpdate() { }

    /// <inheritdoc cref="GlobalItem.GrabRange"/>
    public virtual void GrabRange(Player player, ref int grabRange) { }

    /// <inheritdoc cref="GlobalItem.GrabStyle"/>
    public virtual bool GrabStyle(Player player) => false;

    /// <inheritdoc cref="GlobalItem.CanPickup"/>
    public virtual bool CanPickup(Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnPickup"/>
    public virtual bool OnPickup(Player player) => true;

    /// <inheritdoc cref="GlobalItem.ItemSpace"/>
    public virtual bool ItemSpace(Player player) => false;
    #endregion Lifetime

    #region Update
    /// <inheritdoc cref="GlobalItem.UpdateInventory"/>
    public virtual void UpdateInventory(Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateInfoAccessory"/>
    public virtual void UpdateInfoAccessory(Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateEquip"/>
    public virtual void UpdateEquip(Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateAccessory"/>
    public virtual void UpdateAccessory(Player player, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.UpdateVanity"/>
    public virtual void UpdateVanity(Player player) { }

    /// <inheritdoc cref="GlobalItem.UpdateVisibleAccessory"/>
    public virtual void UpdateVisibleAccessory(Player player, bool hideVisual) { }

    /// <inheritdoc cref="GlobalItem.UpdateItemDye"/>
    public virtual void UpdateItemDye(Player player, int dye, bool hideVisual) { }
    #endregion Update

    #region Draw
    /// <inheritdoc cref="GlobalItem.GetAlpha"/>
    public virtual Color? GetAlpha(Color lightColor) => null;

    /// <inheritdoc cref="GlobalItem.PreDrawInWorld"/>
    public virtual bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawInWorld"/>
    public virtual void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) { }

    /// <inheritdoc cref="GlobalItem.PreDrawInInventory"/>
    public virtual bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawInInventory"/>
    public virtual void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) { }
    #endregion Draw

    #region Prefix
    /// <inheritdoc cref="GlobalItem.ChoosePrefix"/>
    public virtual int ChoosePrefix(UnifiedRandom rand) => -1;

    /// <inheritdoc cref="GlobalItem.PrefixChance"/>
    public virtual bool? PrefixChance(int pre, UnifiedRandom rand) => null;

    /// <inheritdoc cref="GlobalItem.AllowPrefix"/>
    public virtual bool AllowPrefix(int pre) => true;

    /// <inheritdoc cref="GlobalItem.ReforgePrice"/>
    public virtual bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount) => true;

    /// <inheritdoc cref="GlobalItem.CanReforge"/>
    public virtual bool CanReforge() => true;

    /// <inheritdoc cref="GlobalItem.PreReforge"/>
    public virtual void PreReforge() { }

    /// <inheritdoc cref="GlobalItem.PostReforge"/>
    public virtual void PostReforge() { }
    #endregion Prefix

    #region Use
    /// <inheritdoc cref="GlobalItem.AltFunctionUse"/>
    public virtual bool AltFunctionUse(Player player) => false;

    /// <inheritdoc cref="GlobalItem.CanUseItem"/>
    public virtual bool CanUseItem(Player player) => true;

    /// <inheritdoc cref="GlobalItem.CanAutoReuseItem"/>
    public virtual bool? CanAutoReuseItem(Player player) => null;

    /// <inheritdoc cref="GlobalItem.UseStyle"/>
    public virtual void UseStyle(Player player, Rectangle heldItemFrame) { }

    /// <inheritdoc cref="GlobalItem.HoldStyle"/>
    public virtual void HoldStyle(Player player, Rectangle heldItemFrame) { }

    /// <inheritdoc cref="GlobalItem.HoldItem"/>
    public virtual void HoldItem(Player player) { }

    /// <inheritdoc cref="GlobalItem.UseTimeMultiplier"/>
    public virtual float UseTimeMultiplier(Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseAnimationMultiplier"/>
    public virtual float UseAnimationMultiplier(Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseSpeedMultiplier"/>
    public virtual float UseSpeedMultiplier(Player player) => 1f;

    /// <inheritdoc cref="GlobalItem.UseItem"/>
    public virtual bool? UseItem(Player player) => null;

    /// <inheritdoc cref="GlobalItem.UseAnimation"/>
    public virtual void UseAnimation(Player player) { }

    /// <inheritdoc cref="GlobalItem.CanShoot"/>
    public virtual bool CanShoot(Player player) => true;

    /// <inheritdoc cref="GlobalItem.Shoot"/>
    public virtual bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => true;

    /// <inheritdoc cref="GlobalItem.CanRightClick"/>
    public virtual bool CanRightClick() => false;

    /// <inheritdoc cref="GlobalItem.RightClick"/>
    public virtual void RightClick(Player player) { }
    #endregion Use

    #region ModifyStats
    /// <inheritdoc cref="GlobalItem.ModifyWeaponDamage"/>
    public virtual void ModifyWeaponDamage(Player player, ref StatModifier damage) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponKnockback"/>
    public virtual void ModifyWeaponKnockback(Player player, ref StatModifier knockback) { }

    /// <inheritdoc cref="GlobalItem.ModifyWeaponCrit"/>
    public virtual void ModifyWeaponCrit(Player player, ref float crit) { }

    /// <inheritdoc cref="GlobalItem.ModifyShootStats"/>
    public virtual void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }

    /// <inheritdoc cref="GlobalItem.ModifyItemScale"/>
    public virtual void ModifyItemScale(Player player, ref float scale) { }
    #endregion ModifyStats

    #region Hit
    /// <inheritdoc cref="GlobalItem.CanHitNPC"/>
    public virtual bool? CanHitNPC(Player player, NPC target) => null;

    /// <inheritdoc cref="GlobalItem.CanMeleeAttackCollideWithNPC"/>
    public virtual bool? CanMeleeAttackCollideWithNPC(Rectangle meleeAttackHitbox, Player player, NPC target) => null;

    /// <inheritdoc cref="GlobalItem.ModifyHitNPC"/>
    public virtual void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) { }

    /// <inheritdoc cref="GlobalItem.OnHitNPC"/>
    public virtual void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) { }

    /// <inheritdoc cref="GlobalItem.CanHitPvp"/>
    public virtual bool CanHitPvp(Player player, Player target) => true;

    /// <inheritdoc cref="GlobalItem.ModifyHitPvp"/>
    public virtual void ModifyHitPvp(Player player, Player target, ref Player.HurtModifiers modifiers) { }

    /// <inheritdoc cref="GlobalItem.OnHitPvp"/>
    public virtual void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) { }
    #endregion Hit

    #region SpecialEffects
    /// <inheritdoc cref="GlobalItem.GetHealLife"/>
    public virtual void GetHealLife(Player player, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="GlobalItem.GetHealMana"/>
    public virtual void GetHealMana(Player player, bool quickHeal, ref int healValue) { }

    /// <inheritdoc cref="GlobalItem.ModifyManaCost"/>
    public virtual void ModifyManaCost(Player player, ref float reduce, ref float mult) { }

    /// <inheritdoc cref="GlobalItem.OnMissingMana"/>
    public virtual void OnMissingMana(Player player, int neededMana) { }

    /// <inheritdoc cref="GlobalItem.OnConsumeMana"/>
    public virtual void OnConsumeMana(Player player, int manaConsumed) { }

    /// <inheritdoc cref="GlobalItem.CanResearch"/>
    public virtual bool CanResearch() => true;

    /// <inheritdoc cref="GlobalItem.OnResearched"/>
    public virtual void OnResearched(bool fullyResearched) { }

    /// <inheritdoc cref="GlobalItem.ModifyResearchSorting"/>
    public virtual void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) { }

    /// <inheritdoc cref="GlobalItem.NeedsAmmo"/>
    public virtual bool NeedsAmmo(Player player) => true;

    /// <inheritdoc cref="GlobalItem.UseItemHitbox"/>
    public virtual void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) { }

    /// <inheritdoc cref="GlobalItem.MeleeEffects"/>
    public virtual void MeleeEffects(Player player, Rectangle hitbox) { }

    /// <inheritdoc cref="GlobalItem.CanCatchNPC"/>
    public virtual bool? CanCatchNPC(NPC target, Player player) => null;

    /// <inheritdoc cref="GlobalItem.OnCatchNPC"/>
    public virtual void OnCatchNPC(NPC npc, Player player, bool failed) { }

    /// <inheritdoc cref="GlobalItem.ConsumeItem"/>
    public virtual bool ConsumeItem(Player player) => true;

    /// <inheritdoc cref="GlobalItem.OnConsumeItem"/>
    public virtual void OnConsumeItem(Player player) { }

    /// <inheritdoc cref="GlobalItem.UseItemFrame"/>
    public virtual void UseItemFrame(Player player) { }

    /// <inheritdoc cref="GlobalItem.HoldItemFrame"/>
    public virtual void HoldItemFrame(Player player) { }

    /// <inheritdoc cref="GlobalItem.VerticalWingSpeeds"/>
    public virtual void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
        ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    { }

    /// <inheritdoc cref="GlobalItem.HorizontalWingSpeeds"/>
    public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) { }

    /// <inheritdoc cref="GlobalItem.CanEquipAccessory"/>
    public virtual bool CanEquipAccessory(Player player, int slot, bool modded) => true;
    #endregion SpecialEffects

    #region Tooltip
    /// <inheritdoc cref="GlobalItem.PreDrawTooltip"/>
    public virtual bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawTooltip"/>
    public virtual void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines) { }

    /// <inheritdoc cref="GlobalItem.PreDrawTooltipLine"/>
    public virtual bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) => true;

    /// <inheritdoc cref="GlobalItem.PostDrawTooltipLine"/>
    public virtual void PostDrawTooltipLine(DrawableTooltipLine line) { }

    /// <inheritdoc cref="GlobalItem.ModifyTooltips"/>
    public virtual void ModifyTooltips(List<TooltipLine> tooltips) { }
    #endregion Tooltip

    #region WorldSaving
    /// <inheritdoc cref="GlobalItem.SaveData"/>
    public virtual void SaveData(TagCompound tag) { }

    /// <inheritdoc cref="GlobalItem.LoadData"/>
    public virtual void LoadData(TagCompound tag) { }
    #endregion WorldSaving

    #region Net
    /// <inheritdoc cref="GlobalItem.NetSend"/>
    public virtual void NetSend(BinaryWriter writer) { }

    /// <inheritdoc cref="GlobalItem.NetReceive"/>
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
        foreach (SimpleEntityBehaviorSet<NPC, TNPCBehavior> simpleSet in BehaviorSet._data.Values)
        {
            foreach (TNPCBehavior npcBehavior in simpleSet.GetBehaviors())
                npcBehavior.SetStaticDefaults();
        }
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

    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
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
        foreach (SimpleEntityBehaviorSet<Projectile, TProjectileBehavior> simpleSet in BehaviorSet._data.Values)
        {
            foreach (TProjectileBehavior projectileBehavior in simpleSet.GetBehaviors())
                projectileBehavior.SetStaticDefaults();
        }
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

    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        if (TryGetBehavior(projectile, out TProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.CanHitPvp(target))
                return false;
        }
        return true;
    }

    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
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
        foreach (SimpleEntityBehaviorSet<Item, TItemBehavior> simpleSet in BehaviorSet._data.Values)
        {
            foreach (TItemBehavior itemBehavior in simpleSet.GetBehaviors())
                itemBehavior.SetStaticDefaults();
        }
    }

    public override void SetDefaults(Item item)
    {
        if (TryGetBehavior(item, out TItemBehavior itemBehavior))
            itemBehavior.SetDefaults();
    }

    public override void AddRecipes()
    {
        foreach (SimpleEntityBehaviorSet<Item, TItemBehavior> simpleSet in BehaviorSet._data.Values)
        {
            foreach (TItemBehavior itemBehavior in simpleSet.GetBehaviors())
                itemBehavior.AddRecipes();
        }
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
    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
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

    [Obsolete("Poor performance.", true)]
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
    public static readonly GeneralEntityBehaviorSet<Player, PlayerBehavior> BehaviorSet = new();

    public override void SetStaticDefaults()
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors())
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
        if (TOMain.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.CopyClientState(targetCopy);
        }
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (TOMain.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.SyncPlayer(toWho, fromWho, newPlayer);
        }
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        if (TOMain.SyncEnabled)
        {
            //foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            //    behavior.SendClientChanges(clientPlayer);
        }
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

    /*
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
    }*/

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

    /*
    public override bool? CanHitNPCWithItem(Item item, NPC target)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanHitNPCWithItem(item, target);
            if (result is not null)
                return result;
        }
        return null;
    }*/

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

    /*
    public override bool? CanHitNPCWithProj(Projectile proj, NPC target)
    {
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            bool? result = behavior.CanHitNPCWithProj(proj, target);
            if (result is not null)
                return result;
        }
        return null;
    }*/

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

    /*
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
    }*/

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

    /*
    public override bool CanBeHitByProjectile(Projectile proj)
    {
        bool result = true;
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
            result &= behavior.CanBeHitByProjectile(proj);
        return result;
    }*/

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

    public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        List<Item> allItems = [];
        foreach (PlayerBehavior behavior in BehaviorSet.GetBehaviors(Player))
        {
            IEnumerable<Item> items = behavior.AddMaterialsForCrafting(out ItemConsumedCallback callback);
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

    public static readonly GlobalEntityBehaviorSet<NPC, GlobalNPCBehavior> BehaviorSet = new();

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
        if (TOMain.SyncEnabled)
        {
            //foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.SendExtraAI(npc, bitWriter, binaryWriter);
        }
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (TOMain.SyncEnabled)
        {
            //foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.ReceiveExtraAI(npc, bitReader, binaryReader);
        }
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

    /*
    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        bool result = true;
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPlayer(npc, target, ref cooldownSlot);
        return result;
    }*/

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

    /*
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
    }*/

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

    /*
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
    }*/

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

    /*
    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        foreach (GlobalNPCBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanBeHitByProjectile(npc, projectile);
            if (result is not null)
                return result;
        }
        return null;
    }*/

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

    public static readonly GlobalEntityBehaviorSet<Projectile, GlobalProjectileBehavior> BehaviorSet = new();

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
        if (TOMain.SyncEnabled)
        {
            //foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.SendExtraAI(projectile, bitWriter, binaryWriter);
        }
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        if (TOMain.SyncEnabled)
        {
            //foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.ReceiveExtraAI(projectile, bitReader, binaryReader);
        }
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
        bool result = false;
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            result |= behavior.MinionContactDamage(projectile);
        return result;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
            behavior.ModifyDamageHitbox(projectile, ref hitbox);
    }

    /*
    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.CanHitNPC(projectile, target);
            if (result is not null)
                return result;
        }
        return null;
    }*/

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

    /*
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
    }*/

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

    /*
    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
    {
        foreach (GlobalProjectileBehavior behavior in BehaviorSet.GetBehaviors())
        {
            bool? result = behavior.Colliding(projectile, projHitbox, targetHitbox);
            if (result is not null)
                return result;
        }
        return null;
    }*/

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

    public static readonly GlobalEntityBehaviorSet<Item, GlobalItemBehavior> BehaviorSet = new();

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

    /*
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
    }*/

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

    /*
    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        bool result = true;
        foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            result &= behavior.CanHitPvp(item, player, target);
        return result;
    }*/

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
        if (TOMain.SyncEnabled)
        {
            //foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.NetSend(item, writer);
        }
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        if (TOMain.SyncEnabled)
        {
            //foreach (GlobalItemBehavior behavior in BehaviorSet.GetBehaviors())
            //    behavior.NetReceive(item, reader);
        }
    }

    void IResourceLoader.PostSetupContent() => BehaviorSet.FillSet();

    void IResourceLoader.OnModUnload() => BehaviorSet.Clear();
}
#endregion General Behavior Handler