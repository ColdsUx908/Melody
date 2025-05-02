using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityAnomalies.Contents.AnomalyMode.NPCs;
using CalamityAnomalies.Contents.BossRushMode;
using CalamityMod.Events;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core.IL;

namespace CalamityAnomalies.IL;

/// <summary>
/// 钩子。
/// <br/>应用类：<see cref="CalamityGlobalNPC"/>。
/// </summary>
public class CalGlobalNPCDetour : ITODetourProvider
{
    private static Type Type_CalamityGlobalNPC { get; } = typeof(CalamityGlobalNPC);

    internal static MethodInfo Method_CalamityGlobalNPC_SetDefaults { get; } = Type_CalamityGlobalNPC.GetMethod("SetDefaults", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_ModifyIncomingHit { get; } = Type_CalamityGlobalNPC.GetMethod("ApplyDR", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_PreAI { get; } = Type_CalamityGlobalNPC.GetMethod("PreAI", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_PostAI { get; } = Type_CalamityGlobalNPC.GetMethod("PostAI", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_GetAlpha { get; } = Type_CalamityGlobalNPC.GetMethod("GetAlpha", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_PreDraw { get; } = Type_CalamityGlobalNPC.GetMethod("PreDraw", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs { get; } = Type_CalamityGlobalNPC.GetMethod("BossRushForceDespawnOtherNPCs", TOMain.UniversalBindingFlags);

    internal delegate void Orig_CalamityGlobalNPC_SetDefaults(CalamityGlobalNPC self, NPC npc);
    internal delegate void Orig_CalamityGlobalNPC_ModifyIncomingHit(CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers);
    internal delegate bool Orig_CalamityGlobalNPC_PreAI(CalamityGlobalNPC self, NPC npc);
    internal delegate void Orig_CalamityGlobalNPC_PostAI(CalamityGlobalNPC self, NPC npc);
    internal delegate Color? Orig_CalamityGlobalNPC_GetAlpha(CalamityGlobalNPC self, NPC npc, Color drawColor);
    internal delegate bool Orig_CalamityGlobalNPC_Predraw(CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
    internal delegate void Orig_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs(CalamityGlobalNPC self, NPC npc, Mod mod);

    internal static void On_CalamityGlobalNPC_SetDefaults(Orig_CalamityGlobalNPC_SetDefaults orig, CalamityGlobalNPC self, NPC npc)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = false;
            orig(self, npc);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, npc);
    }

    internal static void On_CalamityGlobalNPC_ModifyIncomingHit(Orig_CalamityGlobalNPC_ModifyIncomingHit orig, CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = false;
            orig(self, npc, ref modifiers);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, npc, ref modifiers);
    }

    internal static bool On_CalamityGlobalNPC_PreAI(Orig_CalamityGlobalNPC_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (CAWorld.Anomaly && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && !anomalyNPCOverride.AllowOrigCalMethod(OrigCalMethodType.PreAI))
            return true;

        if (CAWorld.BossRush)
            BossRushEvent.BossRushActive = true;

        return orig(self, npc);
    }

    internal static void On_CalamityGlobalNPC_PostAI(Orig_CalamityGlobalNPC_PostAI orig, CalamityGlobalNPC self, NPC npc)
    {
        BossRushEvent.BossRushActive = BossRushManageMent.RealBossRushEventActive;
        orig(self, npc);
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

    /// <summary>
    /// 处理BossRush事件的NPC强制消失。
    /// <br/>仅在BossRush模式中生效。
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    /// <param name="npc"></param>
    /// <param name="mod"></param>
    internal static void On_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs(Orig_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs orig, CalamityGlobalNPC self, NPC npc, Mod mod)
    {
        if (CAWorld.BossRush && !BossRushManageMent.RealBossRushEventActive)
            return;

        orig(self, npc, mod);
    }

    Dictionary<MethodInfo, Delegate> ITODetourProvider.DetoursToApply => new()
    {
        [Method_CalamityGlobalNPC_SetDefaults] = On_CalamityGlobalNPC_SetDefaults,
        [Method_CalamityGlobalNPC_ModifyIncomingHit] = On_CalamityGlobalNPC_ModifyIncomingHit,
        [Method_CalamityGlobalNPC_PreAI] = On_CalamityGlobalNPC_PreAI,
        [Method_CalamityGlobalNPC_PostAI] = On_CalamityGlobalNPC_PostAI,
        [Method_CalamityGlobalNPC_GetAlpha] = On_CalamityGlobalNPC_GetAlpha,
        [Method_CalamityGlobalNPC_PreDraw] = On_CalamityGlobalNPC_PreDraw,
        [Method_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs] = On_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs,
    };
}
