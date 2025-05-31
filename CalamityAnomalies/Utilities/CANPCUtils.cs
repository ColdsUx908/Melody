using CalamityMod;
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
using Terraria;
using Transoceanic;
using Transoceanic.GameData;
using Transoceanic.GameData.Utilities;

namespace CalamityAnomalies.Utilities;

public static class CANPCUtils
{
    public static bool IsDesertScourge(this NPC npc) => npc.ModNPC is DesertScourgeHead or DesertScourgeBody or DesertScourgeTail;

    public static bool IsDesertNuisance(this NPC npc) => npc.ModNPC is DesertNuisanceHead or DesertNuisanceBody or DesertNuisanceTail;

    public static bool IsDesertNuisanceYoung(this NPC npc) => npc.ModNPC is DesertNuisanceHeadYoung or DesertNuisanceBodyYoung or DesertNuisanceTailYoung;

    public static bool IsAquaticScourge(this NPC npc) => npc.ModNPC is AquaticScourgeHead or AquaticScourgeBody or AquaticScourgeTail;

    public static bool IsLeviathanBoss(this NPC npc) => npc.ModNPC is Leviathan or Anahita;

    public static bool IsRavager(this NPC npc) => npc.ModNPC is RavagerBody or RavagerClawLeft or RavagerClawRight or RavagerLegLeft or RavagerLegRight or RavagerHead or RavagerHead2;

    public static bool IsProfanedGuardianBoss(this NPC npc) => npc.ModNPC is ProfanedGuardianCommander or ProfanedGuardianDefender or ProfanedGuardianHealer;

    public static bool IsProfanedGuardianSpawned(this NPC npc) => npc.ModNPC is ProvSpawnOffense or ProvSpawnDefense or ProvSpawnHealer;

    public static bool IsStormWeaver(this NPC npc) => npc.ModNPC is StormWeaverHead or StormWeaverBody or StormWeaverTail;

    public static bool IsDoG(this NPC npc) => npc.ModNPC is DevourerofGodsHead or DevourerofGodsBody or DevourerofGodsTail;

    public static bool IsCosmicGuardian(this NPC npc) => npc.ModNPC is CosmicGuardianHead or CosmicGuardianBody or CosmicGuardianTail;

    public static bool IsThanatos(this NPC npc) => npc.active && npc.ModNPC is ThanatosHead or ThanatosBody1 or ThanatosBody2 or ThanatosTail;

    public static bool IsThanatosHead(this NPC npc) => npc.ModNPC is ThanatosHead;

    public static bool IsExoTwins(this NPC npc) => npc.ModNPC is Artemis or Apollo;

    public static bool IsAres(this NPC npc) => npc.ModNPC is AresLaserCannon or AresTeslaCannon or AresGaussNuke or AresPlasmaFlamethrower;

    public static bool IsExoMechs(this NPC npc) => npc.IsThanatos() || npc.IsExoTwins() || npc.IsAres();

    public static bool IsDefeatingLeviathan(NPC npc) => npc.IsLeviathanBoss() && !TOIteratorFactory.NewActiveNPCIterator(IsLeviathanBoss, npc).Any();

    public static bool IsDefeatingProfanedGuardians(NPC npc) => npc.IsProfanedGuardianBoss() && !TOIteratorFactory.NewActiveNPCIterator(IsProfanedGuardianBoss, npc).Any();

    public static bool IsDefeatingExoMechs(NPC npc) =>
        npc.IsAres() && !TOMain.ActiveNPCs.Any(k => !k.IsExoTwins() && !k.IsThanatos())
        || npc.IsExoTwins() && !TOMain.ActiveNPCs.Any(k => !k.IsAres() && !k.IsThanatos())
        || npc.IsThanatosHead() && !TOMain.ActiveNPCs.Any(k => !k.IsExoTwins() && !k.IsAres());

    public static bool DownedEvilBossT2 => DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator;

    public static bool CirrusActive
    {
        get
        {
            if (CalamityGlobalNPC.SCal != -1)
            {
                NPC supremeCalamitas = Main.npc[CalamityGlobalNPC.SCal];
                return supremeCalamitas.active && supremeCalamitas.GetModNPC<SupremeCalamitas>().cirrus;
            }
            return false;
        }
    }
}
