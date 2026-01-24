namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRainbow : CAModNPC
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
            Union32 union32 = AI_Union_2;
            union32.bits[0] = value;
            AI_Union_2 = union32;
        }
    }

    public bool HasEnteredPhase2
    {
        get => AI_Union_2.bits[1];
        set
        {
            Union32 union32 = AI_Union_2;
            union32.bits[1] = value;
            AI_Union_2 = union32;
        }
    }

    public bool CanAttack
    {
        get => !AI_Union_2.bits[2];
        set
        {
            Union32 union32 = AI_Union_2;
            union32.bits[2] = !value;
            AI_Union_2 = union32;
        }
    }

    public bool IsAttacking
    {
        get => AI_Union_2.bits[3];
        set
        {
            Union32 union32 = AI_Union_2;
            union32.bits[3] = value;
            AI_Union_2 = union32;
        }
    }

    public override string Texture => JewelHandler.JewelTexturePath;
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

        NPC.lifeMax = 600;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.3f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit5;
        NPC.DeathSound = SoundID.NPCDeath15;
        CalamityNPC.VulnerableToSickness = false;
    }

    public override void AI()
    {
        if (!OceanNPC.TryGetMaster(NPCID.KingSlime, out NPC master))
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
            Projectile.NewProjectileAction<RainbowExplosion>(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, NPC.GetProjectileDamage<RainbowExplosion>(), 0f, action: p =>
            {
                RainbowExplosion rainbowExplosion = p.GetModProjectile<RainbowExplosion>();
                rainbowExplosion.Jewel = NPC;
                rainbowExplosion.Master = master;
            });

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
            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            EnterNextAttack();
            SpawnParticle();
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

        void SpawnParticle()
        {
            int particleAmount = Main.zenithWorld ? 30 : CurrentAttack switch
            {
                Attack.CrossingAttack => 30,
                Attack.RainbowRain => 50,
                _ => 20
            };
            float minVelocity = 3f;
            float maxVelocity = CurrentAttack switch
            {
                Attack.CrossingAttack => 8f,
                Attack.RainbowRain => 10f,
                _ => 6f
            };
            for (int i = 0; i < particleAmount; i++)
                JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(minVelocity, maxVelocity), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
        }

        void NormalAttack()
        {
            if (!TOWorld.GeneralClient)
                return;

            int amount = 7;
            float singleRadian = MathHelper.ToRadians(15f);
            float radian = singleRadian * (amount - 1);
            float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
            Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(15f, initialRotation), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);

            IsAttacking = false;
        }

        void CrossingAttack()
        {
            if (!TOWorld.GeneralClient)
                return;

            int amount = 11;
            float singleRadian = MathHelper.ToRadians(17.5f);
            float radian = singleRadian * (amount - 1);
            float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
            Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(17.5f, initialRotation), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);
            amount--;
            radian = singleRadian * (amount - 1);
            initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
            Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(15f, initialRotation), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);

            IsAttacking = false;
        }

        void RingAttack()
        {
            if (!TOWorld.GeneralClient)
                return;

            if (Timer1 % 4 == 0)
            {
                int attackNum = Timer1 / 4;
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
                float initialRotation = (Target.Center - NPC.Center).ToRotation() + attackNum * TOMathHelper.PiOver5 + Main.rand.NextFloat(TOMathHelper.PiOver12);
                Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(18f - attackNum / 2f, initialRotation), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);
            }

            Timer1++;

            if (Timer1 > 20)
                IsAttacking = false;
        }

        void RainbowRain()
        {
            if (!TOWorld.GeneralClient)
                return;

            if (Timer1 == 0)
            {
                NormalAttack();
                IsAttacking = true;
            }
            if (Timer1 % 3 == 0)
                Projectile.NewProjectileAction<JewelProjectileRainbow>(NPC.GetSource_FromAI(), Target.Center + new Vector2(Main.rand.NextFloat(-2000f, 2000f), Main.rand.NextFloat(-1500f, -1000f)), new PolarVector2(Main.rand.NextFloat(10f, 14f), MathHelper.PiOver2 + Main.rand.NextFloat(-TOMathHelper.PiOver8, TOMathHelper.PiOver8)), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);

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
        if (CAClientConfig.Instance.AuxiliaryVisualEffects && ratio > 0f)
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
                float rotation = (old - oldold).ToRotation() - MathHelper.PiOver2;
                Vector2 scale = new(1f, Vector2.Distance(oldold, old) / texture.Height);
                Color color = white * (1f - (float)i / oldPos.Length);
                spriteBatch.Draw(texture, oldold + entity.Size / 2f - screenPos, null, color, rotation, origin, scale, effects, 0f);
            }
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, JewelHandler.GetRandomDustID(), hit.HitDirection, -1f, 0, default, 1f);
        Main.dust[dust].noGravity = true;

        if (NPC.life <= 0)
        {
            NPC.position = NPC.Center;
            NPC.width = NPC.height = 45;
            NPC.position.X = NPC.position.X - (NPC.width / 2);
            NPC.position.Y = NPC.position.Y - (NPC.height / 2);

            for (int i = 0; i < 4; i++)
            {
                int dust1 = Dust.NewDust(NPC.position, NPC.width, NPC.height, JewelHandler.GetRandomDustID(), 0f, 0f, 100, default, 2f);
                Main.dust[dust1].noGravity = true;
                Main.dust[dust1].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[dust1].scale = 0.5f;
                    Main.dust[dust1].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }

            for (int j = 0; j < 20; j++)
            {
                int dust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, JewelHandler.GetRandomDustID(), 0f, 0f, 100, default, 3f);
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].velocity *= 5f;
                dust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, JewelHandler.GetRandomDustID(), 0f, 0f, 100, default, 2f);
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].velocity *= 2f;
            }
        }
    }
}
