using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.Core.GameData.Utilities;

public static class TOProjectileUtils
{
    public static T GetModProjectile<T>(this Projectile projectile) where T : ModProjectile => projectile.ModProjectile as T;

    /// <summary>
    /// 将弹幕速度设置为指定值，同时更新旋转。
    /// <br>为性能考虑，不要在不改变方向的情况中重复调用该方法。</br>
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="velocity"></param>
    public static void SetVelocityandRotation(this Projectile projectile, Vector2 velocity)
    {
        projectile.velocity = velocity;
        projectile.VelocityToRotation();
    }

    /// <summary>
    /// 适用于贴图方向向上的弹幕，用于将 <see cref="GameData.velocity"/> 转换为 <see cref="Projectile.rotation"/>，并应用于弹幕。
    /// </summary>
    public static void VelocityToRotation(this Projectile projectile)
    {
        float temp = projectile.velocity.ToRotation();

        projectile.rotation = temp switch
        {
            0f => 0f,
            _ => MathHelper.PiOver2 + temp
        };
    }
}