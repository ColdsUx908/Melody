using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.GlobalInstances.GlobalNPCs;
using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core.ExtraGameData;
using Transoceanic.Core.GameData.Utilities;
using Transoceanic.GlobalInstances;
using Transoceanic.GlobalInstances.GlobalNPCs;

namespace CalamityAnomalies.Override;

public enum OrigMethodType_CalamityGlobalNPC
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CANPCOverride : EntityOverride<NPC>
{
    #region 实成员
    protected NPC NPC
    {
        get => field ?? TOMain.DummyNPC;
        set;
    } = null;

    protected TOGlobalNPC OceanNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? (field = NPC.Ocean()) : NPC.Ocean());
        set;
    } = null;

    protected CAGlobalNPC AnomalyNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? (field = NPC.Anomaly()) : NPC.Anomaly());
        set;
    } = null;

    protected CalamityGlobalNPC CalamityNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? (field = NPC.Calamity()) : NPC.Calamity());
        set;
    } = null;

    public Player Target { get; private set; } = null;

    public bool TargetClosestIfInvalid(bool faceTarget = true, float distanceThreshold = 4000f)
    {
        bool result = NPC.TargetClosestIfInvalid(faceTarget, distanceThreshold);
        Target = result ? Main.player[NPC.target] : null;
        return result;
    }

    public override void Connect(NPC npc)
    {
        NPC = npc;
        OceanNPC = npc.Ocean();
        AnomalyNPC = npc.Anomaly();
        CalamityNPC = npc.Calamity();
    }

    public override void Disconnect()
    {
        NPC = null;
        OceanNPC = null;
        AnomalyNPC = null;
        CalamityNPC = null;
    }

    public int AI_Timer1
    {
        get => (int)AnomalyNPC.AnomalyAI[^3];
        set => AnomalyNPC.SetAnomalyAI(value, ^3);
    }

    public int AI_Timer2
    {
        get => (int)AnomalyNPC.AnomalyAI[^2];
        set => AnomalyNPC.SetAnomalyAI(value, ^2);
    }

    public int AI_Timer3
    {
        get => (int)AnomalyNPC.AnomalyAI[^1];
        set => AnomalyNPC.SetAnomalyAI(value, ^1);
    }
    #endregion

    #region 虚成员
    /// <summary>
    /// NPC是否应用Boss无敌帧。
    /// <br/>默认返回 <see langword="null"/>，即沿用默认设置。
    /// </summary>
    public virtual bool? UseBossImmunityCooldownID => null;

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC type) => true;

    #region Defaults
    /// <summary>
    /// 设置负数type的NPC的额外属性。
    /// </summary>
    public virtual void SetDefaultsFromNetId() { }
    #endregion

    #region Active
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
    #endregion

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
    #endregion

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
    /// 在绘制灾厄的Boss血条之前调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawCalBossBar(On_BossHealthBarManager.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawCalBossBar(On_BossHealthBarManager.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }
    #endregion

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
    #endregion
    #endregion
}

public abstract class CANPCOverride<T> : CANPCOverride where T : ModNPC
{
    protected T ModNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? (field = NPC.GetModNPC<T>()) : NPC.GetModNPC<T>());
        set;
    }

    public override int OverrideType => ModContent.NPCType<T>();

    public override void Connect(NPC npc)
    {
        base.Connect(npc);
        ModNPC = npc.GetModNPC<T>();
    }

    public override void Disconnect()
    {
        base.Disconnect();
        ModNPC = null;
    }
}

public abstract class AnomalyNPCOverride : CANPCOverride
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class AnomalyNPCOverride<T> : CANPCOverride<T> where T : ModNPC
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}