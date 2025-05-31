using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.IL;

namespace CalamityAnomalies;

public class CAMain
{
    public static Assembly Assembly { get; } = CalamityAnomalies.Instance.Code;

    public const string ModLocalizationPrefix = "Mods.CalamityAnomalies.";

    public static Type Type_CalamityMod { get; } = typeof(CalamityMod.CalamityMod);

    public static Mod CalamityModInstance { get; internal set; }

    public static Color AnomalyUltramundaneColor { get; } = new(0xE8, 0x97, 0xFF);
}

public class CAMainHelper : ITOLoader
{
    void ITOLoader.PostSetupContent()
    {
        CAMain.CalamityModInstance = (Mod)CAMain.Type_CalamityMod.GetField("Instance", TOReflectionUtils.UniversalBindingFlags).GetValue(null);
    }

    void ITOLoader.OnModUnload()
    {
        CAMain.CalamityModInstance = null;
    }
}
