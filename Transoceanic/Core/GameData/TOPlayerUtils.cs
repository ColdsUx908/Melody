using Terraria;

namespace Transoceanic.Core.GameData;

public static class TOPlayerUtils
{
    public static bool IsAlive(this Player player) => !player.dead && !player.ghost;

    public static bool IsPvP(this Player player) => player.IsAlive() && player.hostile;

    public static bool IsTeammate(this Player player) => player.IsAlive() && player.team != 0 && player.team == Main.LocalPlayer.team;
}
