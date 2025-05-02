using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CalamityAnomalies.Contents.BossRushMode;
using CalamityMod;
using Transoceanic;
using Transoceanic.Core.IL;

namespace CalamityAnomalies.IL;

/// <summary>
/// 钩子。
/// <br/>应用类：<see cref="CalamityUtils"/>（静态）。
/// </summary>
public class CalUtilsDetour : ITODetourProvider
{
    internal static Type Type_CalamityUtils { get; } = typeof(CalamityUtils);
    internal static MethodInfo Method_CalamityUtils_KillAllHostileProjectiles { get; } = Type_CalamityUtils.GetMethod("KillAllHostileProjectiles", TOMain.UniversalBindingFlags);

    internal delegate void Orig_CalamityUtils_KillAllHostileProjectiles();

    internal static void On_CalamityUtils_KillAllHostileProjectiles(Orig_CalamityUtils_KillAllHostileProjectiles orig)
    {
        if (CAWorld.BossRush && !BossRushManageMent.RealBossRushEventActive)
            return;

        orig();
    }

    Dictionary<MethodInfo, Delegate> ITODetourProvider.DetoursToApply => new()
    {
        [Method_CalamityUtils_KillAllHostileProjectiles] = On_CalamityUtils_KillAllHostileProjectiles,
    };
}
