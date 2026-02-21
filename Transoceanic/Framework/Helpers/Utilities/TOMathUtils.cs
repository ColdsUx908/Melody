using System.Numerics;

namespace Transoceanic.Framework.Helpers;

public static class TOMathUtils
{
    public const float PiOver3 = MathHelper.Pi / 3f;
    public const float PiOver5 = MathHelper.Pi / 5f;
    public const float PiOver6 = MathHelper.Pi / 6f;
    public const float PiOver8 = MathHelper.Pi / 8f;
    public const float PiOver10 = MathHelper.Pi / 10f;
    public const float PiOver12 = MathHelper.Pi / 12f;
    public const float PiOver16 = MathHelper.Pi / 16f;
    public const float PiOver24 = MathHelper.Pi / 24f;
    public const float PiOver30 = MathHelper.Pi / 30f;

    public static float NormalizeAngle(float angle)
    {
        float temp = angle % MathHelper.TwoPi;
        return temp switch
        {
            < 0 => temp + MathHelper.TwoPi,
            _ => temp
        };
    }

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

    public static (T Min, T Max) MinMax<T>(T value, params ReadOnlySpan<T> values) where T : INumber<T>
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
    /// 必要时交换 <paramref name="left"/> 和 <paramref name="right"/> 的值，确保 <paramref name="left"/> 不大于 <paramref name="right"/>。
    /// </summary>
    public static void NormalizeMinMax<T>(ref T left, ref T right) where T : INumber<T>
    {
        if (left > right)
            Utils.Swap(ref left, ref right);
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

    #region 单位转换
    /// <summary>
    /// 将像素每帧转换为英里每小时的转换因子。
    /// <br/>计算公式为：<c>C = 60f / 8f * 0.681818f</c>
    /// （一像素为 <c>1/8</c> 英尺）。
    /// </summary>
    public const float MphsPerPpt = 5.1136364f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Pixptick_To_Mph(float value) => value * MphsPerPpt;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Mph_To_Pixptick(float value) => value / MphsPerPpt;
    #endregion 单位转换

    #region 插值
    /// <summary>
    /// 二次方缓入。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float QuadraticEaseIn(float ratio) => ratio * ratio;

    /// <summary>
    /// 二次方缓出。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float QuadraticEaseOut(float ratio) => ratio * (2f - ratio);

    /// <summary>
    /// 二次方缓入缓出。
    /// </summary>
    public static float QuadraticEaseInOut(float ratio) => ratio < 0.5f ? 2f * ratio * ratio : -2f * ratio * ratio + 4f * ratio - 1f;

    /// <summary>
    /// 三次方缓入。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CubicEaseIn(float ratio) => ratio * ratio * ratio;

    /// <summary>
    /// 三次方缓出。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CubicEaseOut(float ratio)
    {
        float inv = 1f - ratio;
        return 1f - inv * inv * inv;
    }

    /// <summary>
    /// 三次方缓入缓出。
    /// </summary>
    public static float CubicEaseInOut(float ratio) =>
        ratio < 0.5f ? 4f * ratio * ratio * ratio : 4f * ratio * ratio * ratio - 12f * ratio * ratio + 12f * ratio - 3f;

    /// <summary>
    /// 指数缓入。
    /// </summary>
    public static float ExponentialEaseIn(float ratio, float exponent) => MathF.Pow(ratio, exponent);

    /// <summary>
    /// 指数缓出。
    /// </summary>
    public static float ExponentialEaseOut(float ratio, float exponent) => 1f - MathF.Pow(1f - ratio, exponent);

    /// <summary>
    /// 指数缓入缓出。
    /// </summary>
    public static float ExponentialEaseInOut(float ratio, float exponent) =>
        ratio < 0.5f ? 0.5f * MathF.Pow(2f * ratio, exponent) : 1f - 0.5f * MathF.Pow(2f * (1f - ratio), exponent);

    /// <summary>
    /// 正弦缓入。
    /// <br/>计算公式为：<c>y = 1 - cos(π/2 * x)</c>
    /// </summary>
    public static float SineEaseIn(float ratio) => 1f - MathF.Cos(ratio * MathHelper.PiOver2);

    /// <summary>
    /// 正弦缓出。
    /// <br/>计算公式为：<c>y = sin(π/2 * x)</c>
    /// </summary>
    public static float SineEaseOut(float ratio) => MathF.Sin(ratio * MathHelper.PiOver2);

    /// <summary>
    /// 正弦缓入缓出。
    /// <br/>计算公式为：<c>y = 0.5 - 0.5 * cos(π * x)</c>
    /// </summary>
    public static float SineEaseInOut(float ratio) => 0.5f - 0.5f * MathF.Cos(ratio * MathHelper.Pi);

    /// <summary>
    /// 对数缓入。
    /// <br/>计算公式为：<c>y = 1 - ln((e - 1) * (1 - x) + 1)</c>
    /// </summary>
    public static float LogarithmicEaseIn(float ratio) => 1f - MathF.Log((MathF.E - 1f) * (1f - ratio) + 1f);

    /// <summary>
    /// 对数缓出。
    /// <br/>计算公式为：<c>y = ln((e - 1) * x + 1)</c>
    /// </summary>
    public static float LogarithmicEaseOut(float ratio) => MathF.Log((MathF.E - 1f) * ratio + 1f);

    /// <summary>
    /// 对数缓入缓出。
    /// <br/>前半段使用对数缓入，后半段使用对数缓出。
    /// </summary>
    public static float LogarithmicEaseInOut(float ratio) =>
        ratio < 0.5f ? 0.5f * (1f - MathF.Log((MathF.E - 1f) * (1f - 2f * ratio) + 1f)) : 0.5f * MathF.Log((MathF.E - 1f) * (2f * ratio - 1f) + 1f) + 0.5f;

    /// <summary>
    /// 更平滑插值，使用五次方。
    /// </summary>
    public static float SmootherStep(float ratio) => ratio * ratio * ratio * (ratio * (ratio * 6f - 15f) + 10f);

    /// <inheritdoc cref="SmootherStep(float)"/>
    public static float SmootherStep(float from, float to, float amount) => from + (to - from) * SmootherStep(amount);
    #endregion 插值

    #region 时间波动函数
    /// <summary>
    /// 生成一个形如 <c>y = A * sin(</c>ω<c>t + </c>φ<c>)</c> 的正弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max">A。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值。默认为 <see langword="false"/>。</param>
    public static float GetTimeSin(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false) => (MathF.Sin(TOSharedData.GeneralSeconds * omega + primary) + unsigned.ToInt()) * max;

    /// <summary>
    /// 生成一个形如 <c>y = A * cos(</c>ω<c>t + </c>φ<c>)</c> 的余弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max">A。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值。默认为 <see langword="false"/>。</param>
    public static float GetTimeCos(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false) => (MathF.Cos(TOSharedData.GeneralSeconds * omega + primary) + unsigned.ToInt()) * max;

    /// <summary>
    /// 生成形如 <c>(sin, cos) = (A * sin(</c>ω<c>t + </c>φ<c>, A * cos(</c>ω<c>t + </c>φ<c>))</c> 的正余弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max"><c>A</c>。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值（即加上 <c>A</c> / 2）。默认为 <see langword="false"/>。</param>
    public static (float Sin, float Cos) GetTimeSinCos(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false)
    {
        (float sin1, float cos1) = MathF.SinCos(TOSharedData.GeneralSeconds * omega + primary);
        return ((sin1 + unsigned.ToInt()) * max, (cos1 + unsigned.ToInt()) * max);
    }
    #endregion 时间波动函数
}