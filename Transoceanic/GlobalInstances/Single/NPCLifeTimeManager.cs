namespace Transoceanic.GlobalInstances.Single;

public sealed class NPCLifeTimeManager : TOGlobalNPCBehavior
{
    public override decimal Priority => 1000m;

    public override void SetDefaults(NPC npc)
    {
        TOGlobalNPC oceanNPC = npc.Ocean();
        oceanNPC.Master = Main.maxNPCs;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        TOGlobalNPC oceanNPC = npc.Ocean();
        oceanNPC.SpawnTime = TOWorld.GameTimer.TotalTicks;
    }

    public override bool PreAI(NPC npc)
    {
        TOGlobalNPC oceanNPC = npc.Ocean();
        oceanNPC.ActiveTime++;
        return true;
    }
}
