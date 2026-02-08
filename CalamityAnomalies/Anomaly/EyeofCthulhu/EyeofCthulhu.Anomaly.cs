using CalamityMod.Events;
using CalamityMod.World;
using Transoceanic;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed class EyeofCthulhu_Anomaly : AnomalyNPCBehavior
{
    #region 数据
    public enum Phase
    {
        Initialize,
        Phase1,
        PhaseChange_1To2,
        Phase2,
    }

    public enum Behavior
    {
        Phase1_Hover,
        Phase1_Charge,

        PhaseChange,
        PhaseChange_State2,

        Phase2_Hover,
        Phase2_NormalCharge,
        Phase2_RapidCharge,
        Phase2_HorizontalHover,
        Phase2_HorizontalCharge,
    }

    public const float DespawnDistance = 6000f;
    public const float ProjectileOffset = 50f;
    public static float Phase2LifeRatio => CASharedData.AnomalyUltramundane ? 0.8f : 0.75f;
    public static float Phase2_2LifeRatio => CASharedData.AnomalyUltramundane ? 0.65f : 0.4f;
    public static float Phase2_3LifeRatio => CASharedData.AnomalyUltramundane ? 0.5f : 0.3f;
    public static float Phase2_4LifeRatio => CASharedData.AnomalyUltramundane ? 0.35f : 0.15f;
    public bool Phase2_2 => NPC.LifeRatio < Phase2_2LifeRatio;
    public bool Phase2_3 => NPC.LifeRatio < Phase2_3LifeRatio;
    public bool Phase2_4 => NPC.LifeRatio < Phase2_4LifeRatio;
    public int SetDamage => (int)Math.Round(NPC.defDamage * (Phase2_2 ? 1.4f : 1.2f));
    public int ReducedSetDamage => (int)Math.Round(NPC.defDamage * (Phase2_2 ? 0.7f : 0.6f));

    public static float LineUpDist => 15f;
    public static float ServantAndProjectileVelocity => 10f;

    public Phase CurrentPhase
    {
        get => (Phase)(int)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    public int CurrentPhase2SubPhase
    {
        get
        {
            float lifeRatio = NPC.LifeRatio;
            if (lifeRatio < Phase2_4LifeRatio)
                return 4;
            if (lifeRatio < Phase2_3LifeRatio)
                return 3;
            if (lifeRatio < Phase2_2LifeRatio)
                return 2;
            return 1;
        }
    }

    public Behavior CurrentBehavior
    {
        get => (Behavior)(int)NPC.ai[2];
        set => NPC.ai[2] = (int)value;
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    public bool BuffedCharge
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[0];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[0] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[0] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public int ChargeCounter
    {
        get => AnomalyNPC.AnomalyAI32[1].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[1].i != value)
            {
                AnomalyNPC.AnomalyAI32[1].i = value;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }

    public float RotationSpeed
    {
        get => AnomalyNPC.AnomalyAI32[2].f;
        set
        {
            if (AnomalyNPC.AnomalyAI32[2].f != value)
            {
                AnomalyNPC.AnomalyAI32[2].f = value;
                AnomalyNPC.AIChanged32[2] = true;
            }
        }
    }

    public int Phase3ChargeCalculator
    {
        get => AnomalyNPC.AnomalyAI32[3].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[3].i != value)
            {
                AnomalyNPC.AnomalyAI32[3].i = value;
                AnomalyNPC.AIChanged32[3] = true;
            }
        }
    }

    public float EyeRotation => TOMathHelper.NormalizeAngle(MathF.Atan2(NPC.position.Y + NPC.height - 59f - Target.Center.Y, NPC.Center.X - Target.Center.X) + MathHelper.PiOver2);
    public static float EnrageScale => CASharedData.AnomalyUltramundane || Main.IsItDay() ? 2.5f : 1f;
    public bool ShouldCharge => Vector2.Distance(Target.Center, NPC.Center) >= 320f;
    #endregion 数据

    public override int ApplyingType => NPCID.EyeofCthulhu;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior method) => method switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        if (CASharedData.AnomalyUltramundane)
            NPC.lifeMax = (int)(NPC.lifeMax * 1.25f);
    }

    public override bool PreAI()
    {
        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.velocity.Y -= 0.04f;
            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;
            return false;
        }

        MiscAI();

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                CurrentPhase = Phase.Phase1;
                break;
            case Phase.Phase1:
                Phase1();
                break;
            case Phase.PhaseChange_1To2:
                PhaseChange();
                break;
            case Phase.Phase2:
                Phase2();
                break;
        }

        return false;

        void MiscAI()
        {
            if (Main.IsItDay())
                CalamityNPC.CurrentlyEnraged = !BossRushEvent.BossRushActive;

            //更新旋转
            float targetRotation = EyeRotation;
            float acceleration = CurrentBehavior switch
            {
                Behavior.Phase1_Hover => 0.04f,
                Behavior.Phase1_Charge when Timer1 > 40 => 0.1f,
                Behavior.Phase2_Hover => 0.1f,
                Behavior.Phase2_NormalCharge when Timer1 > 40 => 0.16f,
                Behavior.Phase2_RapidCharge when Timer1 > LineUpDist => 0.3f,
                Behavior.Phase2_HorizontalHover => 0.1f,
                _ => 0f
            };

            if (NPC.rotation < targetRotation)
            {
                if (targetRotation - NPC.rotation > MathHelper.Pi)
                    NPC.rotation -= acceleration;
                else
                    NPC.rotation += acceleration;
            }
            else if (NPC.rotation > targetRotation)
            {
                if (NPC.rotation - targetRotation > MathHelper.Pi)
                    NPC.rotation += acceleration;
                else
                    NPC.rotation -= acceleration;
            }

            if (NPC.rotation > targetRotation - acceleration && NPC.rotation < targetRotation + acceleration)
                NPC.rotation = targetRotation;

            NPC.rotation = TOMathHelper.NormalizeAngle(NPC.rotation);

            //视觉效果
            if (Main.rand.NextBool(5))
            {
                Dust.NewDustAction(NPC.Center, NPC.width, (int)(NPC.height * 0.5f), DustID.Blood, new Vector2(NPC.velocity.X, 2f), d =>
                {
                    d.velocity.X *= 0.5f;
                    d.velocity.Y *= 0.1f;
                });
            }
        }

        bool CanShootProjectile() => Collision.CanHitLine(NPC.Center, 1, 1, Target.Center, 1, 1)
            && NPC.SafeDirectionTo(Target.Center).AngleBetween((NPC.rotation + MathHelper.PiOver2).ToRotationVector2()) < MathHelper.ToRadians(18f)
            && Vector2.Distance(NPC.Center, Target.Center) > 240f;

        void SelectNextAttack()
        {
            CurrentAttackPhase = 0;
            Timer1 = 0;
            Timer2 = 0;
            switch (CurrentPhase)
            {
                case Phase.Phase1:
                    switch (CurrentBehavior)
                    {
                        case Behavior.Phase1_Hover:
                            ChargeCounter = 0;
                            CurrentBehavior = Behavior.Phase1_Charge;
                            break;
                        case Behavior.Phase1_Charge:
                            ChargeCounter++;
                            NPC.rotation = EyeRotation;
                            if (ChargeCounter >= 4)
                            {
                                NPC.damage = 0;
                                ChargeCounter = 0;
                                CurrentBehavior = Behavior.Phase1_Hover;
                            }
                            if (NPC.netSpam > 10)
                                NPC.netSpam = 10;
                            break;
                        default:
                            CurrentBehavior = Phase2_2 ? Behavior.Phase2_HorizontalHover : Behavior.Phase2_Hover;
                            break;
                    }
                    break;
                case Phase.Phase2:
                    switch (CurrentBehavior)
                    {
                        case Behavior.Phase2_Hover:
                            CurrentBehavior = Behavior.Phase2_NormalCharge;
                            break;
                        case Behavior.Phase2_HorizontalHover:
                            switch (Phase3ChargeCalculator)
                            {
                                case 0:
                                    CurrentBehavior = Behavior.Phase2_RapidCharge;
                                    BuffedCharge = true;
                                    ChargeCounter = -1;
                                    break;
                                case 2:
                                    CurrentBehavior = Behavior.Phase2_RapidCharge;
                                    BuffedCharge = true;
                                    break;
                                case 1 or 3:
                                    CurrentBehavior = Behavior.Phase2_HorizontalCharge;
                                    BuffedCharge = false;
                                    break;
                            }
                            Phase3ChargeCalculator += (Phase3ChargeCalculator % 2 == 0) ? Main.rand.Next(1, 3) : 1;
                            if (Phase3ChargeCalculator > 3)
                                Phase3ChargeCalculator = Main.rand.Next(2);
                            break;
                        case Behavior.Phase2_NormalCharge:
                            ChargeCounter++;
                            NPC.rotation = EyeRotation;
                            if (ChargeCounter >= 4)
                            {
                                NPC.damage = 0;
                                ChargeCounter = 0;
                                CurrentBehavior = Phase2_2 ? Behavior.Phase2_HorizontalHover : Behavior.Phase2_Hover;
                            }
                            if (NPC.netSpam > 10)
                                NPC.netSpam = 10;
                            break;
                        case Behavior.Phase2_RapidCharge:
                            ChargeCounter++;
                            if (ChargeCounter >= (Phase2_4 ? 0 : Phase2_3 ? 1 : 2))
                            {
                                NPC.damage = ReducedSetDamage;
                                CurrentBehavior = Phase2_2 ? Behavior.Phase2_HorizontalHover : Behavior.Phase2_Hover;
                            }
                            if (NPC.netSpam > 10)
                                NPC.netSpam = 10;
                            break;
                        case Behavior.Phase2_HorizontalCharge:
                        default:
                            CurrentBehavior = Phase2_2 ? Behavior.Phase2_HorizontalHover : Behavior.Phase2_Hover;
                            if (NPC.netSpam > 10)
                                NPC.netSpam = 10;
                            break;
                    }
                    break;
            }
            NPC.netUpdate = true;
        }

        void Phase1()
        {
            switch (CurrentBehavior)
            {
                case Behavior.Phase1_Hover:
                    Hover();
                    break;
                case Behavior.Phase1_Charge:
                    Charge();
                    break;
            }

            void Hover()
            {
                NPC.damage = 0;

                float enrageScale = EnrageScale;
                float hoverSpeed = 7f + 5f * enrageScale + 7f * (1f - NPC.LifeRatio);
                float hoverAcceleration = 0.15f + 0.1f * enrageScale + 0.15f * (1f - NPC.LifeRatio);

                if (CASharedData.AnomalyUltramundane)
                {
                    hoverSpeed += 3f;
                    hoverAcceleration += 0.08f;
                }

                float attackSwitchTimer = 120f - 180f * (1f - NPC.LifeRatio);
                bool timeToCharge = Timer1 >= attackSwitchTimer;
                Vector2 hoverDestination = Target.Center - Vector2.UnitY * 400f;
                Vector2 idealVelocity = NPC.SafeDirectionTo(hoverDestination) * (hoverSpeed + (timeToCharge ? ((Timer1 - attackSwitchTimer) * 0.01f) : 0f));
                NPC.SimpleFlyMovement(idealVelocity, hoverAcceleration + (timeToCharge ? ((Timer1 - attackSwitchTimer) * 0.001f) : 0f));

                Timer1++;

                if (timeToCharge && ShouldCharge)
                    SelectNextAttack();
                else if (NPC.WithinRange(hoverDestination, 900f))
                    HandleServantSpawning();

                void HandleServantSpawning()
                {
                    float enrageScale = EnrageScale;
                    if (!Target.dead)
                        Timer2++;

                    float servantSpawnGateValue = 10f;
                    if (CASharedData.AnomalyUltramundane)
                        servantSpawnGateValue *= 0.8f;

                    if (Timer2 >= servantSpawnGateValue && CanShootProjectile())
                    {
                        Timer2 = 0;
                        NPC.rotation = EyeRotation;

                        Vector2 servantSpawnVelocity = NPC.SafeDirectionTo(Target.Center) * ServantAndProjectileVelocity;
                        Vector2 servantSpawnCenter = NPC.Center + servantSpawnVelocity.SafeNormalize(Vector2.UnitY) * ProjectileOffset;

                        int maxServants = 4;
                        bool spawnServant = NPC.CountNPCS(NPCID.ServantofCthulhu) < maxServants;

                        if (spawnServant)
                            SoundEngine.PlaySound(SoundID.NPCHit1, servantSpawnCenter);

                        if (TOSharedData.GeneralClient)
                        {
                            if (spawnServant)
                            {
                                NPC.NewNPCAction(NPC.GetSource_FromAI(), servantSpawnCenter, NPCID.ServantofCthulhu, action: n =>
                                {
                                    n.velocity = servantSpawnVelocity;
                                    n.ai[2] = enrageScale;
                                });
                            }
                            else
                                Projectile.NewProjectileAction(NPC.GetSource_FromAI(), NPC.Center + servantSpawnVelocity.ToCustomLength(ProjectileOffset), servantSpawnVelocity * 2f, ProjectileID.BloodNautilusShot, (int)(NPC.damage * 0.5f), 0f, action: p => p.timeLeft = 600);
                        }

                        if (spawnServant)
                        {
                            for (int i = 0; i < 10; i++)
                                Dust.NewDustAction(servantSpawnCenter, 20, 20, DustID.Blood, servantSpawnVelocity * 0.4f);
                        }
                    }
                }

                CheckPhaseChange();
            }

            void Charge()
            {
                if (CurrentAttackPhase == 0) //冲刺
                {
                    NPC.damage = NPC.defDamage;
                    NPC.rotation = EyeRotation;

                    float enrageScale = EnrageScale;
                    float chargeSpeed = 8f + ChargeCounter * 2f + 5f * enrageScale + 10f * (1f - NPC.LifeRatio);
                    if (CASharedData.AnomalyUltramundane)
                        chargeSpeed += 4f;

                    NPC.velocity = NPC.SafeDirectionTo(Target.Center) * chargeSpeed;
                    NPC.netUpdate = true;

                    if (NPC.netSpam > 10)
                        NPC.netSpam = 10;

                    CurrentAttackPhase = 1;
                }
                else //冲刺中
                {
                    NPC.damage = NPC.defDamage;

                    int chargeDelay = 70 - (int)Math.Round(40f * (1f - NPC.LifeRatio));
                    if (CASharedData.AnomalyUltramundane)
                        chargeDelay -= 30;

                    float slowDownGateValue = chargeDelay * 0.9f;

                    Timer1++;
                    if (Timer1 >= slowDownGateValue)
                    {
                        NPC.damage = 0;
                        float decelerationScalar = Utils.GetLerpValue(Phase2LifeRatio, 1f, NPC.LifeRatio, true);

                        NPC.velocity *= (MathHelper.Lerp(0.76f, 0.88f, decelerationScalar));
                        if (CASharedData.AnomalyUltramundane)
                            NPC.velocity *= 0.99f;

                        if (Math.Abs(NPC.velocity.X) < 0.1f)
                            NPC.velocity.X = 0f;
                        if (Math.Abs(NPC.velocity.Y) < 0.1f)
                            NPC.velocity.Y = 0f;
                    }
                    else
                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                    if (Timer1 >= chargeDelay && !CheckPhaseChange())
                        SelectNextAttack();
                }
            }

            bool CheckPhaseChange()
            {
                if (NPC.LifeRatio >= Phase2LifeRatio)
                    return false;

                //进入阶段转换
                NPC.damage = 0;
                CurrentPhase = Phase.PhaseChange_1To2;
                CurrentBehavior = Behavior.PhaseChange;
                Timer1 = -15; //15帧缓冲时间
                ChargeCounter = 0;
                NPC.TargetClosest();
                NPC.netUpdate = true;

                if (NPC.netSpam > 10)
                    NPC.netSpam = 10;

                return true;
            }
        }

        void PhaseChange()
        {
            Timer1++;

            NPC.damage = 0;

            RotationSpeed = CurrentAttackPhase == 0 ? Math.Max(RotationSpeed + 0.005f, 0.5f) : Math.Min(RotationSpeed - 0.005f, 0f);
            NPC.rotation += RotationSpeed;

            float servantSpawnGateValue = CASharedData.AnomalyUltramundane ? 4f : 10f;
            if (Timer1 >= 0 && Timer1 % servantSpawnGateValue == 0f) //召唤仆从
            {
                float enrageScale = EnrageScale;
                float servantVelocity = 11.3f;
                Vector2 servantSpawnVelocity = Main.rand.NextVector2CircularEdge(servantVelocity, servantVelocity);
                //if (CAWorld.AnomalyUltramundane)
                //    servantSpawnVelocity *= 3f;

                Vector2 servantSpawnCenter = NPC.Center + servantSpawnVelocity.ToCustomLength(ProjectileOffset);

                if (TOSharedData.GeneralClient)
                {
                    int spawnType = NPCID.ServantofCthulhu;
                    if (Main.zenithWorld) //流血仆从移至GFB
                    {
                        int maxBloodServants = TOSharedData.LegendaryMode ? 3 : 2;
                        bool spawnBloodServant = NPC.CountNPCS(ModContent.NPCType<BloodlettingServant>()) < maxBloodServants;
                        if (spawnBloodServant)
                            spawnType = ModContent.NPCType<BloodlettingServant>();
                    }

                    NPC.NewNPCAction(NPC.GetSource_FromAI(), servantSpawnCenter, spawnType, action: n =>
                    {
                        n.velocity = servantSpawnVelocity;
                        n.ai[2] = enrageScale;
                    });

                    if (CalamityWorld.LegendaryMode)
                    {
                        int type = ProjectileID.BloodNautilusShot;
                        int damage = (int)(NPC.damage * 0.5f);
                        Vector2 projectileVelocity = Main.rand.NextVector2CircularEdge(15f, 15f);
                        int numProj = 3;
                        int spread = 20;
                        float rotation = MathHelper.ToRadians(spread);
                        for (int i = 0; i < numProj; i++)
                        {
                            Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                            Projectile.NewProjectileAction(NPC.GetSource_FromAI(), NPC.Center + perturbedSpeed.ToCustomLength(ProjectileOffset), perturbedSpeed, type, damage, 0f, action: p => p.timeLeft = 600);
                        }
                    }
                }

                for (int i = 0; i < 10; i++)
                    Dust.NewDustAction(servantSpawnCenter, 20, 20, DustID.Blood, servantSpawnVelocity * 0.4f);
            }

            switch (Timer1)
            {
                case 50:
                    SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);

                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int phase2Gore = 0; phase2Gore < 2; phase2Gore++)
                        {
                            for (int type = 8; type >= 6; type--)
                                Gore.NewGoreAction(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)), type);
                        }
                    }

                    for (int i = 0; i < 20; i++)
                        Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                    break;
                case 100: //进入二阶段
                    CurrentPhase = Phase.Phase2;
                    CurrentBehavior = Behavior.Phase2_Hover;

                    Timer1 = -15; //15帧缓冲
                    break;
            }

            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));
            NPC.velocity *= 0.98f;

            if (Math.Abs(NPC.velocity.X) < 0.1f)
                NPC.velocity.X = 0f;
            if (Math.Abs(NPC.velocity.Y) < 0.1f)
                NPC.velocity.Y = 0f;
        }

        void Phase2()
        {
            NPC.defense = 0;

            switch (CurrentBehavior)
            {
                case Behavior.Phase2_Hover:
                    Hover();
                    break;
                case Behavior.Phase2_NormalCharge:
                    NormalCharge();
                    break;
                case Behavior.Phase2_RapidCharge:
                    RapidCharge();
                    break;
                case Behavior.Phase2_HorizontalHover:
                    HorizontalHover();
                    break;
                case Behavior.Phase2_HorizontalCharge:
                    HorizontalCharge();
                    break;
            }

            void Hover()
            {
                NPC.damage = ReducedSetDamage;

                float enrageScale = EnrageScale;
                float hoverSpeed = 5.5f + 3f * (Phase2LifeRatio - NPC.LifeRatio);
                float hoverAcceleration = 0.06f + 0.02f * (Phase2LifeRatio - NPC.LifeRatio);

                hoverSpeed += 4f * enrageScale + 5.5f * (Phase2LifeRatio - NPC.LifeRatio);
                hoverAcceleration += 0.04f * enrageScale + 0.06f * (Phase2LifeRatio - NPC.LifeRatio);

                Vector2 hoverDestination = Target.Center - Vector2.UnitY * 400f;
                float distanceFromHoverDestination = NPC.Distance(hoverDestination);

                if (distanceFromHoverDestination > 400f)
                {
                    hoverSpeed += 1.25f;
                    hoverAcceleration += 0.075f;
                    if (distanceFromHoverDestination > 600f)
                    {
                        hoverSpeed += 1.25f;
                        hoverAcceleration += 0.075f;
                        if (distanceFromHoverDestination > 800f)
                        {
                            hoverSpeed += 1.25f;
                            hoverAcceleration += 0.075f;
                        }
                    }
                }

                if (CASharedData.AnomalyUltramundane)
                {
                    hoverSpeed += 1f;
                    hoverAcceleration += 0.1f;
                }

                float phaseLimit = 160f - 150f * (Phase2LifeRatio - NPC.LifeRatio);
                bool timeToCharge = Timer1 >= phaseLimit;
                Vector2 idealHoverVelocity = NPC.SafeDirectionTo(hoverDestination) * (hoverSpeed + (timeToCharge ? ((Timer1 - phaseLimit) * 0.01f) : 0f));
                NPC.SimpleFlyMovement(idealHoverVelocity, hoverAcceleration + (timeToCharge ? ((Timer1 - phaseLimit) * 0.001f) : 0f));

                Timer1++;

                ShootProjectile();

                if (timeToCharge && ShouldCharge)
                    SelectNextAttack();

                void ShootProjectile()
                {
                    float projectileGateValue = Phase2_2 ? 40f : 60f;

                    if (Timer1 % projectileGateValue == 0f && CanShootProjectile())
                    {
                        Vector2 projectileVelocity = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * ServantAndProjectileVelocity * 2f;

                        if (TOSharedData.GeneralClient)
                        {
                            int type = ProjectileID.BloodNautilusShot;
                            int damage = (int)(NPC.damage * 0.5f);
                            int numProj = Main.rand.Next(4, 6);
                            int spread = numProj * 3;
                            float rotation = MathHelper.ToRadians(spread);
                            for (int i = 0; i < numProj; i++)
                            {
                                Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                Projectile.NewProjectileAction(NPC.GetSource_FromAI(), NPC.Center + perturbedSpeed.ToCustomLength(ProjectileOffset), perturbedSpeed, type, damage, 0f, action: p => p.timeLeft = 600);
                            }
                        }
                    }
                }
            }

            void NormalCharge()
            {
                if (CurrentAttackPhase == 0) //冲刺
                {
                    NPC.damage = SetDamage;

                    SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    NPC.rotation = EyeRotation;

                    float enrageScale = EnrageScale;
                    float additionalVelocityPerCharge = 3f;
                    float chargeSpeed = 10f + (3.5f * (Phase2LifeRatio - NPC.LifeRatio)) + ChargeCounter * additionalVelocityPerCharge + 4f * enrageScale + 6.5f * (Phase2LifeRatio - NPC.LifeRatio);

                    if (ChargeCounter == 1)
                        chargeSpeed *= 1.15f;
                    if (ChargeCounter == 2)
                        chargeSpeed *= 1.3f;
                    if (CASharedData.AnomalyUltramundane)
                        chargeSpeed *= 1.2f;

                    NPC.velocity = NPC.SafeDirectionTo(Target.Center) * chargeSpeed;
                    CurrentAttackPhase++;
                    NPC.netUpdate = true;

                    if (NPC.netSpam > 10)
                        NPC.netSpam = 10;
                }
                else //冲刺中
                {
                    NPC.damage = SetDamage;

                    int phase2ChargeDelay = 60 - (int)Math.Round(35f * (Phase2LifeRatio - NPC.LifeRatio));

                    float slowDownGateValue = phase2ChargeDelay * 0.95f;

                    Timer1++;
                    if (Timer1 >= slowDownGateValue)
                    {
                        NPC.damage = ReducedSetDamage;

                        float decelerationScalar = Utils.GetLerpValue(Phase2_2LifeRatio, Phase2LifeRatio, NPC.LifeRatio, true);

                        NPC.velocity *= (MathHelper.Lerp(0.6f, 0.7f, decelerationScalar));

                        if (Math.Abs(NPC.velocity.X) < 0.1f)
                            NPC.velocity.X = 0f;
                        if (Math.Abs(NPC.velocity.Y) < 0.1f)
                            NPC.velocity.Y = 0f;
                    }
                    else
                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                    if (Timer1 >= phase2ChargeDelay)
                        SelectNextAttack();
                }
            }

            void RapidCharge()
            {
                if (CurrentAttackPhase == 0) //冲刺
                {
                    if (TOSharedData.GeneralClient)
                    {
                        NPC.damage = SetDamage;

                        float enrageScale = EnrageScale;
                        float speedBoost = 10f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float finalChargeSpeed = 18f + speedBoost;
                        finalChargeSpeed += 10f * enrageScale;

                        Vector2 eyeChargeDirection = NPC.Center;
                        float targetX = Target.Center.X - eyeChargeDirection.X;
                        float targetY = Target.Center.Y - eyeChargeDirection.Y;
                        float targetVelocity = Math.Abs(Target.velocity.X) + Math.Abs(Target.velocity.Y) / 4f;
                        targetVelocity += 10f - targetVelocity;

                        if (targetVelocity < 2f)
                            targetVelocity = 2f;
                        if (targetVelocity > 6f)
                            targetVelocity = 6f;

                        if (BuffedCharge)
                        {
                            targetVelocity *= 4f;
                            finalChargeSpeed *= 1.3f;
                        }

                        targetX -= Target.velocity.X * targetVelocity;
                        targetY -= Target.velocity.Y * targetVelocity / 4f;

                        float targetDistance = (float)Math.Sqrt(targetX * targetX + targetY * targetY);
                        float targetDistCopy = targetDistance;

                        targetDistance = finalChargeSpeed / targetDistance;
                        NPC.velocity.X = targetX * targetDistance;
                        NPC.velocity.Y = targetY * targetDistance;

                        if (targetDistCopy < 100f)
                        {
                            if (Math.Abs(NPC.velocity.X) > Math.Abs(NPC.velocity.Y))
                            {
                                float absoluteXVel = Math.Abs(NPC.velocity.X);
                                float absoluteYVel = Math.Abs(NPC.velocity.Y);

                                if (NPC.Center.X > Target.Center.X)
                                    absoluteYVel *= -1f;
                                if (NPC.Center.Y > Target.Center.Y)
                                    absoluteXVel *= -1f;

                                NPC.velocity.X = absoluteYVel;
                                NPC.velocity.Y = absoluteXVel;
                            }
                        }
                        else if (Math.Abs(NPC.velocity.X) > Math.Abs(NPC.velocity.Y))
                        {
                            float absoluteEyeVel = (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y)) / 2f;
                            float absoluteEyeVelBackup = absoluteEyeVel;

                            if (NPC.Center.X > Target.Center.X)
                                absoluteEyeVelBackup *= -1f;
                            if (NPC.Center.Y > Target.Center.Y)
                                absoluteEyeVel *= -1f;

                            NPC.velocity.X = absoluteEyeVelBackup;
                            NPC.velocity.Y = absoluteEyeVel;
                        }

                        CurrentAttackPhase++;
                        NPC.netUpdate = true;

                        if (NPC.netSpam > 10)
                            NPC.netSpam = 10;
                    }
                }
                else //冲刺中
                {
                    NPC.damage = SetDamage;

                    if (Timer1 == 0)
                        SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);

                    float lineUpDistControl = LineUpDist;
                    Timer1++;

                    if (Timer1 == lineUpDistControl && Vector2.Distance(NPC.position, Target.position) < 200f)
                        Timer1--;

                    if (Timer1 >= lineUpDistControl)
                    {
                        NPC.damage = ReducedSetDamage;
                        NPC.velocity *= 0.95f;

                        if (Math.Abs(NPC.velocity.X) < 0.1f)
                            NPC.velocity.X = 0f;
                        if (Math.Abs(NPC.velocity.Y) < 0.1f)
                            NPC.velocity.Y = 0f;
                    }
                    else
                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                    float lineUpDistNetUpdate = lineUpDistControl + 13f;
                    if (Timer1 >= lineUpDistNetUpdate)
                        SelectNextAttack();
                }
            }

            void HorizontalHover()
            {
                NPC.damage = ReducedSetDamage;

                float enrageScale = EnrageScale;
                float offset = 540f;
                float speedBoost = 15f * (Phase2_2LifeRatio - NPC.LifeRatio);
                float accelerationBoost = 0.425f * (Phase2_2LifeRatio - NPC.LifeRatio);

                float hoverSpeed = 8f + speedBoost;
                float hoverAcceleration = 0.25f + accelerationBoost;

                bool horizontalCharge = Phase3ChargeCalculator is 1 or 3;
                float timeGateValue = horizontalCharge ? 100f - 80f * (Phase2_2LifeRatio - NPC.LifeRatio) : 85f - 70f * (Phase2_2LifeRatio - NPC.LifeRatio);

                if (Timer1 > timeGateValue)
                {
                    float velocityScalar = Timer1 - timeGateValue;
                    hoverSpeed += velocityScalar * 0.05f;
                    hoverAcceleration += velocityScalar * 0.0025f;
                }

                hoverSpeed += enrageScale * 4f;
                hoverAcceleration += enrageScale * 0.125f;

                Vector2 hoverDestination;
                if (horizontalCharge)
                {
                    float horizontalChargeOffset = 450f;
                    offset = Phase3ChargeCalculator == 1 ? -horizontalChargeOffset : horizontalChargeOffset;
                    hoverDestination = Target.Center + Vector2.UnitX * offset;
                }
                else
                    hoverDestination = Target.Center + Vector2.UnitY * offset;

                if (horizontalCharge)
                {
                    hoverSpeed *= 1.5f;
                    hoverAcceleration *= 1.5f;
                }

                Vector2 idealHoverVelocity = NPC.SafeDirectionTo(hoverDestination) * hoverSpeed;
                NPC.SimpleFlyMovement(idealHoverVelocity, hoverAcceleration);

                TrySpawnServant(horizontalCharge);

                float requiredDistanceForHorizontalCharge = 160f;
                if (Timer1 >= timeGateValue && (NPC.Distance(hoverDestination) < requiredDistanceForHorizontalCharge || !horizontalCharge))
                    SelectNextAttack();

                void TrySpawnServant(bool horizontalCharge)
                {
                    float enrageScale = EnrageScale;
                    float servantSpawnGateValue = horizontalCharge ? 23f : 17f;
                    float maxServantSpawnsPerAttack = 2f;

                    Timer1++;
                    if (Timer1 % servantSpawnGateValue == 0f && CanShootProjectile() && Timer1 <= servantSpawnGateValue * maxServantSpawnsPerAttack) //召唤仆从
                    {
                        Vector2 servantSpawnVelocity = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * ServantAndProjectileVelocity;
                        Vector2 servantSpawnCenter = NPC.Center + servantSpawnVelocity.SafeNormalize(Vector2.UnitY) * ProjectileOffset;

                        int spawnType = NPCID.ServantofCthulhu;
                        bool spawnServant;

                        if (Main.zenithWorld) //流血仆从移至GFB
                        {
                            int maxBloodServants = TOSharedData.LegendaryMode ? 3 : 2;
                            bool spawnBloodServant = NPC.CountNPCS(ModContent.NPCType<BloodlettingServant>()) < maxBloodServants;
                            if (spawnBloodServant)
                            {
                                spawnType = ModContent.NPCType<BloodlettingServant>();
                                spawnServant = true;
                                enrageScale += 0.5f;
                            }
                            else
                            {
                                int maxServants = 1;
                                spawnServant = !Phase2_3 && NPC.CountNPCS(NPCID.ServantofCthulhu) < maxServants;
                            }
                        }
                        else
                        {
                            int maxServants = Phase2_4 ? 1 : Phase2_3 ? 2 : 3;
                            spawnServant = NPC.CountNPCS(NPCID.ServantofCthulhu) < maxServants;
                        }

                        if (spawnServant)
                            SoundEngine.PlaySound(SoundID.NPCDeath13, servantSpawnCenter);

                        if (TOSharedData.GeneralClient)
                        {
                            if (spawnServant)
                            {
                                NPC.NewNPCAction(NPC.GetSource_FromAI(), servantSpawnCenter, spawnType, action: n =>
                                {
                                    n.velocity = servantSpawnVelocity;
                                    n.ai[2] = enrageScale;
                                });
                            }
                            else if (!CalamityWorld.LegendaryMode)
                            {
                                int type = ProjectileID.BloodNautilusShot;
                                int damage = (int)(NPC.damage * 0.5f);
                                Projectile.NewProjectileAction(NPC.GetSource_FromAI(), NPC.Center + servantSpawnVelocity.ToCustomLength(ProjectileOffset), servantSpawnVelocity * 2f, type, damage, 0f, action: p => p.timeLeft = 600);
                            }

                            if (CalamityWorld.LegendaryMode)
                            {
                                int type = ProjectileID.BloodNautilusShot;
                                int damage = (int)(NPC.damage * 0.5f);
                                Vector2 projectileVelocity = servantSpawnVelocity * 3f;
                                int numProj = 3;
                                int spread = 20;
                                float rotation = MathHelper.ToRadians(spread);
                                for (int i = 0; i < numProj; i++)
                                {
                                    Vector2 perturbedSpeed = projectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (float)(numProj - 1)));
                                    Projectile.NewProjectileAction(NPC.GetSource_FromAI(), NPC.Center + perturbedSpeed.ToCustomLength(ProjectileOffset), perturbedSpeed, type, damage, 0f, action: p => p.timeLeft = 600);
                                }
                            }
                        }

                        if (spawnServant)
                        {
                            for (int i = 0; i < 10; i++)
                                Dust.NewDustAction(servantSpawnCenter, 20, 20, DustID.Blood, servantSpawnVelocity * 0.4f);
                        }
                    }
                }
            }

            void HorizontalCharge()
            {
                if (CurrentAttackPhase == 0) //冲刺前准备
                {
                    NPC.damage = SetDamage;

                    if (TOSharedData.GeneralClient)
                    {
                        float enrageScale = EnrageScale;
                        float speedBoost = 15f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float chargeSpeed = 18f + speedBoost;
                        chargeSpeed += 10f * enrageScale;

                        NPC.velocity = NPC.SafeDirectionTo(Target.Center) * chargeSpeed;
                        CurrentAttackPhase++;
                        NPC.netUpdate = true;

                        if (NPC.netSpam > 10)
                            NPC.netSpam = 10;
                    }
                }
                else //冲刺
                {
                    NPC.damage = SetDamage;

                    if (Timer1 == 0)
                        SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);

                    float lineUpDistControl = (float)Math.Round(LineUpDist * 2.5f);
                    Timer1++;

                    if (Timer1 == lineUpDistControl && Vector2.Distance(NPC.position, Target.position) < 200f)
                        Timer1--;

                    if (Timer1 >= lineUpDistControl)
                    {
                        NPC.damage = ReducedSetDamage;
                        NPC.velocity *= 0.95f;

                        if (Math.Abs(NPC.velocity.X) < 0.1f)
                            NPC.velocity.X = 0f;
                        if (Math.Abs(NPC.velocity.Y) < 0.1f)
                            NPC.velocity.Y = 0f;
                    }
                    else
                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                    float lineUpDistNetUpdate = lineUpDistControl + 13f;
                    if (Timer1 >= lineUpDistNetUpdate)
                        SelectNextAttack();
                }
            }
        }
    }
}
