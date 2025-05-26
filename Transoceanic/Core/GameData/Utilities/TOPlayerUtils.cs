using Terraria;

namespace Transoceanic.Core.GameData.Utilities;

public static class TOPlayerUtils
{
    public static bool IsAlive(this Player player) => !player.dead && !player.ghost;

    public static bool IsPvP(this Player player) => player.IsAlive() && player.hostile;

    public static bool IsTeammate(this Player player) => player.IsAlive() && player.team != 0 && player.team == Main.LocalPlayer.team;

    /// <summary>
    /// 获取玩家的手持物品。
    /// </summary>
    /// <param name="player"></param>
    /// <returns>若玩家光标持有物品，返回该物品；否则返回玩家物品栏中选中的物品。</returns>
    public static Item ActiveItem(this Player player) => Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
}
