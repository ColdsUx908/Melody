using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.NPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override bool CheckActive(NPC npc)
    {
        if (disableNaturalDespawning)
            return false;
        return true;
    }
}
