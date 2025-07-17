using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.World;

namespace CalamityAnomalies.Core;

public static class CAUtils
{
    public static bool IsDefeatingLeviathan(NPC npc) => npc.LeviathanBoss && !TOIteratorFactory.NewActiveNPCIterator(n => n.LeviathanBoss, npc).Any();

    public static bool IsDefeatingProfanedGuardians(NPC npc) => npc.ProfanedGuardianBoss && !TOIteratorFactory.NewActiveNPCIterator(n => n.ProfanedGuardianBoss, npc).Any();

    public static bool IsDefeatingExoMechs(NPC npc) =>
        npc.Ares && !NPC.ActiveNPCs.Any(n => !n.ExoTwins && !n.Thanatos)
        || npc.ExoTwins && !NPC.ActiveNPCs.Any(n => !n.Ares && !n.Thanatos)
        || npc.ThanatosHead && !NPC.ActiveNPCs.Any(n => !n.ExoTwins && !n.Ares);

    public static bool DownedEvilBossT2 => DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator;

    public static void LogILFailure(string name, string reason) => CAMain.Instance.Logger.Warn($"""IL edit "{name}" failed! {reason}""");

    public static TooltipLine CreateNewTooltipLine(int num, Action<TooltipLine> action, bool tweak)
    {
        TooltipLine newLine = new(CAMain.Instance, $"Tooltip{num}", "");
        if (tweak)
            newLine.OverrideColor = CAMain.GetGradientColor(0.25f);
        action(newLine);
        return newLine;
    }
}
