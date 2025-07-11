
namespace Transoceanic.GlobalInstances;

public class TOGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    private const int dataSlot = 33;
    private const int dataSlot2 = 17;

    public override GlobalItem Clone(Item from, Item to)
    {
        TOGlobalItem clone = (TOGlobalItem)base.Clone(from, to);


        return clone;
    }

    private bool _internalEquipped = false;
    private int _lastEquippedTime = -1;
    private int _lastUnequippedTime = -1;
    private bool ShouldUpdateLastEquippedTime => _internalEquipped && _lastEquippedTime <= _lastUnequippedTime;
    private bool ShouldUpdateLastUnequippedTime => Main.hasFocus && !_internalEquipped && _lastUnequippedTime <= _lastEquippedTime;

    public bool IsEquipped => _lastEquippedTime > _lastUnequippedTime;

    /// <summary>
    /// 获取一个物品的装备时长。
    /// </summary>
    /// <param name="max"></param>
    /// <returns>装备时长。
    /// <br/>在物品装备时，返回值从0逐渐增加至max；未装备时，从max逐渐减少至0。
    /// </returns>
    public int GetEquippedTimer(int max) => IsEquipped
            ? Math.Clamp(TOMain.GameTimer.TotalTicks - _lastEquippedTime, 0, max)
            : Math.Clamp(max - TOMain.GameTimer.TotalTicks + _lastUnequippedTime, 0, max);

    public override void UpdateEquip(Item item, Player player)
    {
        _internalEquipped = true;
        if (ShouldUpdateLastEquippedTime)
            _lastEquippedTime = TOMain.GameTimer.TotalTicks;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (ShouldUpdateLastUnequippedTime)
            _lastUnequippedTime = TOMain.GameTimer.TotalTicks;
        _internalEquipped = false;
    }
}
