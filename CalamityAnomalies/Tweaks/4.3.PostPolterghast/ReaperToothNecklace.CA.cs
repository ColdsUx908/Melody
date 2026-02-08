using CalamityMod.Items.Accessories;

namespace CalamityAnomalies.Tweaks;

public sealed class ReaperToothNecklace_Override : CAItemOverride<ReaperToothNecklace>
{
    public override CAGamePhase Phase => CAGamePhase.PostPolterghast;

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetDamage<GenericDamageClass>() += 0.15f;
        player.GetArmorPenetration<GenericDamageClass>() += 75;
    }
}

public sealed class ReaperToothNecklace_Tweak : CAItemTweak<ReaperToothNecklace>
{
    public override CAGamePhase Phase => CAGamePhase.PostPolterghast;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        new CAItemTooltipModifier(Item, tooltips)
            .ModifyWithCATweakColorDefault(this, 1);
    }
}
