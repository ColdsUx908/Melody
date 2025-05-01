using System;
using Microsoft.Xna.Framework;
using Terraria;
using Transoceanic.Core.Localization;
using Transoceanic.Core.MathHelp;

namespace Transoceanic.Core.ExtraData.Maths;

/// <summary>
/// 二维极坐标向量。
/// </summary>
public readonly struct PolarVector2 : IEquatable<PolarVector2>
{
    /// <summary>
    /// 模长。
    /// <br/>非负。
    /// </summary>
    public float Radius { get; }

    /// <summary>
    /// 角度。
    /// <br/>范围为 [0, 2π)。
    /// </summary>
    public float Angle { get; }

    /// <summary>
    /// 角度（角度制）值。
    /// </summary>
    public float AngleInDegree => MathHelper.ToDegrees(Angle);

    /// <summary>
    /// 角度除以Pi的值。
    /// </summary>
    public float AngleOverPi => Angle / MathHelper.Pi;

    /// <summary>
    /// 角度除以2Pi的值。
    /// </summary>
    public float AngleOverPeriod => Angle / MathHelper.TwoPi;

    /// <summary>
    /// 主要构造函数。
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <exception cref="ArgumentException"></exception>
    public PolarVector2(float radius, float angle)
    {
        try
        {
            Radius = radius switch
            {
                >= 0f => radius,
                < 0f => throw new ArgumentOutOfRangeException(nameof(radius), radius, "Radius negative"),
                _ => throw new NotFiniteNumberException("Radius not finite.")
            };
            float temp = angle % MathHelper.TwoPi;
            Angle = temp switch
            {
                _ when Radius == 0f => 0f, //极径为零时设置角度为零
                < 0 => temp + MathHelper.TwoPi,
                _ => temp
            };
        }
        catch (Exception e)
        {
            TOLocalizationUtils.ChatDebugErrorMessage("PolarVector2", Main.LocalPlayer, e.Message);
        }
    }

    public PolarVector2(float angle) : this(1f, angle) { }

    public PolarVector2() : this(0f, 0f) { }

    public PolarVector2(Vector2 value) : this(value.Length(), value.ToAngle()) { }

    /// <summary>
    /// 拷贝构造函数。
    /// <br/>不存在异常风险。
    /// </summary>
    /// <param name="original"></param>
    public PolarVector2(PolarVector2 original)
    {
        Radius = original.Radius;
        Angle = original.Angle;
    }

    /// <summary>
    /// 解构函数。
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    public void Deconstruct(out float radius, out float angle) => (radius, angle) = (Radius, Angle);

    /// <summary>
    /// 将直角向量转换为极坐标向量。
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator PolarVector2(Vector2 value) => new(value);

    /// <summary>
    /// 将极坐标向量转换为直角向量。
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Vector2(PolarVector2 value)
    {
        (float sin, float cos) = MathF.SinCos(value.Angle);
        return new(value.Radius * cos, value.Radius * sin);
    }

    public static PolarVector2 operator +(PolarVector2 a) => a; //没什么用，只是为了对称

    /// <summary>
    /// 极坐标向量取反。
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static PolarVector2 operator -(PolarVector2 a) => new(a.Radius, a.Angle + MathHelper.Pi);

    /*
    /// <summary>
    /// 极坐标向量加法，基于直角向量实现。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static PolarVector2 operator +(PolarVector2 a, PolarVector2 b) => new((Vector2)a + (Vector2)b);

    /// <summary>
    /// 极坐标向量减法，基于直角向量实现。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static PolarVector2 operator -(PolarVector2 a, PolarVector2 b) => new((Vector2)a - (Vector2)b);
    */

    public static PolarVector2 operator +(PolarVector2 a, PolarVector2 b)
    {
        //极坐标加法公式：
        //新极径 r = sqrt(r1² + r2² + 2r1r2cos(θ2-θ1))
        //新角度 θ = θ1 + arctan(r2sin(θ2-θ1) / (r1 + r2cos(θ2-θ1)))

        (float sinDelta, float cosDelta) = MathF.SinCos(b.Angle - a.Angle);
        float radius = MathF.Sqrt(a.Radius * a.Radius + b.Radius * b.Radius + 2 * a.Radius * b.Radius * cosDelta);

        float numerator = b.Radius * sinDelta;
        float denominator = a.Radius + b.Radius * cosDelta;
        float angleOffset = denominator != 0 ? MathF.Atan2(numerator, denominator) : 0f;
        float angle = a.Angle + angleOffset;

        return new(radius, angle);
    }

    public static PolarVector2 operator -(PolarVector2 a, PolarVector2 b) => a + -b;

    /// <summary>
    /// 极坐标向量数乘。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="NotFiniteNumberException"></exception>
    public static PolarVector2 operator *(PolarVector2 a, float b) => b switch
    {
        > 0f => new(a.Radius * b, a.Angle),
        0f => Zero,
        < 0f => new(a.Radius * -b, a.Angle + MathHelper.Pi),
        _ => throw new NotFiniteNumberException(b),
    };

    /// <summary>
    /// 极坐标向量数除。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="NotFiniteNumberException"></exception>
    /// <exception cref="DivideByZeroException"></exception>
    public static PolarVector2 operator /(PolarVector2 a, float b) => b switch
    {
        0f => throw new DivideByZeroException(),
        _ => a * (1 / b)
    };

    public PolarVector2 RotatedBy(float offset) => new(Radius, Angle + offset);

    /// <summary>
    /// 获取极坐标向量夹角。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float IncludedAngle(PolarVector2 a, PolarVector2 b)
    {
        //两个向量中有任意一个为零向量时，返回0。
        if (a == Zero || b == Zero)
            return 0f;

        float angle = Math.Abs(a.Angle - b.Angle);
        return angle switch
        {
            MathHelper.Pi => 0f,
            > MathHelper.Pi => MathHelper.TwoPi - angle,
            _ => angle
        };
    }

    public override bool Equals(object obj) => obj is PolarVector2 other && Equals(other);

    public bool Equals(PolarVector2 other) => Radius == other.Radius && Angle == other.Angle;

    public override int GetHashCode() => HashCode.Combine(Radius, Angle);

    public static bool operator ==(PolarVector2 left, PolarVector2 right) => left.Equals(right);

    public static bool operator !=(PolarVector2 left, PolarVector2 right) => !(left == right);

    /// <summary>
    /// 获取字符串表示形式。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"({Radius}, {Angle})";

    /// <summary>
    /// 极坐标向量点乘。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Dot(PolarVector2 a, PolarVector2 b) => a.Radius * b.Radius * MathF.Cos(IncludedAngle(a, b));

    #region 预定义量
    public static readonly PolarVector2 Zero = new(0f, 0f);

    public static readonly PolarVector2 UnitX = new(0f);

    public static readonly PolarVector2 UnitY = new(MathHelper.PiOver2);

    /// <summary>
    /// 钟点方向单位向量。
    /// <br/>索引直接对应钟点数，不要使用 <c>Index - 1</c>。
    /// <para/>示例：
    /// <code>
    /// PolarVector2 ClockOne = PolarVector2.UnitClocks[1] * 5f; //一点钟方向，长度为5
    /// 
    /// //遍历十二个方向
    /// foreach (PolarVector2 vector in PolarVector.UnitClocks)
    /// {
    ///     //代码
    /// }
    /// </code>
    /// </summary>
    public static readonly PolarVector2[] UnitClocks =
        [
        //0点钟方向（同12点钟方向）
        -UnitY,

        //1点钟方向 (5π/3)
        new(TOMathHelper.PiOver3 * 5f), 
        //2点钟方向 (11π/6)
        new(TOMathHelper.PiOver6 * 11f),
        //3点钟方向 (0)
        UnitX,
        //4点钟方向 (π/6)
        new(TOMathHelper.PiOver6),
        //5点钟方向 (π/3)
        new(TOMathHelper.PiOver3),
        //6点钟方向 (π/2)
        UnitY,
        //7点钟方向 (2π/3)
        new(TOMathHelper.PiOver3 * 2f),
        //8点钟方向 (5π/6)
        new(TOMathHelper.PiOver6 * 5f),
        //9点钟方向 (π)
        -UnitX,
        //10点钟方向 (7π/6)
        new(TOMathHelper.PiOver6 * 7f),
        //11点钟方向 (4π/3)
        new(TOMathHelper.PiOver3 * 4f),
        ];
    #endregion
}
