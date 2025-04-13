using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Transoceanic.Core;

public static class TODrawUtils
{
    /// <summary>
    /// 在物品栏中绘制特定大小的物品贴图，不受物品栏自动缩放限制。
    /// 在 <see cref="ModItem.PreDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/> 方法中使用。
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="position"></param>
    /// <param name="frame"></param>
    /// <param name="drawColor"></param>
    /// <param name="itemColor"></param>
    /// <param name="origin"></param>
    /// <param name="scale"></param>
    /// <param name="texture"></param>
    /// <param name="wantedScale"></param>
    /// <param name="drawOffset"></param>
    public static void InventoryCustomSize(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Vector2 origin,
        Texture2D texture, float wantedScale = 1f, Vector2 drawOffset = default)
        => spriteBatch.Draw(texture, position + drawOffset * wantedScale, frame, drawColor, 0f, origin, wantedScale, SpriteEffects.None, 0);
}