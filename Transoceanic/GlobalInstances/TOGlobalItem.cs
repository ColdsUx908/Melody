
namespace Transoceanic.GlobalInstances;

public class TOGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public override GlobalItem Clone(Item from, Item to)
    {
        TOGlobalItem clone = (TOGlobalItem)base.Clone(from, to);

        return clone;
    }
}
