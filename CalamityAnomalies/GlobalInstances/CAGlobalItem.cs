using CalamityAnomalies;
using CalamityAnomalies.Items.ItemRarities;

namespace CalamityAnomalies.GlobalInstances;

public class CAGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    private const int dataSlot = 32;
    private const int dataSlot2 = 16;

    public DataUnion32[] Data { get; } = new DataUnion32[dataSlot];
    public DataUnion64[] Data2 { get; } = new DataUnion64[dataSlot2];

    public override GlobalItem Clone(Item from, Item to)
    {
        CAGlobalItem clone = (CAGlobalItem)base.Clone(from, to);

        Array.Copy(Data, clone.Data, dataSlot);
        Array.Copy(Data2, clone.Data2, dataSlot2);

        return clone;
    }

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (CAItemBehavior itemBehavior in CABehaviorHelper.ItemBehaviors)
            itemBehavior.SetStaticDefaults();
    }

    public override void SetDefaults(Item item)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.SetDefaults();
    }

    public override void AddRecipes()
    {
        foreach (CAItemBehavior itemBehavior in CABehaviorHelper.ItemBehaviors)
            itemBehavior.AddRecipes();
    }
    #endregion Defaults

    #region Lifetime
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnCreated(context);
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnSpawn(source);
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.Update(ref gravity, ref maxFallSpeed);
    }

    public override void PostUpdate(Item item)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.PostUpdate();
    }

    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.GrabRange(player, ref grabRange);
    }

    public override bool GrabStyle(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.GrabStyle(player))
                return false;
        }

        return false;
    }

    public override bool CanPickup(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanPickup(player))
                return false;
        }

        return true;
    }

    public override bool OnPickup(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.OnPickup(player))
                return false;
        }

        return true;
    }

    public override bool ItemSpace(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.ItemSpace(player))
                return false;
        }

        return false;
    }
    #endregion Lifetime

    #region Update
    public override void UpdateInventory(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UpdateInventory(player);
    }

    public override void UpdateInfoAccessory(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UpdateInfoAccessory(player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UpdateEquip(player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UpdateAccessory(player, hideVisual);
    }

    public override void UpdateVanity(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UpdateVanity(player);
    }

    public override void UpdateVisibleAccessory(Item item, Player player, bool hideVisual)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UpdateVisibleAccessory(player, hideVisual);
    }

    public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UpdateItemDye(player, dye, hideVisual);
    }
    #endregion Update

    #region Draw
    public override Color? GetAlpha(Item item, Color lightColor)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            Color? result = itemBehavior.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI))
                return false;
        }

        return true;
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
                return false;
        }

        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
    #endregion Draw

    #region Prefix
    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            int result = itemBehavior.ChoosePrefix(rand);
            if (result != -1)
                return result;
        }

        return -1;
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.PrefixChance(pre, rand);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool AllowPrefix(Item item, int pre)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.AllowPrefix(pre))
                return false;
        }

        return true;
    }

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.ReforgePrice(ref reforgePrice, ref canApplyDiscount))
                return false;
        }

        return true;
    }

    public override bool CanReforge(Item item)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanReforge())
                return false;
        }

        return true;
    }

    public override void PreReforge(Item item)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.PreReforge();
    }

    public override void PostReforge(Item item)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.PostReforge();
    }
    #endregion Prefix

    #region Use
    public override bool AltFunctionUse(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (itemBehavior.AltFunctionUse(player))
                return true;
        }

        return false;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanUseItem(player))
                return false;
        }

        return true;
    }

    public override bool? CanAutoReuseItem(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanAutoReuseItem(player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UseStyle(player, heldItemFrame);
    }

    public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.HoldStyle(player, heldItemFrame);
    }

    public override void HoldItem(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.HoldItem(player);
    }

    public override float UseTimeMultiplier(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseTimeMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override float UseAnimationMultiplier(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseAnimationMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override float UseSpeedMultiplier(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            float result = itemBehavior.UseSpeedMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.UseItem(player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void UseAnimation(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UseAnimation(player);
    }

    public override bool CanShoot(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanShoot(player))
                return false;
        }

        return true;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.Shoot(player, source, position, velocity, type, damage, knockback))
                return false;
        }

        return true;
    }

    public override bool CanRightClick(Item item)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanRightClick())
                return false;
        }

        return false;
    }

    public override void RightClick(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.RightClick(player);
    }
    #endregion Use

    #region ModifyStats
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponDamage(player, ref damage);
    }

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponKnockback(player, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyWeaponCrit(player, ref crit);
    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override void ModifyItemScale(Item item, Player player, ref float scale)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyItemScale(player, ref scale);
    }
    #endregion ModifyStats

    #region Hit
    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanHitNPC(player, target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyHitNPC(player, target, ref modifiers);
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnHitNPC(player, target, hit, damageDone);
    }

    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            bool result = itemBehavior.CanHitPvp(player, target);
            if (!result)
                return false;
        }

        return true;
    }

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyHitPvp(player, target, ref modifiers);
    }

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnHitPvp(player, target, hurtInfo);
    }
    #endregion Hit

    #region SpecialEffects
    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.GetHealLife(player, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.GetHealMana(player, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyManaCost(player, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, Player player, int neededMana)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnMissingMana(player, neededMana);
    }

    public override void OnConsumeMana(Item item, Player player, int manaConsumed)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnConsumeMana(player, manaConsumed);
    }

    public override bool CanResearch(Item item)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanResearch())
                return false;
        }

        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnResearched(fullyResearched);
    }

    public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.ModifyResearchSorting(ref itemGroup);
    }

    public override bool NeedsAmmo(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.NeedsAmmo(player))
                return false;
        }

        return true;
    }

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UseItemHitbox(player, ref hitbox, ref noHitbox);
    }

    public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.MeleeEffects(player, hitbox);
    }

    public override bool? CanCatchNPC(Item item, NPC target, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            bool? result = itemBehavior.CanCatchNPC(target, player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnCatchNPC(npc, player, failed);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.ConsumeItem(player))
                return false;
        }

        return true;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.OnConsumeItem(player);
    }

    public override void UseItemFrame(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.UseItemFrame(player);
    }

    public override void HoldItemFrame(Item item, Player player)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.HoldItemFrame(player);
    }

    public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
        ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
                ref maxAscentMultiplier, ref constantAscend);
    }

    public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.HorizontalWingSpeeds(player, ref speed, ref acceleration);
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.CanEquipAccessory(player, slot, modded))
                return false;
        }

        return true;
    }
    #endregion SpecialEffects

    #region Tooltip
    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawTooltip(lines, ref x, ref y))
                return false;
        }

        return true;
    }

    public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.PostDrawTooltip(lines);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
        {
            if (!itemBehavior.PreDrawTooltipLine(line, ref yOffset))
                return false;
        }

        return true;
    }

    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.PostDrawTooltipLine(line);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
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

    #region Net
    public override void NetSend(Item item, BinaryWriter writer)
    {
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
    }
    #endregion Net

    #region Data
    public override void SaveData(Item item, TagCompound tag)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.SaveData(tag);
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior))
            itemBehavior.LoadData(tag);
    }
    #endregion Data

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
