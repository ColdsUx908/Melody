using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.GlobalProjectiles;

public partial class TOGlobalProjectile : GlobalProjectile
{
    #region 扩展属性
    private Vector2 acceleration = Vector2.Zero;

    /// <summary>
    /// 弹幕加速度。
    /// </summary>
    public Vector2 Acceleration { get => acceleration; set => acceleration = value; }
    /// <summary>
    /// 弹幕速度乘数。每帧将弹幕速度乘以一个值
    /// </summary>
    public double VelocityMult { get; set; } = 1;
    #endregion
}
