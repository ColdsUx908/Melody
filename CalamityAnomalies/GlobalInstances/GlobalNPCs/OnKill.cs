using CalamityMod;
using CalamityMod.NPCs.Abyss;
using Terraria;
using Terraria.ModLoader;
using Transoceanic;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
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
