using CalamityAnomalies.Override;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            int result = itemOverride.ChoosePrefix(rand);
            if (result != -1)
                return result;
        }

        return -1;
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.PrefixChance(pre, rand);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool AllowPrefix(Item item, int pre)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.AllowPrefix(pre))
                return false;
        }

        return true;
    }

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.ReforgePrice(ref reforgePrice, ref canApplyDiscount))
                return false;
        }

        return true;
    }

    public override bool CanReforge(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanReforge())
                return false;
        }

        return true;
    }

    public override void PreReforge(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PreReforge();
    }

    public override void PostReforge(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostReforge();
    }
}
