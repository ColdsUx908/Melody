namespace Transoceanic;

public partial class TOMain
{
    public const bool DEBUG_Default = false;

    /// <summary>
    /// 调试模式。开启后会在游戏中显示一些调试信息。
    /// </summary>
    public static bool DEBUG { get; internal set; } = DEBUG_Default;
}
