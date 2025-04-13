using Terraria;
using Transoceanic.GlobalEntity.GlobalItems;
using Transoceanic.GlobalEntity.GlobalNPCs;
using Transoceanic.GlobalEntity.GlobalProjectiles;
using Transoceanic.GlobalEntity.TOPlayer;

namespace Transoceanic.Core;

public static class TOGeneralExtentions
{
    public static TOGlobalPlayer Ocean(this Player player) => player.GetModPlayer<TOGlobalPlayer>();
    public static TOGlobalNPC Ocean(this NPC npc) => npc.GetGlobalNPC<TOGlobalNPC>();
    public static TOGlobalProjectile Ocean(this Projectile projectile) => projectile.GetGlobalProjectile<TOGlobalProjectile>();
    public static TOGlobalItem Ocean(this Item item) => item.GetGlobalItem<TOGlobalItem>();
}
