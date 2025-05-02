using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityAnomalies.GlobalInstances.CAPlayer;

public class CAPlayer : ModPlayer
{
    public PlayerDownedBossCalamity PlayerDownedBossCalamity { get; } = new();
    public PlayerDownedBossCalamity PlayerDownedBossAnomaly { get; } = new();

    public bool AntiEPBPlayer { get; set; } = false;

    public override void PostUpdate()
    {
        //PlayerDownedBossCalamity.WorldPolluted();
    }

    public override void SaveData(TagCompound tag)
    {
        PlayerDownedBossCalamity.SaveData(tag, "PlayerDownedBossCalamity");
        PlayerDownedBossAnomaly.SaveData(tag, "PlayerDownedBossAnomaly");
    }

    public override void LoadData(TagCompound tag)
    {
        PlayerDownedBossCalamity.LoadData(tag, "PlayerDownedBossCalamity");
        PlayerDownedBossAnomaly.LoadData(tag, "PlayerDownedBossAnomaly");
    }
}
