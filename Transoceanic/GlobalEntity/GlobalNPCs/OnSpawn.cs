using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Transoceanic.GlobalEntity.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        Identifier = ++_identifierAllocator; //城镇NPC这类NPC不会拥有被设置标识符的机会，所以须保证其他NPC的标识符为正数
        SpawnTime = TOMain.GeneralTimer;
    }
}
