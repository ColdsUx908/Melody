﻿using CalamityMod.World;

namespace CalamityAnomalies;

/// <summary>
/// 提供世界变量。
/// </summary>
public class CAWorld
{
    /// <summary>
    /// 异象模式。
    /// </summary>
    public static bool Anomaly { get; set; } = false;
    /// <summary>
    /// 异象超凡。
    /// </summary>
    public static bool AnomalyUltramundane { get; set; } = false;

    /// <summary>
    /// BossRush模式。
    /// </summary>
    public static bool BossRush { get; set; } = false;

    /// <summary>
    /// 在BossRush模式下，是否处于真实的BossRush事件中。
    /// </summary>
    public static bool RealBossRushEventActive { get; internal set; } = false;

    /// <summary>
    /// 传奇复仇。
    /// </summary>
    public static bool LR => CalamityWorld.LegendaryMode && CalamityWorld.revenge;

    /// <summary>
    /// 传奇死亡。
    /// </summary>
    public static bool LD => CalamityWorld.LegendaryMode && CalamityWorld.death;

    /// <summary>
    /// 传奇复仇GFB。
    /// </summary>
    public static bool LRG => CalamityWorld.LegendaryMode && CalamityWorld.revenge && Main.zenithWorld;

    /// <summary>
    /// 传奇死亡GFB。
    /// </summary>
    public static bool LDG => CalamityWorld.LegendaryMode && CalamityWorld.death && Main.zenithWorld;
}

public class CAWorldSavingSystem : ModSystem
{
    public override void OnWorldLoad()
    {
        CAWorld.AnomalyUltramundane = false;
    }

    public override void OnWorldUnload()
    {
        CAWorld.AnomalyUltramundane = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["Anomaly"] = CAWorld.Anomaly;
        tag["BossRush"] = CAWorld.BossRush;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        CAWorld.Anomaly = tag.GetBool("Anomaly");
        CAWorld.BossRush = tag.GetBool("BossRush");
    }
}
