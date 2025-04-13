using System;
using Terraria.ModLoader;

namespace CalamityAnomalies.NPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public int AnomalyKilltime = 0;

    public bool disableNaturalDespawning = false;

    public bool shouldMinimizeCalamityAI = true;

    public bool shouldRunAnomalyAI = true;

    public float[] anomalyAI = new float[50];

    public int AnomalyAITimer { get; private set; } = 0;

    public int AnomalyUltraAITimer { get; private set; } = 0;

    public int AnomalyUltraBarTimer { get; private set; } = 0;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public override bool InstancePerEntity => true;
}
