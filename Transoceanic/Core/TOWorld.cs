using Terraria.GameContent.Creative;
using Transoceanic.Publicizer.Terraria;

namespace Transoceanic.Core;

public sealed class TOWorld : ModSystem
{
    public static bool AprilFools => DateTime.Now is (int _, 4, 1);

    public static bool GeneralClient => Main.netMode != NetmodeID.MultiplayerClient;

    public static float GeneralSeconds => Main.GlobalTimeWrappedHourly;

    public static float GeneralMinutes => GeneralSeconds / 60f;

    public static TerrariaTimer GameTimer { get; internal set; } = default;

    public static double Time24Hour { get; internal set; } = 0.0;

    public static TerrariaTime TerrariaTime { get; internal set; } = default;

    public static bool TrueMasterMode { get; internal set; } = false;

    public static bool MasterMode => TrueMasterMode || JourneyMasterMode;

    public static bool JourneyMasterMode { get; internal set; } = false;

    public static bool TrueLegedaryMode => Main.getGoodWorld && TrueMasterMode;

    public static bool LegendaryMode => Main.getGoodWorld && MasterMode;

    public static List<NPC> BossList { get; internal set; } = [];

    public static bool BossActive { get; internal set; } = false;

    public override void PreUpdateEntities()
    {
        GameTimer++;
        Time24Hour = (Main.dayTime ? 4.5 : 19.5) + Main.time / 3600.0;
        TerrariaTime = new(Time24Hour, Main.GetMoonPhase());

        GameModeData gameModeInfo = Main_Publicizer._currentGameModeInfo;
        TrueMasterMode = gameModeInfo.IsMasterMode;
        if (gameModeInfo.IsJourneyMode)
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