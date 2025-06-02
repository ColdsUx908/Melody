using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Transoceanic.ExtraGameData;
using Transoceanic.Localization;

namespace Transoceanic;

public sealed partial class TOMain
{
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
    /// 是否为“真正的”大师模式（即创建世界时选择“大师难度”）。
    /// </summary>
    public static bool TrueMasterMode { get; private set; } = false;

    /// <summary>
    /// 是否为旅行大师模式（即将敌人难度调整至3.0的旅行模式）。
    /// </summary>
    public static bool JourneyMasterMode { get; private set; } = false;

    /// <summary>
    /// 是否为传奇难度（在“真正的”大师模式或旅行大师模式的基础上开启FTW种子特性）。
    /// </summary>
    public static bool LegendaryMode { get; private set; } = false;
    #endregion

    #region PostUpdateNPCs
    public static List<NPC> BossList { get; private set; } = [];

    public static bool BossActive { get; private set; } = false;
    #endregion

    public sealed class Update : ModSystem
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

    public sealed class Load : ITOLoader
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
}
