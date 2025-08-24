using CalamityMod.Events;

namespace CalamityAnomalies.Anomaly.EmpressofLight;

public sealed class AnomalyEmpressofLight : AnomalyNPCBehavior
{
    #region 枚举、数值、AI状态
    public enum Behavior
    {
        Despawn = -1,

        None = 0,

        SpawnAnimation,
        Phase2Animation,
        BehaviorSwitch,

        //原版攻击模式
        /// <summary>
        /// 七彩矢1（常规）。
        /// </summary>
        PB1,
        /// <summary>
        /// 七彩矢2（弥散）。
        /// </summary>
        PB2,
        /// <summary>
        /// 太阳舞。
        /// </summary>
        SD,
        /// <summary>
        /// 永恒彩虹。
        /// </summary>
        ER,
        /// <summary>
        /// 空灵长枪1（常规）。
        /// </summary>
        EL1,
        /// <summary>
        /// 空灵长枪2A（线状）。
        /// </summary>
        EL2A,
        /// <summary>
        /// 空灵长枪2B（集中）。
        /// </summary>
        EL2B,
        /// <summary>
        /// 空灵长枪3（直线）。
        /// </summary>
        EL3,
        /// <summary>
        /// 冲刺（向左）。
        /// </summary>
        DLeft,
        /// <summary>
        /// 冲刺（向右）。
        /// </summary>
        DRight,

        //额外攻击模式
    }

    public static class Data
    {
        public const int PlaySpawnSoundTime = 10;
        public const int StopSpawningDustTime = 150;
        public const int SpawnTime = 180;
        public const float Phase2GateValue = 0.7f;
    }

    public Behavior CurrentAttack
    {
        get => (int)NPC.ai[0] switch
        {
            0 => Behavior.SpawnAnimation,
            1 => Behavior.BehaviorSwitch,
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
                case Behavior.BehaviorSwitch:
                    NPC.ai[0] = 1f;
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

    public int Timer
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public enum PhaseEnrageData
    {
        Unknown,

        Phase1,
        Phase2,
        Phase3,
        Phase1Enraged,
        Phase2Enraged,
        Phase3Enraged,
    }

    public PhaseEnrageData PhaseEnrage
    {
        get => NPC.ai[3] switch
        {
            0f => PhaseEnrageData.Phase1,
            1f => AnomalyNPC.AnomalyAI32[0].i switch
            {
                0 => PhaseEnrageData.Phase2,
                1 => PhaseEnrageData.Phase3,
                _ => PhaseEnrageData.Unknown
            },
            2f => PhaseEnrageData.Phase1Enraged,
            3f => AnomalyNPC.AnomalyAI32[0].i switch
            {
                0 => PhaseEnrageData.Phase2Enraged,
                1 => PhaseEnrageData.Phase3Enraged,
                _ => PhaseEnrageData.Unknown
            },
            _ => PhaseEnrageData.Unknown
        };
        set
        {
            switch (value)
            {
                case PhaseEnrageData.Unknown:
                    break;
                case PhaseEnrageData.Phase1:
                    NPC.ai[3] = 0f;
                    break;
                case PhaseEnrageData.Phase2:
                    NPC.ai[3] = 1f;
                    if (AnomalyNPC.AnomalyAI32[1].i != 0)
                    {
                        AnomalyNPC.AnomalyAI32[1].i = 0;
                        AnomalyNPC.AIChanged32[1] = true;
                    }
                    break;
                case PhaseEnrageData.Phase3:
                    NPC.ai[3] = 1f;
                    if (AnomalyNPC.AnomalyAI32[1].i != 1)
                    {
                        AnomalyNPC.AnomalyAI32[1].i = 1;
                        AnomalyNPC.AIChanged32[1] = true;
                    }
                    break;
                case PhaseEnrageData.Phase1Enraged:
                    NPC.ai[3] = 2f;
                    break;
                case PhaseEnrageData.Phase2Enraged:
                    NPC.ai[3] = 3f;
                    if (AnomalyNPC.AnomalyAI32[0].i != 0)
                    {
                        AnomalyNPC.AnomalyAI32[0].i = 0;
                        AnomalyNPC.AIChanged32[0] = true;
                    }
                    break;
                case PhaseEnrageData.Phase3Enraged:
                    NPC.ai[3] = 3f;
                    if (AnomalyNPC.AnomalyAI32[1].i != 1)
                    {
                        AnomalyNPC.AnomalyAI32[1].i = 1;
                        AnomalyNPC.AIChanged32[1] = true;
                    }
                    break;
            }
        }
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

    public bool IsInPhase2 => NPC.AI_120_HallowBoss_IsInPhase2(); //Vanilla: NPC.ai[3] is 1f or 3f

    public bool Enrage
    {
        get => NPC.AI_120_HallowBoss_IsGenuinelyEnraged(); //Vanilla: NPC.ai[3] is 2f or 3f
        set
        {
            if (value ^ Enrage)
                NPC.ai[3] += value.ToInt() * 2f;
        }
    }
    #endregion 枚举、数值、属性、AI状态

    public override int ApplyingType => NPCID.HallowBoss;

    public override bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC method) => method switch
    {
        OrigMethodType_CalamityGlobalNPC.PreAI => false,
        _ => true,
    };


    #region AI
    /*
    public override bool PreAI()
    {
        StartUp();
        
        
        //switch (NPC.ShouldEmpressBeEnraged(), CAWorld.AnomalyUltramundane)
        //{
        //    case (false, false):
        //        AI_Night();
        //        break;
        //    case (true, false):
        //        AI_Day();
        //        break;
        //    case (false, true):
        //        AI_NightUltra();
        //        break;
        //    case (true, true):
        //        AI_DayUltra();
        //        break;
        //}

        return false;
    }*/

    public void StartUp()
    {
        NPC.rotation = NPC.velocity.X * 0.005f;
        bool shouldBeInPhase2ButIsStillInPhase1 = OceanNPC.LifeRatio <= Data.Phase2GateValue && IsInPhase2;
        CalamityNPC.DR = shouldBeInPhase2ButIsStillInPhase1 ? 0.99f : 0.15f;
        CalamityNPC.unbreakableDR = shouldBeInPhase2ButIsStillInPhase1;
        if (NPC.life == NPC.lifeMax && NPC.ShouldEmpressBeEnraged() && !Enrage)
            Enrage = true;
        CalamityNPC.CurrentlyEnraged = !BossRushEvent.BossRushActive && Enrage;
        NPC.defense = (int)(NPC.defDefense * PhaseEnrage switch
        {
            PhaseEnrageData.Phase2 or PhaseEnrageData.Phase2Enraged => 1.2f,
            PhaseEnrageData.Phase3 or PhaseEnrageData.Phase3Enraged => 0.8f,
            _ => 1f
        });
        if (++NPC.localAI[0] >= 44f)
            NPC.localAI[0] = 0f;
        NPC.dontTakeDamage = !TakeDamage;
        if (Visible)
            NPC.alpha = Math.Clamp(NPC.alpha - 5, 0, 255);
        Lighting.AddLight(NPC.Center, Vector3.One * NPC.Opacity);
    }

    public void AI_Night()
    {

    }

    public void AI_Day()
    {

    }

    public void AI_NightUltra()
    {

    }

    public void AI_DayUltra()
    {

    }

    #region 行为函数
    public void CreateSpawnDust(bool useAI = true)
    {
        int spawnDustAmount = 2;
        float timer = useAI ? Timer : NPC.Calamity().newAI[0];
        for (int i = 0; i < spawnDustAmount; i++)
        {
            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.RainbowMk2, action: d =>
            {
                d.color = Main.hslToRgb(timer / Data.SpawnTime, 1f, 0.5f);
                d.position = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 3f, NPC.height * 3f) + new Vector2(0f, -150f);
                d.velocity *= Main.rand.NextFloat(0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f + Main.rand.NextFloat(0.7f) * MathHelper.Lerp(1.3f, 0.7f, NPC.Opacity) * Utils.GetLerpValue(0f, 120f, timer, clamped: true);
                d.velocity += new Vector2(0f, 3f);
                d.scale = 0.35f;
                Dust d2 = Dust.CloneDust(d);
                d2.scale /= 2f;
                d2.fadeIn *= 0.85f;
                d2.color = new(255, 255, 255, 255);
            });
        }
    }

    public void AI_120_HallowBoss_DoMagicEffect(Vector2 spot, int effectType, float progress)
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
                            d.velocity += NPC.velocity * 0.3f;
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
                            d.customData = NPC;
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
                            d.velocity += NPC.velocity * 1f;
                        }
                        break;
                }
            });
        }
    }

    public void SpawnAnimation()
    {
        NPC.damage = 0;

        if (Timer == 0f)
        {
            NPC.velocity = new Vector2(0f, 5f);
            if (Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0f, -80f), Vector2.Zero, ProjectileID.HallowBossDeathAurora, 0, 0f, Main.myPlayer);
        }

        if (Timer == Data.PlaySpawnSoundTime)
            SoundEngine.PlaySound(SoundID.Item161, NPC.Center);

        NPC.velocity *= 0.95f;

        if (Timer is > Data.PlaySpawnSoundTime and < Data.StopSpawningDustTime)
            CreateSpawnDust();

        Timer++;
        Visible = false;
        TakeDamage = false;
        NPC.Opacity = MathHelper.Clamp((float)Timer / Data.SpawnTime, 0f, 1f);

        if (Timer >= Data.SpawnTime)
        {
            if (NPC.ShouldEmpressBeEnraged() && !NPC.AI_120_HallowBoss_IsGenuinelyEnraged())
                Enrage = true;

            CurrentAttack = Behavior.BehaviorSwitch;
            Timer = 0;
            NPC.netUpdate = true;
            NPC.TargetClosest();
        }
    }

    public void BehaviorSwitch()
    {

    }
    #endregion
    #endregion AI
}
