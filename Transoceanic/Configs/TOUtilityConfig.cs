using Terraria.ModLoader.Config;

namespace Transoceanic.Configs;

public partial class TOUtilityConfig : ModConfig
{
    public static TOUtilityConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;

    //[Header("UI")]
}
