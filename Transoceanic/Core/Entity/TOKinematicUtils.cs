using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Transoceanic.Data;

namespace Transoceanic.Core;

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
        TOEntityIterator<NPC> npcs = ignoreTiles ? TOEntityIteratorCreator.NewNPCIterator(k => k.CanBeChasedBy()) :
            TOEntityIteratorCreator.NewNPCIterator(k => k.CanBeChasedBy() && Collision.CanHit(origin, 1, 1, k.Center, 1, 1));
        Predicate<NPC> priorityFunc = TONPCUtils.IsBossTO;
        NPC target = null;
        _ = priorityType switch
        {
            PriorityType.Closest => bossPriority ? npcs.MinFrom(k => Vector2.DistanceSquared(origin, k.Center), priorityFunc, out _, out target) : npcs.MinFrom(k => Vector2.DistanceSquared(origin, k.Center), out target),
            PriorityType.LifeMax => bossPriority ? npcs.MaxFrom(k => k.lifeMax, priorityFunc, out _, out target) : npcs.MaxFrom(k => k.lifeMax, out target),
            PriorityType.Life => bossPriority ? npcs.MaxFrom(k => k.life, priorityFunc, out _, out target) : npcs.MaxFrom(k => k.life, out target),
            _ => npcs.MinFrom(k => Vector2.DistanceSquared(origin, k.Center), out target)
        };
        return target is not null && Vector2.DistanceSquared(origin, target.Center) <= maxDistanceToCheck * maxDistanceToCheck ? target : null;
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
        Player target = null;
        if (Main.netMode == NetmodeID.SinglePlayer)
            target = Main.LocalPlayer;
        else
        {
            TOEntityIterator<Player> players = ignoreTiles ? TOEntityIteratorCreator.NewPlayerIterator(TOPlayerUtils.IsAlive) :
                TOEntityIteratorCreator.NewPlayerIterator(k => k.IsAlive() && Collision.CanHit(origin, 1, 1, k.Center, 1, 1));
            _ = priorityType switch
            {
                PriorityType.Closest => players.MinFrom(k => Vector2.DistanceSquared(origin, k.Center), out target),
                PriorityType.LifeMax => players.MaxFrom(k => k.statLifeMax2, out target),
                PriorityType.Life => players.MaxFrom(k => k.statLife, out target),
                _ => players.MinFrom(k => Vector2.DistanceSquared(origin, k.Center), out target)
            };
        }
        return target is not null && Vector2.DistanceSquared(origin, target.Center) <= maxDistanceToCheck * maxDistanceToCheck ? target : null;
    }

    /// <summary>
    /// 获取PvP状态的玩家目标。
    /// </summary>
    /// <param name="owner">调用方法的玩家。不会将该玩家作为潜在目标。</param>
    /// <param name="origin"></param>
    /// <param name="maxDistanceToCheck"></param>
    /// <param name="ignoreTiles"></param>
    /// <param name="priorityType"></param>
    /// <remarks>警告：该方法应由玩家调用。慎用PvP玩家目标功能</remarks>
    /// <returns>若游戏为单人模式，null；若调用玩家未开启PvP，null；若未找到目标；null；否则，玩家实例。</returns>
    public static Player GetPvPPlayerTarget(Player owner, Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, PriorityType priorityType = PriorityType.Closest)
    {
        Player target = null;
        if (Main.netMode == NetmodeID.SinglePlayer)
            return null;

        TOEntityIterator<Player> players = ignoreTiles ? TOEntityIteratorCreator.NewPlayerIterator(TOPlayerUtils.IsPvP) :
            TOEntityIteratorCreator.NewPlayerIterator(k => k.IsPvP() && Collision.CanHit(origin, 1, 1, k.Center, 1, 1));
        _ = priorityType switch
        {
            PriorityType.Closest => players.MinFrom(k => Vector2.DistanceSquared(origin, k.Center), out target),
            PriorityType.LifeMax => players.MaxFrom(k => k.statLifeMax2, out target),
            PriorityType.Life => players.MaxFrom(k => k.statLife, out target),
            _ => players.MinFrom(k => Vector2.DistanceSquared(origin, k.Center), out target)
        };
        return target is not null && Vector2.DistanceSquared(origin, target.Center) <= maxDistanceToCheck * maxDistanceToCheck ? target : null;
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
                    TOMathHelper.ToCustomLength(projectile.velocity, velocityLength);
            }

            if (rotating)
                projectile.VelocityToRotation();
            return true;
        }
        else
            return false;
    }
}