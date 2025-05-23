using CalamityAnomalies.Override;
using CalamityMod;
using CalamityMod.NPCs.Abyss;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Transoceanic;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            npcOverride.OnSpawn(source);
        }
    }

    public override bool CheckActive(NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.CheckActive())
                return false;
        }

        return true;
    }

    public override bool CheckDead(NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.CheckDead())
                return false;
        }

        return true;
    }

    public override bool SpecialOnKill(NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.SpecialOnKill())
                return false;
        }

        return true;
    }

    public override bool PreKill(NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.PreKill())
                return false;
        }

        return true;
    }

    public override void OnKill(NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            npcOverride.OnKill();
        }

        if (npc.ModNPC is EidolonWyrmHead && !DownedBossSystem.downedPrimordialWyrm)
            DownedBossSystem.downedPrimordialWyrm = true;

        foreach (Player player in TOMain.ActivePlayers)
        {
            player.Anomaly().PlayerDownedBossCalamity.BossesOnKill(npc);
        }
    }
}
