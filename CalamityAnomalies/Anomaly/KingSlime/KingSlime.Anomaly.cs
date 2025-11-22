using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.Anomaly.KingSlime;

public class KingSlime_Anomaly : AnomalyNPCBehavior, ILocalizationPrefix
{
    #region 数据
    public enum Behavior
    {
        Despawn = -1,

        None = 0,

        NormalJump = 1,
        HighJump = 2,
        RapidJump = 3,
        Teleport = 4,

        PhaseChange_P1To2 = 5,
    }

    public const float DespawnDistance = 5000f;
    public static float MaxScale => CAWorld.AnomalyUltramundane ? 4.5f : 3f;
    public static float MinScale => 0.5f;
    public static float SpawnSlimeDistance => TOWorld.LegendaryMode && Main.zenithWorld ? 0.01f : CAWorld.AnomalyUltramundane ? 0.03f : 0.04f;
    public static float SpawnSlimePow => Main.zenithWorld ? 0.75f : CAWorld.AnomalyUltramundane ? 0.5f : 0.3f;
    public static float JewelEmeraldLifeRatio => CAWorld.AnomalyUltramundane ? 0.75f : 0.7f;
    public static float JewelRubyLifeRatio => CAWorld.AnomalyUltramundane ? 0.75f : 0.5f;
    public static float JewelSapphireLifeRatio => CAWorld.AnomalyUltramundane ? 0.5f : 0.3f;
    public static float Phase2LifeRatio => 0.25f;

    public string LocalizationPrefix => CAMain.AnomalyLocalizationPrefix + "KingSlime";

    /// <summary>
    /// 当前阶段。0，1，2。
    /// </summary>
    public int CurrentPhase
    {
        get => (int)NPC.ai[0];
        set => NPC.ai[0] = value;
    }

    public bool ShouldEnterPhase2 => CAWorld.AnomalyUltramundane && NPC.LifeRatio < Phase2LifeRatio;
    public bool InvalidPhase1 => ShouldEnterPhase2 && CurrentPhase == 1;

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
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
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
    public bool JewelEmeraldAlive => JewelEmerald.active && JewelEmerald.ModNPC is KingSlimeJewelEmerald && JewelEmerald.Ocean().Master == NPC;
    public bool JewelEmeraldDead => JewelEmeraldSpawned && !JewelEmeraldAlive;

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
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
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
    public bool JewelRubyAlive => JewelRuby.active && JewelRuby.ModNPC is KingSlimeJewelRuby && JewelRuby.Ocean().Master == NPC;
    public bool JewelRubyDead => JewelRubySpawned && !JewelRubyAlive;

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
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
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
    public bool JewelSapphireAlive => JewelSapphire.active && JewelSapphire.ModNPC is KingSlimeJewelSapphire && JewelSapphire.Ocean().Master == NPC;
    public bool JewelSapphireDead => JewelSapphireSpawned && !JewelSapphireAlive;
    public bool HasSapphireBuff => JewelSapphireAlive && !JewelHandler.CheckIfPhase2(JewelSapphire);

    public bool JewelRainbowSpawned
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[3];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[3] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[3] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 王冠蓝宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public unsafe NPC JewelRainbow
    {
        get => Main.npc[AnomalyNPC.AnomalyAI32[1].bytes[3]];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[1].bytes[3] != temp)
            {
                AnomalyNPC.AnomalyAI32[1].bytes[3] = temp;
                AnomalyNPC.AIChanged32[1] = true;
            }
        }
    }
    public bool JewelRainbowAlive => JewelRainbow.active && JewelRainbow.ModNPC is KingSlimeJewelRainbow && JewelRainbow.Ocean().Master == NPC;
    public bool JewelRainbowDead => JewelRainbowSpawned && !JewelRainbowAlive;
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

    public float SapphireBuffRatio
    {
        get => CalamityNPC.newAI[1];
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (CalamityNPC.newAI[1] != temp)
            {
                CalamityNPC.newAI[1] = temp;
                NPC.SyncExtraAI();
            }
        }
    }

    public float RainbowRatio
    {
        get => CalamityNPC.newAI[2];
        set
        {
            float temp = Math.Clamp(value, 0f, 1f);
            if (CalamityNPC.newAI[2] != temp)
            {
                CalamityNPC.newAI[2] = temp;
                NPC.SyncExtraAI();
            }
        }
    }
    #endregion 数据

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
            NPC.lifeMax = (int)(NPC.lifeMax * 1.25f);

        TeleportScaleMultiplier = 1f;
        DespawnScaleMultiplier = 1f;

        JewelEmerald = NPC.DummyNPC;
        JewelRuby = NPC.DummyNPC;
        JewelSapphire = NPC.DummyNPC;
    }

    public override bool CheckActive() => false;

    public override bool PreAI()
    {
        if (CurrentBehavior == Behavior.Despawn || !NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            Despawn();
            ChangeScale();
            return false;
        }
        else
            NPC.FaceNPCTarget(Target);

        NPC.noTileCollide = false;
        bool hasSapphireBuff = HasSapphireBuff;
        if (hasSapphireBuff)
            SapphireBuffRatio += 0.02f;
        else
            SapphireBuffRatio -= 0.025f;
        CalamityNPC.CurrentlyIncreasingDefenseOrDR = hasSapphireBuff;
        NPC.defense = JewelSapphireDead ? (int)(NPC.defDefense * 0.75f) : hasSapphireBuff ? (int)(NPC.defDefense * 1.25f) : NPC.defDefense;
        CalamityNPC.DR = CAWorld.AnomalyUltramundane && hasSapphireBuff ? 0.2f : 0f;
        bool invalidPhase1 = InvalidPhase1;
        AnomalyNPC.ExtraDR = invalidPhase1 ? Utils.Remap(NPC.LifeRatio, Phase2LifeRatio, Phase2LifeRatio - 0.05f, 0f, 1f) : 0f;

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
            case 1 or 2:
                switch (CurrentBehavior)
                {
                    case Behavior.Despawn:
                        Despawn();
                        ChangeScale();
                        return false;
                    case Behavior.NormalJump or Behavior.HighJump or Behavior.RapidJump:
                        Jump();
                        break;
                    case Behavior.Teleport:
                        Teleport();
                        break;
                    case Behavior.PhaseChange_P1To2:
                        PhaseChange();
                        break;
                    default:
                        SelectNextAttack();
                        break;
                }
                break;
        }

        TeleportTimer += Utils.Remap((Target.Center - NPC.Center).Y, 0f, 800f, 1.5f, 5f);

        ChangeScale();
        TrySpawnMinions();

        NPC.netUpdate = true;

        return false;


        #region 行为函数
        void SelectNextAttack()
        {
            if (InvalidPhase1)
                CurrentBehavior = Behavior.PhaseChange_P1To2;
            else if (TeleportTimer > MathHelper.Lerp(1250f, 1000f, NPC.MissingLifeRatio) || !NPC.WithinRange(Target.Center, 2400f))
            {
                CurrentBehavior = Behavior.Teleport;
                TeleportTimer = 0f;
            }
            else
            {
                switch (SmallJumpCounter)
                {
                    case 3:
                        CurrentBehavior = Behavior.HighJump;
                        SmallJumpCounter = 0;
                        break;
                    case 1 or 2 when NPC.LifeRatio < JewelSapphireLifeRatio:
                        CurrentBehavior = Behavior.RapidJump;
                        break;
                    default:
                        CurrentBehavior = Behavior.NormalJump;
                        break;
                }
            }
            CurrentAttackPhase = 0;
            ChangedVelocityDirectionDuringJump = 0;
            Timer1 = 0;
            Timer2 = 0;
        }

        bool StopHorizontalMovement()
        {
            NPC.velocity.X *= 0.85f;
            float velocityXLength = Math.Abs(NPC.velocity.X);
            if (velocityXLength < 0.1f)
            {
                NPC.velocity.X = 0f;
                return true;
            }
            else if (velocityXLength > 0.2f)
                NPC.velocity.X -= 0.15f * Math.Sign(NPC.velocity.X);
            return false;
        }

        void MakeSlimeDust(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                bool useRainbowDust = Main.rand.NextProbability(RainbowRatio);
                Dust.NewDustAction(NPC.Center, NPC.width + 25, NPC.height, useRainbowDust ? JewelHandler.GetRandomDustID() : JewelSapphireAlive ? DustID.GemSapphire : DustID.TintableDust, Vector2.Zero, d =>
                {
                    d.alpha = 150;
                    if (!useRainbowDust)
                        d.color = new(78, 136, 255, 80);
                    d.noGravity = true;
                    d.scale = Utils.Remap(NPC.scale, MinScale, MaxScale, 1.5f, 4.5f);
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
            MakeSlimeDust((int)Utils.Remap(NPC.scale, MinScale, MaxScale, 4f, 10f));

            if (NPC.scale < 0.2f) //体积足够小时执行脱战逻辑
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }

        void SpawnJewelParticle(NPC jewel, int amount)
        {
            for (int i = 0; i < amount; i++)
                JewelHandler.SpawnParticle(jewel, Main.rand.NextFloat(5f, 10f), Main.rand.Next(40, 60), Main.rand.NextFloat(0.4f, 0.7f));
        }

        void ChangeScale() => NPC.ChangeScaleFixBottom(98, 92, MathHelper.Lerp(MaxScale, MinScale, NPC.MissingLifeRatio) * TeleportScaleMultiplier * DespawnScaleMultiplier);

        void SpawnSlime(int type)
        {
            float spawnZoneWidth = NPC.width / 2f - 16f;
            float spawnZoneHeight = NPC.height - 32f;
            Vector2 spawnPosition = new(NPC.Center.X + Main.rand.NextFloat(-spawnZoneWidth, spawnZoneWidth), NPC.Bottom.Y - Main.rand.NextFloat(spawnZoneHeight));
            NPC.NewNPCAction(NPC.GetSource_FromAI(), spawnPosition, type, action: n =>
            {
                n.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, 3f));
                n.ai[0] = -1000 * Main.rand.Next(3);
                n.Ocean().Master = NPC;
            });
        }

        Vector2 GetJewelSpawnPosition() => NPC.Top - new Vector2(0, NPC.height);

        void SpawnJewelAction(NPC n)
        {
            n.Ocean().Master = NPC;
            n.netUpdate = true;
            SoundEngine.PlaySound(SoundID.Item38, GetJewelSpawnPosition());
            SpawnJewelParticle(n, 50);
        }

        void TrySpawnMinions()
        {
            if (!TOWorld.GeneralClient)
                return;

            if (NPC.LifeRatio < JewelEmeraldLifeRatio && !JewelEmeraldSpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelEmerald>(NPC.GetSource_FromAI(), GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                {
                    SpawnJewelAction(n);
                    JewelEmerald = n;
                    JewelEmeraldSpawned = true;
                });
            }
            if (NPC.LifeRatio < JewelRubyLifeRatio && !JewelRubySpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelRuby>(NPC.GetSource_FromAI(), GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                {
                    SpawnJewelAction(n);
                    JewelRuby = n;
                    JewelRubySpawned = true;
                });
            }
            if (NPC.LifeRatio < JewelSapphireLifeRatio && !JewelSapphireSpawned)
            {
                NPC.NewNPCAction<KingSlimeJewelSapphire>(NPC.GetSource_FromAI(), GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                {
                    SpawnJewelAction(n);
                    JewelSapphire = n;
                    JewelSapphireSpawned = true;
                });
            }

            float distance = (float)(LastSpawnSlimeLife - NPC.life) / NPC.lifeMax;
            float distanceNeeded = SpawnSlimeDistance;
            if (distance >= distanceNeeded)
            {
                LastSpawnSlimeLife = NPC.life;
                int spawnAmount = Main.rand.Next(1, Main.zenithWorld ? (int)MathHelper.Lerp(3f, 6f, NPC.MissingLifeRatio) : 3) + Math.Clamp((int)Math.Pow(distance / distanceNeeded, SpawnSlimePow), 0, 5);

                for (int i = 0; i < spawnAmount; i++)
                {
                    int minTypeChoice = (int)MathHelper.Lerp(i < 2 ? 0 : 4, 9f, NPC.MissingLifeRatio);
                    int maxTypeChoice = 16;

                    int type = -1;
                    do
                    {
                        switch (Main.rand.Next(minTypeChoice, maxTypeChoice + 1))
                        {
                            //基础史莱姆
                            case 0: //绿史莱姆
                                type = NPCID.GreenSlime;
                                break;
                            case 1: //蓝史莱姆
                                type = NPCID.BlueSlime;
                                break;
                            case 2: //红史莱姆
                                type = NPCID.RedSlime;
                                break;
                            case 3: //紫史莱姆
                                type = NPCID.PurpleSlime;
                                break;
                            case 4: //黄史莱姆
                                type = NPCID.YellowSlime;
                                break;
                            case 5: //冰雪史莱姆
                                type = NPCID.IceSlime;
                                break;
                            case 6: //丛林史莱姆
                                type = NPCID.JungleSlime;
                                break;
                            case 7: //史莱姆之母
                                type = NPCID.MotherSlime;
                                break;
                            //特殊史莱姆
                            case 8: //尖刺史莱姆
                                type = NPCID.SlimeSpiked;
                                break;
                            case 9: //尖刺冰雪史莱姆
                                type = NPCID.SpikedIceSlime;
                                break;
                            case 10: //尖刺丛林史莱姆
                                type = NPCID.SpikedJungleSlime;
                                break;
                            case 11 when Main.raining: //雨伞史莱姆
                                type = NPCID.UmbrellaSlime;
                                break;
                            case 12 when Main.zenithWorld || Main.bloodMoon: //腐化史莱姆、猩红史莱姆、黑檀枯萎史莱姆、血腥枯萎史莱姆
                                int corrupt = (Main.zenithWorld || NPC.downedBoss2) && Main.rand.NextBool(4) ? ModContent.NPCType<EbonianBlightSlime>() : NPCID.CorruptSlime;
                                int crimson = (Main.zenithWorld || NPC.downedBoss2) && Main.rand.NextBool(4) ? ModContent.NPCType<CrimulanBlightSlime>() : NPCID.Crimslime;
                                type = Main.drunkWorld ? (Main.rand.NextBool() ? crimson : corrupt) : WorldGen.crimson ? crimson : corrupt;
                                break;
                            case 13 when Main.zenithWorld || NPC.downedBoss3: //微光史莱姆
                                type = NPCID.ShimmerSlime;
                                break;
                            case 14 when Main.zenithWorld || Main.hardMode: //夜明史莱姆、彩虹史莱姆
                                type = (Main.raining || CAWorld.AnomalyUltramundane) && Main.rand.NextBool(5) ? NPCID.RainbowSlime : NPCID.IlluminantSlime;
                                break;
                            case 15 when Main.rand.NextBool(15): //粉史莱姆
                                type = NPCID.Pinky;
                                break;
                            case 16 when Main.zenithWorld && Main.rand.NextBool(200): //金史莱姆
                                type = NPCID.GoldenSlime;
                                break;
                        }
                    } while (type == -1); //随机选择史莱姆类型直到成功

                    SpawnSlime(type);
                }

                if (OceanNPC.Master is null && Main.zenithWorld && Main.rand.NextBool(500) && !NPC.ActiveNPCs.Any(n => n.type == NPCID.KingSlime && n.Ocean().Master == OceanNPC.Master)) //GFB世界中0.2%概率生成史莱姆王！
                {
                    NPC.NewNPCAction(NPC.GetBossSpawnSource(Target.whoAmI), NPC.Center, NPCID.KingSlime, action: n =>
                    {
                        n.Ocean().Master = NPC;
                        SoundEngine.PlaySound(SoundID.Roar, n.Center);
                        TOLocalizationUtils.ChatLocalizedText(this, "GFBSummon", Color.LightSeaGreen);
                    });
                }
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
                    StopHorizontalMovement();
                    if (NPC.velocity.Y == 0f && TeleportScaleMultiplier > 0.6f)
                        Timer1++;
                    int jumpDelay = (int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 20f : 27.5f, CAWorld.AnomalyUltramundane ? 15f : 20f, NPC.MissingLifeRatio);
                    if (Timer1 > jumpDelay)
                    {
                        if (CurrentBehavior != Behavior.HighJump)
                            SmallJumpCounter++;
                        NPC.damage = NPC.defDamage;
                        NPC.netUpdate = true;
                        NPC.velocity = GetInitialVelocity();
                        CurrentAttackPhase = 1;
                    }

                    break;
                case 1: //上升
                case 2: //下降
                    NPC.damage = NPC.defDamage;
                    bool rapidJump = CurrentBehavior == Behavior.RapidJump;
                    bool highJump = CurrentBehavior == Behavior.HighJump;
                    bool farAway = Math.Abs(NPC.Center.X - Target.Center.X) > (rapidJump ? 2000f : 1000f);
                    if (NPC.velocity.X * NPC.direction <= 0.1f && farAway) //跳跃过度时调整水平速度
                    {
                        NPC.velocity.X *= rapidJump ? 0.99f : highJump ? 0.945f : 0.965f;
                        switch (Math.Abs(NPC.velocity.X))
                        {
                            case < 0.1f:
                                ChangedVelocityDirectionDuringJump++;
                                NPC.velocity.X += GetDeltaVelocityX() * NPC.direction;
                                break;
                            case > 0.25f:
                                NPC.velocity.X -= (rapidJump ? 0.005f : highJump ? 0.0125f : 0.0075f) * Math.Sign(NPC.velocity.X);
                                break;
                        }
                    }
                    else
                        NPC.velocity.X = Math.Min(Math.Abs(NPC.velocity.X) + GetDeltaVelocityX(), GetMaxVelocityX()) * Math.Sign(NPC.velocity.X);
                    switch (CurrentAttackPhase)
                    {
                        case 1:
                            NPC.noTileCollide = true; //上升时无视物块
                            if (NPC.velocity.Y >= 0) //检测是否已过最高点
                                CurrentAttackPhase = 2;
                            break;
                        case 2:
                            if (NPC.velocity.Y == 0f)
                            {
                                TeleportTimer += CurrentBehavior switch
                                {
                                    Behavior.HighJump => 400f,
                                    Behavior.NormalJump => 100f,
                                    Behavior.RapidJump => 75f,
                                    _ => 0f
                                };
                                SelectNextAttack();
                                if (CurrentBehavior == Behavior.RapidJump)
                                    Timer1 += 10;
                            }
                            else
                            {
                                NPC.GravityMultiplier *= CurrentBehavior switch
                                {
                                    Behavior.HighJump => CAWorld.AnomalyUltramundane ? 1.5f : 1.25f,
                                    _ => 1f
                                };
                                NPC.MaxFallSpeedMultiplier *= CurrentBehavior switch
                                {
                                    Behavior.HighJump => CAWorld.AnomalyUltramundane ? 2.25f : 1.75f,
                                    _ => 1f
                                };
                            }
                            break;
                    }
                    break;
            }

            Vector2 GetInitialVelocity() => new(
                CurrentBehavior switch
                {
                    Behavior.NormalJump => MathHelper.Lerp(4f, 7f, NPC.MissingLifeRatio),
                    Behavior.HighJump => MathHelper.Lerp(6f, 9f, NPC.MissingLifeRatio),
                    Behavior.RapidJump => MathHelper.Lerp(10f, 12.5f, NPC.MissingLifeRatio),
                    _ => 0f
                } * NPC.direction,
                CurrentBehavior switch
                {
                    Behavior.NormalJump => 7f * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 1000f, 0f, 0.5f)),
                    Behavior.HighJump => MathHelper.Lerp(10f, 12.5f, NPC.MissingLifeRatio) * (1f + Math.Clamp(Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 1000f, 0f, 1f)),
                    Behavior.RapidJump => 4f,
                    _ => 0f
                } * -1f);

            float GetMaxVelocityX() => CurrentBehavior switch
            {
                Behavior.RapidJump => MathHelper.Lerp(13.5f, 15f, NPC.MissingLifeRatio) + TOMathHelper.ParabolicInterpolation(Math.Max(Math.Abs(NPC.Center.X - Target.Center.X) - 300f, 0f) / 1000f) * 5f,
                Behavior.HighJump => ChangedVelocityDirectionDuringJump switch
                {
                    0 => MathHelper.Lerp(9f, 11.5f, NPC.MissingLifeRatio),
                    1 => 2.5f,
                    _ => 1.5f
                },
                _ => ChangedVelocityDirectionDuringJump switch
                {
                    0 => MathHelper.Lerp(7f, 9f, NPC.MissingLifeRatio),
                    1 => 2.5f,
                    _ => 1.5f
                }
            };

            float GetDeltaVelocityX() => CurrentBehavior switch
            {
                Behavior.RapidJump => ChangedVelocityDirectionDuringJump switch
                {
                    0 => 0.4f,
                    1 => 0.2f,
                    _ => 0.1f
                },
                _ => ChangedVelocityDirectionDuringJump switch
                {
                    0 => 0.25f,
                    1 => 0.15f,
                    _ => 0.075f
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
                    Vector2 vectorAimedAheadOfTarget = Target.Center + new Vector2(MathF.Round(Target.velocity.X / 2f), 0f).ToCustomLength(800f);
                    //目标点
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
                            if (potentialTile.LiquidType != LiquidID.Lava && Collision.CanHitLine(NPC.Center, 0, 0, predictiveTeleportPoint.ToVector2() * 16, 0, 0))
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
                    MakeSlimeDust((int)Utils.Remap(NPC.scale, MinScale, MaxScale, 5f, 12.5f));
                    TeleportScaleMultiplier -= MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 0.016f : 0.013f, CAWorld.AnomalyUltramundane ? 0.02f : 0.015f, NPC.MissingLifeRatio);
                    if (StopHorizontalMovement() && TeleportScaleMultiplier <= 0.2f)
                    {
                        Gore.NewGoreAction(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-40f, -NPC.height / 2f), NPC.velocity, GoreID.KingSlimeCrown, g => g.timeLeft += 180);
                        NPC.Bottom = TeleportDestination;
                        //if (JewelSapphireAlive) //移动蓝宝石
                        //{
                        //    JewelSapphire.Center = NPC.Center - new Vector2(0f, 200f);
                        //    SpawnJewelParticle(JewelSapphire, 20);
                        //}
                        CurrentAttackPhase = 2;
                    }
                    break;
                case 2: //恢复体型，恢复完成后开始下一次攻击
                    TeleportScaleMultiplier += MathHelper.Lerp(0.03f, 0.05f, NPC.MissingLifeRatio);
                    if (TeleportScaleMultiplier >= 1f)
                    {
                        TeleportScaleMultiplier = 1f;
                        if (Timer2++ >= 1f)
                            SelectNextAttack();
                    }
                    break;
            }
        }

        void PhaseChange()
        {
            RainbowRatio += 0.01f;
            StopHorizontalMovement();
            switch (Timer1++)
            {
                case 0:
                    if (JewelEmeraldAlive)
                        JewelHandler.DisableAttack(JewelEmerald);
                    if (JewelRubyAlive)
                        JewelHandler.DisableAttack(JewelRuby);
                    if (JewelSapphireAlive)
                        JewelHandler.DisableAttack(JewelSapphire);
                    break;
                case 90:
                    SoundEngine.PlaySound(SoundID.Item38, NPC.Center);
                    for (int i = 0; i < 12; i++)
                        SpawnSlime(NPCID.RainbowSlime);
                    Vector2 position = GetJewelSpawnPosition();
                    if (JewelEmeraldAlive)
                    {
                        JewelHandler.CreateDustFromJewelTo(JewelEmerald, position, Main.zenithWorld ? DustID.GemAmethyst : DustID.GemEmerald);
                        JewelHandler.Despawn(JewelEmerald);
                    }
                    if (JewelRubyAlive)
                    {
                        JewelHandler.CreateDustFromJewelTo(JewelRuby, position, Main.zenithWorld ? DustID.IceTorch : DustID.GemRuby);
                        JewelHandler.Despawn(JewelRuby);
                    }
                    if (JewelSapphireAlive)
                    {
                        JewelHandler.CreateDustFromJewelTo(JewelSapphire, position, Main.zenithWorld ? DustID.GemTopaz : DustID.GemSapphire);
                        JewelHandler.Despawn(JewelSapphire);
                    }
                    NPC.NewNPCAction<KingSlimeJewelRainbow>(NPC.GetSource_FromAI(), GetJewelSpawnPosition(), NPC.whoAmI, action: n =>
                    {
                        SpawnJewelAction(n);
                        JewelRainbow = n;
                        JewelRainbowSpawned = true;
                    });
                    break;
                case 120:
                    CurrentPhase = 2;
                    TeleportTimer += 2000f;
                    SelectNextAttack();
                    break;
            }
        }
        #endregion 行为函数
    }

    public override Color? GetAlpha(Color drawColor)
    {
        Color newColor = Color.Lerp(new Color(0, 0, 150, NPC.alpha), new Color(125, 125, 255, NPC.alpha), TOMathHelper.GetTimeSin(0.5f, unsigned: true));
        Color preRainbow = Color.Lerp(drawColor, newColor, SapphireBuffRatio);

        if (Main.remixWorld)
        {
            byte r = preRainbow.R;
            byte g = preRainbow.G;
            byte b = preRainbow.B;
            byte a = preRainbow.A;
            preRainbow = new Color(b, b, (r + g) / 2, a);
        }

        return Color.Lerp(preRainbow, Main.DiscoColor, RainbowRatio);
    }
}
