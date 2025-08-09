namespace Transoceanic.GlobalInstances.Single;

public sealed class PlayerOverdrawnLife : TOPlayerBehavior
{
    public override void PostUpdateMiscEffects()
    {
        if (OceanPlayer.OverdrawnLife > OceanPlayer.OverdrawnLifeLimit && OceanPlayer.TimeWithoutHurt > OceanPlayer.OverdrawnLifeRegenThreshold)
        {
            OceanPlayer.OverdrawnLife = Math.Max(0,
                OceanPlayer.OverdrawnLife - Math.Min(
                    Math.Pow(OceanPlayer.TimeWithoutHurt - OceanPlayer.OverdrawnLifeRegenThreshold, OceanPlayer.OverdrawnLifeRegenExponent) / 300 * OceanPlayer.OverdrawnLifeRegenMult,
                    OceanPlayer.OverdrawnLifeRegenLimit));
        }
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        if (OceanPlayer.OverdrawnLife != 0)
        {
            int temp = (int)Math.Ceiling(OceanPlayer.OverdrawnLife);
            //if (temp > Player.statLife)
            //    ;
            //else
            info.Damage += temp;
        }
    }
}
