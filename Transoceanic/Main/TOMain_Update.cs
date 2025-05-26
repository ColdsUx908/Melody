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

        TerrariaTime = new(Time24Hour, Main.GetMoonPhase());

        GameModeData gameModeData = TOMain.GameModeData;
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
}
