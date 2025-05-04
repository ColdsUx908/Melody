using System;
using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.Players;

public partial class TOPlayer : ModPlayer
{
    public override void OnHurt(Player.HurtInfo info)
    {
        IsHurt = true;
        if (OverdrawnLife != 0)
        {
            OverdrawnLife = Math.Ceiling(OverdrawnLife);
            int temp = (int)OverdrawnLife;
            //if (temp > Player.statLife)
            //    ;
            //else
            info.Damage += temp;
        }
    }
}
