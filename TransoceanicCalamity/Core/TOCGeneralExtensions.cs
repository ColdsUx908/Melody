using Terraria;
using TransoceanicCalamity.GlobalEntity.GlobalNPCs;
using TransoceanicCalamity.TOCPlayer;

namespace TransoceanicCalamity.Core;

public static class TOCGeneralExtensions
{
    public static TOCGlobalPlayer OceanCal(this Player player) => player.GetModPlayer<TOCGlobalPlayer>();

    public static TOCGlobalNPC OceanCal(this NPC npc) => npc.GetGlobalNPC<TOCGlobalNPC>();
}
