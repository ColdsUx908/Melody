using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Transoceanic.Core;

namespace Transoceanic.Systems;

public class TOLoaderSystem : ModSystem
{
    public override void OnModUnload()
    {
        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).
            OrderByDescending(k => k.LoadPriority(LoadMethodType.Load)))
            loader.OnModUnload();
    }
}
