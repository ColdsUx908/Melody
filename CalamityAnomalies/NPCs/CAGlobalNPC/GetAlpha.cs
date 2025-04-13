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
        if (Main.LocalPlayer.Calamity().trippy)
            return new(Main.DiscoR, Main.DiscoG, Main.DiscoB, Main.DiscoR);

        if (CAWorld.Anomaly)
        {
            switch (npc.type)
            {
                case NPCID.KingSlime:
                    return null;
            }
        }

        return base.GetAlpha(npc, drawColor);
    }
}
