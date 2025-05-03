using System.Collections.Generic;
using CalamityAnomalies.Utilities;
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
using Terraria.ModLoader.IO;
using Transoceanic.Core.GameData;
using Transoceanic.Systems;

namespace CalamityAnomalies.GlobalInstances.CAPlayer;

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
    public bool Goliath { get; set; } = false;
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
            Goliath = true;
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

    public override void SaveData(TagCompound tag, string key)
    {
        List<string> downed = [];
        SaveDataToList(downed);
        tag[key] = downed;
    }

    public override void SaveDataToList(List<string> downed)
    {
        base.SaveDataToList(downed);

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
        if (Goliath)
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

    public override void LoadData(TagCompound tag, string key) => LoadDataFromIList(tag.GetList<string>(key));

    public override void LoadDataFromIList(IList<string> downedLoaded)
    {
        base.LoadDataFromIList(downedLoaded);

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
            Goliath = true;
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
        switch (npc.ModNPC)
        {
            case null:
                switch (npc.type)
                {
                    // 原版Boss
                    case NPCID.KingSlime:
                        KingSlime = true;
                        break;
                    case NPCID.EyeofCthulhu:
                        EyeOfCthulhu = true;
                        break;
                    case NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail when npc.boss:
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
                break;
            //灾厄Boss
            case DesertScourgeHead:
                DesertScourge = true;
                break;
            case Crabulon _:
                Crabulon = true;
                break;
            case HiveMind _:
                HiveMind = true;
                break;
            case PerforatorHive:
                Perforator = true;
                break;
            case SlimeGodCore:
                SlimeGod = true;
                break;
            case Cryogen _:
                Cryogen = true;
                break;
            case AquaticScourgeHead:
                AquaticScourge = true;
                break;
            case BrimstoneElemental _:
                BrimstoneElemental = true;
                break;
            case CalamitasClone _:
                CalamitasClone = true;
                break;
            case var _ when CAUtils.IsDefeatingLeviathan(npc):
                Leviathan = true;
                break;
            case AstrumAureus _:
                AstrumAureus = true;
                break;
            case PlaguebringerGoliath:
                Goliath = true;
                break;
            case RavagerBody:
                Ravager = true;
                break;
            case AstrumDeusHead:
                AstrumDeus = true;
                break;
            case var _ when CAUtils.IsDefeatingProfanedGuardians(npc):
                Guardians = true;
                break;
            case Bumblefuck:
                Dragonfolly = true;
                break;
            case Providence _:
                Providence = true;
                break;
            case CeaselessVoid _:
                CeaselessVoid = true;
                break;
            case StormWeaverHead:
                StormWeaver = true;
                break;
            case Signus _:
                Signus = true;
                break;
            case Polterghast _:
                Polterghast = true;
                break;
            case OldDuke:
                BommerDuke = true;
                break;
            case DevourerofGodsHead:
                DoG = true;
                break;
            case Yharon _:
                Yharon = true;
                break;
            case var _ when CAUtils.IsDefeatingExoMechs(npc):
                ExoMechs = true;
                break;
            case SupremeCalamitas:
                Calamitas = true;
                break;
            case EidolonWyrmHead:
                PrimordialWyrm = true;
                break;
                //灾厄迷你Boss未完成
        }
    }
}