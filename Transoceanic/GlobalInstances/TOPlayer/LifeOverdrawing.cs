using System;
using Terraria;
using Terraria.ModLoader;
using Transoceanic.Core.GameData;

namespace Transoceanic.GlobalInstances.TOPlayer;

public class LifeOverdrawing : ModPlayer
{
    public override void PreUpdate()
    {
        TOGlobalPlayer oceanPlayer = Player.Ocean();
        oceanPlayer.TimeWithoutHurt++;
        if (oceanPlayer.OverdrawnLife > oceanPlayer.OverdrawnLifeLimit && oceanPlayer.TimeWithoutHurt > oceanPlayer.OverdrawnLifeRegenThreshold)
        {
            oceanPlayer.OverdrawnLife = Math.Max(0,
                oceanPlayer.OverdrawnLife - Math.Min(
                    Math.Pow(oceanPlayer.TimeWithoutHurt - oceanPlayer.OverdrawnLifeRegenThreshold, oceanPlayer.OverdrawnLifeRegenExponent) / 300 * oceanPlayer.OverdrawnLifeRegenMult,
                    oceanPlayer.OverdrawnLifeRegenLimit));
        }
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        TOGlobalPlayer oceanPlayer = Player.Ocean();
        if (oceanPlayer.OverdrawnLife != 0)
        {
            oceanPlayer.OverdrawnLife = Math.Ceiling(oceanPlayer.OverdrawnLife);
            int temp = (int)oceanPlayer.OverdrawnLife;
            //if (temp > Player.statLife)
            //    ;
            //else
            info.Damage += temp;
        }
    }
}
