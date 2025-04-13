using Terraria.ModLoader;

namespace Transoceanic.Systems;

public class GeneralTimerSystem : ModSystem
{
    public override void PreUpdateEntities()
    {
        TOMain.GeneralTimer++;
    }
}
