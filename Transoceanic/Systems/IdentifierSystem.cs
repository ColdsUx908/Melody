using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Transoceanic.Core.GameData;
using Transoceanic.GlobalInstances.GlobalNPCs;

namespace Transoceanic.Systems;

public class IdentifierSystem : ModSystem
{
    public override void OnWorldLoad()
    {
        foreach (NPC npc in TOMain.ActiveNPCs)
        {
            TOGlobalNPC oceanNPC = npc.Ocean();
            oceanNPC.Identifier = ++TOGlobalNPC._identifierAllocator;
            oceanNPC.SpawnTime = TOMain.GeneralTimer;
        }
    }
}
