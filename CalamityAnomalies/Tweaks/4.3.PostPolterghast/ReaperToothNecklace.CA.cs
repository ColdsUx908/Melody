using CalamityMod.Items.Accessories;
using Terraria;

namespace CalamityAnomalies.Tweaks;

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
        new CAItemTooltipModifier(Item, tooltips)
            .ModifyWithCATweakColorDefault(this, 1);
    }
}
