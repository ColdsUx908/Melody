using CalamityMod.Items.PermanentBoosters;

namespace CalamityAnomalies.Tweaks;

/* 天体洋葱
 * 
 * 在大师模式下可正常使用，提供一个额外的饰品栏。
 */

public sealed class CelestialOnion_Override : CAItemOverride<CelestialOnion>
{
    public override CAGamePhase Phase => CAGamePhase.PostMoonlord;

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 28;
        Item.rare = ItemRarityID.Red;
        Item.maxStack = 9999;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player) => !player.Calamity().extraAccessoryML;

    public override bool? UseItem(Player player)
    {
        CalamityPlayer modPlayer = player.Calamity();
        if (player.itemAnimation > 0 && !modPlayer.extraAccessoryML && player.itemTime == 0)
        {
            player.itemTime = Item.useTime;
            modPlayer.extraAccessoryML = true;
        }
        return true;
    }
}

public sealed class CelestialOnion_Tweak : CAItemTweak<CelestialOnion>
{
    public override CAGamePhase Phase => CAGamePhase.PostMoonlord;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        new CAItemTooltipModifier(Item, tooltips)
            .ModifyWithCATweakColorDefault(this, 1);
    }
}

public sealed class CelestialOnion_AccessorySlot : ModAccessorySlot
{
    public override bool IsEnabled() => CAServerConfig.Instance.Contents && !Main.gameMenu && Player.Calamity().extraAccessoryML;

    public override bool IsHidden() => IsEmpty && !IsEnabled();
}
