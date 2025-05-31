using System;
using Terraria;

namespace Transoceanic.GameData.Utilities;

public static class TOEntityUtils
{
    /// <summary>
    /// 对于有type字段的 <see cref="Entity"/> 的子类变量，获取其type。
    /// <br/>对于 <see cref="NPC"/>，如果其 <see cref="NPC.netID"/> 小于0，则返回 <see cref="NPC.netID"/>，否则返回 <see cref="NPC.type"/>。
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static int GetEntityType(this Entity entity) => entity switch
    {
        NPC npc => npc.netID < 0 ? npc.netID : npc.type,
        Projectile projectile => projectile.type,
        Item item => item.type,
        Player => throw new ArgumentException("Players do not have a type.", nameof(entity)),
        _ => throw new ArgumentException("Unknown Entity", nameof(entity)),
    };
}
