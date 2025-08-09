using CalamityAnomalies.Tweaks._5_2_PostYharon;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Accessories;
using Terraria.GameInput;

namespace CalamityAnomalies.Tweaks._4_3_PostPolterghast;

/* 进升证章
 * 
 * 翅膀飞行时间乘1.5（原灾厄：提升30%）。
 * 移动速度乘1.2（原灾厄：提升10%）。
 * 跳跃速度提升60%（原灾厄：10%）。
 * 继承翱翔徽章的加速度提升效果。
 * 技能冷却时间减为1500（原灾厄：2400）。
 * 所有效果不与翱翔徽章叠加。
 * 
 * 寻神者之礼祝福
 * 无限飞行。
 * 飞升状态时：
 *   移动速度乘数增加至1.4。
 *   跳跃速度再增加120%。
 */

public sealed class AscendantInsignia_Tweak : CAItemTweak<AscendantInsignia>, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.TweakLocalizationPrefix + "4.3.AscendantInsignia.";

    public static bool HasYharimsGiftBuff
    {
        get
        {
            CAPlayer anomalyPlayer = Main.LocalPlayer.Anomaly();
            return anomalyPlayer.YharimsGift && anomalyPlayer.YharimsGift_Blessing == YharimsGift_CurrentBlessing.AscendantInsignia;
        }
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        YharimsGift_Handler.DrawEnergyAndBorderBehindItem(Item, spriteBatch, position, frame, origin, scale);
        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItem.TooltipModifier
            .ModifyWithCATweakColorDefault(this, 0)
            .ModifyWithCATweakColorDefault(this, 1)
            .ModifyWithCATweakColorDefault(this, 3);
        if (HasYharimsGiftBuff)
        {
            AnomalyItem.TooltipModifier.ClearAllCATooltips();
            YharimsGift_Handler.AddBlessingTooltip(Item);
            YharimsGift_Handler.AddGoldTooltip(Item, l => l.Text = this.GetTextFormatWithPrefix("BlessingTooltip0"));
            YharimsGift_Handler.AddGoldTooltip(Item, l => l.Text = this.GetTextFormatWithPrefix("BlessingTooltip1"));
            YharimsGift_Handler.AddGoldTooltip(Item, l => l.Text = this.GetTextFormatWithPrefix("BlessingTooltip2"));
        }
    }
}

public sealed class AscendantInsignia_Player : CAPlayerBehavior2
{
    public override void PostUpdateMiscEffects()
    {
        if (CalamityPlayer.ascendantInsignia)
        {
            Player.moveSpeed *= 1.2f;
            Player.jumpSpeedBoost += 1.2f;
            OceanPlayer.WingTimeMaxMultipliers[2] += 0.2f;
            if (AscendantInsignia_Tweak.HasYharimsGiftBuff)
            {
                CalamityPlayer.infiniteFlight = true;
                if (CalamityPlayer.ascendantInsigniaBuffTime > 0)
                {
                    Player.moveSpeed *= 7f / 6f;
                    Player.jumpSpeedBoost += 6f;
                }
                if (CalamityPlayer.ascendantInsigniaCooldown is >= 1470 and < 1500)
                    Player.moveSpeed *= 0.985f;
            }
        }
    }
}

public sealed class AscendantInsignia_CalamityPlayer : CACalamityPlayerDetour
{
    public override void Detour_ProcessTriggers(Orig_ProcessTriggers orig, CalamityPlayer self, TriggersSet triggersSet)
    {
        orig(self, triggersSet);
        if (CalamityKeybinds.AscendantInsigniaHotKey.JustPressed && self.ascendantInsigniaCooldown == 2400)
            self.ascendantInsigniaCooldown = 1500;
    }

    public override void Detour_MiscEffects(Orig_MiscEffects orig, CalamityPlayer self)
    {
        if (!self.ascendantInsignia && self.ascendantInsigniaBuffTime > 0)
        {
            self.ascendantInsigniaBuffTime = 0;
            self.ascendantInsigniaCooldown = 1500;
            self.Player.AddCooldown(AscendEffect.ID, 1500);
        }
        orig(self);
    }

    public override void Detour_OtherBuffEffects(Orig_OtherBuffEffects orig, CalamityPlayer self)
    {
        bool temp = self.ascendantInsignia;
        self.ascendantInsignia = false;
        orig(self);
        self.ascendantInsignia = temp;
        if (self.ascendantInsignia && self.ascendantInsigniaBuffTime > 0)
        {
            self.ascendantTrail = true;
            self.infiniteFlight = true;
            if (self.ascendantInsigniaBuffTime == 1)
                self.Player.AddCooldown(AscendEffect.ID, 1500);
            self.ascendantInsigniaBuffTime--;
        }
    }
}
