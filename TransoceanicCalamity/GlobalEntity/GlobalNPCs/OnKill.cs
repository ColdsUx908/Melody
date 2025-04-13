using CalamityMod;
using CalamityMod.NPCs.Abyss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TransoceanicCalamity.Core;
using TransoceanicCalamity.TOCPlayer;

namespace TransoceanicCalamity.GlobalEntity.GlobalNPCs;

public partial class TOCGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (npc.type == ModContent.NPCType<EidolonWyrmHead>() && !DownedBossSystem.downedPrimordialWyrm)
        {
            DownedBossSystem.downedPrimordialWyrm = true;
            if (Main.dedServ)
                NetMessage.SendData(MessageID.WorldData);
        }

        foreach (Player player in Main.ActivePlayers)
        {
            TOCGlobalPlayer tocPlayer = player.OceanCal();
            tocPlayer.PlayerDownedBossCalamity.BossesOnKill(npc);
        }
    }
}
