using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRuby_Anomaly : KingSlimeJewel_Anomaly<KingSlimeJewelRuby>
{
    public int ShootCooldownTime => HasEnteredPhase2 ? (Main.zenithWorld ? 240 : 120) : (Main.zenithWorld ? 180 : 90);

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

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return false;
        }

        NPC.damage = 0;
        Lighting.AddLight(NPC.Center, 1f, 0f, 0f);

        if (CanAttack)
        {
            JewelHandler.Move(NPC, Target.Center, 15f, 15f, 0.175f, 0.125f, 250f, -250f, -250f, -400f);
            Timer1++;
        }
        else
        {
            JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.175f, 150f, -150f, 0f, -200f);
            Timer1 -= 2;
        }

        if (Timer1 >= ShootCooldownTime)
        {
            Timer1 = 0;
            Shoot();
        }

        NPC.netUpdate = true;

        return false;

        void Shoot()
        {
            KingSlime_Anomaly kingSlimeBehavior = new() { _entity = master };
            bool validSapphire = !HasEnteredPhase2 && kingSlimeBehavior.JewelSapphireAlive;
            NPC sapphire = validSapphire ? kingSlimeBehavior.JewelSapphire : null;

            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            int particleAmount = Main.zenithWorld ? 30 : 20;
            if (validSapphire)
                particleAmount += 20;
            for (int i = 0; i < particleAmount; i++)
                JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));

            if (!TOWorld.GeneralClient)
                return;

            int amount = HasEnteredPhase2 ? (Main.zenithWorld ? 7 : 3) : (Main.zenithWorld ? 17 : 5);
            float singleRadian = MathHelper.ToRadians(HasEnteredPhase2 ? (Main.zenithWorld ? 18f : 10f) : (Main.zenithWorld ? 18f : CAWorld.AnomalyUltramundane ? 13.5f : 12f));
            float radian = singleRadian * (amount - 1);
            float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
            Projectile.RotatedProj<JewelProjectile>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(Main.zenithWorld ? 16f : 15f, initialRotation), NPC.GetProjectileDamage<JewelProjectile>(), 0f, Main.myPlayer, p =>
            {
                if (Main.zenithWorld)
                {
                    p.velocity.Modulus *= Main.rand.NextFloat(0.7f, TOWorld.LegendaryMode ? 1f : 0.85f);
                    if (TOWorld.LegendaryMode)
                        p.velocity.Rotation += Main.rand.NextFloat(-0.15f, 0.15f);
                }
            });

            if (validSapphire)
            {
                JewelHandler.CreateDustFromJewelTo(sapphire, NPC.Center, Main.zenithWorld ? DustID.GemTopaz : DustID.GemSapphire);

                int type = Main.zenithWorld ? ModContent.ProjectileType<KingSlimeJewelEmeraldClone>() : ModContent.ProjectileType<JewelProjectile>();
                int damage = NPC.GetProjectileDamage(type);
                int amount1 = CAWorld.AnomalyUltramundane ? 7 : Main.zenithWorld ? 9 : 5;
                Projectile.RotatedProj(amount1, MathHelper.TwoPi / amount1, NPC.GetSource_FromAI(), NPC.Center, NPC.GetVelocityTowards(NPC.PlayerTarget, Main.zenithWorld ? 13.5f : 18f), type, damage, 0f, Main.myPlayer, BuffedRubyProjectileAction);
            }

            void BuffedRubyProjectileAction(Projectile p)
            {
                if (Main.zenithWorld)
                {
                    p.velocity.Modulus *= Main.rand.NextFloat(1.2f, TOWorld.LegendaryMode ? 1.5f : 1.3f);
                    if (TOWorld.LegendaryMode)
                        p.velocity.Rotation += Main.rand.NextFloat(-0.2f, 0.2f);
                    p.VelocityToRotation(MathHelper.PiOver2);
                    p.timeLeft = (int)(p.timeLeft * (TOWorld.LegendaryMode ? 2.25f : 1.5f));
                }
            }
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
