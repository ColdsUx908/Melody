using Transoceanic.Core.Utilities;

namespace Transoceanic.Data;

public struct Bits32
{
    private int _value;

    public Bits32(int value) => _value = value;

    public bool this[int index]
    {
        readonly get => TOBitUtils.GetBit(_value, index);
        set => _value = TOBitUtils.SetBit(_value, index, value);
    }
}
