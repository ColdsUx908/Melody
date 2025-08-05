using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CalamityAnomalies.Core;

public sealed class CAServerConfig : ModConfig
{
    public static CAServerConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ServerSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;

    [ReloadRequired]
    [DefaultValue(true)]
    public bool Contents;
}
