namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(Entity entity)
    {
        /// <summary>
        /// 尝试获取实体的 <c>type</c>。
        /// </summary>
        /// <returns>
        /// 获取的 <c>type</c> 值。
        /// <br/>对于 <see cref="NPC"/>，如果其 <see cref="NPC.netID"/> 小于0，则返回 <see cref="NPC.netID"/>，否则返回 <see cref="NPC.type"/>。
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public int EntityType => entity switch
        {
            NPC npc => npc.netID < 0 ? npc.netID : npc.type,
            Projectile projectile => projectile.type,
            Item item => item.type,
            Player => throw new ArgumentException("Players do not have a type.", nameof(entity)),
            _ => throw new ArgumentException("Unknown Entity", nameof(entity)),
        };

        public Vector2 GetVelocityTowards(Entity target, float length) => (target.Center - entity.Center).ToCustomLength(length);

        public bool Homing(Vector2 destination, float homingRatio = 1f, float sightAngle = MathHelper.TwoPi, bool keepVelocity = true, float? velocityOverride = null)
        {
            Vector2 distanceVector = destination - entity.Center;
            float distance = distanceVector.Length();

            if (sightAngle != MathHelper.TwoPi && Vector2.IncludedAngle(entity.velocity, distanceVector) > sightAngle / 2f)
                return false;

            float velocityLength = velocityOverride ?? entity.velocity.Length();
            Vector2 distanceVector2 = distanceVector.ToCustomLength(velocityLength);
            if (homingRatio == 1f)
                entity.velocity = distance < velocityLength ? distanceVector : distanceVector2;
            else
            {
                entity.velocity = Vector2.SmoothStep(entity.velocity, distanceVector2, homingRatio);
                if (keepVelocity)
                    entity.velocity.Modulus = velocityLength;
            }

            return true;
        }

        /// <summary>
        /// 使实体追踪指定目标（反物理规则）。
        /// </summary>
        /// <param name="target">追踪目标。</param>
        /// <param name="homingRatio">追踪强度。为1时强制追踪。</param>
        /// <param name="sightAngle">视野范围。</param>
        /// <param name="keepVelocity">是否在调整角度时保持速度大小不变。仅在追踪强度不为1时有效。</param>
        /// <remarks>须由具体实现决定目标锁定机制。</remarks>
        /// <returns>若追踪成功，true，否则，false。</returns>
        public bool Homing<T>(T target, float homingRatio = 1f, float sightAngle = MathHelper.TwoPi, bool keepVelocity = true, float? velocityOverride = null) where T : Entity =>
            target is not null && target.active && entity.Homing(target.Center, homingRatio, sightAngle, keepVelocity, velocityOverride);
    }
}