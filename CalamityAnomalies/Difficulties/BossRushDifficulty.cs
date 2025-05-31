using CalamityAnomalies.Configs;
using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.ModLoader;
using Transoceanic;

namespace CalamityAnomalies.Difficulties;

/// <summary>
/// BossRush难度。
/// <br/>所有相关更改均用钩子实现。
/// </summary>
public sealed class BossRushMode : DifficultyMode, ITOLoader
{
    private const string localizationPrefix = CAMain.ModLocalizationPrefix + "Difficulty.BossRushMode.";

    public static readonly Color BossRushModeColor = new(0xF0, 0x80, 0x80);

    internal static BossRushMode Instance { get; set; }

    public override bool Enabled
    {
        get => CAWorld.BossRush;
        set => CAWorld.BossRush = value;
    }

    public override Asset<Texture2D> Texture { get; }

    public override LocalizedText ExpandedDescription => Language.GetText(localizationPrefix + "BossRushExpandedInfo");

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
        Name = Language.GetText(localizationPrefix + "BossRush");
        ShortDescription = Language.GetText(localizationPrefix + "BossRushShortInfo");
        ActivationTextKey = localizationPrefix + "BossRushActivate";
        DeactivationTextKey = localizationPrefix + "BossRushDeactivate";
        ActivationSound = SupremeCalamitas.SpawnSound;
        ChatTextColor = BossRushModeColor;
    }

    void ITOLoader.PostSetupContent()
    {
        Instance = this;
        if (CAServerConfig.Instance.BossRushDifficulty)
        {
            DifficultyModeSystem.Difficulties.Add(Instance);
            DifficultyModeSystem.CalculateDifficultyData();
        }
    }

    void ITOLoader.OnModUnload()
    {
        DifficultyModeSystem.Difficulties.Remove(Instance);
        DifficultyModeSystem.CalculateDifficultyData();
        Instance = null;
    }
}

public sealed class BossRushManageMent : ModSystem
{
    public override void PreUpdateEntities()
    {
        CAWorld.RealBossRushEventActive = BossRushEvent.BossRushActive;

        if (CAWorld.BossRush)
        {
            CalamityWorld.revenge = true;
            CalamityWorld.death = true;
        }
    }
}
