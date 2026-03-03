using CalamityAnomalies.DataStructures;

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
        PhaseChange_2To3,
        Phase3,
    }

    public enum Behavior
    {
        Phase1_Hover,
        Phase1_Charge,

        PhaseChange_1To2,

        Phase2_Hover,
        Phase2_NormalCharge,
        Phase2_RapidCharge,
        Phase2_HorizontalHover,
        Phase2_HorizontalCharge,

        PhaseChange_2To3,

        Phase3_Charge,
        Phase3_2,
        Phase3_3,
    }

    public const float DespawnDistance = 6000f;
    public const float ProjectileOffset = 50f;

    public const int PhaseChangeTime_1To2 = 150;
    public const int PhaseChangeGateValue_1To2_1 = PhaseChangeTime_1To2 / 5 * 2;
    public const int PhaseChangeGateValue_1To2_2 = PhaseChangeTime_1To2 / 5 * 3;

    public static readonly UnaryFunctionWithDomain PhaseChange_1To2_RotationSpeedFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, PhaseChangeGateValue_1To2_2, false, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut(x / PhaseChangeGateValue_1To2_1)),
        (new MathInterval(PhaseChangeGateValue_1To2_2, float.PositiveInfinity, true, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut((PhaseChangeTime_1To2 - x) / PhaseChangeGateValue_1To2_1))
        );

    public const int PhaseChangeTime_2To3 = 195;
    public const int PhaseChangeGateValue_2To3_1 = 75;
    public const int PhaseChangeGateValue_2To3_2 = 120;

    public static readonly UnaryFunctionWithDomain PhaseChange_2To3_RotationSpeedFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, PhaseChangeGateValue_2To3_2, false, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut(x / PhaseChangeGateValue_2To3_1)),
        (new MathInterval(PhaseChangeGateValue_2To3_2, float.PositiveInfinity, true, false), x => 0.5f * TOMathUtils.Interpolation.QuadraticEaseInOut((PhaseChangeTime_2To3 - x) / PhaseChangeGateValue_2To3_1))
        );

    public static float Phase2LifeRatio => Ultra ? 0.8f : 0.75f;
    public static float Phase2_2LifeRatio => Ultra ? 0.55f : 0.4f;
    public static float Phase2_3LifeRatio => Ultra ? 0.3f : 0.15f;
    public static float Phase3LifeRatio => Ultra ? 0.1f : 0f;
    public static float Phase3_2LifeRatio => Ultra ? 0.3f : 0f;

    public bool Phase2_2 => NPC.LifeRatio < Phase2_2LifeRatio;
    public bool Phase2_3 => NPC.LifeRatio < Phase2_3LifeRatio;
    public bool Phase3_2 => Phase3 && NPC.LifeRatio < Phase3_2LifeRatio;

    public float DamageMultiplier => Phase3 ? 1.5f : Phase2_2 ? 1.25f : 1f;

    public int SetDamage => (int)Math.Round(NPC.defDamage * DamageMultiplier);
    public int ReducedSetDamage => (int)Math.Round(NPC.defDamage * DamageMultiplier * 0.6f);

    public bool IsInPhase3Arena => Phase3 && ArenaProjectileAlive && NPC.Distance(ArenaProjectile.Center) < ArenaProjectile.GetModProjectile<EyeofCthulhuArena>().ArenaRadius + 30f;

    public float EyeRotation => TOMathUtils.NormalizeWithPeriod((Target.Center - NPC.Center).ToRotation(-MathHelper.PiOver2));
    public float ActualRotation => NPC.rotation + MathHelper.PiOver2;

    public int RapidChargeTime => Phase2_3 ? 12 : 15;
    public static int HorizontalChargeTime => 35;

    private static readonly ProjectileDamageContainer _bloodDamage = new(30, 60, 75, 90, 84, 108);
    public static int BloodDamage => _bloodDamage.Value;

    private static readonly ProjectileDamageContainer _arenaDamage = new(100, 140, 210, 240, 240, 300);
    public static int ArenaDamage => _arenaDamage.Value;

    private static readonly ProjectileDamageContainer _gfbFlameDamage = new(50, 80, 102, 120, 120, 150);
    public static int GFBFlameDamage => _gfbFlameDamage.Value;

    public Phase CurrentPhase
    {
        get => (Phase)(int)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    public bool Phase3 => CurrentPhase == Phase.Phase3;

    public Behavior CurrentBehavior
    {
        get => (Behavior)(int)NPC.ai[2];
        set
        {
            NPC.ai[2] = (int)value;

            ExecuteActionToServants((n, modN) => modN.ShouldUsePhase2Frame = value is Behavior.Phase2_NormalCharge or Behavior.Phase2_RapidCharge or Behavior.Phase2_HorizontalCharge);
        }
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    public bool HorizontalHoverDirectionIsNegative
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

    public int HorizontalHoverDirection
    {
        get => HorizontalHoverDirectionIsNegative ? -1 : 1;
        set => HorizontalHoverDirectionIsNegative = value == -1;
    }

    public bool NextChargeTypeIsHorizontal
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[1];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[1] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[1] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public int ServantSpawnCounter
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

    public int ChargeCounter
    {
        get => AnomalyNPC.AnomalyAI32[4].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[4].i != value)
            {
                AnomalyNPC.AnomalyAI32[4].i = value;
                AnomalyNPC.AIChanged32[4] = true;
            }
        }
    }

    public int ChargeCounter2
    {
        get => AnomalyNPC.AnomalyAI32[5].i;
        set
        {
            if (AnomalyNPC.AnomalyAI32[5].i != value)
            {
                AnomalyNPC.AnomalyAI32[5].i = value;
                AnomalyNPC.AIChanged32[5] = true;
            }
        }
    }

    public float Phase3ColorRatio
    {
        get => AnomalyNPC.AnomalyAI32[6].f;
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (AnomalyNPC.AnomalyAI32[6].f != temp)
            {
                AnomalyNPC.AnomalyAI32[6].f = temp;
                AnomalyNPC.AIChanged32[6] = true;
            }
        }
    }

    public unsafe int UsedEyeIndex1
    {
        get => AnomalyNPC.AnomalyAI32[7].bytes[0];
        set
        {
            byte temp = (byte)TOMathUtils.NormalizeWithPeriod(value, 32);
            if (AnomalyNPC.AnomalyAI32[7].bytes[0] != temp)
            {
                AnomalyNPC.AnomalyAI32[7].bytes[0] = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public unsafe int UsedEyeIndex2
    {
        get => AnomalyNPC.AnomalyAI32[7].bytes[1];
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].bytes[1] != temp)
            {
                AnomalyNPC.AnomalyAI32[7].bytes[1] = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public unsafe int UsedEyeIndex3
    {
        get => AnomalyNPC.AnomalyAI32[7].bytes[2];
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].bytes[2] != temp)
            {
                AnomalyNPC.AnomalyAI32[7].bytes[2] = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public unsafe int UsedEyeIndex4
    {
        get => AnomalyNPC.AnomalyAI32[7].bytes[3];
        set
        {
            byte temp = (byte)(value % 32);
            if (AnomalyNPC.AnomalyAI32[7].bytes[3] != temp)
            {
                AnomalyNPC.AnomalyAI32[7].bytes[3] = temp;
                AnomalyNPC.AIChanged32[7] = true;
            }
        }
    }

    public Vector2 Phase3ArenaCenter
    {
        get => AnomalyNPC.AnomalyAI64[0].v;
        set
        {
            if (AnomalyNPC.AnomalyAI64[0].v != value)
            {
                AnomalyNPC.AnomalyAI64[0].v = value;
                AnomalyNPC.AIChanged64[0] = true;
            }
        }
    }

    #region 仆从
    public bool ServantLeftSpawned
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
    /// <summary>
    /// 左仆从实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public unsafe NPC ServantLeft
    {
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].bytes[0]];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].bytes[0] != temp)
            {
                AnomalyNPC.AnomalyAI32[1].bytes[0] = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool ServantLeftAlive => ServantLeft.active && ServantLeft.ModNPC is BloodlettingServant && ServantLeft.Master == NPC;

    public bool ServantRightSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[1];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[1] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[1] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 右仆从实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public unsafe NPC ServantRight
    {
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].bytes[1]];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].bytes[1] != temp)
            {
                AnomalyNPC.AnomalyAI32[1].bytes[1] = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool ServantRightAlive => ServantRight.active && ServantRight.ModNPC is BloodlettingServant && ServantRight.Master == NPC;

    public bool ArenaProjectileSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[2];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[2] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[2] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 竞技场弹幕实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyProjectile</c>。
    /// </summary>
    public Projectile ArenaProjectile
    {
        get => Main.projectile[AnomalyNPC.AnomalyAI32[2].i];
        set
        {
            int temp = value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[2].i != temp)
            {
                AnomalyNPC.AnomalyAI32[2].i = temp;
                AnomalyNPC.AIChanged32[2] = true;
            }
        }
    }
    public bool ArenaProjectileAlive => ArenaProjectile.active && ArenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.Master == NPC;
    public EyeofCthulhuArena ArenaModProjectile => ArenaProjectile.GetModProjectile<EyeofCthulhuArena>();
    #endregion 仆从

    public float GFB_FlameRotation
    {
        get => AnomalyNPC.AnomalyAI32[10].f;
        set
        {
            float temp = TOMathUtils.NormalizeWithPeriod(value);
            if (AnomalyNPC.AnomalyAI32[10].f != temp)
            {
                AnomalyNPC.AnomalyAI32[10].f = temp;
                AnomalyNPC.AIChanged32[10] = true;
            }
        }
    }

    public BitArray32 EyeIndicesUsed
    {
        get => AnomalyNPC.AnomalyAI32[5].bits;
        set
        {
            if (AnomalyNPC.AnomalyAI32[5].bits != value)
            {
                AnomalyNPC.AnomalyAI32[5].bits = value;
                AnomalyNPC.AIChanged32[5] = true;
            }
        }
    }
    #endregion 数据

    public override int ApplyingType => NPCID.EyeofCthulhu;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior method) => method switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[ApplyingType] = 3;
        NPCID.Sets.TrailCacheLength[ApplyingType] = 5;
    }

    public override void SetDefaults()
    {
        ServantLeft = NPC.DummyNPC;
        ServantRight = NPC.DummyNPC;
        ArenaProjectile = Projectile.DummyProjectile;
    }

    #region AI
    public override bool PreAI()
    {
        bool valid = Phase3
            ? NPC.TargetClosestIfInvalid(true, p => ArenaProjectileAlive && ArenaModProjectile.ArenaRing.CircleContains(p.Hitbox, true, true))
            : NPC.TargetClosestIfInvalid(true, DespawnDistance);

        if (!valid)
        {
            NPC.dontTakeDamage = true;
            StopMovement();

            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;

            Timer5++;
            if (Timer5 >= 60)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }

            return false;
        }

        if (Timer5 > 0)
            Timer5--;

        if (Main.rand.NextBool(5))
        {
            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height / 2, DustID.Blood, new Vector2(NPC.velocity.X, 2f), d =>
            {
                d.velocity.X *= 0.5f;
                d.velocity.Y *= 0.1f;
            });
        }

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                CurrentPhase = Phase.Phase1;
                break;
            case Phase.Phase1:
                Phase1AI();
                break;
            case Phase.PhaseChange_1To2:
                PhaseChange_1To2();
                break;
            case Phase.Phase2:
                Phase2AI();
                break;
            case Phase.PhaseChange_2To3:
                PhaseChange_2To3();
                break;
            case Phase.Phase3:
                Phase3AI();
                break;
        }

        return false;

        #region 行为函数
        bool CanShootProjectile() => Vector2.IncludedAngle(new PolarVector2(ActualRotation), Target.Center - NPC.Center) < MathHelper.ToRadians(Ultra ? 30f : 20f)
            && Vector2.Distance(NPC.Center, Target.Center) > 160f;

        void NormalUpdateRotation()
        {
            float targetRotation = EyeRotation;
            float acceleration = CurrentBehavior switch
            {
                Behavior.Phase1_Hover => 0.12f,
                Behavior.Phase1_Charge => 0.16f,
                Behavior.Phase2_Hover => 0.12f,
                Behavior.Phase2_NormalCharge => 0.16f,
                Behavior.Phase2_RapidCharge => 0.3f,
                Behavior.Phase2_HorizontalHover => 0.12f,
                _ => 0.12f
            };
            EoCHandler.UpdateRotation(ref NPC.rotation, targetRotation, acceleration);
        }

        void StopMovement()
        {
            NPC.velocity *= 0.93f;

            if (Math.Abs(NPC.velocity.X) < 0.1f)
                NPC.velocity.X = 0f;
            if (Math.Abs(NPC.velocity.Y) < 0.1f)
                NPC.velocity.Y = 0f;
        }

        void TeleportTo(Vector2 destination, float completionRatio)
        {
            switch (completionRatio)
            {
                case < 0.5f:
                    NPC.Opacity = 1f - completionRatio * 2f;
                    break;
                case 0.5f:
                    NPC.Opacity = 0f;
                    NPC.Center = destination;
                    NPC.velocity = Vector2.Zero;
                    break;
                case > 0.5f:
                    NPC.Opacity = completionRatio * 2f - 1f;
                    break;
            }
        }

        void Phase1AI()
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

            void SelectNextAttack()
            {
                CurrentAttackPhase = 0;
                Timer1 = 0;
                Timer2 = 0;
                switch (CurrentBehavior)
                {
                    case Behavior.Phase1_Hover:
                        ChargeCounter = 0;
                        CurrentBehavior = Behavior.Phase1_Charge;
                        break;
                    case Behavior.Phase1_Charge:
                        ChargeCounter++;
                        NPC.rotation = EyeRotation;
                        if (ChargeCounter >= 3) //冲刺共3次
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
            }

            void Hover()
            {
                NPC.damage = 0;

                float hoverSpeed = (Ultra ? 22.5f : 15f) + 7.5f * NPC.LostLifeRatio;
                float hoverAcceleration = (Ultra ? 0.45f : 0.25f) + 0.15f * NPC.LostLifeRatio;

                Vector2 hoverDestination = Target.Center - Vector2.UnitY * 400f;
                Vector2 idealVelocity = NPC.GetVelocityTowards(hoverDestination, hoverSpeed);
                NPC.SimpleFlyMovement(idealVelocity, hoverAcceleration);

                Timer1++;

                if (Timer1 >= (Ultra ? 125 : 150))
                    SelectNextAttack();
                else if (NPC.WithinRange(hoverDestination, 1280f))
                {
                    int servantSpawnGateValue = Ultra ? 10 : 12;

                    if (Timer1 % servantSpawnGateValue == 0 && CanShootProjectile())
                    {
                        int num = Timer1 / servantSpawnGateValue;
                        NPC.rotation = EyeRotation;

                        Vector2 direction = (Target.Center - NPC.Center).SafeNormalize();
                        Vector2 servantSpawnVelocity = direction * 10f;
                        float projectileSpeed = (Ultra ? 15f : 12f);
                        Vector2 servantSpawnCenter = NPC.Center + servantSpawnVelocity.ToCustomLength(ProjectileOffset);

                        int maxServantsAlive = 2;
                        int maxServantsTotal = 8;
                        bool buff = num is 3 or 6 or 9 or 12;
                        bool shouldSpawnServant = buff && ServantSpawnCounter < maxServantsTotal && NPC.ActiveNPCs.Count(n => n.type == NPCID.ServantofCthulhu && n.Master == NPC) < maxServantsAlive;

                        if (shouldSpawnServant)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit1, servantSpawnCenter);
                            for (int i = 0; i < 10; i++)
                                Dust.NewDustAction(servantSpawnCenter, 20, 20, DustID.Blood, servantSpawnVelocity * 0.4f);
                        }

                        if (TOSharedData.GeneralClient)
                        {
                            if (shouldSpawnServant)
                            {
                                NPC.NewNPCAction(NPC.GetSource_FromAI(), servantSpawnCenter, NPCID.ServantofCthulhu, action: n =>
                                {
                                    n.velocity = servantSpawnVelocity;
                                    n.Master = NPC;
                                    ServantSpawnCounter++;
                                });
                            }

                            int projectileAmount = buff ? (Ultra ? 3 : 2) : 1;
                            EoCHandler.ShootProjectile(NPC, ProjectileID.BloodNautilusShot, BloodDamage, projectileSpeed, projectileAmount, MathHelper.ToRadians(Ultra ? 12f : 10f), p => p.timeLeft = 600);
                        }
                    }
                }

                NormalUpdateRotation();
                CheckPhaseChange();
            }

            void Charge()
            {
                if (CurrentAttackPhase == 0) //冲刺
                {
                    NPC.damage = SetDamage;

                    float chargeSpeed = (Ultra ? 17.5f : 12f) + 6f * NPC.LostLifeRatio;
                    NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target.Center, chargeSpeed), -MathHelper.PiOver2);

                    if (ChargeCounter >= 1)
                        chargeSpeed *= 1.1f;
                    if (ChargeCounter >= 2)
                        chargeSpeed *= 1.1f;

                    NPC.netUpdate = true;

                    if (NPC.netSpam > 10)
                        NPC.netSpam = 10;

                    CurrentAttackPhase = 1;
                }
                else //冲刺中
                {
                    NPC.damage = SetDamage;

                    int chargeDelay = (Ultra ? 40 : 70) - (int)Math.Round(40f * NPC.LostLifeRatio);
                    float slowDownGateValue = chargeDelay * 0.9f;

                    Timer1++;
                    if (Timer1 >= slowDownGateValue)
                    {
                        NPC.damage = 0;
                        NPC.velocity *= Utils.Remap(NPC.LifeRatio, Phase2LifeRatio, 1f, 0.76f, 0.88f);
                        if (Ultra)
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
                CurrentBehavior = Behavior.PhaseChange_1To2;
                CurrentAttackPhase = 0;
                Timer1 = -60; //60帧缓冲时间
                ChargeCounter = 0;
                NPC.netUpdate = true;

                if (NPC.netSpam > 10)
                    NPC.netSpam = 10;

                return true;
            }
        }

        void PhaseChange_1To2()
        {
            Timer1++;

            NPC.damage = 0;
            StopMovement();
            NPC.rotation += PhaseChange_1To2_RotationSpeedFunction.Process(Timer1);

            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

            //仆从、弹幕、粒子

            const int offset = 0;
            int adjustedTimer = Timer1 - offset;
            int num = adjustedTimer is >= PhaseChangeGateValue_1To2_2 - 50 and <= PhaseChangeGateValue_1To2_2 && adjustedTimer % 10 == 0 ? (adjustedTimer + 60 - PhaseChangeGateValue_1To2_2) / 10 : 0; //1~6
            if (num > 0)
            {
                if (Ultra || num % 2 == 0)
                {
                    bool lastAttack = num == 6;

                    float projectileMaxSpeed = 17.5f;
                    Vector2 projectileVelocity = new PolarVector2(projectileMaxSpeed, NPC.rotation + Main.rand.NextFloat(-0.15f, 0.15f));

                    int projectileAmountOver4 = Ultra ? num switch
                    {
                        1 or 2 => 1,
                        3 or 4 or 5 => 2,
                        6 => 8,
                        _ => 0
                    } : num switch
                    {
                        2 => 1,
                        4 => 2,
                        6 => 6,
                        _ => 0
                    };

                    int particleAmount = projectileAmountOver4 * 8;
                    for (int i = 0; i < particleAmount; i++)
                        EoCHandler.SpawnOrbParticle(NPC.Center, lastAttack ? Main.rand.NextFloat(5f, 10f) : Main.rand.NextFloat(3f, 5f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.5f, 1f));

                    EoCHandler.ShootEyeProjectile(NPC, ProjectileID.BloodShot, BloodDamage, projectileVelocity, projectileAmountOver4, p => p.timeLeft = 300);

                    if (lastAttack)
                        EoCHandler.SpawnEyeParticle(NPC, projectileVelocity * 1.4f);
                }
            }

            switch (Timer1)
            {
                case PhaseChangeGateValue_1To2_2:
                    SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    for (int phase2Gore = 0; phase2Gore < 2; phase2Gore++)
                    {
                        for (int type = 8; type >= 6; type--)
                            Gore.NewGoreAction(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)), type);
                    }

                    for (int i = 0; i < 20; i++)
                        Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

                    //生成流血仆从
                    //start参数设置为NPC.whoAmI，以确保发送命令时仆从能够同帧执行
                    if (Ultra)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            NPC.NewNPCAction<BloodlettingServant>(NPC.GetSource_FromAI(), NPC.Center, NPC.whoAmI, n =>
                            {
                                n.Master = NPC;
                                BloodlettingServant modN = n.GetModNPC<BloodlettingServant>();
                                modN.Place = i == 0 ? BloodlettingServant.ServantPlace.Left : BloodlettingServant.ServantPlace.Right;
                                modN.PositionRotation = NPC.rotation;

                                if (i == 0)
                                {
                                    ServantLeft = n;
                                    ServantLeftSpawned = true;
                                }
                                else
                                {
                                    ServantRight = n;
                                    ServantRightSpawned = true;
                                }
                            });
                        }
                    }

                    CurrentAttackPhase = 1;
                    break;
                case PhaseChangeTime_1To2: //进入二阶段
                    CurrentPhase = Phase.Phase2;
                    CurrentBehavior = Behavior.Phase2_Hover;
                    CurrentAttackPhase = 0;
                    Timer1 = -15; //15帧缓冲时间
                    break;
            }
        }

        void Phase2AI()
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

            if (Main.zenithWorld)
                ZenithAI();

            void SelectNextAttack()
            {
                CurrentAttackPhase = 0;
                Timer1 = 0;
                Timer2 = 0;
                switch (CurrentBehavior)
                {
                    case Behavior.Phase2_Hover:
                        CurrentBehavior = Phase2_2 ? Behavior.Phase2_RapidCharge : Behavior.Phase2_NormalCharge;
                        break;
                    case Behavior.Phase2_HorizontalHover:
                        CurrentBehavior = NextChargeTypeIsHorizontal ? Behavior.Phase2_HorizontalCharge : Behavior.Phase2_RapidCharge;
                        break;
                    case Behavior.Phase2_NormalCharge:
                        ChargeCounter++;
                        NPC.rotation = EyeRotation;
                        if (ChargeCounter >= 3) //常规冲刺共3次
                        {
                            NPC.damage = 0;
                            ChargeCounter = 0;
                            CurrentBehavior = Behavior.Phase2_RapidCharge;
                        }
                        if (NPC.netSpam > 10)
                            NPC.netSpam = 10;
                        break;
                    case Behavior.Phase2_RapidCharge:
                        ChargeCounter++;
                        if (ChargeCounter >= (Phase2_3 ? Main.rand.Next(7, 10) : Main.rand.Next(4, 7)))
                        {
                            NPC.damage = ReducedSetDamage;
                            if (Phase2_2)
                            {
                                CurrentBehavior = Behavior.Phase2_HorizontalHover;
                                NextChargeTypeIsHorizontal = true;

                                SendCommandToServants(BehaviorCommand_Servant.IncreaseFollowDistance);
                            }
                            else
                                CurrentBehavior = Behavior.Phase2_Hover;
                            ChargeCounter = 0;
                        }
                        if (NPC.netSpam > 10)
                            NPC.netSpam = 10;
                        break;
                    case Behavior.Phase2_HorizontalCharge:
                        ChargeCounter2++;
                        if (ChargeCounter2 > 3 || Main.rand.NextProbability(Phase2_3 ? 0.4f : 0.6f))
                        {
                            ChargeCounter2 = 0;
                            NextChargeTypeIsHorizontal = false;

                            SendCommandToServants(BehaviorCommand_Servant.ReduceFollowDistance);
                        }
                        else
                            NextChargeTypeIsHorizontal = true;
                        CurrentBehavior = Behavior.Phase2_HorizontalHover;
                        break;
                }
                NPC.netUpdate = true;
            }

            void Hover()
            {
                Timer1++;

                NPC.damage = ReducedSetDamage;

                float hoverSpeed = 5.5f + 3f * (Phase2LifeRatio - NPC.LifeRatio);
                float hoverAcceleration = 0.06f + 0.02f * (Phase2LifeRatio - NPC.LifeRatio);

                hoverSpeed += (Ultra ? 10f : 5f) + 5.5f * (Phase2LifeRatio - NPC.LifeRatio);
                hoverAcceleration += (Ultra ? 0.1f : 0.05f) + 0.06f * (Phase2LifeRatio - NPC.LifeRatio);

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

                float phaseLimit = 160f - 150f * (Phase2LifeRatio - NPC.LifeRatio);
                Vector2 idealHoverVelocity = NPC.SafeDirectionTo(hoverDestination) * hoverSpeed;
                NPC.SimpleFlyMovement(idealHoverVelocity, hoverAcceleration);

                int projectileGateValue = 60;

                if (Timer1 > 0 && Timer1 % projectileGateValue == 0 && CanShootProjectile())
                {
                    if (TOSharedData.GeneralClient)
                    {
                        int type = ProjectileID.BloodNautilusShot;
                        int damage = BloodDamage;
                        int numProj = 5;
                        float rotation = MathHelper.ToRadians(10f);
                        float projectileSpeed = (Ultra ? 20f : 17f) + 3f * (NPC.LifeRatio - Phase2LifeRatio);
                        EoCHandler.ShootProjectile(NPC, type, damage, projectileSpeed, numProj, rotation, p => p.timeLeft = 600);
                    }

                    SendCommandToServants(BehaviorCommand_Servant.ShootBlood);
                }

                if (Timer1 >= phaseLimit && NPC.Distance(Target.Center) > 320f)
                    SelectNextAttack();

                NormalUpdateRotation();
                CheckPhaseChange();
            }

            void NormalCharge()
            {
                if (CurrentAttackPhase == 0) //冲刺
                {
                    NPC.damage = SetDamage;

                    SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
                    NPC.rotation = EyeRotation;

                    float chargeSpeed = (Ultra ? 20f : 15f) + 6f * (Phase2LifeRatio - NPC.LifeRatio);

                    if (ChargeCounter >= 1)
                        chargeSpeed *= 1.1f;
                    if (ChargeCounter >= 2)
                        chargeSpeed *= 1.1f;

                    Vector2 chargeVelocity = NPC.GetVelocityTowards(Target, chargeSpeed);

                    NPC.SetVelocityandRotation(NPC.SafeDirectionTo(Target.Center) * chargeSpeed, -MathHelper.PiOver2);
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

                        NPC.velocity *= MathHelper.Lerp(0.6f, 0.7f, decelerationScalar);

                        if (Math.Abs(NPC.velocity.X) < 0.1f)
                            NPC.velocity.X = 0f;
                        if (Math.Abs(NPC.velocity.Y) < 0.1f)
                            NPC.velocity.Y = 0f;
                    }
                    else
                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                    if (Timer1 >= phase2ChargeDelay && !CheckPhaseChange())
                        SelectNextAttack();
                }
            }

            void RapidCharge()
            {
                switch (CurrentAttackPhase) //冲刺
                {
                    case 0:
                        NPC.damage = SetDamage;

                        SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);

                        float baseChargeSpeed = 33f;
                        float speedBoost = 12f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float speedMultiplier = Ultra ? 1.3f : 1f;
                        float phase2_3Multiplier = Phase2_3 ? 1.2f : 1f;
                        float finalChargeSpeed = (baseChargeSpeed + speedBoost) * speedMultiplier * phase2_3Multiplier;

                        Vector2 eyeChargeDirection = NPC.Center;
                        float distanceX = Target.Center.X - eyeChargeDirection.X;
                        float distanceY = Target.Center.Y - eyeChargeDirection.Y;
                        float targetVelocity = Math.Abs(Target.velocity.X) + Math.Abs(Target.velocity.Y) / 4f;

                        if (targetVelocity < 2f)
                            targetVelocity = 2f;
                        if (targetVelocity > 6f)
                            targetVelocity = 6f;

                        if (ChargeCounter == 0)
                        {
                            targetVelocity *= 4f;
                            finalChargeSpeed *= 1.3f;
                        }

                        distanceX -= Target.velocity.X * targetVelocity;
                        distanceY -= Target.velocity.Y * targetVelocity / 4f;

                        float targetDistance = MathF.Sqrt(distanceX * distanceX + distanceY * distanceY);
                        float targetDistCopy = targetDistance;

                        targetDistance = finalChargeSpeed / targetDistance;
                        NPC.velocity.X = distanceX * targetDistance;
                        NPC.velocity.Y = distanceY * targetDistance;

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

                        NPC.VelocityToRotation(-MathHelper.PiOver2);

                        CurrentAttackPhase = 1;
                        NPC.netUpdate = true;

                        if (NPC.netSpam > 10)
                            NPC.netSpam = 10;
                        break;
                    case 1:
                        NPC.damage = SetDamage;

                        Timer1++;

                        if (Timer1 >= RapidChargeTime && (NPC.Distance(Target.Center) >= 200f || Timer2 > 0))
                        {
                            Timer2++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                        }
                        else
                            NPC.VelocityToRotation(-MathHelper.PiOver2);

                        if (Timer2 >= 15 && !CheckPhaseChange())
                            SelectNextAttack();
                        break;
                }
            }

            void HorizontalHover()
            {
                switch (CurrentAttackPhase)
                {
                    case 0:
                        int direction = NextChargeTypeIsHorizontal ? Math.Sign(ChargeCounter2 == 0 ? -Target.velocity.X : NPC.Center.X - Target.Center.X) : Math.Sign(NPC.Center.Y - Target.Center.Y);
                        if (direction == 0)
                            direction = Main.rand.NextBool(2) ? 1 : -1;
                        HorizontalHoverDirection = direction;
                        CurrentAttackPhase = 1;
                        break;
                    case 1:
                        Timer1++;

                        NPC.damage = ReducedSetDamage;

                        float baseHoverSpeed = 22.5f;
                        float baseHoverAcceleration = 0.45f;

                        float speedBoost = 10f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float accelerationBoost = 0.3f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float speedMultiplier = Ultra ? 1.25f : 1f;

                        float hoverSpeed = (baseHoverSpeed + speedBoost) * speedMultiplier;
                        float hoverAcceleration = baseHoverAcceleration + accelerationBoost * speedMultiplier;

                        float timeGateValue = 100f - 80f * (Phase2_2LifeRatio - NPC.LifeRatio);

                        if (Timer1 > timeGateValue)
                        {
                            float velocityScalar = Timer1 - timeGateValue;
                            hoverSpeed += velocityScalar * 0.075f;
                            hoverAcceleration += velocityScalar * 0.004f;
                        }

                        Vector2 hoverDestination = NextChargeTypeIsHorizontal ? Target.Center + new Vector2(800f * HorizontalHoverDirection, 0f)
                            : Target.Center + new Vector2(0f, 480f * HorizontalHoverDirection);
                        Vector2 idealHoverVelocity = NPC.GetVelocityTowards(hoverDestination, hoverSpeed);
                        NPC.SimpleFlyMovement(idealHoverVelocity, hoverAcceleration);

                        //弹幕
                        int projectileGateValue = NextChargeTypeIsHorizontal ? 20 : 30;
                        int maxServantSpawnsPerAttack = NextChargeTypeIsHorizontal ? (ChargeCounter == 0 ? (Ultra ? 3 : 2) : Ultra ? 2 : 1) : 1;
                        int projectileDelay = 10;
                        int adjustedTimer1 = Timer1 - projectileDelay;
                        if (adjustedTimer1 % projectileGateValue == 0)
                        {
                            int num = adjustedTimer1 / projectileGateValue;
                            if (num >= 0 && num < maxServantSpawnsPerAttack && TOSharedData.GeneralClient && CanShootProjectile() && Timer2 == 0)
                            {
                                bool buff = Ultra && NextChargeTypeIsHorizontal && ChargeCounter2 == 0;
                                int amount = buff ? 5 : 3;
                                float halfRange = MathHelper.ToRadians(buff ? 45f : 22.5f);
                                EoCHandler.ShootProjectile(NPC, ProjectileID.BloodNautilusShot, BloodDamage, 20f, amount, halfRange, p => p.timeLeft = 600);
                            }
                        }

                        if (Timer1 >= timeGateValue)
                        {
                            float requiredDistanceForHorizontalCharge = 200f;
                            Vector2 distance = hoverDestination - NPC.Center;
                            if (!NextChargeTypeIsHorizontal)
                                distance.X /= 20f;
                            if (distance.Length() < requiredDistanceForHorizontalCharge)
                            {
                                Timer2++;
                                int delay = NextChargeTypeIsHorizontal ? 12 : 4;
                                if (Timer2 > delay)
                                    SelectNextAttack();
                            }
                        }

                        break;
                }

                NormalUpdateRotation();
                CheckPhaseChange();
            }

            void HorizontalCharge()
            {
                switch (CurrentAttackPhase)
                {
                    case 0:
                        NPC.damage = SetDamage;

                        float baseChargeSpeed = 33f;
                        float speedBoost = 12f * (Phase2_2LifeRatio - NPC.LifeRatio);
                        float speedMultiplier = Ultra ? 1.25f : 1f;
                        float chargeSpeed = (baseChargeSpeed + speedBoost) * speedMultiplier;

                        NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target.Center, chargeSpeed), -MathHelper.PiOver2);
                        CurrentAttackPhase++;
                        NPC.netUpdate = true;

                        if (NPC.netSpam > 10)
                            NPC.netSpam = 10;
                        break;
                    case 1:
                        NPC.damage = SetDamage;

                        if (Timer1 == 0)
                            SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);

                        Timer1++;

                        if (Timer1 >= HorizontalChargeTime && (NPC.Distance(Target.Center) >= 200f || Timer2 > 0))
                        {
                            Timer2++;
                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                        }
                        else
                            NPC.VelocityToRotation(-MathHelper.PiOver2);

                        if (Timer2 > 15 && !CheckPhaseChange())
                            SelectNextAttack();
                        break;
                }
            }

            void ZenithAI()
            {
                if (TOSharedData.GeneralClient) //火焰
                {
                    Timer3++;

                    float newRotation = GFB_FlameRotation;
                    float targetRotation = ActualRotation;
                    EoCHandler.UpdateRotation(ref newRotation, targetRotation, 10f); //瞬间更新旋转
                    GFB_FlameRotation = newRotation;

                    float flamethrowerSpeed = 10f * Math.Clamp(Timer3 / 60f, 0f, 1f);
                    Vector2 flamethrowerVelocity = new PolarVector2(flamethrowerSpeed, GFB_FlameRotation) + NPC.velocity * 0.45f;
                    flamethrowerVelocity.Modulus = Math.Min(flamethrowerVelocity.Modulus, MathF.Sqrt(NPC.velocity.LengthSquared() + 16));
                    Projectile.NewProjectileAction<BloodFlame>(NPC.GetSource_FromAI(), NPC.Center + new PolarVector2(ProjectileOffset, ActualRotation), flamethrowerVelocity, GFBFlameDamage, 0f, Main.myPlayer);
                }
            }

            bool CheckPhaseChange()
            {
                if (NPC.LifeRatio >= Phase3LifeRatio)
                    return false;

                //进入阶段转换
                NPC.damage = 0;
                CurrentPhase = Phase.PhaseChange_2To3;
                CurrentBehavior = Behavior.PhaseChange_2To3;
                CurrentAttackPhase = 0;
                Timer1 = -90; //60帧缓冲时间
                ChargeCounter = 0;
                NPC.netUpdate = true;

                if (NPC.netSpam > 10)
                    NPC.netSpam = 10;

                return true;
            }
        }

        void PhaseChange_2To3()
        {
            Timer1++;

            NPC.damage = 0;
            NPC.rotation += PhaseChange_2To3_RotationSpeedFunction.Process(Timer1);
            StopMovement();

            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

            if (Timer1 >= PhaseChangeGateValue_2To3_1) //改变颜色
                Phase3ColorRatio += 0.025f;

            if (Timer1 is > 0 and <= PhaseChangeGateValue_2To3_2) //移动竞技场中心
                Phase3ArenaCenter = Vector2.SmootherStep(Phase3ArenaCenter, Target.Center, TOMathUtils.Interpolation.ExponentialEaseIn(Timer1 * 1.5f / PhaseChangeGateValue_2To3_2, 4f));

            if (Timer1 is >= PhaseChangeGateValue_2To3_2 and <= PhaseChangeGateValue_2To3_2 + 60) //回复血量
            {
                float ratio = (Timer1 - PhaseChangeGateValue_2To3_2) / 60f;
                int newLife = (int)MathHelper.Lerp(NPC.life, NPC.lifeMax * MathHelper.Lerp(0.1f, 0.5f, ratio), TOMathUtils.Interpolation.LogarithmicEaseOut(ratio));
                int increasedLife = Math.Clamp(newLife - NPC.life, 0, NPC.lifeMax / 2 - NPC.life);

                if (increasedLife > 0)
                {
                    NPC.life += increasedLife;
                    NPC.HealEffect(increasedLife, true);
                }

                if (NPC.life > NPC.lifeMax)
                    NPC.life = NPC.lifeMax;
            }

            if (Timer1 is >= PhaseChangeGateValue_2To3_2 - 10 and < PhaseChangeGateValue_2To3_2) //提前开始第三阶段传送
                TeleportTo(Vector2.Zero, (Timer1 - (PhaseChangeGateValue_2To3_2 - 10)) / 30f);

            switch (Timer1)
            {
                case 0:
                    Phase3ArenaCenter = NPC.Center;
                    SendCommandToServants(BehaviorCommand_Servant.GetToArenaPosition);
                    if (TOSharedData.GeneralClient)
                        Projectile.NewProjectileAction<EyeofCthulhuArena>(NPC.GetSource_FromAI(), Phase3ArenaCenter, Vector2.Zero, ArenaDamage, 0f, action: p =>
                        {
                            p.GetModProjectile<EyeofCthulhuArena>().Master = NPC;
                            ArenaProjectile = p;
                        });
                    break;
                case PhaseChangeGateValue_2To3_2:
                    SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    for (int i = 0; i < 20; i++)
                        Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.Blood, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f)));

                    //生成冲击波清除克苏鲁之仆；使竞技场激活，流血仆从脱战（通过弹幕实现），克苏鲁之仆死亡
                    Projectile.NewProjectileAction<BloodShockwave>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 100, 0f, action: p =>
                    {
                        p.scale = 0f;
                        BloodShockwave modP = p.GetModProjectile<BloodShockwave>();
                        modP.Master = NPC;
                    });

                    if (ArenaProjectileAlive)
                    {
                        EyeofCthulhuArena modP = ArenaModProjectile;
                        modP.IsActivated = true;
                        if (ServantLeftAlive)
                        {
                            ArenaProjectile.frameCounter = (int)ServantLeft.frameCounter;
                            ArenaProjectile.scale = ServantLeft.scale;
                            modP.ArenaRadius = ServantLeft.GetModNPC<BloodlettingServant>().ArenaRadius;
                        }
                        else if (ServantRightAlive)
                        {
                            ArenaProjectile.frameCounter = (int)ServantRight.frameCounter;
                            ArenaProjectile.scale = ServantRight.scale;
                            modP.ArenaRadius = ServantRight.GetModNPC<BloodlettingServant>().ArenaRadius;
                        }
                    }

                    ExecuteActionToServants((n, modN) =>
                    {
                        n.life = 0;
                        n.active = false;
                        n.netUpdate = true;
                    });

                    CurrentAttackPhase = 1;
                    break;
                case PhaseChangeTime_2To3: //进入三阶段
                    CurrentPhase = Phase.Phase3;
                    CurrentBehavior = Behavior.Phase3_Charge;
                    CurrentAttackPhase = 0;
                    Timer1 = 10;
                    Timer2 = 0;
                    break;
            }
        }
        #endregion 行为函数

        void Phase3AI()
        {
            if (!ArenaProjectileAlive)
                return;

            switch (CurrentBehavior)
            {
                case Behavior.Phase3_Charge:
                    Charge();
                    break;
                case Behavior.Phase3_2:
                    P32();
                    break;
                case Behavior.Phase3_3:
                    P33();
                    break;
            }

            void SelectNextAttack()
            {
                CurrentAttackPhase = 0;
                Timer1 = 0;
                Timer2 = 0;
                switch (CurrentBehavior)
                {
                    case Behavior.Phase3_Charge:
                        ChargeCounter++;
                        if (ChargeCounter >= 7)
                        {
                            ChargeCounter = 0;
                            //CurrentBehavior = Behavior.Phase3_2;
                        }
                        break;
                    case Behavior.Phase3_2:
                        CurrentBehavior = Behavior.Phase3_3;
                        break;
                    case Behavior.Phase3_3:
                        CurrentBehavior = Behavior.Phase3_Charge;
                        break;
                }
            }

            void Charge()
            {
                switch (ChargeCounter)
                {
                    case <= 2: //普通冲刺
                        NormalCharge();
                        break;
                    case 3: //第一次快速冲刺
                        FirstRapidCharge();
                        break;
                    case >= 4 and <= 6: //后三次快速冲刺
                        RestRapidCharge();
                        break;
                    default:
                        SelectNextAttack();
                        break;
                }

                int CalculateDistance(int index1, int index2) => Math.Abs((int)TOMathUtils.ShortestDifference(index1, index2, 32f));

                void SpawnChargeParticle(int index) => ExecuteActionToArenaEye(index, e =>
                {
                    int particleAmount = 25;
                    for (int i = 0; i < particleAmount; i++)
                        EoCHandler.SpawnOrbParticle(e.Center, Main.rand.NextFloat(3f, 4f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.5f, 0.8f));
                });

                void BehaviorDuringCharge(int firstAttackPhase)
                {
                    NPC.damage = SetDamage;
                    NPC.VelocityToRotation(-MathHelper.PiOver2);
                    if (CurrentAttackPhase == firstAttackPhase && NPC.Distance(ArenaProjectile.Center) <= ArenaModProjectile.ArenaRadius + 20f)
                        CurrentAttackPhase = firstAttackPhase + 1;
                    if ((CurrentAttackPhase == firstAttackPhase + 1 && NPC.Distance(ArenaProjectile.Center) > ArenaModProjectile.ArenaRadius + 100f) || Timer2 > 0)
                    {
                        StopMovement();
                        Timer2++;
                        if (Timer2 >= 5)
                            SelectNextAttack();
                    }
                }

                void NormalCharge()
                {
                    bool firstCharge = ChargeCounter == 0;
                    bool shouldUseIndex2 = ChargeCounter >= 1;
                    bool shouldUseIndex3 = ChargeCounter >= 2;
                    bool shouldUseIndex4 = shouldUseIndex3 && Phase3_2;

                    float teleportDuration = firstCharge ? EoCHandler.NormalTeleportDuration + 30f : EoCHandler.NormalTeleportDuration;

                    switch (CurrentAttackPhase)
                    {
                        case 0: //初始化
                            CurrentAttackPhase = 1;

                            NPC.damage = ReducedSetDamage;
                            if (firstCharge)
                                ArenaModProjectile.ChangeRotationSpeedTo(0f, 15);
                            int usedIndex1 = Main.rand.Next(0, 32);
                            UsedEyeIndex1 = usedIndex1;

                            if (shouldUseIndex2)
                            {
                                int usedIndex2 = (int)TOMathUtils.NormalizeWithPeriod(usedIndex1 + Main.rand.Next(5, 17) * Main.rand.NextBool(2).ToDirectionInt(), 32);
                                UsedEyeIndex2 = usedIndex2;
                                if (shouldUseIndex3)
                                {
                                    int usedIndex3;
                                    do
                                    {
                                        usedIndex3 = Main.rand.Next(0, 32);
                                    } while (CalculateDistance(usedIndex1, usedIndex3) < 4 || CalculateDistance(usedIndex2, usedIndex3) < 4);
                                    UsedEyeIndex3 = usedIndex3;
                                    if (shouldUseIndex4)
                                    {
                                        int usedIndex4;
                                        do
                                        {
                                            usedIndex4 = Main.rand.Next(0, 32);
                                        } while (CalculateDistance(usedIndex1, usedIndex4) < 3 || CalculateDistance(usedIndex2, usedIndex4) < 3 || CalculateDistance(usedIndex3, usedIndex4) < 3);
                                        UsedEyeIndex4 = usedIndex4;
                                    }
                                }
                            }
                            goto case 1;
                        case 1: //传送
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            Vector2 destination = ArenaProjectile.Center + new PolarVector2(ArenaModProjectile.ArenaRadius + 250f, ArenaModProjectile.GetEyeRotation(UsedEyeIndex1));
                            TeleportTo(destination, Timer1 / teleportDuration);

                            if (Timer1 == teleportDuration)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 2;
                            }
                            NormalUpdateRotation();
                            break;
                        case 2: //冲刺初始化
                            NPC.damage = SetDamage;
                            SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                            SpawnChargeParticle(UsedEyeIndex1);
                            NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, 30f), -MathHelper.PiOver2);
                            CurrentAttackPhase = 3;
                            break;
                        case 3 or 4: //冲刺中
                            BehaviorDuringCharge(3);
                            break;
                    }

                    if (Timer1 - 1 == teleportDuration - EoCHandler.NormalTeleportDuration)
                    {
                        for (int i = -1; i <= 1; i++)
                            AddHighlightTo((int)TOMathUtils.NormalizeWithPeriod(UsedEyeIndex1 + i, 32), EoCHandler.NormalTeleportDuration + 10);

                        if (shouldUseIndex2)
                        {
                            BehaviorCommand_ArenaEye command = Phase3_2 ? BehaviorCommand_ArenaEye.ShootBlood2 : BehaviorCommand_ArenaEye.ShootBlood;
                            SendCommandToArenaEye(UsedEyeIndex2, command);
                            if (shouldUseIndex3)
                            {
                                SendCommandToArenaEye(UsedEyeIndex3, command);
                                if (shouldUseIndex4)
                                    SendCommandToArenaEye(UsedEyeIndex4, command);
                            }
                        }
                    }
                }

                void FirstRapidCharge()
                {
                    switch (CurrentAttackPhase)
                    {
                        case 0: //初始化：一次性生成4个Index
                            CurrentAttackPhase = 1;

                            NPC.damage = ReducedSetDamage;
                            int usedIndex1 = Main.rand.Next(0, 32);
                            UsedEyeIndex1 = usedIndex1;
                            int usedIndex2 = (int)TOMathUtils.NormalizeWithPeriod(usedIndex1 + Main.rand.Next(5, 17) * Main.rand.NextBool(2).ToDirectionInt(), 32);
                            UsedEyeIndex2 = usedIndex2;
                            int usedIndex3;
                            do
                            {
                                usedIndex3 = Main.rand.Next(0, 32);
                            } while (CalculateDistance(usedIndex1, usedIndex3) < 4 || CalculateDistance(usedIndex2, usedIndex3) < 4);
                            UsedEyeIndex3 = usedIndex3;
                            int usedIndex4;
                            do
                            {
                                usedIndex4 = Main.rand.Next(0, 32);
                            } while (CalculateDistance(usedIndex1, usedIndex4) < 3 || CalculateDistance(usedIndex2, usedIndex4) < 3 || CalculateDistance(usedIndex3, usedIndex4) < 3);
                            UsedEyeIndex4 = usedIndex4;
                            goto case 1;
                        case 1: //一次性生成4个高光
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            switch (Timer1)
                            {
                                case 5: //第一个高光
                                    AddHighlightTo(UsedEyeIndex1, 135);
                                    break;
                                case 35: //第二个高光
                                    AddHighlightTo(UsedEyeIndex2, 135);
                                    break;
                                case 65: //第三个高光
                                    AddHighlightTo(UsedEyeIndex3, 135);
                                    break;
                                case 95: //第四个高光，进入传送冲刺阶段
                                    AddHighlightTo(UsedEyeIndex4, 135);
                                    Timer1 = 0;
                                    CurrentAttackPhase = 2;
                                    break;
                            }
                            break;
                        case 2: //传送
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            float teleportDuration = 80f;
                            Vector2 destination = ArenaProjectile.Center + new PolarVector2(ArenaModProjectile.ArenaRadius + 250f, ArenaModProjectile.GetEyeRotation(UsedEyeIndex1));
                            TeleportTo(destination, Timer1 / teleportDuration);

                            if (Timer1 == teleportDuration)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 3;
                            }
                            break;
                        case 3: //冲刺初始化
                            SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                            SpawnChargeParticle(UsedEyeIndex1);
                            NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, 40f), -MathHelper.PiOver2);
                            NPC.damage = SetDamage;
                            CurrentAttackPhase = 4;
                            break;
                        case 4 or 5: //冲刺中
                            BehaviorDuringCharge(4);
                            break;
                    }
                }

                void RestRapidCharge()
                {
                    int usedIndex = ChargeCounter switch
                    {
                        4 => UsedEyeIndex2,
                        5 => UsedEyeIndex3,
                        6 => UsedEyeIndex4,
                        _ => UsedEyeIndex1
                    };

                    switch (CurrentAttackPhase)
                    {
                        case 0: //传送
                            Timer1++;

                            NPC.damage = ReducedSetDamage;
                            StopMovement();
                            float teleportDuration = 16f;
                            Vector2 destination = ArenaProjectile.Center + new PolarVector2(ArenaModProjectile.ArenaRadius + 250f, ArenaModProjectile.GetEyeRotation(usedIndex));
                            TeleportTo(destination, Timer1 / teleportDuration);

                            if (Timer1 == teleportDuration)
                            {
                                Timer1 = 0;
                                CurrentAttackPhase = 1;
                            }
                            break;
                        case 1: //冲刺初始化
                            SoundEngine.PlaySound(SoundID.ForceRoarPitched, NPC.Center);
                            SpawnChargeParticle(usedIndex);
                            NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, 42f), -MathHelper.PiOver2);
                            NPC.damage = SetDamage;
                            CurrentAttackPhase = 2;
                            break;
                        case 2 or 3: //冲刺中
                            BehaviorDuringCharge(2);
                            break;
                    }
                }
            }

            void P32()
            {

            }

            void P33()
            {

            }

            void AddHighlightTo(int index, int lifetime)
            {
                SoundEngine.PlaySound(SoundID.Item8, ArenaModProjectile.GetEyeCenter(index));
                ExecuteActionToArenaEye(index, e => e.Highlights.Add(new EyeofCthulhuArena.EyeHighlight(lifetime, 20, 10f)));
            }
        }
    }

    private void ExecuteActionToLeftServant(Action<NPC, BloodlettingServant> action)
    {
        if (ServantLeftAlive)
            action?.Invoke(ServantLeft, ServantLeft.GetModNPC<BloodlettingServant>());
    }

    private void ExecuteActionToRightServant(Action<NPC, BloodlettingServant> action)
    {
        if (ServantRightAlive)
            action?.Invoke(ServantRight, ServantRight.GetModNPC<BloodlettingServant>());
    }

    private void ExecuteActionToServants(Action<NPC, BloodlettingServant> action)
    {
        ExecuteActionToLeftServant(action);
        ExecuteActionToRightServant(action);
    }

    private void ExecuteActionToArenaEye(int index, Action<EyeofCthulhuArena.ArenaEye> action)
    {
        if (ArenaProjectileAlive)
        {
            index = (int)TOMathUtils.NormalizeWithPeriod(index, 32);
            ArenaProjectile.GetModProjectile<EyeofCthulhuArena>().ExecuteActionToArenaEye(index, action);
        }
    }

    private void SendCommandToServants(BehaviorCommand_Servant command) => ExecuteActionToServants((n, modN) => modN.MasterCommandReceiver = command);

    private void SendCommandToArenaEye(int index, BehaviorCommand_ArenaEye command) => ExecuteActionToArenaEye(index, e => e.MasterCommandReceiver = command);

    #endregion AI

    public override void FindFrame(int frameHeight)
    {
        int frameNum;

        NPC.frameCounter += 1.0;

        switch (NPC.frameCounter)
        {
            case < 7.0:
                frameNum = 0;
                break;
            case < 14.0:
                frameNum = 1;
                break;
            case < 21.0:
                frameNum = 2;
                break;
            default:
                NPC.frameCounter = 0.0;
                frameNum = 0;
                break;
        }

        bool shouldUsePhase2Frame = CurrentPhase switch
        {
            Phase.Phase2 or Phase.PhaseChange_2To3 or Phase.Phase3 => true,
            Phase.PhaseChange_1To2 => Timer1 >= PhaseChangeGateValue_1To2_2,
            _ => false
        };

        if (shouldUsePhase2Frame)
            frameNum += 3;

        NPC.frame.Y = frameNum * frameHeight;
    }

    public override Color? GetAlpha(Color drawColor)
    {
        if (Phase3ColorRatio > 0f)
        {
            Color phase3Color = Color.Lerp(Color.DarkRed, Color.Tomato, 0.4f);
            return Color.Lerp(drawColor, phase3Color, Phase3ColorRatio * 0.6f);
        }

        return null;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D npcTexture = NPC.Texture;
        Color originalColor = NPC.GetAlpha(drawColor);
        Rectangle frame = NPC.frame;

        if (CurrentBehavior == Behavior.Phase3_Charge)
        {
            int afterimageAmount = 5;
            for (int j = 0; j < afterimageAmount; j++)
            {
                Color afterimageColor = originalColor * ((afterimageAmount - j) / 10f);
                Vector2 afterimagePos = NPC.oldPos[j] + new Vector2(NPC.width, NPC.height) / 2f - new PolarVector2(24f, NPC.oldRot[j] + MathHelper.PiOver2) - screenPos;
                spriteBatch.DrawFromCenter(npcTexture, afterimagePos, frame, afterimageColor, NPC.oldRot[j], NPC.scale);
            }
        }

        spriteBatch.DrawFromCenter(npcTexture, NPC.Center - new PolarVector2(24f, ActualRotation) - screenPos, frame, originalColor, NPC.rotation, NPC.scale);
        return false;
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (!Ultra)
            return;

        if (Phase3)
            modifiers.SourceDamage *= 1.25f;
        else
            modifiers.SetMaxDamage((int)(NPC.life - NPC.lifeMax * Phase3LifeRatio));
    }

    public override bool CheckDead()
    {
        if (Ultra && !Phase3 && !NPC.downedBoss1)
        {
            NPC.life = 1;
            NPC.active = true;
            NPC.netUpdate = true;
            return false;
        }

        return true;
    }

    public override void BossHeadSlot(ref int index)
    {
        if (!IsInPhase3Arena)
            index = -1;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => !IsInPhase3Arena ? false : null;
}
