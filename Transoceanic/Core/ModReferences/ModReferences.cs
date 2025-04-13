using Terraria.ModLoader;

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

    private TOModReferenceContainer() { }

    public static readonly TOModReferenceContainer Empty = new();

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

public static class TOModReferences
{
    public static TOModReferenceContainer Example { get; private set; }
    public static TOModReferenceContainer QoT { get; private set; }
    public static TOModReferenceContainer Luiafk { get; private set; }
    public static TOModReferenceContainer Luminance { get; private set; }
    public static TOModReferenceContainer Nycro { get; private set; }
    public static TOModReferenceContainer Toasty { get; private set; }
    public static TOModReferenceContainer ToastyCal { get; private set; }
    public static TOModReferenceContainer Remnants { get; private set; }
    public static TOModReferenceContainer InfiniteAddPoint { get; private set; }
    public static TOModReferenceContainer Calamity { get; private set; }
    public static TOModReferenceContainer CA { get; private set; }
    public static TOModReferenceContainer CALegacy { get; private set; }
    public static TOModReferenceContainer Catalyst { get; private set; }
    public static TOModReferenceContainer Infernum { get; private set; }
    public static TOModReferenceContainer CalOverhaul { get; private set; }
    public static TOModReferenceContainer HuntoftheOldGod { get; private set; }
    public static TOModReferenceContainer WrathoftheGods { get; private set; }
    public static TOModReferenceContainer CalSpear { get; private set; }
    public static TOModReferenceContainer CalDemutation { get; private set; }
    public static TOModReferenceContainer FargoMutant { get; private set; }
    public static TOModReferenceContainer FargoSouls { get; private set; }
    public static TOModReferenceContainer FargoDLC { get; private set; }
    public static TOModReferenceContainer Thorium { get; private set; }
    public static TOModReferenceContainer ThoriumRework { get; private set; }
    public static TOModReferenceContainer MEAC { get; private set; }
    public static TOModReferenceContainer Radiance { get; private set; }
    public static TOModReferenceContainer Gensokyo { get; private set; }
    public static TOModReferenceContainer StarsAbove { get; private set; }
    public static TOModReferenceContainer Overhaul { get; private set; }
    public static TOModReferenceContainer Redemption { get; private set; }
    public static TOModReferenceContainer StoryofRedCloud { get; private set; }
    public static TOModReferenceContainer StarlightRiver { get; private set; }
    public static TOModReferenceContainer Spirit { get; private set; }
    public static TOModReferenceContainer LunarVeil { get; private set; }
    public static TOModReferenceContainer HollowKnight { get; private set; }
    public static TOModReferenceContainer Spear { get; private set; }

    static TOModReferences()
    {
        Example = new("ExampleMod");
        QoT = new("ImproveGame");
        Luiafk = new("miningcracks_take_on_luiafk");
        Luminance = new("Luminance");
        Nycro = new("EfficientNohits");
        Toasty = new("ToastyQoL");
        ToastyCal = new("ToastyQoLCalamity");
        Remnants = new("Remnants");
        InfiniteAddPoint = new("InfiniteAddPointSystem");
        Calamity = new("CalamityMod");
        CA = new("CalamityAnomalies");
        CALegacy = new("ACalTweak");
        Catalyst = new("Catalyst");
        Infernum = new("InfernumMode");
        CalOverhaul = new("CalamityOverhaul");
        HuntoftheOldGod = new("CalamityHunt");
        WrathoftheGods = new("NoxusBoss");
        CalSpear = new("CalamityThrowingSpear");
        CalDemutation = new("CalamityDemutation");
        FargoMutant = new("Fargowiltas");
        FargoSouls = new("FargowiltasSouls");
        FargoDLC = new("FargowiltasCrossMod");
        Thorium = new("ThoriumMod");
        ThoriumRework = new("ThoriumRework");
        MEAC = new("MEAC");
        Radiance = new("Radiance");
        Gensokyo = new("Gensokyo");
        StarsAbove = new("StarsAbove");
        Overhaul = new("TerrariaOverhaul");
        Redemption = new("Redemption");
        StoryofRedCloud = new("tsorcRevamp");
        StarlightRiver = new("StarlightRiver");
        Spirit = new("SpiritMod");
        LunarVeil = new("Stellamod");
        HollowKnight = new("TheKnightMod");
        Spear = new("SpearToJavelin");
    }
}
