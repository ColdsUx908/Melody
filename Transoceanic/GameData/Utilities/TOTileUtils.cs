namespace Transoceanic.GameData.Utilities;

public static class TOTileUtils
{
    public static bool IsTree(int tileType) => tileType is 5 or 72 or 323 or 571 or >= 583 and <= 589 or 596 or 616 or 634;
}
