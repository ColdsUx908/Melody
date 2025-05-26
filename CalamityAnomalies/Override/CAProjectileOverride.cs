using System.Collections.Generic;
using CalamityAnomalies.Configs;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.GlobalInstances.GlobalProjectiles;
using CalamityMod;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core.ExtraGameData;
using Transoceanic.Core.GameData.Utilities;
using Transoceanic.GlobalInstances;
using Transoceanic.GlobalInstances.GlobalProjectiles;

namespace CalamityAnomalies.Override;

public enum OrigMethodType_CalamityGlobalProjectile
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CAProjectileOverride : EntityOverride<Projectile>
{
    #region 实成员
    protected Projectile Projectile
    {
        get => field ?? TOMain.DummyProjectile;
        set;
    } = null;

    protected TOGlobalProjectile OceanProjectile
    {
        get => field ?? (Projectile.whoAmI < Main.maxProjectiles ? (field = Projectile.Ocean()) : Projectile.Ocean());
        set;
    } = null;

    protected CAGlobalProjectile AnomalyProjectile
    {
        get => field ?? (Projectile.whoAmI < Main.maxProjectiles ? (field = Projectile.Anomaly()    ) : Projectile.Anomaly());
        set;
    } = null;

    protected CalamityGlobalProjectile CalamityProjectile
    {
        get => field ?? (Projectile.whoAmI < Main.maxProjectiles ? (field = Projectile.Calamity()) : Projectile.Calamity());
        set;
    } = null;

    public Player Owner => Main.player[Projectile.owner];

    public override void Connect(Projectile projectile)
    {
        Projectile = projectile;
        OceanProjectile = projectile.Ocean();
        AnomalyProjectile = projectile.Anomaly();
        CalamityProjectile = projectile.Calamity();
    }

    public override void Disconnect()
    {
        Projectile = null;
        OceanProjectile = null;
        AnomalyProjectile = null;
        CalamityProjectile = null;
    }

    #endregion

    #region 虚成员
    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile type) => true;

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

public abstract class CAProjectileOverride<T> : CAProjectileOverride where T : ModProjectile
{
    protected T ModProjectile
    {
        get => field ?? (Projectile.whoAmI < Main.maxProjectiles ? (field = Projectile.GetModProjectile<T>()) : Projectile.GetModProjectile<T>());
        set;
    } = null;

    public override int OverrideType => ModContent.ProjectileType<T>();

    public override void Connect(Projectile projectile)
    {
        base.Connect(projectile);
        ModProjectile = projectile.ModProjectile<T>();
    }

    public override void Disconnect()
    {
        base.Disconnect();
        ModProjectile = null;
    }
}

public abstract class AnomalyProjectileOverride : CAProjectileOverride
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class AnomalyProjecileOverride<T> : CAProjectileOverride<T> where T : ModProjectile
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class CAProjectileTweak : CAProjectileOverride
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.TweaksEnabled;
}

public abstract class CAProjectileTweak<T> : CAProjectileOverride<T> where T : ModProjectile
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.TweaksEnabled;
}
