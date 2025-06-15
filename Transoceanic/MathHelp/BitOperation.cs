namespace Transoceanic.MathHelp;

public static class BitOperation
{
    public static bool GetBit(sbyte number, int bitPosition)
    {
        if (bitPosition >= 8)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(byte number, int bitPosition)
    {
        if (bitPosition >= 8)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(short number, int bitPosition)
    {
        if (bitPosition >= 16)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(ushort number, int bitPosition)
    {
        if (bitPosition >= 16)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(int number, int bitPosition)
    {
        if (bitPosition >= 32)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(uint number, int bitPosition)
    {
        if (bitPosition >= 32)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(long number, int bitPosition)
    {
        if (bitPosition >= 64)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(ulong number, int bitPosition)
    {
        if (bitPosition >= 64)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1ul << bitPosition)) != 0;
    }

    public static bool GetBit(Int128 number, int bitPosition)
    {
        if (bitPosition >= 128)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (1 << bitPosition)) != 0;
    }

    public static bool GetBit(UInt128 number, int bitPosition)
    {
        if (bitPosition >= 128)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & (UInt128.One << bitPosition)) != 0;
    }

    public static sbyte SetBit(sbyte number, int bitPosition, bool value)
    {
        if (bitPosition >= 8)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (sbyte)((number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition));
    }

    public static byte SetBit(byte number, int bitPosition, bool value)
    {
        if (bitPosition >= 8)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (byte)((number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition));
    }

    public static short SetBit(short number, int bitPosition, bool value)
    {
        if (bitPosition >= 16)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (short)((number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition));
    }

    public static ushort SetBit(ushort number, int bitPosition, bool value)
    {
        if (bitPosition >= 16)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (ushort)((number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition));
    }

    public static int SetBit(int number, int bitPosition, bool value)
    {
        if (bitPosition >= 32)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition);
    }

    public static uint SetBit(uint number, int bitPosition, bool value)
    {
        if (bitPosition >= 32)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & ~(1u << bitPosition)) | ((uint)value.ToInt() << bitPosition);
    }

    public static long SetBit(long number, int bitPosition, bool value)
    {
        if (bitPosition >= 64)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & ~(1 << bitPosition)) | ((long)value.ToInt() << bitPosition);
    }

    public static ulong SetBit(ulong number, int bitPosition, bool value)
    {
        if (bitPosition >= 64)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & ~(1ul << bitPosition)) | ((ulong)value.ToInt() << bitPosition);
    }

    public static Int128 SetBit(Int128 number, int bitPosition, bool value)
    {
        if (bitPosition >= 128)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & ~(1 << bitPosition)) | (value.ToInt() << bitPosition);
    }

    public static UInt128 SetBit(UInt128 number, int bitPosition, bool value)
    {
        if (bitPosition >= 128)
            throw new ArgumentOutOfRangeException(nameof(bitPosition));

        return (number & ~(UInt128.One << bitPosition)) | ((UInt128)value.ToInt() << bitPosition);
    }
}
