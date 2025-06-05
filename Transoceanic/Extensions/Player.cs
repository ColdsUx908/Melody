using Transoceanic.GlobalInstances;

namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(Player player)
    {
        public TOPlayer Ocean() => player.GetModPlayer<TOPlayer>();

        public bool Alive => !player.dead && !player.ghost;

        public bool PvP => player.Alive && player.hostile;

        public bool IsTeammateOf(Player other) => player.Alive && player.team != 0 && player.team == other.team;

        public TOExclusiveIterator<Player> Teammates => TOIteratorFactory.NewActivePlayerIterator(k => k.IsTeammateOf(player), Main.player);

        /// <summary>
        /// 获取玩家的手持物品。
        /// </summary>
        /// <returns>若玩家光标持有物品，返回该物品；否则返回玩家物品栏中选中的物品。</returns>
        public Item ActiveItem => Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
    }
}
