namespace Transoceanic.Visual;

public class ItemTooltipDictionary
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
}
