namespace Transoceanic.DataStructures.Particles;

public class PointingParticle : Particle
{
    public override bool AutoKillByLifeTime => true;
    public override DrawBlendMode BlendMode => _useAdditiveBlend ? DrawBlendMode.AdditiveBlend : DrawBlendMode.AlphaBlend;


    [ParticleTextureAsset]
    private static Asset<Texture2D> _textureAsset;
    public static Texture2D Texture => _textureAsset?.Value;

    public Color InitialColor;
    public bool AffectedByGravity;
    public bool _useAdditiveBlend = true;

    public PointingParticle(Vector2 center, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, bool useAddativeBlend = true, bool affectedByLight = false)
    {
        Center = center;
        Velocity = velocity;
        AffectedByGravity = affectedByGravity;
        Scale = scale;
        Lifetime = lifetime;
        Color = InitialColor = color;
        _useAdditiveBlend = useAddativeBlend;
        AffectedByLight = affectedByLight;
    }

    public override void Update()
    {
        Scale *= 0.95f;
        Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
        if (AffectedByLight)
            Color = Lighting.GetColor(Center.ToTileCoordinates()).MultiplyRGBA(Color);

        Velocity *= 0.95f;
        if (Velocity.Length() < 12f && AffectedByGravity)
        {
            Velocity.X *= 0.94f;
            Velocity.Y += 0.25f;
        }
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
    }

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        Vector2 scale = new Vector2(0.5f, 1.6f) * Scale;

        spriteBatch.DrawFromCenter(Texture, Center - Main.screenPosition, Color, null, Rotation, scale, 0, 0f);
        spriteBatch.DrawFromCenter(Texture, Center - Main.screenPosition, Color, null, Rotation, scale * new Vector2(0.45f, 1f), 0, 0f);

        return false;
    }
}
