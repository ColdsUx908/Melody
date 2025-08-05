using CalamityMod.Items.PermanentBoosters;

namespace CalamityAnomalies.Tweaks._4_1_PostML;

/* 天体洋葱
 * 
 * 在大师模式下可正常使用，提供一个额外的饰品栏。
 */

public sealed class CelestialOnion_Tweak : CAItemTweak<CelestialOnion>, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.TweakLocalizationPrefix + "4.1.CelestialOnion.";

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItem.TooltipModifier
            .ModifyWithCATweakColorDefault(this, 1);
    }
}

public sealed class CelestialOnion_Detour : CAModItemDetour<CelestialOnion>
{
    public override bool Detour_CanUseItem(Orig_CanUseItem orig, CelestialOnion self, Player player) => !player.Calamity().extraAccessoryML;
}

public sealed class CelestialOnion_AccessorySlot : ModAccessorySlot
{
    public override bool IsEnabled() => CAServerConfig.Instance.Contents && !Main.gameMenu && Player.Calamity().extraAccessoryML;

    public override bool IsHidden() => IsEmpty && !IsEnabled();
}
