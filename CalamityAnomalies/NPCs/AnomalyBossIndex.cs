using Terraria;
using Terraria.ID;

namespace CalamityAnomalies.NPCs;

public static class AnomalyBossIndex
{
    public static int KingSlime
    {
        get;
        set
        {
            if (field == -1 || (field != value && Main.npc[field].type == NPCID.KingSlime && Main.npc[field].active))
                field = value;
        }
    }
}
