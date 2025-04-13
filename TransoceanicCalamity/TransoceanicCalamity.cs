using Terraria.ModLoader;
using Transoceanic;

namespace TransoceanicCalamity;

public class TransoceanicCalamity : Mod
{
    internal static TransoceanicCalamity Instance;

    public override void Load()
    {
        Instance = this;
    }

    public override void Unload()
    {
        Instance = null;
    }
}
