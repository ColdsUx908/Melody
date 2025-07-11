namespace Transoceanic.Core.Utilities;

public static class TOTileUtils
{
    public static bool TryGetTile(int i, int j, int fluff, out Tile tile)
    {
        if (WorldGen.InWorld(i, j, fluff))
        {
            tile = Main.tile[i, j];
            return true;
        }
        else
        {
            tile = default;
            return false;
        }
    }

    public static bool IsTree(int tileType) => tileType is 5 or 72 or 323 or 571 or >= 583 and <= 589 or 596 or 616 or 634;

    public static IEnumerable<(Tile tile, int i, int j)> GetBorderTiles(Point corner1, Point corner2, int fluff = 0) =>
        GetBorderTiles(Math.Min(corner1.X, corner2.X), Math.Max(corner1.X, corner2.X), Math.Min(corner1.Y, corner2.Y), Math.Max(corner1.Y, corner2.Y), fluff);

    public static IEnumerable<(Tile tile, int i, int j)> GetBorderTiles(int minX, int maxX, int minY, int maxY, int fluff)
    {
        switch (minX == maxX, minY == maxY)
        {
            case (true, true):
                if (TryGetTile(minX, minY, fluff, out Tile tile1))
                    yield return (tile1, minX, minY);
                break;
            case (true, false):
                for (int j = minY; j <= maxY; j++)
                {
                    if (TryGetTile(minX, j, fluff, out Tile tile2))
                        yield return (tile2, minX, j);
                }
                break;
            case (false, true):
                for (int i = minX; i <= maxX; i++)
                {
                    if (TryGetTile(i, minY, fluff, out Tile tile3))
                        yield return (tile3, i, minY);
                }
                break;
            case (false, false):
                for (int i = minX; i <= maxX; i++)
                {
                    if (TryGetTile(i, minY, fluff, out Tile tileUp))
                        yield return (tileUp, i, minY);
                    if (TryGetTile(i, maxY, fluff, out Tile tileDown))
                        yield return (tileDown, i, maxY);
                }
                for (int j = minY + 1; j < maxY; j++)
                {
                    if (TryGetTile(minX, j, fluff, out Tile tileLeft))
                        yield return (tileLeft, minX, j);
                    if (TryGetTile(maxX, j, fluff, out Tile tileRight))
                        yield return (tileRight, maxX, j);
                }
                break;
        }
    }
}
