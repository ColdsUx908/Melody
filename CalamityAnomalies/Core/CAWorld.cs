using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.World;

namespace CalamityAnomalies.Core;

/// <summary>
/// 提供世界变量。
/// </summary>
public sealed class CAWorld : ModSystem
{
    /// <summary>
    /// 异象模式。
    /// </summary>
    public static bool Anomaly = false;

    /// <summary>
    /// 异象超凡。
    /// </summary>
    public static bool AnomalyUltramundane = false;

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

    public static bool PermaFrostActive
    {
        get
        {
            if (CalamityGlobalNPC.SCal != -1)
            {
                NPC supremeCalamitas = Main.npc[CalamityGlobalNPC.SCal];
                return supremeCalamitas.active && supremeCalamitas.GetModNPC<SupremeCalamitas>().permafrost;
            }
            return false;
        }
    }

    public override void OnWorldLoad()
    {
        AnomalyUltramundane = false;
    }

    public override void OnWorldUnload()
    {
        AnomalyUltramundane = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["Anomaly"] = Anomaly;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Anomaly = tag.GetBool("Anomaly");
    }
}
