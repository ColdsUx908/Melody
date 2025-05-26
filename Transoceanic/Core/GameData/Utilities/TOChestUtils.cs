using Microsoft.Xna.Framework;
using Terraria;

namespace Transoceanic.Core.GameData.Utilities;

public static class TOChestUtils
{
    /// <summary>
    /// 获取箱子（左下角格子）在世界中的位置。
    /// </summary>
    public static Point Position(this Chest chest) => new(chest.x, chest.y);

    /// <summary>
    /// 获取箱子（左下角格子）在世界中的位置对应的向量。
    /// </summary>
    public static Vector2 Coordinate(this Chest chest) => new Point(chest.x, chest.y).ToWorldCoordinates();

    public static bool HasItem(this Chest chest, int itemType, out int index, out Item item)
    {
        for (int i = 0; i < chest.item.Length; i++)
        {
            Item current = chest.item[i];
            if (current.type == itemType)
            {
                index = i;
                item = current;
                return true;
            }
        }
        index = -1;
        item = null;
        return false;
    }
}
