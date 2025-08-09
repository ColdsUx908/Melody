using CalamityAnomalies.Items;
using CalamityAnomalies.Publicizer.CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Rogue;
using Terraria.GameContent.ItemDropRules;

namespace CalamityAnomalies.DeveloperContents;

public sealed class ColdheartIcicle : ModItem
{
    public const int SpriteWidth = 24;
    public const string TexturePath = "CalamityMod/Items/ColdheartIcicle";

    public override string Texture => TexturePath;

    public override string LocalizationCategory => "DeveloperContents";

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
        Item.Calamity().devItem = true;
    }
}

public sealed class ColdheartIcicle_ItemLoot : CAGlobalNPCBehavior
{
    public override void ModifyGlobalLoot(GlobalLoot globalLoot)
    {
    }
}

public sealed class ColdheartIcicleProj : BaseShortswordProjectile, ILocalizedModType
{
    public override string Texture => ColdheartIcicle.TexturePath;

    public new string LocalizationCategory => "DeveloperContents";

    public override void SetDefaults()
    {
        Projectile.width = 34;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.scale = 1f;
        Projectile.DamageType = TrueMeleeNoSpeedDamageClass_Publicizer.Instance;
        Projectile.timeLeft = 360;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
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

        if (Main.rand.NextBool(5))
        {
            Dust.NewDustAction(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.IceRod, action: d =>
            {
                d.noGravity = true;
                if (Main.rand.NextBool(2))
                    d.color = TOMain.CelestialColor;
            });
        }
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        int extraDamage = target.statLifeMax2 / 50;
        target.statLife -= extraDamage;
        CombatText.NewText(target.getRect(), Color.Cyan, extraDamage.ToString(), true, false);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (target.type != NPCID.TargetDummy)
        {
            int extraDamage = target.lifeMax / 50;
            target.life -= extraDamage;
            CombatText.NewText(target.getRect(), Color.Cyan, extraDamage.ToString(), true, false);
        }
        target.checkDead();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float collisionPoint = 0f;
        Vector2 center = Projectile.Center;
        Vector2 direction = (center - Owner.MountedCenter).SafelyNormalized;
        return TOCollisionUtils.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), center - direction * 8.5f, center + direction * 17f, 12f, ref collisionPoint);
    }
}
