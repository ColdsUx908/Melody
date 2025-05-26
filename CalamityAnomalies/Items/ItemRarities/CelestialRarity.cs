using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Transoceanic;

namespace CalamityAnomalies.Items.ItemRarities;

public class Celestial : ModRarity
{
    public override Color RarityColor => TOMain.CelestialColor;
    public override int GetPrefixedRarity(int offset, float valueMult) => Type;
}

