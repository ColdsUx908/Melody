using System.Collections.Generic;
using CalamityAnomalies.Override;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalProjectiles;

public partial class CAGlobalProjectile : GlobalProjectile
{
    public override Color? GetAlpha(Projectile projectile, Color lightColor)
    {
        if (projectile.HasProjectileOverride(out CAProjectileOverride projectileOverride))
            return projectileOverride.GetAlpha(lightColor);

        return null;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        if (projectile.HasProjectileOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.PreDrawExtras())
                return false;
        }

        return true;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (projectile.HasProjectileOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.PreDraw(ref lightColor))
                return false;
        }

        return true;
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (projectile.HasProjectileOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.PostDraw(lightColor);
    }

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (projectile.HasProjectileOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
}
