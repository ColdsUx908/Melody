namespace Transoceanic.Visual;

public partial class ItemTooltipModifier
{
    protected readonly int _tooltip0;
    protected readonly int _tooltipLast;
    protected readonly int _tooltipMax;

    protected bool TooltipCorrupted => _tooltipMax == _tooltipLast - _tooltip0;

    public List<TooltipLine> Tooltips { get; set; }

    public static readonly Regex _tooltipRegex = GetTooltipRegex();

    [GeneratedRegex("""^Tooltip(\d+)$""")]
    private static partial Regex GetTooltipRegex();

    public ItemTooltipModifier(List<TooltipLine> tooltips)
    {
        Tooltips = tooltips;
        _tooltip0 = tooltips.FindFirstTerrariaTooltipIndex();
        _tooltipLast = tooltips.FindLastTerrariaTooltipIndex(out int num);
        _tooltipMax = num;
    }

    public virtual ItemTooltipModifier Modify(int num, Action<TooltipLine> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_tooltip0 >= 0)
        {
            int index = _tooltip0 + num;
            if (index < Tooltips.Count)
            {
                TooltipLine line = Tooltips[index];
                if (line.Mod == "Terraria" && line.Name == $"Tooltip{num}")
                {
                    action(line);
                    return this;
                }
            }
            for (int i = _tooltip0; i < Tooltips.Count; i++)
            {
                TooltipLine line = Tooltips[i];
                if (line.Mod == "Terraria" && line.Name == $"Tooltip{num}")
                {
                    action(line);
                    return this;
                }
            }
            for (int i = _tooltip0 - 1; i >= 0; i--)
            {
                TooltipLine line = Tooltips[i];
                if (line.Mod == "Terraria" && line.Name == $"Tooltip{num}")
                {
                    action(line);
                    return this;
                }
            }
        }
        return this;
    }
}
