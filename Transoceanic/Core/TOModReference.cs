namespace Transoceanic.Core;

/// <summary>
/// 检测特定Mod是否已加载，可获取实例。
/// </summary>
public record TOModReferenceContainer
{
    public TOModReferenceContainer(string name)
    {
        Name = name;
        IsLoaded = ModLoader.TryGetMod(Name, out Mod mod);
        Mod = mod;
    }

    public static readonly TOModReferenceContainer Empty = new("");

    /// <summary>
    /// Mod内部名。
    /// </summary>
    public string Name { get; private set; } = "";

    /// <summary>
    /// Mod是否已加载。
    /// </summary>
    public bool IsLoaded { get; private set; } = false;

    /// <summary>
    /// Mod实例。若 <see cref="IsLoaded"/> 为false，值为null。
    /// </summary>
    public Mod Mod { get; private set; } = null;
}

public sealed class TOModReferences : IResourceLoader
{
    //按字母顺序排列
    public static TOModReferenceContainer CA { get; private set; }
    public static TOModReferenceContainer Calamity { get; private set; }
    public static TOModReferenceContainer CalDemutation { get; private set; }
    public static TOModReferenceContainer CALegacy { get; private set; }
    public static TOModReferenceContainer CalFables { get; private set; }
    public static TOModReferenceContainer CalOverhaul { get; private set; }
    public static TOModReferenceContainer CalSpear { get; private set; }
    public static TOModReferenceContainer Catalyst { get; private set; }
    public static TOModReferenceContainer Example { get; private set; }
    public static TOModReferenceContainer FargoDLC { get; private set; }
    public static TOModReferenceContainer FargoMutant { get; private set; }
    public static TOModReferenceContainer FargoSouls { get; private set; }
    public static TOModReferenceContainer Gensokyo { get; private set; }
    public static TOModReferenceContainer HollowKnight { get; private set; }
    public static TOModReferenceContainer HuntoftheOldGod { get; private set; }
    public static TOModReferenceContainer Infernum { get; private set; }
    public static TOModReferenceContainer InfiniteAddPoint { get; private set; }
    public static TOModReferenceContainer Luiafk { get; private set; }
    public static TOModReferenceContainer Luminance { get; private set; }
    public static TOModReferenceContainer LunarVeil { get; private set; }
    public static TOModReferenceContainer MEAC { get; private set; }
    public static TOModReferenceContainer Nycro { get; private set; }
    public static TOModReferenceContainer Overhaul { get; private set; }
    public static TOModReferenceContainer QoT { get; private set; }
    public static TOModReferenceContainer Radiance { get; private set; }
    public static TOModReferenceContainer Redemption { get; private set; }
    public static TOModReferenceContainer Remnants { get; private set; }
    public static TOModReferenceContainer Spear { get; private set; }
    public static TOModReferenceContainer Spirit { get; private set; }
    public static TOModReferenceContainer StarlightRiver { get; private set; }
    public static TOModReferenceContainer StarsAbove { get; private set; }
    public static TOModReferenceContainer StoryofRedCloud { get; private set; }
    public static TOModReferenceContainer Thorium { get; private set; }
    public static TOModReferenceContainer ThoriumRework { get; private set; }
    public static TOModReferenceContainer WrathoftheGods { get; private set; }

    [LoadPriority(1000)]
    void IResourceLoader.PostSetupContent()
    {
        CA = new("CalamityAnomalies");
        Calamity = new("CalamityMod");
        CalDemutation = new("CalamityDemutation");
        CALegacy = new("ACalTweak");
        CalFables = new("CalamityFables");
        CalOverhaul = new("CalamityOverhaul");
        CalSpear = new("CalamityThrowingSpear");
        Catalyst = new("Catalyst");
        Example = new("ExampleMod");
        FargoDLC = new("FargowiltasCrossMod");
        FargoMutant = new("Fargowiltas");
        FargoSouls = new("FargowiltasSouls");
        Gensokyo = new("Gensokyo");
        HollowKnight = new("TheKnightMod");
        HuntoftheOldGod = new("CalamityHunt");
        Infernum = new("InfernumMode");
        InfiniteAddPoint = new("InfiniteAddPointSystem");
        Luiafk = new("miningcracks_take_on_luiafk");
        Luminance = new("Luminance");
        LunarVeil = new("Stellamod");
        MEAC = new("MEAC");
        Nycro = new("EfficientNohits");
        Overhaul = new("TerrariaOverhaul");
        QoT = new("ImproveGame");
        Radiance = new("Radiance");
        Redemption = new("Redemption");
        Remnants = new("Remnants");
        Spear = new("SpearToJavelin");
        Spirit = new("SpiritMod");
        StarlightRiver = new("StarlightRiver");
        StarsAbove = new("StarsAbove");
        StoryofRedCloud = new("tsorcRevamp");
        Thorium = new("ThoriumMod");
        ThoriumRework = new("ThoriumRework");
        WrathoftheGods = new("NoxusBoss");
    }

    void IResourceLoader.OnModUnload()
    {
        CA = null;
        Calamity = null;
        CalDemutation = null;
        CALegacy = null;
        CalFables = null;
        CalOverhaul = null;
        CalSpear = null;
        Catalyst = null;
        Example = null;
        FargoDLC = null;
        FargoMutant = null;
        FargoSouls = null;
        Gensokyo = null;
        HollowKnight = null;
        HuntoftheOldGod = null;
        Infernum = null;
        InfiniteAddPoint = null;
        Luiafk = null;
        Luminance = null;
        LunarVeil = null;
        MEAC = null;
        Nycro = null;
        Overhaul = null;
        QoT = null;
        Radiance = null;
        Redemption = null;
        Remnants = null;
        Spear = null;
        Spirit = null;
        StarlightRiver = null;
        StarsAbove = null;
        StoryofRedCloud = null;
        Thorium = null;
        ThoriumRework = null;
        WrathoftheGods = null;
    }
}
