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
    public static int GeneralTimer { get; internal set; }

    public static readonly Assembly Assembly = Transoceanic.Instance.Code;

    public static readonly Type MainType = typeof(Main);

    public static GameModeData GameModeData => (GameModeData)MainType.GetField("_currentGameModeInfo", UniversalBindingFlags).GetValue(null);


    public static TOIterator<NPC> ActiveNPCs => TOIteratorFactory.NewActiveNPCIterator();

    public static TOIterator<NPC> Enemies => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsEnemy);

    public static TOIterator<NPC> Bosses => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsBossTO);

    public static TOIterator<Projectile> ActiveProjectiles => TOIteratorFactory.NewActiveProjectileIterator();

    public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewActivePlayerIterator();

    public static TOIterator<Player> PVPPlayers => TOIteratorFactory.NewActivePlayerIterator(TOPlayerUtils.IsPvP);

    public static List<NPC> BossList { get; internal set; }

    public static bool BossActive { get; internal set; } = false;

    public static bool TrueMasterMode { get; internal set; } = false;

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

    public static readonly Color TODebugErrorColor = new(0xFF, 0x00, 0x00);

    public static readonly Color CelestialColor = new(0xAF, 0xFF, 0xFF);

    public const int CelestialValue = 25000000;
    #endregion
}
