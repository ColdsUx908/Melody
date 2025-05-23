using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Transoceanic.Core.ExtraMathData;

namespace Transoceanic.Core.MathHelp;

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
    /// 获取两个布尔值的逻辑状态，用于控制语句。
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <remarks>警告：须确保参数的传递顺序符合需求。</remarks>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TwoBooleanStatus GetTwoBooleanStatus(bool A, bool B) => A ? B ? TwoBooleanStatus.Both : TwoBooleanStatus.ATrue : B ? TwoBooleanStatus.BTrue : TwoBooleanStatus.Neither;

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
        ClampMap(oldMin, oldMax, newMin, newMax, Math.Clamp(value, oldMin, oldMax));
}