namespace Transoceanic.Data;

public struct Bits32
{
    private int _value;

    public Bits32(int value) => _value = value;

    public bool this[int index]
    {
        readonly get => BitOperation.GetBit(_value, index);
        set => _value = BitOperation.SetBit(_value, index, value);
    }
}
