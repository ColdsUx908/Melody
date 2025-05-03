using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using CalamityMod.Events;
using CalamityMod.World;
using Terraria;
using Transoceanic;
using Transoceanic.Core.IL;

namespace CalamityAnomalies.IL;

/// <summary>
/// 钩子。
/// <br/>应用类：<see cref="CalamityUtils"/>（静态）。
/// </summary>
public class On_CalamityUtils : ITODetourProvider
{
    internal static void Hook_CalamityUtils_KillAllHostileProjectiles(Orig_CalamityUtils_KillAllHostileProjectiles orig)
    {
        if (CAWorld.BossRush && !CAWorld.RealBossRushEventActive)
            return;

        orig();
    }

    internal static void Hook_CalamityUtils_LifeMaxNERB(Orig_CalamityUtils_LifeMaxNERB orig, NPC npc, int normal, int? revengeance = null, int? bossRush = null)
    {
        npc.lifeMax = normal;

        if (bossRush.HasValue && CAWorld.RealBossRushEventActive)
            npc.lifeMax = bossRush.Value;
        else if (revengeance.HasValue && CalamityWorld.revenge)
            npc.lifeMax = revengeance.Value;
    }

    #region Detour
    internal static Type Type_CalamityUtils { get; } = typeof(CalamityUtils);

    internal static MethodInfo Method_CalamityUtils_KillAllHostileProjectiles { get; } = Type_CalamityUtils.GetMethod("KillAllHostileProjectiles", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityUtils_LifeMaxNERB { get; } = Type_CalamityUtils.GetMethod("LifeMaxNERB", TOMain.UniversalBindingFlags);

    internal delegate void Orig_CalamityUtils_KillAllHostileProjectiles();
    internal delegate void Orig_CalamityUtils_LifeMaxNERB(NPC npc, int normal, int? revengeance = null, int? bossRush = null);

    Dictionary<MethodInfo, Delegate> ITODetourProvider.DetoursToApply => new()
    {
        [Method_CalamityUtils_KillAllHostileProjectiles] = Hook_CalamityUtils_KillAllHostileProjectiles,
        [Method_CalamityUtils_LifeMaxNERB] = Hook_CalamityUtils_LifeMaxNERB,
    };
    #endregion
}
