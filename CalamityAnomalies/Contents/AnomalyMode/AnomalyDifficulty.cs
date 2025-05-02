using CalamityMod;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;
using Transoceanic.Core.ExtraData.Maths;
using Transoceanic.Core.Localization;
using Transoceanic.Core.MathHelp;

namespace CalamityAnomalies.Contents.AnomalyMode;

public sealed class AnomalyMode : DifficultyMode, ITOLoader
{
    private const string prefix = CAMain.ModLocalizationPrefix + "Difficulty.AnomalyMode.";

    internal static AnomalyMode Instance { get; set; }

    public override bool Enabled
    {
        get => CAWorld.Anomaly;
        set => CAWorld.Anomaly = value;
    }

    public override Asset<Texture2D> Texture { get; }

    public override LocalizedText ExpandedDescription => Language.GetText(prefix + "AnomalyExpandedInfo");

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
        Texture = ModContent.Request<Texture2D>("CalamityAnomalies/Contents/Textures/UI/ModeIndicator_Anomaly", AssetRequestMode.AsyncLoad);
        DifficultyScale = 2.49147E23f;
        Name = Language.GetText(prefix+ "Anomaly");
        ShortDescription = Language.GetText(prefix + "AnomalyShortInfo");
        ActivationTextKey = prefix + "AnomalyActivate";
        DeactivationTextKey = prefix + "AnomalyDeactivate";
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
    private const string prefix = CAMain.ModLocalizationPrefix + "Difficulty.AnomalyMode.";

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

            switch (TOMathHelper.GetTwoBooleanStatus(!TOMain.LegendaryMode, Main.zenithWorld))
            {
                case TwoBooleanStatus.ATrue when CAWorld.AnomalyUltramundane:
                    NotLegendaryInfo();
                    DisableUltra();
                    break;
                case TwoBooleanStatus.BTrue when CAWorld.AnomalyUltramundane:
                    ZenithInfo();
                    DisableUltra();
                    break;
                case TwoBooleanStatus.Both when CAWorld.AnomalyUltramundane:
                    NotLegendaryInfo();
                    ZenithInfo();
                    DisableUltra();
                    break;
                case TwoBooleanStatus.Neither when !CAWorld.AnomalyUltramundane:
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

        #region 本地函数
        void DisableAnomaly()
        {
            TOLocalizationUtils.ChatLocalizedText(prefix + "AnomalyIllegal", Color.Red);
            CAWorld.Anomaly = false;
        }

        void NotLegendaryInfo()
        {
            TOLocalizationUtils.ChatLocalizedText(prefix + "AnomalyIllegalNotLegendary", Color.Red);
        }

        void ZenithInfo()
        {
            TOLocalizationUtils.ChatLocalizedText(prefix + "AnomalyIllegalZenith", Color.HotPink);
            //SoundEngine.PlaySound();
        }

        void EnableUltra()
        {
            TOLocalizationUtils.ChatLocalizedText(prefix + "AnomalyUltramundaneActivate", Color.Red);
            CAWorld.AnomalyUltramundane = true;
        }

        void DisableUltra()
        {
            TOLocalizationUtils.ChatLocalizedText(prefix + "AnomalyUltramundaneDeactivate", Color.Red);
            CAWorld.AnomalyUltramundane = false;
        }
        #endregion
    }
}
