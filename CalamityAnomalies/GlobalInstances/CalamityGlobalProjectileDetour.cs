namespace CalamityAnomalies.GlobalInstances;

public sealed class CalamityGlobalProjectileDetour : GlobalProjectileDetour<CalamityGlobalProjectile>
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.PreAI)))
        {
            if (!projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreAI))
                return true;
        }

        return orig(self, projectile);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalProjectile self, Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.GetAlpha)))
        {
            if (!projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.GetAlpha))
                return null;
        }

        if (projectile.Anomaly().NeverTrippy)
        {
            CalamityPlayer calamityPlayer = Main.LocalPlayer.Calamity();
            bool temp = calamityPlayer.trippy;
            calamityPlayer.trippy = false;
            Color? result = orig(self, projectile, lightColor);
            calamityPlayer.trippy = temp;
            return result;
        }

        return orig(self, projectile, lightColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalProjectile self, Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.PreDraw)))
        {
            if (!projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreDraw))
                return true;
        }

        return orig(self, projectile, ref lightColor);
    }
}
