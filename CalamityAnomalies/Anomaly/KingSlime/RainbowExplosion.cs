using CalamityMod.NPCs.NormalNPCs;
using Transoceanic.Framework.Helpers.AbstractionHandlers;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class RainbowExplosion : CAModProjectile, IResourceLoader
{
    private static List<int> _slimeTypesToClear;

    public NPC Jewel
    {
        get
        {
            int temp = (int)Projectile.ai[0];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.ai[0] = value?.whoAmI ?? Main.maxNPCs;
    }
    public NPC Master
    {
        get
        {
            int temp = (int)Projectile.ai[1];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.ai[1] = value?.whoAmI ?? Main.maxNPCs;
    }

    public float LifeCompletion => Timer1 / 150f;

    public override string Texture => ParticleHandler.BaseParticleTexturePath + "HollowCircleHardEdgeHD";
    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 100;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        if (Jewel is not null && Jewel.active && Jewel.ModNPC is KingSlimeJewelRainbow)
            Projectile.Center = Jewel.Center;

        Timer1++;

        Projectile.scale = MathHelper.Lerp(0f, 4f, TOMathUtils.ExponentialEaseOut(LifeCompletion, 4f)); //有效杀伤半径：185格
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        ParticleHandler.EnterDrawRegion_AdditiveBlend(spriteBatch);
        spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, Color.LerpMany(Color.RainbowColors, TOMathUtils.QuadraticEaseOut(LifeCompletion)), null, 0f, Projectile.scale);
        ParticleHandler.ExitParticleDrawRegion(spriteBatch);
        return false;
    }

    public override bool? CanHitNPC(NPC target) =>
          target.Ocean.TryGetMaster(NPCID.KingSlime, out NPC master)
        && master == Master
        && _slimeTypesToClear.Contains(target.type);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, PulseRing.TextureRadiusHD * Projectile.scale).Collides(targetHitbox);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SetInstantKillBetter(target);

    void IResourceLoader.PostSetupContent()
    {
        _slimeTypesToClear =
        [
            NPCID.GreenSlime,
            NPCID.BlueSlime,
            NPCID.RedSlime,
            NPCID.PurpleSlime,
            NPCID.YellowSlime,
            NPCID.IceSlime,
            NPCID.JungleSlime,
            NPCID.SlimeSpiked,
            NPCID.SpikedIceSlime,
            NPCID.SpikedJungleSlime,
            NPCID.UmbrellaSlime,
            NPCID.CorruptSlime,
            NPCID.Crimslime,
            NPCID.ShimmerSlime,
            NPCID.IlluminantSlime,
            ModContent.NPCType<EbonianBlightSlime>(),
            ModContent.NPCType<CrimulanBlightSlime>(),
        ];
    }

    void IResourceLoader.OnModUnload()
    {
        _slimeTypesToClear = null;
    }
}
