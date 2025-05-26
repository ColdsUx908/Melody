using System;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public int AnomalyKilltime { get; private set; } = 0;

    public bool ShouldRunAnomalyAI { get; set; } = true;

    public int AnomalyAITimer { get; private set; } = 0;

    public int AnomalyUltraAITimer { get; private set; } = 0;

    public int AnomalyUltraBarTimer { get; private set; } = 0;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int BossRushAITimer { get; private set; } = 0;

    private const int MaxAISlots = 64;

    /// <summary>
    /// 额外的AI槽位，共64个。
    /// </summary>
    public float[] AnomalyAI { get; } = new float[MaxAISlots];

    public bool[] AIChanged { get; } = new bool[MaxAISlots];

    public void SetAnomalyAI(float value, Index index)
    {
        AnomalyAI[index] = value;
        AIChanged[index] = true;
    }

    public bool NeverTrippy { get; set; } = false;
}
