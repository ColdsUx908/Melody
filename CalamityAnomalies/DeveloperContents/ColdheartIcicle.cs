#define CELESS_DEV

using CalamityAnomalies.Assets.Textures;
using CalamityAnomalies.Items;
using CalamityAnomalies.Publicizer.CalamityMod;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent.ItemDropRules;
using Transoceanic.Data.Geometry;

namespace CalamityAnomalies.DeveloperContents;

#region 物品
public sealed class ColdheartIcicle : ModItem
{
    #region Static
    public const int SpriteWidth = 24;
    public const string TexturePath = "CalamityMod/Items/ColdheartIcicle";

    public static bool IsLegendOwner(Player player) => player.name == "ColdsUx";

    public static bool IsDream(Player player) => player.name == "Celessalia";
    #endregion Static

    #region 传奇
    public int Phase = 1;
    public int SubPhase = 1;

    public void LegendaryUpdate()
    {
        if (DownedBossSystem.downedPrimordialWyrm)
        {
            Phase = 7;
            SubPhase = 1;
        }
        else if (DownedBossSystem.downedSignus)
        {
            Phase = 6;
            if (CAUtils.Focus)
                SubPhase = 6;
            else if (DownedBossSystem.downedExoMechs || DownedBossSystem.downedCalamitas)
                SubPhase = 5;
            else if (DownedBossSystem.downedYharon)
                SubPhase = 4;
            else if (DownedBossSystem.downedDoG)
                SubPhase = 3;
            else if (DownedBossSystem.downedPolterghast)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (NPC.downedAncientCultist)
        {
            Phase = 5;
            if (DownedBossSystem.downedProvidence)
                SubPhase = 3;
            else if (NPC.downedMoonlord)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (DownedBossSystem.downedLeviathan)
        {
            Phase = 4;
            if (NPC.downedGolemBoss)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (DownedBossSystem.downedCryogen)
        {
            Phase = 3;
            if (NPC.downedPlantBoss)
                SubPhase = 3;
            else if (TONPCUtils.DownedMechBossAll)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (NPC.downedDeerclops)
        {
            Phase = 2;
            if (Main.hardMode)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else
        {
            Phase = 1;
            if (NPC.downedBoss3)
                SubPhase = 5;
            else if (DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator)
                SubPhase = 4;
            else if (NPC.downedBoss2)
                SubPhase = 3;
            else if (NPC.downedBoss1)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
    }

    public void LegendaryUpdate(Player player)
    {
        LegendaryUpdate();
        CAPlayer anomalyPlayer = player.Anomaly();
        anomalyPlayer.Coldheart_Phase = Phase;
        anomalyPlayer.Coldheart_SubPhase = SubPhase;
    }
    #endregion 传奇

    public override string Texture => TexturePath;

    public override string LocalizationCategory => "DeveloperContents";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Rapier;
        Item.damage = 20;
        Item.DamageType = TrueMeleeNoSpeedDamageClass_Publicizer.Instance;
        Item.width = SpriteWidth;
        Item.height = SpriteWidth;
        Item.useTime = 27;
        Item.useAnimation = 27;
        Item.autoReuse = true;
        Item.UseSound = SoundID.Item1;
        Item.useTurn = true;
        Item.knockBack = 3f;
        Item.value = TOMain.CelestialPrice;
        Item.shoot = ModContent.ProjectileType<ColdheartIcicleProj>();
        Item.shootSpeed = 1.25f;
        Item.rare = ModContent.RarityType<Celestial>();
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.ArmorPenetration = 350258;
        Item.Calamity().devItem = true;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        LegendaryUpdate();
    }

    public override void UpdateInventory(Player player)
    {
        LegendaryUpdate();
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
#if CELESS_DEV
        if (IsDream(player))
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ColdheartIcicleDream>()] < 1)
            {
                Projectile.NewProjectileAction<ColdheartIcicleDream>(Item.GetSource_FromThis(), player.Center, Vector2.Zero, 0, 0, player.whoAmI, p =>
                {
                    p.Center = player.Center;
                    p.SetVelocityandRotation(Vector2.Zero);
                    ColdheartIcicleDream modP = p.GetModProjectile<ColdheartIcicleDream>();
                    modP.Target = player;
                    modP.Behavior = player.altFunctionUse == 2 ? ColdheartIcicleDream.BehaviorType.SetOut : ColdheartIcicleDream.BehaviorType.Dream;
                });
            }
            return false;
        }
        else
            return true;
#else
        return true;
#endif
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        damage *= Phase switch
        {
            1 => SubPhase switch
            {
                1 => 1f,        //20
                2 => 1.25f,     //25
                3 => 2f,        //40
                4 => 3f,        //60
                5 => 3.5f,      //70
                _ => 1f
            },
            2 => SubPhase switch
            {
                1 => 4f,        //80
                2 => 8f,        //160
                _ => 1f
            },
            3 => SubPhase switch
            {
                1 => 12.5f,     //250
                2 => 18f,       //360
                3 => 25f,       //500
                _ => 1f
            },
            4 => SubPhase switch
            {
                1 => 35f,       //700
                2 => 50f,       //1000
                _ => 1f
            },
            5 => SubPhase switch
            {
                1 => 65f,       //1300
                2 => 100f,      //2000
                3 => 120f,      //2400
                _ => 1f
            },
            6 => SubPhase switch
            {
                1 => 150f,       //3000
                2 => 200f,       //4000
                3 => 300f,       //6000
                4 => 400f,       //8000
                5 => 450f,       //9000
                6 => 750f,       //15000
                _ => 1f
            },
            7 => 1500f,          //30000
            _ => 1f
        };
    }

    public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback)
    {
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectileAction<ColdheartIcicleProj>(source, position, velocity, damage, knockback, player.whoAmI, p =>
        {
            ColdheartIcicleProj modP = p.GetModProjectile<ColdheartIcicleProj>();
            modP.IsRightClick = player.altFunctionUse == 2;
            modP.Phase = Phase;
            modP.SubPhase = SubPhase;
        });
        if (player.altFunctionUse == 2)
            Projectile.NewProjectileAction<ColdheartIcicleSnowflake>(source, player.Center, velocity.ToCustomLength(3f), damage, 0f, player.whoAmI, p => p.VelocityToRotation());
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        CAItemTooltipModifier.Instance.Update(tooltips)
            .ClearAllCATooltips();
        if (Main.keyState.IsKeyDown(Keys.LeftShift))
        {

        }
        else
            CAItemTooltipModifier.Instance.AddExpendedDisplayLine();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        return base.PreDrawTooltipLine(line, ref yOffset);
    }
}
#endregion 物品

#region 弹幕
public sealed class ColdheartIcicleProj : BaseShortswordProjectile, ICAModProjectile, ILocalizedModType
{
    public override string Texture => ColdheartIcicle.TexturePath;

    public new string LocalizationCategory => "DeveloperContents";

    public bool IsRightClick;
    public int Phase = 1;
    public int SubPhase = 1;

    public override float FadeInDuration => base.FadeInDuration;
    public override float FadeOutDuration => base.FadeOutDuration;
    public override float TotalDuration => base.TotalDuration;

    public override void SetDefaults()
    {
        Projectile.width = 34;
        Projectile.height = 12;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = TrueMeleeNoSpeedDamageClass_Publicizer.Instance;
        Projectile.timeLeft = 360;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ArmorPenetration = 350258;
    }

    public override void SetVisualOffsets()
    {
        const int HalfSpriteWidth = ColdheartIcicle.SpriteWidth / 2;
        DrawOriginOffsetX = 0f;
        DrawOffsetX = Projectile.width / 2 - HalfSpriteWidth;
        DrawOriginOffsetY = Projectile.height / 2 - HalfSpriteWidth;
    }

    public override void AI()
    {
        Behavior();
        if (Projectile.OnOwnerClient)
        {
            if (IsRightClick)
            {
            }
            else
            {
            }
        }
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        float extraDamage = target.statLifeMax2 / 50f;
        modifiers.FinalDamage.Flat += extraDamage;
        CombatText.NewText(target.getRect(), Color.Cyan, extraDamage.ToString(), true, false);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.ForceCrit();
        if (target.type != NPCID.TargetDummy)
        {
            float extraDamage = target.lifeMax / 50;
            modifiers.FinalDamage.Flat += extraDamage;
        }
    }

    public void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    {
        baseDRModifier *= 0f;
        timedDRModifier *= 0f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        TOCombatTextUtils.ChangeHitNPCText(t => t.color = Color.Cyan);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        CollisionHelper.Collides(new RotatedRectangle(FloatRectangle.FromInnerPoint(Projectile.Center, 9f, 17.5f, 2.25f, 2.25f), Projectile.rotation), targetHitbox);

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(IsRightClick);
        writer.Write(Phase);
        writer.Write(SubPhase);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        IsRightClick = reader.ReadBoolean();
        Phase = reader.ReadInt32();
        SubPhase = reader.ReadInt32();
    }
}

public sealed class ColdheartIcicleDream : ModProjectile, IResourceLoader
{
    public static Asset<Texture2D> _orbTex;
    public static Texture2D OrbTex => _orbTex?.Value;

    public int Timer
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public int Timer2
    {
        get => (int)Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }

    public int TargetIndex = -1;
    public Entity Target
    {
        get => TargetIndex switch
        {
            >= 300 => Main.npc[TargetIndex - 300],
            >= 0 => Main.player[TargetIndex],
            _ => Projectile.Owner
        };
        set
        {
            if (value is NPC npc)
                TargetIndex = npc.whoAmI + 300;
            else if (value is Player player)
                TargetIndex = player.whoAmI;
            else
                TargetIndex = -1;
        }
    }

#if CELESS_DEV
    public const int LifeTime = 100;
    public const int LifeTime2 = 120;

    public enum BehaviorType
    {
        Normal,
        /// <summary>
        /// 幻梦。
        /// </summary>
        Dream,
        /// <summary>
        /// 启程。
        /// </summary>
        SetOut
    }

    public BehaviorType Behavior
    {
        get => (BehaviorType)(int)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    public float RealRotation
    {
        get => Projectile.rotation - MathHelper.PiOver2;
        set => Projectile.rotation = value + MathHelper.PiOver2;
    }

    #region 幻梦
    public float SnowflakeScale;
    public float SnowflakeRotation = MathHelper.PiOver2;
    public bool ShouldStopRotating;

    public Vector2 SnowflakeCenter
    {
        get
        {
            float ratio = Math.Clamp(Timer / 240f, 0f, 1f);
            return Projectile.Center + new PolarVector2(450f * ratio * (2f - ratio), RealRotation - MathHelper.PiOver2);
        }
    }
    #endregion 幻梦

    #region 启程
    public int SetOutPhase = 1;
    #endregion 启程
#else
    public const int LifeTime = 50;
#endif

    public override string Texture => CAMain.CalamityInvisibleProj;

    public override string LocalizationCategory => "DeveloperContents";

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.netImportant = true;
        Projectile.scale = 1f;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = LifeTime;
        Projectile.extraUpdates = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    /* 绘制六段类椭圆圆弧组成的雪花图案。
     * 当 amount > 1f 时，对椭圆进行缩放，以绘制中心曲边六边形。
     */

    public static class EllipseData
    {
        /// <summary>
        /// 椭圆半长轴缩放常数。
        /// </summary>
        public const float AMultiplier = 1.1075061f;
        /// <summary>
        /// 椭圆半短轴缩放常数。
        /// </summary>
        public const float BMultiplier = 0.7658784f;
        /// <summary>
        /// 参数方程最大角度偏移量。
        /// </summary>
        public const float MaxAngleOffset = 1.3127555f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
#if CELESS_DEV
        switch (Behavior)
        {
            case BehaviorType.Normal:
                NormalPreDraw();
                return false;
            case BehaviorType.Dream:
                Main.Rasterizer.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                if (Timer < 2200)
                {
                    float scale = 150f; //圆心半径
                    Vector2 origin = OrbTex.Size() * 0.5f;
                    for (int j = 0; j < 6; j++)
                    {
                        int localTimer = Timer > 2000 ? (2200 - Timer) / 2 : Math.Min(Timer - LifeTime2 * j - 235, LifeTime);
                        for (int i = 0; i <= localTimer * 10; i++)
                        {
                            float a = scale * EllipseData.AMultiplier; //椭圆半长轴
                            float b = scale * EllipseData.BMultiplier; //椭圆半短轴
                            float amount = i * 1.83f / (LifeTime * 10);
                            if (amount > 1f)
                            {
                                float multiplier = MathHelper.Lerp(0.4f, 1f, 2f - amount);
                                a *= multiplier;
                                b *= multiplier;
                            }
                            float angle = TOMathHelper.PiOver6 + TOMathHelper.PiOver3 * (j - 2);
                            Vector2 circleCenter = Projectile.Center + new PolarVector2(scale, angle);
                            (float sin, float cos) = MathF.SinCos(MathHelper.Lerp(-EllipseData.MaxAngleOffset, EllipseData.MaxAngleOffset, amount));
                            Vector2 position = circleCenter + new Vector2(a * cos, b * sin).RotatedBy(angle);
                            Main.spriteBatch.Draw(OrbTex, position - Main.screenPosition, null, Color.White with { A = 0 }, 0f, origin, 0.5f * TOMathHelper.Map(0f, 1.83f, 0.4f, 1.2f, amount), SpriteEffects.None, 0f);
                        }
                    }
                }
                ColdheartIcicleSnowflake.DrawSnowflake(SnowflakeCenter, SnowflakeScale, SnowflakeRotation);
                return false;
            case BehaviorType.SetOut:
                ColdheartIcicleSnowflake.DrawSnowflake(Projectile.Center, Projectile.scale, RealRotation);
                return false;
            default:
                return false;
        }
#else
        NormalPreDraw();
        return false;
#endif
        void NormalPreDraw()
        {
            Main.Rasterizer.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            if (Timer < 300)
            {
                float scale = 40f; //圆心半径
                Vector2 origin = OrbTex.Size() * 0.5f;
                for (int j = 0; j < 6; j++)
                {
                    int localTimer = Timer2 > 0 ? (100 - Timer2) / 2 : Math.Min(Timer - j * 10, LifeTime);
                    for (int i = 0; i <= localTimer * 10; i++)
                    {
                        float a = scale * EllipseData.AMultiplier; //椭圆半长轴
                        float b = scale * EllipseData.BMultiplier; //椭圆半短轴
                        float amount = i * 1.83f / (LifeTime * 10);
                        if (amount > 1f)
                        {
                            float multiplier = MathHelper.Lerp(0.4f, 1f, 2f - amount);
                            a *= multiplier;
                            b *= multiplier;
                        }
                        float angle = TOMathHelper.PiOver6 + TOMathHelper.PiOver3 * (j - 2);
                        Vector2 circleCenter = Projectile.Center + new PolarVector2(scale, angle);
                        (float sin, float cos) = MathF.SinCos(MathHelper.Lerp(-EllipseData.MaxAngleOffset, EllipseData.MaxAngleOffset, amount));
                        Vector2 position = circleCenter + new Vector2(a * cos, b * sin).RotatedBy(angle);
                        Main.spriteBatch.Draw(OrbTex, position - Main.screenPosition, null, Color.White with { A = 0 }, 0f, origin, 0.3f * TOMathHelper.Map(0f, 1.83f, 0.4f, 1.2f, amount), SpriteEffects.None, 0f);
                    }
                }
            }
        }
    }

    public override void AI()
    {
        Timer++;
#if CELESS_DEV
        switch (Behavior)
        {
            case BehaviorType.Normal:
                NormalAI();
                break;
            case BehaviorType.Dream:
                Projectile.Center = Projectile.Owner.Center;
                Projectile.timeLeft = LifeTime;
                float interpolation = TOMathHelper.ParabolicInterpolation(Timer > 2398 ? (2598 - Timer) / 200f : Math.Clamp(Timer / 240f, 0f, 1f));
                if (Main.rand.NextProbability((float)(Timer > 2398 ? (2598 - Timer) / 200f : Math.Clamp(Timer / 240f, 0f, 1f)) * 0.8f))
                    GeneralParticleHandler.SpawnParticle(new ColdheartIcicleGlowOrbParticle(SnowflakeCenter, Main.rand.NextVector2Circular(6f, 4f), Main.rand.NextFloat(0.9f, 1.4f), Main.rand.Next(40, 75), 0.8f, Main.rand.NextFloat(0.4f, 0.7f), Color.White, needed: true));
                SnowflakeScale = 0.7f * interpolation;
                SnowflakeRotation += 0.03f * interpolation * interpolation;
                if (!ShouldStopRotating)
                {
                    Projectile.rotation += TOMathHelper.PiOver3 / LifeTime2 * interpolation;
                    if (RealRotation > MathHelper.TwoPi - MathHelper.PiOver4)
                        Projectile.rotation -= TOMathHelper.PiOver3 / 130f * (float)TOMathHelper.Map(MathHelper.TwoPi + MathHelper.PiOver4, MathHelper.TwoPi + MathHelper.PiOver2, 0f, 1f, Projectile.rotation);
                    if (RealRotation > MathHelper.TwoPi)
                    {
                        RealRotation = 0f;
                        ShouldStopRotating = true;
                    }
                }
                if (Timer > 2598)
                    Projectile.Kill();
                break;
            case BehaviorType.SetOut:
                const int DepartX = 45768;
                const int DepartY = 7720;
                const int DestinationX = 46816;
                const int DestinationY = 7312;
                Vector2 depart = new(DepartX, DepartY);
                Vector2 destination = new(DestinationX, DestinationY);
                Projectile.timeLeft = LifeTime;
                float interpolation3 = 1f;
                float interpolation4 = 1f;
                switch (SetOutPhase)
                {
                    case 1:
                        if (Timer == 1)
                        {
                            Projectile.Center = destination;
                            Projectile.velocity = new PolarVector2(3f, MathHelper.PiOver2 + TOMathHelper.PiOver6);
                        }
                        float distance = Projectile.Distance(depart);
                        Projectile.Homing(depart, distance < 50f ? 1f : 0.075f, velocityOverride: Projectile.velocity.Length() * TOMathHelper.Map(0f, 1000f, 0.9943f, 1f, distance, true));
                        if (Projectile.Center == depart && Projectile.Owner.Center.X > DepartX - 88)
                        {
                            Timer = 0;
                            SetOutPhase = 2;
                        }
                        break;
                    case 2:
                        if (Timer > 100)
                        {
                            Timer = 0;
                            SetOutPhase = 3;
                        }
                        break;
                    case 3:
                        if (Timer == 1)
                        {
                            Projectile.Center = depart;
                            Projectile.velocity = new PolarVector2(0.02f, -MathHelper.PiOver2);
                        }
                        float distance2 = Projectile.Distance(destination);
                        Projectile.Homing(destination, distance2 < 50f ? 1f : distance2 < 200f ? 0.1f : 0.045f, velocityOverride: distance2 < 500f ? Projectile.velocity.Length() * TOMathHelper.Map(0f, 500f, 0.9947f, 1f, distance2, true) : Math.Min(Projectile.velocity.Length() + 0.01f, 1.5f));
                        if (Projectile.Center == destination && Projectile.Owner.Center.X > DestinationX + 256)
                        {
                            Timer = 0;
                            SetOutPhase = 4;
                        }
                        break;
                    case 4:
                        if (Timer > 200)
                            interpolation3 = TOMathHelper.ParabolicInterpolation((float)((700 - Timer) / 500f));
                        if (Timer > 100)
                            interpolation4 = TOMathHelper.ParabolicInterpolation((float)((700 - Timer) / 600f));
                        if (Timer == 500)
                        {
                            for (int i = 0; i < 2000; i++)
                                GeneralParticleHandler.SpawnParticle(new ColdheartIcicleGlowOrbParticle(Projectile.Owner.Center + new Vector2(Main.rand.NextFloat(-1600f, 1600f), -1300f - Main.rand.NextFloat(3700f)), Main.rand.NextVector2Circular(8f, 6f), Main.rand.NextFloat(0.25f, 0.7f), Main.rand.Next(370, 660), 0.8f, Main.rand.NextFloat(0.4f, 0.7f), Color.White, needed: true));
                        }
                        if (Timer > 700)
                            Projectile.Kill();
                        break;
                }
                Projectile.scale = 0.7f * interpolation3;
                Projectile.rotation += MathHelper.Lerp(0.0025f, 0.03f, interpolation4);
                break;
        }
#else
        NormalAI();
#endif
        void NormalAI()
        {
            Projectile.Center = Target.Center;
            Projectile.timeLeft = LifeTime;
            if (!Target.active || (Target is NPC npc && npc.life <= 0) || (Target is Player player && (player.dead || player.ghost)) || Timer2 > 0)
                Timer2++;
            if (Timer2 >= 100)
                Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? CanHitNPC(NPC target) => false;

    public override void SendExtraAI(BinaryWriter writer)
    {
#if CELESS_DEV
        writer.Write(SnowflakeScale);
        writer.Write(SnowflakeRotation);
        writer.Write(ShouldStopRotating);
        writer.Write(SetOutPhase);
#endif
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
#if CELESS_DEV
        SnowflakeScale = reader.ReadSingle();
        SnowflakeRotation = reader.ReadSingle();
        ShouldStopRotating = reader.ReadBoolean();
        SetOutPhase = reader.ReadInt32();
#endif
    }

    void IResourceLoader.PostSetupContent() => _orbTex = CalamityMod_Publicizer.Instance.Assets.Request<Texture2D>("Particles/GlowOrbParticle");
}

public sealed class ColdheartIcicleSnowflake : ModProjectile, ICAModProjectile
{
    public const int LeftTime = 360;

    public int Timer
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override string Texture => CAMain.CalamityInvisibleProj;

    public override string LocalizationCategory => "DeveloperContents";

    public override void SetDefaults()
    {
        Projectile.width = 80;
        Projectile.height = 80;
        Projectile.netImportant = true;
        Projectile.scale = 0.5f;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = LeftTime;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ArmorPenetration = 350258;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawSnowflake(Projectile.Center, Projectile.scale, Projectile.rotation);
        return false;
    }

    public static void DrawSnowflake(Vector2 center, float scale, float rotation)
    {
        Vector2 position = center - Main.screenPosition;
        Vector2 origin = new(CATextures.Scale1.Width / 2f, CATextures.Scale1.Height);
        for (int i = 0; i < 6; i++)
        {
            float angle = rotation + MathHelper.PiOver2 + TOMathHelper.PiOver3 * i;
            Main.EntitySpriteDraw(CATextures.Scale1, position, null, Color.White, angle, origin, scale, SpriteEffects.None, 0f);
        }
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, Color.White.ToVector3());

        Timer++;
        float interpolation = TOMathHelper.ParabolicInterpolation(Timer <= 10 ? Timer / 10f : Timer >= LeftTime - 25 ? (LeftTime - Timer) / 25f : 1f);
        Projectile.velocity.Modulus = TOMathHelper.Map(0, LeftTime, 18f, 30f, Timer) * interpolation;
        Projectile.scale = 0.5f * interpolation;
        Projectile.rotation += 0.05f * interpolation;
        GeneralParticleHandler.SpawnParticle(new ColdheartIcicleGlowOrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(0.8f, 1.3f), Main.rand.Next(40, 75), 0.8f, Main.rand.NextFloat(0.35f, 0.6f), Color.White, needed: true));
        if (Projectile.timeLeft <= 25)
            GeneralParticleHandler.SpawnParticle(new ColdheartIcicleGlowOrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f), Main.rand.NextFloat(0.8f, 1.3f), Main.rand.Next(40, 75), 0.8f, Main.rand.NextFloat(0.35f, 0.6f), Color.White, needed: true));
        if (Projectile.timeLeft <= 8)
        {
            GeneralParticleHandler.SpawnParticle(new ColdheartIcicleGlowOrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(6f, 6f), Main.rand.NextFloat(0.8f, 1.3f), Main.rand.Next(40, 75), 0.8f, Main.rand.NextFloat(0.35f, 0.6f), Color.White, needed: true));
            GeneralParticleHandler.SpawnParticle(new ColdheartIcicleGlowOrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(7f, 7f), Main.rand.NextFloat(0.8f, 1.3f), Main.rand.Next(40, 75), 0.8f, Main.rand.NextFloat(0.35f, 0.6f), Color.White, needed: true));
        }
    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        CollisionHelper.Collides(new Circle(Projectile.Center, 64f * Projectile.scale), targetHitbox);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.ForceCrit();
    }

    public void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    {
        baseDRModifier *= 0f;
        timedDRModifier *= 0f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
        TOCombatTextUtils.ChangeHitNPCText(t => t.color = TOMain.CelestialColor);
        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == target)
                return;
        }
        Projectile.NewProjectileAction<ColdheartIcicleDream>(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, 0, 0, Projectile.owner, p =>
        {
            p.Center = target.Center;
            p.rotation = Projectile.rotation;
            ColdheartIcicleDream modP = p.GetModProjectile<ColdheartIcicleDream>();
            modP.Target = target;
        });
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == target)
                return;
        }
        SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
        Projectile.NewProjectileAction<ColdheartIcicleDream>(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, 0, 0, Projectile.owner, p =>
        {
            p.Center = target.Center;
            p.ai[1] = Projectile.rotation;
            ColdheartIcicleDream modP = p.GetModProjectile<ColdheartIcicleDream>();
            modP.Target = target;
        });
    }
}

public sealed class ColdheartIcicleSnowFlake_GlobalNPC : CAGlobalNPCBehavior
{
    public override void OnKill(NPC npc)
    {
        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == npc)
                dream.Timer2++;
        }
    }
}

public sealed class ColdheartIcicleIceRain : ModProjectile
{
    public override string Texture => CAMain.CATexturePath + "Touhou/Ice1";

    public override string LocalizationCategory => "DeveloperContents";
}
#endregion 弹幕

#region 粒子
public sealed class ColdheartIcicleGlowOrbParticle : GlowOrbParticle
{
    public float InitialScale;
    public float GravityMultiplier;
    public float LifeEndRatio;
    public new bool AffectedByGravity => GravityMultiplier > 0f;

    public ColdheartIcicleGlowOrbParticle(Vector2 relativePosition, Vector2 velocity, float gravityMultiplier, int lifetime, float lifeEndRatio, float scale, Color color, bool AddativeBlend = true, bool needed = false, bool GlowCenter = true) : base(relativePosition, velocity, true, lifetime, scale, color, AddativeBlend, needed, GlowCenter)
    {
        InitialScale = scale;
        GravityMultiplier = gravityMultiplier;
        LifeEndRatio = lifeEndRatio;
    }

    public override void Update()
    {
        if (LifetimeCompletion > LifeEndRatio)
        {
            float interpolation = TOMathHelper.ParabolicInterpolation(1f - (LifetimeCompletion - LifeEndRatio) / (1f - LifeEndRatio));
            fadeOut = interpolation;
            Scale = InitialScale * interpolation;
        }
        Color = Color.Lerp(InitialColor, InitialColor * 0.2f, MathF.Pow(LifetimeCompletion, 3));
        Velocity *= 0.98f;
        if (Velocity.Y < 12f * GravityMultiplier && AffectedByGravity)
            Velocity.Y += 0.25f * GravityMultiplier;
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
    }
}
#endregion 粒子

#region 获取
public sealed class ColdheartIcicle_Loot : CAGlobalNPCBehavior
{
    public override bool ShouldProcess => false;

    public override void ModifyGlobalLoot(GlobalLoot globalLoot)
    {
        globalLoot.Add(ItemDropRule.ByCustomCondition(i => !i.IsInSimulation && i.npc.value > 0f && i.player.ZoneSnow && !Main.snowMoon, null, null, ModContent.ItemType<ColdheartIcicle>(), 400000));
        globalLoot.Add(ItemDropRule.ByCustomCondition(i => !i.IsInSimulation && i.npc.FrostMoonEnemy && Main.snowMoon, null, null, ModContent.ItemType<ColdheartIcicle>(), 20000));
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.ModNPC is DevourerofGodsHead)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ColdheartIcicle>(), 500));
    }
}

public sealed class ColdheartIcicle_Starter : CAPlayerBehavior
{
    public override bool ShouldProcess => false;

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        if (!mediumCoreDeath && ColdheartIcicle.IsLegendOwner(Player))
            yield return Item.CreateItem<ColdheartIcicle>();
    }
}
#endregion 获取