using CalamityAnomalies.Items.ItemRarities;

namespace CalamityAnomalies.GlobalInstances;

public sealed class CAGlobalItem : GlobalItemWithBehavior<CAItemBehavior>
{
    public override SingleEntityBehaviorSet<Item, CAItemBehavior> BehaviorSet => CABehaviorHelper.ItemBehaviors;

    private const int dataSlot = 33;
    private const int dataSlot2 = 17;

    public Union32[] Data { get; } = new Union32[dataSlot];
    public Union64[] Data2 { get; } = new Union64[dataSlot2];

    public override GlobalItem Clone(Item from, Item to)
    {
        CAGlobalItem clone = (CAGlobalItem)base.Clone(from, to);

        Array.Copy(Data, clone.Data, dataSlot);
        Array.Copy(Data2, clone.Data2, dataSlot2);

        return clone;
    }

    #region Defaults
    public override void SetStaticDefaults() => base.SetStaticDefaults();

    public override void SetDefaults(Item item) => base.SetDefaults(item);

    public override void AddRecipes() => base.AddRecipes();
    #endregion Defaults

    #region Lifetime
    public override void OnCreated(Item item, ItemCreationContext context) => base.OnCreated(item, context);

    public override void OnSpawn(Item item, IEntitySource source) => base.OnSpawn(item, source);

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed) => base.Update(item, ref gravity, ref maxFallSpeed);

    public override void PostUpdate(Item item) => base.PostUpdate(item);

    public override void GrabRange(Item item, Player player, ref int grabRange) => base.GrabRange(item, player, ref grabRange);

    public override bool GrabStyle(Item item, Player player) => base.GrabStyle(item, player);

    public override bool CanPickup(Item item, Player player) => base.CanPickup(item, player);

    public override bool OnPickup(Item item, Player player) => base.OnPickup(item, player);

    public override bool ItemSpace(Item item, Player player) => base.ItemSpace(item, player);
    #endregion Lifetime

    #region Update
    public override void UpdateInventory(Item item, Player player) => base.UpdateInventory(item, player);

    public override void UpdateInfoAccessory(Item item, Player player) => base.UpdateInfoAccessory(item, player);

    public override void UpdateEquip(Item item, Player player) => base.UpdateEquip(item, player);

    public override void UpdateAccessory(Item item, Player player, bool hideVisual) => base.UpdateAccessory(item, player, hideVisual);

    public override void UpdateVanity(Item item, Player player) => base.UpdateVanity(item, player);

    public override void UpdateVisibleAccessory(Item item, Player player, bool hideVisual) => base.UpdateVisibleAccessory(item, player, hideVisual);

    public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual) => base.UpdateItemDye(item, player, dye, hideVisual);
    #endregion Update

    #region Draw
    public override Color? GetAlpha(Item item, Color lightColor) => base.GetAlpha(item, lightColor);

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => base.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => base.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    #endregion Draw

    #region Prefix
    public override int ChoosePrefix(Item item, UnifiedRandom rand) => base.ChoosePrefix(item, rand);

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand) => base.PrefixChance(item, pre, rand);

    public override bool AllowPrefix(Item item, int pre) => base.AllowPrefix(item, pre);

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) => base.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);

    public override bool CanReforge(Item item) => base.CanReforge(item);

    public override void PreReforge(Item item) => base.PreReforge(item);

    public override void PostReforge(Item item) => base.PostReforge(item);
    #endregion Prefix

    #region Use
    public override bool AltFunctionUse(Item item, Player player) => base.AltFunctionUse(item, player);

    public override bool CanUseItem(Item item, Player player) => base.CanUseItem(item, player);

    public override bool? CanAutoReuseItem(Item item, Player player) => base.CanAutoReuseItem(item, player);

    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => base.UseStyle(item, player, heldItemFrame);

    public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame) => base.HoldStyle(item, player, heldItemFrame);

    public override void HoldItem(Item item, Player player) => base.HoldItem(item, player);

    public override float UseTimeMultiplier(Item item, Player player) => base.UseTimeMultiplier(item, player);

    public override float UseAnimationMultiplier(Item item, Player player) => base.UseAnimationMultiplier(item, player);

    public override float UseSpeedMultiplier(Item item, Player player) => base.UseSpeedMultiplier(item, player);

    public override bool? UseItem(Item item, Player player) => base.UseItem(item, player);

    public override void UseAnimation(Item item, Player player) => base.UseAnimation(item, player);

    public override bool CanShoot(Item item, Player player) => base.CanShoot(item, player);

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => base.Shoot(item, player, source, position, velocity, type, damage, knockback);

    public override bool CanRightClick(Item item) => base.CanRightClick(item);

    public override void RightClick(Item item, Player player) => base.RightClick(item, player);
    #endregion Use

    #region ModifyStats
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) => base.ModifyWeaponDamage(item, player, ref damage);

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) => base.ModifyWeaponKnockback(item, player, ref knockback);

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit) => base.ModifyWeaponCrit(item, player, ref crit);

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => base.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);

    public override void ModifyItemScale(Item item, Player player, ref float scale) => base.ModifyItemScale(item, player, ref scale);
    #endregion ModifyStats

    #region Hit
    public override bool? CanHitNPC(Item item, Player player, NPC target) => base.CanHitNPC(item, player, target);

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target) => base.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, player, target);

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers) => base.ModifyHitNPC(item, player, target, ref modifiers);

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone) => base.OnHitNPC(item, player, target, hit, damageDone);

    public override bool CanHitPvp(Item item, Player player, Player target) => base.CanHitPvp(item, player, target);

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers) => base.ModifyHitPvp(item, player, target, ref modifiers);

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo) => base.OnHitPvp(item, player, target, hurtInfo);
    #endregion Hit

    #region SpecialEffects
    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue) => base.GetHealLife(item, player, quickHeal, ref healValue);

    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue) => base.GetHealMana(item, player, quickHeal, ref healValue);

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult) => base.ModifyManaCost(item, player, ref reduce, ref mult);

    public override void OnMissingMana(Item item, Player player, int neededMana) => base.OnMissingMana(item, player, neededMana);

    public override void OnConsumeMana(Item item, Player player, int manaConsumed) => base.OnConsumeMana(item, player, manaConsumed);

    public override bool CanResearch(Item item) => base.CanResearch(item);

    public override void OnResearched(Item item, bool fullyResearched) => base.OnResearched(item, fullyResearched);

    public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => base.ModifyResearchSorting(item, ref itemGroup);

    public override bool NeedsAmmo(Item item, Player player) => base.NeedsAmmo(item, player);

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) => base.UseItemHitbox(item, player, ref hitbox, ref noHitbox);

    public override void MeleeEffects(Item item, Player player, Rectangle hitbox) => base.MeleeEffects(item, player, hitbox);

    public override bool? CanCatchNPC(Item item, NPC target, Player player) => base.CanCatchNPC(item, target, player);

    public override void OnCatchNPC(Item item, NPC npc, Player player, bool failed) => base.OnCatchNPC(item, npc, player, failed);

    public override bool ConsumeItem(Item item, Player player) => base.ConsumeItem(item, player);

    public override void OnConsumeItem(Item item, Player player) => base.OnConsumeItem(item, player);

    public override void UseItemFrame(Item item, Player player) => base.UseItemFrame(item, player);

    public override void HoldItemFrame(Item item, Player player) => base.HoldItemFrame(item, player);

    public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) => base.VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);

    public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration) => base.HorizontalWingSpeeds(item, player, ref speed, ref acceleration);

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded) => base.CanEquipAccessory(item, player, slot, modded);
    #endregion SpecialEffects

    #region Tooltip
    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => base.PreDrawTooltip(item, lines, ref x, ref y);

    public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines) => base.PostDrawTooltip(item, lines);

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) => base.PreDrawTooltipLine(item, line, ref yOffset);

    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line) => base.PostDrawTooltipLine(item, line);

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (TryGetBehavior(item, out CAItemBehavior itemBehavior))
        {
            itemBehavior.ModifyTooltips(tooltips);
            tooltips.Add(new(CalamityAnomalies.Instance, "OverrideIdentifier", Language.GetTextValue(CAMain.ModLocalizationPrefix + "Tooltips.OverrideIdentifier")));
        }

        TooltipLine nameLine = tooltips.AsValueEnumerable().FirstOrDefault(k => k.Name == "ItemName" && k.Mod == "Terraria");
        if (nameLine is not null && item.rare == ModContent.RarityType<Celestial>())
        {
        }
    }
    #endregion Tooltip

    #region WorldSaving
    public override void SaveData(Item item, TagCompound tag) => base.SaveData(item, tag);

    public override void LoadData(Item item, TagCompound tag) => base.LoadData(item, tag);
    #endregion WorldSaving

    #region Net
    public override void NetSend(Item item, BinaryWriter writer) => base.NetSend(item, writer);

    public override void NetReceive(Item item, BinaryReader reader) => base.NetReceive(item, reader);
    #endregion Net

    #region NotOverriden
    public override bool? CanConsumeBait(Player player, Item bait) => null;

    public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) { }

    public override bool? CanChooseAmmo(Item weapon, Item ammo, Player player) => null;

    public override bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player) => null;

    public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player) => true;

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player) => true;

    public override void OnConsumeAmmo(Item weapon, Item ammo, Player player) { }

    public override void OnConsumedAsAmmo(Item ammo, Item weapon, Player player) { }

    public override string IsArmorSet(Item head, Item body, Item legs) => base.IsArmorSet(head, body, legs);

    public override void UpdateArmorSet(Player player, string set) { }

    public override string IsVanitySet(int head, int body, int legs) => base.IsVanitySet(head, body, legs);

    public override void PreUpdateVanitySet(Player player, string set) { }

    public override void UpdateVanitySet(Player player, string set) { }

    public override void ArmorSetShadows(Player player, string set) { }

    public override void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) { }

    public override void ModifyItemLoot(Item item, ItemLoot itemLoot) { }

    public override bool CanStack(Item destination, Item source) => true;

    public override bool CanStackInWorld(Item destination, Item source) => true;

    public override void OnStack(Item destination, Item source, int numToTransfer) { }

    public override void SplitStack(Item destination, Item source, int numToTransfer) { }

    public override void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) { }

    public override void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) { }

    public override bool WingUpdate(int wings, Player player, bool inUse) => false;

    public override Vector2? HoldoutOffset(int type) => null;

    public override Vector2? HoldoutOrigin(int type) => null;

    public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) => true;

    public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack) { }

    public override void CaughtFishStack(int type, ref int stack) { }

    public override bool IsAnglerQuestAvailable(int type) => true;

    public override void AnglerChat(int type, ref string chat, ref string catchLocation) { }
    #endregion NotOverriden
}
