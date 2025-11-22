namespace Transoceanic.Utilities;

public static class TOAssetUtils
{
    public static string FormatVanillaItemTexturePath(int type) => $"Terraria/Images/Item_{type}";

    public static Texture2D GetItemTexture(int type)
    {
        Main.instance.LoadItem(type);
        return TextureAssets.Item[type].Value;
    }

    public static Texture2D GetNPCTexture(int type)
    {
        Main.instance.LoadNPC(type);
        return TextureAssets.Npc[type].Value;
    }

    public static Texture2D GetProjectileTexture(int type)
    {
        Main.instance.LoadProjectile(type);
        return TextureAssets.Projectile[type].Value;
    }

    public static Texture2D GetGoreTexture(int type)
    {
        Main.instance.LoadGore(type);
        return TextureAssets.Gore[type].Value;
    }

    public static Texture2D GetWallTexture(int type)
    {
        Main.instance.LoadWall(type);
        return TextureAssets.Wall[type].Value;
    }

    public static Texture2D GetTileTexture(int type)
    {
        Main.instance.LoadTiles(type);
        return TextureAssets.Tile[type].Value;
    }

    public static Texture2D GetItemFlameTexture(int type)
    {
        Main.instance.LoadItemFlames(type);
        return TextureAssets.ItemFlame[type].Value;
    }

    public static Texture2D GetBackgroundTexture(int type)
    {
        Main.instance.LoadBackground(type);
        return TextureAssets.Background[type].Value;
    }
}
