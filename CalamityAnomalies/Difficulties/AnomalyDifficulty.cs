﻿using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.World;

namespace CalamityAnomalies.Difficulties;

public class AnomalyMode : DifficultyMode, ITOLoader
{
    private const string localizationPrefix = CAMain.ModLocalizationPrefix + "Difficulty.AnomalyMode.";

    internal static AnomalyMode Instance { get; set; }

    public override bool Enabled
    {
        get => CAWorld.Anomaly;
        set => CAWorld.Anomaly = value;
    }

    public override Asset<Texture2D> Texture { get; }

    public override LocalizedText ExpandedDescription => Language.GetText(localizationPrefix + "AnomalyExpandedInfo");

    public override int FavoredDifficultyAtTier(int tier)
    {
        DifficultyMode[] tierList = DifficultyModeSystem.DifficultyTiers[tier];
        for (int i = 0; i < tierList.Length; i++)
        {
            if (tierList[i].Name == CalamityUtils.GetText("UI.Death"))
                return i;
        }
        return 0;
    }

    public AnomalyMode()
    {
        Texture = ModContent.Request<Texture2D>("CalamityAnomalies/Textures/UI/ModeIndicator_Anomaly", AssetRequestMode.AsyncLoad);
        DifficultyScale = 2.49147E23f;
        Name = Language.GetText(localizationPrefix + "Anomaly");
        ShortDescription = Language.GetText(localizationPrefix + "AnomalyShortInfo");
        ActivationTextKey = localizationPrefix + "AnomalyActivate";
        DeactivationTextKey = localizationPrefix + "AnomalyDeactivate";
        ActivationSound = SupremeCalamitas.BulletHellEndSound;
        ChatTextColor = Color.HotPink;
    }

    void ITOLoader.PostSetupContent()
    {
        DifficultyModeSystem.Difficulties.Add(Instance = this);
        DifficultyModeSystem.CalculateDifficultyData();
    }

    void ITOLoader.OnModUnload()
    {
        DifficultyModeSystem.Difficulties.Remove(Instance);
        DifficultyModeSystem.CalculateDifficultyData();
        Instance = null;
    }
}

public sealed class AnomalyManagement : ModSystem
{
    private const string localizationPrefix = CAMain.ModLocalizationPrefix + "Difficulty.AnomalyMode.";

    public override void PreUpdateWorld()
    {
        if (CAWorld.Anomaly)
        {
            if (!TOMain.MasterMode)
            {
                DisableAnomaly();
                return;
            }

            CalamityWorld.revenge = true;
            CalamityWorld.death = true;
            CAWorld.BossRush = false;

            switch (TOMain.LegendaryMode, !Main.zenithWorld)
            {
                case (false, true) when CAWorld.AnomalyUltramundane: //不是传奇难度，不在GFB世界
                    NotLegendaryInfo();
                    DisableUltra();
                    break;
                case (true, false) when CAWorld.AnomalyUltramundane: //是传奇难度，在GFB世界
                    ZenithInfo();
                    DisableUltra();
                    break;
                case (false, false) when CAWorld.AnomalyUltramundane: //不是传奇难度，且在GFB世界
                    NotLegendaryInfo();
                    ZenithInfo();
                    DisableUltra();
                    break;
                case (true, true) when !CAWorld.AnomalyUltramundane: //是传奇难度，且不在GFB世界，应开启异象超凡
                    EnableUltra();
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (CAWorld.AnomalyUltramundane)
                DisableUltra();
        }
    }

    private static void DisableUltra()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(localizationPrefix + "AnomalyUltramundaneDeactivate", Color.Red);
        CAWorld.AnomalyUltramundane = false;
    }

    private static void EnableUltra()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(localizationPrefix + "AnomalyUltramundaneActivate", Color.Red);
        CAWorld.AnomalyUltramundane = true;
    }

    private static void ZenithInfo()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(localizationPrefix + "AnomalyUltraInvalidZenith", Color.HotPink);
        //SoundEngine.PlaySound();
    }

    private static void NotLegendaryInfo()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(localizationPrefix + "AnomalyUltraInvalidNotLegendary", Color.Red);
    }

    private static void DisableAnomaly()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(localizationPrefix + "AnomalyInvalid", Color.Red);
        CAWorld.Anomaly = false;
    }
}
