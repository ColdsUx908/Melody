using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.TOPlayer;

/// <summary>
/// 神佑效果。
/// </summary>
public class PlayerGods : ModPlayer
{
    /*
    public override void PostUpdate()
    {
        TOGlobalPlayer oceanPlayer = Player.Ocean();

        switch (Player.name)
        {
            case "Celessalia":
                oceanPlayer.Celesgod = oceanPlayer.Annigod = true;
                if (oceanPlayer.GameTime == 300)
                {
                    TOLocalizationUtils.ChatLocalizedText(TOMain.ModLocalizationPrefix + "Gods.CelessAlive", TOMain.CelestialColor, Player);
                    TOLocalizationUtils.ChatLocalizedText(TOMain.ModLocalizationPrefix + "Gods.AnniBehind", TOMain.CelestialColor, Player);
                }
                break;
            case "Anniah":
                oceanPlayer.Celesgod = false;
                oceanPlayer.Annigod = true;
                if (oceanPlayer.GameTime == 300)
                    TOLocalizationUtils.ChatLocalizedText(TOMain.ModLocalizationPrefix + "Gods.AnniAlive", TOMain.CelestialColor, Player);
                break;
            default:
                oceanPlayer.Celesgod = oceanPlayer.Annigod = false;
                break;
        }
    }
    */
}
