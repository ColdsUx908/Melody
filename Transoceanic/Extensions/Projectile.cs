namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(Projectile projectile)
    {
        public TOGlobalProjectile Ocean() => projectile.GetGlobalProjectile<TOGlobalProjectile>();

        public T GetModProjectile<T>() where T : ModProjectile => projectile.ModProjectile as T;

        public bool OnOwnerClient => projectile.owner == Main.myPlayer;

        /// <summary>
        /// 将弹幕速度设置为指定值，同时更新旋转。
        /// <br>为性能考虑，不要在不改变方向的情况中重复调用该方法。</br>
        /// </summary>
        /// <param name="velocity"></param>
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            projectile.velocity = velocity;
            projectile.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 适用于贴图方向向上的弹幕，用于将 <see cref="Entity.velocity"/> 转换为 <see cref="Projectile.rotation"/>，并应用于弹幕。
        /// </summary>
        public void VelocityToRotation(float rotationOffset = 0f) => projectile.rotation = projectile.velocity.ToRotation() + rotationOffset;
    }
}
