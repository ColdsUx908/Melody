using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.CanHitNPC(player, target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.ModifyHitNPC(player, target, ref modifiers);
        }
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.OnHitNPC(player, target, hit, damageDone);
        }
    }

    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool result = itemOverride.CanHitPvp(player, target);
            if (!result)
                return false;
        }

        return true;
    }

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.ModifyHitPvp(player, target, ref modifiers);
        }
    }

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.OnHitPvp(player, target, hurtInfo);
        }
    }
}
