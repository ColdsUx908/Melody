using System;
using CalamityAnomalies.Override;
using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override bool PreAI(NPC npc)
    {
        CalamityGlobalNPC calamityNPC = npc.Calamity();

        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
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

            AnomalyAITimer++;
            if (CAWorld.AnomalyUltramundane)
            {
                AnomalyUltraAITimer++;
                AnomalyUltraBarTimer = Math.Clamp(AnomalyUltraBarTimer + 1, 0, 120);
            }
            else
            {
                AnomalyUltraAITimer = 0;
                AnomalyUltraBarTimer = Math.Clamp(AnomalyUltraBarTimer - 4, 0, 120);
            }

            if (!npcOverride.PreAI())
                return false;
            #endregion
        }

        AnomalyAITimer = 0;

        if (CAWorld.BossRush)
            BossRushAITimer++;
        else
            BossRushAITimer = 0;

        return true;
    }

    public override void AI(NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
            npcOverride.AI();
    }

    public override void PostAI(NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
            npcOverride.PostAI();
    }
}
