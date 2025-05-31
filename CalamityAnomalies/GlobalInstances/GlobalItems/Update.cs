using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override void UpdateInventory(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateInventory(player);
    }

    public override void UpdateInfoAccessory(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateInfoAccessory(player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateEquip(player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateAccessory(player, hideVisual);
    }

    public override void UpdateVanity(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateVanity(player);
    }
}
