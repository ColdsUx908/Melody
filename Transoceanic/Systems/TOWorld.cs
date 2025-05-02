using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace Transoceanic;

public class TOWorld : ModSystem
{
    public override void PreUpdateEntities()
    {
        GameModeData gameModeData = TOMain.GameModeData;
        TOMain.TrueMasterMode = gameModeData.IsMasterMode;

        if (gameModeData.IsJourneyMode)
        {
            bool journeyMaster = false;
            CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
            bool currentJourneyMaster = power.StrengthMultiplierToGiveNPCs == 3f;
            if (power.GetIsUnlocked())
                journeyMaster = currentJourneyMaster;
            else if (!currentJourneyMaster)
                journeyMaster = false;
            TOMain.JourneyMasterMode = journeyMaster;
        }
        else
            TOMain.JourneyMasterMode = false;

        TOMain.LegendaryMode = Main.getGoodWorld && TOMain.MasterMode;
    }

    public override void PostUpdateNPCs()
    {
        TOMain.BossList = TOMain.Bosses.ToList();
        TOMain.BossActive = TOMain.BossList.Count > 0;
    }
}
