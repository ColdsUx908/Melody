using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.Anomaly.KingSlime;

public class AnomalyKingSlime : AnomalyNPCBehavior
{
    #region 枚举、数值、属性、AI状态
    public enum Behavior
    {
        Despawn = -1,

        None = 0,

        NormalJump_Phase1 = 1,
        HighJump_Phase1 = 2,
        RapidJump_Phase1 = 3,
        Teleport_Phase1 = 4,

        PhaseChange_1And2 = 5,
    }

    public static class Data
    {
        public const float DespawnDistance = 5000f;

        public static float MaxScale => CAWorld.AnomalyUltramundane ? 7.5f : 6f;

        public static float MinScale => 0.5f;

        public static float SpawnSlimeGateValue => CAWorld.AnomalyUltramundane ? 0.025f : 0.03f;

        public static float SpawnSlimePow => CAWorld.AnomalyUltramundane ? 0.5f : 0.3f;

    }

    /// <summary>
    /// 当前阶段。0，1，2。
    /// </summary>
    public int CurrentPhase
    {
        get => (int)NPC.ai[0];
        set => NPC.ai[0] = value;
    }

    public Behavior CurrentBehavior
    {
        get => (Behavior)(int)NPC.ai[1];
        set => NPC.ai[1] = (int)value;
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    public int LastSpawnSlimeLife
    {
        get => (int)NPC.ai[3];
        set => NPC.ai[3] = value;
    }

    public float TeleportTimer
    {
        get => NPC.localAI[0];
        set => NPC.localAI[0] = value;
    }

    public int SmallJumpCounter
    {
        get => (int)NPC.localAI[1];
        set => NPC.localAI[1] = value;
    }

    public int ChangedVelocityDirectionDuringJump
    {
        get => (int)NPC.localAI[2];
        set => NPC.localAI[2] = value;
    }

    public float TeleportScaleMultiplier
    {
        get => NPC.localAI[3];
        set => NPC.localAI[3] = value;
    }

    public float DespawnScaleMultiplier
    {
        get => CalamityNPC.newAI[0];
        set
        {
            if (CalamityNPC.newAI[0] != value)
            {
                CalamityNPC.newAI[0] = value;
                NPC.SyncExtraAI();
            }
        }
    }

    #region 宝石
    public bool JewelEmeraldSpawned
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
    /// 王冠绿宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public unsafe NPC JewelEmerald
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

    public bool JewelEmeraldAlive => JewelEmerald.active && JewelEmerald.ModNPC is KingSlimeJewelEmerald && JewelEmerald.Ocean().Master == NPC.whoAmI;

    public bool JewelRubySpawned
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
    /// 王冠红宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public unsafe NPC JewelRuby
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

    public bool JewelRubyAlive => JewelRuby.active && JewelRuby.ModNPC is KingSlimeJewelRuby && JewelRuby.Ocean().Master == NPC.whoAmI;

    public bool JewelSapphireSpawned
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
    /// 王冠蓝宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public unsafe NPC JewelSapphire
    {
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].bytes[2]];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].bytes[2] != temp)
            {
                AnomalyNPC.AnomalyAI32[1].bytes[2] = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }

    public bool JewelSapphireAlive => JewelSapphire.active && JewelSapphire.ModNPC is KingSlimeJewelSapphire && JewelSapphire.Ocean().Master == NPC.whoAmI;

    #endregion 宝石

    public Vector2 TeleportDestination
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


    #endregion 枚举、数值、属性、AI状态

    public override int ApplyingType => NPCID.KingSlime;

    public override bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC method) => method switch
    {
        OrigMethodType_CalamityGlobalNPC.PreAI => false,
        OrigMethodType_CalamityGlobalNPC.GetAlpha => false,
        _ => true,
    };

    public override void SetDefaults()
    {
        if (CAWorld.AnomalyUltramundane)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 1.25f);
            NPC.Calamity().DR += 0.1f;
        }

        TeleportScaleMultiplier = 1f;
        DespawnScaleMultiplier = 1f;

        JewelEmerald = NPC.DummyNPC;
        JewelRuby = NPC.DummyNPC;
        JewelSapphire = NPC.DummyNPC;
    }

    public override bool CheckActive() => false;

    public override bool PreAI()
    {
        if (CurrentBehavior == Behavior.Despawn || !NPC.TargetClosestIfInvalid(true, Data.DespawnDistance))
        {
            Despawn();
            ChangeScale();
            return false;
        }
        else
            NPC.FaceNPCTarget(Target);

        switch (CurrentPhase)
        {
            case 0: //初始化
                Timer1++;
                if (NPC.velocity.Y == 0f)
                    Timer2++;
                if (Timer2 > 20)
                {
                    NPC.velocity = Vector2.Zero;
                    LastSpawnSlimeLife = NPC.life;
                    CurrentPhase = 1;
                    SelectNextAttack();
                }
                else if (Timer1 > 600)
                {
                    CurrentBehavior = Behavior.Despawn;
                    Despawn();
                }
                break;
            case 1:
                switch (CurrentBehavior)
                {
                    case Behavior.Despawn:
                        Despawn();
                        ChangeScale();
                        return false;
                    case Behavior.None:
                        SelectNextAttack();
                        break;
                    case Behavior.NormalJump_Phase1:
                    case Behavior.HighJump_Phase1:
                    case Behavior.RapidJump_Phase1:
                        Jump();
                        break;
                    case Behavior.Teleport_Phase1:
                        Teleport();
                        break;
                }
                break;
        }

        TeleportTimer += Math.Max(NPC.Center.Y - Target.Center.Y, 0f) > 0f ? 3f : MathHelper.Lerp(1f, 1.5f, OceanNPC.LifeRatioReverse);

        ChangeScale();
        TrySpawnMinions();

        NPC.netUpdate = true;

        return false;


        #region 行为函数
        void SelectNextAttack(int initialAITimer1 = 0)
        {
            ChangedVelocityDirectionDuringJump = 0;
            if (TeleportTimer > 1000f || !NPC.WithinRange(Target.Center, 3000f))
            {
                CurrentBehavior = Behavior.Teleport_Phase1;
                TeleportTimer = 0f;
            }
            else
            {
                switch (SmallJumpCounter)
                {
                    case 3:
                        CurrentBehavior = Behavior.HighJump_Phase1;
                        SmallJumpCounter = 0;
                        break;
                    case 1 or 2 when OceanNPC.LifeRatio < 0.3f:
                        CurrentBehavior = Behavior.RapidJump_Phase1;
                        break;
                    default:
                        CurrentBehavior = Behavior.NormalJump_Phase1;
                        break;
                }
            }
            CurrentAttackPhase = 0;
            Timer1 = initialAITimer1;
        }

        bool StopHorizontalMovement()
        {
            switch (Math.Abs(NPC.velocity.X))
            {
                case < 0.1f:
                    NPC.velocity.X = 0f;
                    return true;
                case > 0.2f:
                    NPC.velocity.X -= 0.15f * Math.Sign(NPC.velocity.X);
                    break;
            }
            NPC.velocity.X *= 0.85f;
            return false;
        }

        void MakeSlimeDust(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Dust.NewDustAction(NPC.Center, NPC.width + 25, NPC.height,
                    JewelSapphireAlive ? DustID.GemSapphire : DustID.TintableDust, default, d =>
                    {
                        d.alpha = 150;
                        d.color = new(78, 136, 255, 80);
                        d.noGravity = true;
                        d.scale = TOMathHelper.Map(Data.MinScale, Data.MaxScale, 2f, 5f, NPC.scale, true);
                        d.velocity *= 0.5f;
                    });
            }
        }

        void Despawn()
        {
            StopHorizontalMovement(); //停止水平移动，避免奇怪的滑行现象
            NPC.dontTakeDamage = true;
            NPC.damage = 0;
            DespawnScaleMultiplier *= 0.97f;
            MakeSlimeDust((int)TOMathHelper.Map(Data.MinScale, Data.MaxScale, 5f, 12.5f, NPC.scale, true));

            if (NPC.scale < 0.2f) //体积足够小时执行脱战逻辑
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }

        static void MakeJewelDust(NPC jewel, int amount)
        {
            short type = jewel.ModNPC switch
            {
                KingSlimeJewelEmerald => DustID.GemEmerald,
                KingSlimeJewelRuby => DustID.GemRuby,
                KingSlimeJewelSapphire => DustID.GemSapphire,
                _ => -1
            };
            if (type < 0)
                return;

            for (int i = 0; i < amount; i++)
            {
                Dust.NewDustAction(jewel.Center, jewel.width * 3, jewel.height * 3, type, Vector2.Zero, d =>
                {
                    d.alpha = 100;
                    d.noGravity = true;
                    d.velocity *= Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(1f, 2f);
                    if (Main.rand.NextBool())
                    {
                        d.scale = 0.6f;
                        d.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                    else
                        d.scale = 2f;
                });
            }
        }

        void ChangeScale() =>
            NPC.BetterChangeScale(98, 92, MathHelper.Lerp(Data.MaxScale, Data.MinScale, OceanNPC.LifeRatioReverse) * TeleportScaleMultiplier * DespawnScaleMultiplier);

        void TrySpawnMinions()
        {
            if (!TOWorld.GeneralClient)
                return;

            Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);

            if (OceanNPC.LifeRatio < 0.8f && !JewelEmeraldSpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelEmerald>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
                    n.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item38, spawnPosition);
                    MakeJewelDust(n, 50);
                    JewelEmerald = n;
                    JewelEmeraldSpawned = true;
                });
            }

            if (OceanNPC.LifeRatio < 0.6f && !JewelRubySpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelRuby>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
                    n.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item38, spawnPosition);
                    MakeJewelDust(n, 50);
                    JewelRuby = n;
                    JewelRubySpawned = true;
                });
            }

            if (OceanNPC.LifeRatio < 1f / 3f && !JewelSapphireSpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelSapphire>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
                    n.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item38, spawnPosition);
                    MakeJewelDust(n, 50);
                    JewelSapphire = n;
                    JewelSapphireSpawned = true;
                });
            }

            float distance = (float)(LastSpawnSlimeLife - NPC.life) / NPC.lifeMax;
            float distanceNeeded = Data.SpawnSlimeGateValue;
            if (distance >= distanceNeeded)
            {
                LastSpawnSlimeLife = NPC.life;
                int spawnAmount1 = Main.rand.Next(1, 3) + (int)Math.Pow(distance / distanceNeeded, Data.SpawnSlimePow);
                int spawnAmount2 = CAWorld.AnomalyUltramundane ? Main.rand.Next(1, 2) : 0;

                for (int i = 0; i < spawnAmount1; i++)
                {
                    int minTypeChoice = (int)MathHelper.Lerp(i < 2 ? 0 : 5, 7f, 1f - OceanNPC.LifeRatio);
                    int maxTypeChoice = (int)MathHelper.Lerp((float)7f, 9f, 1f - OceanNPC.LifeRatio);
                    int spawnType = Main.rand.Next(minTypeChoice, maxTypeChoice + 1) switch
                    {
                        0 => NPCID.GreenSlime,
                        1 => Main.raining ? NPCID.UmbrellaSlime : NPCID.BlueSlime,
                        2 => NPCID.IceSlime,
                        3 => NPCID.RedSlime,
                        4 => NPCID.PurpleSlime,
                        5 => NPCID.YellowSlime,
                        6 => NPCID.SlimeSpiked,
                        7 => NPCID.SpikedIceSlime,
                        8 => NPCID.SpikedJungleSlime,
                        _ => NPCID.SlimeSpiked,
                    };

                    SpawnSlimeCore(spawnType);
                }

                //生成彩虹史莱姆
                for (int i = 0; i < spawnAmount2; i++)
                    SpawnSlimeCore(NPCID.RainbowSlime);

                ///生成粉史莱姆
                if (Main.rand.NextBool(4))
                    SpawnSlimeCore(NPCID.Pinky);
            }

            void SpawnSlimeCore(int type)
            {
                int spawnZoneWidth = NPC.width - 32;
                int spawnZoneHeight = NPC.height - 32;
                Vector2 spawnPosition = new(NPC.position.X + Main.rand.NextFloat(spawnZoneWidth), NPC.position.Y + Main.rand.NextFloat(spawnZoneHeight));
                NPC.NewNPCAction(NPC.GetSource_FromAI(), spawnPosition, type, action: n =>
                {
                    n.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, 3f));
                    n.ai[0] = -1000 * Main.rand.Next(3);
                    n.Ocean().Master = NPC.whoAmI;
                });
            }
        }

        void Jump()
        {
            switch (CurrentAttackPhase)
            {
                case 0: //静止一段时间后起跳
                    Timer2++;
                    NPC.damage = 0;
                    NPC.netUpdate = true;
                    NPC.GravityMultiplier *= Timer2 > 25 && NPC.velocity.Y > 0f ? 1.25f : 1f;
                    NPC.MaxFallSpeedMultiplier *= Timer2 > 20 && NPC.velocity.Y > 0f ? 1.35f : 1f;
                    if (StopHorizontalMovement() && NPC.velocity.Y == 0f)
                    {
                        Timer1++;
                        int jumpDelay = (int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 20f : 27.5f, CAWorld.AnomalyUltramundane ? 15f : 20f, OceanNPC.LifeRatioReverse);
                        if (Timer1 > jumpDelay)
                        {
                            if (CurrentBehavior != Behavior.HighJump_Phase1)
                                SmallJumpCounter++;
                            NPC.damage = NPC.defDamage;
                            NPC.netUpdate = true;
                            NPC.velocity = GetInitialVelocity();
                            CurrentAttackPhase = 1;
                            Timer1 = 0;
                        }
                    }
                    break;
                case 1: //上升
                case 2: //下降
                    NPC.damage = NPC.defDamage;
                    if (CurrentBehavior == Behavior.RapidJump_Phase1 || NPC.velocity.X * NPC.direction > 0.1f)
                        NPC.velocity.X = Math.Min(Math.Abs(NPC.velocity.X) + GetDeltaVelocityX(), GetMaxVelocityX()) * Math.Sign(NPC.velocity.X);
                    else
                    {
                        NPC.velocity.X *= 0.93f;
                        switch (Math.Abs(NPC.velocity.X))
                        {
                            case < 0.1f:
                                ChangedVelocityDirectionDuringJump++;
                                NPC.velocity.X += GetDeltaVelocityX() * NPC.direction;
                                break;
                            case > 0.25f:
                                NPC.velocity.X -= 0.2f * Math.Sign(NPC.velocity.X);
                                break;
                        }
                    }
                    switch (CurrentAttackPhase)
                    {
                        case 1:
                            if (NPC.velocity.Y >= 0) //检测是否已过最高点
                                CurrentAttackPhase = 2;
                            break;
                        case 2:
                            if (NPC.velocity.Y == 0f)
                            {
                                TeleportTimer += CurrentBehavior == Behavior.HighJump_Phase1 ? 300f : 100f;
                                SelectNextAttack(CurrentBehavior == Behavior.RapidJump_Phase1 ? (int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 10 : 20, CAWorld.AnomalyUltramundane ? 7.5f : 12.5f, OceanNPC.LifeRatioReverse) : 0);
                            }
                            else
                            {
                                NPC.GravityMultiplier *= CurrentBehavior switch
                                {
                                    Behavior.HighJump_Phase1 => CAWorld.AnomalyUltramundane ? 1.5f : 1.25f,
                                    _ => 1f
                                };
                                NPC.MaxFallSpeedMultiplier *= CurrentBehavior switch
                                {
                                    Behavior.HighJump_Phase1 => CAWorld.AnomalyUltramundane ? 2.25f : 1.75f,
                                    _ => 1f
                                };
                            }
                            break;
                    }
                    break;
            }

            Vector2 GetInitialVelocity() => CurrentBehavior switch
            {
                Behavior.NormalJump_Phase1 => new(
                    MathHelper.Lerp(5f, 7.5f, OceanNPC.LifeRatioReverse) * NPC.direction,
                    -7.5f * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 800f, 0f, 0.75f))),
                Behavior.HighJump_Phase1 => new(
                    MathHelper.Lerp(7.5f, 10f, OceanNPC.LifeRatioReverse) * NPC.direction,
                    -10f * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 800f, 0f, 1.25f))),
                Behavior.RapidJump_Phase1 => new(
                    MathHelper.Lerp(10f, 12.5f, OceanNPC.LifeRatioReverse) * NPC.direction,
                    -5f),
                _ => Vector2.Zero
            };

            float GetMaxVelocityX() => CurrentBehavior switch
            {
                Behavior.RapidJump_Phase1 => 18f,
                _ => ChangedVelocityDirectionDuringJump switch
                {
                    0 => 12.5f,
                    1 => 8f,
                    _ => 6.5f
                }
            };

            float GetDeltaVelocityX() => CurrentBehavior switch
            {
                Behavior.RapidJump_Phase1 => ChangedVelocityDirectionDuringJump switch
                {
                    0 => 0.8f,
                    1 => 0.55f,
                    _ => 0.35f
                },
                _ => ChangedVelocityDirectionDuringJump switch
                {
                    0 => 0.5f,
                    1 => 0.4f,
                    _ => 0.25f
                }
            };
        }

        void Teleport()
        {
            NPC.damage = 0;
            switch (CurrentAttackPhase)
            {
                case 0: //寻的
                    Vector2? destination = null;
                    //Vector2 randomDefault = Main.rand.NextBool() ? Vector2.UnitX : -Vector2.UnitX;
                    Vector2 vectorAimedAheadOfTarget = Target.Center + new Vector2(MathF.Round(Target.velocity.X / 2f), 0f).ToCustomLength(800f);
                    Point predictiveTeleportPoint = vectorAimedAheadOfTarget.ToTileCoordinates();
                    predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                    predictiveTeleportPoint.Y = Math.Clamp(predictiveTeleportPoint.Y, 10, Main.maxTilesY - 10);
                    int randomPredictiveTeleportOffset = 5;

                    for (int i = 0; i < 100; i++)
                    {
                        int teleportTileX = Main.rand.Next(predictiveTeleportPoint.X - randomPredictiveTeleportOffset, predictiveTeleportPoint.X + randomPredictiveTeleportOffset + 1);
                        int teleportTileY = Main.rand.Next(predictiveTeleportPoint.Y - randomPredictiveTeleportOffset, predictiveTeleportPoint.Y);
                        Tile potentialTile = Main.tile[teleportTileX, teleportTileY];
                        if (!potentialTile.HasUnactuatedTile)
                        {
                            if (potentialTile.LiquidType != LiquidID.Lava
                                && Collision.CanHitLine(NPC.Center, 0, 0, predictiveTeleportPoint.ToVector2() * 16, 0, 0))
                            {
                                destination = new Vector2((teleportTileX + 0.5f) * 16f, (teleportTileY + 1f) * 16f);
                                break; //在此处退出循环
                            }
                            else
                            {
                                predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                                predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                            }
                        }
                        else
                        {
                            predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                            predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                        }
                    }

                    TeleportDestination = destination ?? Target.Bottom;
                    CurrentAttackPhase = 1;
                    break;
                case 1: //停止水平移动并缩小体型，满足条件时传送
                    MakeSlimeDust((int)TOMathHelper.Map(Data.MinScale, Data.MaxScale, 5f, 12.5f, NPC.scale, true));
                    TeleportScaleMultiplier -= MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 0.016f : 0.013f, CAWorld.AnomalyUltramundane ? 0.02f : 0.015f, OceanNPC.LifeRatioReverse);
                    if (StopHorizontalMovement() && TeleportScaleMultiplier <= 0.2f)
                    {
                        Gore.NewGoreAction(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-40f, -NPC.height / 2f), NPC.velocity, GoreID.KingSlimeCrown, g => g.timeLeft += 180);
                        NPC.Bottom = TeleportDestination;
                        if (JewelSapphireAlive) //移动蓝宝石
                        {
                            JewelSapphire.Center = NPC.Center - new Vector2(0f, 200f);
                            MakeJewelDust(JewelSapphire, 20);
                        }
                        CurrentAttackPhase = 2;
                    }
                    break;
                case 2: //恢复体型，恢复完成后开始下一次攻击
                    TeleportScaleMultiplier += MathHelper.Lerp(0.03f, 0.05f, OceanNPC.LifeRatioReverse);
                    if (TeleportScaleMultiplier >= 1f)
                    {
                        TeleportScaleMultiplier = 1f;
                        SelectNextAttack((int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 10 : 20, CAWorld.AnomalyUltramundane ? 5 : 12.5f, OceanNPC.LifeRatioReverse));
                    }
                    break;
            }
        }
        #endregion 行为函数
    }

    public override Color? GetAlpha(Color drawColor)
    {
        if (JewelSapphireAlive)
            return Color.Lerp(new Color(0, 0, 150, NPC.alpha), new Color(125, 125, 255, NPC.alpha), TOMathHelper.GetTimeSin(0.5f, unsigned: true));

        return null;
    }
}
