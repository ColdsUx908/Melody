

namespace Transoceanic.Data;

public struct SmoothInt : IEquatable<SmoothInt>
{
    public int _lastOn;
    public int _lastOff;

    public readonly int GetValue(int actual, int max) =>
        _lastOn > _lastOff ? Math.Clamp(actual - _lastOn, 0, max)
        : Math.Clamp(max - actual + _lastOff, 0, max);

    public readonly bool Equals(SmoothInt other) => _lastOn == other._lastOn && _lastOff == other._lastOff;

    public override readonly bool Equals([NotNullWhen(true)] object obj) => obj is SmoothInt other && Equals(other);

    public static bool operator ==(SmoothInt left, SmoothInt right) => left.Equals(right);

    public static bool operator !=(SmoothInt left, SmoothInt right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(_lastOn, _lastOff);
}
