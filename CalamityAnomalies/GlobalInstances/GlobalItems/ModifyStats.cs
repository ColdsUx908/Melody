using CalamityAnomalies.Override;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyWeaponDamage(player, ref damage);
    }

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyWeaponKnockback(player, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyWeaponCrit(player, ref crit);
    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override void ModifyItemScale(Item item, Player player, ref float scale)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyItemScale(player, ref scale);
    }
}