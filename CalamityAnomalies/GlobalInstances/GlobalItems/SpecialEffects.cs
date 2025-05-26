using CalamityAnomalies.Override;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.GetHealLife(player, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.GetHealMana(player, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyManaCost(player, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, Player player, int neededMana)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnMissingMana(player, neededMana);
    }

    public override void OnConsumeMana(Item item, Player player, int manaConsumed)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnConsumeMana(player, manaConsumed);
    }

    public override bool CanResearch(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanResearch())
                return false;
        }
        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnResearched(fullyResearched);
    }

    public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyResearchSorting(ref itemGroup);
    }

    public override bool NeedsAmmo(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.NeedsAmmo(player))
                return false;
        }

        return true;
    }

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UseItemHitbox(player, ref hitbox, ref noHitbox);
    }

    public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.MeleeEffects(player, hitbox);
    }

    public override bool? CanCatchNPC(Item item, NPC target, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.CanCatchNPC(target, player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnCatchNPC(npc, player, failed);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.ConsumeItem(player))
                return false;
        }

        return true;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnConsumeItem(player);
    }

    public override void UseItemFrame(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UseItemFrame(player);
    }

    public override void HoldItemFrame(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.HoldItemFrame(player);
    }

    public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
        ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
                ref maxAscentMultiplier, ref constantAscend);
    }

    public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.HorizontalWingSpeeds(player, ref speed, ref acceleration);
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanEquipAccessory(player, slot, modded))
                return false;
        }

        return true;
    }
}
