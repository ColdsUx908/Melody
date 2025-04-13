using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.Items.ItemRarities;

public class RarityColorOverride : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        TooltipLine nameLine = tooltips.FirstOrDefault(x => x.Name == "ItemName" && x.Mod == "Terraria");
        if (nameLine != null && item.rare == ModContent.RarityType<Celestial>())
        {
            List<Color> colorSet =
            [
                new(188, 192, 193), // white
                new(157, 100, 183), // purple
                new(249, 166, 77), // honey-ish orange
                new(255, 105, 234), // pink
                new(67, 204, 219), // sky blue
                new(249, 245, 99), // bright yellow
                new(236, 168, 247), // purplish pink
            ];
            if (nameLine != null)
            {
                int colorIndex = (int)(Main.GlobalTimeWrappedHourly / 2 % colorSet.Count);
                Color currentColor = colorSet[colorIndex];
                Color nextColor = colorSet[(colorIndex + 1) % colorSet.Count];
                nameLine.OverrideColor = Color.Lerp(currentColor, nextColor, Main.GlobalTimeWrappedHourly % 2f > 1f ? 1f : Main.GlobalTimeWrappedHourly % 1f);
            }
        }
    }
}
