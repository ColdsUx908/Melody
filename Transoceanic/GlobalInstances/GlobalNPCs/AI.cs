using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public override bool PreAI(NPC npc)
    {
        LifeRatio = (float)npc.life / npc.lifeMax;
        LifeRatioReverse = 1f - LifeRatio;

        return true;
    }

    public override void AI(NPC npc)
    {
    }

    public override void PostAI(NPC npc)
    {
    }
}
