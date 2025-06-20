using CalamityMod.Events;

namespace CalamityAnomalies.GlobalInstances;

public class CalamityGlobalProjectileDetour : GlobalProjectileDetour<CalamityGlobalProjectile>
{
    public override void Detour_SetDefaults(Orig_SetDefaults orig, CalamityGlobalProjectile self, Projectile projectile)
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

    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileBehavior projectileOverride))
        {
            if (!projectileOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreAI))
                return true;
        }

        if (CAWorld.BossRush)
            BossRushEvent.BossRushActive = true;

        return orig(self, projectile);
    }

    public override void Detour_PostAI(Orig_PostAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        orig(self, projectile);
        BossRushEvent.BossRushActive = CAWorld.RealBossRushEventActive;
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalProjectile self, Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileBehavior projectileOverride))
        {
            if (!projectileOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.GetAlpha))
                return null;
        }

        return orig(self, projectile, lightColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalProjectile self, Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileBehavior projectileOverride))
        {
            if (!projectileOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreDraw))
                return true;
        }

        return orig(self, projectile, ref lightColor);
    }
}
