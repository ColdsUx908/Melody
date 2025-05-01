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

        bool journeyMaster = false;
        if (gameModeData.IsJourneyMode)
        {
            CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
            bool currentJourneyMaster = power.StrengthMultiplierToGiveNPCs == 3f;
            if (power.GetIsUnlocked())
                journeyMaster = currentJourneyMaster;
            else if (!currentJourneyMaster)
                journeyMaster = false;
        }

        TOMain.LegendaryMode = Main.getGoodWorld && (TOMain.TrueMasterMode || journeyMaster);
    }

    public override void PostUpdateNPCs()
    {
        TOMain.BossList = TOMain.Bosses.ToList();
        TOMain.BossActive = TOMain.BossList.Count > 0;
    }
}
