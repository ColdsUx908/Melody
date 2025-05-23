using CalamityAnomalies.Override;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        if (item.HasItemOverride(out CAItemOverride itemOverride))
        {
            return itemOverride.CanHitNPC(player, target);
        }

        return null;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        if (item.HasItemOverride(out CAItemOverride itemOverride))
        {
            return itemOverride.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);
        }

        return null;
    }

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (item.HasItemOverride(out CAItemOverride itemOverride))
        {
            itemOverride.ModifyHitNPC(player, target, ref modifiers);
        }
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (item.HasItemOverride(out CAItemOverride itemOverride))
        {
            itemOverride.OnHitNPC(player, target, hit, damageDone);
        }
    }

    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        if (item.HasItemOverride(out CAItemOverride itemOverride))
        {
            return itemOverride.CanHitPvp(player, target);
        }

        return true;
    }

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        if (item.HasItemOverride(out CAItemOverride itemOverride))
        {
            itemOverride.ModifyHitPvp(player, target, ref modifiers);
        }
    }

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        if (item.HasItemOverride(out CAItemOverride itemOverride))
        {
            itemOverride.OnHitPvp(player, target, hurtInfo);
        }
    }
}
