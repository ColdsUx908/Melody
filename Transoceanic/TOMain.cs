global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Text.RegularExpressions;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using ReLogic.Graphics;
global using ReLogic.Utilities;
global using Terraria;
global using Terraria.DataStructures;
global using Terraria.Enums;
global using Terraria.GameContent;
global using Terraria.Graphics;
global using Terraria.ID;
global using Terraria.IO;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using Terraria.ModLoader.Core;
global using Terraria.ModLoader.IO;
global using Terraria.Utilities;
global using Transoceanic.Commands;
global using Transoceanic.Core;
global using Transoceanic.Core.Extensions;
global using Transoceanic.Data;
global using Transoceanic.GlobalInstances;
global using Transoceanic.Localization;
global using Transoceanic.Maths;
global using Transoceanic.Visual;
global using ZLinq;
using Transoceanic.RuntimeEditing;

namespace Transoceanic;

public sealed class TOMain : Mod
{
    internal static TOMain Instance { get; private set; }

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
            TOWorld.GameTimer = 0;

            foreach (ITOLoader loader in
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).AsValueEnumerable()
                orderby pair.type.GetMethod(nameof(ITOLoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
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

    public override void PostSetupContent()
    {
        foreach (IResourceLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IResourceLoader>().AsValueEnumerable()
            orderby pair.type.GetMethod(nameof(IResourceLoader.PostSetupContent), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
            select pair.instance)
        {
            loader.PostSetupContent();
        }
    }

    public override void Unload()
    {
        Unloading = true;
        try
        {
            if (Loaded)
            {
                foreach (ITOLoader loader in (
                    from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>().AsValueEnumerable()
                    orderby pair.type.GetMethod(nameof(ITOLoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                    select pair.instance).Reverse())
                {
                    loader.Unload();
                }

                TOWorld.GameTimer = 0;
                TOWorld.Time24Hour = 0.0;
                TOWorld.TerrariaTime = default;
                TOWorld.TrueMasterMode = false;
                TOWorld.JourneyMasterMode = false;
                TOWorld.BossList = [];
                TOWorld.BossActive = false;

                SyncEnabled = false;
                Instance = null;
            }
        }
        finally
        {
            Loaded = false;
            Unloaded = true;
            Unloading = false;
        }
    }

    public static bool DEBUG { get; internal set; } =
#if DEBUG
        true
#else
        false
#endif
        ;

    private const string DEBUGPlayerName = "~ColdsUx";

    public static bool IsDEBUGPlayer(Player player) => DEBUG && player.name == DEBUGPlayerName;

    /// <summary>
    /// 是否启用Transoceanic模组内置的网络同步。
    /// <br/>由于Transoceanic为客户端模组，该选项必须由依赖模组手动开启。
    /// </summary>
    public static bool SyncEnabled
    {
        get;
        set
        {
            if (field && !value && !Unloading)
                throw new InvalidOperationException("SyncEnabled cannot be set to false after it has been set to true, unless unloading.");
            field = value;
        }
    } = false;

    public static Assembly Assembly => field ??= Instance.Code;

    public static Assembly TerrariaAssembly { get; } = typeof(Main).Assembly;

    public static Type[] TerrariaTypes { get; } = TerrariaAssembly.GetTypes();

    #region Constant
    public const string ModLocalizationPrefix = "Mods.Transoceanic.";

    public const string DebugPrefix = ModLocalizationPrefix + "DEBUG.";

    public const string DebugErrorMessageKey = ModLocalizationPrefix + "DEBUG.ErrorMessage";

    public static Color TODebugWarnColor { get; } = Color.Orange;

    public static Color TODebugErrorColor { get; } = new(0xFF, 0x00, 0x00);

    public static Color CelestialColor { get; } = new(0xAF, 0xFF, 0xFF);

    public static Color DiscoColor => new(Main.DiscoR, Main.DiscoG, Main.DiscoB, Main.DiscoR);

    public const int CelestialPrice = 25000000;

    #endregion Constant
}
