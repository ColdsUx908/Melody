namespace Transoceanic.Visual;

public sealed class ItemTooltipDictionary
{
    private readonly Dictionary<(string mod, string name), (int index, TooltipLine line)> _data;

    public ItemTooltipDictionary(List<TooltipLine> tooltips)
    {
        _data = [];
        for (int i = 0; i < tooltips.Count; i++)
        {
            TooltipLine line = tooltips[i];
            _data[(line.Mod, line.Name)] = (i, line);
        }
    }

    public bool TryGet(string mod, string name, out int index, out TooltipLine line)
    {
        mod ??= "Terraria";
        if (_data.TryGetValue((mod, name), out (int index, TooltipLine line) value))
        {
            (index, line) = value;
            return true;
        }
        index = -1;
        line = null;
        return false;
    }

    public bool Modify(string mod, string name, Action<TooltipLine> action)
    {
        mod ??= "Terraria";
        if (TryGet(mod, name, out _, out TooltipLine line))
        {
            action(line);
            return true;
        }
        return false;
    }
}
