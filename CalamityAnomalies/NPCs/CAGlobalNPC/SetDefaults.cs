using CalamityAnomalies.Systems;
using CalamityMod;
using CalamityMod.NPCs;
using Terraria;
using Terraria.ModLoader;
using Transoceanic.Core;
using Transoceanic.GlobalEntity.GlobalNPCs;
using TransoceanicCalamity.Core;
using TransoceanicCalamity.GlobalEntity.GlobalNPCs;

namespace CalamityAnomalies.NPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override void SetDefaults(NPC npc)
    {
        TOGlobalNPC oceanNPC = npc.Ocean();
        TOCGlobalNPC oceanCalNPC = npc.OceanCal();
        CAGlobalNPC anomalyNPC = npc.Anomaly();
        CalamityGlobalNPC calamityNPC = npc.Calamity();

        //初始化anomalyAI数组
        for (int i = 0; i < anomalyAI.Length; i++)
            anomalyAI[i] = 0f;

        if (!CAWorld.Anomaly)
            return;

        if (!NPCOverride.NPCRegistered(npc.type, out NPCOverrideContainer container))
            return;

        if (!anomalyNPC.shouldRunAnomalyAI)
            return;

        NPCOverride behaviorOverride = container.BehaviorOverride;
        behaviorOverride.TryConnectWithNPC(npc);
        behaviorOverride.SetDefaults();
        behaviorOverride.ClearNPCInstances();
    }
}
