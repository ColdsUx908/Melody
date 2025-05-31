using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnCreated(context);
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnSpawn(source);
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.Update(ref gravity, ref maxFallSpeed);
    }

    public override void PostUpdate(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostUpdate();
    }

    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.GrabRange(player, ref grabRange);
    }

    public override bool GrabStyle(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.GrabStyle(player))
                return false;
        }

        return false;
    }

    public override bool CanPickup(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanPickup(player))
                return false;
        }

        return true;
    }

    public override bool OnPickup(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.OnPickup(player))
                return false;
        }

        return true;
    }

    public override bool ItemSpace(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.ItemSpace(player))
                return false;
        }

        return false;
    }
}
