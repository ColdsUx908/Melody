using System;
using CalamityAnomalies.Systems;
using CalamityMod.Events;
using CalamityMod.NPCs.NormalNPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;

namespace CalamityAnomalies.NPCs.AnomalyBosses.KingSlime;

public partial class AnomalyKingSlime
{
    public override void AnomalyAI()
    {
        #region 变量
        float lifeRatio = (float)NPC.life / NPC.lifeMax;

        bool aUltra = CAWorld.AnomalyUltramundane;
        bool bossRush = BossRushEvent.BossRushActive;
        bool masterMode = Main.masterMode;
        int index = aUltra ? 1 : 0;

        bool jewel_EmeraldSpawn = lifeRatio < 0.8f;
        bool jewel_RubySpawn = lifeRatio < 0.6f;
        bool jewel_SapphireSpawn = lifeRatio < 1f / 3f;

        bool jewel_EmeraldSpawned = AI_JewelSpawn > 0;
        bool jewel_RubySpawned = AI_JewelSpawn > 1;
        bool jewel_SapphireSpawned = AI_JewelSpawn > 2;

        TOEntityIterator<NPC> activeNPCs = TOMain.ActiveNPCs;

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
        AnomalyNPC.disableNaturalDespawning = true;
        if (!NPC.TargetClosestIfTargetIsInvalid(out Player target, true, Constant.despawnDistance))
            AI_CurrentAttack = AttackType.Despawn;
        else
            NPC.FaceTargetTO(target);
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
                        return; ;
                    case AttackType.NormalJump_Phase1:
                    case AttackType.HighJump_Phase1:
                    case AttackType.RapidJump_Phase1:
                        Jump();
                        break;
                }
                break;
        }

        TrySpawnMinions();
        return;

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

        //脱战
        void Despawn()
        {
            //停止水平移动，避免奇怪的滑行现象
            StopHorizontalMovement();

            NPC.dontTakeDamage = true;
            NPC.damage = 0;

            // Release slime dust to accompany the despawn behavior.
            for (int i = 0; i < 10; i++)
            {
                TOEntityActivator.NewDustAction(NPC.position - new Vector2(20f, 0f), NPC.width / 2 + 20, NPC.height / 2, DustID.TintableDust, 150, new(78, 136, 255, 80), spawned =>
                {
                    spawned.noGravity = true;
                    spawned.scale = 2f;
                    spawned.velocity *= 0.5f;
                });
            }

            ChangeScale(NPC.scale * 0.97f);

            // Despawn if sufficiently small. This is bypassed if the target is sufficiently far away, in which case the despawn happens immediately.
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
            TrySpawnJewelEmerald();
            TrySpawnJewelRuby();
            TrySpawnJewelSapphire();
            TrySpawnSlime();

            void TrySpawnJewelEmerald()
            {
                if (!jewel_EmeraldSpawn || jewel_EmeraldSpawned)
                    return;

                AI_JewelSpawn = 1;
                jewel_EmeraldSpawned = true;

                Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);
                DustWhenSpawnJewel(DustID.GemEmerald, spawnPosition);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                if (TOMain.GeneralClient)
                    TOEntityActivator.NewNPCAction(NPC.GetSource_FromAI(), spawnPosition, ModContent.NPCType<KingSlimeJewelEmerald>(), action: n =>
                    {
                        n.Ocean().Master = NPC.whoAmI;
                        AI_JewelEmerald = n;
                    });
            }

            void TrySpawnJewelRuby()
            {
                if (!jewel_RubySpawn || jewel_RubySpawned)
                    return;

                AI_JewelSpawn = 2;
                jewel_RubySpawned = true;

                Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);
                DustWhenSpawnJewel(DustID.GemRuby, spawnPosition);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                if (TOMain.GeneralClient)
                    TOEntityActivator.NewNPCAction(NPC.GetSource_FromAI(), spawnPosition, ModContent.NPCType<KingSlimeJewelRuby>(), action: n =>
                    {
                        n.Ocean().Master = NPC.whoAmI;
                        AI_JewelRuby = n;
                    });
            }

            void TrySpawnJewelSapphire()
            {
                if (!jewel_SapphireSpawn || jewel_SapphireSpawned)
                    return;

                AI_JewelSpawn = 3;
                jewel_SapphireSpawned = true;

                Vector2 spawnPosition = NPC.Top - new Vector2(0, NPC.height);
                DustWhenSpawnJewel(DustID.GemSapphire, spawnPosition);

                SoundEngine.PlaySound(SoundID.Item38, spawnPosition);

                if (TOMain.GeneralClient)
                    TOEntityActivator.NewNPCAction(NPC.GetSource_FromAI(), spawnPosition, ModContent.NPCType<KingSlimeJewelSapphire>(), action: n =>
                    {
                        n.Ocean().Master = NPC.whoAmI;
                        AI_JewelSapphire = n;
                    });
            }

            void DustWhenSpawnJewel(int dustID, Vector2 spawnPosition)
            {
                for (int i = 0; i < 40; i++)
                {
                    TOEntityActivator.NewDustAction(spawnPosition, NPC.width / 4, NPC.height / 4, dustID, 100, action: d =>
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
                if (!TOMain.GeneralClient)
                    return;

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
                    TOEntityActivator.NewNPCAction(NPC.GetSource_FromAI(), spawnPosition, type, action: n =>
                    {
                        n.velocity = spawnVelocity;
                        n.ai[0] = -1000 * Main.rand.Next(3);
                        n.Ocean().Master = NPC.whoAmI;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n.whoAmI);
                    });
                }
            }
        }

        void SelectNextAttack()
        {
            AI_ChangedVelocityDirectionWhenJump = 0;
            AI_CurrentAttack = AttackType.NormalJump_Phase1;
            AI_CurrentAttackPhase = 0;
            AI_CurrentAttackTimer = 0;
        }

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
                    if (AI_CurrentAttackPhase == 2 && NPC.oldVelocity.Y <= 0 && NPC.velocity.Y >= 0) //检测是否已过最高点
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
        #endregion
    }
}
