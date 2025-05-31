using CalamityMod;
using CalamityMod.Items.PermanentBoosters;
using Terraria;
using Terraria.ModLoader;
using Transoceanic.ExtraGameData;

namespace CalamityAnomalies.Tweaks._4_1_PostML;

//天体洋葱

public sealed class CelestialOnionDetour : ModItemDetour<CelestialOnion>
{
    public override bool Detour_CanUseItem(Orig_CanUseItem orig, CelestialOnion self, Player player) => !player.Calamity().extraAccessoryML;
}

public sealed class CelestialOnionAccessorySlotDetour : ModAccessorySlotDetour<CelestialOnionAccessorySlot>
{
    public override bool Detour_IsEnabled(Orig_IsEnabled orig, CelestialOnionAccessorySlot self) => ModAccessorySlot.Player.active && ModAccessorySlot.Player.Calamity().extraAccessoryML;
}

/* 为灾厄2.0.5移除天体洋葱饰品栏作准备
public class NewCelestialOnionAccessorySlot : ModAccessorySlot
{
    public override bool IsEnabled() => Player.Calamity().extraAccessoryML;

    public override bool IsHidden() => IsEmpty && !IsEnabled();
}
*/
