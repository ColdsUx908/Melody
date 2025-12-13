using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelRainbow : CAModNPC
{
    public const float DespawnDistance = 5000f;
    public const int AttackTypes = 4;
    public int ShootCooldownTime => (int)MathHelper.Lerp(210f, 180f, NPC.LostLifeRatio);

    public int CurrentAttackType
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value % AttackTypes;
    }

    public bool HasEnteredPhase2
    {
        get => NPC.ai[2] == 1f;
        set => NPC.ai[2] = value.ToInt();
    }

    public bool CanAttack
    {
        get => NPC.ai[3] != 1f;
        set => NPC.ai[3] = (!value).ToInt();
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

        NPC.knockBackResist = 0.5f;
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

        if (CanAttack)
        {
            JewelHandler.Move(NPC, Target.Center, 15f, 15f, 0.175f, 0.125f, 250f, -250f, -250f, -400f);
            Timer1++;
        }
        else
        {
            JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.15f, 150f, -150f, 0f, -200f);
            Timer1 -= 2;
        }

        if (Timer1 >= ShootCooldownTime)
        {
            Timer1 = 0;
            Shoot();
            SpawnParticle();
        }

        NPC.netUpdate = true;

        return;

        void Shoot()
        {
            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            if (!TOWorld.GeneralClient)
                return;

            bool buffedAttack = NPC.LifeRatio < 0.5f;
            switch (++CurrentAttackType)
            {
                case 0:
                    {
                        int amount = 24;
                        float initialRotation = (Target.Center - NPC.Center).ToRotation();
                        Projectile.RotatedProj<JewelProjectileRainbow>(amount, MathHelper.TwoPi / amount, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(16.5f, initialRotation), NPC.GetProjectileDamage<JewelProjectile>(), 0f);
                        for (int i = 0; i < 50; i++)
                            Projectile.NewProjectileAction<JewelProjectileRainbow>(NPC.GetSource_FromAI(), Target.Center + new Vector2(Main.rand.NextFloat(-2500f, 2500f), Main.rand.NextFloat(-2200f, -900f)), new PolarVector2(Main.rand.NextFloat(9f, 13f), MathHelper.PiOver2 + Main.rand.NextFloat(-TOMathHelper.PiOver8, TOMathHelper.PiOver8)), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);
                    }
                    break;
                case 1 or 2:
                    {
                        int amount = 7;
                        float singleRadian = MathHelper.ToRadians(15f);
                        float radian = singleRadian * (amount - 1);
                        float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
                        Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(15f, initialRotation), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);
                    }
                    break;
                case 3:
                    {
                        int amount = buffedAttack ? 11 : 9;
                        float singleRadian = MathHelper.ToRadians(17.5f);
                        float radian = singleRadian * (amount - 1);
                        float initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
                        Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(16.5f, initialRotation), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);
                        amount--;
                        radian = singleRadian * (amount - 1);
                        initialRotation = (Target.Center - NPC.Center).ToRotation() - radian / 2f;
                        Projectile.RotatedProj<JewelProjectileRainbow>(amount, singleRadian, NPC.GetSource_FromAI(), NPC.Center, new PolarVector2(15f, initialRotation), NPC.GetProjectileDamage<JewelProjectileRainbow>(), 0f);
                    }
                    break;
            }
        }

        void SpawnParticle()
        {
            int particleAmount = Main.zenithWorld ? 30 : CurrentAttackType switch
            {
                2 => 30,
                3 => 50,
                _ => 20
            };
            float minVelocity = 3f;
            float maxVelocity = CurrentAttackType switch
            {
                2 => 8f,
                3 => 10f,
                _ => 6f
            };
            for (int i = 0; i < particleAmount; i++)
                JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(minVelocity, maxVelocity), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawRainbowTrail(spriteBatch, screenPos, NPC, NPC.oldPos);

        float timeLeftGateValue = CurrentAttackType switch
        {
            2 => 45,
            3 => 55,
            _ => 40
        };
        float gateValue = ShootCooldownTime - timeLeftGateValue;
        float ratio = Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        float radius = CurrentAttackType switch
        {
            2 => 145f,
            3 => 160f,
            _ => 135f
        };
        float scale = CurrentAttackType switch
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
}
