namespace Transoceanic.GameData;

/// <summary>
/// 运动学工具类。
/// </summary>
public static class TOKinematic
{
    /// <summary>
    /// 获取NPC目标。
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="maxDistanceToCheck"></param>
    /// <param name="ignoreTiles"></param>
    /// <remarks>警告：遍历NPC对性能有较大影响。</remarks>
    /// <returns>若未找到目标，null；否则，NPC实例。</returns>
    public static NPC GetNPCTarget(Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false, PriorityType priorityType = PriorityType.Closest)
    {
        float maxDistanceToCheckSquared = maxDistanceToCheck * maxDistanceToCheck;
        NPC target = null;
        bool hasPriority = false;
        switch (priorityType)
        {
            case PriorityType.LifeMax:
                if (bossPriority)
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? k => k.CanBeChasedBy() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                            : k => k.CanBeChasedBy() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                    {
                        if (target is null)
                        {
                            target = npc;
                            continue;
                        }
                        switch (hasPriority, npc.TOBoss)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when npc.lifeMax > target.lifeMax:
                                target = npc;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? k => k.CanBeChasedBy() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                            : k => k.CanBeChasedBy() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                    {
                        if (target is null || npc.lifeMax < target.lifeMax)
                            target = npc;
                    }
                }
                return target;
            case PriorityType.Life:
                if (bossPriority)
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? k => k.CanBeChasedBy() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                            : k => k.CanBeChasedBy() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                    {
                        if (target is null)
                        {
                            target = npc;
                            continue;
                        }
                        switch (hasPriority, npc.TOBoss)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when npc.life > target.life:
                                target = npc;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? k => k.CanBeChasedBy() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                            : k => k.CanBeChasedBy() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                    {
                        if (target is null || npc.life < target.life)
                            target = npc;
                    }
                }
                return target;
            default:
                float distanceTemp1 = 0f;
                float distanceTemp2 = maxDistanceToCheckSquared;
                if (bossPriority)
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? k => k.CanBeChasedBy() && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared
                            : k => k.CanBeChasedBy() && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                    {
                        if (target is null)
                        {
                            target = npc;
                            distanceTemp2 = distanceTemp1;
                            continue;
                        }
                        switch (hasPriority, npc.TOBoss)
                        {
                            case (true, false):
                                break;
                            case (false, true):
                                target = npc;
                                distanceTemp2 = distanceTemp1;
                                hasPriority = true;
                                break;
                            case (true, true) or (false, false) when distanceTemp1 < distanceTemp2:
                                target = npc;
                                distanceTemp2 = distanceTemp1;
                                break;
                        }
                    }
                }
                else
                {
                    foreach (NPC npc in
                        TOIteratorFactory.NewNPCIterator(
                            ignoreTiles ? k => k.CanBeChasedBy() && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared
                            : k => k.CanBeChasedBy() && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                    {
                        if (target is null || distanceTemp1 < distanceTemp2)
                        {
                            target = npc;
                            distanceTemp2 = distanceTemp1;
                        }
                    }
                }
                return target;
        }
    }


    /// <summary>
    /// 获取玩家目标。
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="maxDistanceToCheck"></param>
    /// <param name="ignoreTiles"></param>
    /// <param name="priorityType"></param>
    /// <remarks>警告：该方法应由NPC调用。</remarks>
    /// <returns>若未找到目标；null；否则返回玩家实例。</returns>
    public static Player GetPlayerTarget(Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, PriorityType priorityType = PriorityType.Closest)
    {
        float maxDistanceToCheckSquared = maxDistanceToCheck * maxDistanceToCheck;

        if (Main.netMode == NetmodeID.SinglePlayer)
            return Main.LocalPlayer.Alive && Vector2.DistanceSquared(origin, Main.LocalPlayer.Center) <= maxDistanceToCheckSquared ? Main.LocalPlayer : null;

        Player target = null;
        switch (priorityType)
        {
            case PriorityType.LifeMax:
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? k => k.Alive && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                        : k => k.Alive && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                {
                    if (target is null || player.statLifeMax2 > target.statLifeMax2)
                        target = player;
                }
                return target;
            case PriorityType.Life:
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? k => k.Alive && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                        : k => k.Alive && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                {
                    if (target is null || player.statLife > target.statLife)
                        target = player;
                }
                return target;
            default:
                float distanceTemp1 = 0f;
                float distanceTemp2 = maxDistanceToCheckSquared;
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? k => k.Alive && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared
                        : k => k.Alive && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                {
                    if (target is null || distanceTemp1 < distanceTemp2)
                    {
                        target = player;
                        distanceTemp2 = distanceTemp1;
                    }
                }
                return target;
        }
    }

    /// <summary>
    /// 获取PvP状态的玩家目标。
    /// </summary>
    /// <param name="owner">调用方法的玩家。不会将该玩家作为潜在目标。</param>
    /// <param name="origin"></param>
    /// <param name="maxDistanceToCheck"></param>
    /// <param name="ignoreTiles"></param>
    /// <param name="priorityType"></param>
    /// <remarks>警告：该方法应由玩家调用。慎用PvP玩家目标功能。</remarks>
    /// <returns>若游戏为单人模式，null；若调用玩家未开启PvP，null；若未找到目标；null；否则，玩家实例。</returns>
    public static Player GetPvPPlayerTarget(Player owner, Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, PriorityType priorityType = PriorityType.Closest)
    {
        if (Main.netMode == NetmodeID.SinglePlayer || !owner.active || !owner.hostile)
            return null;

        float maxDistanceToCheckSquared = maxDistanceToCheck * maxDistanceToCheck;
        Player target = null;

        switch (priorityType)
        {
            case PriorityType.LifeMax:
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? k => k.PvP && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                        : k => k.PvP && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1), owner))
                {
                    if (target is null || player.statLifeMax2 > target.statLifeMax2)
                        target = player;
                }
                return target;
            case PriorityType.Life:
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? k => k.PvP && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                        : k => k.PvP && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1), owner))
                {
                    if (target is null || player.statLife > target.statLife)
                        target = player;
                }
                return target;
            default:
                float distanceTemp1 = 0f;
                float distanceTemp2 = maxDistanceToCheckSquared;
                foreach (Player player in
                    TOIteratorFactory.NewPlayerIterator(
                        ignoreTiles ? k => k.PvP && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared
                        : k => k.PvP && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1), owner))
                {
                    if (target is null || distanceTemp1 < distanceTemp2)
                    {
                        target = player;
                        distanceTemp2 = distanceTemp1;
                    }
                }
                return target;
        }
    }
}