using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace CalamityAnomalies.Configs;
public class CAClientConfig : ModConfig
{
    public CAClientConfig()
    {
    }

    public static CAClientConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;

    [Header("Content")]

    [ReloadRequired]
    [DefaultValue(false)]
    public bool BossRushDifficulty { get; set; }
}
