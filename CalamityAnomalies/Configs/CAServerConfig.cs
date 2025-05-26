using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace CalamityAnomalies.Configs;
public class CAServerConfig : ModConfig
{
    public CAServerConfig()
    {
    }

    public static CAServerConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;

    [Header("Content")]

    [ReloadRequired]
    [DefaultValue(false)]
    public bool TweaksEnabled { get; set; }

    [ReloadRequired]
    [DefaultValue(false)]
    public bool BossRushDifficulty { get; set; }
}
