using CalamityMod.Items.Accessories;

namespace CalamityAnomalies.Tweaks._4_3_PostPolterghast;

public sealed class ReaperToothNecklace_CA : CAItemTweak<ReaperToothNecklace>, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.TweakLocalizationPrefix + "4.3.ReaperToothNecklace.";

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetArmorPenetration<GenericDamageClass>() += 60;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItem.TooltipModifier.ModifyWithCATweakColorDefault(this, 1);
    }
}
