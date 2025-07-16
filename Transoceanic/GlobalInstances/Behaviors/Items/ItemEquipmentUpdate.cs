namespace Transoceanic.GlobalInstances.Behaviors.Items;

public sealed class ItemEquipmentUpdate : TOGlobalItemBehavior
{
    public override decimal Priority => 500m;

    public override void UpdateEquip(Item item, Player player)
    {
        TOGlobalItem oceanItem = item.Ocean();
        oceanItem.InternalEquipped += 2;
        if (oceanItem.ShouldUpdateLastEquippedTime)
            oceanItem._lastEquippedTime = TOWorld.GameTimer.TotalTicks;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        TOGlobalItem oceanItem = item.Ocean();
        if (oceanItem.ShouldUpdateLastUnequippedTime)
            oceanItem._lastUnequippedTime = TOWorld.GameTimer.TotalTicks;
        oceanItem.InternalEquipped--;
    }
}
