namespace Transoceanic.MathHelp;

public static partial class TOMathHelper
{
    /// <summary>
    /// 生成一个形如 <c>y = A * sin(</c>ω<c>t + </c>φ<c>)</c> 的正弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max">A。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值。默认为 <see langword="false"/>。</param>
    public static float GetTimeSin(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false) => (MathF.Sin(TOMain.GeneralSeconds * omega + primary) + unsigned.ToInt()) * max;

    /// <summary>
    /// 生成一个形如 <c>y = A * cos(</c>ω<c>t + </c>φ<c>)</c> 的余弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max">A。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值。默认为 <see langword="false"/>。</param>
    public static float GetTimeCos(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false) => (MathF.Cos(TOMain.GeneralSeconds * omega + primary) + unsigned.ToInt()) * max;

    /// <summary>
    /// 生成形如 <c>(sin, cos) = (A * sin(</c>ω<c>t + </c>φ<c>, A * cos(</c>ω<c>t + </c>φ<c>))</c> 的正余弦波。其中 <c>t</c> 随游戏内时间变化，单位是秒。
    /// </summary>
    /// <param name="max">A。默认为1。</param>
    /// <param name="omega">ω。默认为1。</param>
    /// <param name="primary">φ。默认为0。</param>
    /// <param name="unsigned">是否将结果转换为非负值。默认为 <see langword="false"/>。</param>
    public static (float sin, float cos) GetTimeSinCos(float max = 1f, float omega = 1f, float primary = 0f, bool unsigned = false)
    {
        (float sin1, float cos1) = MathF.SinCos(TOMain.GeneralSeconds * omega + primary);
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

    /// <summary>
    /// 计算一点到矩形的最短距离。
    /// </summary>
    /// <param name="point"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static Vector2 MinDistanceFromPointToRectangle(Vector2 point, Rectangle rect) => new(
        Math.Clamp(point.X, rect.Left, rect.Right) - point.X,
        Math.Clamp(point.Y, rect.Top, rect.Bottom) - point.Y);

    public static float Map(float oldMin, float oldMax, float newMin, float newMax, float value)
    {
        if (oldMin > oldMax)
            throw new ArgumentOutOfRangeException($"{nameof(oldMin)}, {nameof(oldMax)}", "oldMin must be less than or equal to oldMax.");
        if (newMin > newMax)
            throw new ArgumentOutOfRangeException($"{nameof(newMin)}, {nameof(newMax)}", "newMin must be less than or equal to newMax.");

        return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }

    public static float ClampMap(float oldMin, float oldMax, float newMin, float newMax, float value) =>
        Map(oldMin, oldMax, newMin, newMax, Math.Clamp(value, oldMin, oldMax));
}