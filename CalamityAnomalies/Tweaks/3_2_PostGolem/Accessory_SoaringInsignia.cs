namespace CalamityAnomalies.Tweaks._3_2_PostGolem;

/* 翱翔徽章
 * 改动
 * 翅膀飞行时间乘1.3（原版：无限飞行，原灾厄：提升25%）。
 * 移动速度乘1.15（原版：提升10%）。
 * 跳跃速度提升36%（与原版相同，原灾厄：10%）。
 * 加速度乘1.35（原版：1.75，原灾厄：1.1）。
 */

public class SoaringInsignia_Tweak : CAItemTweak
{
    private const string localizationPrefix = CAMain.TweakLocalizationPrefix + "3.2.SoaringInsignia.";

    public override int ApplyingType => ItemID.EmpressFlightBooster;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        CreateTooltipModifier(tooltips)
            .ModifyWithCATweakColor(0, k => k.Text = Language.GetTextValue(localizationPrefix + "Tooltip0"))
            .ModifyWithCATweakColor(1, k => k.Text = Language.GetTextValue(localizationPrefix + "Tooltip1"));
    }
}

public class SoaringInsignia_Player : CAPlayerBehavior
{
    public override void PostUpdateMiscEffects()
    {
        if (Player.empressBrooch)
        {
            Player.moveSpeed -= 0.1f;
            if (!CalamityPlayer.ascendantInsignia)
                Player.moveSpeed *= 1.15f;
            AnomalyPlayer.WingTimeMaxMultipliers[1] += 0.3f;
        }
    }
}

public class SoaringInsignia_CalamityPlayer : CalamityPlayerDetour
{
    public override void Detour_OtherBuffEffects(Orig_OtherBuffEffects orig, CalamityPlayer self)
    {
        bool temp = self.Player.empressBrooch;
        self.Player.empressBrooch = false;
        orig(self);
        self.Player.empressBrooch = temp;
    }
}
