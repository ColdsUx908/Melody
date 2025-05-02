using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances;

public partial class CAGlobalNPC : GlobalNPC
{
    public int AnomalyKilltime { get; private set; } = 0;

    public bool DisableNaturalDespawning { get; set; } = true;

    public bool ShouldRunAnomalyAI { get; set; } = true;

    private const int MaxAISlots = 50;

    public float[] AnomalyAI { get; } = new float[MaxAISlots];

    public bool[] ShouldSyncAnomalyAI { get; } = new bool[MaxAISlots];

    public int AnomalyAITimer { get; private set; } = 0;

    public int AnomalyUltraAITimer { get; private set; } = 0;

    public int AnomalyUltraBarTimer { get; private set; } = 0;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int BossRushAITimer { get; private set; } = 0;

    public override bool InstancePerEntity => true;
}
