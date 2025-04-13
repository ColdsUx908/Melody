namespace CalamityAnomalies.Net;

public enum CANetPacketType : byte
{
    SyncAllAnomalyAI = 0,
    SyncAnomalyAIWithIndexes = 1,
}

public static class CANetPacketID
{
    public const byte syncAllAnomalyAI = 0;
    public const byte syncAnomalyAIWithIndexes = 1;
}
