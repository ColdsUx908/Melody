using CalamityAnomalies.NPCs;
using CalamityAnomalies.Players;
using Terraria;

namespace CalamityAnomalies;

public static partial class CAUtils
{
    /// <summary>
    /// 获取NPC对应的 <see cref="CAGlobalNPC"/> 实例。
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public static CAGlobalNPC Anomaly(this NPC npc) => npc.GetGlobalNPC<CAGlobalNPC>();

    /// <summary>
    /// 获取玩家对应的 <see cref="CAPlayer"/> 实例。
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static CAPlayer Anomaly(this Player player) => player.GetModPlayer<CAPlayer>();
}
