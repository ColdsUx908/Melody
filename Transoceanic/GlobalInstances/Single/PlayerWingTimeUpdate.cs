namespace Transoceanic.GlobalInstances.Single;

public sealed class PlayerWingTimeUpdate : TOPlayerBehavior
{
    public override void ResetEffects()
    {
        for (int i = 0; i < OceanPlayer.WingTimeMaxMultipliers.Length; i++)
            OceanPlayer.WingTimeMaxMultipliers[i] = AddableFloat.Zero;
    }

    public override void PostUpdateMiscEffects()
    {
        if (Player.wingTimeMax > 0)
        {
            float multiplier = 1f;
            foreach (AddableFloat wingTimeMaxMultiplier in OceanPlayer.WingTimeMaxMultipliers)
                multiplier *= 1f + wingTimeMaxMultiplier.Value;
            Player.wingTimeMax = (int)(Player.wingTimeMax * multiplier);
        }
    }
}
