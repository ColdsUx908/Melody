using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using Terraria.GameInput;

namespace CalamityAnomalies.Tweaks._5_2_PostYharon;

public enum YharimsGift_CurrentBlessing
{
    None,
    AscendantInsignia,
}

public static class YharimsGift_Handler
{
    public static readonly ChargingEnergyParticleSet _enchantmentEnergyParticles = new(-1, 5, GiftColor, Color.White, 0.04f, 32f);

    public static readonly int _totalBlessings = Enum.GetValues<YharimsGift_CurrentBlessing>().Length;

    public const string LocalizationPrefix = CAMain.TweakLocalizationPrefix + "5.2.YharimsGift.";

    public static YharimsGift_CurrentBlessing CurrentBlessing => Main.LocalPlayer.Anomaly().YharimsGift_Blessing;

    public static int CurrentBlessingType => CurrentBlessing switch
    {
        YharimsGift_CurrentBlessing.AscendantInsignia => ModContent.ItemType<AscendantInsignia>(),
        _ => -1
    };

    public static int GetBlessingIndex(int type)
    {
        if (type == ModContent.ItemType<AscendantInsignia>())
            return (int)YharimsGift_CurrentBlessing.AscendantInsignia;

        return 0;
    }

    public static string BlessingName => ModContent.GetModItem(CurrentBlessingType)?.DisplayName.Value ?? "None";

    public static Color GiftColor => Color.Lerp(Color.Orange, Color.Gold, TOMathHelper.GetTimeSin(0.4f, 0.7f, TOMathHelper.PiOver3, true));

    public static Color GiftColor2 => Color.Lerp(Color.OrangeRed, GiftColor, 0.5f);

    public static void AddBlessingTooltip() => CAItemTooltipModifier.Instance.AddCATooltip(l =>
    {
        l.Text = Language.GetTextValue(LocalizationPrefix + "BlessingTooltip");
        l.OverrideColor = GiftColor;
    });

    public static void AddGoldTooltip(Action<TooltipLine> action) => CAItemTooltipModifier.Instance.AddCATooltip(l =>
    {
        l.OverrideColor = Color.Gold;
        action(l);
    });

    public static void DrawEnergyAndBorderBehindItem(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale)
    {
        CAPlayer anomalyPlayer = Main.LocalPlayer.Anomaly();
        if (anomalyPlayer.YharimsGift && CurrentBlessingType == item.type)
        {
            _enchantmentEnergyParticles.InterpolationSpeed = MathHelper.Lerp(0.065f, 0.1f, TOMathHelper.GetTimeSin(0.5f, 0.7f, TOMathHelper.PiOver3, true));
            _enchantmentEnergyParticles.DrawSet(position + Main.screenPosition);
        }
        if (anomalyPlayer.YharimsGift_Last is not null)
        {
            TOGlobalItem blessingOcean = item.Ocean();
            TOGlobalItem giftOcean = anomalyPlayer.YharimsGift_Last.Ocean();
            int index = GetBlessingIndex(item.type);
            if (index != 0)
            {
                int buffTimer = anomalyPlayer.YharimsGift_Change[index].GetValue(TOWorld.GameTimer.TotalTicks, 40, item.type == CurrentBlessingType);
                int timer = TOMathHelper.Min(blessingOcean.GetEquippedTimer(40), giftOcean.GetEquippedTimer(40), buffTimer);
                item.DrawInventoryWithBorder(spriteBatch, position, frame, origin, scale, 24, (TOMathHelper.GetTimeSin(0.3f, 0.7f, TOMathHelper.PiOver12, true) + 2.8f) * timer / 40f, GiftColor2);
            }
        }
    }
}

public sealed class YharimsGift_Tweak : CAItemTweak<YharimsGift>, ICATweakLocalizationPrefix
{
    CATweakPhase ICATweakLocalizationPrefix.Phase => CATweakPhase.PostYharon;

    string ICATweakLocalizationPrefix.Name => "YharimsGift";

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CAPlayer anomalyPlayer = player.Anomaly();
        anomalyPlayer.YharimsGift_Last = Item;
        anomalyPlayer.YharimsGift.Value = true;
        YharimsGift_Handler._enchantmentEnergyParticles.Update();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        CAItemTooltipModifier.Instance
            .ClearAllCATooltips()
            .AddCATooltip(l =>
            {
                l.Text = this.GetTextFormatWithPrefix("CATooltip0", YharimsGift_Handler.BlessingName);
                l.OverrideColor = YharimsGift_Handler.GiftColor;
            });
    }
}

public sealed class YharimsGift_Player : CAPlayerBehavior2, ICATweakLocalizationPrefix
{
    CATweakPhase ICATweakLocalizationPrefix.Phase => CATweakPhase.PostYharon;

    string ICATweakLocalizationPrefix.Name => "YharimsGift";

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (CAKeybinds.ChangeYharimsGiftBuff.JustPressed)
        {
            AnomalyPlayer.YharimsGift_Blessing = (YharimsGift_CurrentBlessing)(((int)AnomalyPlayer.YharimsGift_Blessing + 1) % YharimsGift_Handler._totalBlessings);
            for (int i = 0; i < YharimsGift_Handler._totalBlessings; i++)
            {
                if (i == (int)AnomalyPlayer.YharimsGift_Blessing)
                    AnomalyPlayer.YharimsGift_Change[i].LastOn = TOWorld.GameTimer.TotalTicks;
                else
                    AnomalyPlayer.YharimsGift_Change[i].LastOff = TOWorld.GameTimer.TotalTicks;
            }
            TOLocalizationUtils.ChatLiteralText(this.GetTextFormatWithPrefix("BlessingChange", YharimsGift_Handler.BlessingName), Color.Gold, Main.LocalPlayer);
        }
    }

    public override void ResetEffects()
    {
        AnomalyPlayer.YharimsGift.Value = false;
    }

    public override void SaveData(TagCompound tag) => tag["YharimsGift_Blessing"] = (int)AnomalyPlayer.YharimsGift_Blessing;

    public override void LoadData(TagCompound tag) => AnomalyPlayer.YharimsGift_Blessing = tag.TryGet("YharimsGift_Blessing", out int yharimsGiftBuff) ? (YharimsGift_CurrentBlessing)yharimsGiftBuff : YharimsGift_CurrentBlessing.None;
}
