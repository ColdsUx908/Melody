﻿using System;
using CalamityAnomalies.Override;
using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Transoceanic.Core.IL;
using static CalamityMod.NPCs.CalamityGlobalNPC;

namespace CalamityAnomalies.IL;

[TODetour(typeof(CalamityGlobalNPC))]
public class On_CalamityGlobalNPC
{
    internal delegate void Orig_SetDefaults(CalamityGlobalNPC self, NPC npc);

    internal static void Detour_SetDefaults(Orig_SetDefaults orig, CalamityGlobalNPC self, NPC npc)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = CAWorld.RealBossRushEventActive;

            //针对特殊NPC的修改，主要是血量
            switch (npc.ModNPC)
            {
                case CrabShroom:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 8000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 180 : 30;
                    if (Main.getGoodWorld)
                        npc.lifeMax *= 2;
                    Adjust(npc, true);
                    break;
                case HiveBlob:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 1300 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 180 : 30;
                    if (Main.getGoodWorld)
                        npc.lifeMax *= 2;
                    Adjust(npc, false);
                    break;
                case HiveBlob2:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 1300 : 75;
                    if (Main.getGoodWorld)
                        npc.lifeMax *= 2;
                    Adjust(npc, false);
                    break;
                case DankCreeper:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 2000 : 120;
                    if (Main.getGoodWorld)
                        npc.lifeMax *= 4;
                    npc.knockBackResist = 0.3f;
                    Adjust(npc, true);
                    break;
                case DarkHeart:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 1800 : 75;
                    if (Main.getGoodWorld)
                        npc.lifeMax *= 4;
                    Adjust(npc, false);
                    break;
                case PerforatorBodyMedium or PerforatorTailMedium:
                    npc.LifeMaxNERB(180, 216, 7000);
                    if (Main.zenithWorld)
                        npc.lifeMax *= 4;
                    Adjust(npc, true);
                    break;
                case CorruptSlimeSpawn:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 10000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 360 : 180;
                    Adjust(npc, true);
                    break;
                case CorruptSlimeSpawn2:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 5000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 180 : 90;
                    Adjust(npc, true);
                    break;
                case CrimsonSlimeSpawn:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 10000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 220 : 110;
                    Adjust(npc, true);
                    break;
                case CrimsonSlimeSpawn2:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 12000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 260 : 130;
                    Adjust(npc, false);
                    break;
                case CryogenShield:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 10000 : CalamityWorld.death ? 700 : 1400;
                    Adjust(npc, false);
                    break;
                case Brimling:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 10000 : 1000;
                    Adjust(npc, false);
                    break;
                case SoulSeeker:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 15000 : CalamityWorld.death ? 1500 : 2500;
                    Adjust(npc, true);
                    break;
                case AquaticAberration:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 10000 : 600;
                    Adjust(npc, true);
                    break;
                case AnahitasIceShield:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 1000 : 650;
                    Adjust(npc, false);
                    break;
                case PlagueHomingMissile:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 5000 : 500;
                    Adjust(npc, true);
                    break;
                case PlagueMine:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 10000 : 1000;
                    Adjust(npc, true);
                    break;
                case ProfanedRocks:
                    npc.lifeMax = BossRushEvent.BossRushActive ? ProfanedRocks.MaxBossRushHP : ProfanedRocks.MaxHP;
                    Adjust(npc, false);
                    break;
                case Bumblefuck2:
                    npc.LifeMaxNERB(9375, 11250, 5000);
                    Adjust(npc, true);
                    break;
                case ProvSpawnHealer:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 20000 : 15000;
                    Adjust(npc, true);
                    break;
                case ProvSpawnDefense:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 30000 : 18750;
                    Adjust(npc, true);
                    break;
                case ProvSpawnOffense:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 40000 : 31875;
                    Adjust(npc, true);
                    break;
                case DarkEnergy:
                    npc.lifeMax = BossRushEvent.BossRushActive ? DarkEnergy.MaxBossRushHP : DarkEnergy.MaxHP;
                    Adjust(npc, true);
                    break;
                case OldDukeToothBall:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 16000 : 8000;
                    Adjust(npc, true);
                    break;
                case SulphurousSharkron:
                    npc.lifeMax = BossRushEvent.BossRushActive ? 10000 : 6000;
                    Adjust(npc, true);
                    break;
                case AresBody or AresLaserCannon or AresTeslaCannon or AresGaussNuke or AresPlasmaFlamethrower:
                    npc.LifeMaxNERB(1250000, 1495000, 650000);
                    Adjust(npc, false);
                    break;
            }

            orig(self, npc);
            BossRushEvent.BossRushActive = temp;

            return;
        }

        orig(self, npc);
    }

    private static void Adjust(NPC npc, bool shouldAdjustStatScaling)
    {
        npc.lifeMax += (int)(npc.lifeMax * CalamityConfig.Instance.BossHealthBoost * 0.01);
        if (shouldAdjustStatScaling && Main.masterMode)
            npc.lifeMax = (int)Math.Round(npc.lifeMax * MasterModeEnemyHPMultiplier); //只更改血量，因为攻击已经在ModNPC的SetDefaults()中处理了
    }

    internal delegate void Orig_ModifyIncomingHit(CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers);

    internal static void Detour_ModifyIncomingHit(Orig_ModifyIncomingHit orig, CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = false;
            orig(self, npc, ref modifiers);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, npc, ref modifiers);
    }

    internal delegate bool Orig_PreAI(CalamityGlobalNPC self, NPC npc);

    internal static bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.PreAI))
                return true;
        }

        if (CAWorld.BossRush)
            BossRushEvent.BossRushActive = true;

        return orig(self, npc);
    }

    internal delegate void Orig_PostAI(CalamityGlobalNPC self, NPC npc);

    internal static void Detour_PostAI(Orig_PostAI orig, CalamityGlobalNPC self, NPC npc)
    {
        orig(self, npc);
        BossRushEvent.BossRushActive = CAWorld.RealBossRushEventActive;
    }

    internal delegate Color? Orig_GetAlpha(CalamityGlobalNPC self, NPC npc, Color drawColor);

    internal static Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.GetAlpha))
                return null;
        }

        return orig(self, npc, drawColor);
    }

    internal delegate bool Orig_Predraw(CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);

    internal static bool Detour_PreDraw(Orig_Predraw orig, CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.HasNPCOverride(out CANPCOverride npcOverride))
        {
            if (npcOverride.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.PreDraw))
                return true;
        }

        return orig(self, npc, spriteBatch, screenPos, drawColor);
    }

    internal delegate void Orig_BossRushForceDespawnOtherNPCs(CalamityGlobalNPC self, NPC npc, Mod mod);

    internal static void Detour_BossRushForceDespawnOtherNPCs(Orig_BossRushForceDespawnOtherNPCs orig, CalamityGlobalNPC self, NPC npc, Mod mod)
    {
        if (CAWorld.BossRush && !CAWorld.RealBossRushEventActive)
            return;

        orig(self, npc, mod);
    }

    internal delegate void Orig_OnKill(CalamityGlobalNPC self, NPC npc);

    internal static void Detour_OnKill(Orig_OnKill orig, CalamityGlobalNPC self, NPC npc)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = CAWorld.RealBossRushEventActive;
            orig(self, npc);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, npc);
    }
}
