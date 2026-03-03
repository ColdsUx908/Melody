namespace Transoceanic.DataStructures;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Union32
{
    static Union32()
    {
        if (Unsafe.SizeOf<BitArray32>() != 4)
            throw new InvalidOperationException("BitArray32 must be 4 bytes");
    }

    [FieldOffset(0)] public float f;
    [FieldOffset(0)] public int i;
    [FieldOffset(0)] public fixed short shorts[2];
    [FieldOffset(0)] public fixed byte bytes[4];
    [FieldOffset(0)] public BitArray32 bits;
    [FieldOffset(0)] public Color color;

    public Union32(float f) => this.f = f;

    public T GetValueAs<T>() where T : unmanaged
    {
        if (sizeof(T) != 4)
            throw new InvalidOperationException($"Type {typeof(T)} must be 4 bytes in size.");

        return Unsafe.As<float, T>(ref f);
    }
    public void SetValueAs<T>(T value) where T : unmanaged
    {
        if (sizeof(T) != 4)
            throw new InvalidOperationException($"Type {typeof(T)} must be 4 bytes in size.");

        f = Unsafe.As<T, float>(ref value);
    }

    public static explicit operator Union32(float f) => new(f);
    public static explicit operator Union32(int i) => new() { i = i };
    public static explicit operator Union32(BitArray32 bits) => new() { bits = bits };
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Union64
{
    static Union64()
    {
        if (Unsafe.SizeOf<BitArray64>() != 8)
            throw new InvalidOperationException("BitArray64 must be 8 bytes");
    }

    [FieldOffset(0)] public double d;
    [FieldOffset(0)] public long l;
    [FieldOffset(0)] public BitArray64 bits;
    [FieldOffset(0)] public Vector2 v;
    [FieldOffset(0)] public Point p;

    public Union64(double d) => this.d = d;

    public T GetValueAs<T>() where T : unmanaged
    {
        if (sizeof(T) != 8)
            throw new InvalidOperationException($"Type {typeof(T)} must be 8 bytes in size.");

        return Unsafe.As<double, T>(ref d);
    }
    public void SetValueAs<T>(T value) where T : unmanaged
    {
        if (sizeof(T) != 8)
            throw new InvalidOperationException($"Type {typeof(T)} must be 8 bytes in size.");

        d = Unsafe.As<T, double>(ref value);
    }

    public static explicit operator Union64(double d) => new(d);
    public static explicit operator Union64(long l) => new() { l = l };
    public static explicit operator Union64(BitArray64 bits) => new() { bits = bits };
}
