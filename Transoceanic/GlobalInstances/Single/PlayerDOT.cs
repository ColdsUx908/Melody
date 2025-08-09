namespace Transoceanic.GlobalInstances.Single;

public sealed class PlayerDOT : TOPlayerBehavior
{
    public override void UpdateBadLifeRegen()
    {
        float totalDOT = 0f;
        foreach (ModDOT dot in ModDOTHandler.ModDOTSet)
        {
            if (dot.HasBuff(Player))
                totalDOT += dot.GetDamage(Player);
        }
        foreach ((Predicate<Player> hasBuffPlayer, _, Func<Player, float> damagePlayer, _, _) in ModDOTHandler.ExternalDOTSet.Values)
        {
            if (hasBuffPlayer(Player))
                totalDOT += damagePlayer(Player);
        }

        Player.lifeRegen -= (int)totalDOT;
    }
}
