using System.Text.RegularExpressions;
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

namespace CalamityAnomalies.Core;

public static class CAExtensions
{
    extension(Item item)
    {
        public CAGlobalItem Anomaly() => item.GetGlobalItem<CAGlobalItem>();

        public bool TryGetBehavior(out CAItemBehavior itemBehavior, [CallerMemberName] string methodName = null!) => CABehaviorHelper.ItemBehaviors.TryGetBehavior(item, methodName, out itemBehavior);
    }

    extension(List<TooltipLine> tooltips)
    {
        public void ModifyTooltip_CATweak(Predicate<TooltipLine> predicate, Action<TooltipLine> action)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(action);
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];
                if (predicate(line))
                {
                    line.OverrideColor = CAMain.GetGradientColor(0.25f);
                    action(line);
                    return;
                }
            }
        }

        public void ModifyVanillaTooltipByName_CATweak(string name, Action<TooltipLine> action) =>
            tooltips.ModifyTooltip_CATweak(l => l.Mod == "Terraria" && l.Name == name, action);

        public void ModifyTooltipByNum_CATweak(int num, Action<TooltipLine> action) =>
            tooltips.ModifyVanillaTooltipByName_CATweak($"Tooltip{num}", action);

        public void ModifyOrAddCATooltip(int num, Action<TooltipLine> action, bool tweak = true)
        {
            ArgumentNullException.ThrowIfNull(action);

            for (int i = tooltips.Count - 1; i >= 0; i--)
            {
                TooltipLine line = tooltips[i];

                if (line.Mod == "CalamityAnomalies")
                {
                    Match match = Regex.Match(line.Name, """^Tooltip(\d+)$""");
                    if (!match.Success)
                        continue;
                    int lineNum = int.Parse(match.Groups[1].Value);
                    if (lineNum == num)
                    {
                        action(line);
                        return;
                    }
                    if (lineNum < num)
                    {
                        tooltips.Insert(i + 1, CAUtils.CreateNewTooltipLine(num, action, tweak));
                        return;
                    }
                }
            }

            int lastTerrariaIndex = tooltips.FindLastTerrariaTooltipIndex();
            if (lastTerrariaIndex == -1)
                tooltips.Add(CAUtils.CreateNewTooltipLine(num, action, tweak));
            else
                tooltips.Insert(lastTerrariaIndex + 1, CAUtils.CreateNewTooltipLine(num, action, tweak));
        }
    }

    extension(NPC npc)
    {
        public CAGlobalNPC Anomaly() => npc.GetGlobalNPC<CAGlobalNPC>();

        public bool TryGetBehavior(out CANPCBehavior npcBehavior, [CallerMemberName] string methodName = null!) => CABehaviorHelper.NPCBehaviors.TryGetBehavior(npc, methodName, out npcBehavior);

        public bool DesertScourge => npc.ModNPC is DesertScourgeHead or DesertScourgeBody or DesertScourgeTail;

        public bool DesertNuisance => npc.ModNPC is DesertNuisanceHead or DesertNuisanceBody or DesertNuisanceTail;

        public bool DesertNuisanceYoung => npc.ModNPC is DesertNuisanceHeadYoung or DesertNuisanceBodyYoung or DesertNuisanceTailYoung;

        public bool AquaticScourge => npc.ModNPC is AquaticScourgeHead or AquaticScourgeBody or AquaticScourgeTail;

        public bool LeviathanBoss => npc.ModNPC is Leviathan or Anahita;

        public bool Ravager => npc.ModNPC is RavagerBody or RavagerClawLeft or RavagerClawRight or RavagerLegLeft or RavagerLegRight or RavagerHead or RavagerHead2;

        public bool ProfanedGuardianBoss => npc.ModNPC is ProfanedGuardianCommander or ProfanedGuardianDefender or ProfanedGuardianHealer;

        public bool ProfanedGuardianSpawned => npc.ModNPC is ProvSpawnOffense or ProvSpawnDefense or ProvSpawnHealer;

        public bool StormWeaver => npc.ModNPC is StormWeaverHead or StormWeaverBody or StormWeaverTail;

        public bool DoG => npc.ModNPC is DevourerofGodsHead or DevourerofGodsBody or DevourerofGodsTail;

        public bool CosmicGuardian => npc.ModNPC is CosmicGuardianHead or CosmicGuardianBody or CosmicGuardianTail;

        public bool Thanatos => npc.active && npc.ModNPC is ThanatosHead or ThanatosBody1 or ThanatosBody2 or ThanatosTail;

        public bool ThanatosHead => npc.ModNPC is ThanatosHead;

        public bool ExoTwins => npc.ModNPC is Artemis or Apollo;

        public bool Ares => npc.ModNPC is AresLaserCannon or AresTeslaCannon or AresGaussNuke or AresPlasmaFlamethrower;

        public bool ExoMechs => npc.Thanatos || npc.ExoTwins || npc.Ares;

        public void ApplyCalamityBossHealthBoost() => npc.lifeMax += (int)(npc.lifeMax * CalamityConfig.Instance.BossHealthBoost * 0.01);

        public int GetProjectileDamage<T>() where T : ModProjectile => npc.GetProjectileDamage(ModContent.ProjectileType<T>());
    }

    extension(Player player)
    {
        public CAPlayer Anomaly() => player.GetModPlayer<CAPlayer>();

        public NewCalamityPlayer NewCalamity() => player.GetModPlayer<NewCalamityPlayer>();
    }

    extension(Projectile projectile)
    {
        public CAGlobalProjectile Anomaly() => projectile.GetGlobalProjectile<CAGlobalProjectile>();

        public bool TryGetBehavior(out CAProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null!) => CABehaviorHelper.ProjectileBehaviors.TryGetBehavior(projectile, methodName, out projectileBehavior);
    }
}
