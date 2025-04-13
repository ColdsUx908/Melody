using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.GlobalEntity.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public int Master { get; set; } = -1;

    public ulong Identifier { get; private set; }
    internal static ulong _identifierAllocator;

    public int SpawnTime { get; private set; } = -1;

    public override bool InstancePerEntity => true;

}
