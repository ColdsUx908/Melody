﻿namespace Transoceanic.Data;

public struct GuaranteedBoolean : IEquatable<GuaranteedBoolean>
{
    private int _value;

    public GuaranteedBoolean(int value) => _value = value;

    public bool Value
    {
        readonly get => _value > 0;
        set
        {
            if (value)
                _value = Math.Clamp(_value + 2, 0, 2);
            else
                _value = Math.Clamp(_value - 1, 0, 2);
        }
    }

    public static implicit operator bool(GuaranteedBoolean guaranteedBoolean) => guaranteedBoolean.Value;

    public override readonly int GetHashCode() => _value.GetHashCode();

    public readonly bool Equals(GuaranteedBoolean other) => _value == other._value;

    public override readonly bool Equals(object obj) => obj is GuaranteedBoolean other && Equals(other);

    public static bool operator ==(GuaranteedBoolean left, GuaranteedBoolean right) => left.Equals(right);

    public static bool operator !=(GuaranteedBoolean left, GuaranteedBoolean right) => !(left == right);
}
