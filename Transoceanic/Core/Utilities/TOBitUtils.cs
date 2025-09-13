namespace Transoceanic.Core.Utilities;

public static class TOBitUtils
{
    public static bool GetBit(sbyte number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 8);
        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(byte number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 8);
        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(short number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 16);
        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(ushort number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 16);
        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(int number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 32);
        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(uint number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 32);
        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(long number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 64);
        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(ulong number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 64);
        return (number & (1ul << bitPosition)) != 0;
    }

    public static bool GetBit(Int128 number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 128);
        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(UInt128 number, int bitPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 128);
        return (number & (UInt128.One << bitPosition)) != 0;
    }

    public static sbyte SetBit(sbyte number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 8);
        return (sbyte)((number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition));
    }

    public static byte SetBit(byte number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 8);
        return (byte)((number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition));
    }

    public static short SetBit(short number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 16);
        return (short)((number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition));
    }

    public static ushort SetBit(ushort number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 16);
        return (ushort)((number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition));
    }

    public static int SetBit(int number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 32);
        return (number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition);
    }

    public static uint SetBit(uint number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 32);
        return (number & ~(1u << bitPosition)) | ((uint)value.ToInt() << bitPosition);
    }

    public static long SetBit(long number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 64);
        return (number & ~(1 << bitPosition)) | ((long)value.ToInt() << bitPosition);
    }

    public static ulong SetBit(ulong number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 64);
        return (number & ~(1ul << bitPosition)) | ((ulong)value.ToInt() << bitPosition);
    }

    public static Int128 SetBit(Int128 number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 128);
        return (number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition);
    }

    public static UInt128 SetBit(UInt128 number, int bitPosition, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitPosition, 128);
        return (number & ~(UInt128.One << bitPosition)) | ((UInt128)value.ToInt() << bitPosition);
    }
}
