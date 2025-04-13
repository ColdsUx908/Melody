using System;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Transoceanic.Core;

namespace Transoceanic.Data;

/// <summary>
/// 两个布尔值的逻辑状态。
/// <br>使用 <see cref="TOMathHelper.GetTwoBooleanStatus(bool, bool)"/> 方法来获取值。</br>
/// </summary>
public enum TwoBooleanStatus : byte
{
    /// <summary>
    /// 二者均为false。
    /// </summary>
    Neither = 0,
    /// <summary>
    /// A为true，B为false。
    /// </summary>
    ATrue = 1,
    /// <summary>
    /// A为false，B为true。
    /// </summary>
    BTrue = 2,
    /// <summary>
    /// 二者均为true。
    /// </summary>
    Both = 3
}

#region 极坐标向量
/// <summary>
/// 二维极坐标向量。
/// </summary>
public struct PolarVector2
{
    public static class ExceptionCode
    {
        public const byte RadiusNegative = 0x01;
    }

    /// <summary>
    /// 模长。
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotFiniteNumberException"></exception>
    public float Radius
    {
        get;
        set
        {
            try
            {
                switch (value)
                {
                    case > 0f:
                        field = value;
                        break;
                    case 0f:
                        field = 0f;
                        Angle = 0f;
                        break;
                    case < 0f:
                        throw new ArgumentOutOfRangeException(nameof(value), value, "Negative");
                    default:
                        throw new NotFiniteNumberException("NotFinite");
                }
            }
            catch (Exception e)
            {
                if (TOMain.DEBUG)
                    TOLocalizationUtils.ChatLocalizedTextWith(TOMain.DebugPrefix + "PolarVector2.RadiusInvalid", textColor: TOMain.TODebugErrorColor, value, e.Message);
                else
                    throw; //不处理异常
            }
        }
    }

    /// <summary>
    /// 角度。
    /// </summary>
    public float Angle
    {
        get;
        set
        {
            float temp = value % MathHelper.TwoPi;
            field = temp switch
            {
                < 0 => temp + MathHelper.TwoPi,
                _ => temp
            };
        }
    }

    /// <summary>
    /// 角度（角度制）值。
    /// </summary>
    public float AngleInDegree
    {
        get => MathHelper.ToDegrees(Angle);
        set => Angle = MathHelper.ToRadians(value);
    }

    /// <summary>
    /// 角度除以Pi的值。
    /// </summary>
    public float AngleOverPi
    {
        get => Angle / MathHelper.Pi;
        set => Angle = value * MathHelper.Pi;
    }

    /// <summary>
    /// 角度除以2Pi的值。
    /// </summary>
    public float AngleOverPeriod
    {
        get => Angle / MathHelper.TwoPi;
        set => Angle = value * MathHelper.TwoPi;
    }

    /// <summary>
    /// 主要构造函数。
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <exception cref="ArgumentException"></exception>
    public PolarVector2(float radius, float angle)
    {
        Radius = radius;
        Angle = angle;
    }

    public PolarVector2(float angle) : this(1f, angle) { }

    public PolarVector2() : this(0f, 0f) { }

    public PolarVector2(Vector2 value)
    {
        Radius = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y);
        Angle = value.ToAngle();
    }

    public static readonly PolarVector2 Zero = new(0f, 0f);

    /* 这个隐式转换会导致Vector2和PolarVector2进行运算时的方法调用二义性，故注释掉
    /// <summary>
    /// 将直角向量转换为极坐标向量。
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator PolarVector2(Vector2 value) => new(value);
    */

    /// <summary>
    /// 将极坐标向量转换为直角向量。
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Vector2(PolarVector2 value) => new(value.Radius * MathF.Cos(value.Angle), value.Radius * MathF.Sin(value.Angle));

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

    /// <summary>
    /// 极坐标向量取反。
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static PolarVector2 operator -(PolarVector2 a) => new(a.Radius, a.Angle + MathHelper.Pi);

    /// <summary>
    /// 极坐标向量数乘。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="NotFiniteNumberException"></exception>
    public static PolarVector2 operator *(PolarVector2 a, float b) => b switch
    {
        > 0f => new PolarVector2(a.Radius * b, a.Angle),
        0f => Zero,
        < 0f => new PolarVector2(a.Radius * -b, a.Angle + MathHelper.Pi),
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

    public override readonly bool Equals(object obj) => obj is PolarVector2 vector && Radius == vector.Radius && Angle == vector.Angle;

    public override readonly int GetHashCode() => HashCode.Combine(Radius, Angle);

    public static bool operator ==(PolarVector2 left, PolarVector2 right) => left.Equals(right);

    public static bool operator !=(PolarVector2 left, PolarVector2 right) => !(left == right);

    /// <summary>
    /// 获取字符串表示形式。
    /// </summary>
    /// <returns></returns>
    public override readonly string ToString() => $"({Radius}, {Angle})";

    /// <summary>
    /// 极坐标向量点乘。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Dot(PolarVector2 a, PolarVector2 b) => a.Radius * b.Radius * MathF.Cos(IncludedAngle(a, b));
}
#endregion
