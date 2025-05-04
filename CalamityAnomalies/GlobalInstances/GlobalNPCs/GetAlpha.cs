using CalamityAnomalies.Contents.AnomalyMode.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (CAWorld.Anomaly
            && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride))
        {
            anomalyNPCOverride.TryConnectWithNPC(npc);
            return anomalyNPCOverride.ClearNPCInstances(anomalyNPCOverride.GetAlpha(drawColor));
        }

        return null;
    }
}
