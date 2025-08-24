using CalamityMod.Items.Accessories;

namespace CalamityAnomalies.Tweaks._5_1_PostDoG;

/* 元素之握
 * 
 * 近战伤害提升25%（原灾厄：15%）。
 * 真近战伤害提升30%（原灾厄：20%）。
 * 近战暴击率提升10%（原灾厄：5%）。
 */

public sealed class ElementalGauntlet_Tweak : CAItemTweak<ElementalGauntlet>, ICATweakLocalizationPrefix
{
    CATweakPhase ICATweakLocalizationPrefix.Phase => CATweakPhase.PostDoG;

    string ICATweakLocalizationPrefix.Name => "ElementalGauntlet";

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetDamage<MeleeDamageClass>() += 0.1f;
        player.GetDamage<TrueMeleeDamageClass>() += 0.1f;
        player.GetCritChance<MeleeDamageClass>() += 5;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        CAItemTooltipModifier.Instance
            .ModifyWithCATweakColorDefault(this, 1)
            .ModifyWithCATweakColorDefault(this, 3);
    }
}
