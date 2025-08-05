namespace Transoceanic.Data;

public abstract class LegendaryItem : ModItem
{
    public bool HasPower = false;

    public virtual void SetPhase(Player player) { }

    /// <summary>
    /// 设置神器之威。
    /// </summary>
    public virtual void SetPower(Player player) => HasPower = false;
}
