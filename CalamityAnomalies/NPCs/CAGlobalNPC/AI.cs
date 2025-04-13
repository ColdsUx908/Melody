using System;
using CalamityAnomalies.Systems;
using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic.Core;
using Transoceanic.GlobalEntity.GlobalNPCs;
using TransoceanicCalamity.Core;
using TransoceanicCalamity.GlobalEntity.GlobalNPCs;

namespace CalamityAnomalies.NPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override bool PreAI(NPC npc)
    {
        TOGlobalNPC oceanNPC = npc.Ocean();
        TOCGlobalNPC oceanCalNPC = npc.OceanCal();
        CAGlobalNPC anomalyNPC = npc.Anomaly();
        CalamityGlobalNPC calamityNPC = npc.Calamity();

        if (!CAWorld.Anomaly ||
            !NPCOverride.NPCRegistered(npc.type, out NPCOverrideContainer container) ||
            !container.HasCustomMethods.HasFlag(HasCustomNPCMethods.AnomalyAI) ||
            !shouldRunAnomalyAI) //只在需要时运行异象AI
        {
            AnomalyAITimer = 0;
            return true;
        }

        #region
        //禁用灾厄动态伤害减免。
        if (calamityNPC.KillTime >= 1 && calamityNPC.AITimer < calamityNPC.KillTime)
            calamityNPC.AITimer = calamityNPC.KillTime;

        // Disable netOffset effects.
        npc.netOffset = Vector2.Zero;

        // Disable the effects of certain unpredictable freeze debuffs.
        // Time Bolt and a few other weapon-specific debuffs are not counted here since those are more deliberate weapon mechanics.
        // That said, I don't know a single person who uses Time Bolt so it's probably irrelevant either way lol.
        npc.buffImmune[ModContent.BuffType<Eutrophication>()] = true;
        npc.buffImmune[ModContent.BuffType<GalvanicCorrosion>()] = true;
        npc.buffImmune[ModContent.BuffType<GlacialState>()] = true;
        npc.buffImmune[ModContent.BuffType<TemporalSadness>()] = true;
        npc.buffImmune[BuffID.Webbed] = true;

        NPCOverride behaviorOverride = container.BehaviorOverride;
        AnomalyAITimer++;
        if (CAWorld.AnomalyUltramundane)
        {
            AnomalyUltraAITimer++;
            AnomalyUltraBarTimer = Math.Min(AnomalyUltraBarTimer + 1, 120);
        }
        else
        {
            AnomalyUltraAITimer = 0;
            AnomalyUltraBarTimer = Math.Max(AnomalyUltraBarTimer - 4, 0);
        }
        behaviorOverride.TryConnectWithNPC(npc);
        behaviorOverride.AnomalyAI();
        behaviorOverride.ClearNPCInstances();
        return !behaviorOverride.DisableAI;
        #endregion
    }
}
