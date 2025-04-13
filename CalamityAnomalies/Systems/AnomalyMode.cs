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
using Transoceanic.Core;
using Transoceanic.Data;
using Transoceanic.Systems;

namespace CalamityAnomalies.Systems;

public class AnomalyMode : DifficultyMode
{
    internal static AnomalyMode Instance { get; set; } = null;

    public override bool Enabled
    {
        get => CAWorld.Anomaly;
        set => CAWorld.Anomaly = value;
    }

    public override Asset<Texture2D> Texture => field ??= ModContent.Request<Texture2D>("CalamityAnomalies/Textures/Anomaly/ModeIndicator_Anomaly", AssetRequestMode.AsyncLoad);
    public override LocalizedText ExpandedDescription => Language.GetOrRegister(CAMain.ModLocalizationPrefix + "Difficulty.AnomalyExpandedInfo");

    public override int FavoredDifficultyAtTier(int tier)
    {
        DifficultyMode[] tierList = DifficultyModeSystem.DifficultyTiers[tier];
        for (int i = 0; i < tierList.Length; i++)
            if (tierList[i].Name == CalamityUtils.GetText("UI.Death"))
                return i;
        return 0;
    }

    public AnomalyMode()
    {
        DifficultyScale = 2.49147E23f;
        Name = Language.GetOrRegister(CAMain.ModLocalizationPrefix + "Difficulty.Anomaly");
        ShortDescription = Language.GetOrRegister(CAMain.ModLocalizationPrefix + "Difficulty.AnomalyShortInfo");
        ActivationTextKey = CAMain.ModLocalizationPrefix + "Difficulty.AnomalyActivate";
        DeactivationTextKey = CAMain.ModLocalizationPrefix + "Difficulty.AnomalyDeactivate";
        ActivationSound = SupremeCalamitas.BulletHellEndSound;
        ChatTextColor = Color.HotPink;
    }
}

public class AnomalyManagement : ModSystem
{
    public override void PreUpdateWorld()
    {
        if (CAWorld.Anomaly)
        {
            if (!Main.masterMode)
            {
                DisableAnomaly();
                return;
            }

            CalamityWorld.revenge = true;
            CalamityWorld.death = true;

            switch (TOMathHelper.GetTwoBooleanStatus(!TOWorld.LegendaryMode, Main.zenithWorld))
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
            TOLocalizationUtils.ChatLocalizedText(CAMain.ModLocalizationPrefix + "Difficulty.AnomalyIllegal", Color.Red);
            CAWorld.Anomaly = false;
        }

        void NotLegendaryInfo()
        {
            TOLocalizationUtils.ChatLocalizedText(CAMain.ModLocalizationPrefix + "Difficulty.AnomalyIllegalNotLegendary", Color.Red);
        }

        void ZenithInfo()
        {
            TOLocalizationUtils.ChatLocalizedText(CAMain.ModLocalizationPrefix + "Difficulty.AnomalyIllegalZenith", Color.HotPink);
            //SoundEngine.PlaySound();
        }

        void EnableUltra()
        {
            TOLocalizationUtils.ChatLocalizedText(CAMain.ModLocalizationPrefix + "Difficulty.AnomalyUltramundaneActivate", Color.Red);
            CAWorld.AnomalyUltramundane = true;
        }

        void DisableUltra()
        {
            TOLocalizationUtils.ChatLocalizedText(CAMain.ModLocalizationPrefix + "Difficulty.AnomalyUltramundaneDeactivate", Color.Red);
            CAWorld.AnomalyUltramundane = false;
        }
        #endregion
    }
}
