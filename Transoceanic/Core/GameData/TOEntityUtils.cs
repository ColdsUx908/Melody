using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Transoceanic.Core.GameData;

public static class TOEntityUtils
{
    public static int GetEntityType(this Entity entity) => entity switch
    {
        NPC npc => npc.type,
        Projectile projectile => projectile.type,
        Item item => item.type,
        Player => throw new ArgumentException("Players do not have a type.", nameof(entity)),
        _ => throw new ArgumentException("Unknown Entity", nameof(entity)),
    };
}
