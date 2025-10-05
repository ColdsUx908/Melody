using CalamityMod.Particles;

namespace CalamityAnomalies.GameContents;

public sealed class FadingGlowOrbParticle : GlowOrbParticle
{
    public float InitialScale;
    public float GravityMultiplier;
    public float LifeEndRatio;
    public new bool AffectedByGravity => GravityMultiplier > 0f;

    public FadingGlowOrbParticle(Vector2 relativePosition, Vector2 velocity, float gravityMultiplier, int lifetime, float lifeEndRatio, float scale, Color color, bool AddativeBlend = true, bool needed = false, bool GlowCenter = true) : base(relativePosition, velocity, true, lifetime, scale, color, AddativeBlend, needed, GlowCenter)
    {
        InitialScale = scale;
        GravityMultiplier = gravityMultiplier;
        LifeEndRatio = lifeEndRatio;
    }

    public override void Update()
    {
        if (LifetimeCompletion > LifeEndRatio)
        {
            float interpolation = TOMathHelper.ParabolicInterpolation(1f - (LifetimeCompletion - LifeEndRatio) / (1f - LifeEndRatio));
            fadeOut = interpolation;
            Scale = InitialScale * interpolation;
        }
        Color = Color.Lerp(InitialColor, InitialColor * 0.2f, MathF.Pow(LifetimeCompletion, 3));
        Velocity *= 0.98f;
        if (Velocity.Y < 12f * GravityMultiplier && AffectedByGravity)
            Velocity.Y += 0.25f * GravityMultiplier;
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
    }
}