using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using static Transoceanic.TOMain;

namespace Transoceanic;

public sealed class TOMain_Update : ModSystem
{
    public override void PreUpdateEntities()
    {
        GeneralTimer++;

        TerrariaTime = new(Main.time, Main.GetMoonPhase());

        GameModeData gameModeData = TOMain.GameModeData;
        TrueMasterMode = gameModeData.IsMasterMode;

        if (gameModeData.IsJourneyMode)
        {
            bool journeyMaster = false;
            CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
            bool currentJourneyMaster = power.StrengthMultiplierToGiveNPCs == 3f;
            if (power.GetIsUnlocked())
                journeyMaster = currentJourneyMaster;
            else if (!currentJourneyMaster)
                journeyMaster = false;
            JourneyMasterMode = journeyMaster;
        }
        else
            JourneyMasterMode = false;

        LegendaryMode = Main.getGoodWorld && MasterMode;
    }

    public override void PostUpdateNPCs()
    {
        BossList = Bosses.ToList();
        BossActive = BossList.Count > 0;
    }
}
