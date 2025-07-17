using CalamityAnomalies.Publicizer.CalamityMod.NPCs;
using CalamityMod.Events;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using static CalamityMod.NPCs.Yharon.Yharon;

namespace CalamityAnomalies.Tweaks._5_1_PostDoG;

/* 犽戎
 * 
 * “巨龙重生”。
 * 在进入二阶段时释放一道无害的冲击波（GFB中有1伤害），获得15秒无敌。
 * 在转换阶段时获得97%无法削减的伤害减免（BossRush时为99%）（原灾厄：70%，BossRush时为99%）。
 */

public sealed class Yharon_Tweak : CANPCTweak<Yharon>
{
    #region 数据、属性
    public static class Data
    {
        public const float Phase2GateValue = 0.1f;

        public const int Phase2InvincibilityTime = 900;
    }

    public Yharon_Publicizer YharonPublicizer { get; private set; } = null;

#pragma warning disable IDE1006
    public Rectangle safeBox
    {
        get => YharonPublicizer.safeBox;
        set => YharonPublicizer.safeBox = value;
    }

    public bool enraged
    {
        get => YharonPublicizer.enraged;
        set => YharonPublicizer.enraged = value;
    }

    public bool protectionBoost
    {
        get => YharonPublicizer.protectionBoost;
        set => YharonPublicizer.protectionBoost = value;
    }

    public bool moveCloser
    {
        get => YharonPublicizer.moveCloser;
        set => YharonPublicizer.moveCloser = value;
    }

    public bool useTornado
    {
        get => YharonPublicizer.useTornado;
        set => YharonPublicizer.useTornado = value;
    }

    public int secondPhasePhase
    {
        get => YharonPublicizer.secondPhasePhase;
        set => YharonPublicizer.secondPhasePhase = value;
    }

    public int teleportLocation
    {
        get => YharonPublicizer.teleportLocation;
        set => YharonPublicizer.teleportLocation = value;
    }

    public bool startSecondAI
    {
        get => YharonPublicizer.startSecondAI;
        set => YharonPublicizer.startSecondAI = value;
    }

    public bool spawnArena
    {
        get => YharonPublicizer.spawnArena;
        set => YharonPublicizer.spawnArena = value;
    }

    public int invincibilityCounter
    {
        get => YharonPublicizer.invincibilityCounter;
        set => YharonPublicizer.invincibilityCounter = value;
    }

    public int fastChargeTelegraphTime
    {
        get => YharonPublicizer.fastChargeTelegraphTime;
        set => YharonPublicizer.fastChargeTelegraphTime = value;
    }
#pragma warning restore IDE1006

    public ref SlotId RoarSoundSlot => ref ModNPC.RoarSoundSlot;

    public bool Initialized
    {
        get => CalamityNPC.newAI[0] > 0f;
        set
        {
            CalamityNPC.newAI[0] = value.ToInt();
            NPC.SyncExtraAI();
        }
    }

    public int InitialLifeMax
    {
        get => (int)CalamityNPC.newAI[1];
        set
        {
            CalamityNPC.newAI[1] = value;
            NPC.SyncExtraAI();
        }
    }
    #endregion 数据、属性

    public override void Connect(NPC npc)
    {
        base.Connect(npc);
        YharonPublicizer = new(ModNPC);
    }

    #region Active
    public override bool CheckDead()
    {
        if (!startSecondAI || invincibilityCounter < Data.Phase2InvincibilityTime)
        {
            NPC.life = 1;
            NPC.active = true;
            NPC.netUpdate = true;
            return false;
        }

        return true;
    }
    #endregion Active

    #region AI

    public override bool PreAI()
    {
        if (CalamityConfig.Instance.BossesStopWeather)
            CalamityMod_.StopRain();

        if (!Initialized)
        {
            InitialLifeMax = NPC.lifeMax;
            Initialized = true;
        }

        CalamityNPC.DR = normalDR;
        CalamityNPC.unbreakableDR = false;
        CalamityNPC.CurrentlyIncreasingDefenseOrDR = false;

        if (!startSecondAI)
            AI1();
        else
            AI2();

        return false;
    }

    public void AI1()
    {
        // Variables
        bool bossRush = BossRushEvent.BossRushActive;
        bool expertMode = Main.expertMode || bossRush;
        bool revenge = CalamityWorld.revenge || bossRush;
        bool death = CalamityWorld.death || bossRush;

        CalamityGlobalNPC.yharon = NPC.whoAmI;
        CalamityGlobalNPC.yharonP2 = -1;

        int setDamage = NPC.defDamage;

        // Phase booleans
        bool phase2Check = death || OceanNPC.LifeRatio <= (revenge ? 0.8f : expertMode ? 0.7f : 0.5f);
        bool phase3Check = OceanNPC.LifeRatio <= (death ? 0.6f : revenge ? 0.5f : expertMode ? 0.4f : 0.25f);
        bool phase4Check = OceanNPC.LifeRatio <= 0.1f;
        bool phase1Change = NPC.ai[0] > -1f;
        bool phase2Change = NPC.ai[0] > 5f;
        bool phase3Change = NPC.ai[0] > 12f;

        // Timer, velocity and acceleration for idle phase before phase switch
        int phaseSwitchTimer = expertMode ? 36 : 40;
        float acceleration = expertMode ? 0.75f : 0.7f;
        float velocity = expertMode ? 12f : 11f;

        if (phase3Change)
        {
            acceleration = expertMode ? 0.85f : 0.8f;
            velocity = expertMode ? 14f : 13f;
            phaseSwitchTimer = expertMode ? 25 : 28;
            fastChargeTelegraphTime = 100;
        }
        else if (phase2Change)
        {
            acceleration = expertMode ? 0.8f : 0.75f;
            velocity = expertMode ? 13f : 12f;
            phaseSwitchTimer = expertMode ? 32 : 36;
            fastChargeTelegraphTime = 110;
        }
        else
            phaseSwitchTimer = 25;

        // Timers and velocity for charging
        float reduceSpeedChargeDistance = 540f;
        int chargeTime = expertMode ? 40 : 45;
        float chargeSpeed = expertMode ? 28f : 26f;
        float fastChargeVelocityMultiplier = bossRush ? 2f : 1.5f;
        bool playFastChargeRoarSound = NPC.localAI[1] == fastChargeTelegraphTime * 0.5f;
        bool doFastCharge = NPC.localAI[1] > fastChargeTelegraphTime;

        if (phase3Change)
        {
            chargeTime = 35;
            chargeSpeed = 30f;
        }
        else if (phase2Change)
        {
            chargeTime = expertMode ? 38 : 43;

            if (expertMode)
                chargeSpeed = 28.5f;
        }

        if (revenge)
        {
            int chargeTimeDecrease = bossRush ? 6 : death ? 4 : 2;
            float velocityMult = bossRush ? 1.15f : death ? 1.1f : 1.05f;
            phaseSwitchTimer -= chargeTimeDecrease;
            acceleration *= velocityMult;
            velocity *= velocityMult;
            chargeTime -= chargeTimeDecrease;
            chargeSpeed *= velocityMult;
        }

        float reduceSpeedFlareBombDistance = 570f;
        int flareBombPhaseTimer = bossRush ? 30 : death ? 40 : 60;
        int flareBombSpawnDivisor = flareBombPhaseTimer / 20;
        float flareBombPhaseAcceleration = bossRush ? 1f : death ? 0.92f : 0.8f;
        float flareBombPhaseVelocity = bossRush ? 16f : death ? 14f : 12f;

        int fireTornadoPhaseTimer = 90;

        int flareDustPhaseTimer = bossRush ? 160 : death ? 200 : 240;
        int flareDustPhaseTimer2 = bossRush ? 80 : death ? 100 : 120;

        float spinTime = flareDustPhaseTimer / 2;

        int flareDustSpawnDivisor = flareDustPhaseTimer / 10;
        int flareDustSpawnDivisor2 = flareDustPhaseTimer2 / 30;
        int flareDustSpawnDivisor3 = flareDustPhaseTimer / 25;

        float spinPhaseVelocity = 25f;
        float spinPhaseRotation = MathHelper.TwoPi * 3 / spinTime;

        float increasedIdleTimeAfterBulletHell = 120f;
        bool moveSlowerAfterBulletHell = NPC.ai[2] < 0f;
        if (moveSlowerAfterBulletHell)
        {
            float reducedMovementMultiplier = MathHelper.Lerp(0.1f, 1f, (NPC.ai[2] + increasedIdleTimeAfterBulletHell) / increasedIdleTimeAfterBulletHell);
            acceleration *= reducedMovementMultiplier;
            velocity *= reducedMovementMultiplier;
        }

        float teleportPhaseTimer = 30f;

        int spawnPhaseTimer = 75;

        // Target
        if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            NPC.TargetClosest();

        // Despawn safety, make sure to target another player if the current player target is too far away
        if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
            NPC.TargetClosest();

        Player player = Main.player[NPC.target];

        // Despawn
        if (player.dead || !player.active)
        {
            NPC.TargetClosest();
            player = Main.player[NPC.target];
            if (player.dead || !player.active)
            {
                NPC.velocity.Y -= 0.4f;

                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;

                if (NPC.ai[0] > 12f)
                    NPC.ai[0] = 13f;
                else if (NPC.ai[0] > 5f)
                    NPC.ai[0] = 6f;
                else
                    NPC.ai[0] = 0f;

                NPC.ai[2] = 0f;
            }
        }
        else if (NPC.timeLeft < 1800)
            NPC.timeLeft = 1800;

        int xPos = 60 * NPC.direction;
        Vector2 vector = Vector2.Normalize(player.Center - NPC.Center) * (NPC.width + 20) / 2f + NPC.Center;
        Vector2 fromMouth = new((int)vector.X + xPos, (int)vector.Y - 15);

        // Create the arena, but not as a multiplayer client.
        // In single player, the arena gets created and never gets synced because it's single player.
        // In multiplayer, only the server/host creates the arena, and everyone else receives it on the next frame via SendExtraAI.
        // Everyone however sets spawnArena to true to confirm that the fight has started.
        if (!spawnArena)
        {
            spawnArena = true;
            enraged = false;
            if (TOWorld.GeneralClient)
            {
                int safeBoxWidth = Main.zenithWorld ? 3000 : Main.getGoodWorld ? 2000 : bossRush ? 4000 : revenge ? 6000 : 7000;
                safeBox = new Rectangle
                {
                    X = (int)(player.Center.X - safeBoxWidth / 2),
                    Y = (int)Main.topWorld,
                    Width = safeBoxWidth,
                    Height = Main.maxTilesY * 16
                };
                for (int i = -1; i < 2; i += 2)
                    Projectile.NewProjectileAction<SkyFlareRevenge>(NPC.GetSource_FromAI(), player.Center + new Vector2(i * safeBoxWidth / 2, 100f), Vector2.Zero, 0, 0f, Main.myPlayer);
            }

            // Force Yharon to send a sync packet so that the arena gets sent immediately
            NPC.netUpdate = true;
        }
        // Enrage code doesn't run on frame 1 so that Yharon won't be enraged for 1 frame in multiplayer
        else
        {
            enraged = !player.Hitbox.Intersects(safeBox);
            NPC.Calamity().CurrentlyEnraged = enraged;
            if (enraged)
            {
                phaseSwitchTimer = 15;
                protectionBoost = true;
                setDamage *= 5;
                chargeSpeed += 25f;
            }
            else
                protectionBoost = false;
        }

        if (Main.getGoodWorld)
            phaseSwitchTimer /= 2;

        // Set DR based on protection boost (aka enrage)
        bool chargeTelegraph = (NPC.ai[0] == 0f || NPC.ai[0] == 6f || NPC.ai[0] == 13f) && NPC.localAI[1] > 0f;
        bool bulletHell = NPC.ai[0] is 8f or 15f;
        NPC.dontTakeDamage = bulletHell;

        bool shouldIncreaseDR = protectionBoost;
        if (phase3Change)
            shouldIncreaseDR |= phase4Check;
        else if (phase2Change)
            shouldIncreaseDR |= phase3Check;
        else if (phase1Change)
            shouldIncreaseDR |= phase2Check;
        if (shouldIncreaseDR)
            IncreaseDR();

        // Trigger spawn effects
        if (NPC.localAI[0] == 0f)
        {
            NPC.localAI[0] = 1f;
            NPC.Opacity = 0f;
            NPC.rotation = 0f;
            if (TOWorld.GeneralClient)
            {
                NPC.ai[0] = -1f;
                NPC.netUpdate = true;
            }
        }

        // Rotation
        float npcRotation = (float)Math.Atan2(player.Center.Y - NPC.Center.Y, player.Center.X - NPC.Center.X);
        if (NPC.spriteDirection == 1)
            npcRotation += MathHelper.Pi;
        if (npcRotation < 0f)
            npcRotation += MathHelper.TwoPi;
        if (npcRotation > MathHelper.TwoPi)
            npcRotation -= MathHelper.TwoPi;
        if (NPC.ai[0] is (-1f) or 3f or 4f or 9f or 10f or 16f)
            npcRotation = 0f;

        float npcRotationSpeed = 0.04f;
        if (NPC.ai[0] is 1f or 5f or 7f or 11f or 12f or 14f or 18f or 19f)
            npcRotationSpeed = 0f;
        if (NPC.ai[0] is 3f or 4f or 9f or 16f)
            npcRotationSpeed = 0.01f;

        if (npcRotationSpeed != 0f)
            NPC.rotation = NPC.rotation.AngleTowards(npcRotation, npcRotationSpeed);

        // Alpha effects
        if (NPC.ai[0] != -1f && !bulletHell && ((NPC.ai[0] != 6f && NPC.ai[0] != 13f) || NPC.ai[2] <= phaseSwitchTimer))
        {
            bool colliding = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);

            if (colliding)
                NPC.Opacity -= 0.1f;
            else
                NPC.Opacity += 0.1f;

            if (NPC.Opacity > 1f)
                NPC.Opacity = 1f;

            if (NPC.Opacity < 0.6f)
                NPC.Opacity = 0.6f;
        }

        // Spawn effects
        if (NPC.ai[0] == -1f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.velocity *= 0.98f;

            int playerFacingDirection = Math.Sign(player.Center.X - NPC.Center.X);
            if (playerFacingDirection != 0)
            {
                NPC.direction = playerFacingDirection;
                NPC.spriteDirection = -NPC.direction;
            }

            if (NPC.ai[2] > 20f)
            {
                NPC.velocity.Y = -2f;
                NPC.Opacity += 0.1f;

                bool colliding = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);

                if (colliding)
                    NPC.Opacity -= 0.1f;

                if (NPC.Opacity > 1f)
                    NPC.Opacity = 1f;

                if (NPC.Opacity < 0.6f)
                    NPC.Opacity = 0.6f;
            }

            if (NPC.ai[2] == fireTornadoPhaseTimer - 30)
            {
                int dustAmt = 72;
                for (int i = 0; i < dustAmt; i++)
                {
                    Vector2 dustRotation = Vector2.Normalize(NPC.velocity) * new Vector2(NPC.width / 2f, NPC.height) * 0.75f * 0.5f;
                    dustRotation = dustRotation.RotatedBy((i - (dustAmt / 2 - 1)) * MathHelper.TwoPi / dustAmt) + NPC.Center;
                    Vector2 dustDirection = dustRotation - NPC.Center;
                    Dust.NewDustPerfectAction(dustRotation + dustDirection, DustID.CopperCoin, d =>
                    {
                        d.velocity = dustDirection.ToCustomLength(3f);
                        d.alpha = 100;
                        d.scale = 1.4f;
                        d.noGravity = true;
                        d.noLight = true;
                    });
                }

                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= spawnPhaseTimer)
            {
                NPC.ai[0] = 0f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] = Main.rand.Next(4);
                NPC.netUpdate = true;
            }
        }

        #region Phase1
        // Phase switch
        else if (NPC.ai[0] == 0f && !player.dead)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[1] == 0f)
                NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

            Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
            Vector2 distanceFromDestination = destination - NPC.Center;
            Vector2 desiredVelocity = Vector2.Normalize(distanceFromDestination - NPC.velocity) * velocity;

            if (Vector2.Distance(NPC.Center, destination) > reduceSpeedChargeDistance)
                NPC.SimpleFlyMovement(desiredVelocity, acceleration);
            else
                NPC.velocity *= 0.98f;

            int phaseSwitchFaceDirection = Math.Sign(player.Center.X - NPC.Center.X);
            if (phaseSwitchFaceDirection != 0)
            {
                if (NPC.ai[2] == 0f && phaseSwitchFaceDirection != NPC.direction)
                    NPC.rotation += MathHelper.Pi;

                NPC.direction = phaseSwitchFaceDirection;

                if (NPC.spriteDirection != -NPC.direction)
                    NPC.rotation += MathHelper.Pi;

                NPC.spriteDirection = -NPC.direction;
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= phaseSwitchTimer)
            {
                int aiState = 0;
                switch ((int)NPC.ai[3])
                {
                    case 0:
                    case 1:
                    case 2:
                        aiState = 1;
                        break;
                    case 3:
                        aiState = 5;
                        break;
                    case 4:
                        NPC.ai[3] = 1f;
                        aiState = 2;
                        break;
                    case 5:
                        NPC.ai[3] = 0f;
                        aiState = 3;
                        break;
                }

                if (phase2Check)
                    aiState = 4;

                if (aiState == 1)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;

                    NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed;
                    NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                    if (phaseSwitchFaceDirection != 0)
                    {
                        NPC.direction = phaseSwitchFaceDirection;

                        if (NPC.spriteDirection == 1)
                            NPC.rotation += MathHelper.Pi;

                        NPC.spriteDirection = -NPC.direction;
                    }
                }
                else if (aiState == 2)
                {
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 3)
                {
                    NPC.ai[0] = 3f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 4)
                {
                    NPC.ai[0] = 4f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 5)
                {
                    if (playFastChargeRoarSound)
                        RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                    if (doFastCharge)
                    {
                        NPC.ai[0] = 5f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.localAI[1] = 0f;
                        NPC.netUpdate = true;

                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed * fastChargeVelocityMultiplier;
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                        if (phaseSwitchFaceDirection != 0)
                        {
                            NPC.direction = phaseSwitchFaceDirection;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += MathHelper.Pi;

                            NPC.spriteDirection = -NPC.direction;
                        }
                    }
                    else
                        NPC.localAI[1] += 1f;
                }
            }
        }

        // Charge
        else if (NPC.ai[0] == 1f)
        {
            // Set damage
            NPC.damage = setDamage;

            ChargeDust(7);

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= chargeTime)
            {
                NPC.ai[0] = 0f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 2f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Fireball breath
        else if (NPC.ai[0] == 2f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[1] == 0f)
                NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

            Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
            Vector2 destinationDist = destination - NPC.Center;
            Vector2 flareSpeed = Vector2.Normalize(destinationDist - NPC.velocity) * flareBombPhaseVelocity;

            if (Vector2.Distance(NPC.Center, destination) > reduceSpeedFlareBombDistance)
                NPC.SimpleFlyMovement(flareSpeed, flareBombPhaseAcceleration);
            else
                NPC.velocity *= 0.98f;

            if (NPC.ai[2] == 0f)
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

            if (NPC.ai[2] % flareBombSpawnDivisor == 0f && TOWorld.GeneralClient)
            {
                Projectile.NewProjectileAction<FlareBomb>(NPC.GetSource_FromAI(), fromMouth, Vector2.Zero, NPC.GetProjectileDamage<FlareBomb>(), 0f, Main.myPlayer, p =>
                {
                    p.ai[0] = NPC.target;
                    p.ai[1] = 1f;
                });
            }

            int playerFaceDirection = Math.Sign(player.Center.X - NPC.Center.X);
            if (playerFaceDirection != 0)
            {
                NPC.direction = playerFaceDirection;

                if (NPC.spriteDirection != -NPC.direction)
                    NPC.rotation += MathHelper.Pi;

                NPC.spriteDirection = -NPC.direction;
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= flareBombPhaseTimer)
            {
                NPC.ai[0] = 0f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Fire tornadoes
        else if (NPC.ai[0] == 3f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.velocity *= 0.98f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

            if (NPC.ai[2] == fireTornadoPhaseTimer - 30)
                SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

            if (TOWorld.GeneralClient && NPC.ai[2] == fireTornadoPhaseTimer - 30)
            {
                for (int i = -1; i < 2; i += 2)
                    Projectile.NewProjectileAction<Flare>(NPC.GetSource_FromAI(), NPC.Center, new Vector2(NPC.direction * 4 * i, 8f), 0, 0f, Main.myPlayer);
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= fireTornadoPhaseTimer)
            {
                NPC.ai[0] = 0f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Enter new phase
        else if (NPC.ai[0] == 4f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.velocity *= 0.9f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

            if (NPC.ai[2] == 180 - 60)
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= 180)
            {
                NPC.ai[0] = 6f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] = Main.rand.Next(5);
                NPC.localAI[1] = 0f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Fast charge
        else if (NPC.ai[0] == 5f)
        {
            // Set damage
            NPC.damage = setDamage;

            ChargeDust(14);

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= chargeTime)
            {
                NPC.ai[0] = 0f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 2f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }
        #endregion

        #region Phase2
        // Phase switch
        else if (NPC.ai[0] == 6f && !player.dead)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[1] == 0f)
                NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

            Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
            Vector2 distanceFromDestination = destination - NPC.Center;
            Vector2 desiredVelocity = Vector2.Normalize(distanceFromDestination - NPC.velocity) * velocity;

            if (Vector2.Distance(NPC.Center, destination) > reduceSpeedChargeDistance)
                NPC.SimpleFlyMovement(desiredVelocity, acceleration);
            else
                NPC.velocity *= 0.98f;

            int playerFaceDirectionFurtherPhases = Math.Sign(player.Center.X - NPC.Center.X);
            if (playerFaceDirectionFurtherPhases != 0)
            {
                if (NPC.ai[2] == 0f && playerFaceDirectionFurtherPhases != NPC.direction)
                    NPC.rotation += MathHelper.Pi;

                NPC.direction = playerFaceDirectionFurtherPhases;

                if (NPC.spriteDirection != -NPC.direction)
                    NPC.rotation += MathHelper.Pi;

                NPC.spriteDirection = -NPC.direction;
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= phaseSwitchTimer)
            {
                int aiState = 0;
                switch ((int)NPC.ai[3])
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        aiState = 1;
                        break;
                    case 4:
                        aiState = 5;
                        break;
                    case 5:
                        aiState = 6;
                        break;
                    case 6:
                        aiState = 2;
                        break;
                    case 7:
                        NPC.ai[3] = 0f;
                        aiState = 3;
                        break;
                }

                if (phase3Check)
                    aiState = 4;

                if (aiState == 1)
                {
                    NPC.ai[0] = 7f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;

                    NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed;
                    NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                    if (playerFaceDirectionFurtherPhases != 0)
                    {
                        NPC.direction = playerFaceDirectionFurtherPhases;

                        if (NPC.spriteDirection == 1)
                            NPC.rotation += MathHelper.Pi;

                        NPC.spriteDirection = -NPC.direction;
                    }
                }
                else if (aiState == 2)
                {
                    if (NPC.Opacity > 0f)
                    {
                        NPC.Opacity -= 0.2f;
                        if (NPC.Opacity < 0f)
                            NPC.Opacity = 0f;
                    }

                    bool spawnBulletHellVortex = NPC.ai[2] == phaseSwitchTimer + 15f;
                    if (spawnBulletHellVortex)
                    {
                        SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                        if (TOWorld.GeneralClient)
                        {
                            float bulletHellTeleportLocationDistance = 540f;
                            Vector2 defaultTeleportLocation = new(0f, -bulletHellTeleportLocationDistance);
                            Vector2 teleportLocation = player.velocity.SafeNormalize(Vector2.Zero) * -1f * bulletHellTeleportLocationDistance;
                            Vector2 center = player.Center + (teleportLocation == Vector2.Zero ? defaultTeleportLocation : teleportLocation);
                            NPC.Center = center;

                            float bulletHellVortexDuration = flareDustPhaseTimer + teleportPhaseTimer - 15f;
                            Projectile.NewProjectileAction<YharonBulletHellVortex>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Main.zenithWorld ? NPC.GetProjectileDamage<YharonBulletHellVortex>() : 0, 0f, Main.myPlayer, p =>
                            {
                                p.ai[0] = bulletHellVortexDuration + (Main.zenithWorld ? 300 : 0);
                                p.ai[1] = NPC.whoAmI;
                            });

                            // Yharon takes a small amount of damage in order to summon the bullet hell. This is to compensate for him being invulnerable during it.
                            int damageAmt = (int)(NPC.lifeMax * (bulletHellVortexDuration / CalamityNPC.KillTime));
                            NPC.life -= damageAmt;
                            if (NPC.life < 1)
                                NPC.life = 1;

                            NPC.HealEffect(-damageAmt, true);
                            NPC.netUpdate = true;
                        }
                    }

                    if (NPC.ai[2] >= phaseSwitchTimer + 15f)
                    {
                        NPC.dontTakeDamage = true;
                        NPC.velocity = Vector2.Zero;
                    }

                    if (NPC.ai[2] < phaseSwitchTimer + teleportPhaseTimer)
                        return;

                    NPC.ai[0] = 8f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 1f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 3)
                {
                    NPC.ai[0] = 9f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 4)
                {
                    NPC.ai[0] = 10f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 5)
                {
                    if (playFastChargeRoarSound)
                        RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                    if (doFastCharge)
                    {
                        NPC.ai[0] = 11f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.localAI[1] = 0f;
                        NPC.netUpdate = true;

                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed * fastChargeVelocityMultiplier;
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                        if (playerFaceDirectionFurtherPhases != 0)
                        {
                            NPC.direction = playerFaceDirectionFurtherPhases;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += MathHelper.Pi;

                            NPC.spriteDirection = -NPC.direction;
                        }
                    }
                    else
                        NPC.localAI[1] += 1f;
                }
                else if (aiState == 6)
                {
                    NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * spinPhaseVelocity;
                    NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                    if (playerFaceDirectionFurtherPhases != 0)
                    {
                        NPC.direction = playerFaceDirectionFurtherPhases;

                        if (NPC.spriteDirection == 1)
                            NPC.rotation += MathHelper.Pi;

                        NPC.spriteDirection = -NPC.direction;
                    }

                    NPC.ai[0] = 12f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
            }
        }

        // Charge
        else if (NPC.ai[0] == 7f)
        {
            // Set damage
            NPC.damage = setDamage;

            ChargeDust(7);

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= chargeTime)
            {
                NPC.ai[0] = 6f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 2f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Flare Dust bullet hell
        else if (NPC.ai[0] == 8f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[2] == 0f)
            {
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                SoundEngine.PlaySound(OrbSound, NPC.Center);
            }

            NPC.ai[2] += 1f;

            if (NPC.ai[2] % flareDustSpawnDivisor == 0f)
            {
                if (TOWorld.GeneralClient)
                {
                    int ringReduction = (int)MathHelper.Lerp(0f, 14f, NPC.ai[2] / flareDustPhaseTimer);
                    int totalProjectiles = 38 - ringReduction; // 36 for first ring, 22 for last ring
                    DoFlareDustBulletHell(0, flareDustSpawnDivisor, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), totalProjectiles, 0f, 0f, false);
                }
            }

            if (NPC.ai[2] >= flareDustPhaseTimer)
            {
                NPC.ai[0] = 6f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = -increasedIdleTimeAfterBulletHell;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Infernado
        else if (NPC.ai[0] == 9f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.velocity *= 0.98f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

            if (NPC.ai[2] == fireTornadoPhaseTimer - 30)
                SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

            if (TOWorld.GeneralClient && NPC.ai[2] == fireTornadoPhaseTimer - 30)
            {
                Projectile.NewProjectileAction<BigFlare>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 0, 0f, Main.myPlayer, p =>
                {
                    p.ai[0] = 1f;
                    p.ai[1] = NPC.target + 1;
                });
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= fireTornadoPhaseTimer)
            {
                NPC.ai[0] = 6f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Enter new phase
        else if (NPC.ai[0] == 10f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.velocity *= 0.9f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

            if (NPC.ai[2] == 180 - 60)
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= 180)
            {
                NPC.ai[0] = 13f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] = Main.rand.Next(5);
                NPC.localAI[1] = 0f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Fast charge
        else if (NPC.ai[0] == 11f)
        {
            // Set damage
            NPC.damage = setDamage;

            ChargeDust(14);

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= chargeTime)
            {
                NPC.ai[0] = 6f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 2f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Flare Dust circle
        else if (NPC.ai[0] == 12f)
        {
            // Set damage
            NPC.damage = 0;

            if (NPC.ai[2] == 0f)
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

            NPC.ai[2] += 1f;

            if (NPC.ai[2] % flareDustSpawnDivisor2 == 0f && TOWorld.GeneralClient)
            {
                Projectile.NewProjectileAction<FlareDust2>(NPC.GetSource_FromAI(), fromMouth, NPC.velocity.SafelyNormalized, NPC.GetProjectileDamage<FlareDust2>(), 0f, Main.myPlayer, p =>
                {
                    p.ai[0] = 12f;
                    p.ai[1] = 1.1f;
                });
            }

            NPC.velocity = NPC.velocity.RotatedBy(-(double)spinPhaseRotation * NPC.direction);
            NPC.rotation -= spinPhaseRotation * NPC.direction;

            if (NPC.ai[2] >= flareDustPhaseTimer2)
            {
                NPC.ai[0] = 6f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 2f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }
        #endregion

        #region Phase3
        // Phase switch
        else if (NPC.ai[0] == 13f && !player.dead)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[1] == 0f)
                NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

            Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
            Vector2 distanceFromDestination = destination - NPC.Center;
            Vector2 desiredVelocity = Vector2.Normalize(distanceFromDestination - NPC.velocity) * velocity;

            if (Vector2.Distance(NPC.Center, destination) > reduceSpeedChargeDistance)
                NPC.SimpleFlyMovement(desiredVelocity, acceleration);
            else
                NPC.velocity *= 0.98f;

            int playerFaceDirectionFurtherPhases = Math.Sign(player.Center.X - NPC.Center.X);
            if (playerFaceDirectionFurtherPhases != 0)
            {
                if (NPC.ai[2] == 0f && playerFaceDirectionFurtherPhases != NPC.direction)
                    NPC.rotation += MathHelper.Pi;

                NPC.direction = playerFaceDirectionFurtherPhases;

                if (NPC.spriteDirection != -NPC.direction)
                    NPC.rotation += MathHelper.Pi;

                NPC.spriteDirection = -NPC.direction;
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= phaseSwitchTimer)
            {
                int aiState = 0;
                switch ((int)NPC.ai[3])
                {
                    case 0:
                    case 1:
                        aiState = 1;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        aiState = 5;
                        break;
                    case 5:
                        aiState = 3;
                        break;
                    case 6:
                        aiState = 6;
                        break;
                    case 7:
                        NPC.ai[3] = 1f;
                        aiState = 7;
                        break;
                    case 8:
                        aiState = 2;
                        break;
                }

                if (phase4Check)
                    aiState = 4;

                if (aiState == 1)
                {
                    NPC.ai[0] = 14f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;

                    NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed;
                    NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                    if (playerFaceDirectionFurtherPhases != 0)
                    {
                        NPC.direction = playerFaceDirectionFurtherPhases;

                        if (NPC.spriteDirection == 1)
                            NPC.rotation += MathHelper.Pi;

                        NPC.spriteDirection = -NPC.direction;
                    }
                }
                else if (aiState == 2)
                {
                    if (NPC.Opacity > 0f)
                    {
                        NPC.Opacity -= 0.2f;
                        if (NPC.Opacity < 0f)
                            NPC.Opacity = 0f;
                    }

                    bool spawnBulletHellVortex = NPC.ai[2] == phaseSwitchTimer + 15f;
                    if (spawnBulletHellVortex)
                    {
                        SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                        if (TOWorld.GeneralClient)
                        {
                            float bulletHellTeleportLocationDistance = 540f;
                            Vector2 defaultTeleportLocation = new(0f, -bulletHellTeleportLocationDistance);
                            Vector2 teleportLocation = player.velocity.SafeNormalize(Vector2.Zero) * -1f * bulletHellTeleportLocationDistance;
                            Vector2 center = player.Center + (teleportLocation == Vector2.Zero ? defaultTeleportLocation : teleportLocation);
                            NPC.Center = center;

                            int type = ModContent.ProjectileType<YharonBulletHellVortex>();
                            int damage = Main.zenithWorld ? NPC.GetProjectileDamage(type) : 0;
                            float bulletHellVortexDuration = flareDustPhaseTimer + teleportPhaseTimer - 15f;
                            Projectile.NewProjectileAction<YharonBulletHellVortex>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Main.zenithWorld ? NPC.GetProjectileDamage<YharonBulletHellVortex>() : 0, 0f, Main.myPlayer, p =>
                            {
                                p.ai[0] = bulletHellVortexDuration + (Main.zenithWorld ? 300 : 0);
                                p.ai[1] = NPC.whoAmI;
                            });

                            // Yharon takes a small amount of damage in order to summon the bullet hell. This is to compensate for him being invulnerable during it.
                            int damageAmt = (int)(NPC.lifeMax * (bulletHellVortexDuration / CalamityNPC.KillTime));
                            NPC.life -= damageAmt;
                            if (NPC.life < 1)
                                NPC.life = 1;

                            NPC.HealEffect(-damageAmt, true);
                            NPC.netUpdate = true;
                        }
                    }

                    if (NPC.ai[2] >= phaseSwitchTimer + 15f)
                    {
                        NPC.dontTakeDamage = true;
                        NPC.velocity = Vector2.Zero;
                    }

                    if (NPC.ai[2] < phaseSwitchTimer + teleportPhaseTimer)
                        return;

                    NPC.ai[0] = 15f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 3)
                {
                    NPC.ai[0] = 16f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 4)
                {
                    NPC.ai[0] = 17f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 5)
                {
                    if (playFastChargeRoarSound)
                        RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                    if (doFastCharge)
                    {
                        NPC.ai[0] = 18f;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.localAI[1] = 0f;
                        NPC.netUpdate = true;

                        NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * chargeSpeed * fastChargeVelocityMultiplier;
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                        if (playerFaceDirectionFurtherPhases != 0)
                        {
                            NPC.direction = playerFaceDirectionFurtherPhases;

                            if (NPC.spriteDirection == 1)
                                NPC.rotation += MathHelper.Pi;

                            NPC.spriteDirection = -NPC.direction;
                        }
                    }
                    else
                        NPC.localAI[1] += 1f;
                }
                else if (aiState == 6)
                {
                    NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * spinPhaseVelocity;
                    NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X);

                    if (playerFaceDirectionFurtherPhases != 0)
                    {
                        NPC.direction = playerFaceDirectionFurtherPhases;

                        if (NPC.spriteDirection == 1)
                            NPC.rotation += MathHelper.Pi;

                        NPC.spriteDirection = -NPC.direction;
                    }

                    NPC.ai[0] = 19f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
                else if (aiState == 7)
                {
                    NPC.ai[0] = 20f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
            }
        }

        // Charge
        else if (NPC.ai[0] == 14f)
        {
            // Set damage
            NPC.damage = setDamage;

            ChargeDust(7);

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= chargeTime)
            {
                NPC.ai[0] = 13f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 2f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Flare Dust bullet hell
        else if (NPC.ai[0] == 15f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[2] == 0f)
            {
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                SoundEngine.PlaySound(OrbSound, NPC.Center);
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] % flareDustSpawnDivisor3 == 0f)
            {
                // Rotate spiral by 7.2 * (300 / 12) = +90 degrees and then back -90 degrees

                if (TOWorld.GeneralClient)
                {
                    DoFlareDustBulletHell(1, flareDustPhaseTimer, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), 8, 12f, 3.6f, false);
                }
            }

            if (NPC.ai[2] >= flareDustPhaseTimer)
            {
                NPC.ai[0] = 13f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = -increasedIdleTimeAfterBulletHell;
                NPC.localAI[2] = 0f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Infernado
        else if (NPC.ai[0] == 16f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.velocity *= 0.98f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

            if (NPC.ai[2] == fireTornadoPhaseTimer - 30)
                SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

            if (TOWorld.GeneralClient && NPC.ai[2] == fireTornadoPhaseTimer - 30)
            {
                Projectile.NewProjectileAction<BigFlare>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 0, 0f, Main.myPlayer, p =>
                {
                    p.ai[0] = 1f;
                    p.ai[1] = NPC.target + 1;
                });
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= fireTornadoPhaseTimer)
            {
                NPC.ai[0] = 13f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 3f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Enter new phase
        else if (NPC.ai[0] == 17f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.velocity *= 0.9f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);
            NPC.ai[2] += 1f;

            switch (NPC.ai[2])
            {
                case 120f:
                    RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                    break;
                case 130f:
                    if (TOWorld.GeneralClient)
                    {
                        Projectile.NewProjectileAction<ResplendentExplosion>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Main.zenithWorld.ToInt(), 0f, Main.myPlayer, p =>
                        {
                            p.ai[1] = 1200f;
                            p.localAI[1] = 0.25f;
                            p.Opacity = 0.8f + Main.rand.NextFloat(-0.05f, 0.05f);
                            p.netUpdate = true;
                        });
                    }
                    break;
                case >= 180f:
                    startSecondAI = true;
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.localAI[1] = 0f;
                    NPC.TargetClosest();
                    NPC.netUpdate = true;
                    break;
            }
        }

        // Fast charge
        else if (NPC.ai[0] == 18f)
        {
            // Set damage
            NPC.damage = setDamage;

            ChargeDust(14);

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= chargeTime)
            {
                NPC.ai[0] = 13f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 2f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Fireball ring
        else if (NPC.ai[0] == 19f)
        {
            // Set damage
            NPC.damage = 0;

            if (NPC.ai[2] == 0f)
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

            NPC.ai[2] += 1f;

            if (NPC.ai[2] % flareDustSpawnDivisor2 == 0f && TOWorld.GeneralClient)
            {
                Projectile.NewProjectileAction<FlareDust2>(NPC.GetSource_FromAI(), fromMouth, NPC.velocity.SafelyNormalized, NPC.GetProjectileDamage<FlareDust2>(), 0f, Main.myPlayer, p =>
                {
                    p.ai[0] = 15f;
                    p.ai[1] = 1.11f;
                });
            }

            NPC.velocity = NPC.velocity.RotatedBy(-(double)spinPhaseRotation * NPC.direction);
            NPC.rotation -= spinPhaseRotation * NPC.direction;

            if (NPC.ai[2] >= flareDustPhaseTimer2 - 50)
            {
                NPC.ai[0] = 13f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 1f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        // Fireball breath
        else if (NPC.ai[0] == 20f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[1] == 0f)
                NPC.ai[1] = Math.Sign((NPC.Center - player.Center).X);

            Vector2 destination = player.Center + new Vector2(NPC.ai[1], 0);
            Vector2 destinationDist = destination - NPC.Center;
            Vector2 flareSpeed = Vector2.Normalize(destinationDist - NPC.velocity) * flareBombPhaseVelocity;

            if (Vector2.Distance(NPC.Center, destination) > reduceSpeedFlareBombDistance)
                NPC.SimpleFlyMovement(flareSpeed, flareBombPhaseAcceleration);
            else
                NPC.velocity *= 0.98f;

            if (NPC.ai[2] == 0f)
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

            if (NPC.ai[2] % flareBombSpawnDivisor == 0f && TOWorld.GeneralClient)
            {
                Projectile.NewProjectileAction<FlareBomb>(NPC.GetSource_FromAI(), fromMouth, Vector2.Zero, NPC.GetProjectileDamage<FlareBomb>(), 0f, Main.myPlayer, p =>
                {
                    p.ai[0] = NPC.target;
                    p.ai[1] = 1f;
                });
            }

            int playerFaceDirection = Math.Sign(player.Center.X - NPC.Center.X);
            if (playerFaceDirection != 0)
            {
                NPC.direction = playerFaceDirection;

                if (NPC.spriteDirection != -NPC.direction)
                    NPC.rotation += MathHelper.Pi;

                NPC.spriteDirection = -NPC.direction;
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= flareBombPhaseTimer - 15)
            {
                NPC.ai[0] = 13f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }
        #endregion
    }

    public void AI2()
    {
        // Variables
        bool bossRush = BossRushEvent.BossRushActive;
        bool expertMode = Main.expertMode || bossRush;
        bool revenge = CalamityWorld.revenge || bossRush;
        bool death = CalamityWorld.death || bossRush;

        CalamityGlobalNPC.yharon = NPC.whoAmI;
        CalamityGlobalNPC.yharonP2 = NPC.whoAmI;

        int setDamage = NPC.defDamage;

        CalamityGlobalNPC.yharonP2 = NPC.whoAmI;

        bool phase2 = death || OceanNPC.LifeRatio <= (revenge ? 0.8f : expertMode ? 0.7f : 0.5f);
        bool phase3 = OceanNPC.LifeRatio <= (death ? 0.65f : revenge ? 0.5f : expertMode ? 0.4f : 0.125f);
        bool phase4 = OceanNPC.LifeRatio <= (death ? 0.3f : revenge ? 0.2f : 0f);

        if (NPC.ai[0] is not 5f and not 8f)
        {
            NPC.Opacity += 0.1f;
            if (NPC.Opacity > 1f)
                NPC.Opacity = 1f;
        }

        if (!moveCloser)
        {
            moveCloser = true;

            string key = "Mods.CalamityMod.Status.Boss.FlameText";
            Color messageColor = Color.Orange;

            CalamityUtils.DisplayLocalizedText(key, messageColor);
        }

        NPC.dontTakeDamage = false;

        // 巨 龙 重 生
        invincibilityCounter++;
        bool invincible = invincibilityCounter <= Data.Phase2InvincibilityTime;
        if (invincible)
        {
            float invincibleRatio = (float)invincibilityCounter / Data.Phase2InvincibilityTime;
            int newLifeMax = InitialLifeMax + (int)((Main.zenithWorld ? InitialLifeMax : InitialLifeMax / 10.0) * invincibleRatio); //GFB中会重生至二倍生命值
            int increasedLifeMax = newLifeMax - NPC.lifeMax;
            if (increasedLifeMax > 0)
                NPC.lifeMax += increasedLifeMax;

            //对数插值: y = ln((e - 1)x / 900 + 1)
            int newLife = (int)MathHelper.Lerp(NPC.life, NPC.lifeMax * MathHelper.Lerp(0.1f, 1f, invincibleRatio), MathF.Log((MathF.E - 1) * invincibleRatio + 1));
            int increasedLife = Math.Clamp(newLife - NPC.life, 0, NPC.lifeMax - NPC.life);

            if (increasedLife > 0)
            {
                NPC.life += increasedLife;
                NPC.HealEffect(increasedLife, true);
                NPC.netUpdate = true;
            }

            if (NPC.life > NPC.lifeMax)
                NPC.life = NPC.lifeMax;

            NPC.dontTakeDamage = true;
            phase2 = phase3 = phase4 = false;
        }

        // Acquire target and determine enrage state
        if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
        {
            NPC.TargetClosest();
            NPC.netUpdate = true;
        }

        // Despawn safety, make sure to target another player if the current player target is too far away
        if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
        {
            NPC.TargetClosest();
            NPC.netUpdate = true;
        }

        Player targetData = Main.player[NPC.target];

        // Despawn
        bool targetDead = false;
        if (targetData.dead || !targetData.active)
        {
            NPC.TargetClosest();
            NPC.netUpdate = true;
            targetData = Main.player[NPC.target];
            if (targetData.dead || !targetData.active)
            {
                targetDead = true;

                NPC.velocity.Y -= 0.4f;

                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;

                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
            }
        }
        else if (NPC.timeLeft < 1800)
            NPC.timeLeft = 1800;

        enraged = !targetData.Hitbox.Intersects(safeBox);
        if (enraged)
        {
            protectionBoost = true;
            setDamage *= 5;
        }
        else
            protectionBoost = false;

        // Set DR based on protection boost (aka enrage)
        bool bulletHell = NPC.ai[0] == 5f;
        NPC.dontTakeDamage = bulletHell;

        if (protectionBoost || (secondPhasePhase == 1 && phase2) || (secondPhasePhase == 2 && phase3) || (secondPhasePhase == 3 && phase4) || invincible || NPC.ai[0] == 9f)
            IncreaseDR();

        float reduceSpeedChargeDistance = 500f;
        float reduceSpeedFireballSpitChargeDistance = 800f;
        float phaseSwitchTimer = bossRush ? 28f : expertMode ? 30f : 32f;
        float acceleration = expertMode ? 0.92f : 0.9f;
        float velocity = expertMode ? 14.5f : 14f;
        float chargeTime = expertMode ? 32f : 35f;
        float chargeSpeed = expertMode ? 32f : 30f;

        float fastChargeVelocityMultiplier = bossRush ? 2f : 1.5f;
        fastChargeTelegraphTime = protectionBoost ? 60 : (100 - secondPhasePhase * 10);
        bool playFastChargeRoarSound = NPC.localAI[1] == fastChargeTelegraphTime * 0.5f;
        bool doFastChargeTelegraph = NPC.localAI[1] <= fastChargeTelegraphTime;

        float fireballBreathTimer = 60f;
        float fireballBreathPhaseTimer = fireballBreathTimer + 80f;
        float fireballBreathPhaseVelocity = expertMode ? 32f : 30f;

        float splittingFireballBreathTimer = 40f;
        float splittingFireballBreathPhaseVelocity = 22f;
        int splittingFireballBreathDivisor = 10;
        int splittingFireballs = 10;
        int splittingFireballBreathTimer2 = splittingFireballs * splittingFireballBreathDivisor;
        float splittingFireballBreathYVelocityTimer = 40f;
        float splittingFireballBreathPhaseTimer = splittingFireballBreathTimer + splittingFireballBreathTimer2 + splittingFireballBreathYVelocityTimer;

        int spinPhaseTimer = secondPhasePhase == 4 ? (bossRush ? 80 : death ? 100 : 120) : (bossRush ? 120 : death ? 150 : 180);
        int flareDustSpawnDivisor = spinPhaseTimer / 10;
        int flareDustSpawnDivisor2 = spinPhaseTimer / 20 + (secondPhasePhase == 4 ? spinPhaseTimer / 60 : 0);

        float increasedIdleTimeAfterBulletHell = 120f;
        bool moveSlowerAfterBulletHell = NPC.ai[1] < 0f;
        if (moveSlowerAfterBulletHell)
        {
            float reducedMovementMultiplier = MathHelper.Lerp(0.1f, 1f, (NPC.ai[1] + increasedIdleTimeAfterBulletHell) / increasedIdleTimeAfterBulletHell);
            acceleration *= reducedMovementMultiplier;
            velocity *= reducedMovementMultiplier;
        }

        float flareSpawnDecelerationTimer = bossRush ? 60f : death ? 75f : 90f;
        int flareSpawnPhaseTimerReduction = revenge ? (int)(flareSpawnDecelerationTimer * (1f - (float)OceanNPC.LifeRatio) * 0.55f) : 0;
        float flareSpawnPhaseTimer = (bossRush ? 120f : death ? 150f : 180f) - flareSpawnPhaseTimerReduction;

        float teleportPhaseTimer = 45f;

        if (revenge)
        {
            float chargeTimeDecrease = bossRush ? 6f : death ? 4f : 2f;
            float velocityMult = bossRush ? 1.15f : death ? 1.1f : 1.05f;
            acceleration *= velocityMult;
            velocity *= velocityMult;
            chargeTime -= chargeTimeDecrease;
            chargeSpeed *= velocityMult;
        }

        if (Main.getGoodWorld)
            phaseSwitchTimer *= 0.5f;

        if (NPC.ai[0] == 0f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.ai[1] += 1f;
            if (NPC.ai[1] >= 10f)
            {
                NPC.ai[1] = 0f;
                NPC.ai[0] = 1f;
                NPC.ai[2] = 0f;
                NPC.netUpdate = true;
            }
        }
        else if (NPC.ai[0] == 1f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[2] == 0f)
                NPC.ai[2] = (NPC.Center.X < targetData.Center.X) ? 1 : -1;

            Vector2 destination = targetData.Center + new Vector2(-NPC.ai[2], 0f);
            Vector2 desiredVelocity = NPC.SafeDirectionTo(destination) * velocity;

            if (!targetDead)
            {
                if (Vector2.Distance(NPC.Center, destination) > reduceSpeedChargeDistance)
                    NPC.SimpleFlyMovement(desiredVelocity, acceleration);
                else
                    NPC.velocity *= 0.98f;
            }

            int spriteDirection = (NPC.Center.X < targetData.Center.X) ? 1 : -1;
            NPC.direction = NPC.spriteDirection = spriteDirection;

            NPC.ai[1] += 1f;
            if (NPC.ai[1] >= phaseSwitchTimer)
            {
                int phase2AttackType = 1;
                if (phase4)
                {
                    switch ((int)NPC.ai[3])
                    {
                        case 0:
                            phase2AttackType = 8; // Teleport
                            break;
                        case 1:
                        case 2:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 3:
                            phase2AttackType = 5; // Fire circle + tornado (only once) + fireballs
                            break;
                    }
                }
                else if (phase3)
                {
                    switch ((int)NPC.ai[3])
                    {
                        case 0:
                            phase2AttackType = 6; // Tornado
                            break;
                        case 1:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 2:
                            phase2AttackType = 8; // Teleport
                            break;
                        case 3:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 4:
                            phase2AttackType = 5; // Fire circle
                            break;
                        case 5:
                            phase2AttackType = Main.rand.NextBool() ? 3 : 4; // Fireballs
                            break;
                        case 6:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 7:
                            phase2AttackType = 8; // Teleport
                            break;
                        case 8:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 9:
                            phase2AttackType = Main.rand.NextBool() ? 4 : 3; // Fireballs
                            break;
                        case 10:
                            phase2AttackType = 6; // Tornado
                            break;
                        case 11:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 12:
                            phase2AttackType = 8; // Teleport
                            break;
                        case 13:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 14:
                            phase2AttackType = 5; // Fire circle
                            break;
                        case 15:
                            phase2AttackType = Main.rand.NextBool() ? 3 : 4; // Fireballs
                            break;
                    }
                }
                else if (phase2)
                {
                    switch ((int)NPC.ai[3])
                    {
                        case 0:
                            phase2AttackType = 6; // Tornado
                            break;
                        case 1:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 2:
                            phase2AttackType = 2; // Charge
                            break;
                        case 3:
                            phase2AttackType = 5; // Fire circle
                            break;
                        case 4:
                            phase2AttackType = Main.rand.NextBool() ? 3 : 4; // Fireballs
                            break;
                        case 5:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 6:
                            phase2AttackType = 2; // Charge
                            break;
                        case 7:
                            phase2AttackType = Main.rand.NextBool() ? 4 : 3; // Fireballs
                            break;
                        case 8:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 9:
                            phase2AttackType = 2; // Charge
                            break;
                        case 10:
                            phase2AttackType = 5; // Fire circle
                            break;
                    }
                }
                else
                {
                    switch ((int)NPC.ai[3])
                    {
                        case 0:
                            phase2AttackType = 6; // Tornado
                            break;
                        case 1:
                        case 2:
                            phase2AttackType = 2; // Charge
                            break;
                        case 3:
                            phase2AttackType = Main.rand.NextBool() ? 3 : 4; // Fireballs
                            break;
                        case 4:
                        case 5:
                            phase2AttackType = 7; // Fast charge
                            break;
                        case 6:
                            phase2AttackType = Main.rand.NextBool() ? 4 : 3; // Fireballs
                            break;
                        case 7:
                        case 8:
                            phase2AttackType = 2; // Charge
                            break;
                        case 9:
                            phase2AttackType = 5; // Fire circle
                            break;
                    }
                }

                if (phase2AttackType == 5 && NPC.ai[1] < phaseSwitchTimer + teleportPhaseTimer)
                {
                    float newRotation = NPC.AngleTo(targetData.Center);
                    float amount = 0.04f;

                    if (NPC.spriteDirection == -1)
                        newRotation += MathHelper.Pi;

                    if (amount != 0f)
                        NPC.rotation = NPC.rotation.AngleTowards(newRotation, amount);

                    if (NPC.Opacity > 0f)
                    {
                        NPC.Opacity -= 0.2f;
                        if (NPC.Opacity < 0f)
                            NPC.Opacity = 0f;
                    }

                    float timeBeforeTeleport = teleportPhaseTimer - 15f;
                    bool spawnBulletHellVortex = NPC.ai[1] == phaseSwitchTimer + timeBeforeTeleport;
                    if (spawnBulletHellVortex)
                    {
                        SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                        if (TOWorld.GeneralClient)
                        {
                            if (CalamityWorld.LegendaryMode && revenge && !NPC.AnyNPCs<Bumblefuck>())
                                NPC.SpawnOnPlayer<Bumblefuck>(NPC.FindClosestPlayer());

                            float bulletHellTeleportLocationDistance = 540f;
                            Vector2 defaultTeleportLocation = new(0f, -bulletHellTeleportLocationDistance);
                            Vector2 teleportLocation = targetData.velocity.SafeNormalize(Vector2.Zero) * -1f * bulletHellTeleportLocationDistance;
                            Vector2 center = targetData.Center + (teleportLocation == Vector2.Zero ? defaultTeleportLocation : teleportLocation);
                            NPC.Center = center;

                            float bulletHellVortexDuration = spinPhaseTimer + 15f;
                            Projectile.NewProjectileAction<YharonBulletHellVortex>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, Main.zenithWorld ? NPC.GetProjectileDamage<YharonBulletHellVortex>() : 0, 0f, Main.myPlayer, p =>
                            {
                                p.ai[0] = bulletHellVortexDuration + (Main.zenithWorld ? 300 : 0);
                                p.ai[1] = NPC.whoAmI;
                            });

                            // Yharon takes a small amount of damage in order to summon the bullet hell. This is to compensate for him being invulnerable during it.
                            int damageAmt = (int)(NPC.lifeMax * (bulletHellVortexDuration / CalamityNPC.KillTime));
                            NPC.life -= damageAmt;
                            if (NPC.life < 1)
                                NPC.life = 1;

                            NPC.HealEffect(-damageAmt, true);
                            NPC.netUpdate = true;
                        }
                    }

                    if (NPC.ai[1] >= phaseSwitchTimer + timeBeforeTeleport)
                    {
                        NPC.dontTakeDamage = true;
                        NPC.velocity = Vector2.Zero;
                    }

                    return;
                }

                if (phase2AttackType == 7 && doFastChargeTelegraph)
                {
                    float newRotation = NPC.AngleTo(targetData.Center);
                    float amount = 0.04f;

                    if (NPC.spriteDirection == -1)
                        newRotation += MathHelper.Pi;

                    if (amount != 0f)
                        NPC.rotation = NPC.rotation.AngleTowards(newRotation, amount);

                    if (playFastChargeRoarSound)
                        RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);

                    NPC.localAI[1] += 1f;

                    return;
                }

                NPC.ai[0] = phase2AttackType;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] += 1f;
                NPC.localAI[1] = 0f;

                switch (secondPhasePhase)
                {
                    case 1:
                        if (phase2)
                        {
                            secondPhasePhase = 2;
                            NPC.ai[0] = 9f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.ai[3] = Main.rand.Next(11);
                        }
                        break;

                    case 2:
                        if (phase3)
                        {
                            secondPhasePhase = 3;
                            NPC.ai[0] = 9f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.ai[3] = Main.rand.Next(16);
                        }
                        break;

                    case 3:
                        if (phase4)
                        {
                            secondPhasePhase = 4;
                            NPC.ai[0] = 9f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.ai[3] = 0f;
                        }
                        break;
                }

                NPC.netUpdate = true;

                float aiLimit = 10f;
                if (phase4)
                    aiLimit = 4f;
                else if (phase3)
                    aiLimit = 16f;
                else if (phase2)
                    aiLimit = 11f;

                if (NPC.ai[3] >= aiLimit)
                    NPC.ai[3] = 0f;

                switch (phase2AttackType)
                {
                    case 2: // Charge
                        {
                            Vector2 vector = NPC.SafeDirectionTo(targetData.Center, Vector2.UnitX * NPC.spriteDirection);
                            NPC.spriteDirection = (vector.X > 0f) ? 1 : -1;
                            NPC.rotation = vector.ToRotation();

                            if (NPC.spriteDirection == -1)
                                NPC.rotation += MathHelper.Pi;

                            NPC.velocity = vector * chargeSpeed;

                            break;
                        }

                    case 3: // Fireballs
                        {
                            Vector2 fireSpitFaceDirection = new((targetData.Center.X > NPC.Center.X) ? 1 : -1, 0f);
                            NPC.spriteDirection = (fireSpitFaceDirection.X > 0f) ? 1 : -1;
                            NPC.velocity = fireSpitFaceDirection * -2f;

                            break;
                        }

                    case 5: // Spin move
                        {
                            NPC.dontTakeDamage = true;
                            NPC.localAI[3] = Main.rand.Next(2);
                            NPC.velocity = Vector2.Zero;

                            break;
                        }

                    case 7: // Fast charge
                        {
                            Vector2 vector = NPC.SafeDirectionTo(targetData.Center, Vector2.UnitX * NPC.spriteDirection);
                            NPC.spriteDirection = (vector.X > 0f) ? 1 : -1;
                            NPC.rotation = vector.ToRotation();

                            if (NPC.spriteDirection == -1)
                                NPC.rotation += MathHelper.Pi;

                            NPC.velocity = vector * chargeSpeed * fastChargeVelocityMultiplier;

                            break;
                        }
                }
            }
        }

        // Charge
        else if (NPC.ai[0] == 2f)
        {
            // Set damage
            NPC.damage = setDamage;

            if (NPC.ai[1] == 1f)
                SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

            ChargeDust(7);

            NPC.ai[1] += 1f;
            if (NPC.ai[1] >= chargeTime)
            {
                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.TargetClosest();
            }
        }

        // Fireball spit charge
        else if (NPC.ai[0] == 3f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            int fireballFaceDirection = (NPC.Center.X < targetData.Center.X) ? 1 : -1;

            NPC.ai[1] += 1f;
            if (NPC.ai[1] < fireballBreathTimer)
            {
                Vector2 destination = targetData.Center + new Vector2(fireballFaceDirection, 0);
                Vector2 distanceFromDestination = destination - NPC.Center;
                Vector2 desiredVelocity = Vector2.Normalize(distanceFromDestination - NPC.velocity) * velocity;

                if (!targetDead)
                {
                    if (Vector2.Distance(NPC.Center, destination) > reduceSpeedFireballSpitChargeDistance)
                        NPC.SimpleFlyMovement(desiredVelocity, acceleration);
                    else
                        NPC.velocity *= 0.98f;
                }

                NPC.direction = NPC.spriteDirection = fireballFaceDirection;

                if (Vector2.Distance(destination, NPC.Center) < 32f)
                    NPC.ai[1] = fireballBreathTimer - 1f;
            }

            if (NPC.ai[1] == fireballBreathTimer)
            {
                Vector2 vector = NPC.SafeDirectionTo(targetData.Center, Vector2.UnitX * NPC.spriteDirection);
                NPC.spriteDirection = (vector.X > 0f) ? 1 : -1;
                NPC.rotation = vector.ToRotation();

                if (NPC.spriteDirection == -1)
                    NPC.rotation += MathHelper.Pi;

                NPC.velocity = vector * fireballBreathPhaseVelocity;

                SoundEngine.PlaySound(FireSound, NPC.Center);
            }

            if (NPC.ai[1] >= fireballBreathTimer)
            {
                if (NPC.ai[1] % (expertMode ? 6f : 8f) == 0f && TOWorld.GeneralClient)
                    Projectile.NewProjectileAction<FlareDust2>(NPC.GetSource_FromAI(), NPC.Center + new Vector2(140f * NPC.direction, -20f).RotatedBy(NPC.rotation), NPC.velocity.SafelyNormalized, NPC.GetProjectileDamage<FlareDust2>(), 0f, Main.myPlayer);

                if (Math.Abs(targetData.Center.X - NPC.Center.X) > 700f && Math.Abs(NPC.velocity.X) < chargeSpeed)
                    NPC.velocity.X += Math.Sign(NPC.velocity.X) * 0.5f;
            }

            if (NPC.ai[1] >= fireballBreathPhaseTimer)
            {
                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
                NPC.TargetClosest();
            }
        }

        // Splitting fireball breath
        else if (NPC.ai[0] == 4f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            int splitFireFaceDirection = (NPC.Center.X < targetData.Center.X) ? 1 : -1;
            NPC.ai[2] = splitFireFaceDirection;

            if (NPC.ai[1] < splittingFireballBreathTimer)
            {
                Vector2 splitFireDestination = targetData.Center + new Vector2(splitFireFaceDirection * -750f, -300f);
                Vector2 splitFireFinalVelocity = NPC.SafeDirectionTo(splitFireDestination) * splittingFireballBreathPhaseVelocity;

                NPC.velocity = Vector2.Lerp(NPC.velocity, splitFireFinalVelocity, 0.0333333351f);

                int direction = (NPC.Center.X < targetData.Center.X) ? 1 : -1;
                NPC.direction = NPC.spriteDirection = direction;

                if (Vector2.Distance(splitFireDestination, NPC.Center) < 32f)
                    NPC.ai[1] = splittingFireballBreathTimer - 1f;
            }
            else if (NPC.ai[1] == splittingFireballBreathTimer)
            {
                Vector2 yharonFireballMoveDirection = NPC.SafeDirectionTo(targetData.Center, Vector2.UnitX * NPC.spriteDirection);
                yharonFireballMoveDirection.Y *= 0.15f;
                yharonFireballMoveDirection = yharonFireballMoveDirection.SafeNormalize(Vector2.UnitX * NPC.direction);

                NPC.spriteDirection = (yharonFireballMoveDirection.X > 0f) ? 1 : -1;
                NPC.rotation = yharonFireballMoveDirection.ToRotation();

                if (NPC.spriteDirection == -1)
                    NPC.rotation += MathHelper.Pi;

                NPC.velocity = yharonFireballMoveDirection * splittingFireballBreathPhaseVelocity;
                SoundEngine.PlaySound(FireSound, NPC.Center);
            }
            else
            {
                NPC.position.X += NPC.SafeDirectionTo(targetData.Center).X * 7f;
                NPC.position.Y += NPC.SafeDirectionTo(targetData.Center + new Vector2(0f, -400f)).Y * 6f;

                float xOffset = 30f;
                Vector2 position = NPC.Center + new Vector2((110f + xOffset) * NPC.direction, -20f).RotatedBy(NPC.rotation);
                int yharonFireballTimer = (int)(NPC.ai[1] - splittingFireballBreathTimer + 1f);

                if (yharonFireballTimer <= splittingFireballBreathTimer2 && yharonFireballTimer % splittingFireballBreathDivisor == 0 && TOWorld.GeneralClient)
                    Projectile.NewProjectileAction<YharonFireball>(NPC.GetSource_FromAI(), position, NPC.velocity, NPC.GetProjectileDamage<YharonFireball>(), 0f, Main.myPlayer);
            }

            if (NPC.ai[1] > splittingFireballBreathPhaseTimer - splittingFireballBreathYVelocityTimer)
                NPC.velocity.Y -= 0.1f;

            NPC.ai[1] += 1f;
            if (NPC.ai[1] >= splittingFireballBreathPhaseTimer)
            {
                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.TargetClosest();
            }
        }

        // Fireball spin
        else if (NPC.ai[0] == 5f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[1] == 1f)
            {
                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
                SoundEngine.PlaySound(OrbSound, NPC.Center);
            }

            NPC.ai[1] += 1f;
            if (TOWorld.GeneralClient)
            {
                if (secondPhasePhase >= 3)
                {
                    // Rotate spiral by 9 * (240 / 12) = +90 degrees and then back -90 degrees

                    // For phase 4: Rotate spiral by 18 * (240 / 16) = +135 degrees and then back -135 degrees

                    if (NPC.ai[1] % flareDustSpawnDivisor2 == 0f)
                    {
                        int totalProjectiles = secondPhasePhase == 4 ? 12 : 10;
                        float projectileVelocity = secondPhasePhase == 4 ? 16f : 12f;
                        float radialOffset = secondPhasePhase == 4 ? 2.8f : 3.2f;
                        if (NPC.localAI[3] == 0f)
                        {
                            DoFlareDustBulletHell(1, spinPhaseTimer, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), totalProjectiles, projectileVelocity, radialOffset, true);
                        }
                        else
                        {
                            int ringReduction = (int)MathHelper.Lerp(0f, 12f, NPC.ai[1] / spinPhaseTimer);
                            int totalProjectiles2 = 38 - ringReduction; // 36 for first ring, 24 for last ring
                            DoFlareDustBulletHell(0, flareDustSpawnDivisor2, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), totalProjectiles2, 0f, 0f, true);
                        }
                    }
                }
                else
                {
                    if (NPC.ai[1] % flareDustSpawnDivisor == 0f)
                    {
                        int ringReduction = (int)MathHelper.Lerp(0f, 12f, NPC.ai[1] / spinPhaseTimer);
                        int totalProjectiles = 38 - ringReduction; // 36 for first ring, 24 for last ring
                        DoFlareDustBulletHell(0, flareDustSpawnDivisor, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareDust>()), totalProjectiles, 0f, 0f, true);
                    }
                }

                if (NPC.ai[1] == 210f && secondPhasePhase == 4 && useTornado)
                {
                    useTornado = false;
                    Projectile.NewProjectileAction<BigFlare2>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 0, 0f, Main.myPlayer, p =>
                    {
                        p.ai[0] = 1f;
                        p.ai[1] = NPC.target + 1;
                    });
                }
            }

            if (NPC.ai[1] >= spinPhaseTimer)
            {
                NPC.ai[0] = 1f;
                NPC.ai[1] = -increasedIdleTimeAfterBulletHell;
                NPC.ai[2] = 0f;
                NPC.localAI[2] = 0f;
                NPC.TargetClosest();
                NPC.velocity /= 2f;
            }
        }

        // Fire ring
        else if (NPC.ai[0] == 6f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.ai[1] == 0f)
            {
                Vector2 destination2 = targetData.Center + new Vector2(0f, -200f);
                Vector2 desiredVelocity2 = NPC.SafeDirectionTo(destination2) * velocity * 1.5f;
                NPC.SimpleFlyMovement(desiredVelocity2, acceleration * 1.5f);

                int flareRingFaceDirection = (NPC.Center.X < targetData.Center.X) ? 1 : -1;
                NPC.direction = NPC.spriteDirection = flareRingFaceDirection;

                NPC.ai[2] += 1f;
                if (NPC.Distance(targetData.Center) < 600f || NPC.ai[2] >= 180f)
                {
                    NPC.ai[1] = 1f;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                if (NPC.ai[1] < flareSpawnDecelerationTimer)
                    NPC.velocity *= 0.95f;
                else
                    NPC.velocity *= 0.98f;

                if (NPC.ai[1] == flareSpawnDecelerationTimer)
                {
                    if (NPC.velocity.Y > 0f)
                        NPC.velocity.Y /= 3f;

                    NPC.velocity.Y -= 3f;
                }

                if (TOWorld.GeneralClient)
                {
                    if (NPC.ai[1] is 20f or 80f or 140f)
                    {
                        SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                        DoFireRing(expertMode ? 300 : 180, NPC.GetProjectileDamage(ModContent.ProjectileType<FlareBomb>()), NPC.target, 1f);
                    }
                }

                NPC.ai[1] += 1f;
            }

            if (NPC.ai[1] >= flareSpawnPhaseTimer)
            {
                SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

                if (TOWorld.GeneralClient)
                {
                    Projectile.NewProjectileAction<BigFlare2>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 0, 0f, Main.myPlayer, p =>
                    {
                        p.ai[0] = 1f;
                        p.ai[1] = NPC.target + 1;
                    });
                }

                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.TargetClosest();
            }
        }

        // Fast charge
        else if (NPC.ai[0] == 7f)
        {
            // Set damage
            NPC.damage = setDamage;

            if (NPC.ai[1] == 1f)
                SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

            ChargeDust(14);

            NPC.ai[1] += 1f;
            if (NPC.ai[1] >= chargeTime)
            {
                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.TargetClosest();
            }
        }

        // Teleport
        else if (NPC.ai[0] == 8f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            if (NPC.Opacity > 0f)
            {
                NPC.Opacity -= 0.1f;
                if (NPC.Opacity < 0f)
                    NPC.Opacity = 0f;
            }

            NPC.velocity *= 0.98f;
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, 0f, 0.02f);

            if (NPC.ai[2] == 15f)
                SoundEngine.PlaySound(ShortRoarSound, NPC.Center);

            if (TOWorld.GeneralClient && NPC.ai[2] == 15f)
            {
                if (NPC.ai[1] == 0f)
                    NPC.ai[1] = 450 * Math.Sign((NPC.Center - targetData.Center).X);

                teleportLocation = Main.rand.NextBool() ? (revenge ? 500 : 600) : (revenge ? -500 : -600);
                Vector2 center = targetData.Center + new Vector2(-NPC.ai[1], teleportLocation);
                NPC.Center = center;
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= teleportPhaseTimer)
            {
                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.localAI[1] = fastChargeTelegraphTime + 1f;
                NPC.netUpdate = true;
            }
        }

        // Enter new phase
        else if (NPC.ai[0] == 9f)
        {
            // Avoid cheap bullshit
            NPC.damage = 0;

            NPC.velocity *= 0.9f;

            Vector2 vector = NPC.SafeDirectionTo(targetData.Center, -Vector2.UnitY);
            NPC.spriteDirection = (vector.X > 0f) ? 1 : -1;
            NPC.rotation = vector.ToRotation();

            if (NPC.spriteDirection == -1)
                NPC.rotation += MathHelper.Pi;

            if (NPC.ai[2] == 120f)
            {
                if (secondPhasePhase == 4)
                {
                    for (int x = 0; x < Main.maxProjectiles; x++)
                    {
                        Projectile projectile = Main.projectile[x];
                        if (projectile.active)
                        {
                            if (projectile.type == ModContent.ProjectileType<Infernado2>())
                            {
                                if (projectile.timeLeft >= 300)
                                    projectile.active = false;
                                else if (projectile.timeLeft > 5)
                                    projectile.timeLeft = (int)(5f * projectile.ai[1]);
                            }
                            else if (projectile.type == ModContent.ProjectileType<BigFlare2>())
                                projectile.active = false;
                        }
                    }
                }

                RoarSoundSlot = SoundEngine.PlaySound(RoarSound, NPC.Center);
            }

            NPC.ai[2] += 1f;
            if (NPC.ai[2] >= 180f)
            {
                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.ai[3] = 0f;
                NPC.TargetClosest();
                NPC.netUpdate = true;
            }
        }

        float facingAngle = NPC.AngleTo(targetData.Center);
        float rotationSpeed = 0.04f;

        switch ((int)NPC.ai[0])
        {
            case 2:
            case 7:
            case 8:
            case 9:
                rotationSpeed = 0f;
                break;

            case 3:
                if (NPC.ai[1] >= fireballBreathTimer)
                    rotationSpeed = 0f;

                break;

            case 4:
                rotationSpeed = 0.01f;
                facingAngle = MathHelper.Pi;

                if (NPC.spriteDirection == 1)
                    facingAngle += MathHelper.Pi;

                break;
            case 6:
                rotationSpeed = 0.02f;
                facingAngle = 0f;

                if (NPC.spriteDirection == -1)
                    facingAngle -= MathHelper.Pi;

                break;
        }

        if (NPC.spriteDirection == -1)
            facingAngle += MathHelper.Pi;

        if (rotationSpeed != 0f)
            NPC.rotation = NPC.rotation.AngleTowards(facingAngle, rotationSpeed);
    }

    private void IncreaseDR()
    {
        CalamityNPC.DR = BossRushEvent.BossRushActive ? 0.99f : 0.97f;
        CalamityNPC.unbreakableDR = true;
        CalamityNPC.CurrentlyIncreasingDefenseOrDR = true;
    }

    private void ChargeDust(int dustAmt)
    {
        for (int i = 0; i < dustAmt; i++)
        {
            Vector2 dustRotate = (Vector2.Normalize(NPC.velocity) * new Vector2((NPC.width + 50) / 2f, NPC.height) * 0.75f).RotatedBy((i - (dustAmt / 2 - 1)) * Math.PI / dustAmt) + NPC.Center;
            Vector2 dustVel = (Main.rand.NextFloat() * MathHelper.Pi - MathHelper.PiOver2).ToRotationVector2() * Main.rand.Next(3, 8);
            Dust.NewDustPerfectAction(dustRotate + dustVel, DustID.CopperCoin, d =>
            {
                d.velocity = dustVel / 2f - NPC.velocity;
                d.noGravity = true;
                d.noLight = true;
            });
        }
    }

    private void DoFlareDustBulletHell(int attackType, int timer, int projectileDamage, int totalProjectiles, float projectileVelocity, float radialOffset, bool phase2)
    {
        SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
        float aiVariableUsed = phase2 ? NPC.ai[1] : NPC.ai[2];
        switch (attackType)
        {
            case 0:
                float offsetAngle = 360 / totalProjectiles;
                int totalSpaces = totalProjectiles / 5;
                int spaceStart = Main.rand.Next(totalProjectiles - totalSpaces);
                float ai0 = aiVariableUsed % (timer * 2) == 0f ? 1f : 0f;

                int spacesMade = 0;
                for (int i = 0; i < totalProjectiles; i++)
                {
                    if (i >= spaceStart && spacesMade < totalSpaces)
                        spacesMade++;
                    else
                        Projectile.NewProjectileAction<FlareDust>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, projectileDamage, 0f, Main.myPlayer, p =>
                        {
                            p.ai[0] = ai0;
                            p.ai[1] = i * offsetAngle;
                        });
                }
                break;

            case 1:
                Vector2 spinningPoint = Vector2.Normalize(new Vector2(-NPC.localAI[2], -projectileVelocity));

                Projectile.RotatedProj<FlareDust>(totalProjectiles, MathHelper.TwoPi / totalProjectiles, NPC.GetSource_FromAI(), NPC.Center, spinningPoint * projectileVelocity, projectileDamage, 0f, Main.myPlayer, p => p.ai[0] = 2f);

                float newRadialOffset = (int)aiVariableUsed / (timer / 4) % 2f == 0f ? radialOffset : -radialOffset;
                NPC.localAI[2] += newRadialOffset;
                break;

            default:
                break;
        }
    }

    public void DoFireRing(int timeLeft, int damage, float ai0, float ai1)
    {
        if (!TOWorld.GeneralClient)
            return;

        float velocity = ai1 == 0f ? 10f : 5f;
        int totalProjectiles = 50;
        float radians = MathHelper.TwoPi / totalProjectiles;
        Projectile.RotatedProj<FlareBomb>(totalProjectiles, MathHelper.TwoPi / totalProjectiles, NPC.GetSource_FromAI(), NPC.Center, new Vector2(0f, -velocity), damage, 0f, Main.myPlayer, p =>
        {
            p.ai[0] = ai0;
            p.ai[1] = ai1;
            p.timeLeft = timeLeft;
        });
    }

    #endregion AI
}

public sealed class Yharon_Detour : ModNPCDetour<Yharon>
{
    public override void Detour_SetDefaults(Orig_SetDefaults orig, Yharon self)
    {
        orig(self);

        NPC npc = self.NPC;
        npc.LifeMaxNERB(650000, 780000, 370000); //为适应巨龙重生，修改了最大生命值
        npc.ApplyCalamityBossHealthBoost();
    }

    public override void Detour_HitEffect(Orig_HitEffect orig, Yharon self, NPC.HitInfo hit)
    {
        NPC npc = self.NPC;
        CalamityGlobalNPC calamityNPC = npc.Calamity();
        // hit sound
        if (npc.soundDelay == 0)
        {
            npc.soundDelay = Main.rand.Next(16, 20);
            SoundEngine.PlaySound(HitSound, npc.Center);
        }

        for (int k = 0; k < 5; k++)
            Dust.NewDustAction(npc.Center, npc.width, npc.height, DustID.Blood, new Vector2(hit.HitDirection, -1f));

        Yharon_Publicizer yharonPublicizer = new(self);
        bool shouldNotDie = !yharonPublicizer.startSecondAI || yharonPublicizer.invincibilityCounter < Yharon_Tweak.Data.Phase2InvincibilityTime;

        if (npc.life <= 0)
        {
            bool shouldSummonProj = true;
            bool summonBuffedProj = false;
            if (shouldNotDie)
            {
                switch ((int)calamityNPC.newAI[2]++)
                {
                    case 0 or 3 or 6 or 10 or 15:
                        break;
                    case 20:
                        summonBuffedProj = true;
                        break;
                    default:
                        shouldSummonProj = false;
                        break;
                }
                npc.SyncExtraAI();
            }
            if (shouldSummonProj)
                self.DoFireRing(300, summonBuffedProj ? 2000 : ((Main.expertMode || BossRushEvent.BossRushActive) ? 125 : 150), -1f, 0f);

            npc.position.X += (npc.width / 2);
            npc.position.Y += (npc.height / 2);
            npc.width = 300;
            npc.height = 280;
            npc.position.X -= (npc.width / 2);
            npc.position.Y -= (npc.height / 2);
            for (int i = 0; i < 40; i++)
            {
                Dust.NewDustAction(npc.Center, npc.width, npc.height, DustID.CopperCoin, Vector2.Zero, d =>
                {
                    d.alpha = 100;
                    d.velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        d.scale = 0.5f;
                        d.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                    else
                        d.scale = 2f;
                });
            }
            for (int j = 0; j < 70; j++)
            {
                Dust.NewDustAction(npc.Center, npc.width, npc.height, DustID.CopperCoin, Vector2.Zero, d =>
                {
                    d.alpha = 100;
                    d.velocity *= 5f;
                    d.noGravity = true;
                    d.scale = 3f;
                });
                Dust.NewDustAction(npc.Center, npc.width, npc.height, DustID.CopperCoin, Vector2.Zero, d =>
                {
                    d.alpha = 100;
                    d.velocity *= 2f;
                    d.scale = 2f;
                });
            }

            // Turn into dust on death.
            if (npc.life <= 0 && !shouldNotDie)
                DeathAshParticle.CreateAshesFromNPC(npc);
        }
    }
}

public sealed class ResplendentExplosion : BaseMassiveExplosionProjectile
{
    public override int Lifetime => 120;

    public override bool UsesScreenshake => true;

    public override float GetScreenshakePower(float pulseCompletionRatio) => CalamityUtils.Convert01To010(pulseCompletionRatio) * 20f;

    public override Color GetCurrentExplosionColor(float pulseCompletionRatio) =>
        Color.Lerp(new Color(0xC4, 0xA9, 0x60), Color.Orange, MathHelper.Clamp(pulseCompletionRatio * 2f, 0f, 1f)) with { A = 0 };

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.timeLeft = Lifetime;
    }
}
