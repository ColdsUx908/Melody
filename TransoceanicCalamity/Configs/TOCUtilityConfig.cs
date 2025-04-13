using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Transoceanic.Configs;

namespace TransoceanicCalamity.Configs;

public class TOCUtilityConfig : ModConfig
{
    public TOCUtilityConfig()
    {
    }

    public static TOCUtilityConfig Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => false;

    public override void OnLoaded() => Instance = this;

    [DefaultValue(false)]
    public bool PlayerDownedBossPolluted { get; set; }
}
