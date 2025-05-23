using System;
using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.Players;

public partial class TOPlayer : ModPlayer
{
    public override void PreUpdate()
    {
        GameTime++;
        /*
        switch (Player.name)
        {
            case "Celessalia":
                Celesgod = Annigod = true;
                if (GameTime == 300)
                {
                    TOLocalizationUtils.ChatLocalizedText(TOMain.ModLocalizationPrefix + "Gods.CelessAlive", TOMain.CelestialColor, Player);
                    TOLocalizationUtils.ChatLocalizedText(TOMain.ModLocalizationPrefix + "Gods.AnniBehind", TOMain.CelestialColor, Player);
                }
                break;
            case "Anniah":
                Celesgod = false;
                Annigod = true;
                if (GameTime == 300)
                    TOLocalizationUtils.ChatLocalizedText(TOMain.ModLocalizationPrefix + "Gods.AnniAlive", TOMain.CelestialColor, Player);
                break;
            default:
                Celesgod = Annigod = false;
                break;
        }
        */
    }

    public override void PostUpdate()
    {
        if (IsHurt)
        {
            IsHurt = false;
            TimeWithoutHurt = 0;
        }
        else
            TimeWithoutHurt++;
        if (OverdrawnLife > OverdrawnLifeLimit && TimeWithoutHurt > OverdrawnLifeRegenThreshold)
        {
            OverdrawnLife = Math.Max(0,
                OverdrawnLife - Math.Min(
                    Math.Pow(TimeWithoutHurt - OverdrawnLifeRegenThreshold, OverdrawnLifeRegenExponent) / 300 * OverdrawnLifeRegenMult,
                    OverdrawnLifeRegenLimit));
        }
    }
}
