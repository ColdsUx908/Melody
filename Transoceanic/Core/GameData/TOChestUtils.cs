using Microsoft.Xna.Framework;
using Terraria;

namespace Transoceanic.Core.GameData;

public static class TOChestUtils
{
    public static Point Position(this Chest chest) => new(chest.x, chest.y);
}
