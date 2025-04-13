using System.Reflection;
using CalamityAnomalies.Systems;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Transoceanic;
using Transoceanic.Core;

namespace CalamityAnomalies.NPCs;

/// <summary>
/// NPC钩子。
/// </summary>
public partial class CANPCHook : ITODetourProvider, ITOLoader
{
    public static MethodInfo CalGlobalNPCPreAI => typeof(CalamityGlobalNPC).GetMethod("PreAI", TOMain.UniversalBindingFlags);
    public static MethodInfo CalGlobalNPCPreDraw => typeof(CalamityGlobalNPC).GetMethod("PreDraw", TOMain.UniversalBindingFlags);

    public delegate bool Orig_CalGlobalNPCPreAI(CalamityGlobalNPC self, NPC npc);
    public delegate bool Orig_CalGlobalNPCPreDraw(CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Color drawColor);

    internal static bool CalPreAIDetour(Orig_CalGlobalNPCPreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (CAWorld.Anomaly && NPCOverride.NPCRegistered(npc.type, out NPCOverrideContainer container) && container.BehaviorOverride.DisableCalamityMethods.HasFlag(DisableCalamityMethods.PreAI))
            return false;

        return orig(self, npc);
    }
}
