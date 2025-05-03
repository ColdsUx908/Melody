using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Transoceanic.Core.GameData;

namespace Transoceanic;

public partial class TOMain
{
    public static ulong GeneralTimer { get; internal set; } = 0;

    public static Assembly Assembly { get; } = Transoceanic.Instance.Code;

    public static Type MainType { get; } = typeof(Main);

    public static GameModeData GameModeData => (GameModeData)MainType.GetField("_currentGameModeInfo", UniversalBindingFlags).GetValue(null);


    public static TOIterator<NPC> ActiveNPCs => TOIteratorFactory.NewActiveNPCIterator();

    public static TOIterator<NPC> Enemies => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsEnemy);

    public static TOIterator<NPC> Bosses => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsBossTO);

    public static TOIterator<Projectile> ActiveProjectiles => TOIteratorFactory.NewActiveProjectileIterator();

    public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewActivePlayerIterator();

    public static TOIterator<Player> PVPPlayers => TOIteratorFactory.NewActivePlayerIterator(TOPlayerUtils.IsPvP);

    public static List<NPC> BossList { get; internal set; }

    public static bool BossActive { get; internal set; } = false;

    /// <summary>
    /// 是否为“真正的”大师模式（即创建世界时选择“大师难度”）。
    /// </summary>
    public static bool TrueMasterMode { get; internal set; } = false;

    /// <summary>
    /// 是否为旅行大师模式（即将敌人难度调整至3.0的旅行模式）。
    /// </summary>
    public static bool JourneyMasterMode { get; internal set; } = false;

    /// <summary>
    /// 是否为大师模式。
    /// <br/>同时检查 <see cref="TrueMasterMode"/> 和 <see cref="JourneyMasterMode"/>。
    /// </summary>
    public static bool MasterMode => TrueMasterMode || JourneyMasterMode;

    public static bool LegendaryMode { get; internal set; } = false;

    /// <summary>
    /// 有些行为仅在单人模式或者多人模式服务端进行。
    /// </summary>
    public static bool GeneralClient => Main.netMode != NetmodeID.MultiplayerClient;


    public static NPC DummyNPC => Main.npc[Main.maxNPCs];

    #region Constant
    public const string ModLocalizationPrefix = "Mods.Transoceanic.";

    public const string DebugPrefix = ModLocalizationPrefix + "DEBUG.";

    public const string DebugErrorMessageKey = ModLocalizationPrefix + "DEBUG.ErrorMessage";

    /// <summary>
    /// 包含所有所需Flag。
    /// </summary>
    public const BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    //public static string ConfigPath => Path.Combine(Main.SavePath, "ModConfigs", "Transoceanic_TOConfig.json");

    public static Color TODebugErrorColor { get; } = new(0xFF, 0x00, 0x00);

    public static Color CelestialColor { get; } = new(0xAF, 0xFF, 0xFF);

    public const int CelestialValue = 25000000;
    #endregion
}
