namespace Transoceanic.DataStructures.Particles;

public class CustomSpriteParticle : Particle
{
    public Texture2D Texture;
    public bool _useAdditiveBlend;
    public float MaxGravity = 0f;
    public float Opacity = 1f;

    public Func<CustomSpriteParticle, Rectangle> CustomFindFrameAction;
    public Action<CustomSpriteParticle> CustomUpdateAction;

    public const int ExtraDataSlots = 8;
    public Union32[] ExtraData = new Union32[ExtraDataSlots];

    public override bool AutoLoadTexture => false;
    public override string TexturePath => TOSharedData.InvisibleProj;
    public override bool AutoKillByLifeTime => true;
    public override DrawBlendMode BlendMode => _useAdditiveBlend ? DrawBlendMode.AdditiveBlend : DrawBlendMode.AlphaBlend;

    public CustomSpriteParticle(Vector2 relativePosition, Vector2 velocity, int lifetime, Texture2D texture, float scale, Color color, float grav = 0f, bool useAdditiveBlend = true, bool important = false,
        Func<CustomSpriteParticle, Rectangle> customFindFrameAction = null, Action<CustomSpriteParticle> customUpdateAction = null)
    {
        Center = relativePosition;
        MaxGravity = grav;
        Velocity = velocity;
        Scale = scale;
        Lifetime = lifetime;
        _useAdditiveBlend = useAdditiveBlend;
        Important = important;
        Color = color;
        Texture = texture;
        CustomFindFrameAction = customFindFrameAction;
        CustomUpdateAction = customUpdateAction;
    }

    public CustomSpriteParticle(Vector2 relativePosition, Vector2 velocity, int lifetime, string texturePath, float scale, Color color, float grav = 0f, bool useAddativeBlend = true, bool important = false,
        Func<CustomSpriteParticle, Rectangle> customFindFrameAction = null, Action<CustomSpriteParticle> customUpdateAction = null)
        : this(relativePosition, velocity, lifetime, ModContent.Request<Texture2D>(texturePath).Value, scale, color, grav, useAddativeBlend, important, customFindFrameAction, customUpdateAction)
    { }

    public override void Update()
    {
        Center += Velocity;

        if (Timer > Lifetime - 20)
        {
            Scale *= 0.9f;
            Opacity *= 0.9f;
        }

        Velocity *= 0.85f;
        if (MaxGravity != 0f)
        {
            if (Velocity.Length() < MaxGravity)
            {
                Velocity.X *= 0.94f;
                Velocity.Y += MaxGravity / 10f;
            }
        }

        CustomUpdateAction?.Invoke(this);
    }

    public override Rectangle? GetFrame() => CustomFindFrameAction?.Invoke(this);

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawFromCenter(Texture, Center - Main.screenPosition, Color.Lerp(Color.Transparent, Color, Opacity), GetFrame(), Rotation, 1f, SpriteEffects.None);
        return false;
    }
}