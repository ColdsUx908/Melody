using Terraria;
using Transoceanic.GlobalInstances.GlobalItems;
using Transoceanic.GlobalInstances.GlobalNPCs;
using Transoceanic.GlobalInstances.GlobalProjectiles;
using Transoceanic.GlobalInstances.Players;

namespace Transoceanic.GlobalInstances;

public static class TOGlobalExtentions
{
    public static TOPlayer Ocean(this Player player) => player.GetModPlayer<TOPlayer>();
    public static TOGlobalNPC Ocean(this NPC npc) => npc.GetGlobalNPC<TOGlobalNPC>();
    public static TOGlobalProjectile Ocean(this Projectile projectile) => projectile.GetGlobalProjectile<TOGlobalProjectile>();
    public static TOGlobalItem Ocean(this Item item) => item.GetGlobalItem<TOGlobalItem>();
}
