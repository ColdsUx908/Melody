using CalamityAnomalies;
using CalamityAnomalies.Items.ItemRarities;

namespace CalamityAnomalies.GlobalInstances;

public class CAGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (CAItemOverride itemOverride in CAOverrideHelper.ItemOverrides.Values)
            itemOverride.SetStaticDefaults();
    }

    public override void SetDefaults(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.SetDefaults();
    }

    public override void AddRecipes()
    {
        foreach (CAItemOverride itemOverride in CAOverrideHelper.ItemOverrides.Values)
            itemOverride.AddRecipes();
    }
    #endregion

    #region Active
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnCreated(context);
    }

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnSpawn(source);
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.Update(ref gravity, ref maxFallSpeed);
    }

    public override void PostUpdate(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostUpdate();
    }

    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.GrabRange(player, ref grabRange);
    }

    public override bool GrabStyle(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.GrabStyle(player))
                return false;
        }

        return false;
    }

    public override bool CanPickup(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanPickup(player))
                return false;
        }

        return true;
    }

    public override bool OnPickup(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.OnPickup(player))
                return false;
        }

        return true;
    }

    public override bool ItemSpace(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.ItemSpace(player))
                return false;
        }

        return false;
    }
    #endregion

    #region Update
    public override void UpdateInventory(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateInventory(player);
    }

    public override void UpdateInfoAccessory(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateInfoAccessory(player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateEquip(player);
    }

    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateAccessory(player, hideVisual);
    }

    public override void UpdateVanity(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateVanity(player);
    }

    public override void UpdateVisibleAccessory(Item item, Player player, bool hideVisual)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateVisibleAccessory(player, hideVisual);
    }

    public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UpdateItemDye(player, dye, hideVisual);
    }
    #endregion

    #region Draw
    public override Color? GetAlpha(Item item, Color lightColor)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            Color? result = itemOverride.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI))
                return false;
        }

        return true;
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
                return false;
        }

        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
    #endregion

    #region Prefix
    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            int result = itemOverride.ChoosePrefix(rand);
            if (result != -1)
                return result;
        }

        return -1;
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.PrefixChance(pre, rand);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool AllowPrefix(Item item, int pre)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.AllowPrefix(pre))
                return false;
        }

        return true;
    }

    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.ReforgePrice(ref reforgePrice, ref canApplyDiscount))
                return false;
        }

        return true;
    }

    public override bool CanReforge(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanReforge())
                return false;
        }

        return true;
    }

    public override void PreReforge(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PreReforge();
    }

    public override void PostReforge(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostReforge();
    }
    #endregion

    #region Use
    public override bool AltFunctionUse(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (itemOverride.AltFunctionUse(player))
                return true;
        }

        return false;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanUseItem(player))
                return false;
        }

        return true;
    }

    public override bool? CanAutoReuseItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.CanAutoReuseItem(player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UseStyle(player, heldItemFrame);
    }

    public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.HoldStyle(player, heldItemFrame);
    }

    public override void HoldItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.HoldItem(player);
    }

    public override float UseTimeMultiplier(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            float result = itemOverride.UseTimeMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override float UseAnimationMultiplier(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            float result = itemOverride.UseAnimationMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override float UseSpeedMultiplier(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            float result = itemOverride.UseSpeedMultiplier(player);
            if (result != 1f)
                return result;
        }

        return 1f;
    }

    public override bool? UseItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.UseItem(player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void UseAnimation(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UseAnimation(player);
    }

    public override bool CanShoot(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanShoot(player))
                return false;
        }

        return true;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.Shoot(player, source, position, velocity, type, damage, knockback))
                return false;
        }

        return true;
    }

    public override bool CanRightClick(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanRightClick())
                return false;
        }

        return false;
    }

    public override void RightClick(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.RightClick(player);
    }
    #endregion

    #region ModifyStats
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyWeaponDamage(player, ref damage);
    }

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyWeaponKnockback(player, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyWeaponCrit(player, ref crit);
    }

    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override void ModifyItemScale(Item item, Player player, ref float scale)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyItemScale(player, ref scale);
    }
    #endregion

    #region Hit
    public override bool? CanHitNPC(Item item, Player player, NPC target)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.CanHitNPC(player, target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.ModifyHitNPC(player, target, ref modifiers);
        }
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.OnHitNPC(player, target, hit, damageDone);
        }
    }

    public override bool CanHitPvp(Item item, Player player, Player target)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool result = itemOverride.CanHitPvp(player, target);
            if (!result)
                return false;
        }

        return true;
    }

    public override void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.ModifyHitPvp(player, target, ref modifiers);
        }
    }

    public override void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.OnHitPvp(player, target, hurtInfo);
        }
    }
    #endregion

    #region SpecialEffects
    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.GetHealLife(player, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.GetHealMana(player, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyManaCost(player, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, Player player, int neededMana)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnMissingMana(player, neededMana);
    }

    public override void OnConsumeMana(Item item, Player player, int manaConsumed)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnConsumeMana(player, manaConsumed);
    }

    public override bool CanResearch(Item item)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanResearch())
                return false;
        }
        return true;
    }

    public override void OnResearched(Item item, bool fullyResearched)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnResearched(fullyResearched);
    }

    public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.ModifyResearchSorting(ref itemGroup);
    }

    public override bool NeedsAmmo(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.NeedsAmmo(player))
                return false;
        }

        return true;
    }

    public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UseItemHitbox(player, ref hitbox, ref noHitbox);
    }

    public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.MeleeEffects(player, hitbox);
    }

    public override bool? CanCatchNPC(Item item, NPC target, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            bool? result = itemOverride.CanCatchNPC(target, player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnCatchNPC(npc, player, failed);
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.ConsumeItem(player))
                return false;
        }

        return true;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.OnConsumeItem(player);
    }

    public override void UseItemFrame(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.UseItemFrame(player);
    }

    public override void HoldItemFrame(Item item, Player player)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.HoldItemFrame(player);
    }

    public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
        ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
                ref maxAscentMultiplier, ref constantAscend);
    }

    public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.HorizontalWingSpeeds(player, ref speed, ref acceleration);
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.CanEquipAccessory(player, slot, modded))
                return false;
        }

        return true;
    }
    #endregion

    #region Tooltip
    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.PreDrawTooltip(lines, ref x, ref y))
                return false;
        }

        return true;
    }

    public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostDrawTooltip(lines);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.PreDrawTooltipLine(line, ref yOffset))
                return false;
        }

        return true;
    }

    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostDrawTooltipLine(line);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            itemOverride.ModifyTooltips(tooltips);

            tooltips.Add(new(CalamityAnomalies.Instance, "OverrideIdentifier", Language.GetTextValue(CAMain.ModLocalizationPrefix + "Tooltips.OverrideIdentifier")));
        }

        TooltipLine nameLine = tooltips.AsValueEnumerable().FirstOrDefault(k => k.Name == "ItemName" && k.Mod == "Terraria");
        if (nameLine is not null && item.rare == ModContent.RarityType<Celestial>())
        {
            List<Color> colorSet =
            [
                new(188, 192, 193), // white
                new(157, 100, 183), // purple
                new(249, 166, 77), // honey-ish orange
                new(255, 105, 234), // pink
                new(67, 204, 219), // sky blue
                new(249, 245, 99), // bright yellow
                new(236, 168, 247), // purplish pink
            ];
            if (nameLine is not null)
            {
                int colorIndex = (int)(Main.GlobalTimeWrappedHourly / 2 % colorSet.Count);
                Color currentColor = colorSet[colorIndex];
                Color nextColor = colorSet[(colorIndex + 1) % colorSet.Count];
                nameLine.OverrideColor = Color.Lerp(currentColor, nextColor, Main.GlobalTimeWrappedHourly % 2f > 1f ? 1f : Main.GlobalTimeWrappedHourly % 1f);
            }
        }
    }
    #endregion

    #region Net
    public override void NetSend(Item item, BinaryWriter writer)
    {
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
    }
    #endregion

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
    #endregion

    #region Data
    public override void SaveData(Item item, TagCompound tag) { }

    public override void LoadData(Item item, TagCompound tag) { }
    #endregion
}
