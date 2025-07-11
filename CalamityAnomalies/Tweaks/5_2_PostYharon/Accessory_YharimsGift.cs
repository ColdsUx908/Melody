using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using Terraria.GameInput;

namespace CalamityAnomalies.Tweaks._5_2_PostYharon;

public enum YharimsGift_CurrentBlessing
{
    None = -1,
    AscendantInsignia = 0,
}

public static class YharimsGift_Handler
{
    internal static readonly ChargingEnergyParticleSet _enchantmentEnergyParticles = new(-1, 5, Color.Orange, Color.White, 0.04f, 32f);

    public const string LocalizationPrefix = CAMain.TweakLocalizationPrefix + "5.2.YharimsGift.";

    public const string LocalizationPrefix2 = CAMain.CalamityModLocalizationPrefix + "Items.Accessories.";

    public static int CurrentBlessingNum
    {
        get => (int)Main.LocalPlayer.Anomaly().YharimsGiftBuff;
        set => Main.LocalPlayer.Anomaly().YharimsGiftBuff = (YharimsGift_CurrentBlessing)(value % (Enum.GetValues<YharimsGift_CurrentBlessing>().Length - 1));
    }

    public static YharimsGift_CurrentBlessing CurrentBuffType => Main.LocalPlayer.Anomaly().YharimsGiftBuff;

    public static string BlessingName => CurrentBuffType switch
    {
        YharimsGift_CurrentBlessing.AscendantInsignia => Language.GetTextValue(LocalizationPrefix2 + "AscendantInsignia.DisplayName"),
        _ => "None"
    };

    public static Color GiftColor => Color.Lerp(Color.Orange, Color.Gold, TOMathHelper.GetTimeSin(0.4f, 0.7f, TOMathHelper.PiOver3, true));

    public static Color GiftColor2 => Color.Lerp(Color.OrangeRed, GiftColor, 0.5f);

    public static void AddBlessingTooltip(Item item) => item.Anomaly().TooltipModifier.AddCATooltip(l =>
    {
        l.Text = Language.GetTextValue(LocalizationPrefix + "BlessingTooltip");
        l.OverrideColor = GiftColor;
    }, false);

    public static void DrawEnergyAndBorderBehindItem(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale)
    {
        _enchantmentEnergyParticles.EdgeColor = GiftColor;
        _enchantmentEnergyParticles.CenterColor = Color.White;
        _enchantmentEnergyParticles.InterpolationSpeed = MathHelper.Lerp(0.065f, 0.1f, TOMathHelper.GetTimeSin(0.5f, 0.7f, TOMathHelper.PiOver3, true));
        _enchantmentEnergyParticles.DrawSet(position + Main.screenPosition);
        TOGlobalItem oceanItem = item.Ocean();
        item.DrawInventoryWithBorder(spriteBatch, position, frame, origin, scale, 24, (TOMathHelper.GetTimeSin(0.5f, 0.7f, TOMathHelper.PiOver12, true) + 2.5f) * oceanItem.GetEquippedTimer(40) / 40f, GiftColor2);
    }
}

public sealed class YharimsGift_Tweak : CAItemTweak<YharimsGift>, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.TweakLocalizationPrefix + "5.2.YharimsGift.";

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CAPlayer anomalyPlayer = player.Anomaly();
        anomalyPlayer.YharimsGift += 2;
        YharimsGift_Handler._enchantmentEnergyParticles.Update();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItem.TooltipModifier
            .AddCATooltip(l =>
            {
                l.Text = this.GetTextFormatWithPrefix("CATooltip0", YharimsGift_Handler.BlessingName);
                l.OverrideColor = YharimsGift_Handler.GiftColor;
            }, false);
    }
}

public sealed class YharimsGift_Player : CAPlayerBehavior, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.TweakLocalizationPrefix + "5.2.YharimsGift.";

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (CAKeybinds.ChangeYharimsGiftBuff.JustPressed)
        {
            YharimsGift_Handler.CurrentBlessingNum++;
            TOLocalizationUtils.ChatLocalizedTextFormat(LocalizationPrefix + "BlessingChange", Main.LocalPlayer, Color.Gold, YharimsGift_Handler.BlessingName);
        }
    }
}
