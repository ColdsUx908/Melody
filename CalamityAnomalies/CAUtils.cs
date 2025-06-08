using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.World;

namespace CalamityAnomalies;

public static class CAUtils
{
    public static bool IsDefeatingLeviathan(NPC npc) => npc.LeviathanBoss && !TOIteratorFactory.NewActiveNPCIterator(k => k.LeviathanBoss, npc).Any();

    public static bool IsDefeatingProfanedGuardians(NPC npc) => npc.ProfanedGuardianBoss && !TOIteratorFactory.NewActiveNPCIterator(k => k.ProfanedGuardianBoss, npc).Any();

    public static bool IsDefeatingExoMechs(NPC npc) =>
        npc.Ares && !NPC.ActiveNPCs.Any(k => !k.ExoTwins && !k.Thanatos)
        || npc.ExoTwins && !NPC.ActiveNPCs.Any(k => !k.Ares && !k.Thanatos)
        || npc.ThanatosHead && !NPC.ActiveNPCs.Any(k => !k.ExoTwins && !k.Ares);

    public static bool DownedEvilBossT2 => DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator;

    public static bool PermaFrostActive
    {
        get
        {
            if (CalamityGlobalNPC.SCal != -1)
            {
                NPC supremeCalamitas = Main.npc[CalamityGlobalNPC.SCal];
                return supremeCalamitas.active && supremeCalamitas.GetModNPC<SupremeCalamitas>().permafrost;
            }
            return false;
        }
    }
}

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
