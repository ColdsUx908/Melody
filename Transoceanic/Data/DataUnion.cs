namespace Transoceanic.Data;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct DataUnion32
{
    [FieldOffset(0)] public float f;
    [FieldOffset(0)] public int i;
    [FieldOffset(0)] public fixed short shorts[2];
    [FieldOffset(0)] public fixed byte bytes[4];
    [FieldOffset(0)] public Bits32 bits;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct DataUnion64
{
    [FieldOffset(0)] public double d;
    [FieldOffset(0)] public long l;
    [FieldOffset(0)] public Bits64 bits;
    [FieldOffset(0)] public Vector2 v;
    [FieldOffset(0)] public Point p;
}
