using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Skies;
using static CalamityMod.Projectiles.Boss.SCalRitualDrama;

namespace CalamityAnomalies.Tweaks._5_2_PostYharon;

public sealed class ScalRitualDrama_Tweak : CAProjectileTweak<SCalRitualDrama>
{
    public override bool PreAI()
    {
        CalamityPlayer calamityPlayer = Main.LocalPlayer.Calamity();

        if (Projectile.timeLeft == 689)
        {
            for (int i = 0; i < 2; i++)
                GeneralParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Red, Color.Magenta, 0.3f), 0f, 0.55f, 270, false));
            GeneralParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 0f, 0.5f, 270, false));
        }

        if (Projectile.timeLeft == 689 - 180)
            GeneralParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, new Color(121, 21, 77), 0f, 0.85f, 90, false));

        // If needed, these effects may continue after the ritual timer, to ensure that there are no awkward
        // background changes between the time it takes for SCal to appear after this projectile is gone.
        // If SCal is already present, this does not happen.
        if (!NPC.AnyNPCs<SupremeCalamitas>())
        {
            SCalSky.OverridingIntensity = Utils.GetLerpValue(90f, TotalRitualTime - 25f, ModProjectile.Time, true);
            calamityPlayer.GeneralScreenShakePower = Utils.GetLerpValue(90f, TotalRitualTime - 25f, ModProjectile.Time, true);
            calamityPlayer.GeneralScreenShakePower *= Utils.GetLerpValue(3400f, 1560f, Main.LocalPlayer.Distance(Projectile.Center), true) * 4f;
        }

        // Summon SCal right before the ritual effect ends.
        // The projectile lingers a little longer, however, to ensure that desync delays in MP do not interfere with the background transition.
        if (ModProjectile.Time == TotalRitualTime - 1f)
            ModProjectile.SummonSCal();

        if (ModProjectile.Time >= TotalRitualTime)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.AnyNPCs<SupremeCalamitas>())
                Projectile.Kill();
            return false;
        }

        int fireReleaseRate = ModProjectile.Time > 150f ? 2 : 1;
        for (int i = 0; i < fireReleaseRate; i++)
        {
            if (Main.rand.NextBool())
            {
                float variance = Main.rand.NextFloat(-25f, 25f);
                Dust.NewDustPerfectAction(Projectile.Center + new Vector2(variance, 20), DustID.RainbowMk2, d =>
                {
                    d.velocity = new PolarVector2(ModProjectile.Time * 0.023f * Main.rand.NextFloat(1.1f, 2.1f), variance * 0.02f - MathHelper.PiOver2);
                    d.color = Main.rand.NextBool() ? Color.Red : new Color(121, 21, 77);
                    d.scale = Main.rand.NextFloat(0.35f, 1.2f);
                    d.fadeIn = 0.7f;
                    d.noGravity = true;
                });
            }
        }

        ModProjectile.Time++;

        return false;
    }
}
