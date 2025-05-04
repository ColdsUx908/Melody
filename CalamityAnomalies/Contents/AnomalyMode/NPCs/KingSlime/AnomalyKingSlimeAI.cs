using System;
using CalamityAnomalies.Contents.AnomalyMode.NPCs;
using CalamityMod.NPCs.NormalNPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core.GameData;
using Transoceanic.Core.MathHelp;
using Transoceanic.GlobalInstances;

namespace CalamityAnomalies.GlobalInstances.AnomalyBosses.KingSlime;

public partial class AnomalyKingSlime : AnomalyNPCOverride
{
    public override void PreAI()
    {
        #region 主体

        #region 变量
        float lifeRatio = (float)NPC.life / NPC.lifeMax;
        float lifeRatioReverse = 1 - lifeRatio;

        bool aUltra = CAWorld.AnomalyUltramundane;
        bool masterMode = Main.masterMode;
        int index = aUltra ? 1 : 0;

        bool jewel_EmeraldSpawn = lifeRatio < 0.8f;
        bool jewel_RubySpawn = lifeRatio < 0.6f;
        bool jewel_SapphireSpawn = lifeRatio < 1f / 3f;

        bool jewel_EmeraldSpawned = AI_JewelSpawn > 0;
        bool jewel_RubySpawned = AI_JewelSpawn > 1;
        bool jewel_SapphireSpawned = AI_JewelSpawn > 2;

        //宝石存活状态，仅检测属于当前NPC的宝石
        bool jewel_EmeraldAlive = AI_JewelEmerald.active && AI_JewelEmerald.ModNPC is KingSlimeJewelEmerald && AI_JewelEmerald.Ocean().Master == NPC.whoAmI;
        bool jewel_RubyAlive = AI_JewelRuby.active && AI_JewelRuby.ModNPC is KingSlimeJewelRuby && AI_JewelRuby.Ocean().Master == NPC.whoAmI;
        bool jewel_SapphireAlive = AI_JewelSapphire.active && AI_JewelSapphire.ModNPC is KingSlimeJewelSapphire && AI_JewelSapphire.Ocean().Master == NPC.whoAmI;

        bool jewel_EmeraldDead = jewel_EmeraldSpawned && !jewel_EmeraldAlive;
        bool jewel_RubyDead = jewel_RubySpawned && !jewel_RubyAlive;
        bool jewel_SapphireDead = jewel_SapphireSpawned && !jewel_SapphireAlive;

        int expectedDamage = NPC.defDamage;
        #endregion

        #region 仇恨与脱战
        AnomalyNPC.DisableNaturalDespawning = true;
        if (AI_CurrentAttack == AttackType.Despawn || !NPC.TargetClosestIfTargetIsInvalid(out Player target, true, Constant.despawnDistance))
        {
            Despawn();
            return;
        }
        else
            NPC.FaceTargetTO(target);
        #endregion

        float distanceBelowTarget = Math.Max(NPC.Center.Y - target.Center.Y, 0f);
        AI_TeleportTimer += distanceBelowTarget > 0f ? 3f : MathHelper.Lerp(1f, 1.5f, lifeRatioReverse);

        float expectedScale = MathHelper.Lerp(Constant.maxScale[index], Constant.minScale[index], lifeRatioReverse) * AI_TeleportScaleMultiplier;
        ChangeScale(expectedScale);

        switch (AI_CurrentPhase)
        {
            case 0: //初始化
                if (NPC.velocity.Y == 0f)
                    AI_Timer1++;
                if (AI_Timer1 > 20)
                {
                    NPC.velocity = Vector2.Zero;
                    AI_LastSpawnSlimeLife = NPC.life;
                    AI_CurrentPhase = 1;
                    SelectNextAttack();
                }
                return;
            case 1:
                switch (AI_CurrentAttack)
                {
                    case AttackType.Despawn:
                        Despawn();
                        return;
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

        TrySpawnMinions();

        NPC.netUpdate = true;

        #endregion

        #region 行为函数
        //快速停止水平移动
        //返回值：是否已经停止
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
                TOActivator.NewDustAction(NPC.Center, NPC.width + 25, NPC.height,
                    jewel_SapphireAlive ? DustID.GemSapphire : DustID.TintableDust, 150, new(78, 136, 255, 80), d =>
                {
                    d.noGravity = true;
                    d.scale = TOMathHelper.Map(Constant.minScale[index], Constant.maxScale[index], 2f, 5f, NPC.scale);
                    d.velocity *= 0.5f;
                });
            }
        }

        void MakeJewelDust(NPC jewel, int amount)
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

        //脱战
        void Despawn()
        {
            //停止水平移动，避免奇怪的滑行现象
            StopHorizontalMovement();

            NPC.dontTakeDamage = true;
            NPC.damage = 0;

            ChangeScale(NPC.scale * 0.97f - 0.15f);
            MakeSlimeDust((int)TOMathHelper.Map(Constant.minScale[index], Constant.maxScale[index], 5f, 12.5f, NPC.scale));

            //体积足够小时执行脱战逻辑。
            if (NPC.scale < 0.2f)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }

        //更改大小
        void ChangeScale(float expectedScale)
        {
            if (expectedScale == NPC.scale)
                return;

            NPC.position.X += NPC.width / 2;
            NPC.position.Y += NPC.height;
            NPC.scale = expectedScale;
            NPC.width = (int)(98f * NPC.scale);
            NPC.height = (int)(92f * NPC.scale);
            NPC.position.X -= NPC.width / 2;
            NPC.position.Y -= NPC.height;
        }

        //召唤宝石和史莱姆
        void TrySpawnMinions()
        {
            if (!TOMain.GeneralClient)
                return;

            TrySpawnJewelEmerald();
            TrySpawnJewelRuby();
            TrySpawnJewelSapphire();
            TrySpawnSlime();

            void TrySpawnJewelEmerald()
            {
                if (!jewel_EmeraldSpawn || jewel_EmeraldSpawned)
                    return;

                Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                TOActivator.NewNPCAction<KingSlimeJewelEmerald>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
                    MakeJewelDust(n, 50);
                    AI_JewelEmerald = n;
                    AI_JewelSpawn = 1;
                    jewel_EmeraldSpawned = true;
                });
            }

            void TrySpawnJewelRuby()
            {
                if (!jewel_RubySpawn || jewel_RubySpawned)
                    return;

                Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                TOActivator.NewNPCAction<KingSlimeJewelRuby>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
                    MakeJewelDust(n, 50);
                    AI_JewelRuby = n;
                    AI_JewelSpawn = 2;
                    jewel_RubySpawned = true;
                });
            }

            void TrySpawnJewelSapphire()
            {
                if (!jewel_SapphireSpawn || jewel_SapphireSpawned)
                    return;

                Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                TOActivator.NewNPCAction<KingSlimeJewelSapphire>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
                    MakeJewelDust(n, 50);
                    AI_JewelSapphire = n;
                    AI_JewelSpawn = 3;
                    jewel_SapphireSpawned = true;
                });
            }

            void TrySpawnSlime()
            {
                float distance = (float)(AI_LastSpawnSlimeLife - NPC.life) / NPC.lifeMax;
                float distanceNeeded = Constant.spawnSlimeThreshold[index];
                if (distance < distanceNeeded)
                    return;

                AI_LastSpawnSlimeLife = NPC.life;
                int spawnAmount1 = Main.rand.Next(1, 3) + (int)Math.Pow(distance / distanceNeeded, Constant.spawnSlimePow[index]);
                int spawnAmount2 = aUltra ? Main.rand.Next(1, 2) : 0;

                for (int i = 0; i < spawnAmount1; i++)
                {
                    float minLowerLimit = i < 2 ? 0 : 5;
                    float maxLowerLimit = 7f;
                    int minTypeChoice = (int)MathHelper.Lerp(minLowerLimit, 7f, 1f - lifeRatio);
                    int maxTypeChoice = (int)MathHelper.Lerp(maxLowerLimit, 9f, 1f - lifeRatio);
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

        void SelectNextAttack(int initialAITimer1 = 0)
        {
            AI_ChangedVelocityDirectionWhenJump = 0;
            if (AI_TeleportTimer > 1800f || Vector2.Distance(NPC.Center, target.Center) > 3000f)
            {
                AI_CurrentAttack = AttackType.Teleport_Phase1;
                AI_TeleportTimer = 0f;
            }
            else
            {
                AI_CurrentAttack = AI_SmallJumpCounter switch
                {
                    3 => AttackType.HighJump_Phase1,
                    1 or 2 when lifeRatio < 0.3f => AttackType.RapidJump_Phase1,
                    _ => AttackType.NormalJump_Phase1
                };
            }
            AI_CurrentAttackPhase = 0;
            AI_Timer1 = initialAITimer1;
        }

        //跳跃
        void Jump()
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
                        int jumpDelay = (int)MathHelper.Lerp(aUltra ? 20f : 27.5f, aUltra ? 15f : 20f, lifeRatioReverse);
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
                    NPC.damage = expectedDamage;
                    NPC.netUpdate = true;
                    NPC.velocity = GetVelocityInitial();
                    AI_CurrentAttackPhase = 2;
                    break;
                case 2: //上升
                case 3: //下降
                    NPC.damage = expectedDamage;
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
                                SelectNextAttack(AI_CurrentAttack == AttackType.RapidJump_Phase1 ? (int)MathHelper.Lerp(aUltra ? 10 : 20, aUltra ? 7.5f : 12.5f, lifeRatioReverse) : 0);
                            NPC.GravityMultiplier *= AI_CurrentAttack switch
                            {
                                AttackType.HighJump_Phase1 => aUltra ? 1.5f : 1.25f,
                                _ => 1f
                            };
                            NPC.MaxFallSpeedMultiplier *= AI_CurrentAttack switch
                            {
                                AttackType.HighJump_Phase1 => aUltra ? 2.25f : 1.75f,
                                _ => 1f
                            };
                            break;
                    }
                    break;
            }

            Vector2 GetVelocityInitial() => AI_CurrentAttack switch
            {
                AttackType.NormalJump_Phase1 => new(
                    MathHelper.Lerp(7.5f, 10f, lifeRatioReverse) * NPC.direction,
                    -10f * (1f + Math.Min(distanceBelowTarget / 800f, 1f))),
                AttackType.HighJump_Phase1 => new(
                    MathHelper.Lerp(7.5f, 10f, lifeRatioReverse) * NPC.direction, -12.5f * (1f + Math.Min(distanceBelowTarget / 800f, 1.5f))),
                AttackType.RapidJump_Phase1 => new(
                    MathHelper.Lerp(10.5f, 16f, lifeRatioReverse) * NPC.direction,
                    -8f),
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
                },
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
                },
            };
        }

        //传送
        void Teleport()
        {
            NPC.damage = 0;
            switch (AI_CurrentAttackPhase)
            {
                case 0: //寻的
                    Vector2? destination = null;
                    Vector2 randomDefault = Main.rand.NextBool() ? Vector2.UnitX : -Vector2.UnitX;
                    Vector2 vectorAimedAheadOfTarget = target.Center + new Vector2((float)Math.Round(target.velocity.X / 2f), 0f).ToCustomLength(800f);
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

                    AI_TeleportDestination = destination ?? target.Bottom;
                    AI_CurrentAttackPhase = 1;
                    break;
                case 1: //停止水平移动并缩小体型，满足条件时传送
                    MakeSlimeDust((int)TOMathHelper.Map(Constant.minScale[index], Constant.maxScale[index], 5f, 12.5f, expectedScale));
                    AI_TeleportScaleMultiplier -= MathHelper.Lerp(aUltra ? 0.016f : 0.013f, aUltra ? 0.02f : 0.015f, lifeRatioReverse);
                    if (StopHorizontalMovement() && AI_TeleportScaleMultiplier < 0.2f)
                    {
                        AI_TeleportScaleMultiplier = 0.2f;
                        NPC.Bottom = AI_TeleportDestination;
                        if (jewel_SapphireAlive) //移动蓝宝石
                        {
                            AI_JewelSapphire.Center = NPC.Center - new Vector2(0f, 200f);
                            MakeJewelDust(AI_JewelSapphire, 20);
                        }
                        AI_CurrentAttackPhase = 2;
                    }
                    break;
                case 2: //恢复体型，恢复完成后开始下一次攻击
                    AI_TeleportScaleMultiplier += MathHelper.Lerp(0.03f, 0.05f, lifeRatioReverse);
                    if (AI_TeleportScaleMultiplier > 1f)
                    {
                        AI_TeleportScaleMultiplier = 1f;
                        SelectNextAttack((int)MathHelper.Lerp(aUltra ? 10 : 20, aUltra ? 5 : 12.5f, lifeRatioReverse));
                    }
                    break;
            }
        }
        #endregion
    }
}
