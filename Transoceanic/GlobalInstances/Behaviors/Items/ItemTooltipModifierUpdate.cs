namespace Transoceanic.GlobalInstances.Behaviors.Items;

public sealed class ItemTooltipModifierUpdate : TOGlobalItemBehavior
{
    public override decimal Priority => 500m;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) => item.Ocean().TooltipDictionary = new(tooltips);
}
