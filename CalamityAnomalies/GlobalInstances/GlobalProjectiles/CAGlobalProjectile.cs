using System;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalProjectiles;

public partial class CAGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    private const int MaxAISlots = 32;

    /// <summary>
    /// 额外的AI槽位，共32个。
    /// </summary>
    public float[] AnomalyAI { get; } = new float[MaxAISlots];

    public bool[] AIChanged { get; } = new bool[MaxAISlots];

    public void SetAnomalyAI(float value, Index index)
    {
        AnomalyAI[index] = value;
        AIChanged[index] = true;
    }
}
