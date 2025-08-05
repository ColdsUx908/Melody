namespace Transoceanic.Visual;

public static class TODrawUtils
{
    public static Vector2 ScreenSize => new(Main.screenWidth, Main.screenHeight);

    public static Vector2 ScreenCenter => Main.screenPosition + ScreenSize / 2f;

    public static Vector2 ScreenCenterTile => ScreenCenter / 16f;

    /// <summary>
    /// 在物品栏中绘制特定大小的物品贴图，不受物品栏自动缩放限制。
    /// 在 <see cref="ModItem.PreDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/> 方法中使用。
    /// </summary>
    public static void DrawInventoryCustomSize(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Vector2 origin,
        Texture2D texture, float wantedScale = 1f, Vector2 drawOffset = default)
        => spriteBatch.Draw(texture, position + drawOffset * wantedScale, frame, drawColor, 0f, origin, wantedScale, SpriteEffects.None, 0);

    public static void DrawBorderTexture(SpriteBatch spriteBatch, Texture2D texture, Vector2 baseDrawPosition, Rectangle? sourceRectangle, Color color, float rotation = 0f, Vector2 origin = default, float scale = 1f, SpriteEffects spriteEffects = SpriteEffects.None, float layerDepth = 0f,
        int way = 8, float borderWidth = 1f)
    {
        if (borderWidth > 0f)
        {
            Color realColor = color with { A = 0 };
            for (int i = 0; i < way; i++)
            {
                Vector2 offset = new PolarVector2(borderWidth, MathHelper.TwoPi / way * i);
                spriteBatch.Draw(texture, baseDrawPosition + offset, sourceRectangle, realColor, rotation, origin, scale, spriteEffects, layerDepth);
            }
        }
    }

    public static void DrawBorderString(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 baseDrawPosition, Color mainColor, Color borderColor, int way = 8, float borderWidth = 1f, float scale = 1f, float rotation = 0f)
    {
        if (borderWidth > 0f)
        {
            for (int i = 0; i < way; i++)
            {
                Vector2 offset = new PolarVector2(borderWidth, MathHelper.TwoPi / way * i);
                spriteBatch.DrawString(font, text, baseDrawPosition + offset, borderColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
        spriteBatch.DrawString(font, text, baseDrawPosition, mainColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
