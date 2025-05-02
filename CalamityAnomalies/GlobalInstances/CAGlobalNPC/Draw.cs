using CalamityAnomalies.Contents.AnomalyMode.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances;

public partial class CAGlobalNPC : GlobalNPC
{
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (CAWorld.Anomaly
            && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && ShouldRunAnomalyAI)
        {
            anomalyNPCOverride.TryConnectWithNPC(npc);
            anomalyNPCOverride.PreDraw(spriteBatch, screenPos, drawColor);
            anomalyNPCOverride.ClearNPCInstances();
            return anomalyNPCOverride.AllowOrigMethod(OrigMethodType.Draw);
        }

        return true;
    }
}
