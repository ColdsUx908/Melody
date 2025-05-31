using CalamityMod.Events;
using Transoceanic.IL;

namespace CalamityAnomalies.Events;

[DetourClassTo<BossRushEvent>]
public class BossRushEventDetour
{
    public delegate void Orig_End();

    public static void Detour_End(Orig_End orig)
    {
        orig();
        CAWorld.RealBossRushEventActive = false;
    }
}
