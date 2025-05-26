using CalamityMod.Events;
using Transoceanic.Core.IL;

namespace CalamityAnomalies.Events;

[TODetour(typeof(BossRushEvent))]
public class On_BossRushEvent
{
    internal delegate void Orig_End();

    internal static void Detour_End(Orig_End orig)
    {
        orig();
        CAWorld.RealBossRushEventActive = false;
    }
}
