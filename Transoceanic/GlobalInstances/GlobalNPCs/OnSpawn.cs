using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        Identifier = ++_identifierAllocator; //城镇NPC这类NPC不会拥有在这里被设置标识符的机会
        SpawnTime = TOMain.GeneralTimer;
    }
}
