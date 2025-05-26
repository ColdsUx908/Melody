using System;
using CalamityAnomalies.Override;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalProjectiles;

public partial class CAGlobalProjectile : GlobalProjectile
{
    public override void SetStaticDefaults()
    {
        foreach (CAProjectileOverride projectileOverride in CAOverrideHelper.ProjectileOverrides.Values)
            projectileOverride.SetStaticDefaults();
    }

    public override void SetDefaults(Projectile projectile)
    {
        Array.Fill(AnomalyAI, 0f);

        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.SetDefaults();
    }
}
