using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Particles;
using Transoceanic.Data.Geometry;

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
    private RainbowDirectionalPulseRing _particle;

    public const float OriginalRadius = 78f;

    public override string Texture => CAMain.CalamityInvisibleProj;
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

        switch (Timer1++)
        {
            case 0:
                GeneralParticleHandler.SpawnParticle(_particle = new(Jewel.Center, Vector2.Zero, new Vector2(1f), 0, 0.05f, 45f, 150));
                break;
        }
    }

    public override bool? CanHitNPC(NPC target) =>
        target.Ocean().TryGetMaster(NPCID.KingSlime, out NPC master)
        && master == Master
        && _slimeTypesToClear.Contains(target.type);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(_particle.Position, OriginalRadius * _particle.Scale).Collides(targetHitbox);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SetInstantKillBetter(target);

    public sealed class RainbowDirectionalPulseRing : DirectionalPulseRing
    {
        public RainbowDirectionalPulseRing(Vector2 position, Vector2 velocity, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime) : base(position, velocity, Color.White, squish, rotation, originalScale, finalScale, lifeTime) { }

        public override void Update()
        {
            base.Update();
            Color = Color.LerpMany(Color.RainbowColors, TOMathHelper.ParabolicInterpolation(LifetimeCompletion));
        }
    }

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
        _slimeTypesToClear.Clear();
    }
}
