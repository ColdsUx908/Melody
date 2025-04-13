using System.IO;
using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Transoceanic.Core;
using System.Reflection;
using Terraria.ID;

namespace Transoceanic;

public partial class TOMain
{
    public static int GeneralTimer { get; internal set; }

    public static Assembly Assembly => Transoceanic.Instance.Code;


    public static TOEntityIterator<NPC> ActiveNPCs => TOEntityIteratorCreator.NewNPCIterator(k => k.active);

    public static TOEntityIterator<NPC> Enemies => TOEntityIteratorCreator.NewNPCIterator(TONPCUtils.IsEnemy);

    public static TOEntityIterator<NPC> Bosses => TOEntityIteratorCreator.NewNPCIterator(TONPCUtils.IsBossTO);

    public static TOEntityIterator<Projectile> ActiveProjectiles => TOEntityIteratorCreator.NewProjectileIterator(k => k.active);

    public static TOEntityIterator<Player> ActivePlayers => TOEntityIteratorCreator.NewPlayerIterator(k => k.active);

    public static TOEntityIterator<Player> PVPPlayers => TOEntityIteratorCreator.NewPlayerIterator(TOPlayerUtils.IsPvP);

    public static List<NPC> BossList { get; internal set; }

    public static bool BossActive { get; internal set; } = false;

    /// <summary>
    /// Binding flags that account for all access/local membership status.
    /// </summary>
    public const BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 有些行为仅在单人模式或者多人模式服务端进行。
    /// </summary>
    public static bool GeneralClient => Main.netMode != NetmodeID.MultiplayerClient;


    public static NPC DummyNPC => Main.npc[Main.maxNPCs];

    #region Constant
    public const string ModLocalizationPrefix = "Mods.Transoceanic.";

    public const string DebugPrefix = ModLocalizationPrefix + "DEBUG.";

    public const string DebugErrorMessageKey = ModLocalizationPrefix + "DEBUG.ErrorMessage";

    public static string ConfigPath => Path.Combine(Main.SavePath, "ModConfigs", "Transoceanic_TOConfig.json");

    public static Color TODebugErrorColor => new(0xFF, 0x00, 0x00);

    public static Color CelestialColor => new(0xAF, 0xFF, 0xFF);

    public const int celestialValue = 25000000;
    #endregion
}
