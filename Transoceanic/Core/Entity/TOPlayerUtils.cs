using Terraria;

namespace Transoceanic.Core;

public static class TOPlayerUtils
{
    public static bool IsAlive(this Player player) => player.active && !player.dead && !player.ghost;

    public static bool IsPvP(this Player player) => player.active && player.hostile;
}
