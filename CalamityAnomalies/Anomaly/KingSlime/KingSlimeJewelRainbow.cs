using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRainbow : CAModNPC, IKingSlimeJewel
{
    public enum Attack
    {
        NormalAttack = 1,
        CrossingAttack = 2,
        RingAttack = 3,
        RainbowRain = 4,
    }

    public const float DespawnDistance = 5000f;
    public const int AttackCycleTypes = 3;
    public static int ShootCooldownTime => 150;

    private static readonly ProjectileDamageContainer _jewelProjectileRainbowDamage = new(40, 65, 90, 110, 105, 125);
    public static int JewelProjectileRainbowDamage => _jewelProjectileRainbowDamage.Value;

    public Attack CurrentAttack
    {
        get => (Attack)(int)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    public int AttackCounter
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public bool HasInitialized
    {
        get => AI_Union_2.bits[0];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[0] = value;
            AI_Union_2 = union;
        }
    }

    public bool HasEnteredPhase2
    {
        get => AI_Union_2.bits[1];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[1] = value;
            AI_Union_2 = union;
        }
    }

    public bool CanAttack
    {
        get => AI_Union_2.bits[2];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[2] = value;
            AI_Union_2 = union;
        }
    }

    public bool KingSlimeDead
    {
        get => AI_Union_2.bits[3];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[3] = value;
            AI_Union_2 = union;
        }
    }

    public bool IsAttacking
    {
        get => AI_Union_2.bits[4];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[4] = value;
            AI_Union_2 = union;
        }
    }

    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.damage = 25;
        NPC.width = 28;
        NPC.height = 28;
        NPC.defense = 10;
        NPC.DR_NERD(0.1f);

        NPC.lifeMax = 700;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.2f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit5;
        NPC.DeathSound = SoundID.NPCDeath15;
        CalamityNPC.VulnerableToSickness = false;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = (int)(NPC.lifeMax * balance);

    public override void AI()
    {
        if (KingSlimeDead)
        {
            JewelHandler.Kill(NPC);
            return;
        }

        if (!NPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
            return;
        }

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return;
        }

        Lighting.AddLight(NPC.Center, 1f, 0f, 0f);

        NPC.damage = 0;
        CalamityNPC.CanHaveBossHealthBar = true;

        if (!HasInitialized)
        {
            Projectile.NewProjectileAction<RainbowShockwave>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, 100, 0f, action: p =>
            {
                p.scale = 0f;
                RainbowShockwave modP = p.GetModProjectile<RainbowShockwave>();
                modP.Jewel = NPC;
                modP.Master = master;
            });

            CanAttack = true;

            HasInitialized = true;
        }

        if (CanAttack)
        {
            JewelHandler.Move(NPC, Target.Center, 15f, 15f, 0.175f, 0.125f, 250f, -250f, -250f, -400f);
            if (!IsAttacking)
                Timer1++;
        }
        else
        {
            JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.15f, 150f, -150f, 0f, -200f);
            Timer2--;
        }

        if (Timer1 >= ShootCooldownTime)
        {
            Timer1 = 0;
            EnterNextAttack();
        }

        if (CanAttack)
        {
            if (IsAttacking)
            {
                switch (CurrentAttack)
                {
                    case Attack.NormalAttack:
                        NormalAttack();
                        break;
                    case Attack.CrossingAttack:
                        CrossingAttack();
                        break;
                    case Attack.RingAttack:
                        RingAttack();
                        break;
                    case Attack.RainbowRain:
                        RainbowRain();
                        break;
                }
            }
        }
        else
            IsAttacking = false;

        NPC.netUpdate = true;

        return;

        void EnterNextAttack()
        {
            CurrentAttack = AttackCounter switch
            {
                0 => Attack.NormalAttack,
                1 => Attack.CrossingAttack,
                2 => Attack.RingAttack,
                3 => Attack.RainbowRain,
                _ => Attack.NormalAttack,
            };
            AttackCounter++;
            if (AttackCounter > 3)
                AttackCounter = 1;
            IsAttacking = true;
        }

        void NormalAttack()
        {
            SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
            for (int i = 0; i < 20; i++)
                JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
            JewelHandler.SpawnPointingParticle(NPC, 6, true);

            if (TOSharedData.GeneralClient)
            {
                int amount = 7;
                float singleRadian = MathHelper.ToRadians(15f);
                float radian = singleRadian * (amount - 1);
                float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
                Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(15f, initialRotation), JewelProjectileRainbowDamage, 0f);
            }

            IsAttacking = false;
        }

        void CrossingAttack()
        {
            SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
            for (int i = 0; i < 30; i++)
                JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 8f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
            JewelHandler.SpawnPointingParticle(NPC, 8, true);

            if (TOSharedData.GeneralClient)
            {
                int amount = 11;
                float singleRadian = MathHelper.ToRadians(17.5f);
                float radian = singleRadian * (amount - 1);
                float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
                Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(17.5f, initialRotation), JewelProjectileRainbowDamage, 0f);
                amount--;
                radian = singleRadian * (amount - 1);
                initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
                Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(15f, initialRotation), JewelProjectileRainbowDamage, 0f);
            }

            IsAttacking = false;
        }

        void RingAttack()
        {
            if (Timer1 % 4 == 0)
            {
                int attackNum = Timer1 / 4;
                int orbParticleAmount = attackNum switch
                {
                    0 => 5,
                    1 => 5,
                    2 => 5,
                    3 => 10,
                    4 => 15,
                    _ => 30
                };
                int pointingParticleAmount = attackNum switch
                {
                    0 => 2,
                    1 => 2,
                    2 => 2,
                    3 => 3,
                    4 => 4,
                    _ => 8
                };
                SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
                for (int i = 0; i < orbParticleAmount; i++)
                    JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
                JewelHandler.SpawnPointingParticle(NPC, pointingParticleAmount, true);


                if (TOSharedData.GeneralClient)
                {
                    int amount = attackNum switch
                    {
                        0 => 3,
                        1 => 6,
                        2 => 9,
                        3 => 12,
                        4 => 16,
                        _ => 36
                    };
                    float singleRadian = MathHelper.TwoPi / amount;
                    float radian = singleRadian * (amount - 1);
                    float initialRotation = (Target.Center - NPC.Center).ToRotation() + attackNum * TOMathUtils.PiOver5 + Main.rand.NextFloat(TOMathUtils.PiOver12);
                    Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(18f - attackNum / 2f, initialRotation), 30, 0f);
                }
            }

            Timer1++;

            if (Timer1 > 20)
                IsAttacking = false;
        }

        void RainbowRain()
        {
            if (TOSharedData.GeneralClient)
            {
                if (Timer1 == 0)
                {
                    NormalAttack();
                    IsAttacking = true;
                }
                if (Timer1 % 3 == 0)
                    Projectile.NewProjectileAction<JewelProjectileRainbow>(NPC.GetSource_FromAI(), Target.Center + new Vector2(Main.rand.NextFloat(-2000f, 2000f), Main.rand.NextFloat(-1500f, -1000f)), new PolarVector2(Main.rand.NextFloat(10f, 14f), MathHelper.PiOver2 + Main.rand.NextFloat(-TOMathUtils.PiOver8, TOMathUtils.PiOver8)), 30, 0f);
            }

            Timer1++;

            if (Timer1 > 105)
            {
                Timer1 -= 30; //休息0.5秒
                IsAttacking = false;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawRainbowTrail(spriteBatch, screenPos, NPC, NPC.oldPos);

        float timeLeftGateValue = AttackCounter switch
        {
            2 => 45,
            3 => 55,
            _ => 40
        };
        float gateValue = ShootCooldownTime - timeLeftGateValue;
        float ratio = Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        float radius = AttackCounter switch
        {
            2 => 145f,
            3 => 160f,
            _ => 135f
        };
        float scale = AttackCounter switch
        {
            2 => 0.375f,
            3 => 0.425f,
            _ => 0.35f
        };
        JewelHandler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, radius, scale);
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
        return false;
    }

    public static void DrawRainbowTrail(SpriteBatch spriteBatch, Vector2 screenPos, Entity entity, Vector2[] oldPos, SpriteEffects effects = SpriteEffects.None)
    {
        Texture2D texture = TOAssetUtils.GetProjectileTexture(ProjectileID.RainbowFront);
        Vector2 origin = new(texture.Width / 2, 0f);
        Color white = Color.White with { A = 127 };
        for (int i = oldPos.Length - 1; i > 0; i--)
        {
            if (oldPos[i] != Vector2.Zero)
            {
                Vector2 old = oldPos[i - 1];
                Vector2 oldold = oldPos[i];
                float rotation = (old - oldold).ToRotation(-MathHelper.PiOver2);
                Vector2 scale = new(1f, Vector2.Distance(oldold, old) / texture.Height);
                Color color = white * (1f - (float)i / oldPos.Length);
                spriteBatch.Draw(texture, oldold + entity.Size / 2f - screenPos, null, color, rotation, origin, scale, effects, 0f);
            }
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        JewelHandler.HitEffect(NPC);
    }

    public override void OnKill()
    {
        JewelHandler.OnKill(NPC);
    }
}
