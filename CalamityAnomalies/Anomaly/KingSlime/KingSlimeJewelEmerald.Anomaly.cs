using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public class KingSlimeJewelEmerald_Anomaly : KingSlimeJewel_Anomaly<KingSlimeJewelEmerald>
{
    public enum Behavior
    {
        Despawn = -1,

        FollowTarget = 0,
        Charge = 1,
    }

    public int ChargeCooldownTime => HasEnteredPhase2 ? 210 : 150;
    public int ChargePreparationTime => HasEnteredPhase2 ? 75 : 60;
    public static int ChargeTime => 60;
    public float ChargeSpeed => CAWorld.AnomalyUltramundane ? (HasEnteredPhase2 ? 24f : 28f) : (HasEnteredPhase2 ? 18f : 22f);

    public Behavior CurrentAttack
    {
        get => (Behavior)(int)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public override void SetDefaults()
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.5f);
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
        Lighting.AddLight(NPC.Center, 0f, 1f, 0f);

        switch (CurrentAttack)
        {
            case Behavior.Despawn:
                JewelHandler.Despawn(NPC);
                return false;
            case Behavior.FollowTarget:
                FollowTarget();
                break;
            case Behavior.Charge:
                Charge();
                break;
        }

        NPC.netUpdate = true;

        return false;

        void FollowTarget()
        {
            if (CanAttack)
            {
                JewelHandler.Move(NPC, Target.Center, 15f, 12f, 0.2f, 0.125f, 350f, -350f, -200f, -400f);
                Timer1++;
            }
            else
            {
                JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.175f, 150f, -150f, 0f, -200f);
                Timer1 -= 2;
            }

            if (Timer1 >= ChargeCooldownTime)
            {
                Timer1 = 0;
                CurrentAttack = Behavior.Charge;
                NPC.netUpdate = true;
            }
        }

        void Charge()
        {
            NPC.knockBackResist = 0f;

            switch (CurrentAttackPhase)
            {
                case 0:
                    if (CanAttack)
                        Timer1++;
                    else
                    {
                        if ((Timer1 -= 2) <= ChargePreparationTime - 30)
                            CurrentAttack = Behavior.FollowTarget;
                        break;
                    }
                    if (Timer1 < ChargePreparationTime) //停止，旋转
                    {
                        NPC.velocity *= 0.94f;
                        NPC.rotation += (0.1f + (float)Timer1 / ChargePreparationTime * 0.4f) * NPC.direction;
                    }
                    else //冲刺
                    {
                        Timer1 = 0;
                        ChargeBehavior();
                    }
                    break;
                case 1: //冲刺中
                    if (CanAttack)
                        Timer1++;
                    else
                    {
                        Timer1 -= 2;
                        CurrentAttack = Behavior.FollowTarget;
                        break;
                    }
                    if (Timer1 >= ChargeTime)
                    {
                        Timer1 = 0;
                        for (int i = 0; i < 15; i++)
                            JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(2f, 3f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.7f));
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        CurrentAttackPhase = 0;
                        CurrentAttack = Behavior.FollowTarget;
                        NPC.velocity = Vector2.Zero;
                        NPC.netUpdate = true;
                    }
                    else
                        NPC.damage = NPC.defDamage;
                    break;
            }

            void ChargeBehavior()
            {
                KingSlime_Anomaly kingSlimeBehavior = new() { _entity = master };
                bool validSapphire = !HasEnteredPhase2 && kingSlimeBehavior.JewelSapphireAlive;
                NPC sapphire = validSapphire ? kingSlimeBehavior.JewelSapphire : null;

                SoundEngine.PlaySound(SoundID.Item38, NPC.Center);
                int particleAmount = HasEnteredPhase2 ? 10 : 15;
                if (validSapphire)
                    particleAmount += 25;
                for (int i = 0; i < particleAmount; i++)
                    JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(4f, 7f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));

                NPC.damage = NPC.defDamage;

                float chargeSpeed = ChargeSpeed;
                if (validSapphire)
                    chargeSpeed *= 1.2f;

                NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, chargeSpeed), MathHelper.PiOver2);
                NPC.netSpam = 0;

                if (TOWorld.GeneralClient && validSapphire)
                {
                    JewelHandler.CreateDustFromJewelTo(sapphire, NPC.Center, Main.zenithWorld ? DustID.GemTopaz : DustID.GemSapphire);

                    int type = Main.zenithWorld ? ModContent.ProjectileType<JewelProjectile>() : ModContent.ProjectileType<KingSlimeJewelEmeraldClone>();
                    int damage = NPC.GetProjectileDamage(type);
                    if (CAWorld.AnomalyUltramundane)
                        damage *= 2;
                    Vector2 velocityUnit = NPC.GetVelocityTowards(NPC.PlayerTarget, 1f);
                    Vector2 offset = velocityUnit.RotatedBy(MathHelper.PiOver2);
                    int amount = CAWorld.AnomalyUltramundane ? 4 : 3;
                    for (int i = -amount; i <= amount; i++)
                    {
                        Projectile.NewProjectileAction(NPC.GetSource_FromAI(), NPC.Center + offset * 24f * i + velocityUnit * (60f - 20f * Math.Abs(i)), velocityUnit * chargeSpeed, type, damage, 0f, Main.myPlayer, p =>
                        {
                            if (Main.zenithWorld)
                                p.timeLeft = 60;
                            else
                                p.VelocityToRotation(MathHelper.PiOver2);
                        });
                    }
                }

                CurrentAttackPhase = 1;
                NPC.netUpdate = true;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float timeLeftGateValue = 30f;
        float gateValue = ChargePreparationTime - timeLeftGateValue;
        float ratio = CurrentAttack == Behavior.Charge && CurrentAttackPhase == 0 && Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        if (CAClientConfig.Instance.AuxiliaryVisualEffects && ratio > 0f)
            JewelHandler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, 120f, 0.35f);
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