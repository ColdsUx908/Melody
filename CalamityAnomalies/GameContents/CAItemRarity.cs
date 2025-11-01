namespace CalamityAnomalies.GameContents;

public sealed class Celestial : ModRarity
{
    public const int CelestialPrice = 25000000;

    public override Color RarityColor => TOMain.CelestialColor;
    public override int GetPrefixedRarity(int offset, float valueMult) => Type;
}

