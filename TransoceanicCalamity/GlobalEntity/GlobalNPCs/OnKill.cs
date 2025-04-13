using Terraria;
using Terraria.ModLoader;
using TransoceanicCalamity.Core;
using TransoceanicCalamity.TOCPlayer;

namespace TransoceanicCalamity.GlobalEntity.GlobalNPCs;

public partial class TOCGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        foreach (Player player in Main.ActivePlayers)
        {
            TOCGlobalPlayer tocPlayer = player.OceanCal();
            tocPlayer.PlayerDownedBossCalamity.BossesOnKill(npc);
        }
    }
}
