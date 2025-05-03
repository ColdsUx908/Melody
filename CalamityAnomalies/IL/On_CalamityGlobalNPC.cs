using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityAnomalies.Contents.AnomalyMode.NPCs;
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
using Transoceanic;
using Transoceanic.Core.IL;
using static CalamityMod.NPCs.CalamityGlobalNPC;

namespace CalamityAnomalies.IL;

/// <summary>
/// 钩子。
/// <br/>应用类：<see cref="CalamityGlobalNPC"/>。
/// </summary>
public class On_CalamityGlobalNPC : ITODetourProvider
{
    internal static void Hook_CalamityGlobalNPC_SetDefaults(Orig_CalamityGlobalNPC_SetDefaults orig, CalamityGlobalNPC self, NPC NPC)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = CAWorld.RealBossRushEventActive;

            //针对特殊NPC的修改，主要是血量
            switch (NPC.ModNPC)
            {
                case CrabShroom:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 8000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 180 : 30;
                    if (Main.getGoodWorld)
                        NPC.lifeMax *= 2;
                    Adjust(NPC, true);
                    break;
                case HiveBlob:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 1300 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 180 : 30;
                    if (Main.getGoodWorld)
                        NPC.lifeMax *= 2;
                    Adjust(NPC, false);
                    break;
                case HiveBlob2:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 1300 : 75;
                    if (Main.getGoodWorld)
                        NPC.lifeMax *= 2;
                    Adjust(NPC, false);
                    break;
                case DankCreeper:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 2000 : 120;
                    if (Main.getGoodWorld)
                        NPC.lifeMax *= 4;
                    Adjust(NPC, true);
                    break;
                case DarkHeart:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 1800 : 75;
                    if (Main.getGoodWorld)
                        NPC.lifeMax *= 4;
                    Adjust(NPC, false);
                    break;
                case PerforatorBodyMedium or PerforatorTailMedium:
                    NPC.LifeMaxNERB(180, 216, 7000);
                    if (Main.zenithWorld)
                        NPC.lifeMax *= 4;
                    Adjust(NPC, true);
                    break;
                case CorruptSlimeSpawn:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 10000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 360 : 180;
                    Adjust(NPC, true);
                    break;
                case CorruptSlimeSpawn2:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 5000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 180 : 90;
                    Adjust(NPC, true);
                    break;
                case CrimsonSlimeSpawn:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 10000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 220 : 110;
                    Adjust(NPC, true);
                    break;
                case CrimsonSlimeSpawn2:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 12000 : (CalamityWorld.LegendaryMode && CalamityWorld.revenge) ? 260 : 130;
                    Adjust(NPC, false);
                    break;
                case CryogenShield:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 10000 : CalamityWorld.death ? 700 : 1400;
                    Adjust(NPC, false);
                    break;
                case Brimling:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 10000 : 1000;
                    Adjust(NPC, false);
                    break;
                case SoulSeeker:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 15000 : CalamityWorld.death ? 1500 : 2500;
                    Adjust(NPC, true);
                    break;
                case AquaticAberration:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 10000 : 600;
                    Adjust(NPC, true);
                    break;
                case AnahitasIceShield:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 1000 : 650;
                    Adjust(NPC, false);
                    break;
                case PlagueHomingMissile:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 5000 : 500;
                    Adjust(NPC, true);
                    break;
                case PlagueMine:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 10000 : 1000;
                    Adjust(NPC, true);
                    break;
                case ProfanedRocks:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? ProfanedRocks.MaxBossRushHP : ProfanedRocks.MaxHP;
                    Adjust(NPC, false);
                    break;
                case Bumblefuck2:
                    NPC.LifeMaxNERB(9375, 11250, 5000);
                    Adjust(NPC, true);
                    break;
                case ProvSpawnHealer:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 20000 : 15000;
                    Adjust(NPC, true);
                    break;
                case ProvSpawnDefense:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 30000 : 18750;
                    Adjust(NPC, true);
                    break;
                case ProvSpawnOffense:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 40000 : 31875;
                    Adjust(NPC, true);
                    break;
                case DarkEnergy:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? DarkEnergy.MaxBossRushHP : DarkEnergy.MaxHP;
                    Adjust(NPC, true);
                    break;
                case OldDukeToothBall:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 16000 : 8000;
                    Adjust(NPC, true);
                    break;
                case SulphurousSharkron:
                    NPC.lifeMax = BossRushEvent.BossRushActive ? 10000 : 6000;
                    Adjust(NPC, true);
                    break;
                case AresBody or AresLaserCannon or AresTeslaCannon or AresGaussNuke or AresPlasmaFlamethrower:
                    NPC.LifeMaxNERB(1250000, 1495000, 650000);
                    Adjust(NPC, false);
                    break;
            }

            orig(self, NPC);
            BossRushEvent.BossRushActive = temp;

            return;
        }

        orig(self, NPC);
    }

    private static void Adjust(NPC npc, bool shouldAdjustStatScaling)
    {
        npc.lifeMax += (int)(npc.lifeMax * CalamityConfig.Instance.BossHealthBoost * 0.01);
        if (shouldAdjustStatScaling && Main.masterMode)
            npc.lifeMax = (int)Math.Round(npc.lifeMax * MasterModeEnemyHPMultiplier); //只更改血量，因为攻击已经在ModNPC的SetDefaults()中处理了
    }

    internal static void Hook_CalamityGlobalNPC_ModifyIncomingHit(Orig_CalamityGlobalNPC_ModifyIncomingHit orig, CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers)
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

    internal static bool Hook_CalamityGlobalNPC_PreAI(Orig_CalamityGlobalNPC_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (CAWorld.Anomaly && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && !anomalyNPCOverride.AllowOrigCalMethod(OrigCalMethodType.PreAI))
            return true;

        if (CAWorld.BossRush)
            BossRushEvent.BossRushActive = true;

        return orig(self, npc);
    }

    internal static void Hook_CalamityGlobalNPC_PostAI(Orig_CalamityGlobalNPC_PostAI orig, CalamityGlobalNPC self, NPC npc)
    {
        orig(self, npc);
        BossRushEvent.BossRushActive = CAWorld.RealBossRushEventActive;
    }

    internal static Color? Hook_CalamityGlobalNPC_GetAlpha(Orig_CalamityGlobalNPC_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (CAWorld.Anomaly && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && !anomalyNPCOverride.AllowOrigCalMethod(OrigCalMethodType.GetAlpha))
            return null;

        return orig(self, npc, drawColor);
    }

    internal static bool Hook_CalamityGlobalNPC_PreDraw(Orig_CalamityGlobalNPC_Predraw orig, CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (CAWorld.Anomaly && AnomalyNPCOverrideHelper.Registered(npc.type, out AnomalyNPCOverride anomalyNPCOverride)
            && anomalyNPCOverride.AllowOrigCalMethod(OrigCalMethodType.PreDraw))
            return true;

        return orig(self, npc, spriteBatch, screenPos, drawColor);
    }

    internal static void Hook_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs(Orig_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs orig, CalamityGlobalNPC self, NPC npc, Mod mod)
    {
        if (CAWorld.BossRush && !CAWorld.RealBossRushEventActive)
            return;

        orig(self, npc, mod);
    }

    internal static void Hook_CalamityGlobalNPC_OnKill(Orig_CalamityGlobalNPC_OnKill orig, CalamityGlobalNPC self, NPC npc)
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

    #region Detour
    private static Type Type_CalamityGlobalNPC { get; } = typeof(CalamityGlobalNPC);

    internal static MethodInfo Method_CalamityGlobalNPC_SetDefaults { get; } = Type_CalamityGlobalNPC.GetMethod("SetDefaults", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_ModifyIncomingHit { get; } = Type_CalamityGlobalNPC.GetMethod("ApplyDR", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_PreAI { get; } = Type_CalamityGlobalNPC.GetMethod("PreAI", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_PostAI { get; } = Type_CalamityGlobalNPC.GetMethod("PostAI", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_GetAlpha { get; } = Type_CalamityGlobalNPC.GetMethod("GetAlpha", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_PreDraw { get; } = Type_CalamityGlobalNPC.GetMethod("PreDraw", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs { get; } = Type_CalamityGlobalNPC.GetMethod("BossRushForceDespawnOtherNPCs", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_CalamityGlobalNPC_OnKill { get; } = Type_CalamityGlobalNPC.GetMethod("OnKill", TOMain.UniversalBindingFlags);

    internal delegate void Orig_CalamityGlobalNPC_SetDefaults(CalamityGlobalNPC self, NPC npc);
    internal delegate void Orig_CalamityGlobalNPC_ModifyIncomingHit(CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers);
    internal delegate bool Orig_CalamityGlobalNPC_PreAI(CalamityGlobalNPC self, NPC npc);
    internal delegate void Orig_CalamityGlobalNPC_PostAI(CalamityGlobalNPC self, NPC npc);
    internal delegate Color? Orig_CalamityGlobalNPC_GetAlpha(CalamityGlobalNPC self, NPC npc, Color drawColor);
    internal delegate bool Orig_CalamityGlobalNPC_Predraw(CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor);
    internal delegate void Orig_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs(CalamityGlobalNPC self, NPC npc, Mod mod);
    internal delegate void Orig_CalamityGlobalNPC_OnKill(CalamityGlobalNPC self, NPC npc);

    Dictionary<MethodInfo, Delegate> ITODetourProvider.DetoursToApply => new()
    {
        [Method_CalamityGlobalNPC_SetDefaults] = Hook_CalamityGlobalNPC_SetDefaults,
        [Method_CalamityGlobalNPC_ModifyIncomingHit] = Hook_CalamityGlobalNPC_ModifyIncomingHit,
        [Method_CalamityGlobalNPC_PreAI] = Hook_CalamityGlobalNPC_PreAI,
        [Method_CalamityGlobalNPC_PostAI] = Hook_CalamityGlobalNPC_PostAI,
        [Method_CalamityGlobalNPC_GetAlpha] = Hook_CalamityGlobalNPC_GetAlpha,
        [Method_CalamityGlobalNPC_PreDraw] = Hook_CalamityGlobalNPC_PreDraw,
        [Method_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs] = Hook_CalamityGlobalNPC_BossRushForceDespawnOtherNPCs,
        [Method_CalamityGlobalNPC_OnKill] = Hook_CalamityGlobalNPC_OnKill,
    };
    #endregion
}
