using System;
using Microsoft.Xna.Framework;

namespace Transoceanic.MathHelp;

public static partial class TOMathHelper
{
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