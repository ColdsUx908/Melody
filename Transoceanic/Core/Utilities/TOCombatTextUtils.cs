namespace Transoceanic.Core.Utilities;

public static class TOCombatTextUtils
{
    public static void ChangeHitNPCText(Action<CombatText> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        CombatText text = null;
        for (int i = Main.maxCombatText - 1; i >= 0; i--)
        {
            CombatText current = Main.combatText[i];
            if ((current is { crit: true, lifeTime: 120, alpha: 1f } && current.color == CombatText.DamagedHostileCrit) || (current is { crit: false, lifeTime: 60, alpha: 1f } && current.color == CombatText.DamagedHostile))
            {
                text = current;
                break;
            }
        }
        if (text is not null)
            action(text);
    }
}
