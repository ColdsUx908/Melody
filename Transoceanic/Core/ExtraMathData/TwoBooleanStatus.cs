using Transoceanic.Core.MathHelp;

namespace Transoceanic.Core.ExtraMathData;

/// <summary>
/// 两个布尔值的逻辑状态。
/// <br>使用 <see cref="TOMathHelper.GetTwoBooleanStatus(bool, bool)"/> 方法来获取值。</br>
/// </summary>
public enum TwoBooleanStatus : byte
{
    /// <summary>
    /// 二者均为false。
    /// </summary>
    Neither = 0,
    /// <summary>
    /// A为true，B为false。
    /// </summary>
    ATrue = 1,
    /// <summary>
    /// A为false，B为true。
    /// </summary>
    BTrue = 2,
    /// <summary>
    /// 二者均为true。
    /// </summary>
    Both = 3
}
