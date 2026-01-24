using CalamityMod.World;

namespace CalamityAnomalies.Anomaly.EmpressofLight;

public sealed partial class AnomalyEmpressofLight : AnomalyNPCBehavior
{
    #region 数据
    public enum Phase
    {
        Initialize,
        Phase1, Phase2, Phase3,
    }

    public enum Behavior
    {
        Despawn = -1,

        None = 0,

        /// <summary>生成动画。</summary>
        SpawnAnimation,
        /// <summary>第二阶段动画。</summary>
        Phase2Animation,

        /// <summary>七彩矢1（常规）。</summary>
        PB1,
        /// <summary>七彩矢2（弥散）。</summary>
        PB2,
        /// <summary>太阳舞。</summary>
        SD,
        /// <summary>永恒彩虹。</summary>
        ER,
        /// <summary>空灵长枪1（常规）。</summary>
        EL1,
        /// <summary>空灵长枪2A（线状）。</summary>
        EL2A,
        /// <summary>空灵长枪2B（集中）。</summary>
        EL2B,
        /// <summary>空灵长枪3（直线）。</summary>
        EL3,
        /// <summary>冲刺（向左）。</summary>
        DLeft,
        /// <summary>冲刺（向右）。</summary>
        DRight,

        //额外攻击模式
    }

    public const float DespawnDistance = 6400f;
    public static float Phase2LifeRatio => CAWorld.AnomalyUltramundane ? 0.75f : 0.65f;
    public static float Phase3LifeRatio => CAWorld.AnomalyUltramundane ? 0.4f : 0.3f;

    public Vector2 RainbowStreakDistance => new Vector2(-250f, -350f) * (Enrage ? 1.1f : 1f);
    public Vector2 EverlastingRainbowDistance => new Vector2(0f, -450f) * (Enrage ? 1.1f : 1f);
    public Vector2 EtherealLanceDistance => new Vector2(0f, -450f) * (Enrage ? 1.1f : 1f);
    public Vector2 SunDanceDistance => new Vector2(-80f, -500f) * (Enrage ? 1.1f : 1f);
    public float Acceleration => 0.66f * (Enrage ? 1.2f : 1f);
    public float Velocity => 16.5f * (Enrage ? 1.2f : 1f);
    public float MovementDistanceGateValue => 40f;
    public int ProjectileDamageMultiplier => Enrage ? 2 : 1;
    public float LessTimeSpentPerPhaseMultiplier => (Phase2 ? 0.375f : 0.75f) * (Main.getGoodWorld ? 0.2f : 1f);

    public Phase CurrentPhase
    {
        get => NPC.ai[3] switch
        {
            0f or 2f => AnomalyNPC.AnomalyAI32[1].i switch
            {
                0 => Phase.Initialize,
                _ => Phase.Phase1
            },
            1f or 3f => AnomalyNPC.AnomalyAI32[1].i switch
            {
                0 => Phase.Phase2,
                _ => Phase.Phase3
            },
            _ => Phase.Initialize
        };
        set
        {
            switch (value)
            {
                case Phase.Initialize:
                    NPC.ai[3] = Enrage ? 2f : 0f;
                    if (AnomalyNPC.AnomalyAI32[1].i != 0)
                    {
                        AnomalyNPC.AnomalyAI32[1].i = 0;
                        AnomalyNPC.AIChanged32[1] = true;
                    }
                    break;
                case Phase.Phase1:
                    NPC.ai[3] = Enrage ? 2f : 0f;
                    if (AnomalyNPC.AnomalyAI32[1].i != 1)
                    {
                        AnomalyNPC.AnomalyAI32[1].i = 1;
                        AnomalyNPC.AIChanged32[1] = true;
                    }
                    break;
                case Phase.Phase2:
                    NPC.ai[3] = Enrage ? 3f : 1f;
                    if (AnomalyNPC.AnomalyAI32[1].i != 0)
                    {
                        AnomalyNPC.AnomalyAI32[1].i = 0;
                        AnomalyNPC.AIChanged32[1] = true;
                    }
                    break;
                case Phase.Phase3:
                    NPC.ai[3] = Enrage ? 3f : 1f;
                    if (AnomalyNPC.AnomalyAI32[1].i != 1)
                    {
                        AnomalyNPC.AnomalyAI32[1].i = 1;
                        AnomalyNPC.AIChanged32[1] = true;
                    }
                    break;
            }
        }
    }

    public Behavior CurrentBehavior
    {
        get => (int)NPC.ai[0] switch
        {
            0 => Behavior.SpawnAnimation,
            2 => Behavior.PB1,
            4 => Behavior.EL1,
            5 => Behavior.ER,
            6 => Behavior.SD,
            7 => CalamityNPC.newAI[2] switch
            {
                1f => Behavior.EL2A,
                0f => Behavior.EL2B,
                _ => Behavior.None
            },
            8 => Behavior.DLeft,
            9 => Behavior.DRight,
            10 => Behavior.Phase2Animation,
            11 => Behavior.EL3,
            12 => Behavior.PB2,
            13 => Behavior.Despawn,
            _ => Behavior.None
        };
        set
        {
            switch (value)
            {
                case Behavior.Despawn:
                    NPC.ai[0] = 13f;
                    break;
                case Behavior.None:
                    break;
                case Behavior.SpawnAnimation:
                    NPC.ai[0] = 0f;
                    break;
                case Behavior.Phase2Animation:
                    NPC.ai[0] = 10f;
                    break;
                case Behavior.PB1:
                    NPC.ai[0] = 2f;
                    break;
                case Behavior.PB2:
                    NPC.ai[0] = 12f;
                    break;
                case Behavior.SD:
                    NPC.ai[0] = 6f;
                    break;
                case Behavior.ER:
                    NPC.ai[0] = 5f;
                    break;
                case Behavior.EL1:
                    NPC.ai[0] = 4f;
                    EL1_AnotherType = Main.rand.NextBool(2);
                    break;
                case Behavior.EL2A:
                    NPC.ai[0] = 7f;
                    CalamityNPC.newAI[2] = 1f;
                    NPC.SyncExtraAI();
                    break;
                case Behavior.EL2B:
                    NPC.ai[0] = 7f;
                    CalamityNPC.newAI[2] = 0f;
                    NPC.SyncExtraAI();
                    break;
                case Behavior.EL3:
                    NPC.ai[0] = 11f;
                    break;
                case Behavior.DLeft:
                    NPC.ai[0] = 8f;
                    break;
                case Behavior.DRight:
                    NPC.ai[0] = 9f;
                    break;
            }
        }
    }

    public bool Enrage
    {
        get => NPC.AI_120_HallowBoss_IsGenuinelyEnraged(); //Vanilla: NPC.ai[3] is 2f or 3f
        set
        {
            if (value ^ Enrage)
                NPC.ai[3] += value ? 2f : -2f;
        }
    }

    public int AttackCounter
    {
        get => (int)NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    public bool TakeDamage
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

    public bool Visible
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

    public bool EL1_AnotherType
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

    public bool Phase2 => CurrentPhase is Phase.Phase2 or Phase.Phase3; //Vanilla: NPC.ai[3] is 1f or 3f
    public bool Phase3 => CurrentPhase == Phase.Phase3;

    public bool InvalidPhase1 => NPC.LifeRatio <= Phase2LifeRatio && !Phase2;
    public bool InvalidPhase2 => NPC.LifeRatio <= Phase3LifeRatio && Phase2 && !Phase3;
    #endregion 数据

    public override int ApplyingType => NPCID.HallowBoss;

    public override bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC method) => method switch
    {
        OrigMethodType_CalamityGlobalNPC.PreAI => false,
        _ => true,
    };


    public override bool PreAI()
    {
        Visible = true;
        TakeDamage = true;

        PreMainAI();

        if (CurrentBehavior == Behavior.Despawn)
        {
            State_Despawn();
        }
        else
        {
            switch (CurrentPhase)
            {
                case Phase.Initialize:
                    CurrentPhase = Phase.Phase1;
                    break;
                default:
                    switch (CurrentBehavior)
                    {
                        case Behavior.SpawnAnimation:
                            State_SpawnAnimation();
                            break;
                        case Behavior.PB1:
                            State_RainbowStreaks();
                            break;
                        case Behavior.PB2:
                            State_StationaryRainbowStreaks();
                            break;
                        case Behavior.SD:
                            State_SunDance();
                            break;
                        case Behavior.ER:
                            State_EverlastingRainbowSpiral();
                            break;
                        case Behavior.EL1:
                            State_EtherealLances();
                            break;
                        case Behavior.EL2A:
                        case Behavior.EL2B:
                            State_EtherealLanceWalls();
                            break;
                        case Behavior.EL3:
                            State_StraightEtherealLances();
                            break;
                        case Behavior.DLeft:
                        case Behavior.DRight:
                            State_ChargeAttack();
                            break;
                        case Behavior.Phase2Animation:
                            State_Phase2Animation();
                            break;
                        default:
                            State_PhaseSwitch();
                            break;
                    }
                    break;
            }
        }

        PostMainAI();

        return false;



        #region 行为函数
        void PreMainAI()
        {
            ApplyBaseSettings();
            HandlePhase3Visuals();

            void ApplyBaseSettings()
            {
                NPC.rotation = NPC.velocity.X * 0.005f;
                CalamityNPC.DR = 0.15f;

                if (InvalidPhase1)
                    CalamityNPC.DR = 0.99f;

                CalamityNPC.CurrentlyIncreasingDefenseOrDR = InvalidPhase1 || CurrentBehavior == Behavior.SD;

                if (NPC.life == NPC.lifeMax && NPC.ShouldEmpressBeEnraged() && !Enrage)
                    Enrage = true;

                NPC.Calamity().CurrentlyEnraged = Enrage;
            }

            void HandlePhase3Visuals()
            {
                float playSpawnSoundTime = 10f;
                float stopSpawningDustTime = 150f;
                float maxOpacity = Phase3 ? 0.7f : 1f;
                int minAlpha = 255 - (int)(255 * maxOpacity);

                if (Phase3)
                {
                    if (CalamityNPC.newAI[0] == playSpawnSoundTime)
                        SoundEngine.PlaySound(SoundID.Item161, NPC.Center);

                    if (CalamityNPC.newAI[0] > playSpawnSoundTime && CalamityNPC.newAI[0] < stopSpawningDustTime)
                        CreateSpawnDust();

                    CalamityNPC.newAI[0] += 1f;
                    if (CalamityNPC.newAI[0] >= stopSpawningDustTime)
                    {
                        CalamityNPC.newAI[0] = playSpawnSoundTime + 1f;
                        NPC.SyncExtraAI();
                    }
                }
            }
        }

        void PostMainAI()
        {
            NPC.dontTakeDamage = !TakeDamage;

            if (Phase3)
                NPC.defense = (int)Math.Round(NPC.defDefense * 0.8);
            else if (Phase2)
                NPC.defense = (int)Math.Round(NPC.defDefense * 1.2);
            else
                NPC.defense = NPC.defDefense;

            //更新动画帧
            if ((NPC.localAI[0] += 1f) >= 44f)
                NPC.localAI[0] = 0f;

            if (Visible)
                NPC.alpha = Utils.Clamp(NPC.alpha - 5, 0, 255);

            Lighting.AddLight(NPC.Center, Vector3.One * NPC.Opacity);
        }

        // ========== 状态0：生成动画 ==========
        void State_SpawnAnimation()
        {
            float playSpawnSoundTime = 10f;
            float stopSpawningDustTime = 150f;
            float spawnTime = 180f;

            NPC.damage = 0;

            if (Timer1 == 0)
            {
                NPC.velocity = new Vector2(0f, 5f);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0f, -80f), Vector2.Zero, ProjectileID.HallowBossDeathAurora, 0, 0f, Main.myPlayer);
            }

            if (Timer1 == playSpawnSoundTime)
                SoundEngine.PlaySound(SoundID.Item161, NPC.Center);

            NPC.velocity *= 0.95f;

            if (Timer1 > playSpawnSoundTime && Timer1 < stopSpawningDustTime)
                CreateSpawnDust();

            Timer1++;
            Visible = false;
            TakeDamage = false;
            NPC.Opacity = MathHelper.Clamp(Timer1 / spawnTime, 0f, 1f);

            if (Timer1 >= spawnTime)
            {
                if (NPC.ShouldEmpressBeEnraged() && !Enrage)
                    Enrage = true;

                State_PhaseSwitch();
                NPC.netUpdate = true;
                NPC.TargetClosest();
            }
        }

        // ========== 状态1：阶段切换 ==========
        void State_PhaseSwitch()
        {
            NPC.damage = 0;

            float idleTimer = Phase2 ? 10f : 20f;
            if (Main.getGoodWorld)
                idleTimer *= 0.5f;
            if (idleTimer < 10f)
                idleTimer = 10f;

            void HandleMovement()
            {
                if (Timer1 <= 10f)
                {
                    if (Timer1 == 0)
                        NPC.TargetClosest();

                    NPCAimedTarget targetData4 = NPC.GetTargetData();
                    if (targetData4.Invalid)
                    {
                        CurrentBehavior = Behavior.Despawn;
                        Timer1 = 0;
                        AttackCounter++;
                        NPC.velocity /= 4f;
                        NPC.netUpdate = true;
                        return;
                    }

                    Vector2 center = targetData4.Center;
                    center += new Vector2(0f, -400f);
                    if (NPC.Distance(center) > 200f)
                        center -= NPC.DirectionTo(center) * 100f;

                    Vector2 targetDirection = center - NPC.Center;
                    float lerpValue = Utils.GetLerpValue(100f, 600f, targetDirection.Length());
                    float targetDistance = targetDirection.Length();

                    float maxVelocity = 24f;
                    if (targetDistance > maxVelocity)
                        targetDistance = maxVelocity;

                    NPC.velocity = Vector2.Lerp(targetDirection.SafeNormalize(Vector2.Zero) * targetDistance, targetDirection / 6f, lerpValue);
                    NPC.netUpdate = true;
                }
            }

            void DetermineNextAttack()
            {
                NPC.TargetClosest();
                NPCAimedTarget targetData5 = NPC.GetTargetData();
                bool despawnFlag = false;
                if (Enrage)
                {
                    if (!Main.dayTime)
                        despawnFlag = true;

                    if (Main.dayTime && Main.time >= 53400D)
                        despawnFlag = true;
                }

                bool shouldUseDRight = targetData5.Center.X > NPC.Center.X;

                Behavior behavior = targetData5.Invalid || NPC.Distance(targetData5.Center) > DespawnDistance || despawnFlag ? Behavior.Despawn
                    : Phase2 ? (AttackCounter % 10) switch
                    {
                        0 => Main.rand.NextBool(2) ? Behavior.EL2B : Behavior.EL2A,
                        1 => Phase3 ? Behavior.DLeft : Behavior.PB1,
                        2 => shouldUseDRight ? Behavior.DRight : Behavior.DLeft,
                        3 => Behavior.EL3,
                        4 => Behavior.ER,
                        5 => Behavior.PB1,
                        6 => Phase3 ? Behavior.EL1 : Behavior.SD,
                        7 => Behavior.EL1,
                        8 => shouldUseDRight ? Behavior.DRight : Behavior.DLeft,
                        9 => Behavior.PB2,
                        _ => Behavior.PB1,
                    }
                    : (AttackCounter % 10) switch
                    {
                        0 => Behavior.PB1,
                        1 => Behavior.SD,
                        2 => shouldUseDRight ? Behavior.DRight : Behavior.DLeft,
                        3 => Behavior.EL1,
                        4 => Behavior.ER,
                        5 => shouldUseDRight ? Behavior.DRight : Behavior.DLeft,
                        6 => Behavior.PB1,
                        7 => Behavior.EL1,
                        8 => shouldUseDRight ? Behavior.DRight : Behavior.DLeft,
                        9 => Behavior.ER,
                        _ => Behavior.PB1
                    };

                if (behavior is not Behavior.PB1 and not Behavior.PB2)
                    NPC.velocity = NPC.DirectionFrom(targetData5.Center).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * shouldUseDRight.ToDirectionInt()) * 24f;

                CurrentBehavior = behavior;
                Timer1 = 0;
                AttackCounter += Main.rand.Next(2) + 1; //攻击随机性的来源
                NPC.netUpdate = true;
            }

            HandleMovement();
            NPC.velocity *= 0.92f;
            Timer1++;

            if (Timer1 >= idleTimer)
                DetermineNextAttack();
        }

        // ========== 状态2：彩虹条纹攻击 ==========
        void State_RainbowStreaks()
        {
            NPC.damage = 0;

            if (Timer1 == 0)
                SoundEngine.PlaySound(SoundID.Item164, NPC.Center);

            Vector2 randomStreakOffset = new(-55f, -30f);
            NPCAimedTarget targetData11 = NPC.GetTargetData();
            Vector2 targetCenter = targetData11.Invalid ? NPC.Center : targetData11.Center;

            if (NPC.Distance(targetCenter + RainbowStreakDistance) > MovementDistanceGateValue)
                NPC.SimpleFlyMovement(NPC.DirectionTo(targetCenter + RainbowStreakDistance).SafeNormalize(Vector2.Zero) * Velocity, Acceleration);

            if (Timer1 < 60f)
                DoMagicEffect(NPC.Center + randomStreakOffset, 1, Utils.GetLerpValue(0f, 60f, Timer1, clamped: true));

            int streakSpawnFrequency = CalamityWorld.LegendaryMode ? 1 : 2;
            if (Phase3)
                streakSpawnFrequency *= 2;

            void SpawnRainbowStreaks()
            {
                if ((int)Timer1 % streakSpawnFrequency == 0 && Timer1 < 60f)
                {
                    int projectileType = ProjectileID.HallowBossRainbowStreak;
                    int projectileDamage = NPC.GetProjectileDamage(projectileType) * ProjectileDamageMultiplier;

                    float ai3 = Timer1 / 60f;
                    Vector2 rainbowStreakVelocity = new Vector2(0f, -10f).RotatedBy(MathHelper.PiOver2 * Main.rand.NextFloatDirection());
                    if (Phase2)
                        rainbowStreakVelocity = new Vector2(0f, -12f).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat());

                    if (Enrage)
                        rainbowStreakVelocity *= MathHelper.Lerp(0.8f, 1.6f, ai3);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + randomStreakOffset, rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, NPC.target, ai3);
                        if (Phase3)
                        {
                            int proj2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + randomStreakOffset, -rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, NPC.target, 1f - ai3);
                            if (Main.rand.NextBool(60) && CalamityWorld.LegendaryMode)
                            {
                                Main.projectile[proj2].extraUpdates += 1;
                                Main.projectile[proj2].netUpdate = true;
                            }
                        }

                        if (Main.rand.NextBool(60) && CalamityWorld.LegendaryMode)
                        {
                            Main.projectile[proj].extraUpdates += 1;
                            Main.projectile[proj].netUpdate = true;
                        }
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int multiplayerStreakSpawnFrequency = (int)(Timer1 / streakSpawnFrequency);
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (NPC.Boss_CanShootExtraAt(i, multiplayerStreakSpawnFrequency % 3, 3, 2400f))
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + randomStreakOffset, rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, i, ai3);
                        }
                    }
                }
            }

            SpawnRainbowStreaks();
            Timer1++;

            float extraPhaseTime = (Enrage ? 30f : 60f) + 30f * LessTimeSpentPerPhaseMultiplier;
            if (Timer1 >= 60f + extraPhaseTime)
            {
                State_PhaseSwitch();
                NPC.netUpdate = true;
            }
        }

        // ========== 状态4：以太之矛攻击（环绕目标） ==========
        void State_EtherealLances()
        {
            NPC.damage = 0;

            if (Timer1 == 0)
                SoundEngine.PlaySound(SoundID.Item162, NPC.Center);

            float lanceGateValue = 75f;

            if (Timer1 is >= 6 and < 54)
            {
                DoMagicEffect(NPC.Center + new Vector2(-55f, -20f), 2, Utils.GetLerpValue(0f, lanceGateValue, Timer1, clamped: true));
                DoMagicEffect(NPC.Center + new Vector2(55f, -20f), 4, Utils.GetLerpValue(0f, lanceGateValue, Timer1, clamped: true));
            }

            NPCAimedTarget targetData10 = NPC.GetTargetData();
            Vector2 targetCenter = targetData10.Invalid ? NPC.Center : targetData10.Center;
            if (NPC.Distance(targetCenter + EtherealLanceDistance) > MovementDistanceGateValue)
                NPC.SimpleFlyMovement(NPC.DirectionTo(targetCenter + EtherealLanceDistance).SafeNormalize(Vector2.Zero) * Velocity, Acceleration);

            void SpawnEtherealLances()
            {
                int lanceRotation = 10;
                if (Timer1 % (Enrage ? 2f : 3f) == 0f && Timer1 < lanceGateValue)
                {
                    int lanceAmount = Phase3 ? 2 : 1;
                    for (int i = 0; i < lanceAmount; i++)
                    {
                        int lanceFrequency = (int)(Timer1 / (Enrage ? 2f : 3f));
                        lanceRotation += 5 * i;
                        Vector2 lanceDirection = Vector2.UnitX.RotatedBy((float)Math.PI / (lanceRotation * 2) + lanceFrequency * ((float)Math.PI / lanceRotation));
                        if (EL1_AnotherType)
                            lanceDirection.X += (lanceDirection.X > 0f) ? 0.5f : -0.5f;

                        lanceDirection = lanceDirection.SafeNormalize(Vector2.UnitY);
                        float spawnDistance = 600f;

                        Vector2 playerCenter = targetData10.Center;
                        if (NPC.Distance(playerCenter) > 2400f)
                            continue;

                        if (Vector2.Dot(targetData10.Velocity.SafeNormalize(Vector2.UnitY), lanceDirection) > 0f)
                            lanceDirection *= -1f;

                        Vector2 targetHoverPos = playerCenter + targetData10.Velocity * 90;
                        Vector2 spawnLocation = playerCenter + lanceDirection * spawnDistance - targetData10.Velocity * 30f;
                        if (spawnLocation.Distance(playerCenter) < spawnDistance)
                        {
                            Vector2 lanceSpawnDirection = playerCenter - spawnLocation;
                            if (lanceSpawnDirection == Vector2.Zero)
                                lanceSpawnDirection = lanceDirection;

                            spawnLocation = playerCenter - lanceSpawnDirection.SafeNormalize(Vector2.UnitY) * spawnDistance;
                        }

                        int projectileType = ProjectileID.FairyQueenLance;
                        int projectileDamage = NPC.GetProjectileDamage(projectileType) * ProjectileDamageMultiplier;

                        Vector2 v3 = targetHoverPos - spawnLocation;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnLocation, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v3.ToRotation(), Timer1 / lanceGateValue);

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            continue;

                        for (int j = 0; j < Main.maxPlayers; j++)
                        {
                            if (!NPC.Boss_CanShootExtraAt(j, lanceFrequency % 3, 3, 2400f))
                                continue;

                            Player extraPlayer = Main.player[j];
                            playerCenter = extraPlayer.Center;
                            if (Vector2.Dot(extraPlayer.velocity.SafeNormalize(Vector2.UnitY), lanceDirection) > 0f)
                                lanceDirection *= -1f;

                            Vector2 extraPlayerSpawnLocation = playerCenter + extraPlayer.velocity * 90;
                            spawnLocation = playerCenter + lanceDirection * spawnDistance - extraPlayer.velocity * 30f;
                            if (spawnLocation.Distance(playerCenter) < spawnDistance)
                            {
                                Vector2 extraPlayerSpawnDirection = playerCenter - spawnLocation;
                                if (extraPlayerSpawnDirection == Vector2.Zero)
                                    extraPlayerSpawnDirection = lanceDirection;

                                spawnLocation = playerCenter - extraPlayerSpawnDirection.SafeNormalize(Vector2.UnitY) * spawnDistance;
                            }

                            v3 = extraPlayerSpawnLocation - spawnLocation;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnLocation, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v3.ToRotation(), Timer1 / lanceGateValue);
                        }
                    }
                }
            }

            SpawnEtherealLances();
            Timer1++;

            float extraPhaseTime = (Enrage ? 24f : 48f) + 20f * LessTimeSpentPerPhaseMultiplier;
            if (Timer1 >= lanceGateValue + extraPhaseTime)
            {
                State_PhaseSwitch();
                NPC.netUpdate = true;
            }
        }

        // ========== 状态5：永恒彩虹螺旋 ==========
        void State_EverlastingRainbowSpiral()
        {
            NPC.damage = 0;

            if (Timer1 == 0)
                SoundEngine.PlaySound(SoundID.Item163, NPC.Center);

            Vector2 magicSpawnOffset = new(55f, -30f);
            Vector2 everlastingRainbowSpawn = NPC.Center + magicSpawnOffset;
            if (Timer1 < 42f)
                DoMagicEffect(NPC.Center + magicSpawnOffset, 3, Utils.GetLerpValue(0f, 42f, Timer1, clamped: true));

            NPCAimedTarget targetData7 = NPC.GetTargetData();
            Vector2 targetCenter = targetData7.Invalid ? NPC.Center : targetData7.Center;
            if (NPC.Distance(targetCenter + EverlastingRainbowDistance) > MovementDistanceGateValue)
                NPC.SimpleFlyMovement(NPC.DirectionTo(targetCenter + EverlastingRainbowDistance).SafeNormalize(Vector2.Zero) * Velocity, Acceleration);

            void SpawnRainbowSpiral()
            {
                if (Timer1 % 42f == 0f && Timer1 < 42f)
                {
                    float projRotation = MathHelper.TwoPi * Main.rand.NextFloat();
                    float totalProjectiles = CalamityWorld.LegendaryMode ? 30f : Enrage ? 22f : 15f;
                    int projIndex = 0;
                    bool inversePhase2SpreadPattern = Main.rand.NextBool();

                    for (float i = 0f; i < 1f; i += 1f / totalProjectiles)
                    {
                        int projectileType = ProjectileID.HallowBossLastingRainbow;
                        int projectileDamage = NPC.GetProjectileDamage(projectileType) * ProjectileDamageMultiplier;
                        int projectileType2 = ProjectileID.HallowBossRainbowStreak;
                        int projectileDamage2 = NPC.GetProjectileDamage(projectileType2) * ProjectileDamageMultiplier;

                        float projRotationMultiplier = i;
                        Vector2 spinningpoint = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 + MathHelper.TwoPi * projRotationMultiplier + projRotation);

                        float initialVelocity = 2f;
                        if (Enrage && projIndex % 2 == 0)
                            initialVelocity *= 2f;
                        if (CalamityWorld.LegendaryMode)
                            initialVelocity *= 1.5f;

                        if (Phase2)
                        {
                            float maxAddedVelocity = initialVelocity;
                            float addedVelocity = inversePhase2SpreadPattern ? Math.Abs(maxAddedVelocity - Math.Abs(MathHelper.Lerp(-maxAddedVelocity, maxAddedVelocity, Math.Abs(i - 0.5f) * 2f))) : Math.Abs(MathHelper.Lerp(-maxAddedVelocity, maxAddedVelocity, Math.Abs(i - 0.5f) * 2f));
                            initialVelocity += addedVelocity;
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), everlastingRainbowSpawn + spinningpoint.RotatedBy(-MathHelper.PiOver2) * 30f, spinningpoint * initialVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, 0f, projRotationMultiplier);

                            if (Phase3)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), everlastingRainbowSpawn + spinningpoint.RotatedBy(-MathHelper.PiOver2) * 30f, spinningpoint * 3f * initialVelocity, projectileType2, projectileDamage2, 0f, Main.myPlayer, NPC.target, projRotationMultiplier);
                            }
                        }

                        projIndex++;
                    }
                }
            }

            SpawnRainbowSpiral();
            Timer1++;

            float extraPhaseTime = (Enrage ? 36f : 72f) + 30f * LessTimeSpentPerPhaseMultiplier;
            if (Timer1 >= 72f + extraPhaseTime)
            {
                State_PhaseSwitch();
                NPC.netUpdate = true;
            }
        }

        // ========== 状态6：太阳之舞 ==========
        void State_SunDance()
        {
            NPC.damage = 0;
            CalamityNPC.DR = InvalidPhase1 ? 0.99f : 0.575f;

            int totalSunDances = Phase2 ? 2 : 3;
            float sunDanceGateValue = Enrage ? 35f : 40f;
            float totalSunDancePhaseTime = totalSunDances * sunDanceGateValue;

            Vector2 sunDanceHoverOffset = new(0f, -100f);
            Vector2 position = NPC.Center + sunDanceHoverOffset;

            NPCAimedTarget targetData2 = NPC.GetTargetData();
            Vector2 targetCenter = targetData2.Invalid ? NPC.Center : targetData2.Center;
            if (NPC.Distance(targetCenter + SunDanceDistance) > MovementDistanceGateValue)
                NPC.SimpleFlyMovement(NPC.DirectionTo(targetCenter + SunDanceDistance).SafeNormalize(Vector2.Zero) * Velocity * 0.3f, Acceleration * 0.7f);

            void SpawnSunDanceProjectiles()
            {
                if (Timer1 % sunDanceGateValue == 0f && Timer1 < totalSunDancePhaseTime)
                {
                    int projectileType = ProjectileID.FairyQueenSunDance;
                    int projectileDamage = NPC.GetProjectileDamage(projectileType) * ProjectileDamageMultiplier;

                    int sunDanceExtension = (int)(Timer1 / sunDanceGateValue);
                    int targetFloatDirection = (targetData2.Center.X > NPC.Center.X) ? 1 : 0;
                    float projAmount = Phase2 ? 8f : 6f;
                    float projRotation = 1f / projAmount;

                    for (float j = 0f; j < 1f; j += projRotation)
                    {
                        float projDirection = (j + projRotation * 0.5f + sunDanceExtension * projRotation * 0.5f) % 1f;
                        float ai = MathHelper.TwoPi * (projDirection + targetFloatDirection);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), position, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, ai, NPC.whoAmI);
                    }
                }
            }

            SpawnSunDanceProjectiles();
            Timer1++;

            float extraPhaseTime = (Enrage ? 105f : 140f) + 30f * LessTimeSpentPerPhaseMultiplier;
            if (Timer1 >= totalSunDancePhaseTime + extraPhaseTime)
            {
                State_PhaseSwitch();
                NPC.netUpdate = true;
            }
        }

        // ========== 状态7：以太之矛墙 ==========
        void State_EtherealLanceWalls()
        {
            NPC.damage = 0;
            bool expertAttack = CurrentBehavior == Behavior.EL2B;

            int numLanceWalls = expertAttack ? 6 : 4;
            int lanceWallSpawnGateValue = expertAttack ? 36 : 54;
            if (Enrage)
                lanceWallSpawnGateValue -= expertAttack ? 4 : 6;

            int lanceWallPhaseTime = lanceWallSpawnGateValue * numLanceWalls;

            NPCAimedTarget targetData9 = NPC.GetTargetData();
            Vector2 destination = targetData9.Invalid ? NPC.Center : targetData9.Center;
            if (NPC.Distance(destination + EtherealLanceDistance) > MovementDistanceGateValue)
                NPC.SimpleFlyMovement(NPC.DirectionTo(destination + EtherealLanceDistance).SafeNormalize(Vector2.Zero) * Velocity * 0.4f, Acceleration);

            void SpawnLanceWalls()
            {
                if (Timer1 % lanceWallSpawnGateValue == 0 && Timer1 < lanceWallPhaseTime)
                {
                    SoundEngine.PlaySound(SoundID.Item162, NPC.Center);

                    float totalProjectiles = 18f;
                    float lanceSpacing = 150f;
                    float lanceWallSize = totalProjectiles * lanceSpacing;

                    Vector2 lanceSpawnOffset = targetData9.Center;
                    if (NPC.Distance(lanceSpawnOffset) <= 3200f)
                    {
                        Vector2 lanceWallStartingPosition = Vector2.Zero;
                        Vector2 lanceWallDirection = Vector2.UnitY;
                        float lanceWallConvergence = 0.4f;
                        float lanceWallSizeMult = 1.4f;
                        totalProjectiles += 5f;
                        lanceSpacing += 50f;
                        lanceWallSize *= 0.75f;
                        float direction = 1f;

                        int randomLanceWallType;
                        do randomLanceWallType = Main.rand.Next(numLanceWalls);
                        while (randomLanceWallType == CalamityNPC.newAI[3]);

                        CalamityNPC.newAI[3] = randomLanceWallType;
                        CalamityNPC.newAI[1] += 1f;
                        NPC.SyncExtraAI();

                        switch (randomLanceWallType)
                        {
                            case 0:
                                lanceSpawnOffset += new Vector2((0f - lanceWallSize) / 2f, 0f) * direction;
                                lanceWallStartingPosition = new Vector2(0f, lanceWallSize);
                                lanceWallDirection = Vector2.UnitX;
                                break;

                            case 1:
                                lanceSpawnOffset += new Vector2(lanceWallSize / 2f, lanceSpacing / 2f) * direction;
                                lanceWallStartingPosition = new Vector2(0f, lanceWallSize);
                                lanceWallDirection = -Vector2.UnitX;
                                break;

                            case 2:
                                lanceSpawnOffset += new Vector2(0f - lanceWallSize, 0f - lanceWallSize) * lanceWallConvergence * direction;
                                lanceWallStartingPosition = new Vector2(lanceWallSize * lanceWallSizeMult, 0f);
                                lanceWallDirection = new Vector2(1f, 1f);
                                break;

                            case 3:
                                lanceSpawnOffset += new Vector2(lanceWallSize * lanceWallConvergence + lanceSpacing / 2f, (0f - lanceWallSize) * lanceWallConvergence) * direction;
                                lanceWallStartingPosition = new Vector2((0f - lanceWallSize) * lanceWallSizeMult, 0f);
                                lanceWallDirection = new Vector2(-1f, 1f);
                                break;

                            case 4:
                                lanceSpawnOffset += new Vector2(0f - lanceWallSize, lanceWallSize) * lanceWallConvergence * direction;
                                lanceWallStartingPosition = new Vector2(lanceWallSize * lanceWallSizeMult, 0f);
                                lanceWallDirection = lanceSpawnOffset.DirectionTo(targetData9.Center);
                                break;

                            case 5:
                                lanceSpawnOffset += new Vector2(lanceWallSize * lanceWallConvergence + lanceSpacing / 2f, lanceWallSize * lanceWallConvergence) * direction;
                                lanceWallStartingPosition = new Vector2((0f - lanceWallSize) * lanceWallSizeMult, 0f);
                                lanceWallDirection = lanceSpawnOffset.DirectionTo(targetData9.Center);
                                break;
                        }

                        int projectileType = ProjectileID.FairyQueenLance;
                        int projectileDamage = NPC.GetProjectileDamage(projectileType) * ProjectileDamageMultiplier;

                        for (float i = 0f; i <= 1f; i += 1f / totalProjectiles)
                        {
                            Vector2 spawnLocation = lanceSpawnOffset + lanceWallStartingPosition * (i - 0.5f) * (expertAttack ? 1f : 2f);
                            Vector2 v2 = lanceWallDirection;
                            if (expertAttack)
                            {
                                Vector2 lanceWallSpawnPredictiveness = targetData9.Velocity * 20f * i;
                                Vector2 lanceWallSpawnLocation = spawnLocation.DirectionTo(targetData9.Center + lanceWallSpawnPredictiveness);
                                v2 = Vector2.Lerp(lanceWallDirection, lanceWallSpawnLocation, 0.75f).SafeNormalize(Vector2.UnitY);
                            }

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnLocation, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v2.ToRotation(), i);
                        }
                    }

                    if (Main.rand.NextBool(5 - ((int)CalamityNPC.newAI[1] - 2)) && CalamityNPC.newAI[1] >= 2f)
                    {
                        Timer1 = lanceWallPhaseTime;
                        NPC.netUpdate = true;
                    }
                }
            }

            SpawnLanceWalls();
            Timer1++;

            float extraPhaseTime = (Enrage ? 24f : 48f) + 20f * LessTimeSpentPerPhaseMultiplier;
            if (Timer1 >= lanceWallPhaseTime + extraPhaseTime)
            {
                State_PhaseSwitch();
                CalamityNPC.newAI[3] = 0f;
                CalamityNPC.newAI[1] = 0f;
                CalamityNPC.newAI[2] = 0f;
                NPC.SyncExtraAI();
                NPC.netUpdate = true;
            }
        }

        // ========== 状态8/9：冲锋攻击 ==========
        void State_ChargeAttack()
        {
            int chargeDirection = (CurrentBehavior == Behavior.DRight).ToDirectionInt();

            DoMagicEffect(NPC.Center, 5, Utils.GetLerpValue(40f, 90f, Timer1, clamped: true));

            int chargeGateValue = 40;
            int playChargeSoundTime = 20;
            int chargeDuration = Phase3 ? 40 : 50;
            int slowDownTime = 30;
            int totalPhaseTime = chargeGateValue + chargeDuration + slowDownTime;
            float chargeStartDistance = Phase3 ? 1000f : 800f;
            float chargeVelocity = Phase3 ? 100f : 70f;
            float chargeAcceleration = Phase3 ? 0.1f : 0.07f;

            void HandleChargeMovement()
            {
                if (Timer1 <= chargeGateValue)
                {
                    NPC.damage = 0;

                    if (Timer1 == playChargeSoundTime)
                        SoundEngine.PlaySound(SoundID.Item160, NPC.Center);

                    NPCAimedTarget targetData3 = NPC.GetTargetData();
                    Vector2 destination = (targetData3.Invalid ? NPC.Center : targetData3.Center) + new Vector2(chargeDirection * -chargeStartDistance, 0f);
                    NPC.SimpleFlyMovement(NPC.DirectionTo(destination).SafeNormalize(Vector2.Zero) * Velocity, Acceleration * 2f);

                    if (Timer1 == chargeGateValue)
                        NPC.velocity *= 0.3f;
                }
                else if (Timer1 <= chargeGateValue + chargeDuration)
                {
                    HandleChargeRainbowStreaks();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(chargeDirection * chargeVelocity, 0f), chargeAcceleration);

                    if (Timer1 == chargeGateValue + chargeDuration)
                        NPC.velocity *= 0.45f;

                    NPC.damage = (int)Math.Round(NPC.defDamage * (Enrage ? 3D : 1.5));
                }
                else
                {
                    NPC.damage = 0;
                    NPC.velocity *= 0.92f;
                }
            }

            void HandleChargeRainbowStreaks()
            {
                if (Timer1 == chargeGateValue + 1f)
                    SoundEngine.PlaySound(SoundID.Item164, NPC.Center);

                float rainbowStreakGateValue = 2f;
                if ((Timer1 - 1f) % rainbowStreakGateValue == 0f)
                {
                    int projectileType = ProjectileID.HallowBossRainbowStreak;
                    int projectileDamage = NPC.GetProjectileDamage(projectileType) * ProjectileDamageMultiplier;

                    float ai3 = (Timer1 - chargeGateValue - 1f) / chargeDuration;
                    Vector2 rainbowStreakVelocity = new Vector2(0f, -5f).RotatedBy(MathHelper.PiOver2 * Main.rand.NextFloatDirection());
                    if (Phase2)
                        rainbowStreakVelocity = new Vector2(0f, -6f).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat());

                    rainbowStreakVelocity.X *= 2f;
                    if (!Phase2)
                        rainbowStreakVelocity.Y *= 0.5f;

                    if (Enrage)
                        rainbowStreakVelocity *= MathHelper.Lerp(0.8f, 1.6f, ai3);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, NPC.target, ai3);
                        if (Main.rand.NextBool(30) && CalamityWorld.LegendaryMode)
                        {
                            Main.projectile[proj].extraUpdates += 1;
                            Main.projectile[proj].netUpdate = true;
                        }
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int multiplayerStreakSpawnFrequency = (int)((Timer1 - chargeGateValue - 1f) / rainbowStreakGateValue);
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (NPC.Boss_CanShootExtraAt(i, multiplayerStreakSpawnFrequency % 3, 3, 2400f))
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, rainbowStreakVelocity, projectileType, projectileDamage, 0f, Main.myPlayer, i, ai3);
                        }
                    }
                }
            }

            HandleChargeMovement();
            Timer1++;

            float extraPhaseTime = (Enrage ? 24f : 48f) * LessTimeSpentPerPhaseMultiplier;
            if (Timer1 >= totalPhaseTime + extraPhaseTime)
            {
                State_PhaseSwitch();
                NPC.netUpdate = true;
            }
        }

        // ========== 状态10：第二阶段动画 ==========
        void State_Phase2Animation()
        {
            NPC.damage = 0;

            if (Timer1 == 0)
                SoundEngine.PlaySound(SoundID.Item161, NPC.Center);

            TakeDamage = Timer1 is < 30 or > 170;

            NPC.velocity *= 0.95f;

            if (Timer1 == 90)
            {
                CurrentPhase = Phase.Phase2;

                NPC.Center = NPC.GetTargetData().Center + new Vector2(0f, -250f);
                NPC.netUpdate = true;
            }

            Timer1++;
            if (Timer1 >= 180f)
            {
                State_PhaseSwitch();
                AttackCounter = 0;
                NPC.netUpdate = true;
            }
        }

        // ========== 状态11：直线以太之矛 ==========
        void State_StraightEtherealLances()
        {
            NPC.damage = 0;

            if (Timer1 == 0)
                SoundEngine.PlaySound(SoundID.Item162, NPC.Center);

            float lanceGateValue2 = 75f;

            if (Timer1 is >= 6 and < 54)
            {
                DoMagicEffect(NPC.Center + new Vector2(-55f, -20f), 2, Utils.GetLerpValue(0f, lanceGateValue2, Timer1, clamped: true));
                DoMagicEffect(NPC.Center + new Vector2(55f, -20f), 4, Utils.GetLerpValue(0f, lanceGateValue2, Timer1, clamped: true));
            }

            NPCAimedTarget targetData6 = NPC.GetTargetData();
            Vector2 targetCenter = targetData6.Invalid ? NPC.Center : targetData6.Center;
            if (NPC.Distance(targetCenter + EtherealLanceDistance) > MovementDistanceGateValue)
                NPC.SimpleFlyMovement(NPC.DirectionTo(targetCenter + EtherealLanceDistance).SafeNormalize(Vector2.Zero) * Velocity, Acceleration);

            void SpawnStraightLances()
            {
                float etherealLanceGateValue = 5f;
                if (Enrage)
                    etherealLanceGateValue -= 1f;

                if (Timer1 % etherealLanceGateValue == 0f && Timer1 < lanceGateValue2)
                {
                    int numLances = Phase3 ? 4 : 3;
                    for (int i = 0; i < numLances; i++)
                    {
                        bool oppositeLance = i % 2 == 0;

                        Vector2 inverseTargetVel = oppositeLance ? targetData6.Velocity : -targetData6.Velocity;
                        inverseTargetVel.SafeNormalize(-Vector2.UnitY);
                        float spawnDistance = 100f + (i * 100f);

                        targetCenter = targetData6.Center;
                        if (NPC.Distance(targetCenter) > 2400f)
                            continue;

                        Vector2 straightLanceSpawnPredict = targetCenter + (oppositeLance ? -targetData6.Velocity : targetData6.Velocity) * 90;
                        Vector2 straightLanceSpawnDirection = targetCenter + inverseTargetVel * spawnDistance;
                        if (straightLanceSpawnDirection.Distance(targetCenter) < spawnDistance)
                        {
                            Vector2 straightLanceSpawnLocation = targetCenter - straightLanceSpawnDirection;
                            if (straightLanceSpawnLocation == Vector2.Zero)
                                straightLanceSpawnLocation = inverseTargetVel;

                            straightLanceSpawnDirection = targetCenter - straightLanceSpawnLocation.SafeNormalize(Vector2.UnitY) * spawnDistance;
                        }

                        int projectileType = ProjectileID.FairyQueenLance;
                        int projectileDamage = NPC.GetProjectileDamage(projectileType) * ProjectileDamageMultiplier;

                        Vector2 v = straightLanceSpawnPredict - straightLanceSpawnDirection;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), straightLanceSpawnDirection, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v.ToRotation(), Timer1 / lanceGateValue2);

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            continue;

                        int multiplayerExtraStraightLances = (int)(Timer1 / etherealLanceGateValue);
                        for (int l = 0; l < Main.maxPlayers; l++)
                        {
                            if (!NPC.Boss_CanShootExtraAt(l, multiplayerExtraStraightLances % 3, 3, 2400f))
                                continue;

                            Player player = Main.player[l];
                            inverseTargetVel = oppositeLance ? player.velocity : -player.velocity;
                            inverseTargetVel.SafeNormalize(-Vector2.UnitY);
                            targetCenter = player.Center;
                            Vector2 extraPlayerLancePredict = targetCenter + (oppositeLance ? -player.velocity : player.velocity) * 90;
                            straightLanceSpawnDirection = targetCenter + inverseTargetVel * spawnDistance;
                            if (straightLanceSpawnDirection.Distance(targetCenter) < spawnDistance)
                            {
                                Vector2 extraPlayerLanceSpawnLocation = targetCenter - straightLanceSpawnDirection;
                                if (extraPlayerLanceSpawnLocation == Vector2.Zero)
                                    extraPlayerLanceSpawnLocation = inverseTargetVel;

                                straightLanceSpawnDirection = targetCenter - extraPlayerLanceSpawnLocation.SafeNormalize(Vector2.UnitY) * spawnDistance;
                            }

                            v = extraPlayerLancePredict - straightLanceSpawnDirection;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), straightLanceSpawnDirection, Vector2.Zero, projectileType, projectileDamage, 0f, Main.myPlayer, v.ToRotation(), Timer1 / lanceGateValue2);
                        }
                    }
                }
            }

            SpawnStraightLances();
            Timer1++;

            float extraPhaseTime = (Enrage ? 24f : 48f) * LessTimeSpentPerPhaseMultiplier;
            if (Timer1 >= lanceGateValue2 + extraPhaseTime)
            {
                State_PhaseSwitch();
                NPC.netUpdate = true;
            }
        }

        // ========== 状态12：固定位置彩虹条纹 ==========
        void State_StationaryRainbowStreaks()
        {
            NPC.damage = 0;

            Vector2 projRandomOffset = new(-55f, -30f);

            if (Timer1 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item165, NPC.Center);
                NPC.velocity = new Vector2(0f, -12f);
            }

            NPC.velocity *= 0.95f;

            bool shouldSpawnStreaks = Timer1 is < 60 and >= 10;
            if (shouldSpawnStreaks)
                DoMagicEffect(NPC.Center + projRandomOffset, 1, Utils.GetLerpValue(0f, 60f, Timer1, clamped: true));

            void SpawnStationaryStreaks()
            {
                int stationaryStreakSpawnFrequency = 4;
                if (Enrage)
                    stationaryStreakSpawnFrequency -= 1;
                if (Phase3)
                    stationaryStreakSpawnFrequency *= 2;

                float streakHomeTime = (Timer1 - 10f) / 50f;
                if ((int)Timer1 % stationaryStreakSpawnFrequency == 0 && shouldSpawnStreaks)
                {
                    int projectileType = ProjectileID.HallowBossRainbowStreak;
                    int projectileDamage = NPC.GetProjectileDamage(projectileType) * ProjectileDamageMultiplier;

                    Vector2 vector = new Vector2(0f, -24f - (Phase3 ? (6f * streakHomeTime) : 0f)).RotatedBy(MathHelper.TwoPi * streakHomeTime);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + projRandomOffset, vector, projectileType, projectileDamage, 0f, Main.myPlayer, NPC.target, streakHomeTime);
                        if (Phase3)
                        {
                            int proj2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + projRandomOffset, -vector, projectileType, projectileDamage, 0f, Main.myPlayer, NPC.target, 1f - streakHomeTime);
                            if (Main.rand.NextBool(15) && CalamityWorld.LegendaryMode)
                            {
                                Main.projectile[proj2].extraUpdates += 1;
                                Main.projectile[proj2].netUpdate = true;
                            }
                        }

                        if (Main.rand.NextBool(15) && CalamityWorld.LegendaryMode)
                        {
                            Main.projectile[proj].extraUpdates += 1;
                            Main.projectile[proj].netUpdate = true;
                        }
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int extraStationaryStreakSpawnFrequency = (int)(Timer1 % stationaryStreakSpawnFrequency);
                        for (int j = 0; j < Main.maxPlayers; j++)
                        {
                            if (NPC.Boss_CanShootExtraAt(j, extraStationaryStreakSpawnFrequency % 3, 3, 2400f))
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + projRandomOffset, vector, projectileType, projectileDamage, 0f, Main.myPlayer, j, streakHomeTime);
                        }
                    }
                }
            }

            SpawnStationaryStreaks();
            Timer1++;

            float extraPhaseTime = (Enrage ? 36f : 72f) + 30f * LessTimeSpentPerPhaseMultiplier;
            if (Timer1 >= 105f + extraPhaseTime)
            {
                State_PhaseSwitch();
                NPC.netUpdate = true;
            }
        }

        // ========== 状态13：消失 ==========
        void State_Despawn()
        {
            NPC.damage = 0;

            if (Timer1 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item165, NPC.Center);
                NPC.velocity = new Vector2(0f, -7f);
            }

            NPC.velocity *= 0.95f;

            NPC.TargetClosest();
            NPCAimedTarget targetData = NPC.GetTargetData();

            Visible = false;

            bool trueDespawnFlag = false;
            bool shouldDespawn = false;
            if (!trueDespawnFlag)
            {
                if (Enrage)
                {
                    if (!Main.dayTime)
                        shouldDespawn = true;

                    if (Main.dayTime && Main.time >= 53400.0)
                        shouldDespawn = true;
                }

                trueDespawnFlag = trueDespawnFlag || shouldDespawn;
            }

            if (!trueDespawnFlag)
            {
                bool hasNoTarget = targetData.Invalid || NPC.Distance(targetData.Center) > DespawnDistance;
                trueDespawnFlag = trueDespawnFlag || hasNoTarget;
            }

            NPC.alpha = Utils.Clamp(NPC.alpha + trueDespawnFlag.ToDirectionInt() * 5, 0, 255);
            bool alphaExtreme = NPC.alpha is 0 or 255;

            void SpawnDespawnDust()
            {
                int despawnDustAmt = 5;
                for (int i = 0; i < despawnDustAmt; i++)
                {
                    float despawnDustOpacity = MathHelper.Lerp(1.3f, 0.7f, NPC.Opacity);
                    Color newColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                    int despawnRainbowDust = Dust.NewDust(NPC.position - NPC.Size * 0.5f, NPC.width * 2, NPC.height * 2, DustID.RainbowMk2, 0f, 0f, 0, newColor);
                    Main.dust[despawnRainbowDust].position = NPC.Center + Main.rand.NextVector2Circular(NPC.width, NPC.height);
                    Main.dust[despawnRainbowDust].velocity *= Main.rand.NextFloat() * 0.8f;
                    Main.dust[despawnRainbowDust].noGravity = true;
                    Main.dust[despawnRainbowDust].scale = 0.9f + Main.rand.NextFloat() * 1.2f;
                    Main.dust[despawnRainbowDust].fadeIn = 0.4f + Main.rand.NextFloat() * 1.2f * despawnDustOpacity;
                    Main.dust[despawnRainbowDust].velocity += Vector2.UnitY * -2f;
                    Main.dust[despawnRainbowDust].scale = 0.35f;
                    if (despawnRainbowDust != 6000)
                    {
                        Dust dust = Dust.CloneDust(despawnRainbowDust);
                        dust.scale /= 2f;
                        dust.fadeIn *= 0.85f;
                        dust.color = new Color(255, 255, 255, 255);
                    }
                }
            }

            SpawnDespawnDust();
            Timer1++;

            if (Timer1 >= 20f && alphaExtreme)
            {
                if (NPC.alpha == 255)
                {
                    NPC.active = false;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);

                    return;
                }

                State_PhaseSwitch();
                NPC.netUpdate = true;
            }
        }

        void CreateSpawnDust() => Dust.NewDustAction(NPC.Center, NPC.width * 2, NPC.height * 2, DustID.RainbowMk2, action: d =>
            {
                d.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                d.position = NPC.Center + Main.rand.NextVector2Circular(NPC.width, NPC.height);
                d.velocity *= Main.rand.NextFloat(0.8f);
                d.noGravity = true;
                d.scale = 0.9f + Main.rand.NextFloat(1.2f);
                d.fadeIn = 0.4f + Main.rand.NextFloat(1.2f) * MathHelper.Lerp(1.3f, 0.7f, NPC.Opacity);
                d.velocity += Vector2.UnitY * -2f;
                d.scale = 0.35f;

                Dust dClone = Dust.CloneDust(d);
                dClone.scale /= 2f;
                dClone.fadeIn *= 0.85f;
                dClone.color = new Color(255, 255, 255, 255);
            });

        void DoMagicEffect(Vector2 spot, int effectType, float progress)
        {
            float magicDustSpawnArea = 4f;
            float magicDustColorMult = 1f;
            float fadeIn = 0f;
            float magicDustPosChange = 0.5f;
            int magicAmt = 2;
            int magicDustType = 267;
            switch (effectType)
            {
                case 1:
                    magicDustColorMult = 0.5f;
                    fadeIn = 2f;
                    magicDustPosChange = 0f;
                    break;
                case 2:
                case 4:
                    magicDustSpawnArea = 50f;
                    magicDustColorMult = 0.5f;
                    fadeIn = 0f;
                    magicDustPosChange = 0f;
                    magicAmt = 4;
                    break;
                case 3:
                    magicDustSpawnArea = 30f;
                    magicDustColorMult = 0.1f;
                    fadeIn = 2.5f;
                    magicDustPosChange = 0f;
                    break;
                case 5:
                    if (progress == 0f)
                        magicAmt = 0;
                    else
                    {
                        magicAmt = 5;
                        magicDustType = Main.rand.Next(86, 92);
                    }
                    if (progress >= 1f)
                        magicAmt = 0;
                    break;
            }

            for (int i = 0; i < magicAmt; i++)
            {
                Dust.NewDustPerfectAction(spot, magicDustType, d =>
                {
                    d.velocity = Main.rand.NextVector2CircularEdge(magicDustSpawnArea, magicDustSpawnArea) * (Main.rand.NextFloat() * (1f - magicDustPosChange) + magicDustPosChange);
                    d.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                    d.scale = (Main.rand.NextFloat() * 2f + 2f) * magicDustColorMult;
                    d.fadeIn = fadeIn;
                    d.noGravity = true;
                    switch (effectType)
                    {
                        case 2:
                        case 4:
                            {
                                d.velocity *= 0.005f;
                                d.scale = 3f * Utils.GetLerpValue(0.7f, 0f, progress, clamped: true) * Utils.GetLerpValue(0f, 0.3f, progress, clamped: true);
                                d.velocity = (MathHelper.TwoPi * (i / 4f) + MathHelper.PiOver4).ToRotationVector2() * 8f * Utils.GetLerpValue(1f, 0f, progress, clamped: true);
                                d.velocity += base.NPC.velocity * 0.3f;
                                float magicDustColorChange = 0f;
                                if (effectType == 4)
                                    magicDustColorChange = 0.5f;

                                d.color = Main.hslToRgb((i / 5f + magicDustColorChange + progress * 0.5f) % 1f, 1f, 0.5f);
                                d.color.A /= 2;
                                d.alpha = 127;
                                break;
                            }
                        case 5:
                            if (progress == 0f)
                            {
                                d.customData = base.NPC;
                                d.scale = 1.5f;
                                d.fadeIn = 0f;
                                d.velocity = new Vector2(0f, -1f) + Main.rand.NextVector2Circular(1f, 1f);
                                d.color = new Color(255, 255, 255, 80) * 0.3f;
                            }
                            else
                            {
                                d.color = Main.hslToRgb(progress * 2f % 1f, 1f, 0.5f);
                                d.alpha = 0;
                                d.scale = 1f;
                                d.fadeIn = 1.3f;
                                d.velocity *= 3f;
                                d.velocity.X *= 0.1f;
                                d.velocity += base.NPC.velocity * 1f;
                            }
                            break;
                    }
                });
            }
        }
        #endregion 行为函数
    }
}
