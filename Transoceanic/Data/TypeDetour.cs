﻿using MonoMod.RuntimeDetour;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.GameInput;
using Terraria.Map;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Transoceanic.Data;

public abstract class TypeDetour : ITODetourProvider
{
    /// <summary>
    /// <inheritdoc/><para/>
    /// 实现任何与自带方法同名的方法时，<strong>必须</strong>在方法调用中指定泛型参数或通过反射实现。
    /// </summary>
    public virtual void ApplyDetour() { }
}

public abstract class TypeDetour<T> : TypeDetour
{
    public static Type TargetType { get; } = typeof(T);

    public virtual bool AutoApplyStaticDetours => true;

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        if (AutoApplyStaticDetours)
        {
            foreach (MethodInfo detour in GetType().GetRealMethods(TOReflectionUtils.StaticBindingFlags))
                TODetourUtils.ApplyStaticMethodDetour(detour, TargetType);
        }
    }

    /// <summary>
    /// 尝试将指定的Detour应用到 <see cref="T"/> 类型的公共实例方法上。
    /// </summary>
    /// <remarks>这个方法会检查当前类型中是否存在指定的方法，仅在存在时则应用Detour逻辑。</remarks>
    /// <typeparam name="TDelegate">委托类型。</typeparam>
    /// <param name="detour">Detour逻辑的委托。该委托必须是一个与目标方法的签名匹配的具名方法，且方法名必须是 <c>{methodNamePrefix}{methodName}</c> 的形式。</param>
    /// <param name="methodNamePrefix"><c>methodName</c> 应用的方法名前缀。</param>
    protected Hook TryApplyDetour<TDelegate>(TDelegate detour, string methodNamePrefix = "Detour_") where TDelegate : Delegate
    {
        string methodName = detour.Method.Name;
        Match match = Regex.Match(methodName, methodNamePrefix + @"(?<methodName>.*)$");
        if (match.Success && GetType().HasRealMethod(methodName, BindingFlags.Public | BindingFlags.Instance))
            return TODetourUtils.Modify(TargetType.GetMethod(match.Groups["methodName"].Value, BindingFlags.Public | BindingFlags.Instance), detour);
        return null;
    }
}

public abstract class ModTypeDetour<T> : TypeDetour<T>
{
    // Load
    public delegate void Orig_Load(T self);
    public virtual void Detour_Load(Orig_Load orig, T self) => orig(self);

    // IsLoadingEnabled
    public delegate bool Orig_IsLoadingEnabled(T self, Mod mod);
    public virtual bool Detour_IsLoadingEnabled(Orig_IsLoadingEnabled orig, T self, Mod mod) => orig(self, mod);

    // SetupContent
    public delegate void Orig_SetupContent(T self);
    public virtual void Detour_SetupContent(Orig_SetupContent orig, T self) => orig(self);

    // SetStaticDefaults
    public delegate void Orig_SetStaticDefaults(T self);
    public virtual void Detour_SetStaticDefaults(Orig_SetStaticDefaults orig, T self) => orig(self);

    // Unload
    public delegate void Orig_Unload(T self);
    public virtual void Detour_Unload(Orig_Unload orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Load);
        TryApplyDetour(Detour_IsLoadingEnabled);
        TryApplyDetour(Detour_SetupContent);
        TryApplyDetour(Detour_SetStaticDefaults);
        TryApplyDetour(Detour_Unload);
    }
}

public abstract class ModAccessorySlotDetour<T> : ModTypeDetour<T> where T : ModAccessorySlot
{
    // CustomLocation
    public delegate Vector2? Orig_CustomLocation(T self);
    public virtual Vector2? Detour_CustomLocation(Orig_CustomLocation orig, T self) => orig(self);

    // DyeBackgroundTexture
    public delegate string Orig_DyeBackgroundTexture(T self);
    public virtual string Detour_DyeBackgroundTexture(Orig_DyeBackgroundTexture orig, T self) => orig(self);

    // VanityBackgroundTexture
    public delegate string Orig_VanityBackgroundTexture(T self);
    public virtual string Detour_VanityBackgroundTexture(Orig_VanityBackgroundTexture orig, T self) => orig(self);

    // FunctionalBackgroundTexture
    public delegate string Orig_FunctionalBackgroundTexture(T self);
    public virtual string Detour_FunctionalBackgroundTexture(Orig_FunctionalBackgroundTexture orig, T self) => orig(self);

    // DyeTexture
    public delegate string Orig_DyeTexture(T self);
    public virtual string Detour_DyeTexture(Orig_DyeTexture orig, T self) => orig(self);

    // VanityTexture
    public delegate string Orig_VanityTexture(T self);
    public virtual string Detour_VanityTexture(Orig_VanityTexture orig, T self) => orig(self);

    // FunctionalTexture
    public delegate string Orig_FunctionalTexture(T self);
    public virtual string Detour_FunctionalTexture(Orig_FunctionalTexture orig, T self) => orig(self);

    // DrawFunctionalSlot
    public delegate bool Orig_DrawFunctionalSlot(T self);
    public virtual bool Detour_DrawFunctionalSlot(Orig_DrawFunctionalSlot orig, T self) => orig(self);

    // DrawVanitySlot
    public delegate bool Orig_DrawVanitySlot(T self);
    public virtual bool Detour_DrawVanitySlot(Orig_DrawVanitySlot orig, T self) => orig(self);

    // DrawDyeSlot
    public delegate bool Orig_DrawDyeSlot(T self);
    public virtual bool Detour_DrawDyeSlot(Orig_DrawDyeSlot orig, T self) => orig(self);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, AccessorySlotType context, Item item, Vector2 position, bool isHovered);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, AccessorySlotType context, Item item, Vector2 position, bool isHovered) => orig(self, context, item, position, isHovered);

    // PostDraw
    public delegate void Orig_PostDraw(T self, AccessorySlotType context, Item item, Vector2 position, bool isHovered);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, AccessorySlotType context, Item item, Vector2 position, bool isHovered) => orig(self, context, item, position, isHovered);

    // ApplyEquipEffects
    public delegate void Orig_ApplyEquipEffects(T self);
    public virtual void Detour_ApplyEquipEffects(Orig_ApplyEquipEffects orig, T self) => orig(self);

    // CanAcceptItem
    public delegate bool Orig_CanAcceptItem(T self, Item checkItem, AccessorySlotType context);
    public virtual bool Detour_CanAcceptItem(Orig_CanAcceptItem orig, T self, Item checkItem, AccessorySlotType context) => orig(self, checkItem, context);

    // ModifyDefaultSwapSlot
    public delegate bool Orig_ModifyDefaultSwapSlot(T self, Item item, int accSlotToSwapTo);
    public virtual bool Detour_ModifyDefaultSwapSlot(Orig_ModifyDefaultSwapSlot orig, T self, Item item, int accSlotToSwapTo) => orig(self, item, accSlotToSwapTo);

    // IsHidden
    public delegate bool Orig_IsHidden(T self);
    public virtual bool Detour_IsHidden(Orig_IsHidden orig, T self) => orig(self);

    // IsEnabled
    public delegate bool Orig_IsEnabled(T self);
    public virtual bool Detour_IsEnabled(Orig_IsEnabled orig, T self) => orig(self);

    // IsVisibleWhenNotEnabled
    public delegate bool Orig_IsVisibleWhenNotEnabled(T self);
    public virtual bool Detour_IsVisibleWhenNotEnabled(Orig_IsVisibleWhenNotEnabled orig, T self) => orig(self);

    // OnMouseHover
    public delegate void Orig_OnMouseHover(T self, AccessorySlotType context);
    public virtual void Detour_OnMouseHover(Orig_OnMouseHover orig, T self, AccessorySlotType context) => orig(self, context);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_CustomLocation);
        TryApplyDetour(Detour_DyeBackgroundTexture);
        TryApplyDetour(Detour_VanityBackgroundTexture);
        TryApplyDetour(Detour_FunctionalBackgroundTexture);
        TryApplyDetour(Detour_DyeTexture);
        TryApplyDetour(Detour_VanityTexture);
        TryApplyDetour(Detour_FunctionalTexture);
        TryApplyDetour(Detour_DrawFunctionalSlot);
        TryApplyDetour(Detour_DrawVanitySlot);
        TryApplyDetour(Detour_DrawDyeSlot);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_ApplyEquipEffects);
        TryApplyDetour(Detour_CanAcceptItem);
        TryApplyDetour(Detour_ModifyDefaultSwapSlot);
        TryApplyDetour(Detour_IsHidden);
        TryApplyDetour(Detour_IsEnabled);
        TryApplyDetour(Detour_IsVisibleWhenNotEnabled);
        TryApplyDetour(Detour_OnMouseHover);
    }
}

public abstract class ModBannerTileDetour<T> : ModTileDetour<T> where T : ModBannerTile { }

public abstract class ModBiomeDetour<T> : ModSceneEffectDetour<T> where T : ModBiome
{
    // BiomeTorchItemType
    public delegate int Orig_BiomeTorchItemType(T self);
    public virtual int Detour_BiomeTorchItemType(Orig_BiomeTorchItemType orig, T self) => orig(self);

    // BiomeCampfireItemType
    public delegate int Orig_BiomeCampfireItemType(T self);
    public virtual int Detour_BiomeCampfireItemType(Orig_BiomeCampfireItemType orig, T self) => orig(self);

    // DisplayName
    public delegate LocalizedText Orig_DisplayName(T self);
    public virtual LocalizedText Detour_DisplayName(Orig_DisplayName orig, T self) => orig(self);

    // BestiaryIcon
    public delegate string Orig_BestiaryIcon(T self);
    public virtual string Detour_BestiaryIcon(Orig_BestiaryIcon orig, T self) => orig(self);

    // BackgroundPath
    public delegate string Orig_BackgroundPath(T self);
    public virtual string Detour_BackgroundPath(Orig_BackgroundPath orig, T self) => orig(self);

    // BackgroundColor
    public delegate Color? Orig_BackgroundColor(T self);
    public virtual Color? Detour_BackgroundColor(Orig_BackgroundColor orig, T self) => orig(self);

    // IsBiomeActive
    public delegate bool Orig_IsBiomeActive(T self, Player player);
    public virtual bool Detour_IsBiomeActive(Orig_IsBiomeActive orig, T self, Player player) => orig(self, player);

    // OnEnter
    public delegate void Orig_OnEnter(T self, Player player);
    public virtual void Detour_OnEnter(Orig_OnEnter orig, T self, Player player) => orig(self, player);

    // OnInBiome
    public delegate void Orig_OnInBiome(T self, Player player);
    public virtual void Detour_OnInBiome(Orig_OnInBiome orig, T self, Player player) => orig(self, player);

    // OnLeave
    public delegate void Orig_OnLeave(T self, Player player);
    public virtual void Detour_OnLeave(Orig_OnLeave orig, T self, Player player) => orig(self, player);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_BiomeTorchItemType);
        TryApplyDetour(Detour_BiomeCampfireItemType);
        TryApplyDetour(Detour_DisplayName);
        TryApplyDetour(Detour_BestiaryIcon);
        TryApplyDetour(Detour_BackgroundPath);
        TryApplyDetour(Detour_BackgroundColor);
        TryApplyDetour(Detour_IsBiomeActive);
        TryApplyDetour(Detour_OnEnter);
        TryApplyDetour(Detour_OnInBiome);
        TryApplyDetour(Detour_OnLeave);
    }
}

public abstract class ModBiomeConversionDetour<T> : ModTypeDetour<T> where T : ModBiomeConversion
{
    // PostSetupContent
    public delegate void Orig_PostSetupContent(T self);
    public virtual void Detour_PostSetupContent(Orig_PostSetupContent orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_PostSetupContent);
    }
}

public abstract class ModBlockTypeDetour<T> : ModTypeDetour<T> where T : ModBlockType
{
    // GetMapOption
    public delegate ushort Orig_GetMapOption(T self, int i, int j);
    public virtual ushort Detour_GetMapOption(Orig_GetMapOption orig, T self, int i, int j) => orig(self, i, j);

    // KillSound
    public delegate bool Orig_KillSound(T self, int i, int j, bool fail);
    public virtual bool Detour_KillSound(Orig_KillSound orig, T self, int i, int j, bool fail) => orig(self, i, j, fail);

    // NumDust
    public delegate void Orig_NumDust(T self, int i, int j, bool fail, ref int num);
    public virtual void Detour_NumDust(Orig_NumDust orig, T self, int i, int j, bool fail, ref int num) => orig(self, i, j, fail, ref num);

    // CreateDust
    public delegate bool Orig_CreateDust(T self, int i, int j, ref int type);
    public virtual bool Detour_CreateDust(Orig_CreateDust orig, T self, int i, int j, ref int type) => orig(self, i, j, ref type);

    // CanPlace
    public delegate bool Orig_CanPlace(T self, int i, int j);
    public virtual bool Detour_CanPlace(Orig_CanPlace orig, T self, int i, int j) => orig(self, i, j);

    // CanExplode
    public delegate bool Orig_CanExplode(T self, int i, int j);
    public virtual bool Detour_CanExplode(Orig_CanExplode orig, T self, int i, int j) => orig(self, i, j);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, int i, int j, SpriteBatch spriteBatch);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, int i, int j, SpriteBatch spriteBatch) => orig(self, i, j, spriteBatch);

    // PostDraw
    public delegate void Orig_PostDraw(T self, int i, int j, SpriteBatch spriteBatch);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, int i, int j, SpriteBatch spriteBatch) => orig(self, i, j, spriteBatch);

    // RandomUpdate
    public delegate void Orig_RandomUpdate(T self, int i, int j);
    public virtual void Detour_RandomUpdate(Orig_RandomUpdate orig, T self, int i, int j) => orig(self, i, j);

    // PlaceInWorld
    public delegate void Orig_PlaceInWorld(T self, int i, int j, Item item);
    public virtual void Detour_PlaceInWorld(Orig_PlaceInWorld orig, T self, int i, int j, Item item) => orig(self, i, j, item);

    // ModifyLight
    public delegate void Orig_ModifyLight(T self, int i, int j, ref float r, ref float g, ref float b);
    public virtual void Detour_ModifyLight(Orig_ModifyLight orig, T self, int i, int j, ref float r, ref float g, ref float b) => orig(self, i, j, ref r, ref g, ref b);

    // Convert
    public delegate void Orig_Convert(T self, int i, int j, int conversionType);
    public virtual void Detour_Convert(Orig_Convert orig, T self, int i, int j, int conversionType) => orig(self, i, j, conversionType);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_GetMapOption);
        TryApplyDetour(Detour_KillSound);
        TryApplyDetour(Detour_NumDust);
        TryApplyDetour(Detour_CreateDust);
        TryApplyDetour(Detour_CanPlace);
        TryApplyDetour(Detour_CanExplode);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_RandomUpdate);
        TryApplyDetour(Detour_PlaceInWorld);
        TryApplyDetour(Detour_ModifyLight);
        TryApplyDetour(Detour_Convert);
    }
}

public abstract class ModBossBarDetour<T> : ModTypeDetour<T> where T : ModBossBar
{
    // ModifyInfo
    public delegate bool? Orig_ModifyInfo(T self, ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax);
    public virtual bool? Detour_ModifyInfo(Orig_ModifyInfo orig, T self, ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) => orig(self, ref info, ref life, ref lifeMax, ref shield, ref shieldMax);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) => orig(self, spriteBatch, npc, ref drawParams);

    // PostDraw
    public delegate void Orig_PostDraw(T self, SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams) => orig(self, spriteBatch, npc, drawParams);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_ModifyInfo);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
    }
}

public abstract class ModBossBarStyleDetour<T> : ModTypeDetour<T> where T : ModBossBarStyle
{
    // Update
    public delegate void Orig_Update(T self, IBigProgressBar currentBar, ref BigProgressBarInfo info);
    public virtual void Detour_Update(Orig_Update orig, T self, IBigProgressBar currentBar, ref BigProgressBarInfo info) => orig(self, currentBar, ref info);

    // OnSelected
    public delegate void Orig_OnSelected(T self);
    public virtual void Detour_OnSelected(Orig_OnSelected orig, T self) => orig(self);

    // OnDeselected
    public delegate void Orig_OnDeselected(T self);
    public virtual void Detour_OnDeselected(Orig_OnDeselected orig, T self) => orig(self);

    // Draw
    public delegate void Orig_Draw(T self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info);
    public virtual void Detour_Draw(Orig_Draw orig, T self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info) => orig(self, spriteBatch, currentBar, info);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Update);
        TryApplyDetour(Detour_OnSelected);
        TryApplyDetour(Detour_OnDeselected);
        TryApplyDetour(Detour_Draw);
    }
}

public abstract class ModBuffDetour<T> : ModTypeDetour<T> where T : ModBuff
{
    // Update (Player)
    public delegate void Orig_UpdatePlayer(T self, Player player, ref int buffIndex);
    public virtual void Detour_UpdatePlayer(Orig_UpdatePlayer orig, T self, Player player, ref int buffIndex) => orig(self, player, ref buffIndex);

    // Update (NPC)
    public delegate void Orig_UpdateNPC(T self, NPC npc, ref int buffIndex);
    public virtual void Detour_UpdateNPC(Orig_UpdateNPC orig, T self, NPC npc, ref int buffIndex) => orig(self, npc, ref buffIndex);

    // ReApply (Player)
    public delegate bool Orig_ReApplyPlayer(T self, Player player, int time, int buffIndex);
    public virtual bool Detour_ReApplyPlayer(Orig_ReApplyPlayer orig, T self, Player player, int time, int buffIndex) => orig(self, player, time, buffIndex);

    // ReApply (NPC)
    public delegate bool Orig_ReApplyNPC(T self, NPC npc, int time, int buffIndex);
    public virtual bool Detour_ReApplyNPC(Orig_ReApplyNPC orig, T self, NPC npc, int time, int buffIndex) => orig(self, npc, time, buffIndex);

    // ModifyBuffText
    public delegate void Orig_ModifyBuffText(T self, ref string buffName, ref string tip, ref int rare);
    public virtual void Detour_ModifyBuffText(Orig_ModifyBuffText orig, T self, ref string buffName, ref string tip, ref int rare) => orig(self, ref buffName, ref tip, ref rare);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) => orig(self, spriteBatch, buffIndex, ref drawParams);

    // PostDraw
    public delegate void Orig_PostDraw(T self, SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) => orig(self, spriteBatch, buffIndex, drawParams);

    // RightClick
    public delegate bool Orig_RightClick(T self, int buffIndex);
    public virtual bool Detour_RightClick(Orig_RightClick orig, T self, int buffIndex) => orig(self, buffIndex);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_UpdatePlayer);
        TryApplyDetour(Detour_UpdateNPC);
        TryApplyDetour(Detour_ReApplyPlayer);
        TryApplyDetour(Detour_ReApplyNPC);
        TryApplyDetour(Detour_ModifyBuffText);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_RightClick);
    }
}

public abstract class ModCactusDetour<T> : TypeDetour<T> where T : ModCactus
{
    // SetStaticDefaults
    public delegate void Orig_SetStaticDefaults(T self);
    public virtual void Detour_SetStaticDefaults(Orig_SetStaticDefaults orig, T self) => orig(self);

    // GetTexture
    public delegate Asset<Texture2D> Orig_GetTexture(T self);
    public virtual Asset<Texture2D> Detour_GetTexture(Orig_GetTexture orig, T self) => orig(self);

    // GetFruitTexture
    public delegate Asset<Texture2D> Orig_GetFruitTexture(T self);
    public virtual Asset<Texture2D> Detour_GetFruitTexture(Orig_GetFruitTexture orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_SetStaticDefaults);
        TryApplyDetour(Detour_GetTexture);
        TryApplyDetour(Detour_GetFruitTexture);
    }
}

public abstract class ModCloudDetour<T> : ModTypeDetour<T> where T : ModCloud
{
    // SpawnChance
    public delegate float Orig_SpawnChance(T self);
    public virtual float Detour_SpawnChance(Orig_SpawnChance orig, T self) => orig(self);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self, Cloud cloud);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, Cloud cloud) => orig(self, cloud);

    // Draw
    public delegate bool Orig_Draw(T self, SpriteBatch spriteBatch, Cloud cloud, int cloudIndex, ref DrawData drawData);
    public virtual bool Detour_Draw(Orig_Draw orig, T self, SpriteBatch spriteBatch, Cloud cloud, int cloudIndex, ref DrawData drawData) => orig(self, spriteBatch, cloud, cloudIndex, ref drawData);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_SpawnChance);
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_Draw);
    }
}

public abstract class ModCommandDetour<T> : ModTypeDetour<T> where T : ModCommand
{
    // Action
    public delegate void Orig_Action(T self, CommandCaller caller, string input, string[] args);
    public virtual void Detour_Action(Orig_Action orig, T self, CommandCaller caller, string input, string[] args) => orig(self, caller, input, args);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Action);
    }
}

public abstract class ModDustDetour<T> : ModTypeDetour<T> where T : ModDust
{
    // PreDraw
    public delegate bool Orig_PreDraw(T self, Dust dust);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, Dust dust) => orig(self, dust);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self, Dust dust);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, Dust dust) => orig(self, dust);

    // Update
    public delegate bool Orig_Update(T self, Dust dust);
    public virtual bool Detour_Update(Orig_Update orig, T self, Dust dust) => orig(self, dust);

    // MidUpdate
    public delegate bool Orig_MidUpdate(T self, Dust dust);
    public virtual bool Detour_MidUpdate(Orig_MidUpdate orig, T self, Dust dust) => orig(self, dust);

    // GetAlpha
    public delegate Color? Orig_GetAlpha(T self, Dust dust, Color lightColor);
    public virtual Color? Detour_GetAlpha(Orig_GetAlpha orig, T self, Dust dust, Color lightColor) => orig(self, dust, lightColor);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_Update);
        TryApplyDetour(Detour_MidUpdate);
        TryApplyDetour(Detour_GetAlpha);
    }
}

public abstract class ModEmoteBubbleDetour<T> : ModTypeDetour<T> where T : ModEmoteBubble
{
    // IsUnlocked
    public delegate bool Orig_IsUnlocked(T self);
    public virtual bool Detour_IsUnlocked(Orig_IsUnlocked orig, T self) => orig(self);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self) => orig(self);

    // UpdateFrame
    public delegate bool Orig_UpdateFrame(T self);
    public virtual bool Detour_UpdateFrame(Orig_UpdateFrame orig, T self) => orig(self);

    // UpdateFrameInEmoteMenu
    public delegate bool Orig_UpdateFrameInEmoteMenu(T self, ref int frameCounter);
    public virtual bool Detour_UpdateFrameInEmoteMenu(Orig_UpdateFrameInEmoteMenu orig, T self, ref int frameCounter) => orig(self, ref frameCounter);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) => orig(self, spriteBatch, texture, position, frame, origin, spriteEffects);

    // PostDraw
    public delegate void Orig_PostDraw(T self, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) => orig(self, spriteBatch, texture, position, frame, origin, spriteEffects);

    // PreDrawInEmoteMenu
    public delegate bool Orig_PreDrawInEmoteMenu(T self, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin);
    public virtual bool Detour_PreDrawInEmoteMenu(Orig_PreDrawInEmoteMenu orig, T self, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) => orig(self, spriteBatch, uiEmoteButton, position, frame, origin);

    // PostDrawInEmoteMenu
    public delegate void Orig_PostDrawInEmoteMenu(T self, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin);
    public virtual void Detour_PostDrawInEmoteMenu(Orig_PostDrawInEmoteMenu orig, T self, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) => orig(self, spriteBatch, uiEmoteButton, position, frame, origin);

    // GetFrame
    public delegate Rectangle? Orig_GetFrame(T self);
    public virtual Rectangle? Detour_GetFrame(Orig_GetFrame orig, T self) => orig(self);

    // GetFrameInEmoteMenu
    public delegate Rectangle? Orig_GetFrameInEmoteMenu(T self, int frame, int frameCounter);
    public virtual Rectangle? Detour_GetFrameInEmoteMenu(Orig_GetFrameInEmoteMenu orig, T self, int frame, int frameCounter) => orig(self, frame, frameCounter);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_IsUnlocked);
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_UpdateFrame);
        TryApplyDetour(Detour_UpdateFrameInEmoteMenu);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_PreDrawInEmoteMenu);
        TryApplyDetour(Detour_PostDrawInEmoteMenu);
        TryApplyDetour(Detour_GetFrame);
        TryApplyDetour(Detour_GetFrameInEmoteMenu);
    }
}

public abstract class ModGoreDetour<T> : ModTypeDetour<T> where T : ModGore
{
    // OnSpawn
    public delegate void Orig_OnSpawn(T self, Gore gore, IEntitySource source);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, Gore gore, IEntitySource source) => orig(self, gore, source);

    // Update
    public delegate bool Orig_Update(T self, Gore gore);
    public virtual bool Detour_Update(Orig_Update orig, T self, Gore gore) => orig(self, gore);

    // GetAlpha
    public delegate Color? Orig_GetAlpha(T self, Gore gore, Color lightColor);
    public virtual Color? Detour_GetAlpha(Orig_GetAlpha orig, T self, Gore gore, Color lightColor) => orig(self, gore, lightColor);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_Update);
        TryApplyDetour(Detour_GetAlpha);
    }
}

public abstract class ModHairDetour<T> : ModTypeDetour<T> where T : ModHair
{
    // AutoStaticDefaults
    public delegate void Orig_AutoStaticDefaults(T self);
    public virtual void Detour_AutoStaticDefaults(Orig_AutoStaticDefaults orig, T self) => orig(self);

    // GetUnlockConditions
    public delegate IEnumerable<Condition> Orig_GetUnlockConditions(T self);
    public virtual IEnumerable<Condition> Detour_GetUnlockConditions(Orig_GetUnlockConditions orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_AutoStaticDefaults);
        TryApplyDetour(Detour_GetUnlockConditions);
    }
}

public abstract class ModItemDetour<T> : ModTypeDetour<T> where T : ModItem
{
    // SetDefaults
    public delegate void Orig_SetDefaults(T self);
    public virtual void Detour_SetDefaults(Orig_SetDefaults orig, T self) => orig(self);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self, IEntitySource source);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, IEntitySource source) => orig(self, source);

    // OnCreated
    public delegate void Orig_OnCreated(T self, ItemCreationContext context);
    public virtual void Detour_OnCreated(Orig_OnCreated orig, T self, ItemCreationContext context) => orig(self, context);

    // AutoDefaults
    public delegate void Orig_AutoDefaults(T self);
    public virtual void Detour_AutoDefaults(Orig_AutoDefaults orig, T self) => orig(self);

    // AutoStaticDefaults
    public delegate void Orig_AutoStaticDefaults(T self);
    public virtual void Detour_AutoStaticDefaults(Orig_AutoStaticDefaults orig, T self) => orig(self);

    // ChoosePrefix
    public delegate int Orig_ChoosePrefix(T self, UnifiedRandom rand);
    public virtual int Detour_ChoosePrefix(Orig_ChoosePrefix orig, T self, UnifiedRandom rand) => orig(self, rand);

    // MeleePrefix
    public delegate bool Orig_MeleePrefix(T self);
    public virtual bool Detour_MeleePrefix(Orig_MeleePrefix orig, T self) => orig(self);

    // WeaponPrefix
    public delegate bool Orig_WeaponPrefix(T self);
    public virtual bool Detour_WeaponPrefix(Orig_WeaponPrefix orig, T self) => orig(self);

    // RangedPrefix
    public delegate bool Orig_RangedPrefix(T self);
    public virtual bool Detour_RangedPrefix(Orig_RangedPrefix orig, T self) => orig(self);

    // MagicPrefix
    public delegate bool Orig_MagicPrefix(T self);
    public virtual bool Detour_MagicPrefix(Orig_MagicPrefix orig, T self) => orig(self);

    // PrefixChance
    public delegate bool? Orig_PrefixChance(T self, int pre, UnifiedRandom rand);
    public virtual bool? Detour_PrefixChance(Orig_PrefixChance orig, T self, int pre, UnifiedRandom rand) => orig(self, pre, rand);

    // AllowPrefix
    public delegate bool Orig_AllowPrefix(T self, int pre);
    public virtual bool Detour_AllowPrefix(Orig_AllowPrefix orig, T self, int pre) => orig(self, pre);

    // CanUseItem
    public delegate bool Orig_CanUseItem(T self, Player player);
    public virtual bool Detour_CanUseItem(Orig_CanUseItem orig, T self, Player player) => orig(self, player);

    // CanAutoReuseItem
    public delegate bool? Orig_CanAutoReuseItem(T self, Player player);
    public virtual bool? Detour_CanAutoReuseItem(Orig_CanAutoReuseItem orig, T self, Player player) => orig(self, player);

    // UseStyle
    public delegate void Orig_UseStyle(T self, Player player, Rectangle heldItemFrame);
    public virtual void Detour_UseStyle(Orig_UseStyle orig, T self, Player player, Rectangle heldItemFrame) => orig(self, player, heldItemFrame);

    // HoldStyle
    public delegate void Orig_HoldStyle(T self, Player player, Rectangle heldItemFrame);
    public virtual void Detour_HoldStyle(Orig_HoldStyle orig, T self, Player player, Rectangle heldItemFrame) => orig(self, player, heldItemFrame);

    // HoldItem
    public delegate void Orig_HoldItem(T self, Player player);
    public virtual void Detour_HoldItem(Orig_HoldItem orig, T self, Player player) => orig(self, player);

    // UseTimeMultiplier
    public delegate float Orig_UseTimeMultiplier(T self, Player player);
    public virtual float Detour_UseTimeMultiplier(Orig_UseTimeMultiplier orig, T self, Player player) => orig(self, player);

    // UseAnimationMultiplier
    public delegate float Orig_UseAnimationMultiplier(T self, Player player);
    public virtual float Detour_UseAnimationMultiplier(Orig_UseAnimationMultiplier orig, T self, Player player) => orig(self, player);

    // UseSpeedMultiplier
    public delegate float Orig_UseSpeedMultiplier(T self, Player player);
    public virtual float Detour_UseSpeedMultiplier(Orig_UseSpeedMultiplier orig, T self, Player player) => orig(self, player);

    // GetHealLife
    public delegate void Orig_GetHealLife(T self, Player player, bool quickHeal, ref int healValue);
    public virtual void Detour_GetHealLife(Orig_GetHealLife orig, T self, Player player, bool quickHeal, ref int healValue) => orig(self, player, quickHeal, ref healValue);

    // GetHealMana
    public delegate void Orig_GetHealMana(T self, Player player, bool quickHeal, ref int healValue);
    public virtual void Detour_GetHealMana(Orig_GetHealMana orig, T self, Player player, bool quickHeal, ref int healValue) => orig(self, player, quickHeal, ref healValue);

    // ModifyManaCost
    public delegate void Orig_ModifyManaCost(T self, Player player, ref float reduce, ref float mult);
    public virtual void Detour_ModifyManaCost(Orig_ModifyManaCost orig, T self, Player player, ref float reduce, ref float mult) => orig(self, player, ref reduce, ref mult);

    // OnMissingMana
    public delegate void Orig_OnMissingMana(T self, Player player, int neededMana);
    public virtual void Detour_OnMissingMana(Orig_OnMissingMana orig, T self, Player player, int neededMana) => orig(self, player, neededMana);

    // OnConsumeMana
    public delegate void Orig_OnConsumeMana(T self, Player player, int manaConsumed);
    public virtual void Detour_OnConsumeMana(Orig_OnConsumeMana orig, T self, Player player, int manaConsumed) => orig(self, player, manaConsumed);

    // ModifyWeaponDamage
    public delegate void Orig_ModifyWeaponDamage(T self, Player player, ref StatModifier damage);
    public virtual void Detour_ModifyWeaponDamage(Orig_ModifyWeaponDamage orig, T self, Player player, ref StatModifier damage) => orig(self, player, ref damage);

    // ModifyResearchSorting
    public delegate void Orig_ModifyResearchSorting(T self, ref ContentSamples.CreativeHelper.ItemGroup itemGroup);
    public virtual void Detour_ModifyResearchSorting(Orig_ModifyResearchSorting orig, T self, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => orig(self, ref itemGroup);

    // CanConsumeBait
    public delegate bool? Orig_CanConsumeBait(T self, Player player);
    public virtual bool? Detour_CanConsumeBait(Orig_CanConsumeBait orig, T self, Player player) => orig(self, player);

    // CanResearch
    public delegate bool Orig_CanResearch(T self);
    public virtual bool Detour_CanResearch(Orig_CanResearch orig, T self) => orig(self);

    // OnResearched
    public delegate void Orig_OnResearched(T self, bool fullyResearched);
    public virtual void Detour_OnResearched(Orig_OnResearched orig, T self, bool fullyResearched) => orig(self, fullyResearched);

    // ModifyWeaponKnockback
    public delegate void Orig_ModifyWeaponKnockback(T self, Player player, ref StatModifier knockback);
    public virtual void Detour_ModifyWeaponKnockback(Orig_ModifyWeaponKnockback orig, T self, Player player, ref StatModifier knockback) => orig(self, player, ref knockback);

    // ModifyWeaponCrit
    public delegate void Orig_ModifyWeaponCrit(T self, Player player, ref float crit);
    public virtual void Detour_ModifyWeaponCrit(Orig_ModifyWeaponCrit orig, T self, Player player, ref float crit) => orig(self, player, ref crit);

    // NeedsAmmo
    public delegate bool Orig_NeedsAmmo(T self, Player player);
    public virtual bool Detour_NeedsAmmo(Orig_NeedsAmmo orig, T self, Player player) => orig(self, player);

    // PickAmmo
    public delegate void Orig_PickAmmo(T self, Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback);
    public virtual void Detour_PickAmmo(Orig_PickAmmo orig, T self, Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) => orig(self, weapon, player, ref type, ref speed, ref damage, ref knockback);

    // CanChooseAmmo
    public delegate bool? Orig_CanChooseAmmo(T self, Item ammo, Player player);
    public virtual bool? Detour_CanChooseAmmo(Orig_CanChooseAmmo orig, T self, Item ammo, Player player) => orig(self, ammo, player);

    // CanBeChosenAsAmmo
    public delegate bool? Orig_CanBeChosenAsAmmo(T self, Item weapon, Player player);
    public virtual bool? Detour_CanBeChosenAsAmmo(Orig_CanBeChosenAsAmmo orig, T self, Item weapon, Player player) => orig(self, weapon, player);

    // CanConsumeAmmo
    public delegate bool Orig_CanConsumeAmmo(T self, Item ammo, Player player);
    public virtual bool Detour_CanConsumeAmmo(Orig_CanConsumeAmmo orig, T self, Item ammo, Player player) => orig(self, ammo, player);

    // CanBeConsumedAsAmmo
    public delegate bool Orig_CanBeConsumedAsAmmo(T self, Item weapon, Player player);
    public virtual bool Detour_CanBeConsumedAsAmmo(Orig_CanBeConsumedAsAmmo orig, T self, Item weapon, Player player) => orig(self, weapon, player);

    // OnConsumeAmmo
    public delegate void Orig_OnConsumeAmmo(T self, Item ammo, Player player);
    public virtual void Detour_OnConsumeAmmo(Orig_OnConsumeAmmo orig, T self, Item ammo, Player player) => orig(self, ammo, player);

    // OnConsumedAsAmmo
    public delegate void Orig_OnConsumedAsAmmo(T self, Item weapon, Player player);
    public virtual void Detour_OnConsumedAsAmmo(Orig_OnConsumedAsAmmo orig, T self, Item weapon, Player player) => orig(self, weapon, player);

    // CanShoot
    public delegate bool Orig_CanShoot(T self, Player player);
    public virtual bool Detour_CanShoot(Orig_CanShoot orig, T self, Player player) => orig(self, player);

    // ModifyShootStats
    public delegate void Orig_ModifyShootStats(T self, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);
    public virtual void Detour_ModifyShootStats(Orig_ModifyShootStats orig, T self, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => orig(self, player, ref position, ref velocity, ref type, ref damage, ref knockback);

    // Shoot
    public delegate bool Orig_Shoot(T self, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback);
    public virtual bool Detour_Shoot(Orig_Shoot orig, T self, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => orig(self, player, source, position, velocity, type, damage, knockback);

    // UseItemHitbox
    public delegate void Orig_UseItemHitbox(T self, Player player, ref Rectangle hitbox, ref bool noHitbox);
    public virtual void Detour_UseItemHitbox(Orig_UseItemHitbox orig, T self, Player player, ref Rectangle hitbox, ref bool noHitbox) => orig(self, player, ref hitbox, ref noHitbox);

    // MeleeEffects
    public delegate void Orig_MeleeEffects(T self, Player player, Rectangle hitbox);
    public virtual void Detour_MeleeEffects(Orig_MeleeEffects orig, T self, Player player, Rectangle hitbox) => orig(self, player, hitbox);

    // CanCatchNPC
    public delegate bool? Orig_CanCatchNPC(T self, NPC target, Player player);
    public virtual bool? Detour_CanCatchNPC(Orig_CanCatchNPC orig, T self, NPC target, Player player) => orig(self, target, player);

    // OnCatchNPC
    public delegate void Orig_OnCatchNPC(T self, NPC npc, Player player, bool failed);
    public virtual void Detour_OnCatchNPC(Orig_OnCatchNPC orig, T self, NPC npc, Player player, bool failed) => orig(self, npc, player, failed);

    // ModifyItemScale
    public delegate void Orig_ModifyItemScale(T self, Player player, ref float scale);
    public virtual void Detour_ModifyItemScale(Orig_ModifyItemScale orig, T self, Player player, ref float scale) => orig(self, player, ref scale);

    // CanHitNPC
    public delegate bool? Orig_CanHitNPC(T self, Player player, NPC target);
    public virtual bool? Detour_CanHitNPC(Orig_CanHitNPC orig, T self, Player player, NPC target) => orig(self, player, target);

    // CanMeleeAttackCollideWithNPC
    public delegate bool? Orig_CanMeleeAttackCollideWithNPC(T self, Rectangle meleeAttackHitbox, Player player, NPC target);
    public virtual bool? Detour_CanMeleeAttackCollideWithNPC(Orig_CanMeleeAttackCollideWithNPC orig, T self, Rectangle meleeAttackHitbox, Player player, NPC target) => orig(self, meleeAttackHitbox, player, target);

    // ModifyHitNPC
    public delegate void Orig_ModifyHitNPC(T self, Player player, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, T self, Player player, NPC target, ref NPC.HitModifiers modifiers) => orig(self, player, target, ref modifiers);

    // OnHitNPC
    public delegate void Orig_OnHitNPC(T self, Player player, NPC target, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitNPC(Orig_OnHitNPC orig, T self, Player player, NPC target, NPC.HitInfo hit, int damageDone) => orig(self, player, target, hit, damageDone);

    // CanHitPvp
    public delegate bool Orig_CanHitPvp(T self, Player player, Player target);
    public virtual bool Detour_CanHitPvp(Orig_CanHitPvp orig, T self, Player player, Player target) => orig(self, player, target);

    // ModifyHitPvp
    public delegate void Orig_ModifyHitPvp(T self, Player player, Player target, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHitPvp(Orig_ModifyHitPvp orig, T self, Player player, Player target, ref Player.HurtModifiers modifiers) => orig(self, player, target, ref modifiers);

    // OnHitPvp
    public delegate void Orig_OnHitPvp(T self, Player player, Player target, Player.HurtInfo hurtInfo);
    public virtual void Detour_OnHitPvp(Orig_OnHitPvp orig, T self, Player player, Player target, Player.HurtInfo hurtInfo) => orig(self, player, target, hurtInfo);

    // UseItem
    public delegate bool? Orig_UseItem(T self, Player player);
    public virtual bool? Detour_UseItem(Orig_UseItem orig, T self, Player player) => orig(self, player);

    // UseAnimation
    public delegate void Orig_UseAnimation(T self, Player player);
    public virtual void Detour_UseAnimation(Orig_UseAnimation orig, T self, Player player) => orig(self, player);

    // ConsumeItem
    public delegate bool Orig_ConsumeItem(T self, Player player);
    public virtual bool Detour_ConsumeItem(Orig_ConsumeItem orig, T self, Player player) => orig(self, player);

    // OnConsumeItem
    public delegate void Orig_OnConsumeItem(T self, Player player);
    public virtual void Detour_OnConsumeItem(Orig_OnConsumeItem orig, T self, Player player) => orig(self, player);

    // UseItemFrame
    public delegate void Orig_UseItemFrame(T self, Player player);
    public virtual void Detour_UseItemFrame(Orig_UseItemFrame orig, T self, Player player) => orig(self, player);

    // HoldItemFrame
    public delegate void Orig_HoldItemFrame(T self, Player player);
    public virtual void Detour_HoldItemFrame(Orig_HoldItemFrame orig, T self, Player player) => orig(self, player);

    // AltFunctionUse
    public delegate bool Orig_AltFunctionUse(T self, Player player);
    public virtual bool Detour_AltFunctionUse(Orig_AltFunctionUse orig, T self, Player player) => orig(self, player);

    // UpdateInventory
    public delegate void Orig_UpdateInventory(T self, Player player);
    public virtual void Detour_UpdateInventory(Orig_UpdateInventory orig, T self, Player player) => orig(self, player);

    // UpdateInfoAccessory
    public delegate void Orig_UpdateInfoAccessory(T self, Player player);
    public virtual void Detour_UpdateInfoAccessory(Orig_UpdateInfoAccessory orig, T self, Player player) => orig(self, player);

    // UpdateEquip
    public delegate void Orig_UpdateEquip(T self, Player player);
    public virtual void Detour_UpdateEquip(Orig_UpdateEquip orig, T self, Player player) => orig(self, player);

    // UpdateAccessory
    public delegate void Orig_UpdateAccessory(T self, Player player, bool hideVisual);
    public virtual void Detour_UpdateAccessory(Orig_UpdateAccessory orig, T self, Player player, bool hideVisual) => orig(self, player, hideVisual);

    // UpdateVanity
    public delegate void Orig_UpdateVanity(T self, Player player);
    public virtual void Detour_UpdateVanity(Orig_UpdateVanity orig, T self, Player player) => orig(self, player);

    // EquipFrameEffects
    public delegate void Orig_EquipFrameEffects(T self, Player player, EquipType type);
    public virtual void Detour_EquipFrameEffects(Orig_EquipFrameEffects orig, T self, Player player, EquipType type) => orig(self, player, type);

    // IsArmorSet
    public delegate bool Orig_IsArmorSet(T self, Item head, Item body, Item legs);
    public virtual bool Detour_IsArmorSet(Orig_IsArmorSet orig, T self, Item head, Item body, Item legs) => orig(self, head, body, legs);

    // UpdateArmorSet
    public delegate void Orig_UpdateArmorSet(T self, Player player);
    public virtual void Detour_UpdateArmorSet(Orig_UpdateArmorSet orig, T self, Player player) => orig(self, player);

    // IsVanitySet
    public delegate bool Orig_IsVanitySet(T self, int head, int body, int legs);
    public virtual bool Detour_IsVanitySet(Orig_IsVanitySet orig, T self, int head, int body, int legs) => orig(self, head, body, legs);

    // PreUpdateVanitySet
    public delegate void Orig_PreUpdateVanitySet(T self, Player player);
    public virtual void Detour_PreUpdateVanitySet(Orig_PreUpdateVanitySet orig, T self, Player player) => orig(self, player);

    // UpdateVanitySet
    public delegate void Orig_UpdateVanitySet(T self, Player player);
    public virtual void Detour_UpdateVanitySet(Orig_UpdateVanitySet orig, T self, Player player) => orig(self, player);

    // ArmorSetShadows
    public delegate void Orig_ArmorSetShadows(T self, Player player);
    public virtual void Detour_ArmorSetShadows(Orig_ArmorSetShadows orig, T self, Player player) => orig(self, player);

    // SetMatch
    public delegate void Orig_SetMatch(T self, bool male, ref int equipSlot, ref bool robes);
    public virtual void Detour_SetMatch(Orig_SetMatch orig, T self, bool male, ref int equipSlot, ref bool robes) => orig(self, male, ref equipSlot, ref robes);

    // CanRightClick
    public delegate bool Orig_CanRightClick(T self);
    public virtual bool Detour_CanRightClick(Orig_CanRightClick orig, T self) => orig(self);

    // RightClick
    public delegate void Orig_RightClick(T self, Player player);
    public virtual void Detour_RightClick(Orig_RightClick orig, T self, Player player) => orig(self, player);

    // ModifyItemLoot
    public delegate void Orig_ModifyItemLoot(T self, ItemLoot itemLoot);
    public virtual void Detour_ModifyItemLoot(Orig_ModifyItemLoot orig, T self, ItemLoot itemLoot) => orig(self, itemLoot);

    // CanStack
    public delegate bool Orig_CanStack(T self, Item source);
    public virtual bool Detour_CanStack(Orig_CanStack orig, T self, Item source) => orig(self, source);

    // CanStackInWorld
    public delegate bool Orig_CanStackInWorld(T self, Item source);
    public virtual bool Detour_CanStackInWorld(Orig_CanStackInWorld orig, T self, Item source) => orig(self, source);

    // OnStack
    public delegate void Orig_OnStack(T self, Item source, int numToTransfer);
    public virtual void Detour_OnStack(Orig_OnStack orig, T self, Item source, int numToTransfer) => orig(self, source, numToTransfer);

    // SplitStack
    public delegate void Orig_SplitStack(T self, Item source, int numToTransfer);
    public virtual void Detour_SplitStack(Orig_SplitStack orig, T self, Item source, int numToTransfer) => orig(self, source, numToTransfer);

    // ReforgePrice
    public delegate bool Orig_ReforgePrice(T self, ref int reforgePrice, ref bool canApplyDiscount);
    public virtual bool Detour_ReforgePrice(Orig_ReforgePrice orig, T self, ref int reforgePrice, ref bool canApplyDiscount) => orig(self, ref reforgePrice, ref canApplyDiscount);

    // CanReforge
    public delegate bool Orig_CanReforge(T self);
    public virtual bool Detour_CanReforge(Orig_CanReforge orig, T self) => orig(self);

    // PreReforge
    public delegate void Orig_PreReforge(T self);
    public virtual void Detour_PreReforge(Orig_PreReforge orig, T self) => orig(self);

    // PostReforge
    public delegate void Orig_PostReforge(T self);
    public virtual void Detour_PostReforge(Orig_PostReforge orig, T self) => orig(self);

    // DrawArmorColor
    public delegate void Orig_DrawArmorColor(T self, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor);
    public virtual void Detour_DrawArmorColor(Orig_DrawArmorColor orig, T self, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) => orig(self, drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);

    // ArmorArmGlowMask
    public delegate void Orig_ArmorArmGlowMask(T self, Player drawPlayer, float shadow, ref int glowMask, ref Color color);
    public virtual void Detour_ArmorArmGlowMask(Orig_ArmorArmGlowMask orig, T self, Player drawPlayer, float shadow, ref int glowMask, ref Color color) => orig(self, drawPlayer, shadow, ref glowMask, ref color);

    // VerticalWingSpeeds
    public delegate void Orig_VerticalWingSpeeds(T self, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend);
    public virtual void Detour_VerticalWingSpeeds(Orig_VerticalWingSpeeds orig, T self, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) => orig(self, player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);

    // HorizontalWingSpeeds
    public delegate void Orig_HorizontalWingSpeeds(T self, Player player, ref float speed, ref float acceleration);
    public virtual void Detour_HorizontalWingSpeeds(Orig_HorizontalWingSpeeds orig, T self, Player player, ref float speed, ref float acceleration) => orig(self, player, ref speed, ref acceleration);

    // WingUpdate
    public delegate bool Orig_WingUpdate(T self, Player player, bool inUse);
    public virtual bool Detour_WingUpdate(Orig_WingUpdate orig, T self, Player player, bool inUse) => orig(self, player, inUse);

    // Update
    public delegate void Orig_Update(T self, ref float gravity, ref float maxFallSpeed);
    public virtual void Detour_Update(Orig_Update orig, T self, ref float gravity, ref float maxFallSpeed) => orig(self, ref gravity, ref maxFallSpeed);

    // PostUpdate
    public delegate void Orig_PostUpdate(T self);
    public virtual void Detour_PostUpdate(Orig_PostUpdate orig, T self) => orig(self);

    // GrabRange
    public delegate void Orig_GrabRange(T self, Player player, ref int grabRange);
    public virtual void Detour_GrabRange(Orig_GrabRange orig, T self, Player player, ref int grabRange) => orig(self, player, ref grabRange);

    // GrabStyle
    public delegate bool Orig_GrabStyle(T self, Player player);
    public virtual bool Detour_GrabStyle(Orig_GrabStyle orig, T self, Player player) => orig(self, player);

    // CanPickup
    public delegate bool Orig_CanPickup(T self, Player player);
    public virtual bool Detour_CanPickup(Orig_CanPickup orig, T self, Player player) => orig(self, player);

    // OnPickup
    public delegate bool Orig_OnPickup(T self, Player player);
    public virtual bool Detour_OnPickup(Orig_OnPickup orig, T self, Player player) => orig(self, player);

    // ItemSpace
    public delegate bool Orig_ItemSpace(T self, Player player);
    public virtual bool Detour_ItemSpace(Orig_ItemSpace orig, T self, Player player) => orig(self, player);

    // GetAlpha
    public delegate Color? Orig_GetAlpha(T self, Color lightColor);
    public virtual Color? Detour_GetAlpha(Orig_GetAlpha orig, T self, Color lightColor) => orig(self, lightColor);

    // PreDrawInWorld
    public delegate bool Orig_PreDrawInWorld(T self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI);
    public virtual bool Detour_PreDrawInWorld(Orig_PreDrawInWorld orig, T self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => orig(self, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

    // PostDrawInWorld
    public delegate void Orig_PostDrawInWorld(T self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI);
    public virtual void Detour_PostDrawInWorld(Orig_PostDrawInWorld orig, T self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => orig(self, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);

    // PreDrawInInventory
    public delegate bool Orig_PreDrawInInventory(T self, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    public virtual bool Detour_PreDrawInInventory(Orig_PreDrawInInventory orig, T self, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => orig(self, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

    // PostDrawInInventory
    public delegate void Orig_PostDrawInInventory(T self, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    public virtual void Detour_PostDrawInInventory(Orig_PostDrawInInventory orig, T self, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => orig(self, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

    // HoldoutOffset
    public delegate Vector2? Orig_HoldoutOffset(T self);
    public virtual Vector2? Detour_HoldoutOffset(Orig_HoldoutOffset orig, T self) => orig(self);

    // HoldoutOrigin
    public delegate Vector2? Orig_HoldoutOrigin(T self);
    public virtual Vector2? Detour_HoldoutOrigin(Orig_HoldoutOrigin orig, T self) => orig(self);

    // CanEquipAccessory
    public delegate bool Orig_CanEquipAccessory(T self, Player player, int slot, bool modded);
    public virtual bool Detour_CanEquipAccessory(Orig_CanEquipAccessory orig, T self, Player player, int slot, bool modded) => orig(self, player, slot, modded);

    // CanAccessoryBeEquippedWith
    public delegate bool Orig_CanAccessoryBeEquippedWith(T self, Item equippedItem, Item incomingItem, Player player);
    public virtual bool Detour_CanAccessoryBeEquippedWith(Orig_CanAccessoryBeEquippedWith orig, T self, Item equippedItem, Item incomingItem, Player player) => orig(self, equippedItem, incomingItem, player);

    // ExtractinatorUse
    public delegate void Orig_ExtractinatorUse(T self, int extractinatorBlockType, ref int resultType, ref int resultStack);
    public virtual void Detour_ExtractinatorUse(Orig_ExtractinatorUse orig, T self, int extractinatorBlockType, ref int resultType, ref int resultStack) => orig(self, extractinatorBlockType, ref resultType, ref resultStack);

    // ModifyFishingLine
    public delegate void Orig_ModifyFishingLine(T self, Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor);
    public virtual void Detour_ModifyFishingLine(Orig_ModifyFishingLine orig, T self, Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor) => orig(self, bobber, ref lineOriginOffset, ref lineColor);

    // CaughtFishStack
    public delegate void Orig_CaughtFishStack(T self, ref int stack);
    public virtual void Detour_CaughtFishStack(Orig_CaughtFishStack orig, T self, ref int stack) => orig(self, ref stack);

    // IsQuestFish
    public delegate bool Orig_IsQuestFish(T self);
    public virtual bool Detour_IsQuestFish(Orig_IsQuestFish orig, T self) => orig(self);

    // IsAnglerQuestAvailable
    public delegate bool Orig_IsAnglerQuestAvailable(T self);
    public virtual bool Detour_IsAnglerQuestAvailable(Orig_IsAnglerQuestAvailable orig, T self) => orig(self);

    // AnglerQuestChat
    public delegate void Orig_AnglerQuestChat(T self, ref string description, ref string catchLocation);
    public virtual void Detour_AnglerQuestChat(Orig_AnglerQuestChat orig, T self, ref string description, ref string catchLocation) => orig(self, ref description, ref catchLocation);

    // SaveData
    public delegate void Orig_SaveData(T self, TagCompound tag);
    public virtual void Detour_SaveData(Orig_SaveData orig, T self, TagCompound tag) => orig(self, tag);

    // LoadData
    public delegate void Orig_LoadData(T self, TagCompound tag);
    public virtual void Detour_LoadData(Orig_LoadData orig, T self, TagCompound tag) => orig(self, tag);

    // NetSend
    public delegate void Orig_NetSend(T self, BinaryWriter writer);
    public virtual void Detour_NetSend(Orig_NetSend orig, T self, BinaryWriter writer) => orig(self, writer);

    // NetReceive
    public delegate void Orig_NetReceive(T self, BinaryReader reader);
    public virtual void Detour_NetReceive(Orig_NetReceive orig, T self, BinaryReader reader) => orig(self, reader);

    // AddRecipes
    public delegate void Orig_AddRecipes(T self);
    public virtual void Detour_AddRecipes(Orig_AddRecipes orig, T self) => orig(self);

    // PreDrawTooltip
    public delegate bool Orig_PreDrawTooltip(T self, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y);
    public virtual bool Detour_PreDrawTooltip(Orig_PreDrawTooltip orig, T self, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => orig(self, lines, ref x, ref y);

    // PostDrawTooltip
    public delegate void Orig_PostDrawTooltip(T self, ReadOnlyCollection<DrawableTooltipLine> lines);
    public virtual void Detour_PostDrawTooltip(Orig_PostDrawTooltip orig, T self, ReadOnlyCollection<DrawableTooltipLine> lines) => orig(self, lines);

    // PreDrawTooltipLine
    public delegate bool Orig_PreDrawTooltipLine(T self, DrawableTooltipLine line, ref int yOffset);
    public virtual bool Detour_PreDrawTooltipLine(Orig_PreDrawTooltipLine orig, T self, DrawableTooltipLine line, ref int yOffset) => orig(self, line, ref yOffset);

    // PostDrawTooltipLine
    public delegate void Orig_PostDrawTooltipLine(T self, DrawableTooltipLine line);
    public virtual void Detour_PostDrawTooltipLine(Orig_PostDrawTooltipLine orig, T self, DrawableTooltipLine line) => orig(self, line);

    // ModifyTooltips
    public delegate void Orig_ModifyTooltips(T self, List<TooltipLine> tooltips);
    public virtual void Detour_ModifyTooltips(Orig_ModifyTooltips orig, T self, List<TooltipLine> tooltips) => orig(self, tooltips);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_SetDefaults);
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_OnCreated);
        TryApplyDetour(Detour_AutoDefaults);
        TryApplyDetour(Detour_AutoStaticDefaults);
        TryApplyDetour(Detour_ChoosePrefix);
        TryApplyDetour(Detour_MeleePrefix);
        TryApplyDetour(Detour_WeaponPrefix);
        TryApplyDetour(Detour_RangedPrefix);
        TryApplyDetour(Detour_MagicPrefix);
        TryApplyDetour(Detour_PrefixChance);
        TryApplyDetour(Detour_AllowPrefix);
        TryApplyDetour(Detour_CanUseItem);
        TryApplyDetour(Detour_CanAutoReuseItem);
        TryApplyDetour(Detour_UseStyle);
        TryApplyDetour(Detour_HoldStyle);
        TryApplyDetour(Detour_HoldItem);
        TryApplyDetour(Detour_UseTimeMultiplier);
        TryApplyDetour(Detour_UseAnimationMultiplier);
        TryApplyDetour(Detour_UseSpeedMultiplier);
        TryApplyDetour(Detour_GetHealLife);
        TryApplyDetour(Detour_GetHealMana);
        TryApplyDetour(Detour_ModifyManaCost);
        TryApplyDetour(Detour_OnMissingMana);
        TryApplyDetour(Detour_OnConsumeMana);
        TryApplyDetour(Detour_ModifyWeaponDamage);
        TryApplyDetour(Detour_ModifyResearchSorting);
        TryApplyDetour(Detour_CanConsumeBait);
        TryApplyDetour(Detour_CanResearch);
        TryApplyDetour(Detour_OnResearched);
        TryApplyDetour(Detour_ModifyWeaponKnockback);
        TryApplyDetour(Detour_ModifyWeaponCrit);
        TryApplyDetour(Detour_NeedsAmmo);
        TryApplyDetour(Detour_PickAmmo);
        TryApplyDetour(Detour_CanChooseAmmo);
        TryApplyDetour(Detour_CanBeChosenAsAmmo);
        TryApplyDetour(Detour_CanConsumeAmmo);
        TryApplyDetour(Detour_CanBeConsumedAsAmmo);
        TryApplyDetour(Detour_OnConsumeAmmo);
        TryApplyDetour(Detour_OnConsumedAsAmmo);
        TryApplyDetour(Detour_CanShoot);
        TryApplyDetour(Detour_ModifyShootStats);
        TryApplyDetour(Detour_Shoot);
        TryApplyDetour(Detour_UseItemHitbox);
        TryApplyDetour(Detour_MeleeEffects);
        TryApplyDetour(Detour_CanCatchNPC);
        TryApplyDetour(Detour_OnCatchNPC);
        TryApplyDetour(Detour_ModifyItemScale);
        TryApplyDetour(Detour_CanHitNPC);
        TryApplyDetour(Detour_CanMeleeAttackCollideWithNPC);
        TryApplyDetour(Detour_ModifyHitNPC);
        TryApplyDetour(Detour_OnHitNPC);
        TryApplyDetour(Detour_CanHitPvp);
        TryApplyDetour(Detour_ModifyHitPvp);
        TryApplyDetour(Detour_OnHitPvp);
        TryApplyDetour(Detour_UseItem);
        TryApplyDetour(Detour_UseAnimation);
        TryApplyDetour(Detour_ConsumeItem);
        TryApplyDetour(Detour_OnConsumeItem);
        TryApplyDetour(Detour_UseItemFrame);
        TryApplyDetour(Detour_HoldItemFrame);
        TryApplyDetour(Detour_AltFunctionUse);
        TryApplyDetour(Detour_UpdateInventory);
        TryApplyDetour(Detour_UpdateInfoAccessory);
        TryApplyDetour(Detour_UpdateEquip);
        TryApplyDetour(Detour_UpdateAccessory);
        TryApplyDetour(Detour_UpdateVanity);
        TryApplyDetour(Detour_EquipFrameEffects);
        TryApplyDetour(Detour_IsArmorSet);
        TryApplyDetour(Detour_UpdateArmorSet);
        TryApplyDetour(Detour_IsVanitySet);
        TryApplyDetour(Detour_PreUpdateVanitySet);
        TryApplyDetour(Detour_UpdateVanitySet);
        TryApplyDetour(Detour_ArmorSetShadows);
        TryApplyDetour(Detour_SetMatch);
        TryApplyDetour(Detour_CanRightClick);
        TryApplyDetour(Detour_RightClick);
        TryApplyDetour(Detour_ModifyItemLoot);
        TryApplyDetour(Detour_CanStack);
        TryApplyDetour(Detour_CanStackInWorld);
        TryApplyDetour(Detour_OnStack);
        TryApplyDetour(Detour_SplitStack);
        TryApplyDetour(Detour_ReforgePrice);
        TryApplyDetour(Detour_CanReforge);
        TryApplyDetour(Detour_PreReforge);
        TryApplyDetour(Detour_PostReforge);
        TryApplyDetour(Detour_DrawArmorColor);
        TryApplyDetour(Detour_ArmorArmGlowMask);
        TryApplyDetour(Detour_VerticalWingSpeeds);
        TryApplyDetour(Detour_HorizontalWingSpeeds);
        TryApplyDetour(Detour_WingUpdate);
        TryApplyDetour(Detour_Update);
        TryApplyDetour(Detour_PostUpdate);
        TryApplyDetour(Detour_GrabRange);
        TryApplyDetour(Detour_GrabStyle);
        TryApplyDetour(Detour_CanPickup);
        TryApplyDetour(Detour_OnPickup);
        TryApplyDetour(Detour_ItemSpace);
        TryApplyDetour(Detour_GetAlpha);
        TryApplyDetour(Detour_PreDrawInWorld);
        TryApplyDetour(Detour_PostDrawInWorld);
        TryApplyDetour(Detour_PreDrawInInventory);
        TryApplyDetour(Detour_PostDrawInInventory);
        TryApplyDetour(Detour_HoldoutOffset);
        TryApplyDetour(Detour_HoldoutOrigin);
        TryApplyDetour(Detour_CanEquipAccessory);
        TryApplyDetour(Detour_CanAccessoryBeEquippedWith);
        TryApplyDetour(Detour_ExtractinatorUse);
        TryApplyDetour(Detour_ModifyFishingLine);
        TryApplyDetour(Detour_CaughtFishStack);
        TryApplyDetour(Detour_IsQuestFish);
        TryApplyDetour(Detour_IsAnglerQuestAvailable);
        TryApplyDetour(Detour_AnglerQuestChat);
        TryApplyDetour(Detour_SaveData);
        TryApplyDetour(Detour_LoadData);
        TryApplyDetour(Detour_NetSend);
        TryApplyDetour(Detour_NetReceive);
        TryApplyDetour(Detour_AddRecipes);
        TryApplyDetour(Detour_PreDrawTooltip);
        TryApplyDetour(Detour_PostDrawTooltip);
        TryApplyDetour(Detour_PreDrawTooltipLine);
        TryApplyDetour(Detour_PostDrawTooltipLine);
        TryApplyDetour(Detour_ModifyTooltips);
    }
}

public abstract class ModKeybindDetour<T> : TypeDetour<T> where T : ModKeybind
{
    // GetAssignedKeys
    public delegate List<string> Orig_GetAssignedKeys(T self, InputMode mode);
    public virtual List<string> Detour_GetAssignedKeys(Orig_GetAssignedKeys orig, T self, InputMode mode) => orig(self, mode);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_GetAssignedKeys);
    }
}

public abstract class ModMapLayerDetour<T> : ModTypeDetour<T> where T : ModMapLayer
{
    // GetDefaultPosition
    public delegate ModMapLayer.Position Orig_GetDefaultPosition(T self);
    public virtual ModMapLayer.Position Detour_GetDefaultPosition(Orig_GetDefaultPosition orig, T self) => orig(self);

    // GetModdedConstraints
    public delegate IEnumerable<ModMapLayer.Position> Orig_GetModdedConstraints(T self);
    public virtual IEnumerable<ModMapLayer.Position> Detour_GetModdedConstraints(Orig_GetModdedConstraints orig, T self) => orig(self);

    // Draw
    public delegate void Orig_Draw(T self, ref MapOverlayDrawContext context, ref string text);
    public virtual void Detour_Draw(Orig_Draw orig, T self, ref MapOverlayDrawContext context, ref string text) => orig(self, ref context, ref text);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_GetDefaultPosition);
        TryApplyDetour(Detour_GetModdedConstraints);
        TryApplyDetour(Detour_Draw);
    }
}

public abstract class ModMenuDetour<T> : ModTypeDetour<T> where T : ModMenu
{
    // OnSelected
    public delegate void Orig_OnSelected(T self);
    public virtual void Detour_OnSelected(Orig_OnSelected orig, T self) => orig(self);

    // OnDeselected
    public delegate void Orig_OnDeselected(T self);
    public virtual void Detour_OnDeselected(Orig_OnDeselected orig, T self) => orig(self);

    // Update
    public delegate void Orig_Update(T self, bool isOnTitleScreen);
    public virtual void Detour_Update(Orig_Update orig, T self, bool isOnTitleScreen) => orig(self, isOnTitleScreen);

    // PreDrawLogo
    public delegate bool Orig_PreDrawLogo(T self, SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor);
    public virtual bool Detour_PreDrawLogo(Orig_PreDrawLogo orig, T self, SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) => orig(self, spriteBatch, ref logoDrawCenter, ref logoRotation, ref logoScale, ref drawColor);

    // PostDrawLogo
    public delegate void Orig_PostDrawLogo(T self, SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor);
    public virtual void Detour_PostDrawLogo(Orig_PostDrawLogo orig, T self, SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor) => orig(self, spriteBatch, logoDrawCenter, logoRotation, logoScale, drawColor);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_OnSelected);
        TryApplyDetour(Detour_OnDeselected);
        TryApplyDetour(Detour_Update);
        TryApplyDetour(Detour_PreDrawLogo);
        TryApplyDetour(Detour_PostDrawLogo);
    }
}

public abstract class ModMountDetour<T> : ModTypeDetour<T> where T : ModMount
{
    // JumpHeight
    public delegate void Orig_JumpHeight(T self, Player mountedPlayer, ref int jumpHeight, float xVelocity);
    public virtual void Detour_JumpHeight(Orig_JumpHeight orig, T self, Player mountedPlayer, ref int jumpHeight, float xVelocity) => orig(self, mountedPlayer, ref jumpHeight, xVelocity);

    // JumpSpeed
    public delegate void Orig_JumpSpeed(T self, Player mountedPlayer, ref float jumpSeed, float xVelocity);
    public virtual void Detour_JumpSpeed(Orig_JumpSpeed orig, T self, Player mountedPlayer, ref float jumpSeed, float xVelocity) => orig(self, mountedPlayer, ref jumpSeed, xVelocity);

    // UpdateEffects
    public delegate void Orig_UpdateEffects(T self, Player player);
    public virtual void Detour_UpdateEffects(Orig_UpdateEffects orig, T self, Player player) => orig(self, player);

    // UpdateFrame
    public delegate bool Orig_UpdateFrame(T self, Player mountedPlayer, int state, Vector2 velocity);
    public virtual bool Detour_UpdateFrame(Orig_UpdateFrame orig, T self, Player mountedPlayer, int state, Vector2 velocity) => orig(self, mountedPlayer, state, velocity);

    // UseAbility
    public delegate void Orig_UseAbility(T self, Player player, Vector2 mousePosition, bool toggleOn);
    public virtual void Detour_UseAbility(Orig_UseAbility orig, T self, Player player, Vector2 mousePosition, bool toggleOn) => orig(self, player, mousePosition, toggleOn);

    // AimAbility
    public delegate void Orig_AimAbility(T self, Player player, Vector2 mousePosition);
    public virtual void Detour_AimAbility(Orig_AimAbility orig, T self, Player player, Vector2 mousePosition) => orig(self, player, mousePosition);

    // SetMount
    public delegate void Orig_SetMount(T self, Player player, ref bool skipDust);
    public virtual void Detour_SetMount(Orig_SetMount orig, T self, Player player, ref bool skipDust) => orig(self, player, ref skipDust);

    // Dismount
    public delegate void Orig_Dismount(T self, Player player, ref bool skipDust);
    public virtual void Detour_Dismount(Orig_Dismount orig, T self, Player player, ref bool skipDust) => orig(self, player, ref skipDust);

    // Draw
    public delegate bool Orig_Draw(T self, List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow);
    public virtual bool Detour_Draw(Orig_Draw orig, T self, List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) => orig(self, playerDrawData, drawType, drawPlayer, ref texture, ref glowTexture, ref drawPosition, ref frame, ref drawColor, ref glowColor, ref rotation, ref spriteEffects, ref drawOrigin, ref drawScale, shadow);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_JumpHeight);
        TryApplyDetour(Detour_JumpSpeed);
        TryApplyDetour(Detour_UpdateEffects);
        TryApplyDetour(Detour_UpdateFrame);
        TryApplyDetour(Detour_UseAbility);
        TryApplyDetour(Detour_AimAbility);
        TryApplyDetour(Detour_SetMount);
        TryApplyDetour(Detour_Dismount);
        TryApplyDetour(Detour_Draw);
    }
}

public abstract class ModNPCDetour<T> : ModTypeDetour<T> where T : ModNPC
{
    // SetDefaults
    public delegate void Orig_SetDefaults(T self);
    public virtual void Detour_SetDefaults(Orig_SetDefaults orig, T self) => orig(self);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self, IEntitySource source);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, IEntitySource source) => orig(self, source);

    // AutoStaticDefaults
    public delegate void Orig_AutoStaticDefaults(T self);
    public virtual void Detour_AutoStaticDefaults(Orig_AutoStaticDefaults orig, T self) => orig(self);

    // ApplyDifficultyAndPlayerScaling
    public delegate void Orig_ApplyDifficultyAndPlayerScaling(T self, int numPlayers, float balance, float bossAdjustment);
    public virtual void Detour_ApplyDifficultyAndPlayerScaling(Orig_ApplyDifficultyAndPlayerScaling orig, T self, int numPlayers, float balance, float bossAdjustment) => orig(self, numPlayers, balance, bossAdjustment);

    // SetBestiary
    public delegate void Orig_SetBestiary(T self, BestiaryDatabase database, BestiaryEntry bestiaryEntry);
    public virtual void Detour_SetBestiary(Orig_SetBestiary orig, T self, BestiaryDatabase database, BestiaryEntry bestiaryEntry) => orig(self, database, bestiaryEntry);

    // ModifyTypeName
    public delegate void Orig_ModifyTypeName(T self, ref string typeName);
    public virtual void Detour_ModifyTypeName(Orig_ModifyTypeName orig, T self, ref string typeName) => orig(self, ref typeName);

    // ModifyHoverBoundingBox
    public delegate void Orig_ModifyHoverBoundingBox(T self, ref Rectangle boundingBox);
    public virtual void Detour_ModifyHoverBoundingBox(Orig_ModifyHoverBoundingBox orig, T self, ref Rectangle boundingBox) => orig(self, ref boundingBox);

    // SetNPCNameList
    public delegate List<string> Orig_SetNPCNameList(T self);
    public virtual List<string> Detour_SetNPCNameList(Orig_SetNPCNameList orig, T self) => orig(self);

    // TownNPCProfile
    public delegate ITownNPCProfile Orig_TownNPCProfile(T self);
    public virtual ITownNPCProfile Detour_TownNPCProfile(Orig_TownNPCProfile orig, T self) => orig(self);

    // ResetEffects
    public delegate void Orig_ResetEffects(T self);
    public virtual void Detour_ResetEffects(Orig_ResetEffects orig, T self) => orig(self);

    // PreAI
    public delegate bool Orig_PreAI(T self);
    public virtual bool Detour_PreAI(Orig_PreAI orig, T self) => orig(self);

    // AI
    public delegate void Orig_AI(T self);
    public virtual void Detour_AI(Orig_AI orig, T self) => orig(self);

    // PostAI
    public delegate void Orig_PostAI(T self);
    public virtual void Detour_PostAI(Orig_PostAI orig, T self) => orig(self);

    // SendExtraAI
    public delegate void Orig_SendExtraAI(T self, BinaryWriter writer);
    public virtual void Detour_SendExtraAI(Orig_SendExtraAI orig, T self, BinaryWriter writer) => orig(self, writer);

    // ReceiveExtraAI
    public delegate void Orig_ReceiveExtraAI(T self, BinaryReader reader);
    public virtual void Detour_ReceiveExtraAI(Orig_ReceiveExtraAI orig, T self, BinaryReader reader) => orig(self, reader);

    // FindFrame
    public delegate void Orig_FindFrame(T self, int frameHeight);
    public virtual void Detour_FindFrame(Orig_FindFrame orig, T self, int frameHeight) => orig(self, frameHeight);

    // HitEffect
    public delegate void Orig_HitEffect(T self, NPC.HitInfo hit);
    public virtual void Detour_HitEffect(Orig_HitEffect orig, T self, NPC.HitInfo hit) => orig(self, hit);

    // UpdateLifeRegen
    public delegate void Orig_UpdateLifeRegen(T self, ref int damage);
    public virtual void Detour_UpdateLifeRegen(Orig_UpdateLifeRegen orig, T self, ref int damage) => orig(self, ref damage);

    // CheckActive
    public delegate bool Orig_CheckActive(T self);
    public virtual bool Detour_CheckActive(Orig_CheckActive orig, T self) => orig(self);

    // CheckDead
    public delegate bool Orig_CheckDead(T self);
    public virtual bool Detour_CheckDead(Orig_CheckDead orig, T self) => orig(self);

    // SpecialOnKill
    public delegate bool Orig_SpecialOnKill(T self);
    public virtual bool Detour_SpecialOnKill(Orig_SpecialOnKill orig, T self) => orig(self);

    // PreKill
    public delegate bool Orig_PreKill(T self);
    public virtual bool Detour_PreKill(Orig_PreKill orig, T self) => orig(self);

    // OnKill
    public delegate void Orig_OnKill(T self);
    public virtual void Detour_OnKill(Orig_OnKill orig, T self) => orig(self);

    // CanFallThroughPlatforms
    public delegate bool? Orig_CanFallThroughPlatforms(T self);
    public virtual bool? Detour_CanFallThroughPlatforms(Orig_CanFallThroughPlatforms orig, T self) => orig(self);

    // CanBeCaughtBy
    public delegate bool? Orig_CanBeCaughtBy(T self, Item item, Player player);
    public virtual bool? Detour_CanBeCaughtBy(Orig_CanBeCaughtBy orig, T self, Item item, Player player) => orig(self, item, player);

    // OnCaughtBy
    public delegate void Orig_OnCaughtBy(T self, Player player, Item item, bool failed);
    public virtual void Detour_OnCaughtBy(Orig_OnCaughtBy orig, T self, Player player, Item item, bool failed) => orig(self, player, item, failed);

    // ModifyNPCLoot
    public delegate void Orig_ModifyNPCLoot(T self, NPCLoot npcLoot);
    public virtual void Detour_ModifyNPCLoot(Orig_ModifyNPCLoot orig, T self, NPCLoot npcLoot) => orig(self, npcLoot);

    // BossLoot
    public delegate void Orig_BossLoot(T self, ref string name, ref int potionType);
    public virtual void Detour_BossLoot(Orig_BossLoot orig, T self, ref string name, ref int potionType) => orig(self, ref name, ref potionType);

    // CanHitPlayer
    public delegate bool Orig_CanHitPlayer(T self, Player target, ref int cooldownSlot);
    public virtual bool Detour_CanHitPlayer(Orig_CanHitPlayer orig, T self, Player target, ref int cooldownSlot) => orig(self, target, ref cooldownSlot);

    // ModifyHitPlayer
    public delegate void Orig_ModifyHitPlayer(T self, Player target, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHitPlayer(Orig_ModifyHitPlayer orig, T self, Player target, ref Player.HurtModifiers modifiers) => orig(self, target, ref modifiers);

    // OnHitPlayer
    public delegate void Orig_OnHitPlayer(T self, Player target, Player.HurtInfo hurtInfo);
    public virtual void Detour_OnHitPlayer(Orig_OnHitPlayer orig, T self, Player target, Player.HurtInfo hurtInfo) => orig(self, target, hurtInfo);

    // CanHitNPC
    public delegate bool Orig_CanHitNPC(T self, NPC target);
    public virtual bool Detour_CanHitNPC(Orig_CanHitNPC orig, T self, NPC target) => orig(self, target);

    // CanBeHitByNPC
    public delegate bool Orig_CanBeHitByNPC(T self, NPC attacker);
    public virtual bool Detour_CanBeHitByNPC(Orig_CanBeHitByNPC orig, T self, NPC attacker) => orig(self, attacker);

    // ModifyHitNPC
    public delegate void Orig_ModifyHitNPC(T self, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, T self, NPC target, ref NPC.HitModifiers modifiers) => orig(self, target, ref modifiers);

    // OnHitNPC
    public delegate void Orig_OnHitNPC(T self, NPC target, NPC.HitInfo hit);
    public virtual void Detour_OnHitNPC(Orig_OnHitNPC orig, T self, NPC target, NPC.HitInfo hit) => orig(self, target, hit);

    // CanBeHitByItem
    public delegate bool? Orig_CanBeHitByItem(T self, Player player, Item item);
    public virtual bool? Detour_CanBeHitByItem(Orig_CanBeHitByItem orig, T self, Player player, Item item) => orig(self, player, item);

    // CanCollideWithPlayerMeleeAttack
    public delegate bool? Orig_CanCollideWithPlayerMeleeAttack(T self, Player player, Item item, Rectangle meleeAttackHitbox);
    public virtual bool? Detour_CanCollideWithPlayerMeleeAttack(Orig_CanCollideWithPlayerMeleeAttack orig, T self, Player player, Item item, Rectangle meleeAttackHitbox) => orig(self, player, item, meleeAttackHitbox);

    // ModifyHitByItem
    public delegate void Orig_ModifyHitByItem(T self, Player player, Item item, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitByItem(Orig_ModifyHitByItem orig, T self, Player player, Item item, ref NPC.HitModifiers modifiers) => orig(self, player, item, ref modifiers);

    // OnHitByItem
    public delegate void Orig_OnHitByItem(T self, Player player, Item item, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitByItem(Orig_OnHitByItem orig, T self, Player player, Item item, NPC.HitInfo hit, int damageDone) => orig(self, player, item, hit, damageDone);

    // CanBeHitByProjectile
    public delegate bool? Orig_CanBeHitByProjectile(T self, Projectile projectile);
    public virtual bool? Detour_CanBeHitByProjectile(Orig_CanBeHitByProjectile orig, T self, Projectile projectile) => orig(self, projectile);

    // ModifyHitByProjectile
    public delegate void Orig_ModifyHitByProjectile(T self, Projectile projectile, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitByProjectile(Orig_ModifyHitByProjectile orig, T self, Projectile projectile, ref NPC.HitModifiers modifiers) => orig(self, projectile, ref modifiers);

    // OnHitByProjectile
    public delegate void Orig_OnHitByProjectile(T self, Projectile projectile, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitByProjectile(Orig_OnHitByProjectile orig, T self, Projectile projectile, NPC.HitInfo hit, int damageDone) => orig(self, projectile, hit, damageDone);

    // ModifyIncomingHit
    public delegate void Orig_ModifyIncomingHit(T self, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyIncomingHit(Orig_ModifyIncomingHit orig, T self, ref NPC.HitModifiers modifiers) => orig(self, ref modifiers);

    // BossHeadSlot
    public delegate void Orig_BossHeadSlot(T self, ref int index);
    public virtual void Detour_BossHeadSlot(Orig_BossHeadSlot orig, T self, ref int index) => orig(self, ref index);

    // BossHeadRotation
    public delegate void Orig_BossHeadRotation(T self, ref float rotation);
    public virtual void Detour_BossHeadRotation(Orig_BossHeadRotation orig, T self, ref float rotation) => orig(self, ref rotation);

    // BossHeadSpriteEffects
    public delegate void Orig_BossHeadSpriteEffects(T self, ref SpriteEffects spriteEffects);
    public virtual void Detour_BossHeadSpriteEffects(Orig_BossHeadSpriteEffects orig, T self, ref SpriteEffects spriteEffects) => orig(self, ref spriteEffects);

    // GetAlpha
    public delegate Color? Orig_GetAlpha(T self, Color drawColor);
    public virtual Color? Detour_GetAlpha(Orig_GetAlpha orig, T self, Color drawColor) => orig(self, drawColor);

    // DrawEffects
    public delegate void Orig_DrawEffects(T self, ref Color drawColor);
    public virtual void Detour_DrawEffects(Orig_DrawEffects orig, T self, ref Color drawColor) => orig(self, ref drawColor);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => orig(self, spriteBatch, screenPos, drawColor);

    // PostDraw
    public delegate void Orig_PostDraw(T self, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => orig(self, spriteBatch, screenPos, drawColor);

    // DrawBehind
    public delegate void Orig_DrawBehind(T self, int index);
    public virtual void Detour_DrawBehind(Orig_DrawBehind orig, T self, int index) => orig(self, index);

    // DrawHealthBar
    public delegate bool? Orig_DrawHealthBar(T self, byte hbPosition, ref float scale, ref Vector2 position);
    public virtual bool? Detour_DrawHealthBar(Orig_DrawHealthBar orig, T self, byte hbPosition, ref float scale, ref Vector2 position) => orig(self, hbPosition, ref scale, ref position);

    // SpawnChance
    public delegate float Orig_SpawnChance(T self, NPCSpawnInfo spawnInfo);
    public virtual float Detour_SpawnChance(Orig_SpawnChance orig, T self, NPCSpawnInfo spawnInfo) => orig(self, spawnInfo);

    // SpawnNPC
    public delegate int Orig_SpawnNPC(T self, int tileX, int tileY);
    public virtual int Detour_SpawnNPC(Orig_SpawnNPC orig, T self, int tileX, int tileY) => orig(self, tileX, tileY);

    // CanTownNPCSpawn
    public delegate bool Orig_CanTownNPCSpawn(T self, int numTownNPCs);
    public virtual bool Detour_CanTownNPCSpawn(Orig_CanTownNPCSpawn orig, T self, int numTownNPCs) => orig(self, numTownNPCs);

    // CheckConditions
    public delegate bool Orig_CheckConditions(T self, int left, int right, int top, int bottom);
    public virtual bool Detour_CheckConditions(Orig_CheckConditions orig, T self, int left, int right, int top, int bottom) => orig(self, left, right, top, bottom);

    // UsesPartyHat
    public delegate bool Orig_UsesPartyHat(T self);
    public virtual bool Detour_UsesPartyHat(Orig_UsesPartyHat orig, T self) => orig(self);

    // CanChat
    public delegate bool Orig_CanChat(T self);
    public virtual bool Detour_CanChat(Orig_CanChat orig, T self) => orig(self);

    // GetChat
    public delegate string Orig_GetChat(T self);
    public virtual string Detour_GetChat(Orig_GetChat orig, T self) => orig(self);

    // SetChatButtons
    public delegate void Orig_SetChatButtons(T self, ref string button, ref string button2);
    public virtual void Detour_SetChatButtons(Orig_SetChatButtons orig, T self, ref string button, ref string button2) => orig(self, ref button, ref button2);

    // OnChatButtonClicked
    public delegate void Orig_OnChatButtonClicked(T self, bool firstButton, ref string shopName);
    public virtual void Detour_OnChatButtonClicked(Orig_OnChatButtonClicked orig, T self, bool firstButton, ref string shopName) => orig(self, firstButton, ref shopName);

    // AddShops
    public delegate void Orig_AddShops(T self);
    public virtual void Detour_AddShops(Orig_AddShops orig, T self) => orig(self);

    // ModifyActiveShop
    public delegate void Orig_ModifyActiveShop(T self, string shopName, Item[] items);
    public virtual void Detour_ModifyActiveShop(Orig_ModifyActiveShop orig, T self, string shopName, Item[] items) => orig(self, shopName, items);

    // CanGoToStatue
    public delegate bool Orig_CanGoToStatue(T self, bool toKingStatue);
    public virtual bool Detour_CanGoToStatue(Orig_CanGoToStatue orig, T self, bool toKingStatue) => orig(self, toKingStatue);

    // OnGoToStatue
    public delegate void Orig_OnGoToStatue(T self, bool toKingStatue);
    public virtual void Detour_OnGoToStatue(Orig_OnGoToStatue orig, T self, bool toKingStatue) => orig(self, toKingStatue);

    // TownNPCAttackStrength
    public delegate void Orig_TownNPCAttackStrength(T self, ref int damage, ref float knockback);
    public virtual void Detour_TownNPCAttackStrength(Orig_TownNPCAttackStrength orig, T self, ref int damage, ref float knockback) => orig(self, ref damage, ref knockback);

    // TownNPCAttackCooldown
    public delegate void Orig_TownNPCAttackCooldown(T self, ref int cooldown, ref int randExtraCooldown);
    public virtual void Detour_TownNPCAttackCooldown(Orig_TownNPCAttackCooldown orig, T self, ref int cooldown, ref int randExtraCooldown) => orig(self, ref cooldown, ref randExtraCooldown);

    // TownNPCAttackProj
    public delegate void Orig_TownNPCAttackProj(T self, ref int projType, ref int attackDelay);
    public virtual void Detour_TownNPCAttackProj(Orig_TownNPCAttackProj orig, T self, ref int projType, ref int attackDelay) => orig(self, ref projType, ref attackDelay);

    // TownNPCAttackProjSpeed
    public delegate void Orig_TownNPCAttackProjSpeed(T self, ref float multiplier, ref float gravityCorrection, ref float randomOffset);
    public virtual void Detour_TownNPCAttackProjSpeed(Orig_TownNPCAttackProjSpeed orig, T self, ref float multiplier, ref float gravityCorrection, ref float randomOffset) => orig(self, ref multiplier, ref gravityCorrection, ref randomOffset);

    // TownNPCAttackShoot
    public delegate void Orig_TownNPCAttackShoot(T self, ref bool inBetweenShots);
    public virtual void Detour_TownNPCAttackShoot(Orig_TownNPCAttackShoot orig, T self, ref bool inBetweenShots) => orig(self, ref inBetweenShots);

    // TownNPCAttackMagic
    public delegate void Orig_TownNPCAttackMagic(T self, ref float auraLightMultiplier);
    public virtual void Detour_TownNPCAttackMagic(Orig_TownNPCAttackMagic orig, T self, ref float auraLightMultiplier) => orig(self, ref auraLightMultiplier);

    // TownNPCAttackSwing
    public delegate void Orig_TownNPCAttackSwing(T self, ref int itemWidth, ref int itemHeight);
    public virtual void Detour_TownNPCAttackSwing(Orig_TownNPCAttackSwing orig, T self, ref int itemWidth, ref int itemHeight) => orig(self, ref itemWidth, ref itemHeight);

    // DrawTownAttackGun
    public delegate void Orig_DrawTownAttackGun(T self, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset);
    public virtual void Detour_DrawTownAttackGun(Orig_DrawTownAttackGun orig, T self, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) => orig(self, ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);

    // DrawTownAttackSwing
    public delegate void Orig_DrawTownAttackSwing(T self, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset);
    public virtual void Detour_DrawTownAttackSwing(Orig_DrawTownAttackSwing orig, T self, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) => orig(self, ref item, ref itemFrame, ref itemSize, ref scale, ref offset);

    // ModifyCollisionData
    public delegate bool Orig_ModifyCollisionData(T self, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox);
    public virtual bool Detour_ModifyCollisionData(Orig_ModifyCollisionData orig, T self, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => orig(self, victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);

    // NeedSaving
    public delegate bool Orig_NeedSaving(T self);
    public virtual bool Detour_NeedSaving(Orig_NeedSaving orig, T self) => orig(self);

    // SaveData
    public delegate void Orig_SaveData(T self, TagCompound tag);
    public virtual void Detour_SaveData(Orig_SaveData orig, T self, TagCompound tag) => orig(self, tag);

    // LoadData
    public delegate void Orig_LoadData(T self, TagCompound tag);
    public virtual void Detour_LoadData(Orig_LoadData orig, T self, TagCompound tag) => orig(self, tag);

    // ChatBubblePosition
    public delegate void Orig_ChatBubblePosition(T self, ref Vector2 position, ref SpriteEffects spriteEffects);
    public virtual void Detour_ChatBubblePosition(Orig_ChatBubblePosition orig, T self, ref Vector2 position, ref SpriteEffects spriteEffects) => orig(self, ref position, ref spriteEffects);

    // PartyHatPosition
    public delegate void Orig_PartyHatPosition(T self, ref Vector2 position, ref SpriteEffects spriteEffects);
    public virtual void Detour_PartyHatPosition(Orig_PartyHatPosition orig, T self, ref Vector2 position, ref SpriteEffects spriteEffects) => orig(self, ref position, ref spriteEffects);

    // EmoteBubblePosition
    public delegate void Orig_EmoteBubblePosition(T self, ref Vector2 position, ref SpriteEffects spriteEffects);
    public virtual void Detour_EmoteBubblePosition(Orig_EmoteBubblePosition orig, T self, ref Vector2 position, ref SpriteEffects spriteEffects) => orig(self, ref position, ref spriteEffects);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_SetDefaults);
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_AutoStaticDefaults);
        TryApplyDetour(Detour_ApplyDifficultyAndPlayerScaling);
        TryApplyDetour(Detour_SetBestiary);
        TryApplyDetour(Detour_ModifyTypeName);
        TryApplyDetour(Detour_ModifyHoverBoundingBox);
        TryApplyDetour(Detour_SetNPCNameList);
        TryApplyDetour(Detour_TownNPCProfile);
        TryApplyDetour(Detour_ResetEffects);
        TryApplyDetour(Detour_PreAI);
        TryApplyDetour(Detour_AI);
        TryApplyDetour(Detour_PostAI);
        TryApplyDetour(Detour_SendExtraAI);
        TryApplyDetour(Detour_ReceiveExtraAI);
        TryApplyDetour(Detour_FindFrame);
        TryApplyDetour(Detour_HitEffect);
        TryApplyDetour(Detour_UpdateLifeRegen);
        TryApplyDetour(Detour_CheckActive);
        TryApplyDetour(Detour_CheckDead);
        TryApplyDetour(Detour_SpecialOnKill);
        TryApplyDetour(Detour_PreKill);
        TryApplyDetour(Detour_OnKill);
        TryApplyDetour(Detour_CanFallThroughPlatforms);
        TryApplyDetour(Detour_CanBeCaughtBy);
        TryApplyDetour(Detour_OnCaughtBy);
        TryApplyDetour(Detour_ModifyNPCLoot);
        TryApplyDetour(Detour_BossLoot);
        TryApplyDetour(Detour_CanHitPlayer);
        TryApplyDetour(Detour_ModifyHitPlayer);
        TryApplyDetour(Detour_OnHitPlayer);
        TryApplyDetour(Detour_CanHitNPC);
        TryApplyDetour(Detour_CanBeHitByNPC);
        TryApplyDetour(Detour_ModifyHitNPC);
        TryApplyDetour(Detour_OnHitNPC);
        TryApplyDetour(Detour_CanBeHitByItem);
        TryApplyDetour(Detour_CanCollideWithPlayerMeleeAttack);
        TryApplyDetour(Detour_ModifyHitByItem);
        TryApplyDetour(Detour_OnHitByItem);
        TryApplyDetour(Detour_CanBeHitByProjectile);
        TryApplyDetour(Detour_ModifyHitByProjectile);
        TryApplyDetour(Detour_OnHitByProjectile);
        TryApplyDetour(Detour_ModifyIncomingHit);
        TryApplyDetour(Detour_BossHeadSlot);
        TryApplyDetour(Detour_BossHeadRotation);
        TryApplyDetour(Detour_BossHeadSpriteEffects);
        TryApplyDetour(Detour_GetAlpha);
        TryApplyDetour(Detour_DrawEffects);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_DrawBehind);
        TryApplyDetour(Detour_DrawHealthBar);
        TryApplyDetour(Detour_SpawnChance);
        TryApplyDetour(Detour_SpawnNPC);
        TryApplyDetour(Detour_CanTownNPCSpawn);
        TryApplyDetour(Detour_CheckConditions);
        TryApplyDetour(Detour_UsesPartyHat);
        TryApplyDetour(Detour_CanChat);
        TryApplyDetour(Detour_GetChat);
        TryApplyDetour(Detour_SetChatButtons);
        TryApplyDetour(Detour_OnChatButtonClicked);
        TryApplyDetour(Detour_AddShops);
        TryApplyDetour(Detour_ModifyActiveShop);
        TryApplyDetour(Detour_CanGoToStatue);
        TryApplyDetour(Detour_OnGoToStatue);
        TryApplyDetour(Detour_TownNPCAttackStrength);
        TryApplyDetour(Detour_TownNPCAttackCooldown);
        TryApplyDetour(Detour_TownNPCAttackProj);
        TryApplyDetour(Detour_TownNPCAttackProjSpeed);
        TryApplyDetour(Detour_TownNPCAttackShoot);
        TryApplyDetour(Detour_TownNPCAttackMagic);
        TryApplyDetour(Detour_TownNPCAttackSwing);
        TryApplyDetour(Detour_DrawTownAttackGun);
        TryApplyDetour(Detour_DrawTownAttackSwing);
        TryApplyDetour(Detour_ModifyCollisionData);
        TryApplyDetour(Detour_NeedSaving);
        TryApplyDetour(Detour_SaveData);
        TryApplyDetour(Detour_LoadData);
        TryApplyDetour(Detour_ChatBubblePosition);
        TryApplyDetour(Detour_PartyHatPosition);
        TryApplyDetour(Detour_EmoteBubblePosition);
    }
}

public abstract class ModPalmTreeDetour<T> : TypeDetour<T> where T : ModPalmTree
{
    // SetStaticDefaults
    public delegate void Orig_SetStaticDefaults(T self);
    public virtual void Detour_SetStaticDefaults(Orig_SetStaticDefaults orig, T self) => orig(self);

    // GetTexture
    public delegate Asset<Texture2D> Orig_GetTexture(T self);
    public virtual Asset<Texture2D> Detour_GetTexture(Orig_GetTexture orig, T self) => orig(self);

    // CountsAsTreeType
    public delegate TreeTypes Orig_CountsAsTreeType(T self);
    public virtual TreeTypes Detour_CountsAsTreeType(Orig_CountsAsTreeType orig, T self) => orig(self);

    // CreateDust
    public delegate int Orig_CreateDust(T self);
    public virtual int Detour_CreateDust(Orig_CreateDust orig, T self) => orig(self);

    // TreeLeaf
    public delegate int Orig_TreeLeaf(T self);
    public virtual int Detour_TreeLeaf(Orig_TreeLeaf orig, T self) => orig(self);

    // Shake
    public delegate bool Orig_Shake(T self, int x, int y, ref bool createLeaves);
    public virtual bool Detour_Shake(Orig_Shake orig, T self, int x, int y, ref bool createLeaves) => orig(self, x, y, ref createLeaves);

    // SaplingGrowthType
    public delegate int Orig_SaplingGrowthType(T self, ref int style);
    public virtual int Detour_SaplingGrowthType(Orig_SaplingGrowthType orig, T self, ref int style) => orig(self, ref style);

    // DropWood
    public delegate int Orig_DropWood(T self);
    public virtual int Detour_DropWood(Orig_DropWood orig, T self) => orig(self);

    // GetTopTextures
    public delegate Asset<Texture2D> Orig_GetTopTextures(T self);
    public virtual Asset<Texture2D> Detour_GetTopTextures(Orig_GetTopTextures orig, T self) => orig(self);

    // GetOasisTopTextures
    public delegate Asset<Texture2D> Orig_GetOasisTopTextures(T self);
    public virtual Asset<Texture2D> Detour_GetOasisTopTextures(Orig_GetOasisTopTextures orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_SetStaticDefaults);
        TryApplyDetour(Detour_GetTexture);
        TryApplyDetour(Detour_CountsAsTreeType);
        TryApplyDetour(Detour_CreateDust);
        TryApplyDetour(Detour_TreeLeaf);
        TryApplyDetour(Detour_Shake);
        TryApplyDetour(Detour_SaplingGrowthType);
        TryApplyDetour(Detour_DropWood);
        TryApplyDetour(Detour_GetTopTextures);
        TryApplyDetour(Detour_GetOasisTopTextures);
    }
}

public abstract class ModPlayerDetour<T> : ModTypeDetour<T> where T : ModPlayer
{
    // Initialize
    public delegate void Orig_Initialize(T self);
    public virtual void Detour_Initialize(Orig_Initialize orig, T self) => orig(self);

    // ResetEffects
    public delegate void Orig_ResetEffects(T self);
    public virtual void Detour_ResetEffects(Orig_ResetEffects orig, T self) => orig(self);

    // ResetInfoAccessories
    public delegate void Orig_ResetInfoAccessories(T self);
    public virtual void Detour_ResetInfoAccessories(Orig_ResetInfoAccessories orig, T self) => orig(self);

    // RefreshInfoAccessoriesFromTeamPlayers
    public delegate void Orig_RefreshInfoAccessoriesFromTeamPlayers(T self, Player otherPlayer);
    public virtual void Detour_RefreshInfoAccessoriesFromTeamPlayers(Orig_RefreshInfoAccessoriesFromTeamPlayers orig, T self, Player otherPlayer) => orig(self, otherPlayer);

    // ModifyMaxStats
    public delegate void Orig_ModifyMaxStats(T self, out StatModifier health, out StatModifier mana);
    public virtual void Detour_ModifyMaxStats(Orig_ModifyMaxStats orig, T self, out StatModifier health, out StatModifier mana) => orig(self, out health, out mana);

    // UpdateDead
    public delegate void Orig_UpdateDead(T self);
    public virtual void Detour_UpdateDead(Orig_UpdateDead orig, T self) => orig(self);

    // PreSaveCustomData
    public delegate void Orig_PreSaveCustomData(T self);
    public virtual void Detour_PreSaveCustomData(Orig_PreSaveCustomData orig, T self) => orig(self);

    // SaveData
    public delegate void Orig_SaveData(T self, TagCompound tag);
    public virtual void Detour_SaveData(Orig_SaveData orig, T self, TagCompound tag) => orig(self, tag);

    // LoadData
    public delegate void Orig_LoadData(T self, TagCompound tag);
    public virtual void Detour_LoadData(Orig_LoadData orig, T self, TagCompound tag) => orig(self, tag);

    // PreSavePlayer
    public delegate void Orig_PreSavePlayer(T self);
    public virtual void Detour_PreSavePlayer(Orig_PreSavePlayer orig, T self) => orig(self);

    // PostSavePlayer
    public delegate void Orig_PostSavePlayer(T self);
    public virtual void Detour_PostSavePlayer(Orig_PostSavePlayer orig, T self) => orig(self);

    // CopyClientState
    public delegate void Orig_CopyClientState(T self, ModPlayer targetCopy);
    public virtual void Detour_CopyClientState(Orig_CopyClientState orig, T self, ModPlayer targetCopy) => orig(self, targetCopy);

    // SyncPlayer
    public delegate void Orig_SyncPlayer(T self, int toWho, int fromWho, bool newPlayer);
    public virtual void Detour_SyncPlayer(Orig_SyncPlayer orig, T self, int toWho, int fromWho, bool newPlayer) => orig(self, toWho, fromWho, newPlayer);

    // SendClientChanges
    public delegate void Orig_SendClientChanges(T self, ModPlayer clientPlayer);
    public virtual void Detour_SendClientChanges(Orig_SendClientChanges orig, T self, ModPlayer clientPlayer) => orig(self, clientPlayer);

    // UpdateBadLifeRegen
    public delegate void Orig_UpdateBadLifeRegen(T self);
    public virtual void Detour_UpdateBadLifeRegen(Orig_UpdateBadLifeRegen orig, T self) => orig(self);

    // UpdateLifeRegen
    public delegate void Orig_UpdateLifeRegen(T self);
    public virtual void Detour_UpdateLifeRegen(Orig_UpdateLifeRegen orig, T self) => orig(self);

    // NaturalLifeRegen
    public delegate void Orig_NaturalLifeRegen(T self, ref float regen);
    public virtual void Detour_NaturalLifeRegen(Orig_NaturalLifeRegen orig, T self, ref float regen) => orig(self, ref regen);

    // UpdateAutopause
    public delegate void Orig_UpdateAutopause(T self);
    public virtual void Detour_UpdateAutopause(Orig_UpdateAutopause orig, T self) => orig(self);

    // PreUpdate
    public delegate void Orig_PreUpdate(T self);
    public virtual void Detour_PreUpdate(Orig_PreUpdate orig, T self) => orig(self);

    // ProcessTriggers
    public delegate void Orig_ProcessTriggers(T self, TriggersSet triggersSet);
    public virtual void Detour_ProcessTriggers(Orig_ProcessTriggers orig, T self, TriggersSet triggersSet) => orig(self, triggersSet);

    // ArmorSetBonusActivated
    public delegate void Orig_ArmorSetBonusActivated(T self);
    public virtual void Detour_ArmorSetBonusActivated(Orig_ArmorSetBonusActivated orig, T self) => orig(self);

    // ArmorSetBonusHeld
    public delegate void Orig_ArmorSetBonusHeld(T self, int holdTime);
    public virtual void Detour_ArmorSetBonusHeld(Orig_ArmorSetBonusHeld orig, T self, int holdTime) => orig(self, holdTime);

    // SetControls
    public delegate void Orig_SetControls(T self);
    public virtual void Detour_SetControls(Orig_SetControls orig, T self) => orig(self);

    // PreUpdateBuffs
    public delegate void Orig_PreUpdateBuffs(T self);
    public virtual void Detour_PreUpdateBuffs(Orig_PreUpdateBuffs orig, T self) => orig(self);

    // PostUpdateBuffs
    public delegate void Orig_PostUpdateBuffs(T self);
    public virtual void Detour_PostUpdateBuffs(Orig_PostUpdateBuffs orig, T self) => orig(self);

    // UpdateEquips
    public delegate void Orig_UpdateEquips(T self);
    public virtual void Detour_UpdateEquips(Orig_UpdateEquips orig, T self) => orig(self);

    // PostUpdateEquips
    public delegate void Orig_PostUpdateEquips(T self);
    public virtual void Detour_PostUpdateEquips(Orig_PostUpdateEquips orig, T self) => orig(self);

    // UpdateVisibleAccessories
    public delegate void Orig_UpdateVisibleAccessories(T self);
    public virtual void Detour_UpdateVisibleAccessories(Orig_UpdateVisibleAccessories orig, T self) => orig(self);

    // UpdateVisibleVanityAccessories
    public delegate void Orig_UpdateVisibleVanityAccessories(T self);
    public virtual void Detour_UpdateVisibleVanityAccessories(Orig_UpdateVisibleVanityAccessories orig, T self) => orig(self);

    // UpdateDyes
    public delegate void Orig_UpdateDyes(T self);
    public virtual void Detour_UpdateDyes(Orig_UpdateDyes orig, T self) => orig(self);

    // PostUpdateMiscEffects
    public delegate void Orig_PostUpdateMiscEffects(T self);
    public virtual void Detour_PostUpdateMiscEffects(Orig_PostUpdateMiscEffects orig, T self) => orig(self);

    // PostUpdateRunSpeeds
    public delegate void Orig_PostUpdateRunSpeeds(T self);
    public virtual void Detour_PostUpdateRunSpeeds(Orig_PostUpdateRunSpeeds orig, T self) => orig(self);

    // PreUpdateMovement
    public delegate void Orig_PreUpdateMovement(T self);
    public virtual void Detour_PreUpdateMovement(Orig_PreUpdateMovement orig, T self) => orig(self);

    // PostUpdate
    public delegate void Orig_PostUpdate(T self);
    public virtual void Detour_PostUpdate(Orig_PostUpdate orig, T self) => orig(self);

    // ModifyExtraJumpDurationMultiplier
    public delegate void Orig_ModifyExtraJumpDurationMultiplier(T self, ExtraJump jump, ref float duration);
    public virtual void Detour_ModifyExtraJumpDurationMultiplier(Orig_ModifyExtraJumpDurationMultiplier orig, T self, ExtraJump jump, ref float duration) => orig(self, jump, ref duration);

    // CanStartExtraJump
    public delegate bool Orig_CanStartExtraJump(T self, ExtraJump jump);
    public virtual bool Detour_CanStartExtraJump(Orig_CanStartExtraJump orig, T self, ExtraJump jump) => orig(self, jump);

    // OnExtraJumpStarted
    public delegate void Orig_OnExtraJumpStarted(T self, ExtraJump jump, ref bool playSound);
    public virtual void Detour_OnExtraJumpStarted(Orig_OnExtraJumpStarted orig, T self, ExtraJump jump, ref bool playSound) => orig(self, jump, ref playSound);

    // OnExtraJumpEnded
    public delegate void Orig_OnExtraJumpEnded(T self, ExtraJump jump);
    public virtual void Detour_OnExtraJumpEnded(Orig_OnExtraJumpEnded orig, T self, ExtraJump jump) => orig(self, jump);

    // OnExtraJumpRefreshed
    public delegate void Orig_OnExtraJumpRefreshed(T self, ExtraJump jump);
    public virtual void Detour_OnExtraJumpRefreshed(Orig_OnExtraJumpRefreshed orig, T self, ExtraJump jump) => orig(self, jump);

    // ExtraJumpVisuals
    public delegate void Orig_ExtraJumpVisuals(T self, ExtraJump jump);
    public virtual void Detour_ExtraJumpVisuals(Orig_ExtraJumpVisuals orig, T self, ExtraJump jump) => orig(self, jump);

    // CanShowExtraJumpVisuals
    public delegate bool Orig_CanShowExtraJumpVisuals(T self, ExtraJump jump);
    public virtual bool Detour_CanShowExtraJumpVisuals(Orig_CanShowExtraJumpVisuals orig, T self, ExtraJump jump) => orig(self, jump);

    // OnExtraJumpCleared
    public delegate void Orig_OnExtraJumpCleared(T self, ExtraJump jump);
    public virtual void Detour_OnExtraJumpCleared(Orig_OnExtraJumpCleared orig, T self, ExtraJump jump) => orig(self, jump);

    // FrameEffects
    public delegate void Orig_FrameEffects(T self);
    public virtual void Detour_FrameEffects(Orig_FrameEffects orig, T self) => orig(self);

    // ImmuneTo
    public delegate bool Orig_ImmuneTo(T self, PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable);
    public virtual bool Detour_ImmuneTo(Orig_ImmuneTo orig, T self, PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => orig(self, damageSource, cooldownCounter, dodgeable);

    // FreeDodge
    public delegate bool Orig_FreeDodge(T self, Player.HurtInfo info);
    public virtual bool Detour_FreeDodge(Orig_FreeDodge orig, T self, Player.HurtInfo info) => orig(self, info);

    // ConsumableDodge
    public delegate bool Orig_ConsumableDodge(T self, Player.HurtInfo info);
    public virtual bool Detour_ConsumableDodge(Orig_ConsumableDodge orig, T self, Player.HurtInfo info) => orig(self, info);

    // ModifyHurt
    public delegate void Orig_ModifyHurt(T self, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHurt(Orig_ModifyHurt orig, T self, ref Player.HurtModifiers modifiers) => orig(self, ref modifiers);

    // OnHurt
    public delegate void Orig_OnHurt(T self, Player.HurtInfo info);
    public virtual void Detour_OnHurt(Orig_OnHurt orig, T self, Player.HurtInfo info) => orig(self, info);

    // PostHurt
    public delegate void Orig_PostHurt(T self, Player.HurtInfo info);
    public virtual void Detour_PostHurt(Orig_PostHurt orig, T self, Player.HurtInfo info) => orig(self, info);

    // PreKill
    public delegate bool Orig_PreKill(T self, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource);
    public virtual bool Detour_PreKill(Orig_PreKill orig, T self, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) => orig(self, damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);

    // Kill
    public delegate void Orig_Kill(T self, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource);
    public virtual void Detour_Kill(Orig_Kill orig, T self, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) => orig(self, damage, hitDirection, pvp, damageSource);

    // PreModifyLuck
    public delegate bool Orig_PreModifyLuck(T self, ref float luck);
    public virtual bool Detour_PreModifyLuck(Orig_PreModifyLuck orig, T self, ref float luck) => orig(self, ref luck);

    // ModifyLuck
    public delegate void Orig_ModifyLuck(T self, ref float luck);
    public virtual void Detour_ModifyLuck(Orig_ModifyLuck orig, T self, ref float luck) => orig(self, ref luck);

    // PreItemCheck
    public delegate bool Orig_PreItemCheck(T self);
    public virtual bool Detour_PreItemCheck(Orig_PreItemCheck orig, T self) => orig(self);

    // PostItemCheck
    public delegate void Orig_PostItemCheck(T self);
    public virtual void Detour_PostItemCheck(Orig_PostItemCheck orig, T self) => orig(self);

    // UseTimeMultiplier
    public delegate float Orig_UseTimeMultiplier(T self, Item item);
    public virtual float Detour_UseTimeMultiplier(Orig_UseTimeMultiplier orig, T self, Item item) => orig(self, item);

    // UseAnimationMultiplier
    public delegate float Orig_UseAnimationMultiplier(T self, Item item);
    public virtual float Detour_UseAnimationMultiplier(Orig_UseAnimationMultiplier orig, T self, Item item) => orig(self, item);

    // UseSpeedMultiplier
    public delegate float Orig_UseSpeedMultiplier(T self, Item item);
    public virtual float Detour_UseSpeedMultiplier(Orig_UseSpeedMultiplier orig, T self, Item item) => orig(self, item);

    // GetHealLife
    public delegate void Orig_GetHealLife(T self, Item item, bool quickHeal, ref int healValue);
    public virtual void Detour_GetHealLife(Orig_GetHealLife orig, T self, Item item, bool quickHeal, ref int healValue) => orig(self, item, quickHeal, ref healValue);

    // GetHealMana
    public delegate void Orig_GetHealMana(T self, Item item, bool quickHeal, ref int healValue);
    public virtual void Detour_GetHealMana(Orig_GetHealMana orig, T self, Item item, bool quickHeal, ref int healValue) => orig(self, item, quickHeal, ref healValue);

    // ModifyManaCost
    public delegate void Orig_ModifyManaCost(T self, Item item, ref float reduce, ref float mult);
    public virtual void Detour_ModifyManaCost(Orig_ModifyManaCost orig, T self, Item item, ref float reduce, ref float mult) => orig(self, item, ref reduce, ref mult);

    // OnMissingMana
    public delegate void Orig_OnMissingMana(T self, Item item, int neededMana);
    public virtual void Detour_OnMissingMana(Orig_OnMissingMana orig, T self, Item item, int neededMana) => orig(self, item, neededMana);

    // OnConsumeMana
    public delegate void Orig_OnConsumeMana(T self, Item item, int manaConsumed);
    public virtual void Detour_OnConsumeMana(Orig_OnConsumeMana orig, T self, Item item, int manaConsumed) => orig(self, item, manaConsumed);

    // ModifyWeaponDamage
    public delegate void Orig_ModifyWeaponDamage(T self, Item item, ref StatModifier damage);
    public virtual void Detour_ModifyWeaponDamage(Orig_ModifyWeaponDamage orig, T self, Item item, ref StatModifier damage) => orig(self, item, ref damage);

    // ModifyWeaponKnockback
    public delegate void Orig_ModifyWeaponKnockback(T self, Item item, ref StatModifier knockback);
    public virtual void Detour_ModifyWeaponKnockback(Orig_ModifyWeaponKnockback orig, T self, Item item, ref StatModifier knockback) => orig(self, item, ref knockback);

    // ModifyWeaponCrit
    public delegate void Orig_ModifyWeaponCrit(T self, Item item, ref float crit);
    public virtual void Detour_ModifyWeaponCrit(Orig_ModifyWeaponCrit orig, T self, Item item, ref float crit) => orig(self, item, ref crit);

    // CanConsumeAmmo
    public delegate bool Orig_CanConsumeAmmo(T self, Item weapon, Item ammo);
    public virtual bool Detour_CanConsumeAmmo(Orig_CanConsumeAmmo orig, T self, Item weapon, Item ammo) => orig(self, weapon, ammo);

    // OnConsumeAmmo
    public delegate void Orig_OnConsumeAmmo(T self, Item weapon, Item ammo);
    public virtual void Detour_OnConsumeAmmo(Orig_OnConsumeAmmo orig, T self, Item weapon, Item ammo) => orig(self, weapon, ammo);

    // CanShoot
    public delegate bool Orig_CanShoot(T self, Item item);
    public virtual bool Detour_CanShoot(Orig_CanShoot orig, T self, Item item) => orig(self, item);

    // ModifyShootStats
    public delegate void Orig_ModifyShootStats(T self, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);
    public virtual void Detour_ModifyShootStats(Orig_ModifyShootStats orig, T self, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => orig(self, item, ref position, ref velocity, ref type, ref damage, ref knockback);

    // Shoot
    public delegate bool Orig_Shoot(T self, Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback);
    public virtual bool Detour_Shoot(Orig_Shoot orig, T self, Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => orig(self, item, source, position, velocity, type, damage, knockback);

    // MeleeEffects
    public delegate void Orig_MeleeEffects(T self, Item item, Rectangle hitbox);
    public virtual void Detour_MeleeEffects(Orig_MeleeEffects orig, T self, Item item, Rectangle hitbox) => orig(self, item, hitbox);

    // EmitEnchantmentVisualsAt
    public delegate void Orig_EmitEnchantmentVisualsAt(T self, Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight);
    public virtual void Detour_EmitEnchantmentVisualsAt(Orig_EmitEnchantmentVisualsAt orig, T self, Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) => orig(self, projectile, boxPosition, boxWidth, boxHeight);

    // CanCatchNPC
    public delegate bool? Orig_CanCatchNPC(T self, NPC target, Item item);
    public virtual bool? Detour_CanCatchNPC(Orig_CanCatchNPC orig, T self, NPC target, Item item) => orig(self, target, item);

    // OnCatchNPC
    public delegate void Orig_OnCatchNPC(T self, NPC npc, Item item, bool failed);
    public virtual void Detour_OnCatchNPC(Orig_OnCatchNPC orig, T self, NPC npc, Item item, bool failed) => orig(self, npc, item, failed);

    // ModifyItemScale
    public delegate void Orig_ModifyItemScale(T self, Item item, ref float scale);
    public virtual void Detour_ModifyItemScale(Orig_ModifyItemScale orig, T self, Item item, ref float scale) => orig(self, item, ref scale);

    // OnHitAnything
    public delegate void Orig_OnHitAnything(T self, float x, float y, Entity victim);
    public virtual void Detour_OnHitAnything(Orig_OnHitAnything orig, T self, float x, float y, Entity victim) => orig(self, x, y, victim);

    // CanHitNPC
    public delegate bool Orig_CanHitNPC(T self, NPC target);
    public virtual bool Detour_CanHitNPC(Orig_CanHitNPC orig, T self, NPC target) => orig(self, target);

    // CanMeleeAttackCollideWithNPC
    public delegate bool? Orig_CanMeleeAttackCollideWithNPC(T self, Item item, Rectangle meleeAttackHitbox, NPC target);
    public virtual bool? Detour_CanMeleeAttackCollideWithNPC(Orig_CanMeleeAttackCollideWithNPC orig, T self, Item item, Rectangle meleeAttackHitbox, NPC target) => orig(self, item, meleeAttackHitbox, target);

    // ModifyHitNPC
    public delegate void Orig_ModifyHitNPC(T self, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, T self, NPC target, ref NPC.HitModifiers modifiers) => orig(self, target, ref modifiers);

    // OnHitNPC
    public delegate void Orig_OnHitNPC(T self, NPC target, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitNPC(Orig_OnHitNPC orig, T self, NPC target, NPC.HitInfo hit, int damageDone) => orig(self, target, hit, damageDone);

    // CanHitNPCWithItem
    public delegate bool? Orig_CanHitNPCWithItem(T self, Item item, NPC target);
    public virtual bool? Detour_CanHitNPCWithItem(Orig_CanHitNPCWithItem orig, T self, Item item, NPC target) => orig(self, item, target);

    // ModifyHitNPCWithItem
    public delegate void Orig_ModifyHitNPCWithItem(T self, Item item, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPCWithItem(Orig_ModifyHitNPCWithItem orig, T self, Item item, NPC target, ref NPC.HitModifiers modifiers) => orig(self, item, target, ref modifiers);

    // OnHitNPCWithItem
    public delegate void Orig_OnHitNPCWithItem(T self, Item item, NPC target, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitNPCWithItem(Orig_OnHitNPCWithItem orig, T self, Item item, NPC target, NPC.HitInfo hit, int damageDone) => orig(self, item, target, hit, damageDone);

    // CanHitNPCWithProj
    public delegate bool? Orig_CanHitNPCWithProj(T self, Projectile proj, NPC target);
    public virtual bool? Detour_CanHitNPCWithProj(Orig_CanHitNPCWithProj orig, T self, Projectile proj, NPC target) => orig(self, proj, target);

    // ModifyHitNPCWithProj
    public delegate void Orig_ModifyHitNPCWithProj(T self, Projectile proj, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPCWithProj(Orig_ModifyHitNPCWithProj orig, T self, Projectile proj, NPC target, ref NPC.HitModifiers modifiers) => orig(self, proj, target, ref modifiers);

    // OnHitNPCWithProj
    public delegate void Orig_OnHitNPCWithProj(T self, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitNPCWithProj(Orig_OnHitNPCWithProj orig, T self, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) => orig(self, proj, target, hit, damageDone);

    // CanHitPvp
    public delegate bool Orig_CanHitPvp(T self, Item item, Player target);
    public virtual bool Detour_CanHitPvp(Orig_CanHitPvp orig, T self, Item item, Player target) => orig(self, item, target);

    // CanHitPvpWithProj
    public delegate bool Orig_CanHitPvpWithProj(T self, Projectile proj, Player target);
    public virtual bool Detour_CanHitPvpWithProj(Orig_CanHitPvpWithProj orig, T self, Projectile proj, Player target) => orig(self, proj, target);

    // CanBeHitByNPC
    public delegate bool Orig_CanBeHitByNPC(T self, NPC npc, ref int cooldownSlot);
    public virtual bool Detour_CanBeHitByNPC(Orig_CanBeHitByNPC orig, T self, NPC npc, ref int cooldownSlot) => orig(self, npc, ref cooldownSlot);

    // ModifyHitByNPC
    public delegate void Orig_ModifyHitByNPC(T self, NPC npc, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHitByNPC(Orig_ModifyHitByNPC orig, T self, NPC npc, ref Player.HurtModifiers modifiers) => orig(self, npc, ref modifiers);

    // OnHitByNPC
    public delegate void Orig_OnHitByNPC(T self, NPC npc, Player.HurtInfo hurtInfo);
    public virtual void Detour_OnHitByNPC(Orig_OnHitByNPC orig, T self, NPC npc, Player.HurtInfo hurtInfo) => orig(self, npc, hurtInfo);

    // CanBeHitByProjectile
    public delegate bool Orig_CanBeHitByProjectile(T self, Projectile proj);
    public virtual bool Detour_CanBeHitByProjectile(Orig_CanBeHitByProjectile orig, T self, Projectile proj) => orig(self, proj);

    // ModifyHitByProjectile
    public delegate void Orig_ModifyHitByProjectile(T self, Projectile proj, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHitByProjectile(Orig_ModifyHitByProjectile orig, T self, Projectile proj, ref Player.HurtModifiers modifiers) => orig(self, proj, ref modifiers);

    // OnHitByProjectile
    public delegate void Orig_OnHitByProjectile(T self, Projectile proj, Player.HurtInfo hurtInfo);
    public virtual void Detour_OnHitByProjectile(Orig_OnHitByProjectile orig, T self, Projectile proj, Player.HurtInfo hurtInfo) => orig(self, proj, hurtInfo);

    // ModifyFishingAttempt
    public delegate void Orig_ModifyFishingAttempt(T self, ref FishingAttempt attempt);
    public virtual void Detour_ModifyFishingAttempt(Orig_ModifyFishingAttempt orig, T self, ref FishingAttempt attempt) => orig(self, ref attempt);

    // CatchFish
    public delegate void Orig_CatchFish(T self, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition);
    public virtual void Detour_CatchFish(Orig_CatchFish orig, T self, FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) => orig(self, attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);

    // ModifyCaughtFish
    public delegate void Orig_ModifyCaughtFish(T self, Item fish);
    public virtual void Detour_ModifyCaughtFish(Orig_ModifyCaughtFish orig, T self, Item fish) => orig(self, fish);

    // CanConsumeBait
    public delegate bool? Orig_CanConsumeBait(T self, Item bait);
    public virtual bool? Detour_CanConsumeBait(Orig_CanConsumeBait orig, T self, Item bait) => orig(self, bait);

    // GetFishingLevel
    public delegate void Orig_GetFishingLevel(T self, Item fishingRod, Item bait, ref float fishingLevel);
    public virtual void Detour_GetFishingLevel(Orig_GetFishingLevel orig, T self, Item fishingRod, Item bait, ref float fishingLevel) => orig(self, fishingRod, bait, ref fishingLevel);

    // AnglerQuestReward
    public delegate void Orig_AnglerQuestReward(T self, float rareMultiplier, List<Item> rewardItems);
    public virtual void Detour_AnglerQuestReward(Orig_AnglerQuestReward orig, T self, float rareMultiplier, List<Item> rewardItems) => orig(self, rareMultiplier, rewardItems);

    // GetDyeTraderReward
    public delegate void Orig_GetDyeTraderReward(T self, List<int> rewardPool);
    public virtual void Detour_GetDyeTraderReward(Orig_GetDyeTraderReward orig, T self, List<int> rewardPool) => orig(self, rewardPool);

    // DrawEffects
    public delegate void Orig_DrawEffects(T self, PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright);
    public virtual void Detour_DrawEffects(Orig_DrawEffects orig, T self, PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) => orig(self, drawInfo, ref r, ref g, ref b, ref a, ref fullBright);

    // ModifyDrawInfo
    public delegate void Orig_ModifyDrawInfo(T self, ref PlayerDrawSet drawInfo);
    public virtual void Detour_ModifyDrawInfo(Orig_ModifyDrawInfo orig, T self, ref PlayerDrawSet drawInfo) => orig(self, ref drawInfo);

    // ModifyDrawLayerOrdering
    public delegate void Orig_ModifyDrawLayerOrdering(T self, IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions);
    public virtual void Detour_ModifyDrawLayerOrdering(Orig_ModifyDrawLayerOrdering orig, T self, IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) => orig(self, positions);

    // HideDrawLayers
    public delegate void Orig_HideDrawLayers(T self, PlayerDrawSet drawInfo);
    public virtual void Detour_HideDrawLayers(Orig_HideDrawLayers orig, T self, PlayerDrawSet drawInfo) => orig(self, drawInfo);

    // ModifyScreenPosition
    public delegate void Orig_ModifyScreenPosition(T self);
    public virtual void Detour_ModifyScreenPosition(Orig_ModifyScreenPosition orig, T self) => orig(self);

    // ModifyZoom
    public delegate void Orig_ModifyZoom(T self, ref float zoom);
    public virtual void Detour_ModifyZoom(Orig_ModifyZoom orig, T self, ref float zoom) => orig(self, ref zoom);

    // PlayerConnect
    public delegate void Orig_PlayerConnect(T self);
    public virtual void Detour_PlayerConnect(Orig_PlayerConnect orig, T self) => orig(self);

    // PlayerDisconnect
    public delegate void Orig_PlayerDisconnect(T self);
    public virtual void Detour_PlayerDisconnect(Orig_PlayerDisconnect orig, T self) => orig(self);

    // OnEnterWorld
    public delegate void Orig_OnEnterWorld(T self);
    public virtual void Detour_OnEnterWorld(Orig_OnEnterWorld orig, T self) => orig(self);

    // OnRespawn
    public delegate void Orig_OnRespawn(T self);
    public virtual void Detour_OnRespawn(Orig_OnRespawn orig, T self) => orig(self);

    // ShiftClickSlot
    public delegate bool Orig_ShiftClickSlot(T self, Item[] inventory, int context, int slot);
    public virtual bool Detour_ShiftClickSlot(Orig_ShiftClickSlot orig, T self, Item[] inventory, int context, int slot) => orig(self, inventory, context, slot);

    // HoverSlot
    public delegate bool Orig_HoverSlot(T self, Item[] inventory, int context, int slot);
    public virtual bool Detour_HoverSlot(Orig_HoverSlot orig, T self, Item[] inventory, int context, int slot) => orig(self, inventory, context, slot);

    // PostSellItem
    public delegate void Orig_PostSellItem(T self, NPC vendor, Item[] shopInventory, Item item);
    public virtual void Detour_PostSellItem(Orig_PostSellItem orig, T self, NPC vendor, Item[] shopInventory, Item item) => orig(self, vendor, shopInventory, item);

    // CanSellItem
    public delegate bool Orig_CanSellItem(T self, NPC vendor, Item[] shopInventory, Item item);
    public virtual bool Detour_CanSellItem(Orig_CanSellItem orig, T self, NPC vendor, Item[] shopInventory, Item item) => orig(self, vendor, shopInventory, item);

    // PostBuyItem
    public delegate void Orig_PostBuyItem(T self, NPC vendor, Item[] shopInventory, Item item);
    public virtual void Detour_PostBuyItem(Orig_PostBuyItem orig, T self, NPC vendor, Item[] shopInventory, Item item) => orig(self, vendor, shopInventory, item);

    // CanBuyItem
    public delegate bool Orig_CanBuyItem(T self, NPC vendor, Item[] shopInventory, Item item);
    public virtual bool Detour_CanBuyItem(Orig_CanBuyItem orig, T self, NPC vendor, Item[] shopInventory, Item item) => orig(self, vendor, shopInventory, item);

    // CanUseItem
    public delegate bool Orig_CanUseItem(T self, Item item);
    public virtual bool Detour_CanUseItem(Orig_CanUseItem orig, T self, Item item) => orig(self, item);

    // CanAutoReuseItem
    public delegate bool? Orig_CanAutoReuseItem(T self, Item item);
    public virtual bool? Detour_CanAutoReuseItem(Orig_CanAutoReuseItem orig, T self, Item item) => orig(self, item);

    // ModifyNurseHeal
    public delegate bool Orig_ModifyNurseHeal(T self, NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText);
    public virtual bool Detour_ModifyNurseHeal(Orig_ModifyNurseHeal orig, T self, NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText) => orig(self, nurse, ref health, ref removeDebuffs, ref chatText);

    // ModifyNursePrice
    public delegate void Orig_ModifyNursePrice(T self, NPC nurse, int health, bool removeDebuffs, ref int price);
    public virtual void Detour_ModifyNursePrice(Orig_ModifyNursePrice orig, T self, NPC nurse, int health, bool removeDebuffs, ref int price) => orig(self, nurse, health, removeDebuffs, ref price);

    // PostNurseHeal
    public delegate void Orig_PostNurseHeal(T self, NPC nurse, int health, bool removeDebuffs, int price);
    public virtual void Detour_PostNurseHeal(Orig_PostNurseHeal orig, T self, NPC nurse, int health, bool removeDebuffs, int price) => orig(self, nurse, health, removeDebuffs, price);

    // AddStartingItems
    public delegate IEnumerable<Item> Orig_AddStartingItems(T self, bool mediumCoreDeath);
    public virtual IEnumerable<Item> Detour_AddStartingItems(Orig_AddStartingItems orig, T self, bool mediumCoreDeath) => orig(self, mediumCoreDeath);

    // ModifyStartingInventory
    public delegate void Orig_ModifyStartingInventory(T self, IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath);
    public virtual void Detour_ModifyStartingInventory(Orig_ModifyStartingInventory orig, T self, IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath) => orig(self, itemsByMod, mediumCoreDeath);

    // AddMaterialsForCrafting
    public delegate IEnumerable<Item> Orig_AddMaterialsForCrafting(T self, out ModPlayer.ItemConsumedCallback itemConsumedCallback);
    public virtual IEnumerable<Item> Detour_AddMaterialsForCrafting(Orig_AddMaterialsForCrafting orig, T self, out ModPlayer.ItemConsumedCallback itemConsumedCallback) => orig(self, out itemConsumedCallback);

    // OnPickup
    public delegate bool Orig_OnPickup(T self, Item item);
    public virtual bool Detour_OnPickup(Orig_OnPickup orig, T self, Item item) => orig(self, item);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Initialize);
        TryApplyDetour(Detour_ResetEffects);
        TryApplyDetour(Detour_ResetInfoAccessories);
        TryApplyDetour(Detour_RefreshInfoAccessoriesFromTeamPlayers);
        TryApplyDetour(Detour_ModifyMaxStats);
        TryApplyDetour(Detour_UpdateDead);
        TryApplyDetour(Detour_PreSaveCustomData);
        TryApplyDetour(Detour_SaveData);
        TryApplyDetour(Detour_LoadData);
        TryApplyDetour(Detour_PreSavePlayer);
        TryApplyDetour(Detour_PostSavePlayer);
        TryApplyDetour(Detour_CopyClientState);
        TryApplyDetour(Detour_SyncPlayer);
        TryApplyDetour(Detour_SendClientChanges);
        TryApplyDetour(Detour_UpdateBadLifeRegen);
        TryApplyDetour(Detour_UpdateLifeRegen);
        TryApplyDetour(Detour_NaturalLifeRegen);
        TryApplyDetour(Detour_UpdateAutopause);
        TryApplyDetour(Detour_PreUpdate);
        TryApplyDetour(Detour_ProcessTriggers);
        TryApplyDetour(Detour_ArmorSetBonusActivated);
        TryApplyDetour(Detour_ArmorSetBonusHeld);
        TryApplyDetour(Detour_SetControls);
        TryApplyDetour(Detour_PreUpdateBuffs);
        TryApplyDetour(Detour_PostUpdateBuffs);
        TryApplyDetour(Detour_UpdateEquips);
        TryApplyDetour(Detour_PostUpdateEquips);
        TryApplyDetour(Detour_UpdateVisibleAccessories);
        TryApplyDetour(Detour_UpdateVisibleVanityAccessories);
        TryApplyDetour(Detour_UpdateDyes);
        TryApplyDetour(Detour_PostUpdateMiscEffects);
        TryApplyDetour(Detour_PostUpdateRunSpeeds);
        TryApplyDetour(Detour_PreUpdateMovement);
        TryApplyDetour(Detour_PostUpdate);
        TryApplyDetour(Detour_ModifyExtraJumpDurationMultiplier);
        TryApplyDetour(Detour_CanStartExtraJump);
        TryApplyDetour(Detour_OnExtraJumpStarted);
        TryApplyDetour(Detour_OnExtraJumpEnded);
        TryApplyDetour(Detour_OnExtraJumpRefreshed);
        TryApplyDetour(Detour_ExtraJumpVisuals);
        TryApplyDetour(Detour_CanShowExtraJumpVisuals);
        TryApplyDetour(Detour_OnExtraJumpCleared);
        TryApplyDetour(Detour_FrameEffects);
        TryApplyDetour(Detour_ImmuneTo);
        TryApplyDetour(Detour_FreeDodge);
        TryApplyDetour(Detour_ConsumableDodge);
        TryApplyDetour(Detour_ModifyHurt);
        TryApplyDetour(Detour_OnHurt);
        TryApplyDetour(Detour_PostHurt);
        TryApplyDetour(Detour_PreKill);
        TryApplyDetour(Detour_Kill);
        TryApplyDetour(Detour_PreModifyLuck);
        TryApplyDetour(Detour_ModifyLuck);
        TryApplyDetour(Detour_PreItemCheck);
        TryApplyDetour(Detour_PostItemCheck);
        TryApplyDetour(Detour_UseTimeMultiplier);
        TryApplyDetour(Detour_UseAnimationMultiplier);
        TryApplyDetour(Detour_UseSpeedMultiplier);
        TryApplyDetour(Detour_GetHealLife);
        TryApplyDetour(Detour_GetHealMana);
        TryApplyDetour(Detour_ModifyManaCost);
        TryApplyDetour(Detour_OnMissingMana);
        TryApplyDetour(Detour_OnConsumeMana);
        TryApplyDetour(Detour_ModifyWeaponDamage);
        TryApplyDetour(Detour_ModifyWeaponKnockback);
        TryApplyDetour(Detour_ModifyWeaponCrit);
        TryApplyDetour(Detour_CanConsumeAmmo);
        TryApplyDetour(Detour_OnConsumeAmmo);
        TryApplyDetour(Detour_CanShoot);
        TryApplyDetour(Detour_ModifyShootStats);
        TryApplyDetour(Detour_Shoot);
        TryApplyDetour(Detour_MeleeEffects);
        TryApplyDetour(Detour_EmitEnchantmentVisualsAt);
        TryApplyDetour(Detour_CanCatchNPC);
        TryApplyDetour(Detour_OnCatchNPC);
        TryApplyDetour(Detour_ModifyItemScale);
        TryApplyDetour(Detour_OnHitAnything);
        TryApplyDetour(Detour_CanHitNPC);
        TryApplyDetour(Detour_CanMeleeAttackCollideWithNPC);
        TryApplyDetour(Detour_ModifyHitNPC);
        TryApplyDetour(Detour_OnHitNPC);
        TryApplyDetour(Detour_CanHitNPCWithItem);
        TryApplyDetour(Detour_ModifyHitNPCWithItem);
        TryApplyDetour(Detour_OnHitNPCWithItem);
        TryApplyDetour(Detour_CanHitNPCWithProj);
        TryApplyDetour(Detour_ModifyHitNPCWithProj);
        TryApplyDetour(Detour_OnHitNPCWithProj);
        TryApplyDetour(Detour_CanHitPvp);
        TryApplyDetour(Detour_CanHitPvpWithProj);
        TryApplyDetour(Detour_CanBeHitByNPC);
        TryApplyDetour(Detour_ModifyHitByNPC);
        TryApplyDetour(Detour_OnHitByNPC);
        TryApplyDetour(Detour_CanBeHitByProjectile);
        TryApplyDetour(Detour_ModifyHitByProjectile);
        TryApplyDetour(Detour_OnHitByProjectile);
        TryApplyDetour(Detour_ModifyFishingAttempt);
        TryApplyDetour(Detour_CatchFish);
        TryApplyDetour(Detour_ModifyCaughtFish);
        TryApplyDetour(Detour_CanConsumeBait);
        TryApplyDetour(Detour_GetFishingLevel);
        TryApplyDetour(Detour_AnglerQuestReward);
        TryApplyDetour(Detour_GetDyeTraderReward);
        TryApplyDetour(Detour_DrawEffects);
        TryApplyDetour(Detour_ModifyDrawInfo);
        TryApplyDetour(Detour_ModifyDrawLayerOrdering);
        TryApplyDetour(Detour_HideDrawLayers);
        TryApplyDetour(Detour_ModifyScreenPosition);
        TryApplyDetour(Detour_ModifyZoom);
        TryApplyDetour(Detour_PlayerConnect);
        TryApplyDetour(Detour_PlayerDisconnect);
        TryApplyDetour(Detour_OnEnterWorld);
        TryApplyDetour(Detour_OnRespawn);
        TryApplyDetour(Detour_ShiftClickSlot);
        TryApplyDetour(Detour_HoverSlot);
        TryApplyDetour(Detour_PostSellItem);
        TryApplyDetour(Detour_CanSellItem);
        TryApplyDetour(Detour_PostBuyItem);
        TryApplyDetour(Detour_CanBuyItem);
        TryApplyDetour(Detour_CanUseItem);
        TryApplyDetour(Detour_CanAutoReuseItem);
        TryApplyDetour(Detour_ModifyNurseHeal);
        TryApplyDetour(Detour_ModifyNursePrice);
        TryApplyDetour(Detour_PostNurseHeal);
        TryApplyDetour(Detour_AddStartingItems);
        TryApplyDetour(Detour_ModifyStartingInventory);
        TryApplyDetour(Detour_AddMaterialsForCrafting);
        TryApplyDetour(Detour_OnPickup);
    }
}

public abstract class ModPrefixDetour<T> : ModTypeDetour<T> where T : ModPrefix
{
    // RollChance
    public delegate float Orig_RollChance(T self, Item item);
    public virtual float Detour_RollChance(Orig_RollChance orig, T self, Item item) => orig(self, item);

    // CanRoll
    public delegate bool Orig_CanRoll(T self, Item item);
    public virtual bool Detour_CanRoll(Orig_CanRoll orig, T self, Item item) => orig(self, item);

    // SetStats
    public delegate void Orig_SetStats(T self, ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus);
    public virtual void Detour_SetStats(Orig_SetStats orig, T self, ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) => orig(self, ref damageMult, ref knockbackMult, ref useTimeMult, ref scaleMult, ref shootSpeedMult, ref manaMult, ref critBonus);

    // AllStatChangesHaveEffectOn
    public delegate bool Orig_AllStatChangesHaveEffectOn(T self, Item item);
    public virtual bool Detour_AllStatChangesHaveEffectOn(Orig_AllStatChangesHaveEffectOn orig, T self, Item item) => orig(self, item);

    // Apply
    public delegate void Orig_Apply(T self, Item item);
    public virtual void Detour_Apply(Orig_Apply orig, T self, Item item) => orig(self, item);

    // ModifyValue
    public delegate void Orig_ModifyValue(T self, ref float valueMult);
    public virtual void Detour_ModifyValue(Orig_ModifyValue orig, T self, ref float valueMult) => orig(self, ref valueMult);

    // ApplyAccessoryEffects
    public delegate void Orig_ApplyAccessoryEffects(T self, Player player);
    public virtual void Detour_ApplyAccessoryEffects(Orig_ApplyAccessoryEffects orig, T self, Player player) => orig(self, player);

    // GetTooltipLines
    public delegate IEnumerable<TooltipLine> Orig_GetTooltipLines(T self, Item item);
    public virtual IEnumerable<TooltipLine> Detour_GetTooltipLines(Orig_GetTooltipLines orig, T self, Item item) => orig(self, item);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_RollChance);
        TryApplyDetour(Detour_CanRoll);
        TryApplyDetour(Detour_SetStats);
        TryApplyDetour(Detour_AllStatChangesHaveEffectOn);
        TryApplyDetour(Detour_Apply);
        TryApplyDetour(Detour_ModifyValue);
        TryApplyDetour(Detour_ApplyAccessoryEffects);
        TryApplyDetour(Detour_GetTooltipLines);
    }
}

public abstract class ModProjectileDetour<T> : ModTypeDetour<T> where T : ModProjectile
{
    // SetDefaults
    public delegate void Orig_SetDefaults(T self);
    public virtual void Detour_SetDefaults(Orig_SetDefaults orig, T self) => orig(self);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self, IEntitySource source);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, IEntitySource source) => orig(self, source);

    // AutoStaticDefaults
    public delegate void Orig_AutoStaticDefaults(T self);
    public virtual void Detour_AutoStaticDefaults(Orig_AutoStaticDefaults orig, T self) => orig(self);

    // PreAI
    public delegate bool Orig_PreAI(T self);
    public virtual bool Detour_PreAI(Orig_PreAI orig, T self) => orig(self);

    // AI
    public delegate void Orig_AI(T self);
    public virtual void Detour_AI(Orig_AI orig, T self) => orig(self);

    // PostAI
    public delegate void Orig_PostAI(T self);
    public virtual void Detour_PostAI(Orig_PostAI orig, T self) => orig(self);

    // SendExtraAI
    public delegate void Orig_SendExtraAI(T self, BinaryWriter writer);
    public virtual void Detour_SendExtraAI(Orig_SendExtraAI orig, T self, BinaryWriter writer) => orig(self, writer);

    // ReceiveExtraAI
    public delegate void Orig_ReceiveExtraAI(T self, BinaryReader reader);
    public virtual void Detour_ReceiveExtraAI(Orig_ReceiveExtraAI orig, T self, BinaryReader reader) => orig(self, reader);

    // ShouldUpdatePosition
    public delegate bool Orig_ShouldUpdatePosition(T self);
    public virtual bool Detour_ShouldUpdatePosition(Orig_ShouldUpdatePosition orig, T self) => orig(self);

    // TileCollideStyle
    public delegate bool Orig_TileCollideStyle(T self, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac);
    public virtual bool Detour_TileCollideStyle(Orig_TileCollideStyle orig, T self, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => orig(self, ref width, ref height, ref fallThrough, ref hitboxCenterFrac);

    // OnTileCollide
    public delegate bool Orig_OnTileCollide(T self, Vector2 oldVelocity);
    public virtual bool Detour_OnTileCollide(Orig_OnTileCollide orig, T self, Vector2 oldVelocity) => orig(self, oldVelocity);

    // CanCutTiles
    public delegate bool? Orig_CanCutTiles(T self);
    public virtual bool? Detour_CanCutTiles(Orig_CanCutTiles orig, T self) => orig(self);

    // CutTiles
    public delegate void Orig_CutTiles(T self);
    public virtual void Detour_CutTiles(Orig_CutTiles orig, T self) => orig(self);

    // PreKill
    public delegate bool Orig_PreKill(T self, int timeLeft);
    public virtual bool Detour_PreKill(Orig_PreKill orig, T self, int timeLeft) => orig(self, timeLeft);

    // OnKill
    public delegate void Orig_OnKill(T self, int timeLeft);
    public virtual void Detour_OnKill(Orig_OnKill orig, T self, int timeLeft) => orig(self, timeLeft);

    // CanDamage
    public delegate bool? Orig_CanDamage(T self);
    public virtual bool? Detour_CanDamage(Orig_CanDamage orig, T self) => orig(self);

    // MinionContactDamage
    public delegate bool Orig_MinionContactDamage(T self);
    public virtual bool Detour_MinionContactDamage(Orig_MinionContactDamage orig, T self) => orig(self);

    // ModifyDamageHitbox
    public delegate void Orig_ModifyDamageHitbox(T self, ref Rectangle hitbox);
    public virtual void Detour_ModifyDamageHitbox(Orig_ModifyDamageHitbox orig, T self, ref Rectangle hitbox) => orig(self, ref hitbox);

    // CanHitNPC
    public delegate bool? Orig_CanHitNPC(T self, NPC target);
    public virtual bool? Detour_CanHitNPC(Orig_CanHitNPC orig, T self, NPC target) => orig(self, target);

    // ModifyHitNPC
    public delegate void Orig_ModifyHitNPC(T self, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, T self, NPC target, ref NPC.HitModifiers modifiers) => orig(self, target, ref modifiers);

    // OnHitNPC
    public delegate void Orig_OnHitNPC(T self, NPC target, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitNPC(Orig_OnHitNPC orig, T self, NPC target, NPC.HitInfo hit, int damageDone) => orig(self, target, hit, damageDone);

    // CanHitPvp
    public delegate bool Orig_CanHitPvp(T self, Player target);
    public virtual bool Detour_CanHitPvp(Orig_CanHitPvp orig, T self, Player target) => orig(self, target);

    // CanHitPlayer
    public delegate bool Orig_CanHitPlayer(T self, Player target);
    public virtual bool Detour_CanHitPlayer(Orig_CanHitPlayer orig, T self, Player target) => orig(self, target);

    // ModifyHitPlayer
    public delegate void Orig_ModifyHitPlayer(T self, Player target, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHitPlayer(Orig_ModifyHitPlayer orig, T self, Player target, ref Player.HurtModifiers modifiers) => orig(self, target, ref modifiers);

    // OnHitPlayer
    public delegate void Orig_OnHitPlayer(T self, Player target, Player.HurtInfo info);
    public virtual void Detour_OnHitPlayer(Orig_OnHitPlayer orig, T self, Player target, Player.HurtInfo info) => orig(self, target, info);

    // Colliding
    public delegate bool? Orig_Colliding(T self, Rectangle projHitbox, Rectangle targetHitbox);
    public virtual bool? Detour_Colliding(Orig_Colliding orig, T self, Rectangle projHitbox, Rectangle targetHitbox) => orig(self, projHitbox, targetHitbox);

    // GetAlpha
    public delegate Color? Orig_GetAlpha(T self, Color lightColor);
    public virtual Color? Detour_GetAlpha(Orig_GetAlpha orig, T self, Color lightColor) => orig(self, lightColor);

    // PreDrawExtras
    public delegate bool Orig_PreDrawExtras(T self);
    public virtual bool Detour_PreDrawExtras(Orig_PreDrawExtras orig, T self) => orig(self);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, ref Color lightColor);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, ref Color lightColor) => orig(self, ref lightColor);

    // PostDraw
    public delegate void Orig_PostDraw(T self, Color lightColor);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, Color lightColor) => orig(self, lightColor);

    // CanUseGrapple
    public delegate bool? Orig_CanUseGrapple(T self, Player player);
    public virtual bool? Detour_CanUseGrapple(Orig_CanUseGrapple orig, T self, Player player) => orig(self, player);

    // UseGrapple
    public delegate void Orig_UseGrapple(T self, Player player, ref int type);
    public virtual void Detour_UseGrapple(Orig_UseGrapple orig, T self, Player player, ref int type) => orig(self, player, ref type);

    // GrappleRange
    public delegate float Orig_GrappleRange(T self, Player player);
    public virtual float Detour_GrappleRange(Orig_GrappleRange orig, T self, Player player) => orig(self, player);

    // NumGrappleHooks
    public delegate void Orig_NumGrappleHooks(T self, Player player, ref int numHooks);
    public virtual void Detour_NumGrappleHooks(Orig_NumGrappleHooks orig, T self, Player player, ref int numHooks) => orig(self, player, ref numHooks);

    // GrappleRetreatSpeed
    public delegate void Orig_GrappleRetreatSpeed(T self, Player player, ref float speed);
    public virtual void Detour_GrappleRetreatSpeed(Orig_GrappleRetreatSpeed orig, T self, Player player, ref float speed) => orig(self, player, ref speed);

    // GrapplePullSpeed
    public delegate void Orig_GrapplePullSpeed(T self, Player player, ref float speed);
    public virtual void Detour_GrapplePullSpeed(Orig_GrapplePullSpeed orig, T self, Player player, ref float speed) => orig(self, player, ref speed);

    // GrappleTargetPoint
    public delegate void Orig_GrappleTargetPoint(T self, Player player, ref float grappleX, ref float grappleY);
    public virtual void Detour_GrappleTargetPoint(Orig_GrappleTargetPoint orig, T self, Player player, ref float grappleX, ref float grappleY) => orig(self, player, ref grappleX, ref grappleY);

    // GrappleCanLatchOnTo
    public delegate bool? Orig_GrappleCanLatchOnTo(T self, Player player, int x, int y);
    public virtual bool? Detour_GrappleCanLatchOnTo(Orig_GrappleCanLatchOnTo orig, T self, Player player, int x, int y) => orig(self, player, x, y);

    // DrawBehind
    public delegate void Orig_DrawBehind(T self, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI);
    public virtual void Detour_DrawBehind(Orig_DrawBehind orig, T self, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => orig(self, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);

    // PrepareBombToBlow
    public delegate void Orig_PrepareBombToBlow(T self);
    public virtual void Detour_PrepareBombToBlow(Orig_PrepareBombToBlow orig, T self) => orig(self);

    // EmitEnchantmentVisualsAt
    public delegate void Orig_EmitEnchantmentVisualsAt(T self, Vector2 boxPosition, int boxWidth, int boxHeight);
    public virtual void Detour_EmitEnchantmentVisualsAt(Orig_EmitEnchantmentVisualsAt orig, T self, Vector2 boxPosition, int boxWidth, int boxHeight) => orig(self, boxPosition, boxWidth, boxHeight);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_SetDefaults);
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_AutoStaticDefaults);
        TryApplyDetour(Detour_PreAI);
        TryApplyDetour(Detour_AI);
        TryApplyDetour(Detour_PostAI);
        TryApplyDetour(Detour_SendExtraAI);
        TryApplyDetour(Detour_ReceiveExtraAI);
        TryApplyDetour(Detour_ShouldUpdatePosition);
        TryApplyDetour(Detour_TileCollideStyle);
        TryApplyDetour(Detour_OnTileCollide);
        TryApplyDetour(Detour_CanCutTiles);
        TryApplyDetour(Detour_CutTiles);
        TryApplyDetour(Detour_PreKill);
        TryApplyDetour(Detour_OnKill);
        TryApplyDetour(Detour_CanDamage);
        TryApplyDetour(Detour_MinionContactDamage);
        TryApplyDetour(Detour_ModifyDamageHitbox);
        TryApplyDetour(Detour_CanHitNPC);
        TryApplyDetour(Detour_ModifyHitNPC);
        TryApplyDetour(Detour_OnHitNPC);
        TryApplyDetour(Detour_CanHitPvp);
        TryApplyDetour(Detour_CanHitPlayer);
        TryApplyDetour(Detour_ModifyHitPlayer);
        TryApplyDetour(Detour_OnHitPlayer);
        TryApplyDetour(Detour_Colliding);
        TryApplyDetour(Detour_GetAlpha);
        TryApplyDetour(Detour_PreDrawExtras);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_CanUseGrapple);
        TryApplyDetour(Detour_UseGrapple);
        TryApplyDetour(Detour_GrappleRange);
        TryApplyDetour(Detour_NumGrappleHooks);
        TryApplyDetour(Detour_GrappleRetreatSpeed);
        TryApplyDetour(Detour_GrapplePullSpeed);
        TryApplyDetour(Detour_GrappleTargetPoint);
        TryApplyDetour(Detour_GrappleCanLatchOnTo);
        TryApplyDetour(Detour_DrawBehind);
        TryApplyDetour(Detour_PrepareBombToBlow);
        TryApplyDetour(Detour_EmitEnchantmentVisualsAt);
    }
}

public abstract class ModPylonDetour<T> : ModTileDetour<T> where T : ModPylon
{
    // CanPlacePylon
    public delegate bool Orig_CanPlacePylon(T self);
    public virtual bool Detour_CanPlacePylon(Orig_CanPlacePylon orig, T self) => orig(self);

    // GetNPCShopEntry
    public delegate NPCShop.Entry Orig_GetNPCShopEntry(T self);
    public virtual NPCShop.Entry Detour_GetNPCShopEntry(Orig_GetNPCShopEntry orig, T self) => orig(self);

    // ValidTeleportCheck_NPCCount
    public delegate bool Orig_ValidTeleportCheck_NPCCount(T self, TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount);
    public virtual bool Detour_ValidTeleportCheck_NPCCount(Orig_ValidTeleportCheck_NPCCount orig, T self, TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) => orig(self, pylonInfo, defaultNecessaryNPCCount);

    // ValidTeleportCheck_AnyDanger
    public delegate bool Orig_ValidTeleportCheck_AnyDanger(T self, TeleportPylonInfo pylonInfo);
    public virtual bool Detour_ValidTeleportCheck_AnyDanger(Orig_ValidTeleportCheck_AnyDanger orig, T self, TeleportPylonInfo pylonInfo) => orig(self, pylonInfo);

    // ValidTeleportCheck_BiomeRequirements
    public delegate bool Orig_ValidTeleportCheck_BiomeRequirements(T self, TeleportPylonInfo pylonInfo, SceneMetrics sceneData);
    public virtual bool Detour_ValidTeleportCheck_BiomeRequirements(Orig_ValidTeleportCheck_BiomeRequirements orig, T self, TeleportPylonInfo pylonInfo, SceneMetrics sceneData) => orig(self, pylonInfo, sceneData);

    // ValidTeleportCheck_DestinationPostCheck
    public delegate void Orig_ValidTeleportCheck_DestinationPostCheck(T self, TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey);
    public virtual void Detour_ValidTeleportCheck_DestinationPostCheck(Orig_ValidTeleportCheck_DestinationPostCheck orig, T self, TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey) => orig(self, destinationPylonInfo, ref destinationPylonValid, ref errorKey);

    // ValidTeleportCheck_NearbyPostCheck
    public delegate void Orig_ValidTeleportCheck_NearbyPostCheck(T self, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool anyNearbyValidPylon, ref string errorKey);
    public virtual void Detour_ValidTeleportCheck_NearbyPostCheck(Orig_ValidTeleportCheck_NearbyPostCheck orig, T self, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool anyNearbyValidPylon, ref string errorKey) => orig(self, nearbyPylonInfo, ref destinationPylonValid, ref anyNearbyValidPylon, ref errorKey);

    // ModifyTeleportationPosition
    public delegate void Orig_ModifyTeleportationPosition(T self, TeleportPylonInfo destinationPylonInfo, ref Vector2 teleportationPosition);
    public virtual void Detour_ModifyTeleportationPosition(Orig_ModifyTeleportationPosition orig, T self, TeleportPylonInfo destinationPylonInfo, ref Vector2 teleportationPosition) => orig(self, destinationPylonInfo, ref teleportationPosition);

    // DrawMapIcon
    public delegate void Orig_DrawMapIcon(T self, ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale);
    public virtual void Detour_DrawMapIcon(Orig_DrawMapIcon orig, T self, ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) => orig(self, ref context, ref mouseOverText, pylonInfo, isNearPylon, drawColor, deselectedScale, selectedScale);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_CanPlacePylon);
        TryApplyDetour(Detour_GetNPCShopEntry);
        TryApplyDetour(Detour_ValidTeleportCheck_NPCCount);
        TryApplyDetour(Detour_ValidTeleportCheck_AnyDanger);
        TryApplyDetour(Detour_ValidTeleportCheck_BiomeRequirements);
        TryApplyDetour(Detour_ValidTeleportCheck_DestinationPostCheck);
        TryApplyDetour(Detour_ValidTeleportCheck_NearbyPostCheck);
        TryApplyDetour(Detour_ModifyTeleportationPosition);
        TryApplyDetour(Detour_DrawMapIcon);
    }
}

public abstract class ModRarityDetour<T> : ModTypeDetour<T> where T : ModRarity
{
    // RarityColor
    public delegate Color Orig_RarityColor(T self);
    public virtual Color Detour_RarityColor(Orig_RarityColor orig, T self) => orig(self);

    // GetPrefixedRarity
    public delegate int Orig_GetPrefixedRarity(T self, int offset, float valueMult);
    public virtual int Detour_GetPrefixedRarity(Orig_GetPrefixedRarity orig, T self, int offset, float valueMult) => orig(self, offset, valueMult);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_RarityColor);
        TryApplyDetour(Detour_GetPrefixedRarity);
    }
}

public abstract class ModResourceDisplaySetDetour<T> : ModTypeDetour<T> where T : ModResourceDisplaySet
{
    // PreDrawResources
    public delegate void Orig_PreDrawResources(T self, PlayerStatsSnapshot snapshot);
    public virtual void Detour_PreDrawResources(Orig_PreDrawResources orig, T self, PlayerStatsSnapshot snapshot) => orig(self, snapshot);

    // DrawLife
    public delegate void Orig_DrawLife(T self, SpriteBatch spriteBatch);
    public virtual void Detour_DrawLife(Orig_DrawLife orig, T self, SpriteBatch spriteBatch) => orig(self, spriteBatch);

    // DrawMana
    public delegate void Orig_DrawMana(T self, SpriteBatch spriteBatch);
    public virtual void Detour_DrawMana(Orig_DrawMana orig, T self, SpriteBatch spriteBatch) => orig(self, spriteBatch);

    // PreHover
    public delegate bool Orig_PreHover(T self, out bool hoveringLife);
    public virtual bool Detour_PreHover(Orig_PreHover orig, T self, out bool hoveringLife) => orig(self, out hoveringLife);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_PreDrawResources);
        TryApplyDetour(Detour_DrawLife);
        TryApplyDetour(Detour_DrawMana);
        TryApplyDetour(Detour_PreHover);
    }
}

public abstract class ModResourceOverlayDetour<T> : ModTypeDetour<T> where T : ModResourceOverlay
{
    // PreDrawResource
    public delegate bool Orig_PreDrawResource(T self, ResourceOverlayDrawContext context);
    public virtual bool Detour_PreDrawResource(Orig_PreDrawResource orig, T self, ResourceOverlayDrawContext context) => orig(self, context);

    // PostDrawResource
    public delegate void Orig_PostDrawResource(T self, ResourceOverlayDrawContext context);
    public virtual void Detour_PostDrawResource(Orig_PostDrawResource orig, T self, ResourceOverlayDrawContext context) => orig(self, context);

    // PreDrawResourceDisplay
    public delegate bool Orig_PreDrawResourceDisplay(T self, PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, ref Color textColor, out bool drawText);
    public virtual bool Detour_PreDrawResourceDisplay(Orig_PreDrawResourceDisplay orig, T self, PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, ref Color textColor, out bool drawText) => orig(self, snapshot, displaySet, drawingLife, ref textColor, out drawText);

    // PostDrawResourceDisplay
    public delegate void Orig_PostDrawResourceDisplay(T self, PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText);
    public virtual void Detour_PostDrawResourceDisplay(Orig_PostDrawResourceDisplay orig, T self, PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText) => orig(self, snapshot, displaySet, drawingLife, textColor, drawText);

    // DisplayHoverText
    public delegate bool Orig_DisplayHoverText(T self, PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife);
    public virtual bool Detour_DisplayHoverText(Orig_DisplayHoverText orig, T self, PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife) => orig(self, snapshot, displaySet, drawingLife);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_PreDrawResource);
        TryApplyDetour(Detour_PostDrawResource);
        TryApplyDetour(Detour_PreDrawResourceDisplay);
        TryApplyDetour(Detour_PostDrawResourceDisplay);
        TryApplyDetour(Detour_DisplayHoverText);
    }
}

public abstract class ModSceneEffectDetour<T> : ModTypeDetour<T> where T : ModSceneEffect
{
    // GetWeight
    public delegate float Orig_GetWeight(T self, Player player);
    public virtual float Detour_GetWeight(Orig_GetWeight orig, T self, Player player) => orig(self, player);

    // IsSceneEffectActive
    public delegate bool Orig_IsSceneEffectActive(T self, Player player);
    public virtual bool Detour_IsSceneEffectActive(Orig_IsSceneEffectActive orig, T self, Player player) => orig(self, player);

    // SpecialVisuals
    public delegate void Orig_SpecialVisuals(T self, Player player, bool isActive);
    public virtual void Detour_SpecialVisuals(Orig_SpecialVisuals orig, T self, Player player, bool isActive) => orig(self, player, isActive);

    // MapBackgroundColor
    public delegate void Orig_MapBackgroundColor(T self, ref Color color);
    public virtual void Detour_MapBackgroundColor(Orig_MapBackgroundColor orig, T self, ref Color color) => orig(self, ref color);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_GetWeight);
        TryApplyDetour(Detour_IsSceneEffectActive);
        TryApplyDetour(Detour_SpecialVisuals);
        TryApplyDetour(Detour_MapBackgroundColor);
    }
}

public abstract class ModSurfaceBackgroundStyleDetour<T> : ModTypeDetour<T> where T : ModSurfaceBackgroundStyle
{
    // ModifyFarFades
    public delegate void Orig_ModifyFarFades(T self, float[] fades, float transitionSpeed);
    public void Detour_ModifyFarFades(Orig_ModifyFarFades orig, T self, float[] fades, float transitionSpeed) => orig(self, fades, transitionSpeed);

    // ChooseFarTexture
    public delegate int Orig_ChooseFarTexture(T self);
    public int Detour_ChooseFarTexture(Orig_ChooseFarTexture orig, T self) => orig(self);

    // ChooseMiddleTexture
    public delegate int Orig_ChooseMiddleTexture(T self);
    public int Detour_ChooseMiddleTexture(Orig_ChooseMiddleTexture orig, T self) => orig(self);

    // PreDrawCloseBackground
    public delegate bool Orig_PreDrawCloseBackground(T self, SpriteBatch spriteBatch);
    public bool Detour_PreDrawCloseBackground(Orig_PreDrawCloseBackground orig, T self, SpriteBatch spriteBatch) => orig(self, spriteBatch);

    // ChooseCloseTexture
    public delegate int Orig_ChooseCloseTexture(T self, ref float scale, ref double parallax, ref float a, ref float b);
    public int Detour_ChooseCloseTexture(Orig_ChooseCloseTexture orig, T self, ref float scale, ref double parallax, ref float a, ref float b) => orig(self, ref scale, ref parallax, ref a, ref b);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_ModifyFarFades);
        TryApplyDetour(Detour_ChooseFarTexture);
        TryApplyDetour(Detour_ChooseMiddleTexture);
        TryApplyDetour(Detour_PreDrawCloseBackground);
        TryApplyDetour(Detour_ChooseCloseTexture);
    }
}

public abstract class ModSystemDetour<T> : ModTypeDetour<T> where T : ModSystem
{
    // OnModLoad
    public delegate void Orig_OnModLoad(T self);
    public void Detour_OnModLoad(Orig_OnModLoad orig, T self) => orig(self);

    // OnModUnload
    public delegate void Orig_OnModUnload(T self);
    public void Detour_OnModUnload(Orig_OnModUnload orig, T self) => orig(self);

    // PostSetupContent
    public delegate void Orig_PostSetupContent(T self);
    public void Detour_PostSetupContent(Orig_PostSetupContent orig, T self) => orig(self);

    // OnLocalizationsLoaded
    public delegate void Orig_OnLocalizationsLoaded(T self);
    public void Detour_OnLocalizationsLoaded(Orig_OnLocalizationsLoaded orig, T self) => orig(self);

    // AddRecipes
    public delegate void Orig_AddRecipes(T self);
    public void Detour_AddRecipes(Orig_AddRecipes orig, T self) => orig(self);

    // PostAddRecipes
    public delegate void Orig_PostAddRecipes(T self);
    public void Detour_PostAddRecipes(Orig_PostAddRecipes orig, T self) => orig(self);

    // PostSetupRecipes
    public delegate void Orig_PostSetupRecipes(T self);
    public void Detour_PostSetupRecipes(Orig_PostSetupRecipes orig, T self) => orig(self);

    // AddRecipeGroups
    public delegate void Orig_AddRecipeGroups(T self);
    public void Detour_AddRecipeGroups(Orig_AddRecipeGroups orig, T self) => orig(self);

    // OnWorldLoad
    public delegate void Orig_OnWorldLoad(T self);
    public void Detour_OnWorldLoad(Orig_OnWorldLoad orig, T self) => orig(self);

    // OnWorldUnload
    public delegate void Orig_OnWorldUnload(T self);
    public void Detour_OnWorldUnload(Orig_OnWorldUnload orig, T self) => orig(self);

    // ClearWorld
    public delegate void Orig_ClearWorld(T self);
    public void Detour_ClearWorld(Orig_ClearWorld orig, T self) => orig(self);

    // ModifyScreenPosition
    public delegate void Orig_ModifyScreenPosition(T self);
    public void Detour_ModifyScreenPosition(Orig_ModifyScreenPosition orig, T self) => orig(self);

    // ModifyTransformMatrix
    public delegate void Orig_ModifyTransformMatrix(T self, ref SpriteViewMatrix Transform);
    public void Detour_ModifyTransformMatrix(Orig_ModifyTransformMatrix orig, T self, ref SpriteViewMatrix Transform) => orig(self, ref Transform);

    // UpdateUI
    public delegate void Orig_UpdateUI(T self, GameTime gameTime);
    public void Detour_UpdateUI(Orig_UpdateUI orig, T self, GameTime gameTime) => orig(self, gameTime);

    // PreUpdateEntities
    public delegate void Orig_PreUpdateEntities(T self);
    public void Detour_PreUpdateEntities(Orig_PreUpdateEntities orig, T self) => orig(self);

    // PreUpdatePlayers
    public delegate void Orig_PreUpdatePlayers(T self);
    public void Detour_PreUpdatePlayers(Orig_PreUpdatePlayers orig, T self) => orig(self);

    // PostUpdatePlayers
    public delegate void Orig_PostUpdatePlayers(T self);
    public void Detour_PostUpdatePlayers(Orig_PostUpdatePlayers orig, T self) => orig(self);

    // PreUpdateNPCs
    public delegate void Orig_PreUpdateNPCs(T self);
    public void Detour_PreUpdateNPCs(Orig_PreUpdateNPCs orig, T self) => orig(self);

    // PostUpdateNPCs
    public delegate void Orig_PostUpdateNPCs(T self);
    public void Detour_PostUpdateNPCs(Orig_PostUpdateNPCs orig, T self) => orig(self);

    // PreUpdateGores
    public delegate void Orig_PreUpdateGores(T self);
    public void Detour_PreUpdateGores(Orig_PreUpdateGores orig, T self) => orig(self);

    // PostUpdateGores
    public delegate void Orig_PostUpdateGores(T self);
    public void Detour_PostUpdateGores(Orig_PostUpdateGores orig, T self) => orig(self);

    // PreUpdateProjectiles
    public delegate void Orig_PreUpdateProjectiles(T self);
    public void Detour_PreUpdateProjectiles(Orig_PreUpdateProjectiles orig, T self) => orig(self);

    // PostUpdateProjectiles
    public delegate void Orig_PostUpdateProjectiles(T self);
    public void Detour_PostUpdateProjectiles(Orig_PostUpdateProjectiles orig, T self) => orig(self);

    // PreUpdateItems
    public delegate void Orig_PreUpdateItems(T self);
    public void Detour_PreUpdateItems(Orig_PreUpdateItems orig, T self) => orig(self);

    // PostUpdateItems
    public delegate void Orig_PostUpdateItems(T self);
    public void Detour_PostUpdateItems(Orig_PostUpdateItems orig, T self) => orig(self);

    // PreUpdateDusts
    public delegate void Orig_PreUpdateDusts(T self);
    public void Detour_PreUpdateDusts(Orig_PreUpdateDusts orig, T self) => orig(self);

    // PostUpdateDusts
    public delegate void Orig_PostUpdateDusts(T self);
    public void Detour_PostUpdateDusts(Orig_PostUpdateDusts orig, T self) => orig(self);

    // PreUpdateTime
    public delegate void Orig_PreUpdateTime(T self);
    public void Detour_PreUpdateTime(Orig_PreUpdateTime orig, T self) => orig(self);

    // PostUpdateTime
    public delegate void Orig_PostUpdateTime(T self);
    public void Detour_PostUpdateTime(Orig_PostUpdateTime orig, T self) => orig(self);

    // PreUpdateWorld
    public delegate void Orig_PreUpdateWorld(T self);
    public void Detour_PreUpdateWorld(Orig_PreUpdateWorld orig, T self) => orig(self);

    // PostUpdateWorld
    public delegate void Orig_PostUpdateWorld(T self);
    public void Detour_PostUpdateWorld(Orig_PostUpdateWorld orig, T self) => orig(self);

    // PreUpdateInvasions
    public delegate void Orig_PreUpdateInvasions(T self);
    public void Detour_PreUpdateInvasions(Orig_PreUpdateInvasions orig, T self) => orig(self);

    // PostUpdateInvasions
    public delegate void Orig_PostUpdateInvasions(T self);
    public void Detour_PostUpdateInvasions(Orig_PostUpdateInvasions orig, T self) => orig(self);

    // PostUpdateEverything
    public delegate void Orig_PostUpdateEverything(T self);
    public void Detour_PostUpdateEverything(Orig_PostUpdateEverything orig, T self) => orig(self);

    // ModifyInterfaceLayers
    public delegate void Orig_ModifyInterfaceLayers(T self, List<GameInterfaceLayer> layers);
    public void Detour_ModifyInterfaceLayers(Orig_ModifyInterfaceLayers orig, T self, List<GameInterfaceLayer> layers) => orig(self, layers);

    // ModifyGameTipVisibility
    public delegate void Orig_ModifyGameTipVisibility(T self, IReadOnlyList<GameTipData> gameTips);
    public void Detour_ModifyGameTipVisibility(Orig_ModifyGameTipVisibility orig, T self, IReadOnlyList<GameTipData> gameTips) => orig(self, gameTips);

    // PostDrawInterface
    public delegate void Orig_PostDrawInterface(T self, SpriteBatch spriteBatch);
    public void Detour_PostDrawInterface(Orig_PostDrawInterface orig, T self, SpriteBatch spriteBatch) => orig(self, spriteBatch);

    // PreDrawMapIconOverlay
    public delegate void Orig_PreDrawMapIconOverlay(T self, IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext);
    public void Detour_PreDrawMapIconOverlay(Orig_PreDrawMapIconOverlay orig, T self, IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext) => orig(self, layers, mapOverlayDrawContext);

    // PostDrawFullscreenMap
    public delegate void Orig_PostDrawFullscreenMap(T self, ref string mouseText);
    public void Detour_PostDrawFullscreenMap(Orig_PostDrawFullscreenMap orig, T self, ref string mouseText) => orig(self, ref mouseText);

    // PostUpdateInput
    public delegate void Orig_PostUpdateInput(T self);
    public void Detour_PostUpdateInput(Orig_PostUpdateInput orig, T self) => orig(self);

    // PreSaveAndQuit
    public delegate void Orig_PreSaveAndQuit(T self);
    public void Detour_PreSaveAndQuit(Orig_PreSaveAndQuit orig, T self) => orig(self);

    // PostDrawTiles
    public delegate void Orig_PostDrawTiles(T self);
    public void Detour_PostDrawTiles(Orig_PostDrawTiles orig, T self) => orig(self);

    // ModifyTimeRate
    public delegate void Orig_ModifyTimeRate(T self, ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate);
    public void Detour_ModifyTimeRate(Orig_ModifyTimeRate orig, T self, ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate) => orig(self, ref timeRate, ref tileUpdateRate, ref eventUpdateRate);

    // SaveWorldData
    public delegate void Orig_SaveWorldData(T self, TagCompound tag);
    public void Detour_SaveWorldData(Orig_SaveWorldData orig, T self, TagCompound tag) => orig(self, tag);

    // LoadWorldData
    public delegate void Orig_LoadWorldData(T self, TagCompound tag);
    public void Detour_LoadWorldData(Orig_LoadWorldData orig, T self, TagCompound tag) => orig(self, tag);

    // SaveWorldHeader
    public delegate void Orig_SaveWorldHeader(T self, TagCompound tag);
    public void Detour_SaveWorldHeader(Orig_SaveWorldHeader orig, T self, TagCompound tag) => orig(self, tag);

    // CanWorldBePlayed
    public delegate bool Orig_CanWorldBePlayed(T self, PlayerFileData playerData, WorldFileData worldFileData);
    public void Detour_CanWorldBePlayed(Orig_CanWorldBePlayed orig, T self, PlayerFileData playerData, WorldFileData worldFileData) => orig(self, playerData, worldFileData);

    // WorldCanBePlayedRejectionMessage
    public delegate string Orig_WorldCanBePlayedRejectionMessage(T self, PlayerFileData playerData, WorldFileData worldData);
    public void Detour_WorldCanBePlayedRejectionMessage(Orig_WorldCanBePlayedRejectionMessage orig, T self, PlayerFileData playerData, WorldFileData worldData) => orig(self, playerData, worldData);

    // NetSend
    public delegate void Orig_NetSend(T self, BinaryWriter writer);
    public void Detour_NetSend(Orig_NetSend orig, T self, BinaryWriter writer) => orig(self, writer);

    // NetReceive
    public delegate void Orig_NetReceive(T self, BinaryReader reader);
    public void Detour_NetReceive(Orig_NetReceive orig, T self, BinaryReader reader) => orig(self, reader);

    // HijackGetData
    public delegate bool Orig_HijackGetData(T self, ref byte messageType, ref BinaryReader reader, int playerNumber);
    public void Detour_HijackGetData(Orig_HijackGetData orig, T self, ref byte messageType, ref BinaryReader reader, int playerNumber) => orig(self, ref messageType, ref reader, playerNumber);

    // HijackSendData
    public delegate bool Orig_HijackSendData(T self, int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7);
    public void Detour_HijackSendData(Orig_HijackSendData orig, T self, int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7) => orig(self, whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);

    // PreWorldGen
    public delegate void Orig_PreWorldGen(T self);
    public void Detour_PreWorldGen(Orig_PreWorldGen orig, T self) => orig(self);

    // ModifyWorldGenTasks
    public delegate void Orig_ModifyWorldGenTasks(T self, List<GenPass> tasks, ref double totalWeight);
    public void Detour_ModifyWorldGenTasks(Orig_ModifyWorldGenTasks orig, T self, List<GenPass> tasks, ref double totalWeight) => orig(self, tasks, ref totalWeight);

    // PostWorldGen
    public delegate void Orig_PostWorldGen(T self);
    public void Detour_PostWorldGen(Orig_PostWorldGen orig, T self) => orig(self);

    // ResetNearbyTileEffects
    public delegate void Orig_ResetNearbyTileEffects(T self);
    public void Detour_ResetNearbyTileEffects(Orig_ResetNearbyTileEffects orig, T self) => orig(self);

    // ModifyHardmodeTasks
    public delegate void Orig_ModifyHardmodeTasks(T self, List<GenPass> list);
    public void Detour_ModifyHardmodeTasks(Orig_ModifyHardmodeTasks orig, T self, List<GenPass> list) => orig(self, list);

    // ModifySunLightColor
    public delegate void Orig_ModifySunLightColor(T self, ref Color tileColor, ref Color backgroundColor);
    public void Detour_ModifySunLightColor(Orig_ModifySunLightColor orig, T self, ref Color tileColor, ref Color backgroundColor) => orig(self, ref tileColor, ref backgroundColor);

    // ModifyLightingBrightness
    public delegate void Orig_ModifyLightingBrightness(T self, ref float scale);
    public void Detour_ModifyLightingBrightness(Orig_ModifyLightingBrightness orig, T self, ref float scale) => orig(self, ref scale);

    // TileCountsAvailable
    public delegate void Orig_TileCountsAvailable(T self, ReadOnlySpan<int> tileCounts);
    public void Detour_TileCountsAvailable(Orig_TileCountsAvailable orig, T self, ReadOnlySpan<int> tileCounts) => orig(self, tileCounts);

    // ResizeArrays
    public delegate void Orig_ResizeArrays(T self);
    public void Detour_ResizeArrays(Orig_ResizeArrays orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_OnModLoad);
        TryApplyDetour(Detour_OnModUnload);
        TryApplyDetour(Detour_PostSetupContent);
        TryApplyDetour(Detour_OnLocalizationsLoaded);
        TryApplyDetour(Detour_AddRecipes);
        TryApplyDetour(Detour_PostAddRecipes);
        TryApplyDetour(Detour_PostSetupRecipes);
        TryApplyDetour(Detour_AddRecipeGroups);
        TryApplyDetour(Detour_OnWorldLoad);
        TryApplyDetour(Detour_OnWorldUnload);
        TryApplyDetour(Detour_ClearWorld);
        TryApplyDetour(Detour_ModifyScreenPosition);
        TryApplyDetour(Detour_ModifyTransformMatrix);
        TryApplyDetour(Detour_UpdateUI);
        TryApplyDetour(Detour_PreUpdateEntities);
        TryApplyDetour(Detour_PreUpdatePlayers);
        TryApplyDetour(Detour_PostUpdatePlayers);
        TryApplyDetour(Detour_PreUpdateNPCs);
        TryApplyDetour(Detour_PostUpdateNPCs);
        TryApplyDetour(Detour_PreUpdateGores);
        TryApplyDetour(Detour_PostUpdateGores);
        TryApplyDetour(Detour_PreUpdateProjectiles);
        TryApplyDetour(Detour_PostUpdateProjectiles);
        TryApplyDetour(Detour_PreUpdateItems);
        TryApplyDetour(Detour_PostUpdateItems);
        TryApplyDetour(Detour_PreUpdateDusts);
        TryApplyDetour(Detour_PostUpdateDusts);
        TryApplyDetour(Detour_PreUpdateTime);
        TryApplyDetour(Detour_PostUpdateTime);
        TryApplyDetour(Detour_PreUpdateWorld);
        TryApplyDetour(Detour_PostUpdateWorld);
        TryApplyDetour(Detour_PreUpdateInvasions);
        TryApplyDetour(Detour_PostUpdateInvasions);
        TryApplyDetour(Detour_PostUpdateEverything);
        TryApplyDetour(Detour_ModifyInterfaceLayers);
        TryApplyDetour(Detour_ModifyGameTipVisibility);
        TryApplyDetour(Detour_PostDrawInterface);
        TryApplyDetour(Detour_PreDrawMapIconOverlay);
        TryApplyDetour(Detour_PostDrawFullscreenMap);
        TryApplyDetour(Detour_PostUpdateInput);
        TryApplyDetour(Detour_PreSaveAndQuit);
        TryApplyDetour(Detour_PostDrawTiles);
        TryApplyDetour(Detour_ModifyTimeRate);
        TryApplyDetour(Detour_SaveWorldData);
        TryApplyDetour(Detour_LoadWorldData);
        TryApplyDetour(Detour_SaveWorldHeader);
        TryApplyDetour(Detour_CanWorldBePlayed);
        TryApplyDetour(Detour_WorldCanBePlayedRejectionMessage);
        TryApplyDetour(Detour_NetSend);
        TryApplyDetour(Detour_NetReceive);
        TryApplyDetour(Detour_HijackGetData);
        TryApplyDetour(Detour_HijackSendData);
        TryApplyDetour(Detour_PreWorldGen);
        TryApplyDetour(Detour_ModifyWorldGenTasks);
        TryApplyDetour(Detour_PostWorldGen);
        TryApplyDetour(Detour_ResetNearbyTileEffects);
        TryApplyDetour(Detour_ModifyHardmodeTasks);
        TryApplyDetour(Detour_ModifySunLightColor);
        TryApplyDetour(Detour_ModifyLightingBrightness);
        TryApplyDetour(Detour_TileCountsAvailable);
        TryApplyDetour(Detour_ResizeArrays);
    }
}

public abstract class ModTileDetour<T> : ModTypeDetour<T> where T : ModTile
{
    // PostSetDefaults
    public delegate void Orig_PostSetDefaults(T self);
    public void Detour_PostSetDefaults(Orig_PostSetDefaults orig, T self) => orig(self);

    // HasSmartInteract
    public delegate bool Orig_HasSmartInteract(T self, int i, int j, SmartInteractScanSettings settings);
    public void Detour_HasSmartInteract(Orig_HasSmartInteract orig, T self, int i, int j, SmartInteractScanSettings settings) => orig(self, i, j, settings);

    // ModifySmartInteractCoords
    public delegate void Orig_ModifySmartInteractCoords(T self, ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY);
    public void Detour_ModifySmartInteractCoords(Orig_ModifySmartInteractCoords orig, T self, ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) => orig(self, ref width, ref height, ref frameWidth, ref frameHeight, ref extraY);

    // ModifySittingTargetInfo
    public delegate void Orig_ModifySittingTargetInfo(T self, int i, int j, ref TileRestingInfo info);
    public void Detour_ModifySittingTargetInfo(Orig_ModifySittingTargetInfo orig, T self, int i, int j, ref TileRestingInfo info) => orig(self, i, j, ref info);

    // ModifySleepingTargetInfo
    public delegate void Orig_ModifySleepingTargetInfo(T self, int i, int j, ref TileRestingInfo info);
    public void Detour_ModifySleepingTargetInfo(Orig_ModifySleepingTargetInfo orig, T self, int i, int j, ref TileRestingInfo info) => orig(self, i, j, ref info);

    // DropCritterChance
    public delegate void Orig_DropCritterChance(T self, int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance);
    public void Detour_DropCritterChance(Orig_DropCritterChance orig, T self, int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) => orig(self, i, j, ref wormChance, ref grassHopperChance, ref jungleGrubChance);

    // CanDrop
    public delegate bool Orig_CanDrop(T self, int i, int j);
    public void Detour_CanDrop(Orig_CanDrop orig, T self, int i, int j) => orig(self, i, j);

    // GetItemDrops
    public delegate IEnumerable<Item> Orig_GetItemDrops(T self, int i, int j);
    public void Detour_GetItemDrops(Orig_GetItemDrops orig, T self, int i, int j) => orig(self, i, j);

    // CanKillTile
    public delegate bool Orig_CanKillTile(T self, int i, int j, ref bool blockDamaged);
    public void Detour_CanKillTile(Orig_CanKillTile orig, T self, int i, int j, ref bool blockDamaged) => orig(self, i, j, ref blockDamaged);

    // KillTile
    public delegate void Orig_KillTile(T self, int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem);
    public void Detour_KillTile(Orig_KillTile orig, T self, int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => orig(self, i, j, ref fail, ref effectOnly, ref noItem);

    // KillMultiTile
    public delegate void Orig_KillMultiTile(T self, int i, int j, int frameX, int frameY);
    public void Detour_KillMultiTile(Orig_KillMultiTile orig, T self, int i, int j, int frameX, int frameY) => orig(self, i, j, frameX, frameY);

    // NearbyEffects
    public delegate void Orig_NearbyEffects(T self, int i, int j, bool closer);
    public void Detour_NearbyEffects(Orig_NearbyEffects orig, T self, int i, int j, bool closer) => orig(self, i, j, closer);

    // GetTorchLuck
    public delegate float Orig_GetTorchLuck(T self, Player player);
    public void Detour_GetTorchLuck(Orig_GetTorchLuck orig, T self, Player player) => orig(self, player);

    // IsTileDangerous
    public delegate bool Orig_IsTileDangerous(T self, int i, int j, Player player);
    public void Detour_IsTileDangerous(Orig_IsTileDangerous orig, T self, int i, int j, Player player) => orig(self, i, j, player);

    // IsTileBiomeSightable
    public delegate bool Orig_IsTileBiomeSightable(T self, int i, int j, ref Color sightColor);
    public void Detour_IsTileBiomeSightable(Orig_IsTileBiomeSightable orig, T self, int i, int j, ref Color sightColor) => orig(self, i, j, ref sightColor);

    // IsTileSpelunkable
    public delegate bool Orig_IsTileSpelunkable(T self, int i, int j);
    public void Detour_IsTileSpelunkable(Orig_IsTileSpelunkable orig, T self, int i, int j) => orig(self, i, j);

    // SetSpriteEffects
    public delegate void Orig_SetSpriteEffects(T self, int i, int j, ref SpriteEffects spriteEffects);
    public void Detour_SetSpriteEffects(Orig_SetSpriteEffects orig, T self, int i, int j, ref SpriteEffects spriteEffects) => orig(self, i, j, ref spriteEffects);

    // SetDrawPositions
    public delegate void Orig_SetDrawPositions(T self, int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY);
    public void Detour_SetDrawPositions(Orig_SetDrawPositions orig, T self, int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => orig(self, i, j, ref width, ref offsetY, ref height, ref tileFrameX, ref tileFrameY);

    // AnimateTile
    public delegate void Orig_AnimateTile(T self, ref int frame, ref int frameCounter);
    public void Detour_AnimateTile(Orig_AnimateTile orig, T self, ref int frame, ref int frameCounter) => orig(self, ref frame, ref frameCounter);

    // AnimateIndividualTile
    public delegate void Orig_AnimateIndividualTile(T self, int type, int i, int j, ref int frameXOffset, ref int frameYOffset);
    public void Detour_AnimateIndividualTile(Orig_AnimateIndividualTile orig, T self, int type, int i, int j, ref int frameXOffset, ref int frameYOffset) => orig(self, type, i, j, ref frameXOffset, ref frameYOffset);

    // DrawEffects
    public delegate void Orig_DrawEffects(T self, int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData);
    public void Detour_DrawEffects(Orig_DrawEffects orig, T self, int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) => orig(self, i, j, spriteBatch, ref drawData);

    // EmitParticles
    public delegate void Orig_EmitParticles(T self, int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible);
    public void Detour_EmitParticles(Orig_EmitParticles orig, T self, int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible) => orig(self, i, j, tile, tileFrameX, tileFrameY, tileLight, visible);

    // SpecialDraw
    public delegate void Orig_SpecialDraw(T self, int i, int j, SpriteBatch spriteBatch);
    public void Detour_SpecialDraw(Orig_SpecialDraw orig, T self, int i, int j, SpriteBatch spriteBatch) => orig(self, i, j, spriteBatch);

    // TileFrame
    public delegate bool Orig_TileFrame(T self, int i, int j, ref bool resetFrame, ref bool noBreak);
    public void Detour_TileFrame(Orig_TileFrame orig, T self, int i, int j, ref bool resetFrame, ref bool noBreak) => orig(self, i, j, ref resetFrame, ref noBreak);

    // PostTileFrame
    public delegate void Orig_PostTileFrame(T self, int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight);
    public void Detour_PostTileFrame(Orig_PostTileFrame orig, T self, int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight) => orig(self, i, j, up, down, left, right, upLeft, upRight, downLeft, downRight);

    // ModifyFrameMerge
    public delegate void Orig_ModifyFrameMerge(T self, int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight);
    public void Detour_ModifyFrameMerge(Orig_ModifyFrameMerge orig, T self, int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) => orig(self, i, j, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);

    // RightClick
    public delegate bool Orig_RightClick(T self, int i, int j);
    public void Detour_RightClick(Orig_RightClick orig, T self, int i, int j) => orig(self, i, j);

    // MouseOver
    public delegate void Orig_MouseOver(T self, int i, int j);
    public void Detour_MouseOver(Orig_MouseOver orig, T self, int i, int j) => orig(self, i, j);

    // MouseOverFar
    public delegate void Orig_MouseOverFar(T self, int i, int j);
    public void Detour_MouseOverFar(Orig_MouseOverFar orig, T self, int i, int j) => orig(self, i, j);

    // AutoSelect
    public delegate bool Orig_AutoSelect(T self, int i, int j, Item item);
    public void Detour_AutoSelect(Orig_AutoSelect orig, T self, int i, int j, Item item) => orig(self, i, j, item);

    // HitWire
    public delegate void Orig_HitWire(T self, int i, int j);
    public void Detour_HitWire(Orig_HitWire orig, T self, int i, int j) => orig(self, i, j);

    // Slope
    public delegate bool Orig_Slope(T self, int i, int j);
    public void Detour_Slope(Orig_Slope orig, T self, int i, int j) => orig(self, i, j);

    // FloorVisuals
    public delegate void Orig_FloorVisuals(T self, Player player);
    public void Detour_FloorVisuals(Orig_FloorVisuals orig, T self, Player player) => orig(self, player);

    // HasWalkDust
    public delegate bool Orig_HasWalkDust(T self);
    public void Detour_HasWalkDust(Orig_HasWalkDust orig, T self) => orig(self);

    // WalkDust
    public delegate void Orig_WalkDust(T self, ref int dustType, ref bool makeDust, ref Color color);
    public void Detour_WalkDust(Orig_WalkDust orig, T self, ref int dustType, ref bool makeDust, ref Color color) => orig(self, ref dustType, ref makeDust, ref color);

    // ChangeWaterfallStyle
    public delegate void Orig_ChangeWaterfallStyle(T self, ref int style);
    public void Detour_ChangeWaterfallStyle(Orig_ChangeWaterfallStyle orig, T self, ref int style) => orig(self, ref style);

    // PostSetupTileMerge
    public delegate void Orig_PostSetupTileMerge(T self);
    public void Detour_PostSetupTileMerge(Orig_PostSetupTileMerge orig, T self) => orig(self);

    // IsLockedChest
    public delegate bool Orig_IsLockedChest(T self, int i, int j);
    public void Detour_IsLockedChest(Orig_IsLockedChest orig, T self, int i, int j) => orig(self, i, j);

    // UnlockChest
    public delegate bool Orig_UnlockChest(T self, int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual);
    public void Detour_UnlockChest(Orig_UnlockChest orig, T self, int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) => orig(self, i, j, ref frameXAdjustment, ref dustType, ref manual);

    // LockChest
    public delegate bool Orig_LockChest(T self, int i, int j, ref short frameXAdjustment, ref bool manual);
    public void Detour_LockChest(Orig_LockChest orig, T self, int i, int j, ref short frameXAdjustment, ref bool manual) => orig(self, i, j, ref frameXAdjustment, ref manual);

    // DefaultContainerName
    public delegate LocalizedText Orig_DefaultContainerName(T self, int frameX, int frameY);
    public void Detour_DefaultContainerName(Orig_DefaultContainerName orig, T self, int frameX, int frameY) => orig(self, frameX, frameY);

    // CanReplace
    public delegate bool Orig_CanReplace(T self, int i, int j, int tileTypeBeingPlaced);
    public void Detour_CanReplace(Orig_CanReplace orig, T self, int i, int j, int tileTypeBeingPlaced) => orig(self, i, j, tileTypeBeingPlaced);

    // AdjustMultiTileVineParameters
    public delegate void Orig_AdjustMultiTileVineParameters(T self, int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor);
    public void Detour_AdjustMultiTileVineParameters(Orig_AdjustMultiTileVineParameters orig, T self, int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) => orig(self, i, j, ref overrideWindCycle, ref windPushPowerX, ref windPushPowerY, ref dontRotateTopTiles, ref totalWindMultiplier, ref glowTexture, ref glowColor);

    // GetTileFlameData
    public delegate void Orig_GetTileFlameData(T self, int i, int j, ref TileDrawing.TileFlameData tileFlameData);
    public void Detour_GetTileFlameData(Orig_GetTileFlameData orig, T self, int i, int j, ref TileDrawing.TileFlameData tileFlameData) => orig(self, i, j, ref tileFlameData);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_PostSetDefaults);
        TryApplyDetour(Detour_HasSmartInteract);
        TryApplyDetour(Detour_ModifySmartInteractCoords);
        TryApplyDetour(Detour_ModifySittingTargetInfo);
        TryApplyDetour(Detour_ModifySleepingTargetInfo);
        TryApplyDetour(Detour_DropCritterChance);
        TryApplyDetour(Detour_CanDrop);
        TryApplyDetour(Detour_GetItemDrops);
        TryApplyDetour(Detour_CanKillTile);
        TryApplyDetour(Detour_KillTile);
        TryApplyDetour(Detour_KillMultiTile);
        TryApplyDetour(Detour_NearbyEffects);
        TryApplyDetour(Detour_GetTorchLuck);
        TryApplyDetour(Detour_IsTileDangerous);
        TryApplyDetour(Detour_IsTileBiomeSightable);
        TryApplyDetour(Detour_IsTileSpelunkable);
        TryApplyDetour(Detour_SetSpriteEffects);
        TryApplyDetour(Detour_SetDrawPositions);
        TryApplyDetour(Detour_AnimateTile);
        TryApplyDetour(Detour_AnimateIndividualTile);
        TryApplyDetour(Detour_DrawEffects);
        TryApplyDetour(Detour_EmitParticles);
        TryApplyDetour(Detour_SpecialDraw);
        TryApplyDetour(Detour_TileFrame);
        TryApplyDetour(Detour_PostTileFrame);
        TryApplyDetour(Detour_ModifyFrameMerge);
        TryApplyDetour(Detour_RightClick);
        TryApplyDetour(Detour_MouseOver);
        TryApplyDetour(Detour_MouseOverFar);
        TryApplyDetour(Detour_AutoSelect);
        TryApplyDetour(Detour_HitWire);
        TryApplyDetour(Detour_Slope);
        TryApplyDetour(Detour_FloorVisuals);
        TryApplyDetour(Detour_HasWalkDust);
        TryApplyDetour(Detour_WalkDust);
        TryApplyDetour(Detour_ChangeWaterfallStyle);
        TryApplyDetour(Detour_PostSetupTileMerge);
        TryApplyDetour(Detour_IsLockedChest);
        TryApplyDetour(Detour_UnlockChest);
        TryApplyDetour(Detour_LockChest);
        TryApplyDetour(Detour_DefaultContainerName);
        TryApplyDetour(Detour_CanReplace);
        TryApplyDetour(Detour_AdjustMultiTileVineParameters);
        TryApplyDetour(Detour_GetTileFlameData);
    }
}

public abstract class ModTileEntityDetour<T> : TypeDetour<T> where T : ModTileEntity
{
    // Hook_AfterPlacement
    public delegate int Orig_Hook_AfterPlacement(T self, int i, int j, int type, int style, int direction, int alternate);
    public int Detour_Hook_AfterPlacement(Orig_Hook_AfterPlacement orig, T self, int i, int j, int type, int style, int direction, int alternate) => orig(self, i, j, type, style, direction, alternate);

    // OnNetPlace
    public delegate void Orig_OnNetPlace(T self);
    public void Detour_OnNetPlace(Orig_OnNetPlace orig, T self) => orig(self);

    // PreGlobalUpdate
    public delegate void Orig_PreGlobalUpdate(T self);
    public void Detour_PreGlobalUpdate(Orig_PreGlobalUpdate orig, T self) => orig(self);

    // PostGlobalUpdate
    public delegate void Orig_PostGlobalUpdate(T self);
    public void Detour_PostGlobalUpdate(Orig_PostGlobalUpdate orig, T self) => orig(self);

    // OnKill
    public delegate void Orig_OnKill(T self);
    public void Detour_OnKill(Orig_OnKill orig, T self) => orig(self);

    // IsTileValidForEntity
    public delegate bool Orig_IsTileValidForEntity(T self, int x, int y);
    public bool Detour_IsTileValidForEntity(Orig_IsTileValidForEntity orig, T self, int x, int y) => orig(self, x, y);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Hook_AfterPlacement);
        TryApplyDetour(Detour_OnNetPlace);
        TryApplyDetour(Detour_PreGlobalUpdate);
        TryApplyDetour(Detour_PostGlobalUpdate);
        TryApplyDetour(Detour_OnKill);
        TryApplyDetour(Detour_IsTileValidForEntity);
    }
}

public abstract class ModTreeDetour<T> : TypeDetour<T> where T : ModTree
{
    // SetStaticDefaults
    public delegate void Orig_SetStaticDefaults(T self);
    public virtual void Detour_SetStaticDefaults(Orig_SetStaticDefaults orig, T self) => orig(self);

    // GetTexture
    public delegate Asset<Texture2D> Orig_GetTexture(T self);
    public virtual Asset<Texture2D> Detour_GetTexture(Orig_GetTexture orig, T self) => orig(self);

    // CountsAsTreeType
    public delegate TreeTypes Orig_CountsAsTreeType(T self);
    public virtual TreeTypes Detour_CountsAsTreeType(Orig_CountsAsTreeType orig, T self) => orig(self);

    // CreateDust
    public delegate int Orig_CreateDust(T self);
    public virtual int Detour_CreateDust(Orig_CreateDust orig, T self) => orig(self);

    // TreeLeaf
    public delegate int Orig_TreeLeaf(T self);
    public virtual int Detour_TreeLeaf(Orig_TreeLeaf orig, T self) => orig(self);

    // Shake
    public delegate bool Orig_Shake(T self, int x, int y, ref bool createLeaves);
    public virtual bool Detour_Shake(Orig_Shake orig, T self, int x, int y, ref bool createLeaves) => orig(self, x, y, ref createLeaves);

    // CanDropAcorn
    public delegate bool Orig_CanDropAcorn(T self);
    public virtual bool Detour_CanDropAcorn(Orig_CanDropAcorn orig, T self) => orig(self);

    // SaplingGrowthType
    public delegate int Orig_SaplingGrowthType(T self, ref int style);
    public virtual int Detour_SaplingGrowthType(Orig_SaplingGrowthType orig, T self, ref int style) => orig(self, ref style);

    // DropWood
    public delegate int Orig_DropWood(T self);
    public virtual int Detour_DropWood(Orig_DropWood orig, T self) => orig(self);

    // SetTreeFoliageSettings
    public delegate void Orig_SetTreeFoliageSettings(T self, Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight);
    public virtual void Detour_SetTreeFoliageSettings(Orig_SetTreeFoliageSettings orig, T self, Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) => orig(self, tile, ref xoffset, ref treeFrame, ref floorY, ref topTextureFrameWidth, ref topTextureFrameHeight);

    // GetTopTextures
    public delegate Asset<Texture2D> Orig_GetTopTextures(T self);
    public virtual Asset<Texture2D> Detour_GetTopTextures(Orig_GetTopTextures orig, T self) => orig(self);

    // GetBranchTextures
    public delegate Asset<Texture2D> Orig_GetBranchTextures(T self);
    public virtual Asset<Texture2D> Detour_GetBranchTextures(Orig_GetBranchTextures orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_SetStaticDefaults);
        TryApplyDetour(Detour_GetTexture);
        TryApplyDetour(Detour_CountsAsTreeType);
        TryApplyDetour(Detour_CreateDust);
        TryApplyDetour(Detour_TreeLeaf);
        TryApplyDetour(Detour_Shake);
        TryApplyDetour(Detour_CanDropAcorn);
        TryApplyDetour(Detour_SaplingGrowthType);
        TryApplyDetour(Detour_DropWood);
        TryApplyDetour(Detour_SetTreeFoliageSettings);
        TryApplyDetour(Detour_GetTopTextures);
        TryApplyDetour(Detour_GetBranchTextures);
    }
}

public abstract class ModUndergroundBackgroundStyleDetour<T> : ModTypeDetour<T> where T : ModUndergroundBackgroundStyle
{
    // FillTextureArray
    public delegate void Orig_FillTextureArray(T self, int[] textureSlots);
    public void Detour_FillTextureArray(Orig_FillTextureArray orig, T self, int[] textureSlots) => orig(self, textureSlots);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_FillTextureArray);
    }
}

public abstract class ModWallDetour<T> : ModTypeDetour<T> where T : ModWall
{
    // Drop
    public delegate bool Orig_Drop(T self, int i, int j, ref int type);
    public bool Detour_Drop(Orig_Drop orig, T self, int i, int j, ref int type) => orig(self, i, j, ref type);

    // KillWall
    public delegate void Orig_KillWall(T self, int i, int j, ref bool fail);
    public void Detour_KillWall(Orig_KillWall orig, T self, int i, int j, ref bool fail) => orig(self, i, j, ref fail);

    // AnimateWall
    public delegate void Orig_AnimateWall(T self, ref byte frame, ref byte frameCounter);
    public void Detour_AnimateWall(Orig_AnimateWall orig, T self, ref byte frame, ref byte frameCounter) => orig(self, ref frame, ref frameCounter);

    // WallFrame
    public delegate bool Orig_WallFrame(T self, int i, int j, bool randomizeFrame, ref int style, ref int frameNumber);
    public bool Detour_WallFrame(Orig_WallFrame orig, T self, int i, int j, bool randomizeFrame, ref int style, ref int frameNumber) => orig(self, i, j, randomizeFrame, ref style, ref frameNumber);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Drop);
        TryApplyDetour(Detour_KillWall);
        TryApplyDetour(Detour_AnimateWall);
        TryApplyDetour(Detour_WallFrame);
    }
}

public abstract class ModWaterfallStyleDetour<T> : ModTypeDetour<T> where T : ModWaterfallStyle
{
    // AddLight
    public delegate void Orig_AddLight(T self, int i, int j);
    public void Detour_AddLight(Orig_AddLight orig, T self, int i, int j) => orig(self, i, j);

    // ColorMultiplier
    public delegate void Orig_ColorMultiplier(T self, ref float r, ref float g, ref float b, float a);
    public void Detour_ColorMultiplier(Orig_ColorMultiplier orig, T self, ref float r, ref float g, ref float b, float a) => orig(self, ref r, ref g, ref b, a);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_AddLight);
        TryApplyDetour(Detour_ColorMultiplier);
    }
}

public abstract class ModWaterStyleDetour<T> : ModTypeDetour<T> where T : ModWaterStyle
{
    // ChooseWaterfallStyle
    public delegate int Orig_ChooseWaterfallStyle(T self);
    public int Detour_ChooseWaterfallStyle(Orig_ChooseWaterfallStyle orig, T self) => orig(self);

    // GetSplashDust
    public delegate int Orig_GetSplashDust(T self);
    public int Detour_GetSplashDust(Orig_GetSplashDust orig, T self) => orig(self);

    // GetDropletGore
    public delegate int Orig_GetDropletGore(T self);
    public int Detour_GetDropletGore(Orig_GetDropletGore orig, T self) => orig(self);

    // LightColorMultiplier
    public delegate void Orig_LightColorMultiplier(T self, ref float r, ref float g, ref float b);
    public void Detour_LightColorMultiplier(Orig_LightColorMultiplier orig, T self, ref float r, ref float g, ref float b) => orig(self, ref r, ref g, ref b);

    // BiomeHairColor
    public delegate Color Orig_BiomeHairColor(T self);
    public Color Detour_BiomeHairColor(Orig_BiomeHairColor orig, T self) => orig(self);

    // GetRainTexture
    public delegate Asset<Texture2D> Orig_GetRainTexture(T self);
    public Asset<Texture2D> Detour_GetRainTexture(Orig_GetRainTexture orig, T self) => orig(self);

    // GetRainVariant
    public delegate byte Orig_GetRainVariant(T self);
    public byte Detour_GetRainVariant(Orig_GetRainVariant orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_ChooseWaterfallStyle);
        TryApplyDetour(Detour_GetSplashDust);
        TryApplyDetour(Detour_GetDropletGore);
        TryApplyDetour(Detour_LightColorMultiplier);
        TryApplyDetour(Detour_BiomeHairColor);
        TryApplyDetour(Detour_GetRainTexture);
        TryApplyDetour(Detour_GetRainVariant);
    }
}

public abstract class GlobalTypeDetour<TEntity, TGlobal, TGlobalType> : ModTypeDetour<TGlobalType>
    where TEntity : IEntityWithGlobals<TGlobal>
    where TGlobal : GlobalType<TEntity, TGlobal>
    where TGlobalType : TGlobal
{
    // AppliesToEntity
    public delegate bool Orig_AppliesToEntity(TGlobalType self, TEntity entity, bool lateInstantiation);
    public virtual bool Detour_AppliesToEntity(Orig_AppliesToEntity orig, TGlobalType self, TEntity entity, bool lateInstantiation) => orig(self, entity, lateInstantiation);

    // SetDefaults
    public delegate void Orig_SetDefaults(TGlobalType self, TEntity entity);
    public virtual void Detour_SetDefaults(Orig_SetDefaults orig, TGlobalType self, TEntity entity) => orig(self, entity);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_AppliesToEntity);
        TryApplyDetour(Detour_SetDefaults);
    }
}

public abstract class GlobalBlockTypeDetour<T> : ModTypeDetour<T> where T : GlobalBlockType
{
    // KillSound
    public delegate bool Orig_KillSound(T self, int i, int j, int type, bool fail);
    public virtual bool Detour_KillSound(Orig_KillSound orig, T self, int i, int j, int type, bool fail) => orig(self, i, j, type, fail);

    // NumDust
    public delegate void Orig_NumDust(T self, int i, int j, int type, bool fail, ref int num);
    public virtual void Detour_NumDust(Orig_NumDust orig, T self, int i, int j, int type, bool fail, ref int num) => orig(self, i, j, type, fail, ref num);

    // CreateDust
    public delegate bool Orig_CreateDust(T self, int i, int j, int type, ref int dustType);
    public virtual bool Detour_CreateDust(Orig_CreateDust orig, T self, int i, int j, int type, ref int dustType) => orig(self, i, j, type, ref dustType);

    // CanPlace
    public delegate bool Orig_CanPlace(T self, int i, int j, int type);
    public virtual bool Detour_CanPlace(Orig_CanPlace orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // CanExplode
    public delegate bool Orig_CanExplode(T self, int i, int j, int type);
    public virtual bool Detour_CanExplode(Orig_CanExplode orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, int i, int j, int type, SpriteBatch spriteBatch);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, int i, int j, int type, SpriteBatch spriteBatch) => orig(self, i, j, type, spriteBatch);

    // PostDraw
    public delegate void Orig_PostDraw(T self, int i, int j, int type, SpriteBatch spriteBatch);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, int i, int j, int type, SpriteBatch spriteBatch) => orig(self, i, j, type, spriteBatch);

    // RandomUpdate
    public delegate void Orig_RandomUpdate(T self, int i, int j, int type);
    public virtual void Detour_RandomUpdate(Orig_RandomUpdate orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // PlaceInWorld
    public delegate void Orig_PlaceInWorld(T self, int i, int j, int type, Item item);
    public virtual void Detour_PlaceInWorld(Orig_PlaceInWorld orig, T self, int i, int j, int type, Item item) => orig(self, i, j, type, item);

    // ModifyLight
    public delegate void Orig_ModifyLight(T self, int i, int j, int type, ref float r, ref float g, ref float b);
    public virtual void Detour_ModifyLight(Orig_ModifyLight orig, T self, int i, int j, int type, ref float r, ref float g, ref float b) => orig(self, i, j, type, ref r, ref g, ref b);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_KillSound);
        TryApplyDetour(Detour_NumDust);
        TryApplyDetour(Detour_CreateDust);
        TryApplyDetour(Detour_CanPlace);
        TryApplyDetour(Detour_CanExplode);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_RandomUpdate);
        TryApplyDetour(Detour_PlaceInWorld);
        TryApplyDetour(Detour_ModifyLight);
    }
}

public abstract class GlobalBossBarDetour<T> : ModTypeDetour<T> where T : GlobalBossBar
{
    // PreDraw
    public delegate bool Orig_PreDraw(T self, SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) => orig(self, spriteBatch, npc, ref drawParams);

    // PostDraw
    public delegate void Orig_PostDraw(T self, SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams) => orig(self, spriteBatch, npc, drawParams);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
    }
}

public abstract class GlobalBuffDetour<T> : ModTypeDetour<T> where T : GlobalBuff
{
    // Update (Player)
    public delegate void Orig_UpdatePlayer(T self, int type, Player player, ref int buffIndex);
    public virtual void Detour_UpdatePlayer(Orig_UpdatePlayer orig, T self, int type, Player player, ref int buffIndex) => orig(self, type, player, ref buffIndex);

    // Update (NPC)
    public delegate void Orig_UpdateNPC(T self, int type, NPC npc, ref int buffIndex);
    public virtual void Detour_UpdateNPC(Orig_UpdateNPC orig, T self, int type, NPC npc, ref int buffIndex) => orig(self, type, npc, ref buffIndex);

    // ReApply (Player)
    public delegate bool Orig_ReApplyPlayer(T self, int type, Player player, int time, int buffIndex);
    public virtual bool Detour_ReApplyPlayer(Orig_ReApplyPlayer orig, T self, int type, Player player, int time, int buffIndex) => orig(self, type, player, time, buffIndex);

    // ReApply (NPC)
    public delegate bool Orig_ReApplyNPC(T self, int type, NPC npc, int time, int buffIndex);
    public virtual bool Detour_ReApplyNPC(Orig_ReApplyNPC orig, T self, int type, NPC npc, int time, int buffIndex) => orig(self, type, npc, time, buffIndex);

    // ModifyBuffText
    public delegate void Orig_ModifyBuffText(T self, int type, ref string buffName, ref string tip, ref int rare);
    public virtual void Detour_ModifyBuffText(Orig_ModifyBuffText orig, T self, int type, ref string buffName, ref string tip, ref int rare) => orig(self, type, ref buffName, ref tip, ref rare);

    // CustomBuffTipSize
    public delegate void Orig_CustomBuffTipSize(T self, string buffTip, List<Vector2> sizes);
    public virtual void Detour_CustomBuffTipSize(Orig_CustomBuffTipSize orig, T self, string buffTip, List<Vector2> sizes) => orig(self, buffTip, sizes);

    // DrawCustomBuffTip
    public delegate void Orig_DrawCustomBuffTip(T self, string buffTip, SpriteBatch spriteBatch, int originX, int originY);
    public virtual void Detour_DrawCustomBuffTip(Orig_DrawCustomBuffTip orig, T self, string buffTip, SpriteBatch spriteBatch, int originX, int originY) => orig(self, buffTip, spriteBatch, originX, originY);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams) => orig(self, spriteBatch, type, buffIndex, ref drawParams);

    // PostDraw
    public delegate void Orig_PostDraw(T self, SpriteBatch spriteBatch, int type, int buffIndex, BuffDrawParams drawParams);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, SpriteBatch spriteBatch, int type, int buffIndex, BuffDrawParams drawParams) => orig(self, spriteBatch, type, buffIndex, drawParams);

    // RightClick
    public delegate bool Orig_RightClick(T self, int type, int buffIndex);
    public virtual bool Detour_RightClick(Orig_RightClick orig, T self, int type, int buffIndex) => orig(self, type, buffIndex);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_UpdatePlayer);
        TryApplyDetour(Detour_UpdateNPC);
        TryApplyDetour(Detour_ReApplyPlayer);
        TryApplyDetour(Detour_ReApplyNPC);
        TryApplyDetour(Detour_ModifyBuffText);
        TryApplyDetour(Detour_CustomBuffTipSize);
        TryApplyDetour(Detour_DrawCustomBuffTip);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_RightClick);
    }
}

public abstract class GlobalEmoteBubbleDetour<T> : ModTypeDetour<T> where T : GlobalEmoteBubble
{
    // OnSpawn
    public delegate void Orig_OnSpawn(T self, EmoteBubble emoteBubble);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, EmoteBubble emoteBubble) => orig(self, emoteBubble);

    // UpdateFrame
    public delegate bool Orig_UpdateFrame(T self, EmoteBubble emoteBubble);
    public virtual bool Detour_UpdateFrame(Orig_UpdateFrame orig, T self, EmoteBubble emoteBubble) => orig(self, emoteBubble);

    // UpdateFrameInEmoteMenu
    public delegate bool Orig_UpdateFrameInEmoteMenu(T self, int emoteType, ref int frameCounter);
    public virtual bool Detour_UpdateFrameInEmoteMenu(Orig_UpdateFrameInEmoteMenu orig, T self, int emoteType, ref int frameCounter) => orig(self, emoteType, ref frameCounter);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) => orig(self, emoteBubble, spriteBatch, texture, position, frame, origin, spriteEffects);

    // PostDraw
    public delegate void Orig_PostDraw(T self, EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) => orig(self, emoteBubble, spriteBatch, texture, position, frame, origin, spriteEffects);

    // PreDrawInEmoteMenu
    public delegate bool Orig_PreDrawInEmoteMenu(T self, int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin);
    public virtual bool Detour_PreDrawInEmoteMenu(Orig_PreDrawInEmoteMenu orig, T self, int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) => orig(self, emoteType, spriteBatch, uiEmoteButton, position, frame, origin);

    // PostDrawInEmoteMenu
    public delegate void Orig_PostDrawInEmoteMenu(T self, int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin);
    public virtual void Detour_PostDrawInEmoteMenu(Orig_PostDrawInEmoteMenu orig, T self, int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) => orig(self, emoteType, spriteBatch, uiEmoteButton, position, frame, origin);

    // GetFrame
    public delegate Rectangle? Orig_GetFrame(T self, EmoteBubble emoteBubble);
    public virtual Rectangle? Detour_GetFrame(Orig_GetFrame orig, T self, EmoteBubble emoteBubble) => orig(self, emoteBubble);

    // GetFrameInEmoteMenu
    public delegate Rectangle? Orig_GetFrameInEmoteMenu(T self, int emoteType, int frame, int frameCounter);
    public virtual Rectangle? Detour_GetFrameInEmoteMenu(Orig_GetFrameInEmoteMenu orig, T self, int emoteType, int frame, int frameCounter) => orig(self, emoteType, frame, frameCounter);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_UpdateFrame);
        TryApplyDetour(Detour_UpdateFrameInEmoteMenu);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_PreDrawInEmoteMenu);
        TryApplyDetour(Detour_PostDrawInEmoteMenu);
        TryApplyDetour(Detour_GetFrame);
        TryApplyDetour(Detour_GetFrameInEmoteMenu);
    }
}

public abstract class GlobalInfoDisplayDetour<T> : ModTypeDetour<T> where T : GlobalInfoDisplay
{
    // Active
    public delegate bool? Orig_Active(T self, InfoDisplay currentDisplay);
    public virtual bool? Detour_Active(Orig_Active orig, T self, InfoDisplay currentDisplay) => orig(self, currentDisplay);

    // ModifyDisplayParameters
    public delegate void Orig_ModifyDisplayParameters(T self, InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor);
    public virtual void Detour_ModifyDisplayParameters(Orig_ModifyDisplayParameters orig, T self, InfoDisplay currentDisplay, ref string displayValue, ref string displayName, ref Color displayColor, ref Color displayShadowColor) => orig(self, currentDisplay, ref displayValue, ref displayName, ref displayColor, ref displayShadowColor);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Active);
        TryApplyDetour(Detour_ModifyDisplayParameters);
    }
}

public abstract class GlobalItemDetour<T> : GlobalTypeDetour<Item, GlobalItem, T> where T : GlobalItem
{
    public override bool Detour_AppliesToEntity(Orig_AppliesToEntity orig, T self, Item item, bool lateInstantiation) => base.Detour_AppliesToEntity(orig, self, item, lateInstantiation);

    public override void Detour_SetDefaults(Orig_SetDefaults orig, T self, Item item) => base.Detour_SetDefaults(orig, self, item);

    // OnCreated
    public delegate void Orig_OnCreated(T self, Item item, ItemCreationContext context);
    public virtual void Detour_OnCreated(Orig_OnCreated orig, T self, Item item, ItemCreationContext context) => orig(self, item, context);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self, Item item, IEntitySource source);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, Item item, IEntitySource source) => orig(self, item, source);

    // ChoosePrefix
    public delegate int Orig_ChoosePrefix(T self, Item item, UnifiedRandom rand);
    public virtual int Detour_ChoosePrefix(Orig_ChoosePrefix orig, T self, Item item, UnifiedRandom rand) => orig(self, item, rand);

    // PrefixChance
    public delegate bool? Orig_PrefixChance(T self, Item item, int pre, UnifiedRandom rand);
    public virtual bool? Detour_PrefixChance(Orig_PrefixChance orig, T self, Item item, int pre, UnifiedRandom rand) => orig(self, item, pre, rand);

    // AllowPrefix
    public delegate bool Orig_AllowPrefix(T self, Item item, int pre);
    public virtual bool Detour_AllowPrefix(Orig_AllowPrefix orig, T self, Item item, int pre) => orig(self, item, pre);

    // CanUseItem
    public delegate bool Orig_CanUseItem(T self, Item item, Player player);
    public virtual bool Detour_CanUseItem(Orig_CanUseItem orig, T self, Item item, Player player) => orig(self, item, player);

    // CanAutoReuseItem
    public delegate bool? Orig_CanAutoReuseItem(T self, Item item, Player player);
    public virtual bool? Detour_CanAutoReuseItem(Orig_CanAutoReuseItem orig, T self, Item item, Player player) => orig(self, item, player);

    // UseStyle
    public delegate void Orig_UseStyle(T self, Item item, Player player, Rectangle heldItemFrame);
    public virtual void Detour_UseStyle(Orig_UseStyle orig, T self, Item item, Player player, Rectangle heldItemFrame) => orig(self, item, player, heldItemFrame);

    // HoldStyle
    public delegate void Orig_HoldStyle(T self, Item item, Player player, Rectangle heldItemFrame);
    public virtual void Detour_HoldStyle(Orig_HoldStyle orig, T self, Item item, Player player, Rectangle heldItemFrame) => orig(self, item, player, heldItemFrame);

    // HoldItem
    public delegate void Orig_HoldItem(T self, Item item, Player player);
    public virtual void Detour_HoldItem(Orig_HoldItem orig, T self, Item item, Player player) => orig(self, item, player);

    // UseTimeMultiplier
    public delegate float Orig_UseTimeMultiplier(T self, Item item, Player player);
    public virtual float Detour_UseTimeMultiplier(Orig_UseTimeMultiplier orig, T self, Item item, Player player) => orig(self, item, player);

    // UseAnimationMultiplier
    public delegate float Orig_UseAnimationMultiplier(T self, Item item, Player player);
    public virtual float Detour_UseAnimationMultiplier(Orig_UseAnimationMultiplier orig, T self, Item item, Player player) => orig(self, item, player);

    // UseSpeedMultiplier
    public delegate float Orig_UseSpeedMultiplier(T self, Item item, Player player);
    public virtual float Detour_UseSpeedMultiplier(Orig_UseSpeedMultiplier orig, T self, Item item, Player player) => orig(self, item, player);

    // GetHealLife
    public delegate void Orig_GetHealLife(T self, Item item, Player player, bool quickHeal, ref int healValue);
    public virtual void Detour_GetHealLife(Orig_GetHealLife orig, T self, Item item, Player player, bool quickHeal, ref int healValue) => orig(self, item, player, quickHeal, ref healValue);

    // GetHealMana
    public delegate void Orig_GetHealMana(T self, Item item, Player player, bool quickHeal, ref int healValue);
    public virtual void Detour_GetHealMana(Orig_GetHealMana orig, T self, Item item, Player player, bool quickHeal, ref int healValue) => orig(self, item, player, quickHeal, ref healValue);

    // ModifyManaCost
    public delegate void Orig_ModifyManaCost(T self, Item item, Player player, ref float reduce, ref float mult);
    public virtual void Detour_ModifyManaCost(Orig_ModifyManaCost orig, T self, Item item, Player player, ref float reduce, ref float mult) => orig(self, item, player, ref reduce, ref mult);

    // OnMissingMana
    public delegate void Orig_OnMissingMana(T self, Item item, Player player, int neededMana);
    public virtual void Detour_OnMissingMana(Orig_OnMissingMana orig, T self, Item item, Player player, int neededMana) => orig(self, item, player, neededMana);

    // OnConsumeMana
    public delegate void Orig_OnConsumeMana(T self, Item item, Player player, int manaConsumed);
    public virtual void Detour_OnConsumeMana(Orig_OnConsumeMana orig, T self, Item item, Player player, int manaConsumed) => orig(self, item, player, manaConsumed);

    // ModifyWeaponDamage
    public delegate void Orig_ModifyWeaponDamage(T self, Item item, Player player, ref StatModifier damage);
    public virtual void Detour_ModifyWeaponDamage(Orig_ModifyWeaponDamage orig, T self, Item item, Player player, ref StatModifier damage) => orig(self, item, player, ref damage);

    // ModifyResearchSorting
    public delegate void Orig_ModifyResearchSorting(T self, Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup);
    public virtual void Detour_ModifyResearchSorting(Orig_ModifyResearchSorting orig, T self, Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => orig(self, item, ref itemGroup);

    // CanConsumeBait
    public delegate bool? Orig_CanConsumeBait(T self, Player player, Item bait);
    public virtual bool? Detour_CanConsumeBait(Orig_CanConsumeBait orig, T self, Player player, Item bait) => orig(self, player, bait);

    // CanResearch
    public delegate bool Orig_CanResearch(T self, Item item);
    public virtual bool Detour_CanResearch(Orig_CanResearch orig, T self, Item item) => orig(self, item);

    // OnResearched
    public delegate void Orig_OnResearched(T self, Item item, bool fullyResearched);
    public virtual void Detour_OnResearched(Orig_OnResearched orig, T self, Item item, bool fullyResearched) => orig(self, item, fullyResearched);

    // ModifyWeaponKnockback
    public delegate void Orig_ModifyWeaponKnockback(T self, Item item, Player player, ref StatModifier knockback);
    public virtual void Detour_ModifyWeaponKnockback(Orig_ModifyWeaponKnockback orig, T self, Item item, Player player, ref StatModifier knockback) => orig(self, item, player, ref knockback);

    // ModifyWeaponCrit
    public delegate void Orig_ModifyWeaponCrit(T self, Item item, Player player, ref float crit);
    public virtual void Detour_ModifyWeaponCrit(Orig_ModifyWeaponCrit orig, T self, Item item, Player player, ref float crit) => orig(self, item, player, ref crit);

    // NeedsAmmo
    public delegate bool Orig_NeedsAmmo(T self, Item item, Player player);
    public virtual bool Detour_NeedsAmmo(Orig_NeedsAmmo orig, T self, Item item, Player player) => orig(self, item, player);

    // PickAmmo
    public delegate void Orig_PickAmmo(T self, Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback);
    public virtual void Detour_PickAmmo(Orig_PickAmmo orig, T self, Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) => orig(self, weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);

    // CanChooseAmmo
    public delegate bool? Orig_CanChooseAmmo(T self, Item weapon, Item ammo, Player player);
    public virtual bool? Detour_CanChooseAmmo(Orig_CanChooseAmmo orig, T self, Item weapon, Item ammo, Player player) => orig(self, weapon, ammo, player);

    // CanBeChosenAsAmmo
    public delegate bool? Orig_CanBeChosenAsAmmo(T self, Item ammo, Item weapon, Player player);
    public virtual bool? Detour_CanBeChosenAsAmmo(Orig_CanBeChosenAsAmmo orig, T self, Item ammo, Item weapon, Player player) => orig(self, ammo, weapon, player);

    // CanConsumeAmmo
    public delegate bool Orig_CanConsumeAmmo(T self, Item weapon, Item ammo, Player player);
    public virtual bool Detour_CanConsumeAmmo(Orig_CanConsumeAmmo orig, T self, Item weapon, Item ammo, Player player) => orig(self, weapon, ammo, player);

    // CanBeConsumedAsAmmo
    public delegate bool Orig_CanBeConsumedAsAmmo(T self, Item ammo, Item weapon, Player player);
    public virtual bool Detour_CanBeConsumedAsAmmo(Orig_CanBeConsumedAsAmmo orig, T self, Item ammo, Item weapon, Player player) => orig(self, ammo, weapon, player);

    // OnConsumeAmmo
    public delegate void Orig_OnConsumeAmmo(T self, Item weapon, Item ammo, Player player);
    public virtual void Detour_OnConsumeAmmo(Orig_OnConsumeAmmo orig, T self, Item weapon, Item ammo, Player player) => orig(self, weapon, ammo, player);

    // OnConsumedAsAmmo
    public delegate void Orig_OnConsumedAsAmmo(T self, Item ammo, Item weapon, Player player);
    public virtual void Detour_OnConsumedAsAmmo(Orig_OnConsumedAsAmmo orig, T self, Item ammo, Item weapon, Player player) => orig(self, ammo, weapon, player);

    // CanShoot
    public delegate bool Orig_CanShoot(T self, Item item, Player player);
    public virtual bool Detour_CanShoot(Orig_CanShoot orig, T self, Item item, Player player) => orig(self, item, player);

    // ModifyShootStats
    public delegate void Orig_ModifyShootStats(T self, Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);
    public virtual void Detour_ModifyShootStats(Orig_ModifyShootStats orig, T self, Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => orig(self, item, player, ref position, ref velocity, ref type, ref damage, ref knockback);

    // Shoot
    public delegate bool Orig_Shoot(T self, Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback);
    public virtual bool Detour_Shoot(Orig_Shoot orig, T self, Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => orig(self, item, player, source, position, velocity, type, damage, knockback);

    // UseItemHitbox
    public delegate void Orig_UseItemHitbox(T self, Item item, Player player, ref Rectangle hitbox, ref bool noHitbox);
    public virtual void Detour_UseItemHitbox(Orig_UseItemHitbox orig, T self, Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) => orig(self, item, player, ref hitbox, ref noHitbox);

    // MeleeEffects
    public delegate void Orig_MeleeEffects(T self, Item item, Player player, Rectangle hitbox);
    public virtual void Detour_MeleeEffects(Orig_MeleeEffects orig, T self, Item item, Player player, Rectangle hitbox) => orig(self, item, player, hitbox);

    // CanCatchNPC
    public delegate bool? Orig_CanCatchNPC(T self, Item item, NPC target, Player player);
    public virtual bool? Detour_CanCatchNPC(Orig_CanCatchNPC orig, T self, Item item, NPC target, Player player) => orig(self, item, target, player);

    // OnCatchNPC
    public delegate void Orig_OnCatchNPC(T self, Item item, NPC npc, Player player, bool failed);
    public virtual void Detour_OnCatchNPC(Orig_OnCatchNPC orig, T self, Item item, NPC npc, Player player, bool failed) => orig(self, item, npc, player, failed);

    // ModifyItemScale
    public delegate void Orig_ModifyItemScale(T self, Item item, Player player, ref float scale);
    public virtual void Detour_ModifyItemScale(Orig_ModifyItemScale orig, T self, Item item, Player player, ref float scale) => orig(self, item, player, ref scale);

    // CanHitNPC
    public delegate bool? Orig_CanHitNPC(T self, Item item, Player player, NPC target);
    public virtual bool? Detour_CanHitNPC(Orig_CanHitNPC orig, T self, Item item, Player player, NPC target) => orig(self, item, player, target);

    // CanMeleeAttackCollideWithNPC
    public delegate bool? Orig_CanMeleeAttackCollideWithNPC(T self, Item item, Rectangle meleeAttackHitbox, Player player, NPC target);
    public virtual bool? Detour_CanMeleeAttackCollideWithNPC(Orig_CanMeleeAttackCollideWithNPC orig, T self, Item item, Rectangle meleeAttackHitbox, Player player, NPC target) => orig(self, item, meleeAttackHitbox, player, target);

    // ModifyHitNPC
    public delegate void Orig_ModifyHitNPC(T self, Item item, Player player, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, T self, Item item, Player player, NPC target, ref NPC.HitModifiers modifiers) => orig(self, item, player, target, ref modifiers);

    // OnHitNPC
    public delegate void Orig_OnHitNPC(T self, Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitNPC(Orig_OnHitNPC orig, T self, Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone) => orig(self, item, player, target, hit, damageDone);

    // CanHitPvp
    public delegate bool Orig_CanHitPvp(T self, Item item, Player player, Player target);
    public virtual bool Detour_CanHitPvp(Orig_CanHitPvp orig, T self, Item item, Player player, Player target) => orig(self, item, player, target);

    // ModifyHitPvp
    public delegate void Orig_ModifyHitPvp(T self, Item item, Player player, Player target, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHitPvp(Orig_ModifyHitPvp orig, T self, Item item, Player player, Player target, ref Player.HurtModifiers modifiers) => orig(self, item, player, target, ref modifiers);

    // OnHitPvp
    public delegate void Orig_OnHitPvp(T self, Item item, Player player, Player target, Player.HurtInfo hurtInfo);
    public virtual void Detour_OnHitPvp(Orig_OnHitPvp orig, T self, Item item, Player player, Player target, Player.HurtInfo hurtInfo) => orig(self, item, player, target, hurtInfo);

    // UseItem
    public delegate bool? Orig_UseItem(T self, Item item, Player player);
    public virtual bool? Detour_UseItem(Orig_UseItem orig, T self, Item item, Player player) => orig(self, item, player);

    // UseAnimation
    public delegate void Orig_UseAnimation(T self, Item item, Player player);
    public virtual void Detour_UseAnimation(Orig_UseAnimation orig, T self, Item item, Player player) => orig(self, item, player);

    // ConsumeItem
    public delegate bool Orig_ConsumeItem(T self, Item item, Player player);
    public virtual bool Detour_ConsumeItem(Orig_ConsumeItem orig, T self, Item item, Player player) => orig(self, item, player);

    // OnConsumeItem
    public delegate void Orig_OnConsumeItem(T self, Item item, Player player);
    public virtual void Detour_OnConsumeItem(Orig_OnConsumeItem orig, T self, Item item, Player player) => orig(self, item, player);

    // UseItemFrame
    public delegate void Orig_UseItemFrame(T self, Item item, Player player);
    public virtual void Detour_UseItemFrame(Orig_UseItemFrame orig, T self, Item item, Player player) => orig(self, item, player);

    // HoldItemFrame
    public delegate void Orig_HoldItemFrame(T self, Item item, Player player);
    public virtual void Detour_HoldItemFrame(Orig_HoldItemFrame orig, T self, Item item, Player player) => orig(self, item, player);

    // AltFunctionUse
    public delegate bool Orig_AltFunctionUse(T self, Item item, Player player);
    public virtual bool Detour_AltFunctionUse(Orig_AltFunctionUse orig, T self, Item item, Player player) => orig(self, item, player);

    // UpdateInventory
    public delegate void Orig_UpdateInventory(T self, Item item, Player player);
    public virtual void Detour_UpdateInventory(Orig_UpdateInventory orig, T self, Item item, Player player) => orig(self, item, player);

    // UpdateInfoAccessory
    public delegate void Orig_UpdateInfoAccessory(T self, Item item, Player player);
    public virtual void Detour_UpdateInfoAccessory(Orig_UpdateInfoAccessory orig, T self, Item item, Player player) => orig(self, item, player);

    // UpdateEquip
    public delegate void Orig_UpdateEquip(T self, Item item, Player player);
    public virtual void Detour_UpdateEquip(Orig_UpdateEquip orig, T self, Item item, Player player) => orig(self, item, player);

    // UpdateAccessory
    public delegate void Orig_UpdateAccessory(T self, Item item, Player player, bool hideVisual);
    public virtual void Detour_UpdateAccessory(Orig_UpdateAccessory orig, T self, Item item, Player player, bool hideVisual) => orig(self, item, player, hideVisual);

    // UpdateVanity
    public delegate void Orig_UpdateVanity(T self, Item item, Player player);
    public virtual void Detour_UpdateVanity(Orig_UpdateVanity orig, T self, Item item, Player player) => orig(self, item, player);

    // IsArmorSet
    public delegate string Orig_IsArmorSet(T self, Item head, Item body, Item legs);
    public virtual string Detour_IsArmorSet(Orig_IsArmorSet orig, T self, Item head, Item body, Item legs) => orig(self, head, body, legs);

    // UpdateArmorSet
    public delegate void Orig_UpdateArmorSet(T self, Player player, string set);
    public virtual void Detour_UpdateArmorSet(Orig_UpdateArmorSet orig, T self, Player player, string set) => orig(self, player, set);

    // IsVanitySet
    public delegate string Orig_IsVanitySet(T self, int head, int body, int legs);
    public virtual string Detour_IsVanitySet(Orig_IsVanitySet orig, T self, int head, int body, int legs) => orig(self, head, body, legs);

    // PreUpdateVanitySet
    public delegate void Orig_PreUpdateVanitySet(T self, Player player, string set);
    public virtual void Detour_PreUpdateVanitySet(Orig_PreUpdateVanitySet orig, T self, Player player, string set) => orig(self, player, set);

    // UpdateVanitySet
    public delegate void Orig_UpdateVanitySet(T self, Player player, string set);
    public virtual void Detour_UpdateVanitySet(Orig_UpdateVanitySet orig, T self, Player player, string set) => orig(self, player, set);

    // ArmorSetShadows
    public delegate void Orig_ArmorSetShadows(T self, Player player, string set);
    public virtual void Detour_ArmorSetShadows(Orig_ArmorSetShadows orig, T self, Player player, string set) => orig(self, player, set);

    // SetMatch
    public delegate void Orig_SetMatch(T self, int armorSlot, int type, bool male, ref int equipSlot, ref bool robes);
    public virtual void Detour_SetMatch(Orig_SetMatch orig, T self, int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) => orig(self, armorSlot, type, male, ref equipSlot, ref robes);

    // CanRightClick
    public delegate bool Orig_CanRightClick(T self, Item item);
    public virtual bool Detour_CanRightClick(Orig_CanRightClick orig, T self, Item item) => orig(self, item);

    // RightClick
    public delegate void Orig_RightClick(T self, Item item, Player player);
    public virtual void Detour_RightClick(Orig_RightClick orig, T self, Item item, Player player) => orig(self, item, player);

    // ModifyItemLoot
    public delegate void Orig_ModifyItemLoot(T self, Item item, ItemLoot itemLoot);
    public virtual void Detour_ModifyItemLoot(Orig_ModifyItemLoot orig, T self, Item item, ItemLoot itemLoot) => orig(self, item, itemLoot);

    // CanStack
    public delegate bool Orig_CanStack(T self, Item destination, Item source);
    public virtual bool Detour_CanStack(Orig_CanStack orig, T self, Item destination, Item source) => orig(self, destination, source);

    // CanStackInWorld
    public delegate bool Orig_CanStackInWorld(T self, Item destination, Item source);
    public virtual bool Detour_CanStackInWorld(Orig_CanStackInWorld orig, T self, Item destination, Item source) => orig(self, destination, source);

    // OnStack
    public delegate void Orig_OnStack(T self, Item destination, Item source, int numToTransfer);
    public virtual void Detour_OnStack(Orig_OnStack orig, T self, Item destination, Item source, int numToTransfer) => orig(self, destination, source, numToTransfer);

    // SplitStack
    public delegate void Orig_SplitStack(T self, Item destination, Item source, int numToTransfer);
    public virtual void Detour_SplitStack(Orig_SplitStack orig, T self, Item destination, Item source, int numToTransfer) => orig(self, destination, source, numToTransfer);

    // ReforgePrice
    public delegate bool Orig_ReforgePrice(T self, Item item, ref int reforgePrice, ref bool canApplyDiscount);
    public virtual bool Detour_ReforgePrice(Orig_ReforgePrice orig, T self, Item item, ref int reforgePrice, ref bool canApplyDiscount) => orig(self, item, ref reforgePrice, ref canApplyDiscount);

    // CanReforge
    public delegate bool Orig_CanReforge(T self, Item item);
    public virtual bool Detour_CanReforge(Orig_CanReforge orig, T self, Item item) => orig(self, item);

    // PreReforge
    public delegate void Orig_PreReforge(T self, Item item);
    public virtual void Detour_PreReforge(Orig_PreReforge orig, T self, Item item) => orig(self, item);

    // PostReforge
    public delegate void Orig_PostReforge(T self, Item item);
    public virtual void Detour_PostReforge(Orig_PostReforge orig, T self, Item item) => orig(self, item);

    // DrawArmorColor
    public delegate void Orig_DrawArmorColor(T self, EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor);
    public virtual void Detour_DrawArmorColor(Orig_DrawArmorColor orig, T self, EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) => orig(self, type, slot, drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);

    // ArmorArmGlowMask
    public delegate void Orig_ArmorArmGlowMask(T self, int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color);
    public virtual void Detour_ArmorArmGlowMask(Orig_ArmorArmGlowMask orig, T self, int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) => orig(self, slot, drawPlayer, shadow, ref glowMask, ref color);

    // VerticalWingSpeeds
    public delegate void Orig_VerticalWingSpeeds(T self, Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend);
    public virtual void Detour_VerticalWingSpeeds(Orig_VerticalWingSpeeds orig, T self, Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) => orig(self, item, player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);

    // HorizontalWingSpeeds
    public delegate void Orig_HorizontalWingSpeeds(T self, Item item, Player player, ref float speed, ref float acceleration);
    public virtual void Detour_HorizontalWingSpeeds(Orig_HorizontalWingSpeeds orig, T self, Item item, Player player, ref float speed, ref float acceleration) => orig(self, item, player, ref speed, ref acceleration);

    // WingUpdate
    public delegate bool Orig_WingUpdate(T self, int wings, Player player, bool inUse);
    public virtual bool Detour_WingUpdate(Orig_WingUpdate orig, T self, int wings, Player player, bool inUse) => orig(self, wings, player, inUse);

    // Update
    public delegate void Orig_Update(T self, Item item, ref float gravity, ref float maxFallSpeed);
    public virtual void Detour_Update(Orig_Update orig, T self, Item item, ref float gravity, ref float maxFallSpeed) => orig(self, item, ref gravity, ref maxFallSpeed);

    // PostUpdate
    public delegate void Orig_PostUpdate(T self, Item item);
    public virtual void Detour_PostUpdate(Orig_PostUpdate orig, T self, Item item) => orig(self, item);

    // GrabRange
    public delegate void Orig_GrabRange(T self, Item item, Player player, ref int grabRange);
    public virtual void Detour_GrabRange(Orig_GrabRange orig, T self, Item item, Player player, ref int grabRange) => orig(self, item, player, ref grabRange);

    // GrabStyle
    public delegate bool Orig_GrabStyle(T self, Item item, Player player);
    public virtual bool Detour_GrabStyle(Orig_GrabStyle orig, T self, Item item, Player player) => orig(self, item, player);

    // CanPickup
    public delegate bool Orig_CanPickup(T self, Item item, Player player);
    public virtual bool Detour_CanPickup(Orig_CanPickup orig, T self, Item item, Player player) => orig(self, item, player);

    // OnPickup
    public delegate bool Orig_OnPickup(T self, Item item, Player player);
    public virtual bool Detour_OnPickup(Orig_OnPickup orig, T self, Item item, Player player) => orig(self, item, player);

    // ItemSpace
    public delegate bool Orig_ItemSpace(T self, Item item, Player player);
    public virtual bool Detour_ItemSpace(Orig_ItemSpace orig, T self, Item item, Player player) => orig(self, item, player);

    // GetAlpha
    public delegate Color? Orig_GetAlpha(T self, Item item, Color lightColor);
    public virtual Color? Detour_GetAlpha(Orig_GetAlpha orig, T self, Item item, Color lightColor) => orig(self, item, lightColor);

    // PreDrawInWorld
    public delegate bool Orig_PreDrawInWorld(T self, Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI);
    public virtual bool Detour_PreDrawInWorld(Orig_PreDrawInWorld orig, T self, Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => orig(self, item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

    // PostDrawInWorld
    public delegate void Orig_PostDrawInWorld(T self, Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI);
    public virtual void Detour_PostDrawInWorld(Orig_PostDrawInWorld orig, T self, Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => orig(self, item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);

    // PreDrawInInventory
    public delegate bool Orig_PreDrawInInventory(T self, Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    public virtual bool Detour_PreDrawInInventory(Orig_PreDrawInInventory orig, T self, Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => orig(self, item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

    // PostDrawInInventory
    public delegate void Orig_PostDrawInInventory(T self, Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale);
    public virtual void Detour_PostDrawInInventory(Orig_PostDrawInInventory orig, T self, Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) => orig(self, item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

    // HoldoutOffset
    public delegate Vector2? Orig_HoldoutOffset(T self, int type);
    public virtual Vector2? Detour_HoldoutOffset(Orig_HoldoutOffset orig, T self, int type) => orig(self, type);

    // HoldoutOrigin
    public delegate Vector2? Orig_HoldoutOrigin(T self, int type);
    public virtual Vector2? Detour_HoldoutOrigin(Orig_HoldoutOrigin orig, T self, int type) => orig(self, type);

    // CanEquipAccessory
    public delegate bool Orig_CanEquipAccessory(T self, Item item, Player player, int slot, bool modded);
    public virtual bool Detour_CanEquipAccessory(Orig_CanEquipAccessory orig, T self, Item item, Player player, int slot, bool modded) => orig(self, item, player, slot, modded);

    // CanAccessoryBeEquippedWith
    public delegate bool Orig_CanAccessoryBeEquippedWith(T self, Item equippedItem, Item incomingItem, Player player);
    public virtual bool Detour_CanAccessoryBeEquippedWith(Orig_CanAccessoryBeEquippedWith orig, T self, Item equippedItem, Item incomingItem, Player player) => orig(self, equippedItem, incomingItem, player);

    // ExtractinatorUse
    public delegate void Orig_ExtractinatorUse(T self, int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack);
    public virtual void Detour_ExtractinatorUse(Orig_ExtractinatorUse orig, T self, int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack) => orig(self, extractType, extractinatorBlockType, ref resultType, ref resultStack);

    // CaughtFishStack
    public delegate void Orig_CaughtFishStack(T self, int type, ref int stack);
    public virtual void Detour_CaughtFishStack(Orig_CaughtFishStack orig, T self, int type, ref int stack) => orig(self, type, ref stack);

    // IsAnglerQuestAvailable
    public delegate bool Orig_IsAnglerQuestAvailable(T self, int type);
    public virtual bool Detour_IsAnglerQuestAvailable(Orig_IsAnglerQuestAvailable orig, T self, int type) => orig(self, type);

    // AnglerChat
    public delegate void Orig_AnglerChat(T self, int type, ref string chat, ref string catchLocation);
    public virtual void Detour_AnglerChat(Orig_AnglerChat orig, T self, int type, ref string chat, ref string catchLocation) => orig(self, type, ref chat, ref catchLocation);

    // PreDrawTooltip
    public delegate bool Orig_PreDrawTooltip(T self, Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y);
    public virtual bool Detour_PreDrawTooltip(Orig_PreDrawTooltip orig, T self, Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) => orig(self, item, lines, ref x, ref y);

    // PostDrawTooltip
    public delegate void Orig_PostDrawTooltip(T self, Item item, ReadOnlyCollection<DrawableTooltipLine> lines);
    public virtual void Detour_PostDrawTooltip(Orig_PostDrawTooltip orig, T self, Item item, ReadOnlyCollection<DrawableTooltipLine> lines) => orig(self, item, lines);

    // PreDrawTooltipLine
    public delegate bool Orig_PreDrawTooltipLine(T self, Item item, DrawableTooltipLine line, ref int yOffset);
    public virtual bool Detour_PreDrawTooltipLine(Orig_PreDrawTooltipLine orig, T self, Item item, DrawableTooltipLine line, ref int yOffset) => orig(self, item, line, ref yOffset);

    // PostDrawTooltipLine
    public delegate void Orig_PostDrawTooltipLine(T self, Item item, DrawableTooltipLine line);
    public virtual void Detour_PostDrawTooltipLine(Orig_PostDrawTooltipLine orig, T self, Item item, DrawableTooltipLine line) => orig(self, item, line);

    // ModifyTooltips
    public delegate void Orig_ModifyTooltips(T self, Item item, List<TooltipLine> tooltips);
    public virtual void Detour_ModifyTooltips(Orig_ModifyTooltips orig, T self, Item item, List<TooltipLine> tooltips) => orig(self, item, tooltips);

    // SaveData
    public delegate void Orig_SaveData(T self, Item item, TagCompound tag);
    public virtual void Detour_SaveData(Orig_SaveData orig, T self, Item item, TagCompound tag) => orig(self, item, tag);

    // LoadData
    public delegate void Orig_LoadData(T self, Item item, TagCompound tag);
    public virtual void Detour_LoadData(Orig_LoadData orig, T self, Item item, TagCompound tag) => orig(self, item, tag);

    // NetSend
    public delegate void Orig_NetSend(T self, Item item, BinaryWriter writer);
    public virtual void Detour_NetSend(Orig_NetSend orig, T self, Item item, BinaryWriter writer) => orig(self, item, writer);

    // NetReceive
    public delegate void Orig_NetReceive(T self, Item item, BinaryReader reader);
    public virtual void Detour_NetReceive(Orig_NetReceive orig, T self, Item item, BinaryReader reader) => orig(self, item, reader);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_OnCreated);
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_ChoosePrefix);
        TryApplyDetour(Detour_PrefixChance);
        TryApplyDetour(Detour_AllowPrefix);
        TryApplyDetour(Detour_CanUseItem);
        TryApplyDetour(Detour_CanAutoReuseItem);
        TryApplyDetour(Detour_UseStyle);
        TryApplyDetour(Detour_HoldStyle);
        TryApplyDetour(Detour_HoldItem);
        TryApplyDetour(Detour_UseTimeMultiplier);
        TryApplyDetour(Detour_UseAnimationMultiplier);
        TryApplyDetour(Detour_UseSpeedMultiplier);
        TryApplyDetour(Detour_GetHealLife);
        TryApplyDetour(Detour_GetHealMana);
        TryApplyDetour(Detour_ModifyManaCost);
        TryApplyDetour(Detour_OnMissingMana);
        TryApplyDetour(Detour_OnConsumeMana);
        TryApplyDetour(Detour_ModifyWeaponDamage);
        TryApplyDetour(Detour_ModifyResearchSorting);
        TryApplyDetour(Detour_CanConsumeBait);
        TryApplyDetour(Detour_CanResearch);
        TryApplyDetour(Detour_OnResearched);
        TryApplyDetour(Detour_ModifyWeaponKnockback);
        TryApplyDetour(Detour_ModifyWeaponCrit);
        TryApplyDetour(Detour_NeedsAmmo);
        TryApplyDetour(Detour_PickAmmo);
        TryApplyDetour(Detour_CanChooseAmmo);
        TryApplyDetour(Detour_CanBeChosenAsAmmo);
        TryApplyDetour(Detour_CanConsumeAmmo);
        TryApplyDetour(Detour_CanBeConsumedAsAmmo);
        TryApplyDetour(Detour_OnConsumeAmmo);
        TryApplyDetour(Detour_OnConsumedAsAmmo);
        TryApplyDetour(Detour_CanShoot);
        TryApplyDetour(Detour_ModifyShootStats);
        TryApplyDetour(Detour_Shoot);
        TryApplyDetour(Detour_UseItemHitbox);
        TryApplyDetour(Detour_MeleeEffects);
        TryApplyDetour(Detour_CanCatchNPC);
        TryApplyDetour(Detour_OnCatchNPC);
        TryApplyDetour(Detour_ModifyItemScale);
        TryApplyDetour(Detour_CanHitNPC);
        TryApplyDetour(Detour_CanMeleeAttackCollideWithNPC);
        TryApplyDetour(Detour_ModifyHitNPC);
        TryApplyDetour(Detour_OnHitNPC);
        TryApplyDetour(Detour_CanHitPvp);
        TryApplyDetour(Detour_ModifyHitPvp);
        TryApplyDetour(Detour_OnHitPvp);
        TryApplyDetour(Detour_UseItem);
        TryApplyDetour(Detour_UseAnimation);
        TryApplyDetour(Detour_ConsumeItem);
        TryApplyDetour(Detour_OnConsumeItem);
        TryApplyDetour(Detour_UseItemFrame);
        TryApplyDetour(Detour_HoldItemFrame);
        TryApplyDetour(Detour_AltFunctionUse);
        TryApplyDetour(Detour_UpdateInventory);
        TryApplyDetour(Detour_UpdateInfoAccessory);
        TryApplyDetour(Detour_UpdateEquip);
        TryApplyDetour(Detour_UpdateAccessory);
        TryApplyDetour(Detour_UpdateVanity);
        TryApplyDetour(Detour_IsArmorSet);
        TryApplyDetour(Detour_UpdateArmorSet);
        TryApplyDetour(Detour_IsVanitySet);
        TryApplyDetour(Detour_PreUpdateVanitySet);
        TryApplyDetour(Detour_UpdateVanitySet);
        TryApplyDetour(Detour_ArmorSetShadows);
        TryApplyDetour(Detour_SetMatch);
        TryApplyDetour(Detour_CanRightClick);
        TryApplyDetour(Detour_RightClick);
        TryApplyDetour(Detour_ModifyItemLoot);
        TryApplyDetour(Detour_CanStack);
        TryApplyDetour(Detour_CanStackInWorld);
        TryApplyDetour(Detour_OnStack);
        TryApplyDetour(Detour_SplitStack);
        TryApplyDetour(Detour_ReforgePrice);
        TryApplyDetour(Detour_CanReforge);
        TryApplyDetour(Detour_PreReforge);
        TryApplyDetour(Detour_PostReforge);
        TryApplyDetour(Detour_DrawArmorColor);
        TryApplyDetour(Detour_ArmorArmGlowMask);
        TryApplyDetour(Detour_VerticalWingSpeeds);
        TryApplyDetour(Detour_HorizontalWingSpeeds);
        TryApplyDetour(Detour_WingUpdate);
        TryApplyDetour(Detour_Update);
        TryApplyDetour(Detour_PostUpdate);
        TryApplyDetour(Detour_GrabRange);
        TryApplyDetour(Detour_GrabStyle);
        TryApplyDetour(Detour_CanPickup);
        TryApplyDetour(Detour_OnPickup);
        TryApplyDetour(Detour_ItemSpace);
        TryApplyDetour(Detour_GetAlpha);
        TryApplyDetour(Detour_PreDrawInWorld);
        TryApplyDetour(Detour_PostDrawInWorld);
        TryApplyDetour(Detour_PreDrawInInventory);
        TryApplyDetour(Detour_PostDrawInInventory);
        TryApplyDetour(Detour_HoldoutOffset);
        TryApplyDetour(Detour_HoldoutOrigin);
        TryApplyDetour(Detour_CanEquipAccessory);
        TryApplyDetour(Detour_CanAccessoryBeEquippedWith);
        TryApplyDetour(Detour_ExtractinatorUse);
        TryApplyDetour(Detour_CaughtFishStack);
        TryApplyDetour(Detour_IsAnglerQuestAvailable);
        TryApplyDetour(Detour_AnglerChat);
        TryApplyDetour(Detour_PreDrawTooltip);
        TryApplyDetour(Detour_PostDrawTooltip);
        TryApplyDetour(Detour_PreDrawTooltipLine);
        TryApplyDetour(Detour_PostDrawTooltipLine);
        TryApplyDetour(Detour_ModifyTooltips);
        TryApplyDetour(Detour_SaveData);
        TryApplyDetour(Detour_LoadData);
        TryApplyDetour(Detour_NetSend);
        TryApplyDetour(Detour_NetReceive);
    }
}

public abstract class GlobalNPCDetour<T> : GlobalTypeDetour<NPC, GlobalNPC, T> where T : GlobalNPC
{
    public override bool Detour_AppliesToEntity(Orig_AppliesToEntity orig, T self, NPC npc, bool lateInstantiation) => base.Detour_AppliesToEntity(orig, self, npc, lateInstantiation);

    public override void Detour_SetDefaults(Orig_SetDefaults orig, T self, NPC npc) => base.Detour_SetDefaults(orig, self, npc);

    // SetDefaultsFromNetId
    public delegate void Orig_SetDefaultsFromNetId(T self, NPC npc);
    public virtual void Detour_SetDefaultsFromNetId(Orig_SetDefaultsFromNetId orig, T self, NPC npc) => orig(self, npc);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self, NPC npc, IEntitySource source);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, NPC npc, IEntitySource source) => orig(self, npc, source);

    // ApplyDifficultyAndPlayerScaling
    public delegate void Orig_ApplyDifficultyAndPlayerScaling(T self, NPC npc, int numPlayers, float balance, float bossAdjustment);
    public virtual void Detour_ApplyDifficultyAndPlayerScaling(Orig_ApplyDifficultyAndPlayerScaling orig, T self, NPC npc, int numPlayers, float balance, float bossAdjustment) => orig(self, npc, numPlayers, balance, bossAdjustment);

    // SetBestiary
    public delegate void Orig_SetBestiary(T self, NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry);
    public virtual void Detour_SetBestiary(Orig_SetBestiary orig, T self, NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) => orig(self, npc, database, bestiaryEntry);

    // ModifyTypeName
    public delegate void Orig_ModifyTypeName(T self, NPC npc, ref string typeName);
    public virtual void Detour_ModifyTypeName(Orig_ModifyTypeName orig, T self, NPC npc, ref string typeName) => orig(self, npc, ref typeName);

    // ModifyHoverBoundingBox
    public delegate void Orig_ModifyHoverBoundingBox(T self, NPC npc, ref Rectangle boundingBox);
    public virtual void Detour_ModifyHoverBoundingBox(Orig_ModifyHoverBoundingBox orig, T self, NPC npc, ref Rectangle boundingBox) => orig(self, npc, ref boundingBox);

    // ModifyTownNPCProfile
    public delegate ITownNPCProfile Orig_ModifyTownNPCProfile(T self, NPC npc);
    public virtual ITownNPCProfile Detour_ModifyTownNPCProfile(Orig_ModifyTownNPCProfile orig, T self, NPC npc) => orig(self, npc);

    // ModifyNPCNameList
    public delegate void Orig_ModifyNPCNameList(T self, NPC npc, List<string> nameList);
    public virtual void Detour_ModifyNPCNameList(Orig_ModifyNPCNameList orig, T self, NPC npc, List<string> nameList) => orig(self, npc, nameList);

    // ResetEffects
    public delegate void Orig_ResetEffects(T self, NPC npc);
    public virtual void Detour_ResetEffects(Orig_ResetEffects orig, T self, NPC npc) => orig(self, npc);

    // PreAI
    public delegate bool Orig_PreAI(T self, NPC npc);
    public virtual bool Detour_PreAI(Orig_PreAI orig, T self, NPC npc) => orig(self, npc);

    // AI
    public delegate void Orig_AI(T self, NPC npc);
    public virtual void Detour_AI(Orig_AI orig, T self, NPC npc) => orig(self, npc);

    // PostAI
    public delegate void Orig_PostAI(T self, NPC npc);
    public virtual void Detour_PostAI(Orig_PostAI orig, T self, NPC npc) => orig(self, npc);

    // SendExtraAI
    public delegate void Orig_SendExtraAI(T self, NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter);
    public virtual void Detour_SendExtraAI(Orig_SendExtraAI orig, T self, NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) => orig(self, npc, bitWriter, binaryWriter);

    // ReceiveExtraAI
    public delegate void Orig_ReceiveExtraAI(T self, NPC npc, BitReader bitReader, BinaryReader binaryReader);
    public virtual void Detour_ReceiveExtraAI(Orig_ReceiveExtraAI orig, T self, NPC npc, BitReader bitReader, BinaryReader binaryReader) => orig(self, npc, bitReader, binaryReader);

    // FindFrame
    public delegate void Orig_FindFrame(T self, NPC npc, int frameHeight);
    public virtual void Detour_FindFrame(Orig_FindFrame orig, T self, NPC npc, int frameHeight) => orig(self, npc, frameHeight);

    // HitEffect
    public delegate void Orig_HitEffect(T self, NPC npc, NPC.HitInfo hit);
    public virtual void Detour_HitEffect(Orig_HitEffect orig, T self, NPC npc, NPC.HitInfo hit) => orig(self, npc, hit);

    // UpdateLifeRegen
    public delegate void Orig_UpdateLifeRegen(T self, NPC npc, ref int damage);
    public virtual void Detour_UpdateLifeRegen(Orig_UpdateLifeRegen orig, T self, NPC npc, ref int damage) => orig(self, npc, ref damage);

    // CheckActive
    public delegate bool Orig_CheckActive(T self, NPC npc);
    public virtual bool Detour_CheckActive(Orig_CheckActive orig, T self, NPC npc) => orig(self, npc);

    // CheckDead
    public delegate bool Orig_CheckDead(T self, NPC npc);
    public virtual bool Detour_CheckDead(Orig_CheckDead orig, T self, NPC npc) => orig(self, npc);

    // SpecialOnKill
    public delegate bool Orig_SpecialOnKill(T self, NPC npc);
    public virtual bool Detour_SpecialOnKill(Orig_SpecialOnKill orig, T self, NPC npc) => orig(self, npc);

    // PreKill
    public delegate bool Orig_PreKill(T self, NPC npc);
    public virtual bool Detour_PreKill(Orig_PreKill orig, T self, NPC npc) => orig(self, npc);

    // OnKill
    public delegate void Orig_OnKill(T self, NPC npc);
    public virtual void Detour_OnKill(Orig_OnKill orig, T self, NPC npc) => orig(self, npc);

    // CanFallThroughPlatforms
    public delegate bool? Orig_CanFallThroughPlatforms(T self, NPC npc);
    public virtual bool? Detour_CanFallThroughPlatforms(Orig_CanFallThroughPlatforms orig, T self, NPC npc) => orig(self, npc);

    // CanBeCaughtBy
    public delegate bool? Orig_CanBeCaughtBy(T self, NPC npc, Item item, Player player);
    public virtual bool? Detour_CanBeCaughtBy(Orig_CanBeCaughtBy orig, T self, NPC npc, Item item, Player player) => orig(self, npc, item, player);

    // OnCaughtBy
    public delegate void Orig_OnCaughtBy(T self, NPC npc, Player player, Item item, bool failed);
    public virtual void Detour_OnCaughtBy(Orig_OnCaughtBy orig, T self, NPC npc, Player player, Item item, bool failed) => orig(self, npc, player, item, failed);

    // ModifyNPCLoot
    public delegate void Orig_ModifyNPCLoot(T self, NPC npc, NPCLoot npcLoot);
    public virtual void Detour_ModifyNPCLoot(Orig_ModifyNPCLoot orig, T self, NPC npc, NPCLoot npcLoot) => orig(self, npc, npcLoot);

    // ModifyGlobalLoot
    public delegate void Orig_ModifyGlobalLoot(T self, GlobalLoot globalLoot);
    public virtual void Detour_ModifyGlobalLoot(Orig_ModifyGlobalLoot orig, T self, GlobalLoot globalLoot) => orig(self, globalLoot);

    // CanHitPlayer
    public delegate bool Orig_CanHitPlayer(T self, NPC npc, Player target, ref int cooldownSlot);
    public virtual bool Detour_CanHitPlayer(Orig_CanHitPlayer orig, T self, NPC npc, Player target, ref int cooldownSlot) => orig(self, npc, target, ref cooldownSlot);

    // ModifyHitPlayer
    public delegate void Orig_ModifyHitPlayer(T self, NPC npc, Player target, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHitPlayer(Orig_ModifyHitPlayer orig, T self, NPC npc, Player target, ref Player.HurtModifiers modifiers) => orig(self, npc, target, ref modifiers);

    // OnHitPlayer
    public delegate void Orig_OnHitPlayer(T self, NPC npc, Player target, Player.HurtInfo hurtInfo);
    public virtual void Detour_OnHitPlayer(Orig_OnHitPlayer orig, T self, NPC npc, Player target, Player.HurtInfo hurtInfo) => orig(self, npc, target, hurtInfo);

    // CanHitNPC
    public delegate bool Orig_CanHitNPC(T self, NPC npc, NPC target);
    public virtual bool Detour_CanHitNPC(Orig_CanHitNPC orig, T self, NPC npc, NPC target) => orig(self, npc, target);

    // CanBeHitByNPC
    public delegate bool Orig_CanBeHitByNPC(T self, NPC npc, NPC attacker);
    public virtual bool Detour_CanBeHitByNPC(Orig_CanBeHitByNPC orig, T self, NPC npc, NPC attacker) => orig(self, npc, attacker);

    // ModifyHitNPC
    public delegate void Orig_ModifyHitNPC(T self, NPC npc, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, T self, NPC npc, NPC target, ref NPC.HitModifiers modifiers) => orig(self, npc, target, ref modifiers);

    // OnHitNPC
    public delegate void Orig_OnHitNPC(T self, NPC npc, NPC target, NPC.HitInfo hit);
    public virtual void Detour_OnHitNPC(Orig_OnHitNPC orig, T self, NPC npc, NPC target, NPC.HitInfo hit) => orig(self, npc, target, hit);

    // CanBeHitByItem
    public delegate bool? Orig_CanBeHitByItem(T self, NPC npc, Player player, Item item);
    public virtual bool? Detour_CanBeHitByItem(Orig_CanBeHitByItem orig, T self, NPC npc, Player player, Item item) => orig(self, npc, player, item);

    // CanCollideWithPlayerMeleeAttack
    public delegate bool? Orig_CanCollideWithPlayerMeleeAttack(T self, NPC npc, Player player, Item item, Rectangle meleeAttackHitbox);
    public virtual bool? Detour_CanCollideWithPlayerMeleeAttack(Orig_CanCollideWithPlayerMeleeAttack orig, T self, NPC npc, Player player, Item item, Rectangle meleeAttackHitbox) => orig(self, npc, player, item, meleeAttackHitbox);

    // ModifyHitByItem
    public delegate void Orig_ModifyHitByItem(T self, NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitByItem(Orig_ModifyHitByItem orig, T self, NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) => orig(self, npc, player, item, ref modifiers);

    // OnHitByItem
    public delegate void Orig_OnHitByItem(T self, NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitByItem(Orig_OnHitByItem orig, T self, NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) => orig(self, npc, player, item, hit, damageDone);

    // CanBeHitByProjectile
    public delegate bool? Orig_CanBeHitByProjectile(T self, NPC npc, Projectile projectile);
    public virtual bool? Detour_CanBeHitByProjectile(Orig_CanBeHitByProjectile orig, T self, NPC npc, Projectile projectile) => orig(self, npc, projectile);

    // ModifyHitByProjectile
    public delegate void Orig_ModifyHitByProjectile(T self, NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitByProjectile(Orig_ModifyHitByProjectile orig, T self, NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) => orig(self, npc, projectile, ref modifiers);

    // OnHitByProjectile
    public delegate void Orig_OnHitByProjectile(T self, NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitByProjectile(Orig_OnHitByProjectile orig, T self, NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) => orig(self, npc, projectile, hit, damageDone);

    // ModifyIncomingHit
    public delegate void Orig_ModifyIncomingHit(T self, NPC npc, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyIncomingHit(Orig_ModifyIncomingHit orig, T self, NPC npc, ref NPC.HitModifiers modifiers) => orig(self, npc, ref modifiers);

    // BossHeadSlot
    public delegate void Orig_BossHeadSlot(T self, NPC npc, ref int index);
    public virtual void Detour_BossHeadSlot(Orig_BossHeadSlot orig, T self, NPC npc, ref int index) => orig(self, npc, ref index);

    // BossHeadRotation
    public delegate void Orig_BossHeadRotation(T self, NPC npc, ref float rotation);
    public virtual void Detour_BossHeadRotation(Orig_BossHeadRotation orig, T self, NPC npc, ref float rotation) => orig(self, npc, ref rotation);

    // BossHeadSpriteEffects
    public delegate void Orig_BossHeadSpriteEffects(T self, NPC npc, ref SpriteEffects spriteEffects);
    public virtual void Detour_BossHeadSpriteEffects(Orig_BossHeadSpriteEffects orig, T self, NPC npc, ref SpriteEffects spriteEffects) => orig(self, npc, ref spriteEffects);

    // GetAlpha
    public delegate Color? Orig_GetAlpha(T self, NPC npc, Color drawColor);
    public virtual Color? Detour_GetAlpha(Orig_GetAlpha orig, T self, NPC npc, Color drawColor) => orig(self, npc, drawColor);

    // DrawEffects
    public delegate void Orig_DrawEffects(T self, NPC npc, ref Color drawColor);
    public virtual void Detour_DrawEffects(Orig_DrawEffects orig, T self, NPC npc, ref Color drawColor) => orig(self, npc, ref drawColor);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => orig(self, npc, spriteBatch, screenPos, drawColor);

    // PostDraw
    public delegate void Orig_PostDraw(T self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => orig(self, npc, spriteBatch, screenPos, drawColor);

    // DrawBehind
    public delegate void Orig_DrawBehind(T self, NPC npc, int index);
    public virtual void Detour_DrawBehind(Orig_DrawBehind orig, T self, NPC npc, int index) => orig(self, npc, index);

    // DrawHealthBar
    public delegate bool? Orig_DrawHealthBar(T self, NPC npc, byte hbPosition, ref float scale, ref Vector2 position);
    public virtual bool? Detour_DrawHealthBar(Orig_DrawHealthBar orig, T self, NPC npc, byte hbPosition, ref float scale, ref Vector2 position) => orig(self, npc, hbPosition, ref scale, ref position);

    // EditSpawnRate
    public delegate void Orig_EditSpawnRate(T self, Player player, ref int spawnRate, ref int maxSpawns);
    public virtual void Detour_EditSpawnRate(Orig_EditSpawnRate orig, T self, Player player, ref int spawnRate, ref int maxSpawns) => orig(self, player, ref spawnRate, ref maxSpawns);

    // EditSpawnRange
    public delegate void Orig_EditSpawnRange(T self, Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY);
    public virtual void Detour_EditSpawnRange(Orig_EditSpawnRange orig, T self, Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY) => orig(self, player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);

    // EditSpawnPool
    public delegate void Orig_EditSpawnPool(T self, IDictionary<int, float> pool, NPCSpawnInfo spawnInfo);
    public virtual void Detour_EditSpawnPool(Orig_EditSpawnPool orig, T self, IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) => orig(self, pool, spawnInfo);

    // SpawnNPC
    public delegate void Orig_SpawnNPC(T self, int npc, int tileX, int tileY);
    public virtual void Detour_SpawnNPC(Orig_SpawnNPC orig, T self, int npc, int tileX, int tileY) => orig(self, npc, tileX, tileY);

    // CanChat
    public delegate bool? Orig_CanChat(T self, NPC npc);
    public virtual bool? Detour_CanChat(Orig_CanChat orig, T self, NPC npc) => orig(self, npc);

    // GetChat
    public delegate void Orig_GetChat(T self, NPC npc, ref string chat);
    public virtual void Detour_GetChat(Orig_GetChat orig, T self, NPC npc, ref string chat) => orig(self, npc, ref chat);

    // PreChatButtonClicked
    public delegate bool Orig_PreChatButtonClicked(T self, NPC npc, bool firstButton);
    public virtual bool Detour_PreChatButtonClicked(Orig_PreChatButtonClicked orig, T self, NPC npc, bool firstButton) => orig(self, npc, firstButton);

    // OnChatButtonClicked
    public delegate void Orig_OnChatButtonClicked(T self, NPC npc, bool firstButton);
    public virtual void Detour_OnChatButtonClicked(Orig_OnChatButtonClicked orig, T self, NPC npc, bool firstButton) => orig(self, npc, firstButton);

    // ModifyShop
    public delegate void Orig_ModifyShop(T self, NPCShop shop);
    public virtual void Detour_ModifyShop(Orig_ModifyShop orig, T self, NPCShop shop) => orig(self, shop);

    // ModifyActiveShop
    public delegate void Orig_ModifyActiveShop(T self, NPC npc, string shopName, Item[] items);
    public virtual void Detour_ModifyActiveShop(Orig_ModifyActiveShop orig, T self, NPC npc, string shopName, Item[] items) => orig(self, npc, shopName, items);

    // SetupTravelShop
    public delegate void Orig_SetupTravelShop(T self, int[] shop, ref int nextSlot);
    public virtual void Detour_SetupTravelShop(Orig_SetupTravelShop orig, T self, int[] shop, ref int nextSlot) => orig(self, shop, ref nextSlot);

    // CanGoToStatue
    public delegate bool? Orig_CanGoToStatue(T self, NPC npc, bool toKingStatue);
    public virtual bool? Detour_CanGoToStatue(Orig_CanGoToStatue orig, T self, NPC npc, bool toKingStatue) => orig(self, npc, toKingStatue);

    // OnGoToStatue
    public delegate void Orig_OnGoToStatue(T self, NPC npc, bool toKingStatue);
    public virtual void Detour_OnGoToStatue(Orig_OnGoToStatue orig, T self, NPC npc, bool toKingStatue) => orig(self, npc, toKingStatue);

    // BuffTownNPC
    public delegate void Orig_BuffTownNPC(T self, ref float damageMult, ref int defense);
    public virtual void Detour_BuffTownNPC(Orig_BuffTownNPC orig, T self, ref float damageMult, ref int defense) => orig(self, ref damageMult, ref defense);

    // TownNPCAttackStrength
    public delegate void Orig_TownNPCAttackStrength(T self, NPC npc, ref int damage, ref float knockback);
    public virtual void Detour_TownNPCAttackStrength(Orig_TownNPCAttackStrength orig, T self, NPC npc, ref int damage, ref float knockback) => orig(self, npc, ref damage, ref knockback);

    // TownNPCAttackCooldown
    public delegate void Orig_TownNPCAttackCooldown(T self, NPC npc, ref int cooldown, ref int randExtraCooldown);
    public virtual void Detour_TownNPCAttackCooldown(Orig_TownNPCAttackCooldown orig, T self, NPC npc, ref int cooldown, ref int randExtraCooldown) => orig(self, npc, ref cooldown, ref randExtraCooldown);

    // TownNPCAttackProj
    public delegate void Orig_TownNPCAttackProj(T self, NPC npc, ref int projType, ref int attackDelay);
    public virtual void Detour_TownNPCAttackProj(Orig_TownNPCAttackProj orig, T self, NPC npc, ref int projType, ref int attackDelay) => orig(self, npc, ref projType, ref attackDelay);

    // TownNPCAttackProjSpeed
    public delegate void Orig_TownNPCAttackProjSpeed(T self, NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset);
    public virtual void Detour_TownNPCAttackProjSpeed(Orig_TownNPCAttackProjSpeed orig, T self, NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset) => orig(self, npc, ref multiplier, ref gravityCorrection, ref randomOffset);

    // TownNPCAttackShoot
    public delegate void Orig_TownNPCAttackShoot(T self, NPC npc, ref bool inBetweenShots);
    public virtual void Detour_TownNPCAttackShoot(Orig_TownNPCAttackShoot orig, T self, NPC npc, ref bool inBetweenShots) => orig(self, npc, ref inBetweenShots);

    // TownNPCAttackMagic
    public delegate void Orig_TownNPCAttackMagic(T self, NPC npc, ref float auraLightMultiplier);
    public virtual void Detour_TownNPCAttackMagic(Orig_TownNPCAttackMagic orig, T self, NPC npc, ref float auraLightMultiplier) => orig(self, npc, ref auraLightMultiplier);

    // TownNPCAttackSwing
    public delegate void Orig_TownNPCAttackSwing(T self, NPC npc, ref int itemWidth, ref int itemHeight);
    public virtual void Detour_TownNPCAttackSwing(Orig_TownNPCAttackSwing orig, T self, NPC npc, ref int itemWidth, ref int itemHeight) => orig(self, npc, ref itemWidth, ref itemHeight);

    // DrawTownAttackGun
    public delegate void Orig_DrawTownAttackGun(T self, NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset);
    public virtual void Detour_DrawTownAttackGun(Orig_DrawTownAttackGun orig, T self, NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) => orig(self, npc, ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);

    // DrawTownAttackSwing
    public delegate void Orig_DrawTownAttackSwing(T self, NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset);
    public virtual void Detour_DrawTownAttackSwing(Orig_DrawTownAttackSwing orig, T self, NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) => orig(self, npc, ref item, ref itemFrame, ref itemSize, ref scale, ref offset);

    // ModifyCollisionData
    public delegate bool Orig_ModifyCollisionData(T self, NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox);
    public virtual bool Detour_ModifyCollisionData(Orig_ModifyCollisionData orig, T self, NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => orig(self, npc, victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);

    // NeedSaving
    public delegate bool Orig_NeedSaving(T self, NPC npc);
    public virtual bool Detour_NeedSaving(Orig_NeedSaving orig, T self, NPC npc) => orig(self, npc);

    // SaveData
    public delegate void Orig_SaveData(T self, NPC npc, TagCompound tag);
    public virtual void Detour_SaveData(Orig_SaveData orig, T self, NPC npc, TagCompound tag) => orig(self, npc, tag);

    // LoadData
    public delegate void Orig_LoadData(T self, NPC npc, TagCompound tag);
    public virtual void Detour_LoadData(Orig_LoadData orig, T self, NPC npc, TagCompound tag) => orig(self, npc, tag);

    // PickEmote
    public delegate int? Orig_PickEmote(T self, NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor);
    public virtual int? Detour_PickEmote(Orig_PickEmote orig, T self, NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => orig(self, npc, closestPlayer, emoteList, otherAnchor);

    // ChatBubblePosition
    public delegate void Orig_ChatBubblePosition(T self, NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects);
    public virtual void Detour_ChatBubblePosition(Orig_ChatBubblePosition orig, T self, NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) => orig(self, npc, ref position, ref spriteEffects);

    // PartyHatPosition
    public delegate void Orig_PartyHatPosition(T self, NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects);
    public virtual void Detour_PartyHatPosition(Orig_PartyHatPosition orig, T self, NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) => orig(self, npc, ref position, ref spriteEffects);

    // EmoteBubblePosition
    public delegate void Orig_EmoteBubblePosition(T self, NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects);
    public virtual void Detour_EmoteBubblePosition(Orig_EmoteBubblePosition orig, T self, NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) => orig(self, npc, ref position, ref spriteEffects);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_SetDefaultsFromNetId);
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_ApplyDifficultyAndPlayerScaling);
        TryApplyDetour(Detour_SetBestiary);
        TryApplyDetour(Detour_ModifyTypeName);
        TryApplyDetour(Detour_ModifyHoverBoundingBox);
        TryApplyDetour(Detour_ModifyTownNPCProfile);
        TryApplyDetour(Detour_ModifyNPCNameList);
        TryApplyDetour(Detour_ResetEffects);
        TryApplyDetour(Detour_PreAI);
        TryApplyDetour(Detour_AI);
        TryApplyDetour(Detour_PostAI);
        TryApplyDetour(Detour_SendExtraAI);
        TryApplyDetour(Detour_ReceiveExtraAI);
        TryApplyDetour(Detour_FindFrame);
        TryApplyDetour(Detour_HitEffect);
        TryApplyDetour(Detour_UpdateLifeRegen);
        TryApplyDetour(Detour_CheckActive);
        TryApplyDetour(Detour_CheckDead);
        TryApplyDetour(Detour_SpecialOnKill);
        TryApplyDetour(Detour_PreKill);
        TryApplyDetour(Detour_OnKill);
        TryApplyDetour(Detour_CanFallThroughPlatforms);
        TryApplyDetour(Detour_CanBeCaughtBy);
        TryApplyDetour(Detour_OnCaughtBy);
        TryApplyDetour(Detour_ModifyNPCLoot);
        TryApplyDetour(Detour_ModifyGlobalLoot);
        TryApplyDetour(Detour_CanHitPlayer);
        TryApplyDetour(Detour_ModifyHitPlayer);
        TryApplyDetour(Detour_OnHitPlayer);
        TryApplyDetour(Detour_CanHitNPC);
        TryApplyDetour(Detour_CanBeHitByNPC);
        TryApplyDetour(Detour_ModifyHitNPC);
        TryApplyDetour(Detour_OnHitNPC);
        TryApplyDetour(Detour_CanBeHitByItem);
        TryApplyDetour(Detour_CanCollideWithPlayerMeleeAttack);
        TryApplyDetour(Detour_ModifyHitByItem);
        TryApplyDetour(Detour_OnHitByItem);
        TryApplyDetour(Detour_CanBeHitByProjectile);
        TryApplyDetour(Detour_ModifyHitByProjectile);
        TryApplyDetour(Detour_OnHitByProjectile);
        TryApplyDetour(Detour_ModifyIncomingHit);
        TryApplyDetour(Detour_BossHeadSlot);
        TryApplyDetour(Detour_BossHeadRotation);
        TryApplyDetour(Detour_BossHeadSpriteEffects);
        TryApplyDetour(Detour_GetAlpha);
        TryApplyDetour(Detour_DrawEffects);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_DrawBehind);
        TryApplyDetour(Detour_DrawHealthBar);
        TryApplyDetour(Detour_EditSpawnRate);
        TryApplyDetour(Detour_EditSpawnRange);
        TryApplyDetour(Detour_EditSpawnPool);
        TryApplyDetour(Detour_SpawnNPC);
        TryApplyDetour(Detour_CanChat);
        TryApplyDetour(Detour_GetChat);
        TryApplyDetour(Detour_PreChatButtonClicked);
        TryApplyDetour(Detour_OnChatButtonClicked);
        TryApplyDetour(Detour_ModifyShop);
        TryApplyDetour(Detour_ModifyActiveShop);
        TryApplyDetour(Detour_SetupTravelShop);
        TryApplyDetour(Detour_CanGoToStatue);
        TryApplyDetour(Detour_OnGoToStatue);
        TryApplyDetour(Detour_BuffTownNPC);
        TryApplyDetour(Detour_TownNPCAttackStrength);
        TryApplyDetour(Detour_TownNPCAttackCooldown);
        TryApplyDetour(Detour_TownNPCAttackProj);
        TryApplyDetour(Detour_TownNPCAttackProjSpeed);
        TryApplyDetour(Detour_TownNPCAttackShoot);
        TryApplyDetour(Detour_TownNPCAttackMagic);
        TryApplyDetour(Detour_TownNPCAttackSwing);
        TryApplyDetour(Detour_DrawTownAttackGun);
        TryApplyDetour(Detour_DrawTownAttackSwing);
        TryApplyDetour(Detour_ModifyCollisionData);
        TryApplyDetour(Detour_NeedSaving);
        TryApplyDetour(Detour_SaveData);
        TryApplyDetour(Detour_LoadData);
        TryApplyDetour(Detour_PickEmote);
        TryApplyDetour(Detour_ChatBubblePosition);
        TryApplyDetour(Detour_PartyHatPosition);
        TryApplyDetour(Detour_EmoteBubblePosition);
    }
}

public abstract class GlobalProjectileDetour<T> : GlobalTypeDetour<Projectile, GlobalProjectile, T> where T : GlobalProjectile
{
    public override bool Detour_AppliesToEntity(Orig_AppliesToEntity orig, T self, Projectile projectile, bool lateInstantiation) => base.Detour_AppliesToEntity(orig, self, projectile, lateInstantiation);

    public override void Detour_SetDefaults(Orig_SetDefaults orig, T self, Projectile projectile) => base.Detour_SetDefaults(orig, self, projectile);

    // OnSpawn
    public delegate void Orig_OnSpawn(T self, Projectile projectile, IEntitySource source);
    public virtual void Detour_OnSpawn(Orig_OnSpawn orig, T self, Projectile projectile, IEntitySource source) => orig(self, projectile, source);

    // PreAI
    public delegate bool Orig_PreAI(T self, Projectile projectile);
    public virtual bool Detour_PreAI(Orig_PreAI orig, T self, Projectile projectile) => orig(self, projectile);

    // AI
    public delegate void Orig_AI(T self, Projectile projectile);
    public virtual void Detour_AI(Orig_AI orig, T self, Projectile projectile) => orig(self, projectile);

    // PostAI
    public delegate void Orig_PostAI(T self, Projectile projectile);
    public virtual void Detour_PostAI(Orig_PostAI orig, T self, Projectile projectile) => orig(self, projectile);

    // SendExtraAI
    public delegate void Orig_SendExtraAI(T self, Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter);
    public virtual void Detour_SendExtraAI(Orig_SendExtraAI orig, T self, Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) => orig(self, projectile, bitWriter, binaryWriter);

    // ReceiveExtraAI
    public delegate void Orig_ReceiveExtraAI(T self, Projectile projectile, BitReader bitReader, BinaryReader binaryReader);
    public virtual void Detour_ReceiveExtraAI(Orig_ReceiveExtraAI orig, T self, Projectile projectile, BitReader bitReader, BinaryReader binaryReader) => orig(self, projectile, bitReader, binaryReader);

    // ShouldUpdatePosition
    public delegate bool Orig_ShouldUpdatePosition(T self, Projectile projectile);
    public virtual bool Detour_ShouldUpdatePosition(Orig_ShouldUpdatePosition orig, T self, Projectile projectile) => orig(self, projectile);

    // TileCollideStyle
    public delegate bool Orig_TileCollideStyle(T self, Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac);
    public virtual bool Detour_TileCollideStyle(Orig_TileCollideStyle orig, T self, Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => orig(self, projectile, ref width, ref height, ref fallThrough, ref hitboxCenterFrac);

    // OnTileCollide
    public delegate bool Orig_OnTileCollide(T self, Projectile projectile, Vector2 oldVelocity);
    public virtual bool Detour_OnTileCollide(Orig_OnTileCollide orig, T self, Projectile projectile, Vector2 oldVelocity) => orig(self, projectile, oldVelocity);

    // PreKill
    public delegate bool Orig_PreKill(T self, Projectile projectile, int timeLeft);
    public virtual bool Detour_PreKill(Orig_PreKill orig, T self, Projectile projectile, int timeLeft) => orig(self, projectile, timeLeft);

    // OnKill
    public delegate void Orig_OnKill(T self, Projectile projectile, int timeLeft);
    public virtual void Detour_OnKill(Orig_OnKill orig, T self, Projectile projectile, int timeLeft) => orig(self, projectile, timeLeft);

    // CanCutTiles
    public delegate bool? Orig_CanCutTiles(T self, Projectile projectile);
    public virtual bool? Detour_CanCutTiles(Orig_CanCutTiles orig, T self, Projectile projectile) => orig(self, projectile);

    // CutTiles
    public delegate void Orig_CutTiles(T self, Projectile projectile);
    public virtual void Detour_CutTiles(Orig_CutTiles orig, T self, Projectile projectile) => orig(self, projectile);

    // CanDamage
    public delegate bool? Orig_CanDamage(T self, Projectile projectile);
    public virtual bool? Detour_CanDamage(Orig_CanDamage orig, T self, Projectile projectile) => orig(self, projectile);

    // MinionContactDamage
    public delegate bool Orig_MinionContactDamage(T self, Projectile projectile);
    public virtual bool Detour_MinionContactDamage(Orig_MinionContactDamage orig, T self, Projectile projectile) => orig(self, projectile);

    // ModifyDamageHitbox
    public delegate void Orig_ModifyDamageHitbox(T self, Projectile projectile, ref Rectangle hitbox);
    public virtual void Detour_ModifyDamageHitbox(Orig_ModifyDamageHitbox orig, T self, Projectile projectile, ref Rectangle hitbox) => orig(self, projectile, ref hitbox);

    // CanHitNPC
    public delegate bool? Orig_CanHitNPC(T self, Projectile projectile, NPC target);
    public virtual bool? Detour_CanHitNPC(Orig_CanHitNPC orig, T self, Projectile projectile, NPC target) => orig(self, projectile, target);

    // ModifyHitNPC
    public delegate void Orig_ModifyHitNPC(T self, Projectile projectile, NPC target, ref NPC.HitModifiers modifiers);
    public virtual void Detour_ModifyHitNPC(Orig_ModifyHitNPC orig, T self, Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) => orig(self, projectile, target, ref modifiers);

    // OnHitNPC
    public delegate void Orig_OnHitNPC(T self, Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone);
    public virtual void Detour_OnHitNPC(Orig_OnHitNPC orig, T self, Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) => orig(self, projectile, target, hit, damageDone);

    // CanHitPvp
    public delegate bool Orig_CanHitPvp(T self, Projectile projectile, Player target);
    public virtual bool Detour_CanHitPvp(Orig_CanHitPvp orig, T self, Projectile projectile, Player target) => orig(self, projectile, target);

    // CanHitPlayer
    public delegate bool Orig_CanHitPlayer(T self, Projectile projectile, Player target);
    public virtual bool Detour_CanHitPlayer(Orig_CanHitPlayer orig, T self, Projectile projectile, Player target) => orig(self, projectile, target);

    // ModifyHitPlayer
    public delegate void Orig_ModifyHitPlayer(T self, Projectile projectile, Player target, ref Player.HurtModifiers modifiers);
    public virtual void Detour_ModifyHitPlayer(Orig_ModifyHitPlayer orig, T self, Projectile projectile, Player target, ref Player.HurtModifiers modifiers) => orig(self, projectile, target, ref modifiers);

    // OnHitPlayer
    public delegate void Orig_OnHitPlayer(T self, Projectile projectile, Player target, Player.HurtInfo info);
    public virtual void Detour_OnHitPlayer(Orig_OnHitPlayer orig, T self, Projectile projectile, Player target, Player.HurtInfo info) => orig(self, projectile, target, info);

    // Colliding
    public delegate bool? Orig_Colliding(T self, Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox);
    public virtual bool? Detour_Colliding(Orig_Colliding orig, T self, Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) => orig(self, projectile, projHitbox, targetHitbox);

    // GetAlpha
    public delegate Color? Orig_GetAlpha(T self, Projectile projectile, Color lightColor);
    public virtual Color? Detour_GetAlpha(Orig_GetAlpha orig, T self, Projectile projectile, Color lightColor) => orig(self, projectile, lightColor);

    // PreDrawExtras
    public delegate bool Orig_PreDrawExtras(T self, Projectile projectile);
    public virtual bool Detour_PreDrawExtras(Orig_PreDrawExtras orig, T self, Projectile projectile) => orig(self, projectile);

    // PreDraw
    public delegate bool Orig_PreDraw(T self, Projectile projectile, ref Color lightColor);
    public virtual bool Detour_PreDraw(Orig_PreDraw orig, T self, Projectile projectile, ref Color lightColor) => orig(self, projectile, ref lightColor);

    // PostDraw
    public delegate void Orig_PostDraw(T self, Projectile projectile, Color lightColor);
    public virtual void Detour_PostDraw(Orig_PostDraw orig, T self, Projectile projectile, Color lightColor) => orig(self, projectile, lightColor);

    // DrawBehind
    public delegate void Orig_DrawBehind(T self, Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI);
    public virtual void Detour_DrawBehind(Orig_DrawBehind orig, T self, Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => orig(self, projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);

    // CanUseGrapple
    public delegate bool? Orig_CanUseGrapple(T self, int type, Player player);
    public virtual bool? Detour_CanUseGrapple(Orig_CanUseGrapple orig, T self, int type, Player player) => orig(self, type, player);

    // UseGrapple
    public delegate void Orig_UseGrapple(T self, Player player, ref int type);
    public virtual void Detour_UseGrapple(Orig_UseGrapple orig, T self, Player player, ref int type) => orig(self, player, ref type);

    // NumGrappleHooks
    public delegate void Orig_NumGrappleHooks(T self, Projectile projectile, Player player, ref int numHooks);
    public virtual void Detour_NumGrappleHooks(Orig_NumGrappleHooks orig, T self, Projectile projectile, Player player, ref int numHooks) => orig(self, projectile, player, ref numHooks);

    // GrappleRetreatSpeed
    public delegate void Orig_GrappleRetreatSpeed(T self, Projectile projectile, Player player, ref float speed);
    public virtual void Detour_GrappleRetreatSpeed(Orig_GrappleRetreatSpeed orig, T self, Projectile projectile, Player player, ref float speed) => orig(self, projectile, player, ref speed);

    // GrapplePullSpeed
    public delegate void Orig_GrapplePullSpeed(T self, Projectile projectile, Player player, ref float speed);
    public virtual void Detour_GrapplePullSpeed(Orig_GrapplePullSpeed orig, T self, Projectile projectile, Player player, ref float speed) => orig(self, projectile, player, ref speed);

    // GrappleTargetPoint
    public delegate void Orig_GrappleTargetPoint(T self, Projectile projectile, Player player, ref float grappleX, ref float grappleY);
    public virtual void Detour_GrappleTargetPoint(Orig_GrappleTargetPoint orig, T self, Projectile projectile, Player player, ref float grappleX, ref float grappleY) => orig(self, projectile, player, ref grappleX, ref grappleY);

    // GrappleCanLatchOnTo
    public delegate bool? Orig_GrappleCanLatchOnTo(T self, Projectile projectile, Player player, int x, int y);
    public virtual bool? Detour_GrappleCanLatchOnTo(Orig_GrappleCanLatchOnTo orig, T self, Projectile projectile, Player player, int x, int y) => orig(self, projectile, player, x, y);

    // PrepareBombToBlow
    public delegate void Orig_PrepareBombToBlow(T self, Projectile projectile);
    public virtual void Detour_PrepareBombToBlow(Orig_PrepareBombToBlow orig, T self, Projectile projectile) => orig(self, projectile);

    // EmitEnchantmentVisualsAt
    public delegate void Orig_EmitEnchantmentVisualsAt(T self, Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight);
    public virtual void Detour_EmitEnchantmentVisualsAt(Orig_EmitEnchantmentVisualsAt orig, T self, Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) => orig(self, projectile, boxPosition, boxWidth, boxHeight);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_OnSpawn);
        TryApplyDetour(Detour_PreAI);
        TryApplyDetour(Detour_AI);
        TryApplyDetour(Detour_PostAI);
        TryApplyDetour(Detour_SendExtraAI);
        TryApplyDetour(Detour_ReceiveExtraAI);
        TryApplyDetour(Detour_ShouldUpdatePosition);
        TryApplyDetour(Detour_TileCollideStyle);
        TryApplyDetour(Detour_OnTileCollide);
        TryApplyDetour(Detour_PreKill);
        TryApplyDetour(Detour_OnKill);
        TryApplyDetour(Detour_CanCutTiles);
        TryApplyDetour(Detour_CutTiles);
        TryApplyDetour(Detour_CanDamage);
        TryApplyDetour(Detour_MinionContactDamage);
        TryApplyDetour(Detour_ModifyDamageHitbox);
        TryApplyDetour(Detour_CanHitNPC);
        TryApplyDetour(Detour_ModifyHitNPC);
        TryApplyDetour(Detour_OnHitNPC);
        TryApplyDetour(Detour_CanHitPvp);
        TryApplyDetour(Detour_CanHitPlayer);
        TryApplyDetour(Detour_ModifyHitPlayer);
        TryApplyDetour(Detour_OnHitPlayer);
        TryApplyDetour(Detour_Colliding);
        TryApplyDetour(Detour_GetAlpha);
        TryApplyDetour(Detour_PreDrawExtras);
        TryApplyDetour(Detour_PreDraw);
        TryApplyDetour(Detour_PostDraw);
        TryApplyDetour(Detour_DrawBehind);
        TryApplyDetour(Detour_CanUseGrapple);
        TryApplyDetour(Detour_UseGrapple);
        TryApplyDetour(Detour_NumGrappleHooks);
        TryApplyDetour(Detour_GrappleRetreatSpeed);
        TryApplyDetour(Detour_GrapplePullSpeed);
        TryApplyDetour(Detour_GrappleTargetPoint);
        TryApplyDetour(Detour_GrappleCanLatchOnTo);
        TryApplyDetour(Detour_PrepareBombToBlow);
        TryApplyDetour(Detour_EmitEnchantmentVisualsAt);
    }
}

public abstract class GlobalPylonDetour<T> : ModTypeDetour<T> where T : GlobalPylon
{
    // PreDrawMapIcon
    public delegate bool Orig_PreDrawMapIcon(T self, ref MapOverlayDrawContext context, ref string mouseOverText, ref TeleportPylonInfo pylonInfo, ref bool isNearPylon, ref Color drawColor, ref float deselectedScale, ref float selectedScale);
    public virtual bool Detour_PreDrawMapIcon(Orig_PreDrawMapIcon orig, T self, ref MapOverlayDrawContext context, ref string mouseOverText, ref TeleportPylonInfo pylonInfo, ref bool isNearPylon, ref Color drawColor, ref float deselectedScale, ref float selectedScale) => orig(self, ref context, ref mouseOverText, ref pylonInfo, ref isNearPylon, ref drawColor, ref deselectedScale, ref selectedScale);

    // PreCanPlacePylon
    public delegate bool? Orig_PreCanPlacePylon(T self, int x, int y, int tileType, TeleportPylonType pylonType);
    public virtual bool? Detour_PreCanPlacePylon(Orig_PreCanPlacePylon orig, T self, int x, int y, int tileType, TeleportPylonType pylonType) => orig(self, x, y, tileType, pylonType);

    // ValidTeleportCheck_PreNPCCount
    public delegate bool? Orig_ValidTeleportCheck_PreNPCCount(T self, TeleportPylonInfo pylonInfo, ref int defaultNecessaryNPCCount);
    public virtual bool? Detour_ValidTeleportCheck_PreNPCCount(Orig_ValidTeleportCheck_PreNPCCount orig, T self, TeleportPylonInfo pylonInfo, ref int defaultNecessaryNPCCount) => orig(self, pylonInfo, ref defaultNecessaryNPCCount);

    // ValidTeleportCheck_PreAnyDanger
    public delegate bool? Orig_ValidTeleportCheck_PreAnyDanger(T self, TeleportPylonInfo pylonInfo);
    public virtual bool? Detour_ValidTeleportCheck_PreAnyDanger(Orig_ValidTeleportCheck_PreAnyDanger orig, T self, TeleportPylonInfo pylonInfo) => orig(self, pylonInfo);

    // ValidTeleportCheck_PreBiomeRequirements
    public delegate bool? Orig_ValidTeleportCheck_PreBiomeRequirements(T self, TeleportPylonInfo pylonInfo, SceneMetrics sceneData);
    public virtual bool? Detour_ValidTeleportCheck_PreBiomeRequirements(Orig_ValidTeleportCheck_PreBiomeRequirements orig, T self, TeleportPylonInfo pylonInfo, SceneMetrics sceneData) => orig(self, pylonInfo, sceneData);

    // PostValidTeleportCheck
    public delegate void Orig_PostValidTeleportCheck(T self, TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey);
    public virtual void Detour_PostValidTeleportCheck(Orig_PostValidTeleportCheck orig, T self, TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey) => orig(self, destinationPylonInfo, nearbyPylonInfo, ref destinationPylonValid, ref validNearbyPylonFound, ref errorKey);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_PreDrawMapIcon);
        TryApplyDetour(Detour_PreCanPlacePylon);
        TryApplyDetour(Detour_ValidTeleportCheck_PreNPCCount);
        TryApplyDetour(Detour_ValidTeleportCheck_PreAnyDanger);
        TryApplyDetour(Detour_ValidTeleportCheck_PreBiomeRequirements);
        TryApplyDetour(Detour_PostValidTeleportCheck);
    }
}

public abstract class GlobalTileDetour<T> : GlobalBlockTypeDetour<T> where T : GlobalTile
{
    // DropCritterChance
    public delegate void Orig_DropCritterChance(T self, int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance);
    public virtual void Detour_DropCritterChance(Orig_DropCritterChance orig, T self, int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) => orig(self, i, j, type, ref wormChance, ref grassHopperChance, ref jungleGrubChance);

    // CanDrop
    public delegate bool Orig_CanDrop(T self, int i, int j, int type);
    public virtual bool Detour_CanDrop(Orig_CanDrop orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // Drop
    public delegate void Orig_Drop(T self, int i, int j, int type);
    public virtual void Detour_Drop(Orig_Drop orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // CanKillTile
    public delegate bool Orig_CanKillTile(T self, int i, int j, int type, ref bool blockDamaged);
    public virtual bool Detour_CanKillTile(Orig_CanKillTile orig, T self, int i, int j, int type, ref bool blockDamaged) => orig(self, i, j, type, ref blockDamaged);

    // KillTile
    public delegate void Orig_KillTile(T self, int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem);
    public virtual void Detour_KillTile(Orig_KillTile orig, T self, int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) => orig(self, i, j, type, ref fail, ref effectOnly, ref noItem);

    // NearbyEffects
    public delegate void Orig_NearbyEffects(T self, int i, int j, int type, bool closer);
    public virtual void Detour_NearbyEffects(Orig_NearbyEffects orig, T self, int i, int j, int type, bool closer) => orig(self, i, j, type, closer);

    // IsTileDangerous
    public delegate bool? Orig_IsTileDangerous(T self, int i, int j, int type, Player player);
    public virtual bool? Detour_IsTileDangerous(Orig_IsTileDangerous orig, T self, int i, int j, int type, Player player) => orig(self, i, j, type, player);

    // IsTileBiomeSightable
    public delegate bool? Orig_IsTileBiomeSightable(T self, int i, int j, int type, ref Color sightColor);
    public virtual bool? Detour_IsTileBiomeSightable(Orig_IsTileBiomeSightable orig, T self, int i, int j, int type, ref Color sightColor) => orig(self, i, j, type, ref sightColor);

    // IsTileSpelunkable
    public delegate bool? Orig_IsTileSpelunkable(T self, int i, int j, int type);
    public virtual bool? Detour_IsTileSpelunkable(Orig_IsTileSpelunkable orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // SetSpriteEffects
    public delegate void Orig_SetSpriteEffects(T self, int i, int j, int type, ref SpriteEffects spriteEffects);
    public virtual void Detour_SetSpriteEffects(Orig_SetSpriteEffects orig, T self, int i, int j, int type, ref SpriteEffects spriteEffects) => orig(self, i, j, type, ref spriteEffects);

    // AnimateTile
    public delegate void Orig_AnimateTile(T self);
    public virtual void Detour_AnimateTile(Orig_AnimateTile orig, T self) => orig(self);

    // DrawEffects
    public delegate void Orig_DrawEffects(T self, int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData);
    public virtual void Detour_DrawEffects(Orig_DrawEffects orig, T self, int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData) => orig(self, i, j, type, spriteBatch, ref drawData);

    // EmitParticles
    public delegate void Orig_EmitParticles(T self, int i, int j, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible);
    public virtual void Detour_EmitParticles(Orig_EmitParticles orig, T self, int i, int j, Tile tileCache, ushort typeCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible) => orig(self, i, j, tileCache, typeCache, tileFrameX, tileFrameY, tileLight, visible);

    // SpecialDraw
    public delegate void Orig_SpecialDraw(T self, int i, int j, int type, SpriteBatch spriteBatch);
    public virtual void Detour_SpecialDraw(Orig_SpecialDraw orig, T self, int i, int j, int type, SpriteBatch spriteBatch) => orig(self, i, j, type, spriteBatch);

    // TileFrame
    public delegate bool Orig_TileFrame(T self, int i, int j, int type, ref bool resetFrame, ref bool noBreak);
    public virtual bool Detour_TileFrame(Orig_TileFrame orig, T self, int i, int j, int type, ref bool resetFrame, ref bool noBreak) => orig(self, i, j, type, ref resetFrame, ref noBreak);

    // AdjTiles
    public delegate int[] Orig_AdjTiles(T self, int type);
    public virtual int[] Detour_AdjTiles(Orig_AdjTiles orig, T self, int type) => orig(self, type);

    // RightClick
    public delegate void Orig_RightClick(T self, int i, int j, int type);
    public virtual void Detour_RightClick(Orig_RightClick orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // MouseOver
    public delegate void Orig_MouseOver(T self, int i, int j, int type);
    public virtual void Detour_MouseOver(Orig_MouseOver orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // MouseOverFar
    public delegate void Orig_MouseOverFar(T self, int i, int j, int type);
    public virtual void Detour_MouseOverFar(Orig_MouseOverFar orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // AutoSelect
    public delegate bool Orig_AutoSelect(T self, int i, int j, int type, Item item);
    public virtual bool Detour_AutoSelect(Orig_AutoSelect orig, T self, int i, int j, int type, Item item) => orig(self, i, j, type, item);

    // PreHitWire
    public delegate bool Orig_PreHitWire(T self, int i, int j, int type);
    public virtual bool Detour_PreHitWire(Orig_PreHitWire orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // HitWire
    public delegate void Orig_HitWire(T self, int i, int j, int type);
    public virtual void Detour_HitWire(Orig_HitWire orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // Slope
    public delegate bool Orig_Slope(T self, int i, int j, int type);
    public virtual bool Detour_Slope(Orig_Slope orig, T self, int i, int j, int type) => orig(self, i, j, type);

    // FloorVisuals
    public delegate void Orig_FloorVisuals(T self, int type, Player player);
    public virtual void Detour_FloorVisuals(Orig_FloorVisuals orig, T self, int type, Player player) => orig(self, type, player);

    // ChangeWaterfallStyle
    public delegate void Orig_ChangeWaterfallStyle(T self, int type, ref int style);
    public virtual void Detour_ChangeWaterfallStyle(Orig_ChangeWaterfallStyle orig, T self, int type, ref int style) => orig(self, type, ref style);

    // CanReplace
    public delegate bool Orig_CanReplace(T self, int i, int j, int type, int tileTypeBeingPlaced);
    public virtual bool Detour_CanReplace(Orig_CanReplace orig, T self, int i, int j, int type, int tileTypeBeingPlaced) => orig(self, i, j, type, tileTypeBeingPlaced);

    // PostSetupTileMerge
    public delegate void Orig_PostSetupTileMerge(T self);
    public virtual void Detour_PostSetupTileMerge(Orig_PostSetupTileMerge orig, T self) => orig(self);

    // PreShakeTree
    public delegate void Orig_PreShakeTree(T self, int x, int y, TreeTypes treeType);
    public virtual void Detour_PreShakeTree(Orig_PreShakeTree orig, T self, int x, int y, TreeTypes treeType) => orig(self, x, y, treeType);

    // ShakeTree
    public delegate bool Orig_ShakeTree(T self, int x, int y, TreeTypes treeType);
    public virtual bool Detour_ShakeTree(Orig_ShakeTree orig, T self, int x, int y, TreeTypes treeType) => orig(self, x, y, treeType);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_DropCritterChance);
        TryApplyDetour(Detour_CanDrop);
        TryApplyDetour(Detour_Drop);
        TryApplyDetour(Detour_CanKillTile);
        TryApplyDetour(Detour_KillTile);
        TryApplyDetour(Detour_NearbyEffects);
        TryApplyDetour(Detour_IsTileDangerous);
        TryApplyDetour(Detour_IsTileBiomeSightable);
        TryApplyDetour(Detour_IsTileSpelunkable);
        TryApplyDetour(Detour_SetSpriteEffects);
        TryApplyDetour(Detour_AnimateTile);
        TryApplyDetour(Detour_DrawEffects);
        TryApplyDetour(Detour_EmitParticles);
        TryApplyDetour(Detour_SpecialDraw);
        TryApplyDetour(Detour_TileFrame);
        TryApplyDetour(Detour_AdjTiles);
        TryApplyDetour(Detour_RightClick);
        TryApplyDetour(Detour_MouseOver);
        TryApplyDetour(Detour_MouseOverFar);
        TryApplyDetour(Detour_AutoSelect);
        TryApplyDetour(Detour_PreHitWire);
        TryApplyDetour(Detour_HitWire);
        TryApplyDetour(Detour_Slope);
        TryApplyDetour(Detour_FloorVisuals);
        TryApplyDetour(Detour_ChangeWaterfallStyle);
        TryApplyDetour(Detour_CanReplace);
        TryApplyDetour(Detour_PostSetupTileMerge);
        TryApplyDetour(Detour_PreShakeTree);
        TryApplyDetour(Detour_ShakeTree);
    }
}

public abstract class GlobalWallDetour<T> : GlobalBlockTypeDetour<T> where T : GlobalWall
{
    // Drop
    public delegate bool Orig_Drop(T self, int i, int j, int type, ref int dropType);
    public virtual bool Detour_Drop(Orig_Drop orig, T self, int i, int j, int type, ref int dropType) => orig(self, i, j, type, ref dropType);

    // KillWall
    public delegate void Orig_KillWall(T self, int i, int j, int type, ref bool fail);
    public virtual void Detour_KillWall(Orig_KillWall orig, T self, int i, int j, int type, ref bool fail) => orig(self, i, j, type, ref fail);

    // WallFrame
    public delegate bool Orig_WallFrame(T self, int i, int j, int type, bool randomizeFrame, ref int style, ref int frameNumber);
    public virtual bool Detour_WallFrame(Orig_WallFrame orig, T self, int i, int j, int type, bool randomizeFrame, ref int style, ref int frameNumber) => orig(self, i, j, type, randomizeFrame, ref style, ref frameNumber);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Drop);
        TryApplyDetour(Detour_KillWall);
        TryApplyDetour(Detour_WallFrame);
    }
}