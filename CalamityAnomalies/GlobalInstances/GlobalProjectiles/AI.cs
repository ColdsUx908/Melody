using CalamityAnomalies.Override;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalProjectiles;

public partial class CAGlobalProjectile : GlobalProjectile
{
    public override bool PreAI(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.PreAI())
                return false;
        }

        return true;
    }

    public override void AI(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.AI();
    }

    public override void PostAI(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.PostAI();
    }
}
