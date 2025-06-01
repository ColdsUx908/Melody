using Terraria;

namespace Transoceanic.GlobalInstances;

public static class TOGlobalExtensions
{
    public static TOPlayer Ocean(this Player player) => player.GetModPlayer<TOPlayer>();
    public static TOGlobalNPC Ocean(this NPC npc) => npc.GetGlobalNPC<TOGlobalNPC>();
    public static TOGlobalProjectile Ocean(this Projectile projectile) => projectile.GetGlobalProjectile<TOGlobalProjectile>();
    public static TOGlobalItem Ocean(this Item item) => item.GetGlobalItem<TOGlobalItem>();
}
