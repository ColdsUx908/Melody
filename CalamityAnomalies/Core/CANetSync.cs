
namespace CalamityAnomalies.Core;

public static class CANetSync
{
    public static class ID
    {
        public const byte SyncAnomalyMode = 0;
    }

    public static ModPacket GetCAPacket() => CAMain.Instance.GetPacket();

    public static void SyncAnomalyMode(int sender)
    {/*
        ModPacket packet = GetCAPacket();
        packet.Write(ID.SyncAnomalyMode);
        packet.Write(sender);
        packet.Write(CAWorld.Anomaly);
        packet.Send(-1, sender);*/
    }
}
