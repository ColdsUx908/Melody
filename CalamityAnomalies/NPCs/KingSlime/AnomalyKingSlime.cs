using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.NPCs.KingSlime;

public class AnomalyKingSlime : AnomalyNPCOverride
{
    #region 枚举、数值、属性、AI状态
    public enum AttackType
    {
        Despawn = -1,

        None = 0,

        NormalJump_Phase1 = 1,
        HighJump_Phase1 = 2,
        RapidJump_Phase1 = 3,
        Teleport_Phase1 = 4,

        PhaseChange_1And2 = 5,
    }

    private static class Data
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
        get => (int)AnomalyNPC.AnomalyAI[0];
        set => AnomalyNPC.SetAnomalyAI(value, 0);
    }

    public AttackType CurrentAttack
    {
        get => (AttackType)(int)AnomalyNPC.AnomalyAI[1];
        set => AnomalyNPC.SetAnomalyAI((int)value, 1);
    }

    public int CurrentAttackPhase
    {
        get => (int)AnomalyNPC.AnomalyAI[2];
        set => AnomalyNPC.SetAnomalyAI(value, 2);
    }

    public bool JewelEmeraldSpawned
    {
        get => AnomalyNPC.GetAnomalyAIBit(3, 0);
        set => AnomalyNPC.SetAnomalyAIBit(value, 3, 0);
    }

    public bool JewelRubySpawned
    {
        get => AnomalyNPC.GetAnomalyAIBit(3, 1);
        set => AnomalyNPC.SetAnomalyAIBit(value, 3, 1);
    }

    public bool JewelSapphireSpawned
    {
        get => AnomalyNPC.GetAnomalyAIBit(3, 2);
        set => AnomalyNPC.SetAnomalyAIBit(value, 3, 2);
    }

    public int LastSpawnSlimeLife
    {
        get => (int)AnomalyNPC.AnomalyAI[4];
        set => AnomalyNPC.SetAnomalyAI(value, 4);
    }

    public float TeleportTimer
    {
        get => AnomalyNPC.AnomalyAI[5];
        set => AnomalyNPC.SetAnomalyAI(value, 5);
    }

    /// <summary>
    /// 王冠绿宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC JewelEmerald
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[6]];
        set => AnomalyNPC.SetAnomalyAI(value.whoAmI, 6);
    }

    public bool JewelEmeraldAlive => JewelEmerald.active && JewelEmerald.ModNPC is KingSlimeJewelEmerald && JewelEmerald.Ocean().Master == NPC.whoAmI;

    /// <summary>
    /// 王冠红宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC JewelRuby
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[7]];
        set => AnomalyNPC.SetAnomalyAI(value.whoAmI, 7);
    }

    public bool JewelRubyAlive => JewelRuby.active && JewelRuby.ModNPC is KingSlimeJewelRuby && JewelRuby.Ocean().Master == NPC.whoAmI;

    /// <summary>
    /// 王冠蓝宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC JewelSapphire
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[8]];
        set => AnomalyNPC.SetAnomalyAI(value.whoAmI, 8);
    }

    public bool JewelSapphireAlive => JewelSapphire.active && JewelSapphire.ModNPC is KingSlimeJewelSapphire && JewelSapphire.Ocean().Master == NPC.whoAmI;

    public int SmallJumpCounter
    {
        get => (int)AnomalyNPC.AnomalyAI[9];
        set => AnomalyNPC.SetAnomalyAI(value, 9);
    }

    public int ChangedVelocityDirectionDuringJump
    {
        get => (int)AnomalyNPC.AnomalyAI[10];
        set => AnomalyNPC.SetAnomalyAI(value, 10);
    }

    public float TeleportScaleMultiplier
    {
        get => AnomalyNPC.AnomalyAI[11];
        set => AnomalyNPC.SetAnomalyAI(Math.Clamp(value, 0f, 1f), 11);
    }

    public Vector2 TeleportDestination
    {
        get => new(AnomalyNPC.AnomalyAI[12], AnomalyNPC.AnomalyAI[13]);
        set
        {
            AnomalyNPC.SetAnomalyAI(value.X, 12);
            AnomalyNPC.SetAnomalyAI(value.Y, 13);
        }
    }

    public float DespawnScaleMultiplier
    {
        get => AnomalyNPC.AnomalyAI[14];
        set => AnomalyNPC.SetAnomalyAI(Math.Clamp(value, 0f, 1f), 14);
    }


    #endregion 枚举、数值、属性、AI状态

    public override int OverrideType => NPCID.KingSlime;

    public override bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC type) => type switch
    {
        OrigMethodType_CalamityGlobalNPC.PreAI => false,
        OrigMethodType_CalamityGlobalNPC.GetAlpha => false,
        _ => true,
    };

    #region Defaults
    public override void SetDefaults()
    {
        if (CAWorld.AnomalyUltramundane)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 1.25f);
            NPC.Calamity().DR += 0.1f;
        }

        TeleportScaleMultiplier = 1f;
        DespawnScaleMultiplier = 1f;

        JewelEmerald = TOMain.DummyNPC;
        JewelRuby = TOMain.DummyNPC;
        JewelSapphire = TOMain.DummyNPC;
    }
    #endregion Defaults

    #region Active
    public override bool CheckActive() => false;
    #endregion Active

    #region AI
    public override bool PreAI()
    {
        if (CurrentAttack == AttackType.Despawn || !NPC.TargetClosestIfInvalid(true, Data.DespawnDistance))
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
                    CurrentAttack = AttackType.Despawn;
                    Despawn();
                }
                break;
            case 1:
                switch (CurrentAttack)
                {
                    case AttackType.Despawn:
                        Despawn();
                        ChangeScale();
                        return false;
                    case AttackType.None:
                        SelectNextAttack();
                        break;
                    case AttackType.NormalJump_Phase1:
                    case AttackType.HighJump_Phase1:
                    case AttackType.RapidJump_Phase1:
                        Jump();
                        break;
                    case AttackType.Teleport_Phase1:
                        Teleport();
                        break;
                }
                break;
        }

        TeleportTimer += (float)Math.Max(NPC.Center.Y - Target.Center.Y, 0f) > 0f ? 3f : MathHelper.Lerp(1f, 1.5f, OceanNPC.LifeRatioReverse);

        ChangeScale();
        TrySpawnMinions();

        NPC.netUpdate = true;

        return false;
    }

    private void SelectNextAttack(int initialAITimer1 = 0)
    {
        ChangedVelocityDirectionDuringJump = 0;
        if (TeleportTimer > 1000f || !NPC.WithinRange(Target.Center, 3000f))
        {
            CurrentAttack = AttackType.Teleport_Phase1;
            TeleportTimer = 0f;
        }
        else
        {
            CurrentAttack = SmallJumpCounter switch
            {
                3 => AttackType.HighJump_Phase1,
                1 or 2 when OceanNPC.LifeRatio < 0.3f => AttackType.RapidJump_Phase1,
                _ => AttackType.NormalJump_Phase1
            };
        }
        CurrentAttackPhase = 0;
        Timer1 = initialAITimer1;
    }

    private bool StopHorizontalMovement()
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

    private void MakeSlimeDust(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Dust.NewDustAction(NPC.Center, NPC.width + 25, NPC.height,
                JewelSapphireAlive ? DustID.GemSapphire : DustID.TintableDust, d =>
                {
                    d.alpha = 150;
                    d.color = new(78, 136, 255, 80);
                    d.noGravity = true;
                    d.scale = TOMathHelper.ClampMap(Data.MinScale, Data.MaxScale, 2f, 5f, NPC.scale);
                    d.velocity *= 0.5f;
                });
        }
    }

    private void Despawn()
    {
        StopHorizontalMovement(); //停止水平移动，避免奇怪的滑行现象
        NPC.dontTakeDamage = true;
        NPC.damage = 0;
        DespawnScaleMultiplier *= 0.97f;
        MakeSlimeDust((int)TOMathHelper.ClampMap(Data.MinScale, Data.MaxScale, 5f, 12.5f, NPC.scale));

        if (NPC.scale < 0.2f) //体积足够小时执行脱战逻辑
        {
            NPC.active = false;
            NPC.netUpdate = true;
        }
    }

    private static void MakeJewelDust(NPC jewel, int amount)
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
            Dust.NewDustAction(jewel.Center, jewel.width * 3, jewel.height * 3, type, d =>
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

    private void ChangeScale() => NPC.BetterChangeScale(98, 92, MathHelper.Lerp(
            Data.MaxScale,
            Data.MinScale,
            OceanNPC.LifeRatioReverse) * TeleportScaleMultiplier * DespawnScaleMultiplier);

    private void TrySpawnMinions()
    {
        if (!TOMain.GeneralClient)
            return;

        Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);

        if (OceanNPC.LifeRatio < 0.8f && !JewelEmeraldSpawned)
        {
            SoundEngine.PlaySound(SoundID.Item38, spawnPosition);
            NPC.NewNPCAction<KingSlimeJewelEmerald>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
            {
                n.Ocean().Master = NPC.whoAmI;
                n.netUpdate = true;
                MakeJewelDust(n, 50);
                JewelEmerald = n;
                JewelEmeraldSpawned = true;
            });
        }

        if (OceanNPC.LifeRatio < 0.6f && !JewelRubySpawned)
        {
            SoundEngine.PlaySound(SoundID.Item38, spawnPosition);
            NPC.NewNPCAction<KingSlimeJewelRuby>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
            {
                n.Ocean().Master = NPC.whoAmI;
                n.netUpdate = true;
                MakeJewelDust(n, 50);
                JewelRuby = n;
                JewelRubySpawned = true;
            });
        }

        if (OceanNPC.LifeRatio < 1f / 3f && !JewelSapphireSpawned)
        {
            SoundEngine.PlaySound(SoundID.Item38, spawnPosition);
            NPC.NewNPCAction<KingSlimeJewelSapphire>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
            {
                n.Ocean().Master = NPC.whoAmI;
                n.netUpdate = true;
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
                float minLowerLimit = i < 2 ? 0 : 5;
                float maxLowerLimit = 7f;
                int minTypeChoice = (int)MathHelper.Lerp(minLowerLimit, 7f, 1f - OceanNPC.LifeRatio);
                int maxTypeChoice = (int)MathHelper.Lerp(maxLowerLimit, 9f, 1f - OceanNPC.LifeRatio);
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
    }

    private void SpawnSlimeCore(int type)
    {
        int spawnZoneWidth = NPC.width - 32;
        int spawnZoneHeight = NPC.height - 32;
        Vector2 spawnPosition = new(NPC.position.X + Main.rand.Next(spawnZoneWidth), NPC.position.Y + Main.rand.Next(spawnZoneHeight));
        Vector2 spawnVelocity = new(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, 3f));
        NPC.NewNPCAction(NPC.GetSource_FromAI(), spawnPosition, type, action: n =>
        {
            n.velocity = spawnVelocity;
            n.ai[0] = -1000 * Main.rand.Next(3);
            n.Ocean().Master = NPC.whoAmI;
        });
    }

    private void Jump()
    {
        switch (CurrentAttackPhase)
        {
            case 0: //延迟
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
                        CurrentAttackPhase = 1;
                        Timer1 = 0;
                    }
                }
                break;
            case 1: //起跳
                if (CurrentAttack != AttackType.HighJump_Phase1)
                    SmallJumpCounter++;
                NPC.damage = NPC.defDamage;
                NPC.netUpdate = true;
                NPC.velocity = Jump_VelocityInitial;
                CurrentAttackPhase = 2;
                break;
            case 2: //上升
            case 3: //下降
                NPC.damage = NPC.defDamage;
                if (CurrentAttack == AttackType.RapidJump_Phase1 || NPC.velocity.X * NPC.direction > 0.1f)
                    NPC.velocity.X = Math.Min(Math.Abs(NPC.velocity.X) + Jump_VelocityXDelta, Jump_VelocityXLimit) * Math.Sign(NPC.velocity.X);
                else
                {
                    NPC.velocity.X *= 0.93f;
                    switch (Math.Abs(NPC.velocity.X))
                    {
                        case < 0.1f:
                            ChangedVelocityDirectionDuringJump++;
                            NPC.velocity.X += Jump_VelocityXDelta * NPC.direction;
                            break;
                        case > 0.25f:
                            NPC.velocity.X -= 0.2f * Math.Sign(NPC.velocity.X);
                            break;
                    }
                }
                switch (CurrentAttackPhase)
                {
                    case 2:
                        if (NPC.velocity.Y >= 0) //检测是否已过最高点
                            CurrentAttackPhase = 3;
                        break;
                    case 3:
                        if (NPC.velocity.Y == 0f)
                        {
                            if (CurrentAttack == AttackType.HighJump_Phase1)
                                TeleportTimer += 100f;
                            SelectNextAttack(CurrentAttack == AttackType.RapidJump_Phase1 ? (int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 10 : 20, CAWorld.AnomalyUltramundane ? 7.5f : 12.5f, OceanNPC.LifeRatioReverse) : 0);
                        }
                        NPC.GravityMultiplier *= CurrentAttack switch
                        {
                            AttackType.HighJump_Phase1 => CAWorld.AnomalyUltramundane ? 1.5f : 1.25f,
                            _ => 1f
                        };
                        NPC.MaxFallSpeedMultiplier *= CurrentAttack switch
                        {
                            AttackType.HighJump_Phase1 => CAWorld.AnomalyUltramundane ? 2.25f : 1.75f,
                            _ => 1f
                        };
                        break;
                }
                break;
        }
    }

    private Vector2 Jump_VelocityInitial => CurrentAttack switch
    {
        AttackType.NormalJump_Phase1 => new(
            MathHelper.Lerp(5f, 7.5f, OceanNPC.LifeRatioReverse) * NPC.direction, -7.5f * (1f + Math.Min((float)Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 800f, 0.75f))),
        AttackType.HighJump_Phase1 => new(
            MathHelper.Lerp(7.5f, 10f, OceanNPC.LifeRatioReverse) * NPC.direction, -10f * (1f + Math.Min((float)Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 800f, 1.25f))),
        AttackType.RapidJump_Phase1 => new(
            MathHelper.Lerp(10f, 12.5f, OceanNPC.LifeRatioReverse) * NPC.direction, -5f),
        _ => Vector2.Zero
    };

    private float Jump_VelocityXLimit => CurrentAttack switch
    {
        AttackType.RapidJump_Phase1 => 18f,
        _ => ChangedVelocityDirectionDuringJump switch
        {
            0 => 12.5f,
            1 => 8f,
            _ => 6.5f
        }
    };

    private float Jump_VelocityXDelta => CurrentAttack switch
    {
        AttackType.RapidJump_Phase1 => ChangedVelocityDirectionDuringJump switch
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

    private void Teleport()
    {
        NPC.damage = 0;
        switch (CurrentAttackPhase)
        {
            case 0: //寻的
                Vector2? destination = null;
                //Vector2 randomDefault = Main.rand.NextBool() ? Vector2.UnitX : -Vector2.UnitX;
                Vector2 vectorAimedAheadOfTarget = Target.Center + new Vector2((float)Math.Round(Target.velocity.X / 2f), 0f).ToCustomLength(800f);
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
                            destination = new((teleportTileX + 0.5f) * 16f, (teleportTileY + 1f) * 16f);
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
                MakeSlimeDust((int)TOMathHelper.ClampMap(Data.MinScale, Data.MaxScale, 5f, 12.5f, NPC.scale));
                TeleportScaleMultiplier -= MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 0.016f : 0.013f, CAWorld.AnomalyUltramundane ? 0.02f : 0.015f, OceanNPC.LifeRatioReverse);
                if (StopHorizontalMovement() && TeleportScaleMultiplier < 0.2f)
                {
                    TeleportScaleMultiplier = 0.2f;
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
                if (TeleportScaleMultiplier == 1f)
                    SelectNextAttack((int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 10 : 20, CAWorld.AnomalyUltramundane ? 5 : 12.5f, OceanNPC.LifeRatioReverse));
                break;
        }
    }
    #endregion AI

    #region Draw
    public override Color? GetAlpha(Color drawColor)
    {
        if (JewelSapphireAlive)
            return Color.Lerp(new Color(0, 0, 150, NPC.alpha), new Color(125, 125, 255, NPC.alpha), MathF.Sin(Main.GlobalTimeWrappedHourly) / 2f + 0.5f);

        return null;
    }
    #endregion Draw
}
