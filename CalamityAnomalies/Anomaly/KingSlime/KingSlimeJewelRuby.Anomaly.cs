using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRuby_Anomaly : AnomalyNPCBehavior<KingSlimeJewelRuby>
{
    public const float DespawnDistance = 5000f;
    public int ShootCooldownTime => HasEnteredPhase2 ? (Main.zenithWorld ? 240 : 120) : (Main.zenithWorld ? 180 : 90);

    public bool HasEnteredPhase2
    {
        get => NPC.ai[2] == 1f;
        set => NPC.ai[2] = value.ToInt();
    }

    public bool CanAttack
    {
        get => NPC.ai[3] != 1f;
        set => NPC.ai[3] = (!value).ToInt();
    }

    public override void SetDefaults()
    {
        NPC.width = 28;
        NPC.height = 28;
    }

    public override bool PreAI()
    {
        if (!OceanNPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
            return false;
        }

        Lighting.AddLight(NPC.Center, 1f, 0f, 0f);

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return false;
        }

        JewelHandler.Movement(NPC, Target.Center, 15f, 15f, 0.175f, 250f, -250f, -300f, -500f);

        if (CanAttack)
            Timer1++;
        if (Timer1 >= ShootCooldownTime)
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

            int amount = HasEnteredPhase2 ? (Main.zenithWorld ? 7 : 3) : (Main.zenithWorld ? 11 : 5);
            float singleRadian = MathHelper.ToRadians(HasEnteredPhase2 ? (Main.zenithWorld ? 22.5f : CAWorld.AnomalyUltramundane ? 5f : 13.5f) : (Main.zenithWorld ? 18f : CAWorld.AnomalyUltramundane ? 13.5f : 12f));
            float radian = singleRadian * (amount - 1);
            float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
            Projectile.RotatedProj<JewelProjectile>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(Main.zenithWorld ? 18f : 15f, initialRotation), NPC.GetProjectileDamage<JewelProjectile>(), 0f, Main.myPlayer, p =>
            {
                if (Main.zenithWorld)
                {
                    p.velocity.Modulus *= Main.rand.NextFloat(0.7f, TOWorld.LegendaryMode ? 1f : 0.85f);
                    if (TOWorld.LegendaryMode)
                        p.velocity.Rotation += Main.rand.NextFloat(-0.15f, 0.15f);
                }
            });
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float timeLeftGateValue = 30f;
        float gateValue = ShootCooldownTime - timeLeftGateValue;
        float ratio = Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        if (CAClientConfig.Instance.AuxiliaryVisualEffects && ratio > 0f)
            JewelHandler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, Main.zenithWorld ? 120f : 100f, 0.35f);
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
        return false;
    }

    public override bool CheckDead()
    {
        if (CAWorld.AnomalyUltramundane)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                JewelHandler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }
}
