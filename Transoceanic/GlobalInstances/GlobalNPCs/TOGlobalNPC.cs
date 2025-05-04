using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public int Master { get; set; } = -1;

    public bool HasMaster => Master >= 0 && Master <= Main.maxNPCs && Main.npc[Master].active;

    /// <summary>
    /// NPC的标识符。
    /// <br/>若NPC在进入世界后生成，则标识符为正数。
    /// </summary>
    public ulong Identifier { get; internal set; } = 0;
    /// <summary>
    /// 标识符分配器。
    /// <br/>进入世界时重置为0。
    /// </summary>
    internal static ulong _identifierAllocator;

    public ulong SpawnTime { get; internal set; } = 0;

    public override bool InstancePerEntity => true;

}
