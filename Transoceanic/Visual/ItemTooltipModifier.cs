namespace Transoceanic.Visual;

public partial class ItemTooltipModifier
{
    public static readonly ItemTooltipModifier Instance = new();

    protected int _tooltip0;
    protected int _tooltipLast;
    protected int _tooltipMax;

    public List<TooltipLine> Tooltips;

    public static readonly Regex _tooltipRegex = GetTooltipRegex();

    [GeneratedRegex("""^Tooltip(\d+)$""")]
    private static partial Regex GetTooltipRegex();

    public virtual ItemTooltipModifier Update(List<TooltipLine> tooltips)
    {
        if (Tooltips != tooltips)
            UpdateInner(tooltips);
        return this;
    }

    protected virtual void UpdateInner(List<TooltipLine> tooltips)
    {
        Tooltips = tooltips;
        _tooltip0 = tooltips.FindFirstTerrariaTooltipIndex();
        _tooltipLast = tooltips.FindLastTerrariaTooltipIndex(out _tooltipMax);
    }

    public virtual ItemTooltipModifier Modify(int num, Action<TooltipLine> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (TryFind(num, out TooltipLine line))
            action(line);
        return this;
    }

    public virtual ItemTooltipModifier Hide(int num)
    {
        if (TryFind(num, out TooltipLine line))
            line.Hide();
        return this;
    }

    public bool TryFind(int num, [NotNullWhen(true)] out TooltipLine line)
    {
        if (_tooltip0 >= 0)
        {
            int index = _tooltip0 + num;
            if (index < Tooltips.Count)
            {
                TooltipLine temp = Tooltips[index];
                if (temp.Mod == "Terraria" && temp.Name == $"Tooltip{num}")
                {
                    line = temp;
                    return true;
                }
            }
            for (int i = _tooltip0; i < Tooltips.Count; i++)
            {
                TooltipLine temp = Tooltips[i];
                if (temp.Mod == "Terraria" && temp.Name == $"Tooltip{num}")
                {
                    line = temp;
                    return true;
                }
            }
            for (int i = _tooltip0 - 1; i >= 0; i--)
            {
                TooltipLine temp = Tooltips[i];
                if (temp.Mod == "Terraria" && temp.Name == $"Tooltip{num}")
                {
                    line = temp;
                    return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < Tooltips.Count; i++)
            {
                TooltipLine temp = Tooltips[i];
                if (temp.Mod == "Terraria" && temp.Name == $"Tooltip{num}")
                {
                    line = temp;
                    return true;
                }
            }
        }
        line = null;
        return false;
    }
}
