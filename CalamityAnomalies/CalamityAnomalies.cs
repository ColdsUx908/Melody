global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using CalamityAnomalies.Configs;
global using CalamityAnomalies.Core;
global using CalamityAnomalies.GlobalInstances;
global using CalamityAnomalies.UI;
global using CalamityAnomalies.Visual;
global using CalamityMod;
global using CalamityMod.CalPlayer;
global using CalamityMod.Items;
global using CalamityMod.NPCs;
global using CalamityMod.Projectiles;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using ReLogic.Graphics;
global using ReLogic.Utilities;
global using Terraria;
global using Terraria.Audio;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Terraria.ID;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using Terraria.ModLoader.IO;
global using Terraria.Utilities;
global using Transoceanic;
global using Transoceanic.Core;
global using Transoceanic.Core.Extensions;
global using Transoceanic.Core.Utilities;
global using Transoceanic.Data;
global using Transoceanic.GlobalInstances;
global using Transoceanic.Localization;
global using Transoceanic.Maths;
global using Transoceanic.RuntimeEditing;
global using Transoceanic.Visual;
global using ZLinq;
global using CalamityMod_ = CalamityMod.CalamityMod;

namespace CalamityAnomalies;

public sealed class CalamityAnomalies : Mod
{
    internal static CalamityAnomalies Instance { get; private set; }

    internal static bool Loading { get; private set; } = false;

    internal static bool Loaded { get; private set; } = false;

    internal static bool Unloading { get; private set; } = false;

    internal static bool Unloaded { get; private set; } = false;

    public override void Load()
    {
        Loading = true;
        try
        {
            Instance = this;

            foreach (ICALoader loader in
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ICALoader>(CAMain.Assembly).AsValueEnumerable()
                orderby pair.type.GetMethod("Load", TOReflectionUtils.UniversalBindingFlags)?.GetAttribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                select pair.instance)
            {
                loader.Load();
            }
        }
        finally
        {
            Loaded = true;
            Loading = false;
        }
    }

    public override void Unload()
    {
        Unloading = true;
        try
        {
            if (Loaded)
            {
                foreach (ICALoader loader in (
                    from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ICALoader>(CAMain.Assembly).AsValueEnumerable()
                    orderby pair.type.GetMethod("Load", TOReflectionUtils.UniversalBindingFlags)?.GetAttribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                    select pair.instance).Reverse())
                {
                    loader.Unload();
                }
                Instance = null;
            }
        }
        finally
        {
            Unloaded = true;
            Unloading = false;
        }
    }
}

public sealed class CAMain : IResourceLoader
{
    public static Assembly Assembly { get; } = CalamityAnomalies.Instance.Code;

    public static string ModName { get; } = CalamityAnomalies.Instance.Name;

    public static Color MainColor { get; } = Color.HotPink;

    public static Color SecondaryColor { get; } = Color.Pink;

    public static List<Color> ColorList { get; } = [MainColor, SecondaryColor, MainColor];

    public static Color GetGradientColor(float ratio = 0.5f) => ColorList.LerpMany(TOMathHelper.GetTimeSin(ratio / 2f, unsigned: true));

    public const string ModLocalizationPrefix = "Mods.CalamityAnomalies.";

    public const string TweakLocalizationPrefix = ModLocalizationPrefix + "Tweaks.";

    public const string TexturePrefix = "CalamityAnomalies/Textures/";

    public const string CalamityModLocalizationPrefix = "Mods.CalamityMod.";

    public static Type Type_CalamityMod { get; } = typeof(CalamityMod_);

    public static CalamityMod_ CalamityModInstance { get; internal set; } = null;

    public static Color AnomalyUltramundaneColor { get; } = new(0xE8, 0x97, 0xFF);

    void IResourceLoader.PostSetupContent()
    {
        CalamityModInstance = (CalamityMod_)Type_CalamityMod.GetField("Instance", TOReflectionUtils.StaticBindingFlags).GetValue(null);
        TOMain.SyncEnabled = true;
    }

    void IResourceLoader.OnModUnload()
    {
        CalamityModInstance = null;
    }
}