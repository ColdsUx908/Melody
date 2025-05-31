using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.PreDrawTooltip(lines, ref x, ref y))
                return false;
        }

        return true;
    }

    public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostDrawTooltip(lines);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.PreDrawTooltipLine(line, ref yOffset))
                return false;
        }

        return true;
    }

    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostDrawTooltipLine(line);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.ModifyTooltips(tooltips);

            tooltips.Add(new(CalamityAnomalies.Instance, "OverrideIdentifier", Language.GetTextValue(CAMain.ModLocalizationPrefix + "Tooltips.OverrideIdentifier")));
        }
    }
}
