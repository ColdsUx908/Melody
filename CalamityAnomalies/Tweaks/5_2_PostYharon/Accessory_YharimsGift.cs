using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using Terraria.GameInput;

namespace CalamityAnomalies.Tweaks._5_2_PostYharon;

public class YharimsGift_Tweak : CAItemTweak<YharimsGift>
{
    private const string localizationPrefix = CAMain.TweakLocalizationPrefix + "5.2.YharimsGift.";

    private const string localizationPrefix2 = CAMain.CalamityModLocalizationPrefix + "Items.Accessories.";

    public enum CurrentBuff
    {
        None = -1,
        AscendantInsignia = 0,
    }

    public static int CurrentBuffNum
    {
        get => (int)Main.LocalPlayer.Anomaly().YharimsGiftBuff;
        set => Main.LocalPlayer.Anomaly().YharimsGiftBuff = (CurrentBuff)(value % (Enum.GetValues<CurrentBuff>().Length - 1));
    }

    public static CurrentBuff CurrentBuffType => Main.LocalPlayer.Anomaly().YharimsGiftBuff;

    public static string BuffName => CurrentBuffType switch
    {
        CurrentBuff.AscendantInsignia => Language.GetTextValue(localizationPrefix2 + "AscendantInsignia.DisplayName"),
        _ => "None"
    };

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CAPlayer anomalyPlayer = player.Anomaly();
        if (anomalyPlayer.YharimsGift < 2)
            anomalyPlayer.YharimsGift++;
        _enchantmentEnergyParticles.Update();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        CreateTooltipModifier(tooltips)
            .ClearAllCATooltips()
            .AddCATooltip(k =>
            {
                k.Text = Language.GetTextFormat(localizationPrefix + "CATooltip0", BuffName);
                k.OverrideColor = Color.Gold;
            }, false);
    }

    private static readonly ChargingEnergyParticleSet _enchantmentEnergyParticles = new(-1, 3, Color.Orange, Color.White, 0.04f, 32f);

    public static void DrawEnergyBehindItem(Vector2 position, Color? edgeColor = null, Color? centerColor = null)
    {
        _enchantmentEnergyParticles.EdgeColor = edgeColor ?? Color.Orange;
        _enchantmentEnergyParticles.CenterColor = centerColor ?? Color.White;
        _enchantmentEnergyParticles.InterpolationSpeed = MathHelper.Lerp(0.065f, 0.1f, TOMathHelper.GetTimeSin(0.5f, 0.7f, TOMathHelper.PiOver3, true));
        _enchantmentEnergyParticles.DrawSet(position + Main.screenPosition);
    }
}

public class YharimsGift_Player : CAPlayerBehavior
{
    private const string localizationPrefix = CAMain.TweakLocalizationPrefix + "5.2.YharimsGift.";

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (CAKeybinds.ChangeYharimsGiftBuff.JustPressed)
        {
            YharimsGift_Tweak.CurrentBuffNum++;
            TOLocalizationUtils.ChatLocalizedTextWith(localizationPrefix + "BuffChange", Main.LocalPlayer, Color.Gold, YharimsGift_Tweak.BuffName);
        }
    }
}
