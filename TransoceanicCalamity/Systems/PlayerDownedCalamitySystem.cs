using System.Collections.Generic;
using CalamityMod;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic.Core;
using Transoceanic.Systems;
using TransoceanicCalamity.Core;

namespace TransoceanicCalamity.Systems;

public class PlayerDownedBossCalamity : PlayerDownedBoss
{
    public bool DesertScourge { get; set; } = false;
    public bool Crabulon { get; set; } = false;
    public bool EvilBoss2 { get; set; } = false;
    public bool HiveMind { get; set; } = false;
    public bool Perforator { get; set; } = false;
    public bool SlimeGod { get; set; } = false;
    public bool Cryogen { get; set; } = false;
    public bool AquaticScourge { get; set; } = false;
    public bool BrimstoneElemental { get; set; } = false;
    public bool CalamitasClone { get; set; } = false;
    public bool Leviathan { get; set; } = false;
    public bool AstrumAureus { get; set; } = false;
    public bool Plaguebringer { get; set; } = false;
    public bool Ravager { get; set; } = false;
    public bool AstrumDeus { get; set; } = false;
    public bool Guardians { get; set; } = false;
    public bool Dragonfolly { get; set; } = false;
    public bool Providence { get; set; } = false;
    public bool CeaselessVoid { get; set; } = false;
    public bool StormWeaver { get; set; } = false;
    public bool Signus { get; set; } = false;
    public bool Polterghast { get; set; } = false;
    public bool BommerDuke { get; set; } = false;
    public bool DoG { get; set; } = false;
    public bool Yharon { get; set; } = false;
    public bool Ares { get; set; } = false;
    public bool Thanatos { get; set; } = false;
    public bool ArtemisAndApollo { get; set; } = false;
    public bool ExoMechs
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                LastBoss = true;
                if (Calamitas)
                    Focus = true;
            }
        }
    } = false;
    public bool Calamitas
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                LastBoss = true;
                if (ExoMechs)
                    Focus = true;
            }
        }
    } = false;
    public bool LastBoss { get; set; } = false;
    public bool Focus { get; set; } = false;
    public bool PrimordialWyrm { get; set; } = false;

    public bool GSS { get; set; } = false;
    public bool CLAM { get; set; } = false;
    public bool CLAMHardMode { get; set; } = false;
    public bool CragmawMire { get; set; } = false;
    public bool Mauler { get; set; } = false;
    public bool NuclearTerror { get; set; } = false;

    public bool EoCAcidRain { get; set; } = false;
    public bool AquaticScourgeAcidRain { get; set; } = false;

    public bool BossRush { get; set; } = false;

    /// <summary>
    /// 使玩家跟随世界Boss击败状态。
    /// </summary>
    public override void WorldPolluted()
    {
        base.WorldPolluted();

        if (DownedBossSystem.downedDesertScourge)
            DesertScourge = true;
        if (DownedBossSystem.downedCrabulon)
            Crabulon = true;
        if (DownedBossSystem.downedHiveMind)
            HiveMind = true;
        if (DownedBossSystem.downedPerforator)
            Perforator = true;
        EvilBoss2 = HiveMind || Perforator;
        if (DownedBossSystem.downedSlimeGod)
            SlimeGod = true;
        if (DownedBossSystem.downedCryogen)
            Cryogen = true;
        if (DownedBossSystem.downedAquaticScourge)
            AquaticScourge = true;
        if (DownedBossSystem.downedBrimstoneElemental)
            BrimstoneElemental = true;
        if (DownedBossSystem.downedCalamitasClone)
            CalamitasClone = true;
        if (DownedBossSystem.downedLeviathan)
            Leviathan = true;
        if (DownedBossSystem.downedAstrumAureus)
            AstrumAureus = true;
        if (DownedBossSystem.downedPlaguebringer)
            Plaguebringer = true;
        if (DownedBossSystem.downedRavager)
            Ravager = true;
        if (DownedBossSystem.downedAstrumDeus)
            AstrumDeus = true;
        if (DownedBossSystem.downedGuardians)
            Guardians = true;
        if (DownedBossSystem.downedDragonfolly)
            Dragonfolly = true;
        if (DownedBossSystem.downedProvidence)
            Providence = true;
        if (DownedBossSystem.downedCeaselessVoid)
            CeaselessVoid = true;
        if (DownedBossSystem.downedStormWeaver)
            StormWeaver = true;
        if (DownedBossSystem.downedSignus)
            Signus = true;
        if (DownedBossSystem.downedPolterghast)
            Polterghast = true;
        if (DownedBossSystem.downedBoomerDuke)
            BommerDuke = true;
        if (DownedBossSystem.downedDoG)
            DoG = true;
        if (DownedBossSystem.downedYharon)
            Yharon = true;
        if (DownedBossSystem.downedAres)
            Ares = true;
        if (DownedBossSystem.downedThanatos)
            Thanatos = true;
        if (DownedBossSystem.downedArtemisAndApollo)
            ArtemisAndApollo = true;
        if (DownedBossSystem.downedExoMechs)
            ExoMechs = true;
        if (DownedBossSystem.downedCalamitas)
            Calamitas = true;
        if (DownedBossSystem.downedPrimordialWyrm)
            PrimordialWyrm = true;

        if (DownedBossSystem.downedGSS)
            GSS = true;
        if (DownedBossSystem.downedCLAM)
            CLAM = true;
        if (DownedBossSystem.downedCLAMHardMode)
            CLAMHardMode = true;
        if (DownedBossSystem.downedCragmawMire)
            CragmawMire = true;
        if (DownedBossSystem.downedMauler)
            Mauler = true;
        if (DownedBossSystem.downedNuclearTerror)
            NuclearTerror = true;

        if (DownedBossSystem.downedEoCAcidRain)
            EoCAcidRain = true;
        if (DownedBossSystem.downedAquaticScourgeAcidRain)
            AquaticScourgeAcidRain = true;
        if (DownedBossSystem.downedBossRush)
            BossRush = true;
    }

    public override void SaveDataTo(List<string> downed)
    {
        base.SaveDataTo(downed);

        if (DesertScourge)
            downed.Add("DesertScourge");
        if (Crabulon)
            downed.Add("Crabulon");
        if (EvilBoss2)
            downed.Add("EvilBoss2");
        if (HiveMind)
            downed.Add("HiveMind");
        if (Perforator)
            downed.Add("Perforator");
        if (SlimeGod)
            downed.Add("SlimeGod");
        if (Cryogen)
            downed.Add("Cryogen");
        if (AquaticScourge)
            downed.Add("AquaticScourge");
        if (BrimstoneElemental)
            downed.Add("BrimstoneElemental");
        if (CalamitasClone)
            downed.Add("CalamitasClone");
        if (Leviathan)
            downed.Add("Leviathan");
        if (AstrumAureus)
            downed.Add("AstrumAureus");
        if (Plaguebringer)
            downed.Add("Plaguebringer");
        if (Ravager)
            downed.Add("Ravager");
        if (AstrumDeus)
            downed.Add("AstrumDeus");
        if (Guardians)
            downed.Add("Guardians");
        if (Dragonfolly)
            downed.Add("Dragonfolly");
        if (Providence)
            downed.Add("Providence");
        if (CeaselessVoid)
            downed.Add("CeaselessVoid");
        if (StormWeaver)
            downed.Add("StormWeaver");
        if (Signus)
            downed.Add("Signus");
        if (Polterghast)
            downed.Add("Polterghast");
        if (BommerDuke)
            downed.Add("BommerDuke");
        if (DoG)
            downed.Add("DoG");
        if (Yharon)
            downed.Add("Yharon");
        if (Ares)
            downed.Add("Ares");
        if (Thanatos)
            downed.Add("Thanatos");
        if (ArtemisAndApollo)
            downed.Add("ArtemisAndApollo");
        if (ExoMechs)
            downed.Add("ExoMechs");
        if (Calamitas)
            downed.Add("Calamitas");
        if (PrimordialWyrm)
            downed.Add("PrimordialWyrm");

        if (GSS)
            downed.Add("GSS");
        if (CLAM)
            downed.Add("CLAM");
        if (CLAMHardMode)
            downed.Add("CLAMHardMode");
        if (CragmawMire)
            downed.Add("CragmawMire");
        if (Mauler)
            downed.Add("Mauler");
        if (NuclearTerror)
            downed.Add("NuclearTerror");

        if (EoCAcidRain)
            downed.Add("EoCAcidRain");
        if (AquaticScourgeAcidRain)
            downed.Add("AquaticScourgeAcidRain");
        if (BossRush)
            downed.Add("BossRush");
    }

    public override void LoadDataFrom(IList<string> downedLoaded)
    {
        base.LoadDataFrom(downedLoaded);

        if (downedLoaded.Contains("DesertScourge"))
            DesertScourge = true;
        if (downedLoaded.Contains("Crabulon"))
            Crabulon = true;
        if (downedLoaded.Contains("HiveMind"))
            HiveMind = true;
        if (downedLoaded.Contains("Perforator"))
            Perforator = true;
        if (downedLoaded.Contains("EvilBoss2"))
            EvilBoss2 = true;
        if (downedLoaded.Contains("SlimeGod"))
            SlimeGod = true;
        if (downedLoaded.Contains("Cryogen"))
            Cryogen = true;
        if (downedLoaded.Contains("AquaticScourge"))
            AquaticScourge = true;
        if (downedLoaded.Contains("BrimstoneElemental"))
            BrimstoneElemental = true;
        if (downedLoaded.Contains("CalamitasClone"))
            CalamitasClone = true;
        if (downedLoaded.Contains("Leviathan"))
            Leviathan = true;
        if (downedLoaded.Contains("AstrumAureus"))
            AstrumAureus = true;
        if (downedLoaded.Contains("Plaguebringer"))
            Plaguebringer = true;
        if (downedLoaded.Contains("Ravager"))
            Ravager = true;
        if (downedLoaded.Contains("AstrumDeus"))
            AstrumDeus = true;
        if (downedLoaded.Contains("Guardians"))
            Guardians = true;
        if (downedLoaded.Contains("Dragonfolly"))
            Dragonfolly = true;
        if (downedLoaded.Contains("Providence"))
            Providence = true;
        if (downedLoaded.Contains("CeaselessVoid"))
            CeaselessVoid = true;
        if (downedLoaded.Contains("StormWeaver"))
            StormWeaver = true;
        if (downedLoaded.Contains("Signus"))
            Signus = true;
        if (downedLoaded.Contains("Polterghast"))
            Polterghast = true;
        if (downedLoaded.Contains("BommerDuke"))
            BommerDuke = true;
        if (downedLoaded.Contains("DoG"))
            DoG = true;
        if (downedLoaded.Contains("Yharon"))
            Yharon = true;
        if (downedLoaded.Contains("Ares"))
            Ares = true;
        if (downedLoaded.Contains("Thanatos"))
            Thanatos = true;
        if (downedLoaded.Contains("ArtemisAndApollo"))
            ArtemisAndApollo = true;
        if (downedLoaded.Contains("ExoMechs"))
            ExoMechs = true;
        if (downedLoaded.Contains("Calamitas"))
            Calamitas = true;
        if (downedLoaded.Contains("PrimordialWyrm"))
            PrimordialWyrm = true;

        if (downedLoaded.Contains("GSS"))
            GSS = true;
        if (downedLoaded.Contains("CLAM"))
            CLAM = true;
        if (downedLoaded.Contains("CLAMHardMode"))
            CLAMHardMode = true;
        if (downedLoaded.Contains("CragmawMire"))
            CragmawMire = true;
        if (downedLoaded.Contains("Mauler"))
            Mauler = true;
        if (downedLoaded.Contains("NuclearTerror"))
            NuclearTerror = true;

        if (downedLoaded.Contains("EoCAcidRain"))
            EoCAcidRain = true;
        if (downedLoaded.Contains("AquaticScourgeAcidRain"))
            AquaticScourgeAcidRain = true;
        if (downedLoaded.Contains("BossRush"))
            BossRush = true;
    }

    /// <summary>
    /// 击杀Boss时的处理。
    /// </summary>
    /// <param name="npc"></param>
    public void BossesOnKill(NPC npc)
    {
        switch (npc.type)
        {
            // 原版Boss
            case NPCID.KingSlime:
                KingSlime = true;
                break;
            case NPCID.EyeofCthulhu:
                EyeOfCthulhu = true;
                break;
            case int _ when TONPCUtils.IsDefeatingEoW(npc):
                EaterOfWorld = true;
                break;
            case NPCID.BrainofCthulhu:
                BrainOfCthulhu = true;
                break;
            case NPCID.QueenBee:
                QueenBee = true;
                break;
            case NPCID.SkeletronHead:
                Skeletron = true;
                break;
            case NPCID.Deerclops:
                Deerclops = true;
                break;
            case NPCID.WallofFlesh:
                WallOfFlesh = true;
                break;
            case NPCID.TheDestroyer:
                Destroyer = true;
                break;
            case int _ when TONPCUtils.IsDefeatingTwins(npc):
                Twins = true;
                break;
            case NPCID.SkeletronPrime:
                SkeletronPrime = true;
                break;
            case NPCID.Plantera:
                Plantera = true;
                break;
            case NPCID.Golem:
                Golem = true;
                break;
            case NPCID.CultistBoss:
                LunaticCultist = true;
                break;
            case NPCID.MoonLordCore:
                MoonLord = true;
                break;

            // 原版事件Boss
            case NPCID.MourningWood:
                MourningWood = true;
                break;
            case NPCID.Pumpking:
                Pumpking = true;
                break;
            case NPCID.Everscream:
                Everscream = true;
                break;
            case NPCID.SantaNK1:
                SantaNK1 = true;
                break;
            case NPCID.IceQueen:
                IceQueen = true;
                break;
            case NPCID.DD2Betsy:
                Betsy = true;
                break;
            case NPCID.BloodNautilus:
                Dreadnautilus = true;
                break;
            case NPCID.LunarTowerSolar:
                SolarTower = true;
                break;
            case NPCID.LunarTowerVortex:
                VortexTower = true;
                break;
            case NPCID.LunarTowerNebula:
                NebulaTower = true;
                break;
            case NPCID.LunarTowerStardust:
                StardustTower = true;
                break;
        }

        // 灾厄Boss
        if (npc.type == ModContent.NPCType<DesertScourgeHead>())
            DesertScourge = true;
        else if (npc.type == ModContent.NPCType<Crabulon>())
            Crabulon = true;
        else if (npc.type == ModContent.NPCType<HiveMind>())
            HiveMind = true;
        else if (npc.type == ModContent.NPCType<PerforatorHive>())
            Perforator = true;
        else if (npc.type == ModContent.NPCType<SlimeGodCore>())
            SlimeGod = true;
        else if (npc.type == ModContent.NPCType<Cryogen>())
            Cryogen = true;
        else if (npc.type == ModContent.NPCType<AquaticScourgeHead>())
            AquaticScourge = true;
        else if (npc.type == ModContent.NPCType<BrimstoneElemental>())
            BrimstoneElemental = true;
        else if (npc.type == ModContent.NPCType<CalamitasClone>())
            CalamitasClone = true;
        else if (TOCNPCUtils.IsDefeatingLeviathan(npc))
            Leviathan = true;
        else if (npc.type == ModContent.NPCType<AstrumAureus>())
            AstrumAureus = true;
        else if (npc.type == ModContent.NPCType<PlaguebringerGoliath>())
            Plaguebringer = true;
        else if (npc.type == ModContent.NPCType<RavagerBody>())
            Ravager = true;
        else if (npc.type == ModContent.NPCType<AstrumDeusHead>())
            AstrumDeus = true;
        else if (TOCNPCUtils.IsDefeatingProfanedGuardians(npc))
            Guardians = true;
        else if (npc.type == ModContent.NPCType<Bumblefuck>())
            Dragonfolly = true;
        else if (npc.type == ModContent.NPCType<Providence>())
            Providence = true;
        else if (npc.type == ModContent.NPCType<CeaselessVoid>())
            CeaselessVoid = true;
        else if (npc.type == ModContent.NPCType<StormWeaverHead>())
            StormWeaver = true;
        else if (npc.type == ModContent.NPCType<Signus>())
            Signus = true;
        else if (npc.type == ModContent.NPCType<Polterghast>())
            Polterghast = true;
        else if (npc.type == ModContent.NPCType<OldDuke>())
            BommerDuke = true;
        else if (npc.type == ModContent.NPCType<DevourerofGodsHead>())
            DoG = true;
        else if (npc.type == ModContent.NPCType<Yharon>())
            Yharon = true;
        else if (TOCNPCUtils.IsDefeatingExoMechs(npc))
            ExoMechs = true;
        else if (npc.type == ModContent.NPCType<SupremeCalamitas>())
            Calamitas = true;
        else if (npc.type == ModContent.NPCType<EidolonWyrmHead>())
            PrimordialWyrm = true;
    }
}