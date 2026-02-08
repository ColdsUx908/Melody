namespace Transoceanic.DataStructures.Particles;

public class BloomParticle1 : Particle
{
    public override string TexturePath => ParticleHelper.BaseParticleTexturePath + "BloomCircle";
    public override bool AutoKillByLifeTime => true;
    public override bool UseCustomDraw => true;
    public override DrawBlendMode BlendMode => DrawBlendMode.AdditiveBlend;

    [ParticleTextureAsset]
    private static Asset<Texture2D> _textureAsset;
    public static Texture2D Texture => _textureAsset?.Value;

    public float OriginalScale;
    public float FinalScale;
    public float Opacity;
    public Color BaseColor;
    public bool Fade;

    public BloomParticle1(Vector2 center, Vector2 velocity, Color color, float originalScale, float finalScale, int lifeTime, bool fade = true)
    {
        Center = center;
        Velocity = velocity;
        BaseColor = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale;
        Lifetime = lifeTime;
        Fade = fade;
    }

    public override void Update()
    {
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, TOMathHelper.ExponentialEaseOut(LifetimeCompletion, 4f));
        if (Fade)
            Opacity = 1f - TOMathHelper.SineEaseIn(LifetimeCompletion);

        Color = BaseColor * (Fade ? Opacity : 1);
        Lighting.AddLight(Center, Color.R / 255f, Color.G / 255f, Color.B / 255f);
    }

    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawFromCenter(Texture, Center - Main.screenPosition, Color * (Fade ? Opacity : 1), scale: Scale);
    }

}
