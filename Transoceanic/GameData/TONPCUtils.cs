namespace Transoceanic.GameData;

public static class TONPCUtils
{
    public static bool IsDefeatingTwins(NPC npc) => npc.type switch
    {
        NPCID.Retinazer => !NPC.ActiveNPCs.Any(k => k.type == NPCID.Spazmatism),
        NPCID.Spazmatism => !NPC.ActiveNPCs.Any(k => k.type == NPCID.Retinazer),
        _ => false
    };
}
