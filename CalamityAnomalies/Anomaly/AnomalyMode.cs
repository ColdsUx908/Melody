using CalamityAnomalies.Assets.Textures;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.World;

namespace CalamityAnomalies.Anomaly;

public sealed class AnomalyMode : DifficultyMode, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.ModLocalizationPrefix + "UI.AnomalyMode.";

    internal static AnomalyMode Instance;

    public override bool Enabled
    {
        get => CAWorld.Anomaly;
        set
        {
            bool temp = CAWorld.Anomaly;
            CAWorld.Anomaly = value;
            if (temp ^ value)
                CANetSync.SyncAnomalyMode();
        }
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
        Texture = CATextures._anomalyModeIndicator;
        DifficultyScale = 10000f;
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
    public string LocalizationPrefix => CAMain.ModLocalizationPrefix + "UI.AnomalyMode.";

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

        void DisableAnomaly()
        {
            if (TOWorld.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyInvalid", Color.Red);
            if (CAWorld.AnomalyUltramundane)
                DisableUltra();
            CAWorld.Anomaly = false;
        }

        void DisableUltra()
        {
            if (TOWorld.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltramundaneDeactivate", Color.Red);
            CAWorld.AnomalyUltramundane = false;
        }

        void EnableUltra()
        {
            if (TOWorld.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltramundaneActivate", Color.Red);
            CAWorld.AnomalyUltramundane = true;
        }

        void ZenithInfo()
        {
            if (TOWorld.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltraInvalidZenith", CAMain.MainColor);
            //SoundEngine.PlaySound();
        }

        void NotLegendaryInfo()
        {
            if (TOWorld.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "AnomalyUltraInvalidNotLegendary", Color.Red);
        }
    }

    void IResourceLoader.PostSetupContent()
    {
        if (CAServerConfig.Instance.Contents)
        {
            DifficultyModeSystem.Difficulties.Add(AnomalyMode.Instance = new());
            DifficultyModeSystem.CalculateDifficultyData();
        }
    }

    void IResourceLoader.OnModUnload()
    {
        if (DifficultyModeSystem.Difficulties.Remove(AnomalyMode.Instance))
            DifficultyModeSystem.CalculateDifficultyData();
        AnomalyMode.Instance = null;
    }
}

public sealed class AnomalyModePlayerSync : CAPlayerBehavior2
{
    public override decimal Priority => 100m;

    public override void OnEnterWorld() => CANetSync.SyncAnomalyModeFromServer();
}