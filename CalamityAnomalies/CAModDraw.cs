using Transoceanic.Hooks.Terraria.ModLoader.UI;

namespace CalamityAnomalies;

//TODO: 非常酷的Mod绘制方法
public class CAModDraw : ITODetourProvider
{
    public static void Detour_Draw(On_UIModItem.Orig_Draw orig, object self, SpriteBatch spriteBatch)
    {
        orig(self, spriteBatch);
    }

    void ITODetourProvider.ApplyDetour()
    {
        //TODetourUtils.ModifyMethodWithDetour(On_UIModItem.Method_Draw, Detour_Draw);
    }
}
