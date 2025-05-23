using System;
using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public override void SetDefaults(NPC npc)
    {
        Array.Fill(OceanAI, 0f);

        Master = Main.maxNPCs;
    }
}
