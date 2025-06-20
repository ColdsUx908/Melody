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
using Terraria.GameInput;

namespace CalamityAnomalies.GlobalInstances;

public class CAPlayer : ModPlayer
{
    public class DownedBoss : PlayerDownedBoss
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

    public DownedBoss DownedBossCalamity { get; } = new();
    public DownedBoss DownedBossAnomaly { get; } = new();

    public bool AntiEPBPlayer { get; set; } = false;

    public override void Initialize()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.Initialize();
    }

    public override void ResetEffects()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ResetEffects();
    }

    public override void ResetInfoAccessories()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ResetInfoAccessories();
    }

    public override void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.RefreshInfoAccessoriesFromTeamPlayers(otherPlayer);
    }

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        mana = StatModifier.Default;

        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            behavior.ModifyMaxStats(out StatModifier newHealth, out StatModifier newMana);
            health = health.CombineWith(newHealth);
            mana = mana.CombineWith(newMana);
        }
    }

    public override void UpdateDead()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.UpdateDead();
    }

    public override void PreSaveCustomData()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PreSaveCustomData();
    }

    public override void SaveData(TagCompound tag)
    {
        DownedBossCalamity.SaveData(tag, "PlayerDownedBossCalamity");
        DownedBossAnomaly.SaveData(tag, "PlayerDownedBossAnomaly");

        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        DownedBossCalamity.LoadData(tag, "PlayerDownedBossCalamity");
        DownedBossAnomaly.LoadData(tag, "PlayerDownedBossAnomaly");

        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.LoadData(tag);
    }

    public override void PreSavePlayer()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PreSavePlayer();
    }

    public override void PostSavePlayer()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostSavePlayer();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.CopyClientState(targetCopy);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.SyncPlayer(toWho, fromWho, newPlayer);
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.SendClientChanges(clientPlayer);
    }

    public override void UpdateBadLifeRegen()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.UpdateBadLifeRegen();
    }

    public override void UpdateLifeRegen()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.UpdateLifeRegen();
    }

    public override void NaturalLifeRegen(ref float regen)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.NaturalLifeRegen(ref regen);
    }

    public override void UpdateAutopause()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.UpdateAutopause();
    }

    public override void PreUpdate()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PreUpdate();
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ProcessTriggers(triggersSet);
    }

    public override void ArmorSetBonusActivated()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ArmorSetBonusActivated();
    }

    public override void ArmorSetBonusHeld(int holdTime)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ArmorSetBonusHeld(holdTime);
    }

    public override void SetControls()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.SetControls();
    }

    public override void PreUpdateBuffs()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PreUpdateBuffs();
    }

    public override void PostUpdateBuffs()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostUpdateBuffs();
    }

    public override void UpdateEquips()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.UpdateEquips();
    }

    public override void PostUpdateEquips()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostUpdateEquips();
    }

    public override void UpdateVisibleAccessories()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.UpdateVisibleAccessories();
    }

    public override void UpdateVisibleVanityAccessories()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.UpdateVisibleVanityAccessories();
    }

    public override void UpdateDyes()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.UpdateDyes();
    }

    public override void PostUpdateMiscEffects()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostUpdateMiscEffects();
    }

    public override void PostUpdateRunSpeeds()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostUpdateRunSpeeds();
    }

    public override void PreUpdateMovement()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PreUpdateMovement();
    }

    public override void PostUpdate()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostUpdate();
    }

    public override void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyExtraJumpDurationMultiplier(jump, ref duration);
    }

    public override bool CanStartExtraJump(ExtraJump jump)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanStartExtraJump(jump);
        return result;
    }

    public override void OnExtraJumpStarted(ExtraJump jump, ref bool playSound)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnExtraJumpStarted(jump, ref playSound);
    }

    public override void OnExtraJumpEnded(ExtraJump jump)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnExtraJumpEnded(jump);
    }

    public override void OnExtraJumpRefreshed(ExtraJump jump)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnExtraJumpRefreshed(jump);
    }

    public override void ExtraJumpVisuals(ExtraJump jump)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ExtraJumpVisuals(jump);
    }

    public override bool CanShowExtraJumpVisuals(ExtraJump jump)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanShowExtraJumpVisuals(jump);
        return result;
    }

    public override void OnExtraJumpCleared(ExtraJump jump)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnExtraJumpCleared(jump);
    }

    public override void FrameEffects()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.FrameEffects();
    }

    public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
    {
        bool result = false;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result |= behavior.ImmuneTo(damageSource, cooldownCounter, dodgeable);
        return result;
    }

    public override bool FreeDodge(Player.HurtInfo info)
    {
        bool result = false;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result |= behavior.FreeDodge(info);
        return result;
    }

    public override bool ConsumableDodge(Player.HurtInfo info)
    {
        bool result = false;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result |= behavior.ConsumableDodge(info);
        return result;
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyHurt(ref modifiers);
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnHurt(info);
    }

    public override void PostHurt(Player.HurtInfo info)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostHurt(info);
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
        return result;
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.Kill(damage, hitDirection, pvp, damageSource);
    }

    public override bool PreModifyLuck(ref float luck)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.PreModifyLuck(ref luck);
        return result;
    }

    public override void ModifyLuck(ref float luck)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyLuck(ref luck);
    }

    public override bool PreItemCheck()
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.PreItemCheck();
        return result;
    }

    public override void PostItemCheck()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostItemCheck();
    }

    public override float UseTimeMultiplier(Item item)
    {
        float result = 1f;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result *= behavior.UseTimeMultiplier(item);
        return result;
    }

    public override float UseAnimationMultiplier(Item item)
    {
        float result = 1f;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result *= behavior.UseAnimationMultiplier(item);
        return result;
    }

    public override float UseSpeedMultiplier(Item item)
    {
        float result = 1f;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result *= behavior.UseSpeedMultiplier(item);
        return result;
    }

    public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.GetHealLife(item, quickHeal, ref healValue);
    }

    public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.GetHealMana(item, quickHeal, ref healValue);
    }

    public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyManaCost(item, ref reduce, ref mult);
    }

    public override void OnMissingMana(Item item, int neededMana)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnMissingMana(item, neededMana);
    }

    public override void OnConsumeMana(Item item, int manaConsumed)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnConsumeMana(item, manaConsumed);
    }

    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyWeaponDamage(item, ref damage);
    }

    public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyWeaponKnockback(item, ref knockback);
    }

    public override void ModifyWeaponCrit(Item item, ref float crit)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyWeaponCrit(item, ref crit);
    }

    public override bool CanConsumeAmmo(Item weapon, Item ammo)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanConsumeAmmo(weapon, ammo);
        return result;
    }

    public override void OnConsumeAmmo(Item weapon, Item ammo)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnConsumeAmmo(weapon, ammo);
    }

    public override bool CanShoot(Item item)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanShoot(item);
        return result;
    }

    public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);
    }

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.Shoot(item, source, position, velocity, type, damage, knockback);
        return result;
    }

    public override void MeleeEffects(Item item, Rectangle hitbox)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.MeleeEffects(item, hitbox);
    }

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
    }

    public override bool? CanCatchNPC(NPC target, Item item)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            bool? result = behavior.CanCatchNPC(target, item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void OnCatchNPC(NPC npc, Item item, bool failed)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnCatchNPC(npc, item, failed);
    }

    public override void ModifyItemScale(Item item, ref float scale)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyItemScale(item, ref scale);
    }

    public override void OnHitAnything(float x, float y, Entity victim)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnHitAnything(x, y, victim);
    }

    public override bool CanHitNPC(NPC target)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanHitNPC(target);
        return result;
    }

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            bool? result = behavior.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnHitNPC(target, hit, damageDone);
    }

    public override bool? CanHitNPCWithItem(Item item, NPC target)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            bool? result = behavior.CanHitNPCWithItem(item, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyHitNPCWithItem(item, target, ref modifiers);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnHitNPCWithItem(item, target, hit, damageDone);
    }

    public override bool? CanHitNPCWithProj(Projectile proj, NPC target)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            bool? result = behavior.CanHitNPCWithProj(proj, target);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyHitNPCWithProj(proj, target, ref modifiers);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnHitNPCWithProj(proj, target, hit, damageDone);
    }

    public override bool CanHitPvp(Item item, Player target)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanHitPvp(item, target);
        return result;
    }

    public override bool CanHitPvpWithProj(Projectile proj, Player target)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanHitPvpWithProj(proj, target);
        return result;
    }

    public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanBeHitByNPC(npc, ref cooldownSlot);
        return result;
    }

    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyHitByNPC(npc, ref modifiers);
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnHitByNPC(npc, hurtInfo);
    }

    public override bool CanBeHitByProjectile(Projectile proj)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanBeHitByProjectile(proj);
        return result;
    }

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyHitByProjectile(proj, ref modifiers);
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnHitByProjectile(proj, hurtInfo);
    }

    public override void ModifyFishingAttempt(ref FishingAttempt attempt)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyFishingAttempt(ref attempt);
    }

    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.CatchFish(attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);
    }

    public override void ModifyCaughtFish(Item fish)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyCaughtFish(fish);
    }

    public override bool? CanConsumeBait(Item bait)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            bool? result = behavior.CanConsumeBait(bait);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.GetFishingLevel(fishingRod, bait, ref fishingLevel);
    }

    public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.AnglerQuestReward(rareMultiplier, rewardItems);
    }

    public override void GetDyeTraderReward(List<int> rewardPool)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.GetDyeTraderReward(rewardPool);
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyDrawInfo(ref drawInfo);
    }

    public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyDrawLayerOrdering(positions);
    }

    public override void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.HideDrawLayers(drawInfo);
    }

    public override void ModifyScreenPosition()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyScreenPosition();
    }

    public override void ModifyZoom(ref float zoom)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyZoom(ref zoom);
    }

    public override void PlayerConnect()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PlayerConnect();
    }

    public override void PlayerDisconnect()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PlayerDisconnect();
    }

    public override void OnEnterWorld()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnEnterWorld();
    }

    public override void OnRespawn()
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.OnRespawn();
    }

    public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
    {
        bool result = false;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result |= behavior.ShiftClickSlot(inventory, context, slot);
        return result;
    }

    public override bool HoverSlot(Item[] inventory, int context, int slot)
    {
        bool result = false;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result |= behavior.HoverSlot(inventory, context, slot);
        return result;
    }

    public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostSellItem(vendor, shopInventory, item);
    }

    public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanSellItem(vendor, shopInventory, item);
        return result;
    }

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostBuyItem(vendor, shopInventory, item);
    }

    public override bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanBuyItem(vendor, shopInventory, item);
        return result;
    }

    public override bool CanUseItem(Item item)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanUseItem(item);
        return result;
    }

    public override bool? CanAutoReuseItem(Item item)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            bool? result = behavior.CanAutoReuseItem(item);
            if (result is not null)
                return result;
        }
        return null;
    }

    public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.ModifyNurseHeal(nurse, ref health, ref removeDebuffs, ref chatText);
        return result;
    }

    public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyNursePrice(nurse, health, removeDebuffs, ref price);
    }

    public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.PostNurseHeal(nurse, health, removeDebuffs, price);
    }

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        List<Item> allItems = [];
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            IEnumerable<Item> items = behavior.AddStartingItems(mediumCoreDeath);
            if (items is not null)
                allItems.AddRange(items);
        }
        return allItems;
    }

    public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
    {
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            behavior.ModifyStartingInventory(itemsByMod, mediumCoreDeath);
    }

    public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
    {
        itemConsumedCallback = null;
        List<Item> allItems = [];
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
        {
            IEnumerable<Item> items = behavior.AddMaterialsForCrafting(out ItemConsumedCallback callback);
            if (items is not null)
                allItems.AddRange(items);
            itemConsumedCallback += callback;
        }
        return allItems;
    }

    public override bool OnPickup(Item item)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.OnPickup(item);
        return result;
    }

    public override bool CanBeTeleportedTo(Vector2 teleportPosition, string context)
    {
        bool result = true;
        foreach (CAPlayerBehavior behavior in CABehaviorHelper.PlayerBehaviors)
            result &= behavior.CanBeTeleportedTo(teleportPosition, context);
        return result;
    }
}
