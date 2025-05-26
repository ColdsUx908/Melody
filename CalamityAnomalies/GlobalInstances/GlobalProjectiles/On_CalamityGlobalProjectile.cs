using CalamityAnomalies.Override;
using CalamityMod.Events;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Transoceanic.Core.IL;

namespace CalamityAnomalies.GlobalInstances.GlobalProjectiles;

[TODetour(typeof(CalamityGlobalProjectile))]
public class On_CalamityGlobalProjectile
{
    internal delegate void Orig_SetDefaults(CalamityGlobalProjectile self, Projectile projectile);

    internal static void Detour_SetDefaults(Orig_SetDefaults orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = false;
            orig(self, projectile);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, projectile);
    }

    internal delegate bool Orig_PreAI(CalamityGlobalProjectile self, Projectile projectile);

    internal static bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreAI))
                return true;
        }

        if (CAWorld.BossRush)
            BossRushEvent.BossRushActive = true;

        return orig(self, projectile);
    }

    internal delegate void Orig_PostAI(CalamityGlobalProjectile self, Projectile projectile);

    internal static void Detour_PostAI(Orig_PostAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        orig(self, projectile);
        BossRushEvent.BossRushActive = CAWorld.RealBossRushEventActive;
    }

    internal delegate Color? Orig_GetAlpha(CalamityGlobalProjectile self, Projectile projectile, Color lightColor);

    internal static Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalProjectile self, Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.GetAlpha))
                return null;
        }

        return orig(self, projectile, lightColor);
    }

    internal delegate bool Orig_PreDraw(CalamityGlobalProjectile self, Projectile projectile, ref Color lightColor);

    internal static bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalProjectile self, Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreDraw))
                return true;
        }

        return orig(self, projectile, ref lightColor);
    }
}
