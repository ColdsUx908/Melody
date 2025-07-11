using Transoceanic.RuntimeEditing;

namespace Transoceanic.Publicizer.Terraria;

#pragma warning disable IDE1006
public record Main_Publicizer
{
    public static readonly Type c_type = typeof(Main);

    public static readonly FieldInfo s_f__currentGameModInfo = c_type.GetField("_currentGameModeInfo", TOReflectionUtils.UniversalBindingFlags);

    public static GameModeData _currentGameModeInfo
    {
        get => (GameModeData)s_f__currentGameModInfo.GetValue(null);
        set => s_f__currentGameModInfo.SetValue(null, value);
    }
}
