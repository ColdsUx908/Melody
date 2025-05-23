using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Transoceanic.Core.ExtraGameData;
using Transoceanic.Core.MathHelp;
using Transoceanic.Core.ExtraMathData;

namespace Transoceanic.Core.GameData;

/// <summary>
/// 运动学工具类。
/// </summary>
public static class TOKinematicUtils
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
                        switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, npc.IsBossTO()))
                        {
                            case TwoBooleanStatus.ATrue:
                                break;
                            case TwoBooleanStatus.BTrue:
                                target = npc;
                                hasPriority = true;
                                break;
                            case TwoBooleanStatus.Both:
                            case TwoBooleanStatus.Neither:
                                if (npc.lifeMax > target.lifeMax)
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
                        switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, npc.IsBossTO()))
                        {
                            case TwoBooleanStatus.ATrue:
                                break;
                            case TwoBooleanStatus.BTrue:
                                target = npc;
                                hasPriority = true;
                                break;
                            case TwoBooleanStatus.Both:
                            case TwoBooleanStatus.Neither:
                                if (npc.life > target.life)
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
                        switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, npc.IsBossTO()))
                        {
                            case TwoBooleanStatus.ATrue:
                                break;
                            case TwoBooleanStatus.BTrue:
                                target = npc;
                                distanceTemp2 = distanceTemp1;
                                hasPriority = true;
                                break;
                            case TwoBooleanStatus.Both:
                            case TwoBooleanStatus.Neither:
                                if (distanceTemp1 < distanceTemp2)
                                {
                                    target = npc;
                                    distanceTemp2 = distanceTemp1;
                                }
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
            return Main.LocalPlayer.IsAlive() && Vector2.DistanceSquared(origin, Main.LocalPlayer.Center) <= maxDistanceToCheckSquared ? Main.LocalPlayer : null;

        Player target = null;
        switch (priorityType)
        {
            case PriorityType.LifeMax:
                foreach (Player player in
                    TOIteratorFactory.NewActivePlayerIterator(
                        ignoreTiles ? k => k.IsAlive() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                        : k => k.IsAlive() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                {
                    if (target is null || player.statLifeMax2 > target.statLifeMax2)
                        target = player;
                }
                return target;
            case PriorityType.Life:
                foreach (Player player in
                    TOIteratorFactory.NewActivePlayerIterator(
                        ignoreTiles ? k => k.IsAlive() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                        : k => k.IsAlive() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
                {
                    if (target is null || player.statLife > target.statLife)
                        target = player;
                }
                return target;
            default:
                float distanceTemp1 = 0f;
                float distanceTemp2 = maxDistanceToCheckSquared;
                foreach (Player player in
                    TOIteratorFactory.NewActivePlayerIterator(
                        ignoreTiles ? k => k.IsAlive() && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared
                        : k => k.IsAlive() && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1)))
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
                    TOIteratorFactory.NewActivePlayerIterator(
                        ignoreTiles ? k => k.IsPvP() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                        : k => k.IsPvP() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1), owner))
                {
                    if (target is null || player.statLifeMax2 > target.statLifeMax2)
                        target = player;
                }
                return target;
            case PriorityType.Life:
                foreach (Player player in
                    TOIteratorFactory.NewActivePlayerIterator(
                        ignoreTiles ? k => k.IsPvP() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared
                        : k => k.IsPvP() && Vector2.DistanceSquared(origin, k.Center) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1), owner))
                {
                    if (target is null || player.statLife > target.statLife)
                        target = player;
                }
                return target;
            default:
                float distanceTemp1 = 0f;
                float distanceTemp2 = maxDistanceToCheckSquared;
                foreach (Player player in
                    TOIteratorFactory.NewActivePlayerIterator(
                        ignoreTiles ? k => k.IsPvP() && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared
                        : k => k.IsPvP() && (distanceTemp1 = Vector2.DistanceSquared(origin, k.Center)) <= maxDistanceToCheckSquared && Collision.CanHit(origin, 1, 1, k.Center, 1, 1), owner))
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
    /// 使弹幕追踪指定目标（反物理规则）。
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="target">追踪目标。</param>
    /// <param name="homingRatio">追踪强度。为1f时强制追踪。</param>
    /// <param name="homingAngle">视野范围。</param>
    /// <param name="keepVelocity">是否在调整角度时保持速度大小不变。仅在追踪强度不为1时有效。</param>
    /// <param name="rotating"></param>
    /// <remarks>须由具体实现决定目标锁定机制。</remarks>
    /// <returns>若追踪成功，true，否则，false。</returns>
    public static bool Homing<T>(this Projectile projectile, T target, float homingRatio = 1f, float homingAngle = MathHelper.TwoPi, bool keepVelocity = true, bool rotating = false)
        where T : Entity
    {
        if (target is not null && target.active)
        {
            Vector2 distanceVector = target.Center - projectile.Center;
            float distance = distanceVector.Length();

            if (homingAngle != MathHelper.TwoPi && TOMathHelper.IncludedAngle(projectile.velocity, distanceVector) > homingAngle / 2f)
                return false;

            float velocityLength = projectile.velocity.Length();
            Vector2 distanceVector2 = TOMathHelper.ToCustomLength(distanceVector, velocityLength);
            if (homingRatio == 1f)
                projectile.velocity = distance < velocityLength ? distanceVector : distanceVector2;
            else
            {
                projectile.velocity = Vector2.SmoothStep(projectile.velocity, distanceVector2, homingRatio);
                if (keepVelocity)
                    projectile.velocity = projectile.velocity.ToCustomLength(velocityLength);
            }

            if (rotating)
                projectile.VelocityToRotation();
            return true;
        }
        else
            return false;
    }
}