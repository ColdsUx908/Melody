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
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            Color? result = projectileOverride.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.PreDrawExtras())
                return false;
        }

        return true;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.PreDraw(ref lightColor))
                return false;
        }

        return true;
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.PostDraw(lightColor);
    }

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
}
