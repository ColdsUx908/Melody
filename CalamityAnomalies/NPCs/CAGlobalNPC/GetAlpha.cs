using CalamityAnomalies.Systems;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAnomalies.NPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (!CAWorld.Anomaly ||
            !NPCOverride.NPCRegistered(npc.type, out NPCOverrideContainer container) ||
            !container.BehaviorOverride.DisableCalamityMethods.HasFlag(DisableCalamityMethods.GetAlpha))
            return null;

        return container.BehaviorOverride.GetAlpha(drawColor);
    }
}
