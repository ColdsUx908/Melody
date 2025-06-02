using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Transoceanic.GameData;
using Transoceanic.GameData.Utilities;
using Transoceanic.IL;

namespace Transoceanic;

public sealed partial class TOMain
{
    public static Assembly Assembly { get; } = Transoceanic.Instance.Code;

    public static Type Type_Main { get; } = typeof(Main);

    public static GameModeData GameModeData => (GameModeData)Type_Main.GetField("_currentGameModeInfo", TOReflectionUtils.UniversalBindingFlags).GetValue(null);

    public static TOIterator<NPC> ActiveNPCs => TOIteratorFactory.NewActiveNPCIterator();

    public static TOIterator<NPC> Enemies => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsEnemy);

    public static TOIterator<NPC> Bosses => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsBossTO);

    public static TOIterator<Projectile> ActiveProjectiles => TOIteratorFactory.NewActiveProjectileIterator();

    public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewActivePlayerIterator();

    public static TOExclusiveIterator<Player> Teammates => TOIteratorFactory.NewActivePlayerIterator(TOPlayerUtils.IsTeammate, Main.LocalPlayer);

    public static TOIterator<Player> PVPPlayers => TOIteratorFactory.NewActivePlayerIterator(TOPlayerUtils.IsPvP);

    /// <summary>
    /// 是否为大师模式。
    /// <br/>同时检查 <see cref="TrueMasterMode"/> 和 <see cref="JourneyMasterMode"/>。
    /// </summary>
    public static bool MasterMode => TrueMasterMode || JourneyMasterMode;

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
