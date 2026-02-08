namespace Transoceanic.DataStructures;

public struct Bits32 : IEquatable<Bits32>
{
    private int _value;

    public Bits32(int value) => _value = value;
    public Bits32() : this(0) { }

    public bool this[int index]
    {
        readonly get => TOBitUtils.GetBit(_value, index);
        set => _value = TOBitUtils.SetBit(_value, index, value);
    }

    public readonly bool Equals(Bits32 other) => _value == other._value;
    public override readonly bool Equals(object obj) => obj is Bits32 other && Equals(other);
    public override readonly int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(Bits32 left, Bits32 right) => left.Equals(right);
    public static bool operator !=(Bits32 left, Bits32 right) => !(left == right);

    public override readonly string ToString() => $"Bits32 {{ {Convert.ToString(_value, 2).PadLeft(32, '0')} }}";
}

public struct Bits128 : IEquatable<Bits128>
{
    private Int128 _value;

    public Bits128(Int128 value) => _value = value;
    public Bits128(ulong upper, ulong lower) : this(new Int128(upper, lower)) { }
    public Bits128() : this(Int128.Zero) { }

    public bool this[int index]
    {
        readonly get => TOBitUtils.GetBit(_value, index);
        set => _value = TOBitUtils.SetBit(_value, index, value);
    }

    public readonly bool Equals(Bits128 other) => _value == other._value;
    public override readonly bool Equals(object obj) => obj is Bits32 other && Equals(other);
    public override readonly int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(Bits128 left, Bits128 right) => left.Equals(right);
    public static bool operator !=(Bits128 left, Bits128 right) => !(left == right);

    public override readonly string ToString() => $"Bits128 {{ {Convert.ToString((long)(_value >> 64), 2).PadLeft(64, '0')} {Convert.ToString((long)_value, 2).PadLeft(64, '0')} }}";
}
