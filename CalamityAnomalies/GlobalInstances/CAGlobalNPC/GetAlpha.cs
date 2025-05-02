using CalamityAnomalies.Contents.AnomalyMode.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances;

public partial class CAGlobalNPC : GlobalNPC
{
    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (!CAWorld.Anomaly
            || !AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride))
            return null;

        anomalyNPCOverride.TryConnectWithNPC(npc);
        Color? result = anomalyNPCOverride.GetAlpha(drawColor);
        anomalyNPCOverride.ClearNPCInstances();
        return result;
    }
}
