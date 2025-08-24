﻿namespace CalamityAnomalies.GlobalInstances.Single;

public sealed class CANPCLifetimeManager : CAGlobalNPCBehavior
{
    public override decimal Priority => 500m;

    public override void SetDefaults(NPC npc)
    {
        CAGlobalNPC anomalyNPC = npc.Anomaly();

        anomalyNPC.ShouldRunAnomalyAI = true;
    }

    public override bool PreAI(NPC npc)
    {
        CAGlobalNPC anomalyNPC = npc.Anomaly();

        if (CAWorld.Anomaly)
        {
            anomalyNPC.AnomalyAITimer++;
            if (CAWorld.AnomalyUltramundane)
            {
                anomalyNPC.AnomalyUltraAITimer++;
                anomalyNPC.AnomalyUltraBarTimer = Math.Clamp(anomalyNPC.AnomalyUltraBarTimer + 1, 0, 120);
            }
            else
            {
                anomalyNPC.AnomalyUltraAITimer = 0;
                anomalyNPC.AnomalyUltraBarTimer = Math.Clamp(anomalyNPC.AnomalyUltraBarTimer - 4, 0, 120);
            }
        }
        else
        {
            anomalyNPC.AnomalyAITimer = 0;
            anomalyNPC.AnomalyUltraAITimer = 0;
            anomalyNPC.AnomalyUltraBarTimer = 0;
        }

        return true;
    }
}
