using CalamityMod.Events;
using Transoceanic.IL;

namespace CalamityAnomalies.Events;

[DetourClassTo(typeof(BossRushEvent))]
public class On_BossRushEvent
{
    internal delegate void Orig_End();

    internal static void Detour_End(Orig_End orig)
    {
        orig();
        CAWorld.RealBossRushEventActive = false;
    }
}
