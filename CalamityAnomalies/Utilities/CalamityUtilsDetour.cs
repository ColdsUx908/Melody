using CalamityMod.World;

namespace CalamityAnomalies.Utilities;

[DetourClassTo(typeof(CalamityUtils))]
public class CalamityUtilsDetour
{
    public delegate void Orig_KillAllHostileProjectiles();

    public static void Detour_KillAllHostileProjectiles(Orig_KillAllHostileProjectiles orig)
    {
        if (CAWorld.BossRush && !CAWorld.RealBossRushEventActive)
            return;

        orig();
    }

    public delegate void Orig_LifeMaxNERB(NPC npc, int normal, int? revengeance = null, int? bossRush = null);

    public static void Detour_LifeMaxNERB(Orig_LifeMaxNERB orig, NPC npc, int normal, int? revengeance = null, int? bossRush = null) => npc.lifeMax =
        CAWorld.RealBossRushEventActive ? bossRush ?? normal
        : CalamityWorld.revenge ? revengeance ?? normal
        : normal;
}
