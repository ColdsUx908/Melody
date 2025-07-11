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
 * 技能冷却时间减为1500（25秒）。
 * 所有效果不与翱翔徽章叠加。
 */

public sealed class AscendantInsignia_Tweak : CAItemTweak<AscendantInsignia>, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.TweakLocalizationPrefix + "4.3.AscendantInsignia.";

    public static bool HasYharimsGiftBuff
    {
        get
        {
            CAPlayer anomalyPlayer = Main.LocalPlayer.Anomaly();
            return anomalyPlayer.HasYharimsGift && anomalyPlayer.YharimsGiftBuff == YharimsGift_CurrentBlessing.AscendantInsignia;
        }
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (HasYharimsGiftBuff)
            YharimsGift_Handler.DrawEnergyAndBorderBehindItem(Item, spriteBatch, position, frame, origin, scale);
        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItem.TooltipModifier
            .ModifyWithCATweakColor(0, l => l.Text = this.GetTextValueWithPrefix("Tooltip0"))
            .ModifyWithCATweakColor(1, l => l.Text = this.GetTextValueWithPrefix("Tooltip1"))
            .ModifyWithCATweakColor(3, l => l.Text = this.GetTextValueWithPrefix("Tooltip3"));
        if (HasYharimsGiftBuff)
        {
            YharimsGift_Handler.AddBlessingTooltip(Item);
        }
    }
}

public sealed class AscendantInsignia_Player : CAPlayerBehavior
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

public sealed class AscendantInsignia_CalamityPlayer : CalamityPlayerDetour
{
    public override void Detour_ProcessTriggers(Orig_ProcessTriggers orig, CalamityPlayer self, TriggersSet triggersSet)
    {
        orig(self, triggersSet);
        if (CalamityKeybinds.AscendantInsigniaHotKey.JustPressed)
        {
            if (self.ascendantInsigniaCooldown == 2400)
                self.ascendantInsigniaCooldown = 1500;
        }
    }

    public override void Detour_OtherBuffEffects(Orig_OtherBuffEffects orig, CalamityPlayer self)
    {
        bool temp = self.ascendantInsignia;
        self.ascendantInsignia = false;
        orig(self);
        self.ascendantInsignia = temp;
    }
}
