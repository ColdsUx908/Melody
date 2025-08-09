namespace CalamityAnomalies.Tweaks._3_2_PostGolem;

/* 翱翔徽章
 * 改动
 * 翅膀飞行时间乘1.3（原版：无限飞行，原灾厄：提升25%）。
 * 移动速度乘1.1（原版：提升10%）。
 * 跳跃速度提升36%（与原版相同，原灾厄：10%）。
 * 加速度乘1.3（原版：1.75，原灾厄：1.1）。
 */

public sealed class SoaringInsignia_Tweak : CAItemTweak, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.TweakLocalizationPrefix + "3.2.SoaringInsignia.";

    public override int ApplyingType => ItemID.EmpressFlightBooster;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItem.TooltipModifier
            .ModifyWithCATweakColorDefault(this, 0)
            .ModifyWithCATweakColorDefault(this, 1);
    }
}

public sealed class SoaringInsignia_Player : CAPlayerBehavior2
{
    public override void PostUpdateMiscEffects()
    {
        if (Player.empressBrooch)
        {
            Player.jumpSpeedBoost += 1.3f;
            Player.moveSpeed -= 0.1f;
            if (!CalamityPlayer.ascendantInsignia)
                Player.moveSpeed *= 1.1f;
            OceanPlayer.WingTimeMaxMultipliers[2] += 0.3f;
        }
    }
}

public sealed class SoaringInsignia_CalamityPlayer : CACalamityPlayerDetour
{
    public override void Detour_OtherBuffEffects(Orig_OtherBuffEffects orig, CalamityPlayer self)
    {
        bool temp = self.Player.empressBrooch;
        self.Player.empressBrooch = false;
        orig(self);
        self.Player.empressBrooch = temp;
    }
}
