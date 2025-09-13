using System.Numerics;

namespace Transoceanic.Core.Utilities;

public static class TOMathHelper
{
    public const float PiOver8 = MathHelper.Pi / 8f;

    public const float PiOver3 = MathHelper.Pi / 3f;

    public const float PiOver6 = MathHelper.Pi / 6f;

    public const float PiOver12 = MathHelper.Pi / 12f;

    public const float PiOver30 = MathHelper.Pi / 30f;

    /// <summary>
    /// 将像素每帧转换为英里每小时的转换因子。
    /// <br/>计算公式为：<c>C = 60f / 8f * 0.681818f</c>
    /// （一像素为 <c>1/8</c> 英尺）
    /// </summary>
    public const float MphsPerPpt = 5.1136364f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pixptick_To_Mph(float value) => value * MphsPerPpt;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Mph_To_Pixptick(float value) => value / MphsPerPpt;

    public static float NormalizeAngle(float angle)
    {
        float temp = angle % MathHelper.TwoPi;
        return temp switch
        {
            < 0 => temp + MathHelper.TwoPi,
            _ => temp
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LagrangeInterpolation(float ratio) => ratio * (2f - ratio);

    public static T Min<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
    {
        foreach (T temp in values)
        {
            if (temp < value)
                value = temp;
        }
        return value;
    }

    public static T Max<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
    {
        foreach (T temp in values)
        {
            if (temp > value)
                value = temp;
        }
        return value;
    }

    public static (T min, T max) MinMax<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
    {
        T min = value, max = value;
        foreach (T temp in values)
        {
            if (temp < min)
                min = temp;
            if (temp > max)
                max = temp;
        }
        return (min, max);
    }

    /// <summary>
    /// 生成一个形如 <c>y = A * sin(</c>ω<c>t + </c>φ<c>)</c> 的正弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max">A。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值。默认为 <see langword="false"/>。</param>
    public static float GetTimeSin(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false) => (MathF.Sin(TOWorld.GeneralSeconds * omega + primary) + unsigned.ToInt()) * max;

    /// <summary>
    /// 生成一个形如 <c>y = A * cos(</c>ω<c>t + </c>φ<c>)</c> 的余弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max">A。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值。默认为 <see langword="false"/>。</param>
    public static float GetTimeCos(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false) => (MathF.Cos(TOWorld.GeneralSeconds * omega + primary) + unsigned.ToInt()) * max;

    /// <summary>
    /// 生成形如 <c>(sin, cos) = (A * sin(</c>ω<c>t + </c>φ<c>, A * cos(</c>ω<c>t + </c>φ<c>))</c> 的正余弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max">A。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值。默认为 <see langword="false"/>。</param>
    public static (float sin, float cos) GetTimeSinCos(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false)
    {
        (float sin1, float cos1) = MathF.SinCos(TOWorld.GeneralSeconds * omega + primary);
        return ((sin1 + unsigned.ToInt()) * max, (cos1 + unsigned.ToInt()) * max);
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

    public static (int integer, float fractional) SplitFloat(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a finite number.");
        int integerPart = (int)value;
        return (integerPart, value - integerPart);
    }

    public static float Map(float oldMin, float oldMax, float newMin, float newMax, float value, bool clamp = false)
    {
        if (oldMin > oldMax)
            throw new ArgumentOutOfRangeException($"{nameof(oldMin)}, {nameof(oldMax)}", "oldMin must be less than or equal to oldMax.");
        if (newMin > newMax)
            throw new ArgumentOutOfRangeException($"{nameof(newMin)}, {nameof(newMax)}", "newMin must be less than or equal to newMax.");
        if (clamp)
            value = Math.Clamp(value, oldMin, oldMax);
        return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }

    public static float MapReverse(float oldMin, float oldMax, float newMin, float newMax, float value, bool clamp = false)
    {
        if (oldMin > oldMax)
            throw new ArgumentOutOfRangeException($"{nameof(oldMin)}, {nameof(oldMax)}", "oldMin must be less than or equal to oldMax.");
        if (newMin > newMax)
            throw new ArgumentOutOfRangeException($"{nameof(newMin)}, {nameof(newMax)}", "newMin must be less than or equal to newMax.");
        if (clamp)
            value = Math.Clamp(value, oldMin, oldMax);
        return newMax - (value - oldMin) / (oldMax - oldMin) * (newMax - newMin);
    }
}