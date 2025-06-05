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
        /// <param name="velocity">速度。</param>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使弹幕额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。
        /// </param>
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            projectile.velocity = velocity;
            projectile.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 适用于贴图方向向上的弹幕，用于将 <see cref="Entity.velocity"/> 转换为 <see cref="Projectile.rotation"/>，并应用于弹幕。
        /// </summary>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使弹幕额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。
        public void VelocityToRotation(float rotationOffset = 0f) => projectile.rotation = projectile.velocity.ToRotation() + rotationOffset;
    }

    extension(Projectile)
    {
        /// <summary>
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void NewProjectileAction_TO(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner);
            if (index < Main.maxProjectiles)
                action?.Invoke(Main.projectile[index]);
        }

        /// <summary>
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModProjectile所属类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void NewProjectileAction_TO<T>(IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null) where T : ModProjectile =>
            NewProjectileAction_TO(source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);

        /// <summary>
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <param name="index">输出的Projectile索引。</param>
        /// <param name="projectile">输出的Projectile实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        /// <returns>生成Projectile是否成功。</returns>
        public static bool NewProjectileActionCheck_TO(out int index, out Projectile projectile, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner);
            if (index < Main.maxProjectiles)
            {
                projectile = Main.projectile[index];
                action?.Invoke(projectile);
                return true;
            }
            else
            {
                projectile = null;
                return false;
            }
        }

        /// <summary>
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModProjectile所属类型。</typeparam>
        /// <param name="index">输出的Projectile索引。</param>
        /// <param name="projectile">输出的Projectile实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        /// <returns>生成Projectile是否成功。</returns>
        public static bool NewProjectileActionCheck_TO<T>(out int index, out Projectile projectile, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null) where T : ModProjectile =>
            NewProjectileActionCheck_TO(out index, out projectile, source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);


        /// <summary>
        /// 生成指定数量的Projectile，使用指定的旋转角度。
        /// </summary>
        /// <param name="number">弹幕总数。</param>
        /// <param name="radian">单次旋转角度（顺时针）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void RotatedProj_TO(int number, float radian,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            for (int i = 0; i < number; i++)
                NewProjectileAction_TO(source, position, velocity.RotatedBy(radian * i), type, damage, knockback, owner, action);
        }

        /// <summary>
        /// 生成指定数量的Projectile，使用指定的旋转角度。
        /// </summary>
        /// <param name="indexes">输出的Projectile索引数组。</param>
        /// <param name="projectiles">输出的Projectile实例数组。</param>
        /// <param name="spawnedNumber">输出的实际生成的Projectile数量。</param>
        /// <param name="number">弹幕总数。</param>
        /// <param name="offset">单次旋转角度（顺时针）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        /// <returns>Projectile是否全部生成。</returns>
        public static bool RotatedProjCheck_TO(out List<int> indexes, out List<Projectile> projectiles, out int spawnedNumber, int number, float offset,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            indexes = [];
            projectiles = [];
            spawnedNumber = 0;
            bool allSuccess = true;
            PolarVector2 temp = (PolarVector2)velocity;
            for (int i = 0; i < number; i++)
            {
                if (NewProjectileActionCheck_TO(out int index, out Projectile projectile, source, position, temp.RotatedBy(offset * i), type, damage, knockback, owner, action))
                {
                    indexes.Add(index);
                    projectiles.Add(projectile);
                    spawnedNumber++;
                }
                else
                    allSuccess = false;
            }
            return allSuccess;
        }
    }
}
