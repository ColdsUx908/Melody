using CalamityAnomalies.Assets.Textures;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.World;
using Transoceanic;

namespace CalamityAnomalies.Anomaly;

public sealed class AnomalyMode : DifficultyMode, ILocalizationPrefix
{
    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "Anomaly.AnomalyMode";

    internal static AnomalyMode Instance;

    public override bool Enabled
    {
        get => CASharedData.Anomaly;
        set
        {
            if (CASharedData.Anomaly ^ value)
            {
                CASharedData.Anomaly = value;
                CANetSync.SyncAnomalyMode();
            }
        }
    }

    public override Asset<Texture2D> Texture => _texture ??= CATextures._anomalyModeIndicator;
    public override Asset<Texture2D> OutlineTexture => _outlineTexture ??= CATextures._anomalyModeIndicator_Border;
    public override Asset<Texture2D> TextureDisabled => _textureDisabled ??= CATextures._anomalyModeIndicator_Off;

    public override SoundStyle ActivationSound => _activationSound ??= SupremeCalamitas.BulletHellEndSound;

    public override int BackBoneGameModeID => GameModeID.Master;

    public override bool IsBasedOn(DifficultyMode mode)
    {
        if (mode is MasterDifficulty or DeathDifficulty or MaliceDifficulty)
            return true;
        return false;
    }

    public override float DifficultyScale => 10000f;

    public override LocalizedText Name => this.GetText("Anomaly");

    public override Color ChatTextColor => CASharedData.MainColor;

    public override LocalizedText ShortDescription => this.GetText("AnomalyShortInfo");
    public override LocalizedText ExpandedDescription => this.GetText("AnomalyExpandedInfo");

    public override int[] FavoredDifficultyAtTier(int tier)
    {
        DifficultyMode[] tierList = DifficultyModeSystem.DifficultyTiers[tier];

        List<int> difficulties = [];

        for (int i = 0; i < tierList.Length; i++)
        {
            if (tierList[i] is MasterDifficulty or DeathDifficulty)
                difficulties.Add(i);
        }

        if (difficulties.Count <= 0)
            difficulties.Add(0);

        return [.. difficulties];
    }
}

public sealed class AnomalyModeHandler : ModSystem, IResourceLoader, ILocalizationPrefix
{
    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "Anomaly.AnomalyMode";

    public override void PreUpdateWorld()
    {
        if (CASharedData.Anomaly)
        {
            if (!TOSharedData.MasterMode)
            {
                DisableAnomaly();
                return;
            }

            CalamityWorld.revenge = true;
            CalamityWorld.death = true;

            switch (TOSharedData.LegendaryMode, !Main.zenithWorld)
            {
                case (false, true) when CASharedData.AnomalyUltramundane: //不是传奇难度，不在GFB世界
                    NotLegendaryInfo();
                    DisableUltra();
                    break;
                case (true, false) when CASharedData.AnomalyUltramundane: //是传奇难度，在GFB世界
                    ZenithInfo();
                    DisableUltra();
                    break;
                case (false, false) when CASharedData.AnomalyUltramundane: //不是传奇难度，且在GFB世界
                    NotLegendaryInfo();
                    ZenithInfo();
                    DisableUltra();
                    break;
                case (true, true) when !CASharedData.AnomalyUltramundane: //是传奇难度，且不在GFB世界，应开启异象超凡
                    EnableUltra();
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (CASharedData.AnomalyUltramundane)
                DisableUltra();
        }

        void DisableAnomaly()
        {
            if (TOSharedData.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(this, "AnomalyInvalid", Color.Red);
            if (CASharedData.AnomalyUltramundane)
                DisableUltra();
            CASharedData.Anomaly = false;
        }

        void DisableUltra()
        {
            if (TOSharedData.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(this, "AnomalyUltramundaneDeactivate", Color.Red);
            CASharedData.AnomalyUltramundane = false;
        }

        void EnableUltra()
        {
            if (TOSharedData.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(this, "AnomalyUltramundaneActivate", Color.Red);
            CASharedData.AnomalyUltramundane = true;
        }

        void ZenithInfo()
        {
            if (TOSharedData.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(this, "AnomalyUltraInvalidZenith", CASharedData.MainColor);
            //SoundEngine.PlaySound();
        }

        void NotLegendaryInfo()
        {
            if (TOSharedData.GeneralClient)
                TOLocalizationUtils.ChatLocalizedText(this, "AnomalyUltraInvalidNotLegendary", Color.Red);
        }
    }

    void IResourceLoader.PostSetupContent()
    {
        DifficultyModeSystem.Difficulties.Add(AnomalyMode.Instance = new());
        DifficultyModeSystem.CalculateDifficultyData();
    }

    void IResourceLoader.OnModUnload()
    {
        if (DifficultyModeSystem.Difficulties.Remove(AnomalyMode.Instance))
            DifficultyModeSystem.CalculateDifficultyData();
        AnomalyMode.Instance = null;
    }
}

public sealed class AnomalyModePlayerSync : CAPlayerBehavior
{
    public override decimal Priority => 100m;

    public override void OnEnterWorld() => CANetSync.SyncAnomalyModeFromServer();
}