using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public int AnomalyKilltime { get; private set; } = 0;

    public bool DisableNaturalDespawning { get; set; } = true;

    public bool ShouldRunAnomalyAI { get; set; } = true;

    private const int MaxAISlots = 64;

    /// <summary>
    /// 额外的AI槽位，共64个。
    /// <br/>最后10个槽位由 <see cref="CAGlobalNPC"/> 类保留，不做使用。
    /// </summary>
    public float[] AnomalyAI { get; } = new float[MaxAISlots];

    public bool[] AnomalyAISync { get; } = new bool[MaxAISlots];

    public int AnomalyAITimer { get; private set; } = 0;

    public int AnomalyUltraAITimer { get; private set; } = 0;

    public int AnomalyUltraBarTimer { get; private set; } = 0;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int BossRushAITimer { get; private set; } = 0;

    public override bool InstancePerEntity => true;
}
