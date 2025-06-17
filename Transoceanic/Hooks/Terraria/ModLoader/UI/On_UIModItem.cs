namespace Transoceanic.Hooks.Terraria.ModLoader.UI;

public class On_UIModItem
{
    public static Type Type { get; } = TOHookUtils.GetTerrariaType("UIModItem");

    public static MethodInfo Method_Draw { get; } = Type.GetMethod("Draw", TOReflectionUtils.UniversalBindingFlags);

    public delegate void Orig_Draw(object self, SpriteBatch spriteBatch);
}
