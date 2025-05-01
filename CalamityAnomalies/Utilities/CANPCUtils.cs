using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.ProfanedGuardians;
using Terraria;
using Transoceanic;
using Transoceanic.Core.GameData;

namespace CalamityAnomalies.Utilities;

public static partial class CAUtils
{
    public static bool IsLeviathan(this NPC npc) => npc.ModNPC is Leviathan;

    public static bool IsAnahita(this NPC npc) => npc.ModNPC is Anahita;

    public static bool IsLeviathanBoss(this NPC npc) => npc.ModNPC is Leviathan or Anahita;

    public static bool IsProfanedGuardianCommander(this NPC npc) => npc.ModNPC is ProfanedGuardianCommander;

    public static bool IsProfanedGuardianDefender(this NPC npc) => npc.ModNPC is ProfanedGuardianDefender;

    public static bool IsProfanedGuardianHealer(this NPC npc) => npc.ModNPC is ProfanedGuardianHealer;

    public static bool IsProfanedGuardianBoss(this NPC npc) => npc.ModNPC is ProfanedGuardianCommander or ProfanedGuardianDefender or ProfanedGuardianHealer;

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
}
