﻿using System;
using CalamityAnomalies.Override;
using CalamityMod;
using CalamityMod.NPCs.NormalNPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Transoceanic;
using Transoceanic.Core.GameData;
using Transoceanic.Core.MathHelp;
using Transoceanic.GlobalInstances;

namespace CalamityAnomalies.Contents.AnomalyMode.NPCs.KingSlime;

public class AnomalyKingSlime : AnomalyNPCOverride
{
    #region 枚举、常量、属性、AI状态
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

    private static class Constant
    {
        public const float despawnDistance = 5000f;

        public static readonly float[] maxScale = [6f, 7.5f];
        public static readonly float[] minScale = [0.5f, 0.5f];
        public static readonly float[] spawnSlimeGateValue = [0.03f, 0.025f];
        public static readonly float[] spawnSlimePow = [0.3f, 0.5f];
        public static readonly float[] teleportRate = [0.97f, 0.95f];
    }

    /// <summary>
    /// 当前阶段。0，1，2。
    /// </summary>
    public int AI_CurrentPhase
    {
        get => (int)AnomalyNPC.AnomalyAI[0];
        set => AnomalyNPC.SetAnomalyAI(value, 0);
    }

    public AttackType AI_CurrentAttack
    {
        get => (AttackType)(int)AnomalyNPC.AnomalyAI[1];
        set => AnomalyNPC.SetAnomalyAI((int)value, 1);
    }

    public int AI_CurrentAttackPhase
    {
        get => (int)AnomalyNPC.AnomalyAI[2];
        set => AnomalyNPC.SetAnomalyAI(value, 2);
    }

    public int AI_JewelSpawn
    {
        get => (int)AnomalyNPC.AnomalyAI[3];
        set => AnomalyNPC.SetAnomalyAI(value, 3);
    }

    public bool AI_JewelEmeraldSpawned
    {
        get => TOMathHelper.GetBit(AI_JewelSpawn, 0);
        set => AI_JewelSpawn = TOMathHelper.SetBit(AI_JewelSpawn, 0, true);
    }

    public bool AI_JewelRubySpawned
    {
        get => TOMathHelper.GetBit(AI_JewelSpawn, 0);
        set => AI_JewelSpawn = TOMathHelper.SetBit(AI_JewelSpawn, 0, true);
    }

    public bool AI_JewelSapphireSpawned
    {
        get => TOMathHelper.GetBit(AI_JewelSpawn, 0);
        set => AI_JewelSpawn = TOMathHelper.SetBit(AI_JewelSpawn, 0, true);
    }

    public int AI_LastSpawnSlimeLife
    {
        get => (int)AnomalyNPC.AnomalyAI[4];
        set => AnomalyNPC.SetAnomalyAI(value, 4);
    }

    public float AI_TeleportTimer
    {
        get => AnomalyNPC.AnomalyAI[5];
        set => AnomalyNPC.SetAnomalyAI(value, 5);
    }

    /// <summary>
    /// 王冠绿宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC AI_JewelEmerald
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[6]];
        set => AnomalyNPC.SetAnomalyAI(value.whoAmI, 6);
    }

    public bool Jewel_EmeraldAlive => AI_JewelEmerald.active && AI_JewelEmerald.ModNPC is KingSlimeJewelEmerald && AI_JewelEmerald.Ocean().Master == NPC.whoAmI;

    /// <summary>
    /// 王冠红宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC AI_JewelRuby
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[7]];
        set => AnomalyNPC.SetAnomalyAI(value.whoAmI, 7);
    }

    public bool Jewel_RubyAlive => AI_JewelRuby.active && AI_JewelRuby.ModNPC is KingSlimeJewelRuby && AI_JewelRuby.Ocean().Master == NPC.whoAmI;

    /// <summary>
    /// 王冠蓝宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC AI_JewelSapphire
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[8]];
        set => AnomalyNPC.SetAnomalyAI(value.whoAmI, 8);
    }

    public bool Jewel_SapphireAlive => AI_JewelSapphire.active && AI_JewelSapphire.ModNPC is KingSlimeJewelSapphire && AI_JewelSapphire.Ocean().Master == NPC.whoAmI;

    public int AI_SmallJumpCounter
    {
        get => (int)AnomalyNPC.AnomalyAI[9];
        set => AnomalyNPC.SetAnomalyAI(value, 9);
    }

    public int AI_ChangedVelocityDirectionWhenJump
    {
        get => (int)AnomalyNPC.AnomalyAI[10];
        set => AnomalyNPC.SetAnomalyAI(value, 10);
    }

    public float AI_TeleportScaleMultiplier
    {
        get => AnomalyNPC.AnomalyAI[11];
        set => AnomalyNPC.SetAnomalyAI(Math.Clamp(value, 0f, 1f), 11);
    }

    public Vector2 AI_TeleportDestination
    {
        get => new(AnomalyNPC.AnomalyAI[12], AnomalyNPC.AnomalyAI[13]);
        set
        {
            AnomalyNPC.SetAnomalyAI(value.X, 12);
            AnomalyNPC.SetAnomalyAI(value.Y, 13);
        }
    }

    public float AI_DespawnScaleMultiplier
    {
        get => AnomalyNPC.AnomalyAI[14];
        set => AnomalyNPC.SetAnomalyAI(Math.Clamp(value, 0f, 1f), 14);
    }


    #endregion

    public override int OverrideType => NPCID.KingSlime;

    public override bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC type) => type switch
    {
        OrigMethodType_CalamityGlobalNPC.PreAI => false,
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

        AI_TeleportScaleMultiplier = 1f;
        AI_DespawnScaleMultiplier = 1f;

        AI_JewelEmerald = TOMain.DummyNPC;
        AI_JewelRuby = TOMain.DummyNPC;
        AI_JewelSapphire = TOMain.DummyNPC;
    }
    #endregion

    #region AI
    public override bool PreAI()
    {
        if (AI_CurrentAttack == AttackType.Despawn || !TargetClosestIfInvalid(true, Constant.despawnDistance))
        {
            Despawn();
            return false;
        }
        else
            NPC.FaceTargetTO(Target);

        switch (AI_CurrentPhase)
        {
            case 0: //初始化
                AI_Timer1++;
                if (NPC.velocity.Y == 0f)
                    AI_Timer2++;
                if (AI_Timer2 > 20)
                {
                    NPC.velocity = Vector2.Zero;
                    AI_LastSpawnSlimeLife = NPC.life;
                    AI_CurrentPhase = 1;
                    SelectNextAttack();
                }
                else if (AI_Timer1 > 600)
                {
                    AI_CurrentAttack = AttackType.Despawn;
                    Despawn();
                }
                break;
            case 1:
                switch (AI_CurrentAttack)
                {
                    case AttackType.Despawn:
                        Despawn();
                        return false;
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

        AI_TeleportTimer += (float)Math.Max(NPC.Center.Y - Target.Center.Y, 0f) > 0f ? 3f : MathHelper.Lerp(1f, 1.5f, OceanNPC.LifeRatioReverse);

        ChangeScale();
        TrySpawnMinions();

        NPC.netUpdate = true;

        return false;
    }

    private void SelectNextAttack(int initialAITimer1 = 0)
    {
        AI_ChangedVelocityDirectionWhenJump = 0;
        if (AI_TeleportTimer > 1800f || NPC.WithinRange(Target.Center, 3000f))
        {
            AI_CurrentAttack = AttackType.Teleport_Phase1;
            AI_TeleportTimer = 0f;
        }
        else
        {
            AI_CurrentAttack = AI_SmallJumpCounter switch
            {
                3 => AttackType.HighJump_Phase1,
                1 or 2 when OceanNPC.LifeRatio < 0.3f => AttackType.RapidJump_Phase1,
                _ => AttackType.NormalJump_Phase1
            };
        }
        AI_CurrentAttackPhase = 0;
        AI_Timer1 = initialAITimer1;
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
            TOActivator.NewDustAction(NPC.Center, NPC.width + 25, NPC.height,
                Jewel_SapphireAlive ? DustID.GemSapphire : DustID.TintableDust, 150, new(78, 136, 255, 80), d =>
                {
                    d.noGravity = true;
                    d.scale = TOMathHelper.ClampMap(Constant.minScale[UltraIndex], Constant.maxScale[UltraIndex], 2f, 5f, NPC.scale);
                    d.velocity *= 0.5f;
                });
        }
    }

    private void Despawn()
    {
        //停止水平移动，避免奇怪的滑行现象
        StopHorizontalMovement();

        NPC.dontTakeDamage = true;
        NPC.damage = 0;

        AI_DespawnScaleMultiplier *= 0.97f;
        MakeSlimeDust((int)TOMathHelper.ClampMap(Constant.minScale[UltraIndex], Constant.maxScale[UltraIndex], 5f, 12.5f, NPC.scale));

        //体积足够小时执行脱战逻辑。
        if (NPC.scale < 0.2f)
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
            TOActivator.NewDustAction(jewel.Center, jewel.width * 3, jewel.height * 3, type, 100, default, d =>
            {
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

    private void ChangeScale() => NPC.SafeChangeScale(98, 92, MathHelper.Lerp(
            Constant.maxScale[UltraIndex],
            Constant.minScale[UltraIndex],
            OceanNPC.LifeRatioReverse) * AI_TeleportScaleMultiplier * AI_DespawnScaleMultiplier);

    private void TrySpawnMinions()
    {
        if (!TOMain.GeneralClient)
            return;

        Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);

        if (OceanNPC.LifeRatio < 0.8f && !AI_JewelEmeraldSpawned)
        {
            SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

            TOActivator.NewNPCAction<KingSlimeJewelEmerald>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
            {
                n.Ocean().Master = NPC.whoAmI;
                MakeJewelDust(n, 50);
                AI_JewelEmerald = n;
                AI_JewelEmeraldSpawned = true;
            });
        }
        if (OceanNPC.LifeRatio < 0.6f && !AI_JewelRubySpawned)
        {
            SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

            TOActivator.NewNPCAction<KingSlimeJewelRuby>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
            {
                n.Ocean().Master = NPC.whoAmI;
                MakeJewelDust(n, 50);
                AI_JewelRuby = n;
                AI_JewelRubySpawned = true;
            });
        }
        if (OceanNPC.LifeRatio < 1f / 3f && !AI_JewelSapphireSpawned)
        {
            SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

            TOActivator.NewNPCAction<KingSlimeJewelSapphire>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
            {
                n.Ocean().Master = NPC.whoAmI;
                MakeJewelDust(n, 50);
                AI_JewelSapphire = n;
                AI_JewelSapphireSpawned = true;
            });
        }
        float distance = (float)(AI_LastSpawnSlimeLife - NPC.life) / NPC.lifeMax;
        float distanceNeeded = Constant.spawnSlimeGateValue[UltraIndex];
        if (distance >= distanceNeeded)
        {
            AI_LastSpawnSlimeLife = NPC.life;
            int spawnAmount1 = Main.rand.Next(1, 3) + (int)Math.Pow(distance / distanceNeeded, Constant.spawnSlimePow[UltraIndex]);
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

                SpawnCore(spawnType);
            }

            //生成彩虹史莱姆
            for (int i = 0; i < spawnAmount2; i++)
                SpawnCore(NPCID.RainbowSlime);

            ///生成粉史莱姆
            if (Main.rand.NextBool(4))
                SpawnCore(NPCID.Pinky);

            void SpawnCore(int type)
            {
                int spawnZoneWidth = NPC.width - 32;
                int spawnZoneHeight = NPC.height - 32;
                Vector2 spawnPosition = new(NPC.position.X + Main.rand.Next(spawnZoneWidth), NPC.position.Y + Main.rand.Next(spawnZoneHeight));
                Vector2 spawnVelocity = new(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, 3f));
                TOActivator.NewNPCAction(NPC.GetSource_FromAI(), spawnPosition, type, action: n =>
                {
                    n.velocity = spawnVelocity;
                    n.ai[0] = -1000 * Main.rand.Next(3);
                    n.Ocean().Master = NPC.whoAmI;
                });
            }
        }
    }

    private void Jump()
    {
        switch (AI_CurrentAttackPhase)
        {
            case 0: //延迟
                AI_Timer2++;
                NPC.damage = 0;
                NPC.netUpdate = true;
                NPC.GravityMultiplier *= AI_Timer2 > 25 && NPC.velocity.Y > 0f ? 1.25f : 1f;
                NPC.MaxFallSpeedMultiplier *= AI_Timer2 > 20 && NPC.velocity.Y > 0f ? 1.35f : 1f;
                if (StopHorizontalMovement() && NPC.velocity.Y == 0f)
                {
                    AI_Timer1++;
                    int jumpDelay = (int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 20f : 27.5f, CAWorld.AnomalyUltramundane ? 15f : 20f, OceanNPC.LifeRatioReverse);
                    if (AI_Timer1 > jumpDelay)
                    {
                        AI_CurrentAttackPhase = 1;
                        AI_Timer1 = 0;
                    }
                }
                break;
            case 1: //起跳
                if (AI_CurrentAttack != AttackType.HighJump_Phase1)
                    AI_SmallJumpCounter++;
                NPC.damage = NPC.defDamage;
                NPC.netUpdate = true;
                NPC.velocity = GetVelocityInitial();
                AI_CurrentAttackPhase = 2;
                break;
            case 2: //上升
            case 3: //下降
                NPC.damage = NPC.defDamage;
                if (AI_CurrentAttack == AttackType.RapidJump_Phase1 || NPC.velocity.X * NPC.direction > 0.1f)
                    NPC.velocity.X = Math.Min(Math.Abs(NPC.velocity.X) + GetVelocityXDelta(), GetVelocityXLimit()) * Math.Sign(NPC.velocity.X);
                else
                {
                    NPC.velocity.X *= 0.93f;
                    switch (Math.Abs(NPC.velocity.X))
                    {
                        case < 0.1f:
                            AI_ChangedVelocityDirectionWhenJump++;
                            NPC.velocity.X += GetVelocityXDelta() * NPC.direction;
                            break;
                        case > 0.25f:
                            NPC.velocity.X -= 0.2f * Math.Sign(NPC.velocity.X);
                            break;
                    }
                }
                switch (AI_CurrentAttackPhase)
                {
                    case 2:
                        if (NPC.velocity.Y >= 0) //检测是否已过最高点
                            AI_CurrentAttackPhase = 3;
                        break;
                    case 3:
                        if (NPC.velocity.Y == 0f)
                            SelectNextAttack(AI_CurrentAttack == AttackType.RapidJump_Phase1 ? (int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 10 : 20, CAWorld.AnomalyUltramundane ? 7.5f : 12.5f, OceanNPC.LifeRatioReverse) : 0);
                        NPC.GravityMultiplier *= AI_CurrentAttack switch
                        {
                            AttackType.HighJump_Phase1 => CAWorld.AnomalyUltramundane ? 1.5f : 1.25f,
                            _ => 1f
                        };
                        NPC.MaxFallSpeedMultiplier *= AI_CurrentAttack switch
                        {
                            AttackType.HighJump_Phase1 => CAWorld.AnomalyUltramundane ? 2.25f : 1.75f,
                            _ => 1f
                        };
                        break;
                }
                break;
        }

        Vector2 GetVelocityInitial() => AI_CurrentAttack switch
        {
            AttackType.NormalJump_Phase1 => new(
                MathHelper.Lerp(7.5f, 10f, OceanNPC.LifeRatioReverse) * NPC.direction, -10f * (1f + Math.Min((float)Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 800f, 1f))),
            AttackType.HighJump_Phase1 => new(
                MathHelper.Lerp(7.5f, 10f, OceanNPC.LifeRatioReverse) * NPC.direction, -12.5f * (1f + Math.Min((float)Math.Max(NPC.Center.Y - Target.Center.Y, 0f) / 800f, 1.5f))),
            AttackType.RapidJump_Phase1 => new(
                MathHelper.Lerp(10.5f, 16f, OceanNPC.LifeRatioReverse) * NPC.direction, -8f),
            _ => Vector2.Zero
        };

        float GetVelocityXLimit() => AI_CurrentAttack switch
        {
            AttackType.RapidJump_Phase1 => 18f,
            _ => AI_ChangedVelocityDirectionWhenJump switch
            {
                0 => 12.5f,
                1 => 8f,
                _ => 6.5f
            }
        };

        float GetVelocityXDelta() => AI_CurrentAttack switch
        {
            AttackType.RapidJump_Phase1 => AI_ChangedVelocityDirectionWhenJump switch
            {
                0 => 0.8f,
                1 => 0.55f,
                _ => 0.35f
            },
            _ => AI_ChangedVelocityDirectionWhenJump switch
            {
                0 => 0.5f,
                1 => 0.4f,
                _ => 0.25f
            }
        };
    }

    private void Teleport()
    {
        NPC.damage = 0;
        switch (AI_CurrentAttackPhase)
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

                AI_TeleportDestination = destination ?? Target.Bottom;
                AI_CurrentAttackPhase = 1;
                break;
            case 1: //停止水平移动并缩小体型，满足条件时传送
                MakeSlimeDust((int)TOMathHelper.ClampMap(Constant.minScale[UltraIndex], Constant.maxScale[UltraIndex], 5f, 12.5f, NPC.scale));
                AI_TeleportScaleMultiplier -= MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 0.016f : 0.013f, CAWorld.AnomalyUltramundane ? 0.02f : 0.015f, OceanNPC.LifeRatioReverse);
                if (StopHorizontalMovement() && AI_TeleportScaleMultiplier < 0.2f)
                {
                    AI_TeleportScaleMultiplier = 0.2f;
                    NPC.Bottom = AI_TeleportDestination;
                    if (Jewel_SapphireAlive) //移动蓝宝石
                    {
                        AI_JewelSapphire.Center = NPC.Center - new Vector2(0f, 200f);
                        MakeJewelDust(AI_JewelSapphire, 20);
                    }
                    AI_CurrentAttackPhase = 2;
                }
                break;
            case 2: //恢复体型，恢复完成后开始下一次攻击
                AI_TeleportScaleMultiplier += MathHelper.Lerp(0.03f, 0.05f, OceanNPC.LifeRatioReverse);
                if (AI_TeleportScaleMultiplier > 1f)
                {
                    AI_TeleportScaleMultiplier = 1f;
                    SelectNextAttack((int)MathHelper.Lerp(CAWorld.AnomalyUltramundane ? 10 : 20, CAWorld.AnomalyUltramundane ? 5 : 12.5f, OceanNPC.LifeRatioReverse));
                }
                break;
        }
    }
    #endregion

    #region Active
    public override bool CheckActive() => false;
    #endregion
}
