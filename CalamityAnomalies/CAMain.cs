using System;
using System.Reflection;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;

namespace CalamityAnomalies;

public class CAMain
{
    public static Assembly Assembly { get; } = CalamityAnomalies.Instance.Code;

    public const string ModLocalizationPrefix = "Mods.CalamityAnomalies.";

    public static Type Type_CalamityMod { get; } = typeof(CalamityMod.CalamityMod);

    public static Mod CalamityModInstance { get; internal set; }
}

public class CAMainHelper : ITOLoader
{
    void ITOLoader.PostSetupContent()
    {
        CAMain.CalamityModInstance = (Mod)CAMain.Type_CalamityMod.GetField("Instance", TOMain.UniversalBindingFlags).GetValue(null);
    }

    void ITOLoader.OnModUnload()
    {
        CAMain.CalamityModInstance = null;
    }
}
