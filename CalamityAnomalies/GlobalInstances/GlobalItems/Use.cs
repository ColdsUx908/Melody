using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    /// <inheritdoc cref="ModItem.AltFunctionUse(Player)"/>
    public override bool AltFunctionUse(Item item, Player player)
    {
        return false;
    }

    public override bool? CanAutoReuseItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.CanAutoReuseItem(player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UseStyle(player, heldItemFrame);
    }

    public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.HoldStyle(player, heldItemFrame);
    }

    public override void HoldItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.HoldItem(player);
    }

    public override float UseTimeMultiplier(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            float result = itemOverride.UseTimeMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override float UseAnimationMultiplier(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            float result = itemOverride.UseAnimationMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override float UseSpeedMultiplier(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            float result = itemOverride.UseSpeedMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.UseItem(player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void UseAnimation(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UseAnimation(player);
    }

    public override bool CanShoot(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanShoot(player))
                return false;
        }

        return true;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.Shoot(player, source, position, velocity, type, damage, knockback))
                return false;
        }

        return true;
    }

    public override bool CanRightClick(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanRightClick())
                return false;
        }

        return false;
    }

    public override void RightClick(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.RightClick(player);
    }
}
