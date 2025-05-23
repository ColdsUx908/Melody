using System;
using Microsoft.Xna.Framework;
using Transoceanic.Core.ExtraMathData;

namespace Transoceanic.Core.MathHelp;

public static partial class TOMathHelper
{
    /// <summary>
    /// 计算两个向量的夹角。
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static float IncludedAngle(Vector2 value1, Vector2 value2) => (float)Math.Acos(Vector2.Dot(value1, value2) / (value1.Length() * value2.Length()));

    /// <summary>
    /// 安全地将向量化为单位向量。
    /// </summary>
    /// <param name="value"></param>
    /// <returns>零向量返回零向量，否则返回单位向量。</returns>
    public static Vector2 SafelyNormalize(Vector2 value) => value == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(value);

    /// <summary>
    /// 获取向量的顺时针旋转角。
    /// </summary>
    /// <param name="value"></param>
    /// <returns>零向量返回0，否则返回 [0, 2π) 范围内的浮点值。</returns>
    public static float ToAngle(this Vector2 value) => value.Y switch
    {
        > 0f => MathF.Atan2(value.Y, value.X),
        0f => value.X switch
        {
            >= 0f => 0f, //零向量返回0，方向为x轴正方向返回0
            _ => MathHelper.Pi //方向为x轴负方向返回Pi
        },
        _ => MathHelper.TwoPi + MathF.Atan2(value.Y, value.X), //将Atan2方法返回的负值转换为正值
    };

    /// <summary>
    /// 获取模为特定值的原向量同向向量。不改变原向量值。
    /// </summary>
    /// <param name="value"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static Vector2 ToCustomLength(this Vector2 value, float length) => SafelyNormalize(value) * length;

    /// <summary>
    /// 获取两个向量角平分线的单位方向向量。
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static Vector2 UnitAngleBisector(Vector2 value1, Vector2 value2) => new PolarVector2((value1.ToAngle() + value2.ToAngle()) / 2);
}
