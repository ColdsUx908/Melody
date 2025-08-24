namespace CalamityAnomalies.Assets.Textures;

public sealed class CATextures : IResourceLoader
{
    public static Asset<Texture2D> _ice1;
    public static Texture2D Ice1 => _ice1?.Value;

    public static Asset<Texture2D> _ice2;
    public static Texture2D Ice2 => _ice2?.Value;

    public static Asset<Texture2D> _ice3;
    public static Texture2D Ice3 => _ice3?.Value;

    public static Asset<Texture2D> _ice4;
    public static Texture2D Ice4 => _ice4?.Value;

    public static Asset<Texture2D> _ice5;
    public static Texture2D Ice5 => _ice5?.Value;

    public static Asset<Texture2D> _ice6;
    public static Texture2D Ice6 => _ice6?.Value;

    public static Asset<Texture2D> _ice7;
    public static Texture2D Ice7 => _ice7?.Value;

    public static Asset<Texture2D> _ice8;
    public static Texture2D Ice8 => _ice8?.Value;

    public static Asset<Texture2D> _scale1;
    public static Texture2D Scale1 => _scale1?.Value;

    public static Asset<Texture2D> _anomalyModeIndicator;
    public static Texture2D AnomalyModeIndicator => _anomalyModeIndicator?.Value;

    [LoadPriority(1e5)]
    void IResourceLoader.PostSetupContent()
    {
        _ice1 = CAUtils.RequestTexture("Touhou/Ice1");
        _ice2 = CAUtils.RequestTexture("Touhou/Ice2");
        _ice3 = CAUtils.RequestTexture("Touhou/Ice3");
        //_ice4 = CAUtils.RequestTexture("Touhou/Ice4");
        //_ice5 = CAUtils.RequestTexture("Touhou/Ice5");
        _ice6 = CAUtils.RequestTexture("Touhou/Ice6");
        _ice7 = CAUtils.RequestTexture("Touhou/Ice7");
        _ice8 = CAUtils.RequestTexture("Touhou/Ice8");
        _scale1 = CAUtils.RequestTexture("Touhou/Scale1");
        _anomalyModeIndicator = CAUtils.RequestTexture("UI/AnomalyModeIndicator");
    }

    void IResourceLoader.OnModUnload()
    {
        _ice1 = null;
        _ice2 = null;
        _ice3 = null;
        _ice4 = null;
        _ice5 = null;
        _ice6 = null;
        _ice7 = null;
        _ice8 = null;
        _scale1 = null;
        _anomalyModeIndicator = null;
    }
}
