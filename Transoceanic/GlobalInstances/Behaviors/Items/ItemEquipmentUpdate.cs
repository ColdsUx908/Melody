namespace Transoceanic.GlobalInstances.Behaviors.Items;

public sealed class ItemEquipmentUpdate : TOGlobalItemBehavior
{
    public override decimal Priority => 500m;

    public override void UpdateEquip(Item item, Player player)
    {
        TOGlobalItem oceanItem = item.Ocean();
        oceanItem.InternalEquipped.Value = true;
        if (oceanItem.Equip._lastOn <= oceanItem.Equip._lastOff)
            oceanItem.Equip._lastOn = TOWorld.GameTimer.TotalTicks;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        TOGlobalItem oceanItem = item.Ocean();
        if (!Main.gamePaused && Main.hasFocus && !oceanItem.InternalEquipped && oceanItem.Equip._lastOff < oceanItem.Equip._lastOn)
            oceanItem.Equip._lastOff = TOWorld.GameTimer.TotalTicks;
        oceanItem.InternalEquipped.Value = false;
    }
}
