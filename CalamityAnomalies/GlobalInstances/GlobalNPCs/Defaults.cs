using CalamityAnomalies.Contents.AnomalyMode.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override void SetStaticDefaults()
    {
        foreach (AnomalyNPCOverride anomalyNPCOverride in AnomalyNPCOverrideHelper.AnomalyNPCOverrideSet.Values)
            anomalyNPCOverride.SetStaticDefaults();
    }

    public override void SetDefaults(NPC npc)
    {
        //初始化anomalyAI数组
        for (int i = 0; i < AnomalyAI.Length; i++)
            AnomalyAI[i] = 0f;

        if (CAWorld.Anomaly
            && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && ShouldRunAnomalyAI)
        {
            anomalyNPCOverride.TryConnectWithNPC(npc);
            anomalyNPCOverride.SetDefaults();
            anomalyNPCOverride.ClearNPCInstances();
        }
    }
}
