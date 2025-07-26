using CalamityMod.Projectiles.BaseProjectiles;
using Terraria.Graphics.Effects;

namespace CalamityAnomalies.Core;

#region Calamity Detour
public abstract class BaseLaserbeamProjectileDetour<T> : ModProjectileDetour<T> where T : BaseLaserbeamProjectile
{
    // Behavior
    public delegate void Orig_Behavior(T self);
    public virtual void Detour_Behavior(Orig_Behavior orig, T self) => orig(self);

    // UpdateLaserMotion
    public delegate void Orig_UpdateLaserMotion(T self);
    public virtual void Detour_UpdateLaserMotion(Orig_UpdateLaserMotion orig, T self) => orig(self);

    // DetermineScale
    public delegate void Orig_DetermineScale(T self);
    public virtual void Detour_DetermineScale(Orig_DetermineScale orig, T self) => orig(self);

    // AttachToSomething
    public delegate void Orig_AttachToSomething(T self);
    public virtual void Detour_AttachToSomething(Orig_AttachToSomething orig, T self) => orig(self);

    // DetermineLaserLength
    public delegate float Orig_DetermineLaserLength(T self);
    public virtual float Detour_DetermineLaserLength(Orig_DetermineLaserLength orig, T self) => orig(self);

    // ExtraBehavior
    public delegate void Orig_ExtraBehavior(T self);
    public virtual void Detour_ExtraBehavior(Orig_ExtraBehavior orig, T self) => orig(self);

    // get_Lifetime
    public delegate float Orig_get_Lifetime(T self);
    public virtual float Detour_get_Lifetime(Orig_get_Lifetime orig, T self) => orig(self);

    // get_MaxScale
    public delegate float Orig_get_MaxScale(T self);
    public virtual float Detour_get_MaxScale(Orig_get_MaxScale orig, T self) => orig(self);

    // get_MaxLaserLength
    public delegate float Orig_get_MaxLaserLength(T self);
    public virtual float Detour_get_MaxLaserLength(Orig_get_MaxLaserLength orig, T self) => orig(self);

    // get_LaserBeginTexture
    public delegate Texture2D Orig_get_LaserBeginTexture(T self);
    public virtual Texture2D Detour_get_LaserBeginTexture(Orig_get_LaserBeginTexture orig, T self) => orig(self);

    // get_LaserMiddleTexture
    public delegate Texture2D Orig_get_LaserMiddleTexture(T self);
    public virtual Texture2D Detour_get_LaserMiddleTexture(Orig_get_LaserMiddleTexture orig, T self) => orig(self);

    // get_LaserEndTexture
    public delegate Texture2D Orig_get_LaserEndTexture(T self);
    public virtual Texture2D Detour_get_LaserEndTexture(Orig_get_LaserEndTexture orig, T self) => orig(self);

    // get_ScaleExpandRate
    public delegate float Orig_get_ScaleExpandRate(T self);
    public virtual float Detour_get_ScaleExpandRate(Orig_get_ScaleExpandRate orig, T self) => orig(self);

    // get_LightCastColor
    public delegate Color Orig_get_LightCastColor(T self);
    public virtual Color Detour_get_LightCastColor(Orig_get_LightCastColor orig, T self) => orig(self);

    // get_LaserOverlayColor
    public delegate Color Orig_get_LaserOverlayColor(T self);
    public virtual Color Detour_get_LaserOverlayColor(Orig_get_LaserOverlayColor orig, T self) => orig(self);

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Behavior);
        TryApplyDetour(Detour_UpdateLaserMotion);
        TryApplyDetour(Detour_DetermineScale);
        TryApplyDetour(Detour_AttachToSomething);
        TryApplyDetour(Detour_DetermineLaserLength);
        TryApplyDetour(Detour_ExtraBehavior);
        TryApplyDetour(Detour_get_Lifetime);
        TryApplyDetour(Detour_get_MaxScale);
        TryApplyDetour(Detour_get_MaxLaserLength);
        TryApplyDetour(Detour_get_LaserBeginTexture);
        TryApplyDetour(Detour_get_LaserMiddleTexture);
        TryApplyDetour(Detour_get_LaserEndTexture);
        TryApplyDetour(Detour_get_ScaleExpandRate);
        TryApplyDetour(Detour_get_LightCastColor);
        TryApplyDetour(Detour_get_LaserOverlayColor);
    }
}
#endregion Calamity Detour

#region CA Content Detour
public abstract class CAModTypeDetour<T> : ModTypeDetour<T> where T : ModType
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModAccessorySlotDetour<T> : ModAccessorySlotDetour<T> where T : ModAccessorySlot
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModBannerTileDetour<T> : ModBannerTileDetour<T> where T : ModBannerTile
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModBiomeDetour<T> : ModBiomeDetour<T> where T : ModBiome
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModBiomeConversionDetour<T> : ModBiomeConversionDetour<T> where T : ModBiomeConversion
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModBlockTypeDetour<T> : ModBlockTypeDetour<T> where T : ModBlockType
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModBossBarDetour<T> : ModBossBarDetour<T> where T : ModBossBar
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModBossBarStyleDetour<T> : ModBossBarStyleDetour<T> where T : ModBossBarStyle
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModBuffDetour<T> : ModBuffDetour<T> where T : ModBuff
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModCactusDetour<T> : ModCactusDetour<T> where T : ModCactus
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModCloudDetour<T> : ModCloudDetour<T> where T : ModCloud
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModCommandDetour<T> : ModCommandDetour<T> where T : ModCommand
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModDustDetour<T> : ModDustDetour<T> where T : ModDust
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModEmoteBubbleDetour<T> : ModEmoteBubbleDetour<T> where T : ModEmoteBubble
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModGoreDetour<T> : ModGoreDetour<T> where T : ModGore
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModHairDetour<T> : ModHairDetour<T> where T : ModHair
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModItemDetour<T> : ModItemDetour<T> where T : ModItem
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModMapLayerDetour<T> : ModMapLayerDetour<T> where T : ModMapLayer
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModMenuDetour<T> : ModMenuDetour<T> where T : ModMenu
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModMountDetour<T> : ModMountDetour<T> where T : ModMount
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModNPCDetour<T> : ModNPCDetour<T> where T : ModNPC
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModPalmTreeDetour<T> : ModPalmTreeDetour<T> where T : ModPalmTree
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModPlayerDetour<T> : ModPlayerDetour<T> where T : ModPlayer
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModPrefixDetour<T> : ModPrefixDetour<T> where T : ModPrefix
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModProjectileDetour<T> : ModProjectileDetour<T> where T : ModProjectile
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModPylonDetour<T> : ModPylonDetour<T> where T : ModPylon
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModRarityDetour<T> : ModRarityDetour<T> where T : ModRarity
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModResourceDisplaySetDetour<T> : ModResourceDisplaySetDetour<T> where T : ModResourceDisplaySet
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModResourceOverlayDetour<T> : ModResourceOverlayDetour<T> where T : ModResourceOverlay
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModSceneEffectDetour<T> : ModSceneEffectDetour<T> where T : ModSceneEffect
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModSurfaceBackgroundStyleDetour<T> : ModSurfaceBackgroundStyleDetour<T> where T : ModSurfaceBackgroundStyle
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModSystemDetour<T> : ModSystemDetour<T> where T : ModSystem
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModTexturedTypeDetour<T> : ModTexturedTypeDetour<T> where T : ModTexturedType
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModTileDetour<T> : ModTileDetour<T> where T : ModTile
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModTileEntityDetour<T> : ModTileEntityDetour<T> where T : ModTileEntity
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModTreeDetour<T> : ModTreeDetour<T> where T : ModTree
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModUndergroundBackgroundStyleDetour<T> : ModUndergroundBackgroundStyleDetour<T> where T : ModUndergroundBackgroundStyle
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModWallDetour<T> : ModWallDetour<T> where T : ModWall
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModWaterfallStyleDetour<T> : ModWaterfallStyleDetour<T> where T : ModWaterfallStyle
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAModWaterStyleDetour<T> : ModWaterStyleDetour<T> where T : ModWaterStyle
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalTypeDetour<TEntity, TGlobal, TGlobalType> : GlobalTypeDetour<TEntity, TGlobal, TGlobalType>
    where TEntity : IEntityWithGlobals<TGlobal>
    where TGlobal : GlobalType<TEntity, TGlobal>
    where TGlobalType : TGlobal
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalBlockTypeDetour<T> : GlobalBlockTypeDetour<T> where T : GlobalBlockType
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalBossBarDetour<T> : GlobalBossBarDetour<T> where T : GlobalBossBar
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalBuffDetour<T> : GlobalBuffDetour<T> where T : GlobalBuff
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalEmoteBubbleDetour<T> : GlobalEmoteBubbleDetour<T> where T : GlobalEmoteBubble
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalInfoDisplayDetour<T> : GlobalInfoDisplayDetour<T> where T : GlobalInfoDisplay
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalItemDetour<T> : GlobalItemDetour<T> where T : GlobalItem
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalNPCDetour<T> : GlobalNPCDetour<T> where T : GlobalNPC
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalProjectileDetour<T> : GlobalProjectileDetour<T> where T : GlobalProjectile
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalPylonDetour<T> : GlobalPylonDetour<T> where T : GlobalPylon
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalTileDetour<T> : GlobalTileDetour<T> where T : GlobalTile
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalWallDetour<T> : GlobalWallDetour<T> where T : GlobalWall
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CAGameEffectDetour<T> : GameEffectDetour<T> where T : GameEffect
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CACustomSkyDetour<T> : CustomSkyDetour<T> where T : CustomSky
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}

public abstract class CABaseLaserbeamProjectileDetour<T> : BaseLaserbeamProjectileDetour<T> where T : BaseLaserbeamProjectile
{
    public sealed override bool ShouldApplyDetour => CAServerConfig.Instance.Contents;
}
#endregion CA Content Detour