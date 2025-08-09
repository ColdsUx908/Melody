namespace Transoceanic.Visual;

public partial class ItemTooltipModifier
{
    protected readonly int _tooltip0;
    protected readonly int _tooltipLast;
    protected readonly int _tooltipMax;

    public readonly List<TooltipLine> Tooltips;

    public static readonly Regex _tooltipRegex = GetTooltipRegex();

    [GeneratedRegex("""^Tooltip(\d+)$""")]
    private static partial Regex GetTooltipRegex();

    public ItemTooltipModifier(List<TooltipLine> tooltips)
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
