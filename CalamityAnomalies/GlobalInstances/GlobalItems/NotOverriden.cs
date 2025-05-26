using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override bool? CanConsumeBait(Player player, Item bait) => null;

    public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) { }

    public override bool? CanChooseAmmo(Item weapon, Item ammo, Player player) => null;

    public override bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player) => null;

    public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player) => true;

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) => true;

    public override void OnConsumeAmmo(Item weapon, Item ammo, Player player) { }

    public override void OnConsumedAsAmmo(Item ammo, Item weapon, Player player) { }

    public override void PreUpdateVanitySet(Player player, string set) { }

    public override void UpdateVanitySet(Player player, string set) { }

    public override void ArmorSetShadows(Player player, string set) { }

    public override void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) { }

    public override void ModifyItemLoot(Item item, ItemLoot itemLoot) { }

    public override bool CanStack(Item destination, Item source) => true;

    public override bool CanStackInWorld(Item destination, Item source) => true;

    public override void OnStack(Item destination, Item source, int numToTransfer) { }

    public override void SplitStack(Item destination, Item source, int numToTransfer) { }

    public override void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color,
        ref int glowMask, ref Color glowMaskColor)
    { }

    public override void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) { }

    public override bool WingUpdate(int wings, Player player, bool inUse) => false;

    public override Vector2? HoldoutOffset(int type) => null;

    public override Vector2? HoldoutOrigin(int type) => null;

    public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) => true;

    public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack) { }

    public override void CaughtFishStack(int type, ref int stack) { }

    public override bool IsAnglerQuestAvailable(int type) => true;

    public override void AnglerChat(int type, ref string chat, ref string catchLocation) { }
}
