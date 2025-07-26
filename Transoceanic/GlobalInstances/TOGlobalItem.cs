namespace Transoceanic.GlobalInstances;

public class TOGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public override GlobalItem Clone(Item from, Item to)
    {
        TOGlobalItem clone = (TOGlobalItem)base.Clone(from, to);

        clone.InternalEquipped = InternalEquipped;
        clone._lastEquippedTime = _lastEquippedTime;
        clone._lastUnequippedTime = _lastUnequippedTime;

        return clone;
    }

    public ItemTooltipDictionary TooltipDictionary { get; internal set; } = null;

    internal GuaranteedBoolean InternalEquipped = new();
    internal int _lastEquippedTime = -1;
    internal int _lastUnequippedTime = -1;
    internal bool ShouldUpdateLastEquippedTime => InternalEquipped && _lastEquippedTime <= _lastUnequippedTime;
    internal bool ShouldUpdateLastUnequippedTime => !Main.gamePaused && Main.hasFocus && !InternalEquipped && _lastUnequippedTime <= _lastEquippedTime;

    public bool IsEquipped => _lastEquippedTime > _lastUnequippedTime;

    /// <summary>
    /// 获取一个物品的装备时长。
    /// </summary>
    /// <param name="max"></param>
    /// <returns>装备时长。
    /// <br/>在物品装备时，返回值从0逐渐增加至max；未装备时，从max逐渐减少至0。
    /// </returns>
    public int GetEquippedTimer(int max) => IsEquipped
        ? Math.Clamp(TOWorld.GameTimer.TotalTicks - _lastEquippedTime, 0, max)
        : Math.Clamp(max - TOWorld.GameTimer.TotalTicks + _lastUnequippedTime, 0, max);
}
