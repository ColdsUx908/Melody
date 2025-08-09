using System.Numerics;

namespace Transoceanic.Maths;

public static partial class TOMathHelper
{
    public static sbyte Min(sbyte value, params ReadOnlySpan<sbyte> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static byte Min(byte value, params ReadOnlySpan<byte> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static short Min(short value, params ReadOnlySpan<short> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static ushort Min(ushort value, params ReadOnlySpan<ushort> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static int Min(int value, params ReadOnlySpan<int> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static uint Min(uint value, params ReadOnlySpan<uint> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static long Min(long value, params ReadOnlySpan<long> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static ulong Min(ulong value, params ReadOnlySpan<ulong> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static nint Min(nint value, params ReadOnlySpan<nint> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static nuint Min(nuint value, params ReadOnlySpan<nuint> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static float Min(float value, params ReadOnlySpan<float> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static double Min(double value, params ReadOnlySpan<double> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static decimal Min(decimal value, params ReadOnlySpan<decimal> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Min(value, values[i]);
        return value;
    }

    public static sbyte Max(sbyte value, params ReadOnlySpan<sbyte> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static byte Max(byte value, params ReadOnlySpan<byte> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static short Max(short value, params ReadOnlySpan<short> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static ushort Max(ushort value, params ReadOnlySpan<ushort> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static int Max(int value, params ReadOnlySpan<int> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static uint Max(uint value, params ReadOnlySpan<uint> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static long Max(long value, params ReadOnlySpan<long> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static ulong Max(ulong value, params ReadOnlySpan<ulong> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static nint Max(nint value, params ReadOnlySpan<nint> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static nuint Max(nuint value, params ReadOnlySpan<nuint> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static float Max(float value, params ReadOnlySpan<float> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static double Max(double value, params ReadOnlySpan<double> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }

    public static decimal Max(decimal value, params ReadOnlySpan<decimal> values)
    {
        for (int i = 0; i < values.Length; i++)
            value = Math.Max(value, values[i]);
        return value;
    }
}
