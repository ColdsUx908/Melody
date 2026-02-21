namespace Transoceanic.DataStructures;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Union32
{
    [FieldOffset(0)] public float f;
    [FieldOffset(0)] public int i;
    [FieldOffset(0)] public fixed short shorts[2];
    [FieldOffset(0)] public fixed byte bytes[4];
    [FieldOffset(0)] public Bits32 bits;
    [FieldOffset(0)] public Color color;

    public Union32(float f) => this.f = f;

    public static explicit operator Union32(float f) => new(f);
    public static explicit operator Union32(int i) => new() { i = i };
    public static explicit operator Union32(Bits32 bits) => new() { bits = bits };
}

[StructLayout(LayoutKind.Explicit)]
public struct Union64
{
    [FieldOffset(0)] public double d;
    [FieldOffset(0)] public long l;
    [FieldOffset(0)] public Bits64 bits;
    [FieldOffset(0)] public Vector2 v;
    [FieldOffset(0)] public Point p;

    public Union64(double d) => this.d = d;

    public static explicit operator Union64(double d) => new(d);
    public static explicit operator Union64(long l) => new() { l = l };
    public static explicit operator Union64(Bits64 bits) => new() { bits = bits };
}
