using Terraria;
using Transoceanic.GlobalInstances.GlobalItems;
using Transoceanic.GlobalInstances.GlobalNPCs;
using Transoceanic.GlobalInstances.GlobalProjectiles;
using Transoceanic.GlobalInstances.TOPlayer;

namespace Transoceanic.Core.GameData;

public static class TOGeneralExtentions
{
    public static TOGlobalPlayer Ocean(this Player player) => player.GetModPlayer<TOGlobalPlayer>();
    public static TOGlobalNPC Ocean(this NPC npc) => npc.GetGlobalNPC<TOGlobalNPC>();
    public static TOGlobalProjectile Ocean(this Projectile projectile) => projectile.GetGlobalProjectile<TOGlobalProjectile>();
    public static TOGlobalItem Ocean(this Item item) => item.GetGlobalItem<TOGlobalItem>();
}
