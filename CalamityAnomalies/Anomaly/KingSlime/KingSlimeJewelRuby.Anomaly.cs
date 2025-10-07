using CalamityAnomalies.Assets.Textures;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRuby_Anomaly : AnomalyNPCBehavior<KingSlimeJewelRuby>
{
    public static class Data
    {
        public const float DespawnDistance = 5000f;
        public static int ShootCooldownTime => Main.zenithWorld ? 150 : 75;
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
            int particleAmount = Main.zenithWorld ? 30 : 20;
            for (int i = 0; i < particleAmount; i++)
                JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
        }

        NPC.netUpdate = true;

        return false;

        void Shoot()
        {
            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            if (!TOWorld.GeneralClient)
                return;

            int amount = Main.zenithWorld ? 13 : 5;
            float radian = Main.zenithWorld ? MathHelper.PiOver4 * 3f : MathHelper.PiOver4;
            float singleRadian = radian / (amount - 1);
            float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
            Projectile.RotatedProj<JewelProjectile>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(Main.zenithWorld ? 18f : 15f, initialRotation), NPC.GetProjectileDamage<JewelProjectile>(), 0f, Main.myPlayer);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float timeLeftGateValue = 30f;
        float gateValue = Data.ShootCooldownTime - timeLeftGateValue;
        float ratio = Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        if (CAClientConfig.Instance.AuxiliaryVisualEffects && ratio > 0f)
            JewelHandler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, Main.zenithWorld ? 120f : 100f, 0.35f);
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
        return false;
    }
}
