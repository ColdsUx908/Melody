using Microsoft.Xna.Framework;
using Terraria;

namespace Transoceanic.Core.GameData;

public static class TOChestUtils
{
    /// <summary>
    /// 获取箱子（左下角格子）在世界中的位置。
    /// </summary>
    /// <param name="chest"></param>
    /// <returns></returns>
    public static Point Position(this Chest chest) => new(chest.x, chest.y);

    /// <summary>
    /// 获取箱子（左下角格子）在世界中的位置对应的向量。
    /// </summary>
    /// <param name="chest"></param>
    /// <returns></returns>
    public static Vector2 Coordinate(this Chest chest) => new Point(chest.x, chest.y).ToWorldCoordinates();

    public static bool HasItem(this Chest chest, int itemType, out int index)
    {
        for (int i = 0; i < chest.item.Length; i++)
        {
            if (chest.item[i].type == itemType)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }
}
