using System;
using CalamityAnomalies.Override;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override void SetStaticDefaults()
    {
        foreach (CANPCOverride npcOverride in CAOverrideHelper.NPCOverrides.Values)
            npcOverride.SetStaticDefaults();
    }

    public override void SetDefaults(NPC npc)
    {
        Array.Fill(AnomalyAI, 0f);

        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
            npcOverride.SetDefaults();
    }

    public override void SetDefaultsFromNetId(NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
            npcOverride.SetDefaultsFromNetId();
    }
}
