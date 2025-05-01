using System;
using CalamityMod.NPCs.NormalNPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core.GameData;
using Transoceanic.Core.MathHelp;

namespace CalamityAnomalies.GlobalInstances.AnomalyBosses.KingSlime;

public partial class AnomalyKingSlime
{
    public override void AnomalyAI()
    {
        #region 主体

        #region 变量
        float lifeRatio = (float)NPC.life / NPC.lifeMax;

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
        bool jewel_EmeraldAlive = AI_JewelEmerald.active && AI_JewelEmerald.type == ModContent.NPCType<KingSlimeJewelEmerald>() && AI_JewelEmerald.Ocean().Master == NPC.whoAmI;
        bool jewel_RubyAlive = AI_JewelRuby.active && AI_JewelRuby.type == ModContent.NPCType<KingSlimeJewelRuby>() && AI_JewelRuby.Ocean().Master == NPC.whoAmI;
        bool jewel_SapphireAlive = AI_JewelSapphire.active && AI_JewelSapphire.type == ModContent.NPCType<KingSlimeJewelSapphire>() && AI_JewelSapphire.Ocean().Master == NPC.whoAmI;

        bool jewel_EmeraldDead = jewel_EmeraldSpawned && !jewel_EmeraldAlive;
        bool jewel_RubyDead = jewel_RubySpawned && !jewel_RubyAlive;
        bool jewel_SapphireDead = jewel_SapphireSpawned && !jewel_SapphireAlive;

        int expectedDamage = NPC.defDamage;
        #endregion

        #region 仇恨与脱战
        Player target = null;
        AnomalyNPC.disableNaturalDespawning = true;
        if (AI_CurrentAttack != AttackType.Despawn)
        {
            if (!NPC.TargetClosestIfTargetIsInvalid(out target, true, Constant.despawnDistance))
                AI_CurrentAttack = AttackType.Despawn;
            else
                NPC.FaceTargetTO(target);
        }
        #endregion

        float expectedScale = Constant.minScale[index] + lifeRatio * (Constant.maxScale[index] - Constant.minScale[index]);
        float distanceBelowTarget = Math.Max(NPC.Center.Y - target.Center.Y, 0f);
        ChangeScale(expectedScale);

        switch (AI_CurrentPhase)
        {
            case 0: //初始化
                if (NPC.velocity.Y == 0f)
                    AI_CurrentAttackTimer++;
                if (AI_CurrentAttackTimer > 20)
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
            NPC.velocity.X *= 0.8f;
            if (Math.Abs(NPC.velocity.X) < 0.1f)
            {
                NPC.velocity.X = 0f;
                return true;
            }
            return false;
        }

        void MakeSlimeDust(int round)
        {
            for (int i = 0; i < round; i++)
            {
                TOActivator.NewDustAction(NPC.position - new Vector2(20f, 0f), NPC.width / 2 + 20, NPC.height / 2,
                    jewel_SapphireAlive ? DustID.GemSapphire : DustID.TintableDust, 150, new(78, 136, 255, 80), d =>
                {
                    d.noGravity = true;
                    d.scale = 2f;
                    d.velocity *= 0.5f;
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

            MakeSlimeDust(10);

            ChangeScale(NPC.scale * 0.97f);

            //体积足够小时执行脱战逻辑。
            if (NPC.scale < 0.7f)
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
                DustWhenSpawnJewel(DustID.GemEmerald, spawnPosition);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                TOActivator.NewNPCAction<KingSlimeJewelEmerald>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
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
                DustWhenSpawnJewel(DustID.GemRuby, spawnPosition);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                TOActivator.NewNPCAction<KingSlimeJewelRuby>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
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
                DustWhenSpawnJewel(DustID.GemSapphire, spawnPosition);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                TOActivator.NewNPCAction<KingSlimeJewelSapphire>(NPC.GetSource_FromAI(), spawnPosition, NPC.whoAmI, action: n =>
                {
                    n.Ocean().Master = NPC.whoAmI;
                    AI_JewelSapphire = n;
                    AI_JewelSpawn = 3;
                    jewel_SapphireSpawned = true;
                });
            }

            void DustWhenSpawnJewel(int dustID, Vector2 spawnPosition)
            {
                for (int i = 0; i < 40; i++)
                {
                    TOActivator.NewDustAction(spawnPosition, NPC.width / 4, NPC.height / 4, dustID, 100, action: d =>
                    {
                        d.noGravity = true;
                        d.velocity *= 2f;
                        if (Main.rand.NextBool())
                            d.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                        else
                            d.scale = 2f;
                    });
                }
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

                //void SpawnCoreMod<T>() where T : ModNPC => SpawnCore(ModContent.NPCType<T>());
            }
        }

        void SelectNextAttack()
        {
            AI_ChangedVelocityDirectionWhenJump = 0;
            AI_CurrentAttack = AttackType.NormalJump_Phase1;
            AI_CurrentAttackPhase = 0;
            AI_CurrentAttackTimer = 0;
        }

        //跳跃
        void Jump()
        {
            AI_CurrentAttackTimer++;
            switch (AI_CurrentAttackPhase)
            {
                case 0: //延迟
                    NPC.damage = 0;
                    NPC.netUpdate = true;
                    int jumpDelay = 60;
                    bool hasStopped = StopHorizontalMovement();
                    if (hasStopped && AI_CurrentAttackTimer > jumpDelay)
                        AI_CurrentAttackPhase = 1;
                    break;
                case 1: //起跳
                    NPC.damage = expectedDamage;
                    NPC.netUpdate = true;
                    NPC.velocity = GetVelocityInitial();
                    AI_CurrentAttackPhase = 2;
                    break;
                case 2: //上升
                case 3: //下降
                    NPC.damage = expectedDamage;
                    bool hasCorrectVelocity = NPC.direction switch
                    {
                        1 => NPC.velocity.X > 0.1f,
                        -1 => NPC.velocity.X < -0.1f,
                        _ => false,
                    };
                    if (hasCorrectVelocity)
                    {
                        float velocityXLimit = GetVelocityXLimit();
                        if (Math.Abs(NPC.velocity.X) < velocityXLimit)
                            NPC.velocity.X += GetVelocityXDelta() * NPC.direction;
                        else
                            NPC.velocity.X = velocityXLimit * NPC.direction;
                    }
                    else
                    {
                        NPC.velocity.X *= 0.9f;
                        if (Math.Abs(NPC.velocity.X) <= 0.1f)
                        {
                            AI_ChangedVelocityDirectionWhenJump++;
                            NPC.velocity.X -= GetVelocityXDelta() * NPC.direction;
                        }
                    }
                    if (AI_CurrentAttackPhase == 2 && NPC.velocity.Y >= 0) //检测是否已过最高点
                        AI_CurrentAttackPhase = 3;
                    if (AI_CurrentAttackPhase == 3 && NPC.velocity.Y == 0f)
                        SelectNextAttack();
                    break;
            }

            Vector2 GetVelocityInitial()
            {
                return AI_CurrentAttack switch
                {
                    AttackType.NormalJump_Phase1 => new Vector2(5f * NPC.direction, -6f * (1f + Math.Min(distanceBelowTarget / 400f, 0.8f))),
                    AttackType.HighJump_Phase1 => new Vector2(5f * NPC.direction, -10f * (1f + Math.Min(distanceBelowTarget / 400f, 1.5f))),
                    AttackType.RapidJump_Phase1 => new Vector2(12.5f * NPC.direction, 3f),
                    _ => Vector2.Zero
                };
            }

            float GetVelocityXLimit() => AI_CurrentAttack switch
            {
                AttackType.RapidJump_Phase1 => AI_ChangedVelocityDirectionWhenJump switch
                {
                    0 => 18.5f,
                    1 => 10.5f,
                    _ => 7f
                },
                _ => AI_ChangedVelocityDirectionWhenJump switch
                {
                    0 => 10f,
                    1 => 7.5f,
                    _ => 5f
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
        void Teleport(NPC npc)
        {
            AI_CurrentAttackTimer++;
            npc.damage = 0;
            switch (AI_CurrentAttackPhase)
            {
                case 0: //寻的
                    Vector2? destination = null;
                    Vector2 randomDefault = Main.rand.NextBool() ? Vector2.UnitX : -Vector2.UnitX;
                    Vector2 vectorAimedAheadOfTarget = target.Center + new Vector2((float)Math.Round(target.velocity.X / 2f), 0f).ToCustomLength(800f);
                    Point predictiveTeleportPoint = vectorAimedAheadOfTarget.ToTileCoordinates();
                    predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                    predictiveTeleportPoint.Y = Math.Clamp(predictiveTeleportPoint.Y, 10, Main.maxTilesY - 10);
                    /*
                    if (predictiveTeleportPoint.X < 10)
                        predictiveTeleportPoint.X = 10;
                    if (predictiveTeleportPoint.X > Main.maxTilesX - 10)
                        predictiveTeleportPoint.X = Main.maxTilesX - 10;
                    if (predictiveTeleportPoint.Y < 10)
                        predictiveTeleportPoint.Y = 10;
                    if (predictiveTeleportPoint.Y > Main.maxTilesY - 10)
                        predictiveTeleportPoint.Y = Main.maxTilesY - 10;
                    */
                    int randomPredictiveTeleportOffset = 5;
                    for (int i = 0; i < 100; i++)
                    {
                        int teleportTileX = Main.rand.Next(predictiveTeleportPoint.X - randomPredictiveTeleportOffset, predictiveTeleportPoint.X + randomPredictiveTeleportOffset + 1);
                        int teleportTileY = Main.rand.Next(predictiveTeleportPoint.Y - randomPredictiveTeleportOffset, predictiveTeleportPoint.Y);
                        Tile potentialTile = Main.tile[teleportTileX, teleportTileY];
                        if (!potentialTile.HasUnactuatedTile)
                        {
                            if (potentialTile.LiquidType != LiquidID.Lava
                                && Collision.CanHitLine(npc.Center, 0, 0, predictiveTeleportPoint.ToVector2() * 16, 0, 0))
                            {
                                destination = new((teleportTileX + 0.5f) * 16f, (teleportTileY + 1f) * 16f);
                                break; //在此处退出循环
                            }
                            else
                            {
                                predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                                predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                                /*
                                if (predictiveTeleportPoint.X < 10)
                                    predictiveTeleportPoint.X = 10;
                                if (predictiveTeleportPoint.X > Main.maxTilesX - 10)
                                    predictiveTeleportPoint.X = Main.maxTilesX - 10;
                                */
                            }
                        }
                        else
                        {
                            predictiveTeleportPoint.X += predictiveTeleportPoint.X < 0f ? 1 : -1;
                            predictiveTeleportPoint.X = Math.Clamp(predictiveTeleportPoint.X, 10, Main.maxTilesX - 10);
                            /*
                            if (predictiveTeleportPoint.X < 10)
                                predictiveTeleportPoint.X = 10;
                            if (predictiveTeleportPoint.X > Main.maxTilesX - 10)
                                predictiveTeleportPoint.X = Main.maxTilesX - 10;
                            */
                        }
                    }
                    AI_TeleportDestination = destination ?? target.Bottom;
                    AI_CurrentAttackPhase = 1;
                    break;
                case 1: //停止水平移动并缩小体型
                    MakeSlimeDust(30);
                    ChangeScale(npc.scale * Constant.teleportRate[index]);
                    if (StopHorizontalMovement() && AI_CurrentAttackTimer >= 15)
                        AI_CurrentAttackPhase = 2;
                    break;



            }
        }
        #endregion
    }
}
