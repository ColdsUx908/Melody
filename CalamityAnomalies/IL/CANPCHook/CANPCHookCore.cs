using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic.Core;
using Transoceanic.Core.IL;

namespace CalamityAnomalies.IL;

public partial class CANPCHook : ITODetourProvider, ITOLoader
{
    Dictionary<MethodInfo, Delegate> ITODetourProvider.DetoursToApply => new()
    {
        [Method_CalamityGlobalNPC_PreAI] = On_CalamityGlobalNPC_PreAI,
        [Method_CalamityGlobalNPC_GetAlpha] = On_CalamityGlobalNPC_GetAlpha,
        [Method_CalamityGlobalNPC_PreDraw] = On_CalamityGlobalNPC_PreDraw,
        [Method_BossHealthBarManager_Draw] = On_BossHealthBarManager_Draw,
        [Method_BossHealthBarManager_Update] = On_BossHealthBarManager_Update,
    };

    void ITOLoader.PostSetupContent()
    {
        BossHealthBarManager.MinibossHPBarList.Add(NPCID.LunarTowerVortex);
        BossHealthBarManager.MinibossHPBarList.Add(NPCID.LunarTowerStardust);
        BossHealthBarManager.MinibossHPBarList.Add(NPCID.LunarTowerNebula);
        BossHealthBarManager.MinibossHPBarList.Add(NPCID.LunarTowerSolar);
        BossHealthBarManager.MinibossHPBarList.Add(NPCID.PirateShip);
        BossHealthBarManager.OneToMany[NPCID.SkeletronHead] = [NPCID.SkeletronHand];
        BossHealthBarManager.OneToMany[NPCID.SkeletronPrime] = [NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeCannon, NPCID.PrimeLaser];
        BossHealthBarManager.OneToMany[NPCID.Golem] = [NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead, NPCID.GolemHeadFree];
        BossHealthBarManager.OneToMany[NPCID.BrainofCthulhu] = [NPCID.Creeper];
        BossHealthBarManager.OneToMany[NPCID.MartianSaucerCore] = [NPCID.MartianSaucerTurret, NPCID.MartianSaucerCannon];
        BossHealthBarManager.OneToMany[NPCID.PirateShip] = [NPCID.PirateShipCannon];
        BossHealthBarManager.OneToMany[ModContent.NPCType<CeaselessVoid>()] = [ModContent.NPCType<DarkEnergy>()];
        BossHealthBarManager.OneToMany[ModContent.NPCType<RavagerBody>()] =
        [
            ModContent.NPCType<RavagerClawRight>(),
            ModContent.NPCType<RavagerClawLeft>(),
            ModContent.NPCType<RavagerLegRight>(),
            ModContent.NPCType<RavagerLegLeft>(),
            ModContent.NPCType<RavagerHead>()
        ];
        BossHealthBarManager.OneToMany[ModContent.NPCType<EbonianPaladin>()] = [];
        BossHealthBarManager.OneToMany[ModContent.NPCType<CrimulanPaladin>()] = [];
    }

    void ITOLoader.OnModUnload()
    {
        BossHealthBarManager.MinibossHPBarList.Remove(NPCID.LunarTowerVortex);
        BossHealthBarManager.MinibossHPBarList.Remove(NPCID.LunarTowerStardust);
        BossHealthBarManager.MinibossHPBarList.Remove(NPCID.LunarTowerNebula);
        BossHealthBarManager.MinibossHPBarList.Remove(NPCID.LunarTowerSolar);
        BossHealthBarManager.MinibossHPBarList.Remove(NPCID.PirateShip);
        BossHealthBarManager.OneToMany[NPCID.SkeletronHead] = [NPCID.SkeletronHead, NPCID.SkeletronHand];
        BossHealthBarManager.OneToMany[NPCID.SkeletronPrime] = [NPCID.SkeletronPrime, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeCannon, NPCID.PrimeLaser];
        BossHealthBarManager.OneToMany[NPCID.Golem] = [NPCID.Golem, NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead, NPCID.GolemHeadFree];
        BossHealthBarManager.OneToMany[NPCID.BrainofCthulhu] = [NPCID.BrainofCthulhu, NPCID.Creeper];
        BossHealthBarManager.OneToMany[NPCID.MartianSaucerCore] = [NPCID.MartianSaucerCore, NPCID.MartianSaucerTurret, NPCID.MartianSaucerCannon];
        BossHealthBarManager.OneToMany[NPCID.PirateShip] = [NPCID.PirateShip, NPCID.PirateShipCannon];
        BossHealthBarManager.OneToMany[ModContent.NPCType<CeaselessVoid>()] =
        [
            ModContent.NPCType<CeaselessVoid>(),
            ModContent.NPCType<DarkEnergy>()
        ];
        BossHealthBarManager.OneToMany[ModContent.NPCType<RavagerBody>()] =
        [
            ModContent.NPCType<RavagerBody>(),
            ModContent.NPCType<RavagerClawRight>(),
            ModContent.NPCType<RavagerClawLeft>(),
            ModContent.NPCType<RavagerLegRight>(),
            ModContent.NPCType<RavagerLegLeft>(),
            ModContent.NPCType<RavagerHead>()
        ];
        BossHealthBarManager.OneToMany[ModContent.NPCType<EbonianPaladin>()] = BossHealthBarManager.OneToMany[ModContent.NPCType<CrimulanPaladin>()] =
        [
            ModContent.NPCType<EbonianPaladin>(),
            ModContent.NPCType<SplitEbonianPaladin>(),
            ModContent.NPCType<CrimulanPaladin>(),
            ModContent.NPCType<SplitCrimulanPaladin>()
        ];
    }

    void ITOLoader.OnWorldLoad() => _trackingBars.Clear();

    void ITOLoader.OnWorldUnload() => _trackingBars.Clear();
}
