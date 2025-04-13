using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Transoceanic.Data;

namespace Transoceanic.Core;

public static class TOMathHelper
{
    /// <summary>
    /// 将像素每帧转换为英里每小时的转换因子。
    /// <br/>计算公式为：<c>C = 60f / 8f * 0.681818f</c>
    /// （一像素为 <c>1/8</c> 英尺）
    /// </summary>
    private const float PptToMph_Constant = 5.1136364f;

    public static float Pixptick_To_Mph(float value) => value * PptToMph_Constant;
    public static float Mph_To_Pixptick(float value) => value / PptToMph_Constant;

    public static void AddAssign(this ref Vector2 value1, float value2)
    {
        value1.X += value2;
        value1.Y += value2;
    }

    public static void AddAssign(this ref Vector2 value1, Vector2 value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
    }

    public static bool AtLeastXTrue(int x, params ReadOnlySpan<bool> span)
    {
        if (x <= 0)
            return true;
        if (span.Length < x)
            return false;

        int count = 0;
        foreach (bool value in span)
        {
            if (value && ++count >= x)
                return true;
        }

        return false;
    }
    /// <summary>
    /// 获取两个布尔值的逻辑状态，用于控制语句。
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <remarks>警告：须确保参数的传递顺序符合需求。</remarks>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TwoBooleanStatus GetTwoBooleanStatus(bool A, bool B) => A ? B ? TwoBooleanStatus.Both : TwoBooleanStatus.ATrue : B ? TwoBooleanStatus.BTrue : TwoBooleanStatus.Neither;

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

    public static void ScalarMultiplyAssign(this ref Vector2 value1, float value2)
    {
        value1.X *= value2;
        value1.Y *= value2;
    }

    public static void SubtractAssign(this ref Vector2 value1, float value2)
    {
        value1.X -= value2;
        value1.Y -= value2;
    }
    public static void SubtractAssign(this ref Vector2 value1, Vector2 value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
    }

    /// <summary>
    /// 获取向量的逆时针旋转角。
    /// </summary>
    /// <param name="value"></param>
    /// <returns>零向量返回0，否则返回 [0, <see cref="MathHelper.TwoPi"/>) 范围内的浮点值。按逆时针方向。</returns>
    public static float ToAngle(this Vector2 value) => value.Y switch
    {
        > 0f => (float)Math.Atan2(value.Y, value.X),
        0f => value.X switch
        {
            >= 0f => 0f, //零向量返回0，方向为x轴正方向返回0
            _ => MathHelper.Pi //方向为x轴负方向返回Pi
        },
        _ => MathHelper.TwoPi + (float)Math.Atan2(value.Y, value.X), //将Atan2方法返回的负值转换为正值
    };

    /// <summary>
    /// 获取向量的顺时针旋转角。
    /// </summary>
    /// <param name="value"></param>
    /// <returns>零向量返回0，否则返回 [0, <see cref="MathHelper.TwoPi"/>) 范围内的浮点值。按顺时针方向。</returns>
    public static float ToAngleClockwise(this Vector2 value)
    {
        float angle = value.ToAngle();

        return angle switch
        {
            0f => 0f,
            _ => MathHelper.TwoPi - angle
        };
    }

    /// <summary>
    /// 将向量放缩至指定模长。
    /// </summary>
    /// <param name="value"></param>
    /// <param name="length"></param>
    public static void ToCustomLength(this ref Vector2 value, float length) => value = ToCustomLength(value, length);

    /// <summary>
    /// 获取模为特定值的原向量同向向量。不改变原向量值。
    /// </summary>
    /// <param name="value"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static Vector2 ToCustomLength(Vector2 value, float length) => SafelyNormalize(value) * length;

    /// <summary>
    /// 获取两个向量角平分线的单位方向向量。
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static Vector2 UnitAngleBisector(Vector2 value1, Vector2 value2) => new PolarVector2((value1.ToAngle() + value2.ToAngle()) / 2);

    /// <summary>
    /// 计算一点到矩形的最短距离。
    /// </summary>
    /// <param name="point"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static Vector2 MinDistanceFromPointToRectangle(Vector2 point, Rectangle rect) => new(
        (float)Math.Clamp(point.X, rect.Left, rect.Right) - point.X,
        (float)Math.Clamp(point.Y, rect.Top, rect.Bottom) - point.Y);

    public static int Normalize(this float value) => value switch
    {
        > 0f => 1,
        0f => 0,
        _ => -1
    };
}