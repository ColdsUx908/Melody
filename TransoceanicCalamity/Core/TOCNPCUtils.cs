using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.ProfanedGuardians;
using Terraria;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;

namespace TransoceanicCalamity.Core;

public static class TOCNPCUtils
{
    public static bool IsLeviathan(this NPC npc) => npc.active && npc.type == ModContent.NPCType<Leviathan>();

    public static bool IsAnahita(this NPC npc) => npc.active && npc.type == ModContent.NPCType<Anahita>();

    public static bool IsLeviathanBoss(this NPC npc) => npc.IsLeviathan() || npc.IsAnahita();

    public static bool IsProfanedGuardianCommander(this NPC npc) => npc.active && npc.type == ModContent.NPCType<ProfanedGuardianCommander>();

    public static bool IsProfanedGuardianDefender(this NPC npc) => npc.active && npc.type == ModContent.NPCType<ProfanedGuardianDefender>();

    public static bool IsProfanedGuardianHealer(this NPC npc) => npc.active && npc.type == ModContent.NPCType<ProfanedGuardianHealer>();

    public static bool IsProfanedGuardianBoss(this NPC npc) => npc.active && (npc.IsProfanedGuardianCommander() || npc.IsProfanedGuardianDefender() || npc.IsProfanedGuardianHealer());

    public static bool IsThanatos(this NPC npc) => npc.active && (npc.type == ModContent.NPCType<ThanatosHead>() || npc.type == ModContent.NPCType<ThanatosBody1>() ||
    npc.type == ModContent.NPCType<ThanatosBody2>() || npc.type == ModContent.NPCType<ThanatosTail>());

    public static bool IsThanatosHead(this NPC npc) => npc.active && npc.type == ModContent.NPCType<ThanatosHead>();

    public static bool IsExoTwins(this NPC npc) => npc.active && (npc.type == ModContent.NPCType<Artemis>() || npc.type == ModContent.NPCType<Apollo>());

    public static bool IsAres(this NPC npc) => npc.active && (npc.type == ModContent.NPCType<AresBody>() || npc.type == ModContent.NPCType<AresLaserCannon>() || npc.type == ModContent.NPCType<AresTeslaCannon>() ||
        npc.type == ModContent.NPCType<AresGaussNuke>() || npc.type == ModContent.NPCType<AresPlasmaFlamethrower>());

    public static bool IsExoMechs(this NPC npc) => npc.IsThanatos() || npc.IsExoTwins() || npc.IsAres();

    public static bool IsDefeatingLeviathan(NPC npc) => npc.IsLeviathanBoss() && !TOEntityIteratorCreator.NewNPCIterator(IsLeviathanBoss, npc).Any();

    public static bool IsDefeatingProfanedGuardians(NPC npc) => npc.IsProfanedGuardianBoss() && !TOEntityIteratorCreator.NewNPCIterator(IsLeviathanBoss, npc).Any();

    public static bool IsDefeatingExoMechs(NPC npc) =>
        npc.IsAres() && !TOMain.ActiveNPCs.Any(k => !k.IsExoTwins() && !k.IsThanatos()) ||
        npc.IsExoTwins() && !TOMain.ActiveNPCs.Any(k => !k.IsAres() && !k.IsThanatos()) ||
        npc.IsThanatosHead() && !TOMain.ActiveNPCs.Any(k => !k.IsExoTwins() && !k.IsAres());
}