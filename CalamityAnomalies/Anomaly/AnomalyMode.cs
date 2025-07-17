using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.World;

namespace CalamityAnomalies.Anomaly;

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
        Texture = CAMain.Instance.Assets.Request<Texture2D>("Textures/UI/ModeIndicator_Anomaly", AssetRequestMode.AsyncLoad);
        DifficultyScale = 2.49147E23f;
        Name = this.GetTextWithPrefix("Anomaly");
        ShortDescription = this.GetTextWithPrefix("AnomalyShortInfo");
        ActivationTextKey = LocalizationPrefix + "AnomalyActivate";
        DeactivationTextKey = LocalizationPrefix + "AnomalyDeactivate";
        ActivationSound = SupremeCalamitas.BulletHellEndSound;
        ChatTextColor = CAMain.MainColor;
    }
}

public sealed class AnomalyModeHandler : ModSystem, IResourceLoader, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.ModLocalizationPrefix + "Difficulty.AnomalyMode.";

    public override void PreUpdateWorld()
    {
        if (CAWorld.Anomaly)
        {
            if (!TOWorld.MasterMode)
            {
                DisableAnomaly();
                return;
            }

            CalamityWorld.revenge = true;
            CalamityWorld.death = true;

            switch (TOWorld.LegendaryMode, !Main.zenithWorld)
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

    private void DisableUltra()
    {
        if (TOWorld.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltramundaneDeactivate", Color.Red);
        CAWorld.AnomalyUltramundane = false;
    }

    private void EnableUltra()
    {
        if (TOWorld.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltramundaneActivate", Color.Red);
        CAWorld.AnomalyUltramundane = true;
    }

    private void ZenithInfo()
    {
        if (TOWorld.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltraInvalidZenith", CAMain.MainColor);
        //SoundEngine.PlaySound();
    }

    private void NotLegendaryInfo()
    {
        if (TOWorld.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltraInvalidNotLegendary", Color.Red);
    }

    private void DisableAnomaly()
    {
        if (TOWorld.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyInvalid", Color.Red);
        CAWorld.Anomaly = false;
    }

    void IResourceLoader.PostSetupContent()
    {
        DifficultyModeSystem.Difficulties.Add(AnomalyMode.Instance = new());
        DifficultyModeSystem.CalculateDifficultyData();
    }

    void IResourceLoader.OnModUnload()
    {
        DifficultyModeSystem.Difficulties.Remove(AnomalyMode.Instance);
        DifficultyModeSystem.CalculateDifficultyData();
        AnomalyMode.Instance = null;
    }
}


