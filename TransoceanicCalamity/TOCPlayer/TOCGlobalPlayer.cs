using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Transoceanic.Core;
using TransoceanicCalamity.Configs;
using TransoceanicCalamity.Core;
using TransoceanicCalamity.Systems;

namespace TransoceanicCalamity.TOCPlayer;

public class TOCGlobalPlayer : ModPlayer
{
    public PlayerDownedBossCalamity PlayerDownedBossCalamity { get; set; } = new PlayerDownedBossCalamity();

    public bool AntiEPBPlayer { get; set; } = false;

    public override void PostUpdate()
    {
        if (TOCUtilityConfig.Instance.PlayerDownedBossPolluted)
            PlayerDownedBossCalamity.WorldPolluted();

        if (TOCKeyBindSystem.AntiEPB.JustPressed)
        {
            TOLocalizationUtils.ChatLocalizedTextWith(
                TOCMain.ModLocalizationPrefix + "Systems.AntiEPB.PlayerInfo",
                Player,
                Color.White,
                Player.OceanCal().AntiEPBPlayer = !Player.OceanCal().AntiEPBPlayer);
        }
    }

    public override void SaveData(TagCompound tag)
    {
        List<string> downed = [];

        PlayerDownedBossCalamity.SaveDataTo(downed);

        tag["PlayerDownedBossCalamity"] = downed;
    }

    public override void LoadData(TagCompound tag)
    {
        IList<string> downedLoaded = tag.GetList<string>("PlayerDownedBossCalamity");

        PlayerDownedBossCalamity.LoadDataFrom(downedLoaded);
    }
}
