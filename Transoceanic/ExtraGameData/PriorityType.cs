namespace Transoceanic.ExtraGameData;

/// <summary>
/// NPC优先级设定。
/// </summary>
public enum PriorityType : byte
{
    /// <summary>
    /// 距离最近单位。
    /// </summary>
    Closest = 0,
    /// <summary>
    /// 最大生命值最高单位
    /// </summary>
    LifeMax = 1,
    /// <summary>
    /// 当前生命值最高单位。
    /// </summary>
    Life = 2
}
