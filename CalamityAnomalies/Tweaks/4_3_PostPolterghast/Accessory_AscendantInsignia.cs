using CalamityAnomalies.Tweaks._5_2_PostYharon;
using CalamityMod.Items.Accessories;
using Terraria.GameInput;

namespace CalamityAnomalies.Tweaks._4_3_PostPolterghast;

/* 进升证章
 * 改动
 * 翅膀飞行时间乘1.5（原灾厄：提升30%）。
 * 移动速度乘1.25（原灾厄：提升10%）。
 * 跳跃速度提升60%（原灾厄：10%）。
 * 继承翱翔徽章的加速度提升效果。
 * 技能冷却时间减为1800（30秒）。
 * 所有效果不与翱翔徽章叠加。
 */

public class AscendantInsignia_Tweak : CAItemTweak<AscendantInsignia>
{
    private const string localizationPrefix = CAMain.TweakLocalizationPrefix + "4.3.AscendantInsignia.";

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        CAPlayer anomalyPlayer = Main.LocalPlayer.Anomaly();
        if (anomalyPlayer.YharimsGift > 0 && anomalyPlayer.YharimsGiftBuff == YharimsGift_Tweak.CurrentBuff.AscendantInsignia)
            YharimsGift_Tweak.DrawEnergyBehindItem(position);
        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.ModifyTooltipByNum_CATweak(0, k => k.Text = Language.GetTextValue(localizationPrefix + "Tooltip0"));
        tooltips.ModifyTooltipByNum_CATweak(1, k => k.Text = Language.GetTextValue(localizationPrefix + "Tooltip1"));
        tooltips.ModifyTooltipByNum_CATweak(3, k => k.Text = Language.GetTextValue(localizationPrefix + "Tooltip3"));
    }
}

public class AscendantInsignia_CalamityPlayer : CalamityPlayerDetour
{
    public override void Detour_ProcessTriggers(Orig_ProcessTriggers orig, CalamityPlayer self, TriggersSet triggersSet)
    {
        orig(self, triggersSet);
        if (self.ascendantInsigniaCooldown == 2400)
            self.ascendantInsigniaCooldown = 1800;
    }

    public override void Detour_OtherBuffEffects(Orig_OtherBuffEffects orig, CalamityPlayer self)
    {
        bool temp = self.ascendantInsignia;
        self.ascendantInsignia = false;
        orig(self);
        self.ascendantInsignia = temp;
    }
}

public class AscendantInsignia_Player : CAPlayerBehavior
{
    public override void PostUpdateMiscEffects()
    {
        if (CalamityPlayer.ascendantInsignia)
        {
            Player.moveSpeed *= 1.25f;
            Player.jumpSpeedBoost += 1.2f;
            AnomalyPlayer.WingTimeMaxMultipliers[1] += 0.2f;
        }
    }
}
