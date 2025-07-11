using CalamityMod.Events;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.World;

namespace CalamityAnomalies.Core;

public sealed class AnomalyMode : DifficultyMode, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.ModLocalizationPrefix + "Difficulty.AnomalyMode.";

    internal static AnomalyMode Instance { get; set; }

    public override bool Enabled
    {
        get => CAWorld.Anomaly;
        set => CAWorld.Anomaly = value;
    }

    public override Asset<Texture2D> Texture { get; }

    public override LocalizedText ExpandedDescription => Language.GetText(LocalizationPrefix + "AnomalyExpandedInfo");

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
        Name = this.GetTextWithPrefix("Anomaly");
        ShortDescription = this.GetTextWithPrefix("AnomalyShortInfo");
        ActivationTextKey = LocalizationPrefix + "AnomalyActivate";
        DeactivationTextKey = LocalizationPrefix + "AnomalyDeactivate";
        ActivationSound = SupremeCalamitas.BulletHellEndSound;
        ChatTextColor = CAMain.MainColor;
    }
}

/// <summary>
/// BossRush难度。
/// <br/>所有相关更改均用钩子实现。
/// </summary>
public sealed class BossRushMode : DifficultyMode, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.ModLocalizationPrefix + "Difficulty.BossRushMode.";

    public static readonly Color BossRushModeColor = new(0xF0, 0x80, 0x80);

    internal static BossRushMode Instance { get; set; }

    public override bool Enabled
    {
        get => CAWorld.BossRush;
        set => CAWorld.BossRush = value;
    }

    public override Asset<Texture2D> Texture { get; }

    public override LocalizedText ExpandedDescription => this.GetTextWithPrefix("BossRushExpandedInfo");

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

    public BossRushMode()
    {
        Texture = ModContent.Request<Texture2D>("CalamityAnomalies/Textures/UI/ModeIndicator_BossRush", AssetRequestMode.AsyncLoad);
        DifficultyScale = 2f;
        Name = this.GetTextWithPrefix("BossRush");
        ShortDescription = this.GetTextWithPrefix("BossRushShortInfo");
        ActivationTextKey = LocalizationPrefix + "BossRushActivate";
        DeactivationTextKey = LocalizationPrefix + "BossRushDeactivate";
        ActivationSound = SupremeCalamitas.SpawnSound;
        ChatTextColor = BossRushModeColor;
    }
}

public sealed class CADifficultyManagement : ModSystem, IResourceLoader, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.ModLocalizationPrefix + "Difficulty.AnomalyMode.";

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

        CAWorld.RealBossRushEventActive = BossRushEvent.BossRushActive;

        if (CAWorld.BossRush)
        {
            CalamityWorld.revenge = true;
            CalamityWorld.death = true;
        }
    }

    private void DisableUltra()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltramundaneDeactivate", Color.Red);
        CAWorld.AnomalyUltramundane = false;
    }

    private void EnableUltra()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltramundaneActivate", Color.Red);
        CAWorld.AnomalyUltramundane = true;
    }

    private void ZenithInfo()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltraInvalidZenith", CAMain.MainColor);
        //SoundEngine.PlaySound();
    }

    private void NotLegendaryInfo()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltraInvalidNotLegendary", Color.Red);
    }

    private void DisableAnomaly()
    {
        if (TOMain.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyInvalid", Color.Red);
        CAWorld.Anomaly = false;
    }

    void IResourceLoader.PostSetupContent()
    {
        DifficultyModeSystem.Difficulties.Add(AnomalyMode.Instance = new());
        BossRushMode.Instance = new();
        if (CAServerConfig.Instance.BossRushDifficulty)
            DifficultyModeSystem.Difficulties.Add(BossRushMode.Instance);
        DifficultyModeSystem.CalculateDifficultyData();
    }

    void IResourceLoader.OnModUnload()
    {
        DifficultyModeSystem.Difficulties.Remove(BossRushMode.Instance);
        DifficultyModeSystem.Difficulties.Remove(AnomalyMode.Instance);
        DifficultyModeSystem.CalculateDifficultyData();
        BossRushMode.Instance = null;
        AnomalyMode.Instance = null;
    }
}


