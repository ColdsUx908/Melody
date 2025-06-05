using CalamityAnomalies;
using CalamityMod.NPCs;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;

namespace CalamityAnomalies;

public static class CAUtils
{
    public static bool IsDefeatingLeviathan(NPC npc) => npc.LeviathanBoss && !TOIteratorFactory.NewActiveNPCIterator(k => k.LeviathanBoss, npc).Any();

    public static bool IsDefeatingProfanedGuardians(NPC npc) => npc.ProfanedGuardianBoss && !TOIteratorFactory.NewActiveNPCIterator(k => k.ProfanedGuardianBoss, npc).Any();

    public static bool IsDefeatingExoMechs(NPC npc) =>
        npc.Ares && !TOMain.ActiveNPCs.Any(k => !k.ExoTwins && !k.Thanatos)
        || npc.ExoTwins && !TOMain.ActiveNPCs.Any(k => !k.Ares && !k.Thanatos)
        || npc.ThanatosHead && !TOMain.ActiveNPCs.Any(k => !k.ExoTwins && !k.Ares);

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
