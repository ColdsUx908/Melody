using CalamityAnomalies.Override;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Transoceanic;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.FindFrame(frameHeight);
    }

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (Main.LocalPlayer.Calamity().trippy && !NeverTrippy)
            return TOMain.DiscoColor;

        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            Color? result = npcOverride.GetAlpha(drawColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.PreDraw(spriteBatch, screenPos, drawColor))
                return false;
        }

        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.PostDraw(spriteBatch, screenPos, drawColor);
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.BossHeadSlot(ref index);
    }

    public override void BossHeadRotation(NPC npc, ref float rotation)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.BossHeadRotation(ref rotation);
    }

    public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.BossHeadSpriteEffects(ref spriteEffects);
    }
}
