using System.IO;
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
        if (item.HasItemOverride(out CAItemOverride itemOverride))
            itemOverride.SetDefaults();
    }
}
