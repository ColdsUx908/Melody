namespace Transoceanic.DataStructures.Particles;

public class BloomParticle : Particle
{
    [LoadTexture(ParticleHandler.BaseParticleTexturePath + "BloomCircleLarge")]
    private static Asset<Texture2D> _bloomCircleLarge;
    public static Texture2D BloomCircleLarge => _bloomCircleLarge.Value;

    public override string TexturePath => ParticleHandler.BaseParticleTexturePath + "BloomCircle";
    public override bool AutoKillByLifeTime => true;
    public override BlendState DrawBlendState => BlendState.Additive;

    public float OriginalScale;
    public float FinalScale;
    public float Opacity;
    public Color BaseColor;
    public bool Fade;
    public bool UseLargeTexture;

    public BloomParticle(Vector2 center, Vector2 velocity, Color color, float originalScale, float finalScale, int lifeTime, bool fade = true, bool useLargeTexture = false)
    {
        Center = center;
        Velocity = velocity;
        BaseColor = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale;
        Lifetime = lifeTime;
        Fade = fade;
        UseLargeTexture = useLargeTexture;
    }

    public override void Update()
    {
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, TOMathUtils.Interpolation.ExponentialEaseOut(LifetimeCompletion, 4f));
        if (Fade)
            Opacity = 1f - TOMathUtils.Interpolation.SineEaseIn(LifetimeCompletion);

        Color = BaseColor * (Fade ? Opacity : 1);
        Lighting.AddLight(Center, Color.R / 255f, Color.G / 255f, Color.B / 255f);
    }

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawFromCenter(UseLargeTexture ? BloomCircleLarge : Texture, Center - Main.screenPosition, null, Color * (Fade ? Opacity : 1), scale: Scale);
        return false;
    }
}
