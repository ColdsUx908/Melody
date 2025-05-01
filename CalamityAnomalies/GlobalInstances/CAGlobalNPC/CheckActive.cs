using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances;

public partial class CAGlobalNPC : GlobalNPC
{
    public override bool CheckActive(NPC npc)
    {
        if (disableNaturalDespawning)
            return false;
        return true;
    }
}
