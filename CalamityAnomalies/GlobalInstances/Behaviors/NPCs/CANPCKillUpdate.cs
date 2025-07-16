using CalamityMod.NPCs.Abyss;

namespace CalamityAnomalies.GlobalInstances.Behaviors.NPCs;

public sealed class CANPCKillUpdate : CAGlobalNPCBehavior
{
    public override void OnKill(NPC npc)
    {
        if (npc.ModNPC is EidolonWyrmHead && !DownedBossSystem.downedPrimordialWyrm)
            DownedBossSystem.downedPrimordialWyrm = true;

        foreach (Player player in Player.ActivePlayers)
            player.Anomaly().DownedBossCalamity.BossesOnKill(npc);
    }
}
