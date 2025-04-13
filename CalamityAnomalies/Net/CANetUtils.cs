using CalamityAnomalies.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAnomalies.Net;

public static class CANetUtils
{
    private static ModPacket GetCAPacket() => CalamityAnomalies.Instance.GetPacket();

    public static void SyncAnomalyAI(this NPC npc)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;
        ModPacket packet = GetCAPacket();
        packet.Write(CANetPacketID.SyncAllAnomalyAI);
        packet.Write((byte)npc.whoAmI);

        CAGlobalNPC anomalyNPC = npc.Anomaly();
        ref float[] anomalyAI = ref anomalyNPC.anomalyAI;

        for (int i = 0; i < anomalyAI.Length; i++)
            packet.Write(anomalyAI[i]);

        packet.Send();
    }

    public static void SyncAnomalyAI(this NPC npc, params int[] indexes)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = GetCAPacket();
        packet.Write(CANetPacketID.SyncAnomalyAIWithIndexes);
        packet.Write((byte)indexes.Length);
        packet.Write((byte)npc.whoAmI);

        CAGlobalNPC anomalyNPC = npc.Anomaly();
        ref float[] anomalyAI = ref anomalyNPC.anomalyAI;

        foreach (int i in indexes)
        {
            packet.Write(i);
            packet.Write(anomalyAI[i]);
        }

        packet.Send();
    }
}