using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.ModLoader;
using Transoceanic.ExtraMathData;
using Transoceanic.MathHelp;

namespace Transoceanic.Visual;

public static class TODrawUtils
{
    public static Vector2 ScreenSize => new(Main.screenWidth, Main.screenHeight);

    public static Vector2 ScreenCenter => Main.screenPosition + ScreenSize / 2f;

    public static Vector2 ScreenCenterTile => ScreenCenter / 16f;

    public static Color LerpMany(this List<Color> colors, float ratio)
    {
        if (colors is null)
            return Color.White;
        switch (colors.Count)
        {
            case 0:
                return Color.White;
            case 1:
                return colors[0];
            case 2:
                return Color.Lerp(colors[0], colors[1], ratio);
            default:
                if (ratio <= 0f)
                    return colors[0];
                if (ratio >= colors.Count - 1)
                    return colors[^1];
                (int index, float localRatio) = TOMathHelper.SplitFloat(Math.Clamp(ratio, 0f, colors.Count - 1));
                return Color.Lerp(colors[index], colors[index + 1], localRatio);
        }
    }

    /// <summary>
    /// 在物品栏中绘制特定大小的物品贴图，不受物品栏自动缩放限制。
    /// 在 <see cref="ModItem.PreDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/> 方法中使用。
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="position"></param>
    /// <param name="frame"></param>
    /// <param name="drawColor"></param>
    /// <param name="origin"></param>
    /// <param name="texture"></param>
    /// <param name="wantedScale"></param>
    /// <param name="drawOffset"></param>
    public static void InventoryCustomSize(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Vector2 origin,
        Texture2D texture, float wantedScale = 1f, Vector2 drawOffset = default)
        => spriteBatch.Draw(texture, position + drawOffset * wantedScale, frame, drawColor, 0f, origin, wantedScale, SpriteEffects.None, 0);

    public static void DrawBorderString(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 baseDrawPosition, Color mainColor, Color borderColor, int way = 8, float borderWidth = 1f, float scale = 1f, float rotation = 0f)
    {
        for (int i = 0; i < way; i++)
        {
            Vector2 offset = new PolarVector2(borderWidth, MathHelper.TwoPi / way * i);
            spriteBatch.DrawString(font, text, baseDrawPosition + offset, borderColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
        spriteBatch.DrawString(font, text, baseDrawPosition, mainColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
