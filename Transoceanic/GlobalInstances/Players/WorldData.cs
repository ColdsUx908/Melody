using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Transoceanic.GlobalInstances.Players;

public partial class TOPlayer : ModPlayer
{
    public override void SaveData(TagCompound tag)
    {
    }

    public override void LoadData(TagCompound tag)
    {
    }

    public override void OnEnterWorld()
    {
        GameTime = 0;
    }
}
