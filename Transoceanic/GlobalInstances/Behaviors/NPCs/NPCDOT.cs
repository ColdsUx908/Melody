namespace Transoceanic.GlobalInstances.Behaviors.NPCs;

public sealed class NPCDOT : TOGlobalNPCBehavior
{
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        float totalDOT = 0f;
        int finalDamageValue = 0;
        foreach (ModDOT dot in ModDOTHelper.ModDOTSet)
        {
            if (dot.HasBuff(npc))
                ApplyDOT(dot.GetDamage(npc), dot.GetDamageValue(npc));
        }
        npc.ApplyDOT((int)totalDOT, finalDamageValue, ref damage);

        void ApplyDOT(float dot, int damageValue)
        {
            totalDOT += dot * 2f;
            finalDamageValue = Math.Max(finalDamageValue, damageValue);
        }
    }
}
