using CalamityAnomalies.Override;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override void SetStaticDefaults()
    {
        foreach (CAItemOverride itemOverride in CAOverrideHelper.ItemOverrides.Values)
            itemOverride.SetStaticDefaults();
    }

    public override void SetDefaults(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.SetDefaults();
    }

    public override void AddRecipes()
    {
        foreach (CAItemOverride itemOverride in CAOverrideHelper.ItemOverrides.Values)
            itemOverride.AddRecipes();
    }
}
