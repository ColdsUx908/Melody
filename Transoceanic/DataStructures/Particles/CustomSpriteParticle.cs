using Transoceanic.DataStructures.Assets;

namespace Transoceanic.DataStructures.Particles;

public class CustomSpriteParticle : Particle
{
    public new Texture2D Texture;
    public float MaxGravity = 0f;
    public float Opacity = 1f;

    public Func<CustomSpriteParticle, Rectangle?> CustomGetFrameAction;
    public Action<CustomSpriteParticle> CustomUpdateAction;

    public const int ExtraDataSlots = 8;
    public Union32[] ExtraData = new Union32[ExtraDataSlots];

    public override bool AutoLoadTexture => false;
    public override string TexturePath => TOTextures.InvisibleTexturePath;
    public override bool AutoKillByLifeTime => true;
    public override BlendState DrawBlendState { get; }

    public CustomSpriteParticle(Vector2 relativePosition, Vector2 velocity, int lifetime, Texture2D texture, float scale, Color color, float maxGravity = 0f, BlendState blendState = null,
        Func<CustomSpriteParticle, Rectangle?> customGetFrameAction = null, Action<CustomSpriteParticle> customUpdateAction = null)
    {
        Center = relativePosition;
        MaxGravity = maxGravity;
        Velocity = velocity;
        Scale = scale;
        Lifetime = lifetime;
        DrawBlendState = blendState ?? BlendState.AlphaBlend;
        Color = color;
        Texture = texture;
        CustomGetFrameAction = customGetFrameAction;
        CustomUpdateAction = customUpdateAction;
    }

    public CustomSpriteParticle(Vector2 relativePosition, Vector2 velocity, int lifetime, string texturePath, float scale, Color color, float maxGravity = 0f, BlendState blendState = null,
        Func<CustomSpriteParticle, Rectangle?> customGetFrameAction = null, Action<CustomSpriteParticle> customUpdateAction = null)
        : this(relativePosition, velocity, lifetime, ModContent.Request<Texture2D>(texturePath).Value, scale, color, maxGravity, blendState, customGetFrameAction, customUpdateAction)
    { }

    public override void Update()
    {
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

    public override bool PreDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawFromCenter(Texture, Center - Main.screenPosition, CustomGetFrameAction?.Invoke(this), Color.Lerp(Color.Transparent, Color, Opacity), Rotation, 1f, SpriteEffects.None);
        return false;
    }
}