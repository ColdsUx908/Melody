using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic.ExtraGameData;
using Transoceanic.GameData;
using Transoceanic.GameData.Utilities;
using Transoceanic.IL;
using ZLinq;

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
            foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).AsValueEnumerable()
                .OrderByDescending(k => k.instance.GetPriority(LoaderMethodType.Load)))
            {
                if (!type.MustHaveRealMethodWith("Load", "Unload", TOReflectionUtils.UniversalBindingFlags))
                    throw new Exception($"[{type.Name}] must implement Unload with Load implemented.");
                else
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
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>().AsValueEnumerable()
            .OrderByDescending(k => k.instance.GetPriority(LoaderMethodType.PostSetupContent)))
        {
            if (!type.MustHaveRealMethodWith("PostSetUpContents", "OnModUnload", TOReflectionUtils.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement OnModUnload with PostSetupContent implemented.");
            else
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
                foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly)
                    .OrderByDescending(k => k.GetPriority(LoaderMethodType.UnLoad)))
                {
                    loader.UnLoad();
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

public class TOMain
{
    #region NotUpdate

    /// <summary>
    /// ����ģʽ�������������Ϸ����ʾһЩ������Ϣ���Լ�����һЩ��Ϸ���ݵ��Թ��ܡ�
    /// </summary>
    public static bool DEBUG { get; internal set; } =
#if DEBUG
        true
#else
        false
#endif
        ;

    public static bool IsDEBUGPlayer(Player player) => DEBUG && player.name == "~ColdsUx";

    /// <summary>
    /// ���ڱ�ʶ�Ƿ�����ͬ�����ܡ�
    /// <br/>����TransoceanicΪ�ͻ���Mod��Ĭ������²�����ͬ�����ܣ���������ģ�鿪����
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

    public static GameModeData GameModeData => (GameModeData)Type_Main.GetField("_currentGameModeInfo", TOReflectionUtils.UniversalBindingFlags).GetValue(null);

    public static TOIterator<NPC> ActiveNPCs => TOIteratorFactory.NewActiveNPCIterator();

    public static TOIterator<NPC> Enemies => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsEnemy);

    public static TOIterator<NPC> Bosses => TOIteratorFactory.NewActiveNPCIterator(TONPCUtils.IsBossTO);

    public static TOIterator<Projectile> ActiveProjectiles => TOIteratorFactory.NewActiveProjectileIterator();

    public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewActivePlayerIterator();

    public static TOExclusiveIterator<Player> Teammates => TOIteratorFactory.NewActivePlayerIterator(TOPlayerUtils.IsTeammate, Main.LocalPlayer);

    public static TOIterator<Player> PVPPlayers => TOIteratorFactory.NewActivePlayerIterator(TOPlayerUtils.IsPvP);

    /// <summary>
    /// �Ƿ�Ϊ��ʦģʽ��
    /// <br/>ͬʱ��� <see cref="TrueMasterMode"/> �� <see cref="JourneyMasterMode"/>��
    /// </summary>
    public static bool MasterMode => TrueMasterMode || JourneyMasterMode;

    /// <summary>
    /// ��Щ��Ϊ���ڵ���ģʽ���߶���ģʽ����˽��С�
    /// </summary>
    public static bool GeneralClient => Main.netMode != NetmodeID.MultiplayerClient;

    /// <summary>
    /// ����NPC��
    /// <br/>��Ӧ <see cref="Main.maxNPCs"/> �����һ��NPC��
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

    public const int CelestialPrice = 25000000;
    #endregion

    #endregion

    #region ShouldUpdate

    #region PreUpdateEntities
    public static float GeneralSeconds => Main.GlobalTimeWrappedHourly;

    public static float GeneralMinutes => GeneralSeconds / 60f;

    public static ulong GameTimer
    {
        get;
        internal set
        {
            field = value;
            GameSeconds = value / 60f;
            GameMinutes = GameSeconds / 60f;
            GameHours = GameMinutes / 60f;
        }
    } = 0;

    public static float GameSeconds { get; private set; } = 0f;

    public static float GameMinutes { get; private set; } = 0f;

    public static float GameHours { get; private set; } = 0f;

    public static double Time24Hour { get; private set; } = 0.0;

    public static TerrariaTime TerrariaTime { get; private set; }

    /// <summary>
    /// �Ƿ�Ϊ�������ġ���ʦģʽ������������ʱѡ�񡰴�ʦ�Ѷȡ�����
    /// </summary>
    public static bool TrueMasterMode { get; private set; } = false;

    /// <summary>
    /// �Ƿ�Ϊ���д�ʦģʽ�����������Ѷȵ�����3.0������ģʽ����
    /// </summary>
    public static bool JourneyMasterMode { get; private set; } = false;

    /// <summary>
    /// �Ƿ�Ϊ�����Ѷȣ��ڡ������ġ���ʦģʽ�����д�ʦģʽ�Ļ����Ͽ���FTW�������ԣ���
    /// </summary>
    public static bool LegendaryMode { get; private set; } = false;
    #endregion

    #region PostUpdateNPCs
    public static List<NPC> BossList { get; private set; } = [];

    public static bool BossActive { get; private set; } = false;
    #endregion

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
            BossList = Bosses.ToList();
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

    public class Load : ITOLoader
    {
        void ITOLoader.Load()
        {
            GameTimer = 0;
        }

        void ITOLoader.UnLoad()
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
    #endregion
}
