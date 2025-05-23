using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.GlobalInstances.GlobalItems;
using CalamityMod;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Transoceanic.Core;
using Transoceanic.Core.ExtraGameData;
using Transoceanic.GlobalInstances;
using Transoceanic.GlobalInstances.GlobalItems;

namespace CalamityAnomalies.Override;

public abstract class CAItemOverride : EntityOverride<Item>
{
    #region 实成员
    protected Item Item { get; set; } = null;

    protected TOGlobalItem OceanItem
    {
        get => field ??= Item?.Ocean();
        set;
    } = null;

    protected CAGlobalItem AnomalyItem
    {
        get => field ??= Item?.Anomaly();
        set;
    } = null;

    protected CalamityGlobalItem CalamityItem
    {
        get => field ??= Item?.Calamity();
        set;
    } = null;

    public override void Connect(Item item)
    {
        Item = item;
        OceanItem = item.Ocean();
        AnomalyItem = item.Anomaly();
        CalamityItem = item.Calamity();
    }

    public override void Disconnect()
    {
        Item = null;
        OceanItem = null;
        AnomalyItem = null;
        CalamityItem = null;
    }
    #endregion

    #region 虚成员

    #region Hit
    public virtual bool? CanHitNPC(Player player, NPC target) => null;

    public virtual bool? CanMeleeAttackCollideWithNPC(Rectangle meleeAttackHitbox, Player player, NPC target) => null;

    public virtual void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) { }

    public virtual void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) { }

    public virtual bool CanHitPvp(Player player, Player target) => true;

    public virtual void ModifyHitPvp(Player player, Player target, ref Player.HurtModifiers modifiers) { }

    public virtual void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) { }
    #endregion
    #endregion
}
