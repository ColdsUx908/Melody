namespace CalamityAnomalies.Items;

public sealed class Celestial : ModRarity
{
    public override Color RarityColor => TOMain.CelestialColor;

    public override int GetPrefixedRarity(int offset, float valueMult) => Type;
}

