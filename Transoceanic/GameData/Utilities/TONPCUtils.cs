namespace Transoceanic.GameData.Utilities;

public static class TONPCUtils
{
    public static bool IsDefeatingTwins(NPC npc) => npc.type switch
    {
        NPCID.Retinazer => !TOMain.ActiveNPCs.Any(k => k.type == NPCID.Spazmatism),
        NPCID.Spazmatism => !TOMain.ActiveNPCs.Any(k => k.type == NPCID.Retinazer),
        _ => false
    };

    public static bool AnyNPCs<T>() where T : ModNPC => NPC.AnyNPCs(ModContent.NPCType<T>());

    public static bool AnyNPCs(int type, [NotNullWhen(true)] out NPC npc)
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            npc = Main.npc[i];
            if (npc.active && npc.type == type)
                return true;
        }
        npc = null;
        return false;
    }

    public static bool AnyNPCs<T>([NotNullWhen(true)] out NPC npc) where T : ModNPC
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            npc = Main.npc[i];
            if (npc.active && npc.ModNPC is T)
                return true;
        }
        npc = null;
        return false;
    }

    public static bool AnyNPCs<T>([NotNullWhen(true)] out NPC npc, [NotNullWhen(true)] out T modNPC) where T : ModNPC
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            npc = Main.npc[i];
            if (npc.active && npc.ModNPC is T t)
            {
                modNPC = t;
                return true;
            }
        }
        npc = null;
        modNPC = null;
        return false;
    }
}
