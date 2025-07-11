using Transoceanic.RuntimeEditing;

namespace Transoceanic.Publicizer.Terraria.ModLoader.UI;

#pragma warning disable IDE1006
public record UIModItem_Publicizer
{
    public static readonly Type c_type = TOReflectionUtils.GetTerrariaType("UIModItem");

    public static readonly MethodInfo i_m_Draw = c_type.GetMethod("Draw", TOReflectionUtils.UniversalBindingFlags);

    public delegate void Orig_Draw(object self, SpriteBatch spriteBatch);

    public static void Draw(object self, SpriteBatch spriteBatch) => i_m_Draw.Invoke(self, [spriteBatch]);
}
