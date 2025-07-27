
namespace CalamityAnomalies.Core;

public static class CANetSync
{
    private static class ID
    {
        public const byte SyncAnomalyMode = 0;
        public const byte SyncAnomalyModeFromServer = 1;
    }

    public static ModPacket GetCAPacket() => CAMain.Instance.GetPacket();

    public static void SyncAnomalyMode(int ignoreClient = -1)
    {
        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncAnomalyMode);
        packet.Write(CAWorld.Anomaly);
        packet.Send(-1, ignoreClient);
    }

    public static void SyncAnomalyModeFromServer(int toClient = -1)
    {
        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncAnomalyModeFromServer);
        if (Main.dedServ)
            packet.Write(CAWorld.Anomaly);
        packet.Send(toClient);
    }

    public static void HandlePacket(CAMain mod, BinaryReader reader, int whoAmI)
    {
        byte id = reader.ReadByte();
        switch (id)
        {
            case ID.SyncAnomalyMode:
                CAWorld.Anomaly = reader.ReadBoolean();
                if (Main.dedServ)
                    SyncAnomalyMode(whoAmI);
                break;
            case ID.SyncAnomalyModeFromServer:
                if (Main.dedServ)
                    SyncAnomalyModeFromServer(whoAmI);
                else
                    CAWorld.Anomaly = reader.ReadBoolean();
                break;
        }
    }
}
