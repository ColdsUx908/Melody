using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Transoceanic.GameData.Utilities;
using Transoceanic.GlobalInstances;
using Transoceanic.GlobalInstances.GlobalItems;
using Transoceanic.GlobalInstances.GlobalNPCs;
using Transoceanic.GlobalInstances.GlobalProjectiles;
using Transoceanic.IL;

namespace Transoceanic.ExtraGameData;

public abstract class EntityOverride<TEntity> where TEntity : Entity
{
    public abstract int OverrideType { get; }

    /// <summary>
    /// 优先级，越大越先应用。
    /// </summary>
    public virtual decimal Priority { get; } = 0m;

    public virtual bool ShouldProcess => true;

    public abstract void Connect(TEntity entity);

    public abstract void Disconnect();

    public virtual void SetStaticDefaults() { }

    public virtual void SetDefaults() { }
}

public class EntityOverrideDictionary<TEntity, TOverride> : Dictionary<int, List<(TOverride overrideInstance, HashSet<string> overridenMethods)>>
    where TEntity : Entity
    where TOverride : EntityOverride<TEntity>
{
    public new IEnumerable<TOverride> Values =>
        from overrides in base.Values
        from temp in overrides
        select temp.overrideInstance;

    /// <summary>
    /// 尝试获取指定实体的Override实例。
    /// <br/>按照 <see cref="EntityOverride{TEntity}.Priority"/> 降序依次尝试获取通过 <see cref="EntityOverride{TEntity}.ShouldProcess"/> 检测和方法名检测的Override实例。
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="methodName"></param>
    /// <param name="overrideInstance"></param>
    /// <returns></returns>
    public bool TryGetOverride(TEntity entity, string methodName, [NotNullWhen(true)] out TOverride overrideInstance)
    {
        if (TryGetValue(entity.GetEntityType(), out List<(TOverride overrideInstance, HashSet<string> overridenMethods)> overrideList))
        {
            foreach ((TOverride temp, HashSet<string> overridenMethods) in overrideList)
            {
                if (overridenMethods.Contains(methodName))
                {
                    temp.Disconnect();
                    temp.Connect(entity);
                    if (temp.ShouldProcess)
                    {
                        overrideInstance = temp;
                        return true;
                    }
                }
            }
        }
        overrideInstance = null;
        return false;
    }

    public void FillOverrides(Assembly assemblyToSearch)
    {
        Clear();
        foreach ((int type, IEnumerable<(TOverride overrideInstance, HashSet<string> overridenMethods)> overrides) in
            TOReflectionUtils.GetTypeInstancesDerivedFrom<TOverride>(assemblyToSearch)
            .GroupBy(k => k.OverrideType)
            .ToDictionary(
                keySelector: k => k.Key,
                elementSelector: k =>
                    from overrideInstance in k
                    orderby overrideInstance.Priority
                    select (overrideInstance, overrideInstance.GetType().GetOverridenMethods(TOReflectionUtils.UniversalBindingFlags).Select(k => k.Name).ToHashSet())))
        {
            this[type] = [.. overrides];
        }
    }
}

public abstract class NPCOverride : EntityOverride<NPC>
{
    #region 实成员
    public NPC NPC { get; private set; } = null;

    public TOGlobalNPC OceanNPC { get; private set; } = null;

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
    }

    public override void Disconnect()
    {
        NPC = null;
        OceanNPC = null;
    }
    #endregion

    #region 虚成员
    /// <summary>
    /// NPC是否应用Boss无敌帧。
    /// <br/>默认返回 <see langword="null"/>，即沿用默认设置。
    /// </summary>
    public virtual bool? UseBossImmunityCooldownID => null;

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

public abstract class ProjectileOverride : EntityOverride<Projectile>
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

    public override void Disconnect()
    {
        Projectile = null;
        OceanProjectile = null;
    }

    #endregion

    #region 虚成员
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
    #endregion

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
    #endregion

    #region Draw
    /// <summary>
    /// Allows you to determine the color and transparency in which a projectile is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
    /// </summary>
    /// <param name="projectile"></param>
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
    /// <param name="projectile"> The projectile. </param>
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
    #endregion
    #endregion
}

public abstract class ItemOverride : EntityOverride<Item>
{
    #region 实成员
    public Item Item { get; private set; } = null;

    public TOGlobalItem OceanItem { get; private set; } = null;

    public override void Connect(Item item)
    {
        Item = item;
        OceanItem = item.Ocean();
    }

    public override void Disconnect()
    {
        Item = null;
        OceanItem = null;
    }
    #endregion

    #region 虚成员
    #region Defaults
    /// <summary>
    /// Override this method to add <see cref="Recipe"/>s to the game.<br/>
    /// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Recipes">Basic Recipes Guide</see> teaches how to add new recipes to the game and how to manipulate existing recipes.<br/>
    /// </summary>
    public virtual void AddRecipes() { }
    #endregion

    #region Active
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
    #endregion

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
    #endregion

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
    public virtual bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

    /// <summary>
    /// <inheritdoc cref="ModItem.PostDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/>
    /// </summary>
    public virtual void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor, Vector2 origin, float scale)
    { }
    #endregion

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
    #endregion

    #region Use
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
    #endregion

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
    #endregion

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
    #endregion

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
    #endregion

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
    #endregion
    #endregion
}