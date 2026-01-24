using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelSapphire_Anomaly : KingSlimeJewel_Anomaly<KingSlimeJewelSapphire>
{
    public int BuffedShootCooldownTime => HasEnteredPhase2 ? 300 : 240;

    public override void SetDefaults()
    {
        NPC.width = 28;
        NPC.height = 28;
    }

    public override bool PreAI()
    {
        if (!OceanNPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
            return false;
        }

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return false;
        }

        NPC.damage = 0;
        Lighting.AddLight(NPC.Center, 0f, 0f, 1f);

        JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.5f, 150f, -150f, 50f, -250f);

        NPC.netUpdate = true;
        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, 0f);
        return false;
    }

    public override bool CheckDead()
    {
        if (CAWorld.AnomalyUltramundane)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                JewelHandler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }
}
