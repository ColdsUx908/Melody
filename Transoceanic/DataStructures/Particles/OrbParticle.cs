namespace Transoceanic.DataStructures.Particles;

public class OrbParticle : Particle
{
    public override bool AutoKillByLifeTime => true;
    public override DrawBlendMode BlendMode => UseAdditiveBlend ? DrawBlendMode.AdditiveBlend : DrawBlendMode.AlphaBlend;

    [ParticleTextureAsset]
    private static Asset<Texture2D> _textureAsset;
    public static Texture2D Texture => _textureAsset?.Value;

    public Color InitialColor;
    public bool UseAdditiveBlend = true;
    public float FadeOut = 1;
    public bool GlowCenter;
    public float InitialScale;
    public float GravityMultiplier;
    public float LifeEndRatio;
    public bool AffectedByGravity => GravityMultiplier > 0f;

    public OrbParticle(Vector2 center, Vector2 velocity, int lifetime, float scale, Color color, float gravityMultiplier = 0f, float lifeEndRatio = 0f, bool useAdditiveBlend = true, bool important = false, bool glowCenter = true)
    {
        Center = center;
        Velocity = velocity;
        GravityMultiplier = gravityMultiplier;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
        UseAdditiveBlend = useAdditiveBlend;
        Important = important;
        GlowCenter = glowCenter;
        InitialScale = scale;
        LifeEndRatio = lifeEndRatio;
    }

    public override void Update()
    {
        if (LifetimeCompletion > LifeEndRatio)
        {
            float interpolation = TOMathUtils.QuadraticEaseOut(1f - (LifetimeCompletion - LifeEndRatio) / (1f - LifeEndRatio));
            FadeOut = interpolation;
            Scale = InitialScale * interpolation;
        }
        Color = Color.Lerp(InitialColor, InitialColor * 0.2f, MathF.Pow(LifetimeCompletion, 3));
        Velocity *= 0.98f;
        if (Velocity.Y < 12f * GravityMultiplier && AffectedByGravity)
            Velocity.Y += 0.25f * GravityMultiplier;
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
    }

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Texture;
        Vector2 scale = new(Scale);

        spriteBatch.DrawFromCenter(texture, Center - Main.screenPosition, Color, null, Rotation, scale);
        if (GlowCenter)
            spriteBatch.DrawFromCenter(texture, Center - Main.screenPosition, Color.White * FadeOut, null, Rotation, scale * new Vector2(0.5f, 0.5f));

        return false;
    }
}