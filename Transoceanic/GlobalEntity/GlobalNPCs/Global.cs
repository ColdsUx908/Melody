using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.GlobalEntity.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public int Master { get; set; } = -1;

    public bool HasMaster => Master >= 0 && Master <= Main.maxNPCs && Main.npc[Master].active;

    /// <summary>
    /// NPC的标识符。
    /// <br/>若NPC在进入世界后生成，则标识符为正数。
    /// </summary>
    public ulong Identifier { get; private set; }
    /// <summary>
    /// 标识符分配器。
    /// <br/>进入世界时重置为0。
    /// </summary>
    private static ulong _identifierAllocator;

    public int SpawnTime { get; private set; } = -1;

    public override bool InstancePerEntity => true;

}
