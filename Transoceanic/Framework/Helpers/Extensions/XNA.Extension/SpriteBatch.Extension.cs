namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(SpriteBatch spriteBatch)
    {
        /// <summary>
        /// 绘制纹理，保证传入的坐标为纹理中心点。
        /// </summary>
        public void DrawFromCenter(Texture2D texture, Vector2 center, Color color, Rectangle? sourceRectangle = null, float rotation = 0f, float scale = 1f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f) =>
            spriteBatch.Draw(texture, center, sourceRectangle, color, rotation, (sourceRectangle?.Size() ?? texture.Size()) / 2f, scale, effects, layerDepth);

        /// <summary>
        /// 绘制纹理，保证传入的坐标为纹理中心点。
        /// </summary>
        public void DrawFromCenter(Texture2D texture, Vector2 center, Color color, Rectangle? sourceRectangle = null, float rotation = 0f, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f) =>
            spriteBatch.Draw(texture, center, sourceRectangle, color, rotation, (sourceRectangle?.Size() ?? texture.Size()) / 2f, scale ?? new Vector2(1f), effects, layerDepth);
    }
}
