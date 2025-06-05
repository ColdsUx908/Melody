namespace CalamityAnomalies.Net;

public static class CANetUtils
{
    private static ModPacket GetCAPacket() => CalamityAnomalies.Instance.GetPacket();

    /*
    public static void SyncAnomalyAI(this NPC npc)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;
        ModPacket packet = GetCAPacket();
        packet.Write(CANetPacketID.SyncAllAnomalyAI);
        packet.Write((byte)npc.whoAmI);

        CAGlobalNPC anomalyNPC = npc.Anomaly();

        for (int i = 0; i < anomalyNPC.AnomalyAI.Length; i++)
            packet.Write(anomalyNPC.AnomalyAI[i]);

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

        foreach (int i in indexes)
        {
            packet.Write(i);
            packet.Write(anomalyNPC.AnomalyAI[i]);
        }

        packet.Send();
    }
    */
}