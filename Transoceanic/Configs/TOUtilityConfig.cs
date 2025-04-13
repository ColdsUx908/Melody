using System;
using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace Transoceanic.Configs;

public partial class TOUtilityConfig : ModConfig
{
    public TOUtilityConfig()
    {
    }

    public static TOUtilityConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;

    [Header("UI")]

    [DefaultValue(false)]
    public bool BossTimer { get; set; }

    [DefaultValue(true)]
    public bool DisplayTicks { get; set; }

    [Range(0f, 1.25f)]
    [DefaultValue(0.7917f)]
    public float XScale { get; set; }

    [Range(0f, 1.25f)]
    [DefaultValue(0.963f)]
    public float YScale { get; set; }

    [Range(0f, 10f)]
    [DefaultValue(1.35f)]
    public float Size { get; set; }

    [Range(0, 10)]
    [DefaultValue(1)]
    public int Bar { get; set; }
}
