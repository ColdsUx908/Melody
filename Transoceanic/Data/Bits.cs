using Transoceanic.Core.Utilities;

namespace Transoceanic.Data;

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
    public override readonly bool Equals([NotNullWhen(true)] object obj) => obj is Bits32 other && Equals(other);
    public static bool operator ==(Bits32 left, Bits32 right) => left.Equals(right);
    public static bool operator !=(Bits32 left, Bits32 right) => !(left == right);
    public override readonly int GetHashCode() => _value.GetHashCode();

    public override readonly string ToString() => $"Bits32 {{ {Convert.ToString(_value, 2).PadLeft(32, '0')} }}";
}
