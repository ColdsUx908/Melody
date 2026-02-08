namespace Transoceanic.Framework.Abstractions;

public abstract class Particle
{
    public enum DrawBlendMode
    {
        AlphaBlend,
        NonPremultiplied,
        AdditiveBlend,
    }

    /// <summary>
    /// The ID of the particle type as registered by the general particle handler. This is set automatically when the particle handler loads
    /// </summary>
    public int Type;
    /// <summary>
    /// The amount of frames this particle has existed for. You shouldn't have to touch this manually.
    /// </summary>
    public int Time;

    /// <summary>
    /// 如果设置为 <see langword="true"/>，则该粒子在 <see cref="ParticleHelper.SpawnParticle(Particle)"/> 中被视为重要粒子，即使粒子数目已达上限也会被生成。
    /// </summary>
    public bool Important;

    public virtual bool AutoKillByLifeTime => false;
    public int Lifetime = 0;

    public float LifetimeCompletion => Lifetime != 0 ? Time / (float)Lifetime : 0;

    public Vector2 Center;
    public Vector2 Velocity;

    public Vector2 Origin;
    public Color Color;
    public float Rotation;
    public float Scale;

    public virtual int FrameVariants => 1;
    public int Variant = 0;
    public virtual string TexturePath => "";
    /// <summary>
    /// Set this to true to disable default particle drawing, thus calling Particle.CustomDraw() instead.
    /// </summary>
    public virtual bool UseCustomDraw => false;
    /// <summary>
    /// Use this method if you want to handle the particle drawing yourself. Only called if Particle.UseCustomDraw is set to true.
    /// </summary>
    public virtual void CustomDraw(SpriteBatch spriteBatch) { }

    public virtual void CustomDraw(SpriteBatch spriteBatch, Vector2 basePosition) { }
    /// <summary>
    /// Called for every update of the particle handler.
    /// The particle's velocity gets automatically added to its position, and its time automatically increases.
    /// </summary>
    public virtual void Update() { }

    public virtual DrawBlendMode BlendMode => DrawBlendMode.AlphaBlend;

    /// <summary>
    /// Removes the particle from the handler
    /// </summary>
    public void Kill() => ParticleHelper.RemoveParticle(this);
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class ParticleTextureAssetAttribute : Attribute;
