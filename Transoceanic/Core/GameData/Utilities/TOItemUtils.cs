using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.Core.GameData.Utilities;

public static class TOItemUtils
{
    public static T GetModItem<T>(this Item item) where T : ModItem => item.ModItem as T;
}