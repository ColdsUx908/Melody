namespace CalamityAnomalies.GlobalInstances;

public sealed class CAGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    private const int dataSlot = 64;
    private const int dataSlot2 = 32;

    public readonly Union32[] Data = new Union32[dataSlot];
    public readonly Union64[] Data2 = new Union64[dataSlot2];

    public override GlobalItem Clone(Item from, Item to)
    {
        CAGlobalItem clone = (CAGlobalItem)base.Clone(from, to);

        Array.Copy(Data, clone.Data, dataSlot);
        Array.Copy(Data2, clone.Data2, dataSlot2);

        return clone;
    }
}
