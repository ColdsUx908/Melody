using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CalamityAnomalies.Contents.BossRushMode;
using CalamityMod.Events;
using CalamityMod.Projectiles;
using Terraria;
using Transoceanic;
using Transoceanic.Core;
using Transoceanic.Core.IL;

namespace CalamityAnomalies.IL;

/// <summary>
/// 钩子。
/// <br/>应用类：<see cref="CalamityGlobalProjectile"/>。
/// </summary>
public class CalGlobalProjectileDetour : ITODetourProvider
{
    internal static Type Type_CalamityGlobalProjectile { get; } = typeof(CalamityGlobalProjectile);

    internal static MethodInfo Method_CalamityGlobalProjectile_SetDefaults { get; } = Type_CalamityGlobalProjectile.GetMethod("SetDefaults", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalProjectile_PreAI { get; } = Type_CalamityGlobalProjectile.GetMethod("PreAI", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalProjectile_PostAI { get; } = Type_CalamityGlobalProjectile.GetMethod("PostAI", TOMain.UniversalBindingFlags);

    internal delegate void Orig_CalamityGlobalProjectile_SetDefaults(CalamityGlobalProjectile self, Projectile projectile);
    internal delegate bool Orig_CalamityGlobalProjectile_PreAI(CalamityGlobalProjectile self, Projectile projectile);
    internal delegate void Orig_CalamityGlobalProjectile_PostAI(CalamityGlobalProjectile self, Projectile projectile);

    internal static void On_CalamityGlobalProjectile_SetDefaults(Orig_CalamityGlobalProjectile_SetDefaults orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = false;
            orig(self, projectile);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, projectile);
    }

    internal static bool On_CalamityGlobalProjectile_PreAI(Orig_CalamityGlobalProjectile_PreAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (CAWorld.BossRush)
            BossRushEvent.BossRushActive = true;

        return orig(self, projectile);
    }

    internal static void On_CalamityGlobalProjectile_PostAI(Orig_CalamityGlobalProjectile_PostAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        orig(self, projectile);
        BossRushEvent.BossRushActive = BossRushManageMent.RealBossRushEventActive;
    }

    Dictionary<MethodInfo, Delegate> ITODetourProvider.DetoursToApply => new()
    {
        [Method_CalamityGlobalProjectile_SetDefaults] = On_CalamityGlobalProjectile_SetDefaults,
        [Method_CalamityGlobalProjectile_PreAI] = On_CalamityGlobalProjectile_PreAI,
        [Method_CalamityGlobalProjectile_PostAI] = On_CalamityGlobalProjectile_PostAI,
    };
}
