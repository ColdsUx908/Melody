using CalamityMod.Items.PermanentBoosters;

namespace CalamityAnomalies.Tweaks._4_1_PostML;

//天体洋葱

public class CelestialOnionDetour : ModItemDetour<CelestialOnion>
{
    public override bool Detour_CanUseItem(Orig_CanUseItem orig, CelestialOnion self, Player player) => !player.Calamity().extraAccessoryML;
}

public class NewCelestialOnionAccessorySlot : ModAccessorySlot
{
    public override bool IsEnabled() => CAMain.Tweak && !Main.gameMenu && Player.Calamity().extraAccessoryML;

    public override bool IsHidden() => IsEmpty && !IsEnabled();
}
