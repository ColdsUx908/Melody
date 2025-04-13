using Terraria;
using Terraria.ModLoader;
using TransoceanicCalamity.Core;

namespace TransoceanicCalamity.Systems;

public class AntiEPBSystem : ModSystem
{
    public static bool AntiEPBGeneral { get; set; } = false;

    public override void PostUpdatePlayers()
    {
        AntiEPBGeneral = Main.LocalPlayer.OceanCal().AntiEPBPlayer;
    }
}
