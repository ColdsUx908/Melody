using Terraria;

namespace Transoceanic;

public partial class TOMain
{
    public const bool DEBUG_Default = false;

    /// <summary>
    /// 调试模式。开启后会在游戏中显示一些调试信息，以及开启一些游戏内容调试功能。
    /// </summary>
    public static bool DEBUG { get; internal set; } = DEBUG_Default;

    public static bool IsDEBUGPlayer(Player player) => DEBUG && player.name == "~ColdsUx";
}
