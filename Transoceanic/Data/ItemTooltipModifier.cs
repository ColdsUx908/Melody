namespace Transoceanic.Data;

public class ItemTooltipDictionary
{
    public const string Tooltip = "Tooltip";

    public readonly Item _item;
    public readonly List<TooltipLine> _tooltips;
    public Dictionary<(string mod, string name), (int index, TooltipLine line)> _dictionary;

    public ItemTooltipDictionary(Item item, List<TooltipLine> tooltips)
    {
        _item = item;
        _tooltips = tooltips;
        UpdateDictionary();
    }

    public void UpdateDictionary()
    {
        _dictionary = [];
        for (int i = 0; i < _tooltips.Count; i++)
        {
            TooltipLine line = _tooltips[i];
            _dictionary[(line.Mod, line.Name)] = (i, line);
        }
    }

    public bool TryGet(string mod, string name, out int index, out TooltipLine line)
    {
        if (_dictionary.TryGetValue((mod ?? "Terraria", name), out (int index, TooltipLine line) value))
        {
            (index, line) = value;
            return true;
        }
        index = -1;
        line = null;
        return false;
    }

    public virtual ItemTooltipDictionary Modify(string mod, string name, string newText)
    {
        Modify_Inner(mod, name, newText);
        return this;
    }

    public virtual ItemTooltipDictionary Modify(string mod, string name, string newText, Color newColor)
    {
        Modify_Inner(mod, name, newText, newColor);
        return this;
    }

    public virtual ItemTooltipDictionary Modify(string mod, string name, Action<TooltipLine> action)
    {
        Modify_Inner(mod, name, action);
        return this;
    }

    protected void Modify_Inner(string mod, string name, string newText)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
            line.Text = newText;
    }

    protected void Modify_Inner(string mod, string name, string newText, Color newColor)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
        {
            line.Text = newText;
            line.OverrideColor = newColor;
        }
    }

    protected void Modify_Inner(string mod, string name, Action<TooltipLine> action)
    {
        if (TryGet(mod, name, out _, out TooltipLine line))
            action(line);
    }
}

public partial class ItemTooltipModifier : ItemTooltipDictionary
{
    /// <summary>
    /// Tooltip正则表达式。
    /// </summary>
    /// <inheritdoc cref = "GetTooltipRegex" />
    public static readonly Regex _tooltipRegex = GetTooltipRegex();
    [GeneratedRegex("""^Tooltip(\d+)$""")]
    private static partial Regex GetTooltipRegex();

    public ItemTooltipModifier(Item item, List<TooltipLine> tooltips) : base(item, tooltips) { }

    public override ItemTooltipModifier Modify(string mod, string name, string newText)
    {
        Modify_Inner(mod, name, newText);
        return this;
    }

    public override ItemTooltipModifier Modify(string mod, string name, string newText, Color newColor)
    {
        Modify_Inner(mod, name, newText, newColor);
        return this;
    }

    public override ItemTooltipModifier Modify(string mod, string name, Action<TooltipLine> action)
    {
        Modify_Inner(mod, name, action);
        return this;
    }

    public virtual ItemTooltipModifier ModifyTooltip(int num, string newText) => Modify(null, $"{Tooltip}{num}", newText);

    public virtual ItemTooltipModifier ModifyTooltip(int num, string newText, Color newColor) => Modify(null, $"{Tooltip}{num}", newText, newColor);

    public virtual ItemTooltipModifier ModifyTooltip(int num, Action<TooltipLine> action) => Modify(null, $"{Tooltip}{num}", action);
}
