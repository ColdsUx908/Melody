namespace Transoceanic.Visual;

public class ItemTooltipModifier
{
    protected readonly int _tooltip0;
    protected readonly int _tooltipLast;
    protected readonly int _tooltipMax;

    protected bool TooltipCorrupted => _tooltipMax == _tooltipLast - _tooltip0;

    public List<TooltipLine> Tooltips { get; set; }

    public ItemTooltipModifier(List<TooltipLine> tooltips)
    {
        Tooltips = tooltips;

        for (int i = 0; i < Tooltips.Count; i++)
        {
            TooltipLine line = Tooltips[i];
            if (line.Mod == "Terraria" && line.Name == "Tooltip0")
                _tooltip0 = i;
        }

        for (int i = tooltips.Count - 1 - 1; i >= _tooltip0; i--)
        {
            TooltipLine line = tooltips[i];
            Match match = Regex.Match(line.Name, @"^Tooltip(\d+)$");
            if (match.Success)
            {
                _tooltipLast = i;
                _tooltipMax = int.Parse(match.Groups[1].Value);
            }
        }
    }

    public virtual ItemTooltipModifier Modify(int num, Action<TooltipLine> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        int index = _tooltip0 + num;
        if (index < Tooltips.Count)
        {
            TooltipLine line = Tooltips[index];
            if (line.Mod == "Terraria" && line.Name == $"Tooltip{num}")
                action(line);
        }
        for (int i = _tooltip0; i < Tooltips.Count; i++)
        {
            TooltipLine line = Tooltips[i];
            if (line.Mod == "Terraria" && line.Name == $"Tooltip{num}")
                action(line);
        }
        for (int i = _tooltip0 - 1; i >= 0; i--)
        {
            TooltipLine line = Tooltips[i];
            if (line.Mod == "Terraria" && line.Name == $"Tooltip{num}")
                action(line);
        }
        return this;
    }
}
