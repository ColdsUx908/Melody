namespace Transoceanic.DataStructures.Particles;

public class PulseRing : Particle
{
    public override string TexturePath => ParticleHelper.BaseParticleTexturePath + "HollowCircleHardEdge";
    public override bool AutoKillByLifeTime => true;
    public override bool UseCustomDraw => true;
    public override DrawBlendMode BlendMode => DrawBlendMode.AdditiveBlend;

    [ParticleTextureAsset]
    private static Asset<Texture2D> _textureAsset;
    public static Texture2D Texture => _textureAsset?.Value;
    public const float TextureRadius = 78f;

    private float OriginalScale;
    private float FinalScale;
    private float opacity;
    private Vector2 Squish;
    private Color BaseColor;

    public PulseRing(Vector2 center, Vector2 velocity, Color color, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime)
    {
        Center = center;
        Velocity = velocity;
        BaseColor = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale;
        Lifetime = lifeTime;
        Squish = squish;
        Rotation = rotation;
    }

    public override void Update()
    {
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, TOMathHelper.ExponentialEaseOut(LifetimeCompletion, 4f));
        opacity = 1f - TOMathHelper.SineEaseIn(LifetimeCompletion);

        Color = BaseColor * opacity;
        Lighting.AddLight(Center, Color.R / 255f, Color.G / 255f, Color.B / 255f);
        Velocity *= 0.95f;
    }

    public override void CustomDraw(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(Texture, Center - Main.screenPosition, null, Color * opacity, Rotation, Texture.Size() / 2f, Scale * Squish, SpriteEffects.None, 0);

}
