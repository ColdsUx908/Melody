using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override bool CheckActive(NPC npc)
    {
        if (DisableNaturalDespawning)
            return false;
        return true;
    }
}
