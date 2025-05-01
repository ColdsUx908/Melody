using System;
using System.Reflection;
using CalamityAnomalies.Contents.AnomalyNPCs;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Transoceanic;
using Transoceanic.Core;
using Transoceanic.Core.IL;

namespace CalamityAnomalies.IL;

public partial class CANPCHook : ITODetourProvider, ITOLoader
{
    private static Type Type_CalamityGlobalNPC => typeof(CalamityGlobalNPC);

    public static MethodInfo Method_CalamityGlobalNPC_PreAI => Type_CalamityGlobalNPC.GetMethod("PreAI", TOMain.UniversalBindingFlags);
    public static MethodInfo Method_CalamityGlobalNPC_GetAlpha => Type_CalamityGlobalNPC.GetMethod("GetAlpha", TOMain.UniversalBindingFlags);
    public static MethodInfo Method_CalamityGlobalNPC_PreDraw => Type_CalamityGlobalNPC.GetMethod("PreDraw", TOMain.UniversalBindingFlags);

    public delegate bool Orig_CalamityGlobalNPC_PreAI(CalamityGlobalNPC self, NPC npc);
    public delegate Color? Orig_CalamityGlobalNPC_GetAlpha(CalamityGlobalNPC self, NPC npc, Color drawColor);
    public delegate bool Orig_CalamityGlobalNPC_Predraw(CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);

    internal static bool On_CalamityGlobalNPC_PreAI(Orig_CalamityGlobalNPC_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (CAWorld.Anomaly && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && !anomalyNPCOverride.AllowOrigCalMethod(OrigCalMethodType.PreAI))
            return true;

        return orig(self, npc);
    }

    internal static Color? On_CalamityGlobalNPC_GetAlpha(Orig_CalamityGlobalNPC_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (CAWorld.Anomaly && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && !anomalyNPCOverride.AllowOrigCalMethod(OrigCalMethodType.GetAlpha))
            return null;

        return orig(self, npc, drawColor);
    }

    internal static bool On_CalamityGlobalNPC_PreDraw(Orig_CalamityGlobalNPC_Predraw orig, CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (CAWorld.Anomaly && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && anomalyNPCOverride.AllowOrigCalMethod(OrigCalMethodType.PreDraw))
            return true;

        return orig(self, npc, spriteBatch, screenPos, drawColor);
    }
}
