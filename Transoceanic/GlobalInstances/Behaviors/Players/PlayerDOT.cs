namespace Transoceanic.GlobalInstances.Behaviors.Players;

public sealed class PlayerDOT : TOPlayerBehavior
{
    public override void UpdateBadLifeRegen()
    {
        float totalDOT = 0f;
        foreach (ModDOT dot in ModDOTHelper.ModDOTSet)
        {
            if (dot.HasBuff(Player))
                totalDOT += dot.GetDamage(Player);
        }

        Player.lifeRegen -= (int)totalDOT;
    }
}
