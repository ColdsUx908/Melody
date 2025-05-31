using CalamityMod;
using CalamityMod.World;
using Terraria;
using Transoceanic.IL;

namespace CalamityAnomalies.Utilities;

[DetourClassTo(typeof(CalamityUtils))]
public class On_CalamityUtils
{
    internal delegate void Orig_KillAllHostileProjectiles();

    internal static void Detour_KillAllHostileProjectiles(Orig_KillAllHostileProjectiles orig)
    {
        if (CAWorld.BossRush && !CAWorld.RealBossRushEventActive)
            return;

        orig();
    }

    internal delegate void Orig_LifeMaxNERB(NPC npc, int normal, int? revengeance = null, int? bossRush = null);

    internal static void Detour_LifeMaxNERB(Orig_LifeMaxNERB orig, NPC npc, int normal, int? revengeance = null, int? bossRush = null) => npc.lifeMax =
        CAWorld.RealBossRushEventActive ? bossRush ?? normal
        : CalamityWorld.revenge ? revengeance ?? normal
        : normal;
}
