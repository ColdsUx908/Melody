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
global using Transoceanic.Core.Utilities;
global using Transoceanic.Data;
global using Transoceanic.GameData;
global using Transoceanic.GlobalInstances;
global using Transoceanic.Localization;
global using Transoceanic.Maths;
global using ZLinq;
using Terraria.GameContent.Creative;

namespace Transoceanic;

public class Transoceanic : Mod
{
    internal static Transoceanic Instance { get; private set; }

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

            foreach (ITOLoader loader in
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).AsValueEnumerable()
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

    public override void PostSetupContent()
    {
        foreach (IResourceLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IResourceLoader>().AsValueEnumerable()
            orderby pair.type.GetMethod("PostSetupContent", TOReflectionUtils.UniversalBindingFlags)?.GetAttribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
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
                    orderby pair.type.GetMethod("Load", TOReflectionUtils.UniversalBindingFlags)?.GetAttribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                    select pair.instance).Reverse())
                {
                    loader.Unload();
                }
                TOMain.SyncEnabled = false;
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

public static class TOMain
{
    #region NotUpdate

    public static bool DEBUG { get; internal set; } =
#if DEBUG
        true
#else
        false
#endif
        ;

    public static bool IsDEBUGPlayer(Player player) => DEBUG && player.name == "~ColdsUx";

    /// <summary>
    /// 是否启用Transoceanic模组内置的网络同步。
    /// <br/>由于Transoceanic为客户端模组，该选项必须由依赖模组手动开启。
    /// </summary>
    public static bool SyncEnabled
    {
        get;
        set
        {
            if (field && !value && !Transoceanic.Unloading)
                throw new InvalidOperationException("SyncEnabled cannot be set to false after it has been set to true, unless unloading.");
            field = value;
        }
    } = false;

    public static Assembly Assembly { get; } = Transoceanic.Instance.Code;

    public static Type Type_Main { get; } = typeof(Main);

    public static Assembly TerrariaAssembly { get; } = Type_Main.Assembly;

    public static Type[] TerrariaTypes { get; } = TerrariaAssembly.GetTypes();

    public static FieldInfo Main__currentGameModInfo { get; } = Type_Main.GetField("_currentGameModeInfo", TOReflectionUtils.UniversalBindingFlags);

    public static GameModeData GameModeData => (GameModeData)Main__currentGameModInfo.GetValue(null);

    public static bool MasterMode => TrueMasterMode || JourneyMasterMode;

    public static bool GeneralClient => Main.netMode != NetmodeID.MultiplayerClient;

    public static NPC DummyNPC => Main.npc[Main.maxNPCs];

    public static Projectile DummyProjectile => Main.projectile[Main.maxProjectiles];

    public static Player Server => Main.player[Main.maxPlayers];

    #region Constant
    public const string ModLocalizationPrefix = "Mods.Transoceanic.";

    public const string DebugPrefix = ModLocalizationPrefix + "DEBUG.";

    public const string DebugErrorMessageKey = ModLocalizationPrefix + "DEBUG.ErrorMessage";

    public static Color TODebugWarnColor { get; } = Color.Orange;

    public static Color TODebugErrorColor { get; } = new(0xFF, 0x00, 0x00);

    public static Color CelestialColor { get; } = new(0xAF, 0xFF, 0xFF);

    public static Color DiscoColor { get; internal set; } = new(Main.DiscoR, Main.DiscoG, Main.DiscoB, Main.DiscoR);

    public const int CelestialPrice = 25000000;
    #endregion Constant

    #endregion NotUpdate

    #region ShouldUpdate

    #region PreUpdateEntities
    public static float GeneralSeconds => Main.GlobalTimeWrappedHourly;

    public static float GeneralMinutes => GeneralSeconds / 60f;

    public static TerrariaTimer GameTimer { get; private set; }

    public static double Time24Hour { get; private set; } = 0.0;

    public static TerrariaTime TerrariaTime { get; private set; }

    public static bool TrueMasterMode { get; private set; } = false;

    public static bool JourneyMasterMode { get; private set; } = false;

    public static bool LegendaryMode { get; private set; } = false;
    #endregion PreUpdateEntities

    #region PostUpdateNPCs
    public static List<NPC> BossList { get; private set; } = [];

    public static bool BossActive { get; private set; } = false;
    #endregion PostUpdateNPCs

    public class Update : ModSystem
    {
        public override void PreUpdateEntities()
        {
            GameTimer++;
            Time24Hour = (Main.dayTime ? 4.5 : 19.5) + Main.time / 3600.0;
            TerrariaTime = new(Time24Hour, Main.GetMoonPhase());

            GameModeData gameModeData = GameModeData;
            TrueMasterMode = gameModeData.IsMasterMode;
            if (gameModeData.IsJourneyMode)
            {
                CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
                bool currentJourneyMaster = power.StrengthMultiplierToGiveNPCs == 3f;
                if (power.GetIsUnlocked())
                    JourneyMasterMode = currentJourneyMaster;
                else if (!currentJourneyMaster)
                    JourneyMasterMode = false;
            }
            else
                JourneyMasterMode = false;

            LegendaryMode = Main.getGoodWorld && MasterMode;

            DiscoColor = new(Main.DiscoR, Main.DiscoG, Main.DiscoB, Main.DiscoR);
        }

        public override void PostUpdateNPCs()
        {
            BossList = NPC.Bosses.ToList();
            BossActive = BossList.Count > 0;
        }

        public override void OnWorldLoad()
        {
            GameTimer = 0;
        }

        public override void OnWorldUnload()
        {
            GameTimer = 0;
        }
    }
    #endregion ShouldUpdate

    #region ShouldLoad
    public class Load : ITOLoader
    {
        void ITOLoader.Load()
        {
            GameTimer = 0;
        }

        void ITOLoader.Unload()
        {
            GameTimer = 0;
            Time24Hour = 0.0;
            TerrariaTime = default;
            TrueMasterMode = false;
            JourneyMasterMode = false;
            LegendaryMode = false;
            BossList = [];
            BossActive = false;
        }
    }
    #endregion ShouldLoad
}
