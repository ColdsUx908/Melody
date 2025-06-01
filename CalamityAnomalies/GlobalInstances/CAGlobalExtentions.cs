using CalamityAnomalies.GlobalInstances.GlobalItems;
using Terraria;

namespace CalamityAnomalies.GlobalInstances;

public static class CAGlobalExtensions
{
    public static CAPlayer Anomaly(this Player player) => player.GetModPlayer<CAPlayer>();
    public static CAGlobalNPC Anomaly(this NPC npc) => npc.GetGlobalNPC<CAGlobalNPC>();
    public static CAGlobalItem Anomaly(this Item item) => item.GetGlobalItem<CAGlobalItem>();
    public static CAGlobalProjectile Anomaly(this Projectile projectile) => projectile.GetGlobalProjectile<CAGlobalProjectile>();
}
