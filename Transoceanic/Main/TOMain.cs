using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Transoceanic.Core.ExtraGameData;
using Transoceanic.Core.GameData;
using Transoceanic.Core.GameData.Utilities;
using Transoceanic.Core.IL;

namespace Transoceanic;

public partial class TOMain
{
    public static ulong GeneralTimer { get; internal set; } = 0;

    public static Assembly Assembly { get; } = Transoceanic.Instance.Code;

    public static Type Type_Main { get; } = typeof(Main);

    public static double Time24Hour => (Main.dayTime ? 4.5 : 19.5) + Main.time / 3600.0;

    public static TerrariaTime TerrariaTime { get; internal set; }

    public static GameModeData GameModeData => (GameModeData)Type_Main.GetField("_currentGameModeInfo", TOReflectionUtils.UniversalBindingFlags).GetValue(null);

    public static TOIterator<NPC> ActiveNPCs => TOIteratorFactory.NewActiveNPCIterator();

    public static TOIterator<NPC> Enemies => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsEnemy);

    public static TOIterator<NPC> Bosses => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsBossTO);

    public static TOIterator<Projectile> ActiveProjectiles => TOIteratorFactory.NewActiveProjectileIterator();

    public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewActivePlayerIterator();

    public static TOExclusiveIterator<Player> Teammates => TOIteratorFactory.NewActivePlayerIterator(TOPlayerUtils.IsTeammate, Main.LocalPlayer);

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

    /// <summary>
    /// 假人NPC。
    /// <br/>对应 <see cref="Main.maxNPCs"/> 的最后一个NPC。
    /// </summary>
    public static NPC DummyNPC => Main.npc[Main.maxNPCs];

    public static Projectile DummyProjectile => Main.projectile[Main.maxProjectiles];

    public static Player Server => Main.player[Main.maxPlayers];

    #region Constant
    public const string ModLocalizationPrefix = "Mods.Transoceanic.";

    public const string DebugPrefix = ModLocalizationPrefix + "DEBUG.";

    public const string DebugErrorMessageKey = ModLocalizationPrefix + "DEBUG.ErrorMessage";

    public static Color TODebugErrorColor { get; } = new(0xFF, 0x00, 0x00);

    public static Color CelestialColor { get; } = new(0xAF, 0xFF, 0xFF);

    public static Color DiscoColor { get; internal set; } = new(Main.DiscoR, Main.DiscoG, Main.DiscoB, Main.DiscoR);

    public const int CelestialValue = 25000000;
    #endregion
}
