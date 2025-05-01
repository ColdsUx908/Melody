using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Transoceanic.Core.ExtraData.Maths;

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

    public static IEnumerable<T> GetEverySingleFlag<T>() where T : Enum
    {
        if (!typeof(T).IsDefined(typeof(FlagsAttribute), false))
            throw new ArgumentException("Type is not a flag enum.", nameof(T));
        return from T value in Enum.GetValues(typeof(T)).Cast<T>()
               let valueToInteger = Convert.ToUInt64(value)
               where valueToInteger != 0 && (valueToInteger & (valueToInteger - 1)) == 0
               select value;
    }

    public static IEnumerable<string> GetEverySingleFlagName<T>() where T : Enum
    {
        if (!typeof(T).IsDefined(typeof(FlagsAttribute), false))
            throw new ArgumentException("Type is not a flag enum.", nameof(T));
        return from T value in Enum.GetValues(typeof(T)).Cast<T>()
               let valueToInteger = Convert.ToUInt64(value)
               where valueToInteger != 0 && (valueToInteger & (valueToInteger - 1)) == 0
               select value.ToString();
    }

    public static IEnumerable<(T flag, string name)> GetEverySingleFlagAndName<T>() where T : Enum
    {
        if (!typeof(T).IsDefined(typeof(FlagsAttribute), false))
            throw new ArgumentException("Type is not a flag enum.", nameof(T));
        return from T value in Enum.GetValues(typeof(T)).Cast<T>()
               let valueToInteger = Convert.ToUInt64(value)
               where valueToInteger != 0 && (valueToInteger & (valueToInteger - 1)) == 0
               select (value, value.ToString());
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
}