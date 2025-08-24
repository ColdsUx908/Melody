using CalamityMod.Items.Accessories;

namespace CalamityAnomalies.Tweaks._4_3_PostPolterghast;

public sealed class ReaperToothNecklace_CA : CAItemTweak<ReaperToothNecklace>, ICATweakLocalizationPrefix
{
    CATweakPhase ICATweakLocalizationPrefix.Phase => CATweakPhase.PostPolterghast;

    string ICATweakLocalizationPrefix.Name => "ReaperToothNecklace";

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetArmorPenetration<GenericDamageClass>() += 60;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        CAItemTooltipModifier.Instance
            .ModifyWithCATweakColorDefault(this, 1);
    }
}
