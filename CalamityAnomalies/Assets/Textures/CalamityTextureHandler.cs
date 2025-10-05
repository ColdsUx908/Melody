using CalamityAnomalies.Publicizer.CalamityMod;

namespace CalamityAnomalies.Assets.Textures;

public sealed class CalamityTextureHandler : IResourceLoader
{
    public static Asset<Texture2D> _glowOrbParticle;
    public static Texture2D GlowOrbParticle => _glowOrbParticle?.Value;

    void IResourceLoader.PostSetupContent()
    {
        _glowOrbParticle = CalamityMod_Publicizer.Instance.Assets.Request<Texture2D>("Particles/GlowOrbParticle");
    }

    void IResourceLoader.OnModUnload()
    {
        _glowOrbParticle = null;
    }
}