using CalamityAnomalies.Utilities;
using CalamityMod;
using CalamityMod.NPCs.Abyss;
using Terraria;
using Terraria.ModLoader;
using Transoceanic;

namespace CalamityAnomalies.GlobalInstances;

public partial class CAGlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (npc.type == ModContent.NPCType<EidolonWyrmHead>() && !DownedBossSystem.downedPrimordialWyrm)
            DownedBossSystem.downedPrimordialWyrm = true;

        foreach (Player player in TOMain.ActivePlayers)
        {
            player.Anomaly().PlayerDownedBossCalamity.BossesOnKill(npc);
        }
    }
}
