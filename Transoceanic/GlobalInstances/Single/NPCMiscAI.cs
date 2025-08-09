namespace Transoceanic.GlobalInstances.Single;

public sealed class NPCMiscAI : TOGlobalNPCBehavior
{
    public override decimal Priority => 500m;

    public override bool PreAI(NPC npc)
    {
        TOGlobalNPC oceanNPC = npc.Ocean();
        oceanNPC.LifeRatio = (float)npc.life / npc.lifeMax;
        if (oceanNPC.AlwaysRotating)
            npc.VelocityToRotation(oceanNPC.RotationOffset);
        return true;
    }
}
