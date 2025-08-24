namespace Transoceanic.GlobalInstances.Single;

/*
public sealed class PlayerOverdrawnLife : TOPlayerBehavior
{
    public override void ResetEffects()
    {
        OceanPlayer.OverdrawnLifeLimit = 0;
        OceanPlayer.OverdrawnLifeRegenExponent = 2;
        OceanPlayer.OverdrawnLifeRegenLimit = 0.5;
        OceanPlayer.OverdrawnLifeRegenMult = 1;
        OceanPlayer.OverdrawnLifeRegenThreshold = 600;
    }

    public override void PostUpdateMiscEffects()
    {
        if (OceanPlayer.OverdrawnLife > OceanPlayer.OverdrawnLifeLimit && OceanPlayer.TimeWithoutHurt > OceanPlayer.OverdrawnLifeRegenThreshold)
        {
            OceanPlayer.OverdrawnLife -= Math.Clamp(
                Math.Pow(OceanPlayer.TimeWithoutHurt - OceanPlayer.OverdrawnLifeRegenThreshold, OceanPlayer.OverdrawnLifeRegenExponent) / 300 * OceanPlayer.OverdrawnLifeRegenMult,
                0, OceanPlayer.OverdrawnLifeRegenLimit);
        }
    }

    public override void PostHurt(Player.HurtInfo info)
    {
        if (OceanPlayer.OverdrawnLife != 0)
        {
            double damage = Math.Ceiling(OceanPlayer.OverdrawnLife);
            if (damage > Player.statLife)
                KillPlayer(Player);
        }
    }

    public static void KillPlayer(Player player)
    {
        //TODO
        //player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromKey(TOMain.ModLocalizationPrefix +)), damage, 0, false);
    }
}
*/