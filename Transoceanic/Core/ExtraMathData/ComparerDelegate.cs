namespace Transoceanic.Core.ExtraMathData;

/// <summary>
/// 比较两个实例。
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="left"></param>
/// <param name="right"></param>
/// <returns>在左右两个值中右边是否“胜出”。</returns>
public delegate bool TOComparison<T>(T left, T right);

public enum CompareStatus
{
    LeftBetter,
    RightBetter,
    Equal
}

/// <summary>
/// 比较两个实例。
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="left"></param>
/// <param name="right"></param>
/// <returns>在左右两个值中：
/// <br/>若左边“胜出”，<see cref="CompareStatus.LeftBetter"/>；
/// <br/>若右边“胜出”，<see cref="CompareStatus.RightBetter"/>；
/// <br/>若左右两边“地位相等”，<see cref="CompareStatus.Equal"/>。</returns>
public delegate CompareStatus TOPreciseComparison<T>(T left, T right);
