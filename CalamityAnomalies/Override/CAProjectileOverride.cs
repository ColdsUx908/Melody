using System.Collections.Generic;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.GlobalInstances.GlobalProjectiles;
using CalamityMod;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Transoceanic;
using Transoceanic.Core.ExtraGameData;
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
        get => field ?? (Projectile.whoAmI < Main.maxProjectiles ? field = Projectile.Ocean() : Projectile.Ocean());
        set;
    } = null;

    protected CAGlobalProjectile AnomalyProjectile
    {
        get => field ?? (Projectile.whoAmI < Main.maxProjectiles ? field = Projectile.Anomaly() : Projectile.Anomaly());
        set;
    } = null;

    protected CalamityGlobalProjectile CalamityProjectile
    {
        get => field ?? (Projectile.whoAmI < Main.maxProjectiles ? field = Projectile.Calamity() : Projectile.Calamity());
        set;
    } = null;

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

public abstract class AnomalyProjectileOverride : CAProjectileOverride
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}
