using CalamityAnomalies.Assets.Textures;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRuby_Anomaly : AnomalyNPCBehavior<KingSlimeJewelRuby>
{
    public static class Data
    {
        public const float DespawnDistance = 5000f;
        public static int ShootCooldownTime => 75;
    }

    public override bool PreAI()
    {
        if (!OceanNPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
            return false;
        }

        Lighting.AddLight(NPC.Center, 1f, 0f, 0f);

        if (!NPC.TargetClosestIfInvalid(true, Data.DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return false;
        }

        JewelHandler.Movement(NPC, Target.Center, 7.5f, 7.5f, 0.125f, 100f, -100f, -400f, -500f);

        Timer1++;
        if (Timer1 >= Data.ShootCooldownTime)
        {
            Timer1 = 0;
            Shoot();
            for (int i = 0; i < 20; i++)
                JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
        }

        NPC.netUpdate = true;

        return false;

        void Shoot()
        {
            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            if (!TOWorld.GeneralClient)
                return;

            float radian = MathHelper.PiOver4;
            float singleRadian = radian / 4f;
            float initialRotation = (Target.Center - NPC.Center).ToRotation() - singleRadian * 2f;
            Projectile.RotatedProj<JewelProjectile>(5, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(15f, initialRotation), NPC.GetProjectileDamage<JewelProjectile>(), 0f, Main.myPlayer);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float timeLeftGateValue = 30f;
        float gateValue = Data.ShootCooldownTime - timeLeftGateValue;
        float ratio = Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        if (CAClientConfig.Instance.AuxiliaryVisualEffects && ratio > 0f)
        {
            float interpolation = TOMathHelper.ParabolicInterpolation(1f - ratio);
            float interpolation2 = TOMathHelper.ParabolicInterpolation(Math.Clamp(1f - ratio, 0f, 0.2f) * 5f);
            for (int i = 0; i < 300; i++)
                spriteBatch.DrawFromCenter(CalamityTextureHandler.GlowOrbParticle, NPC.Center + new PolarVector2(interpolation * 80f, MathHelper.TwoPi / 200 * i) - screenPos, Color.Red with { A = 0 } * ratio * 1.5f, null, 0f, 0.25f * interpolation2);
        }
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, Main.zenithWorld ? Color.Cyan : Color.Red, Main.zenithWorld ? new Color(175, 255, 255) : new Color(255, 175, 175), ratio);
        return false;
    }
}
