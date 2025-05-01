using System;
using System.Linq;
using Terraria.ModLoader;
using Transoceanic.Core;
using Transoceanic.Core.IL;

namespace Transoceanic.Systems;

public class TOLoaderSystem : ModSystem
{
    public override void PostAddRecipes()
    {
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>()
            .OrderByDescending(k => k.instance.GetLoadPriority(LoadMethodType.PostAddRecipes)))
        {
            if (!type.MustHaveRealMethodWith("PostAddRecipes", "OnModUnload", TOMain.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement OnModUnload with PostAddRecipes implemented.");
            else
                loader.PostAddRecipes();
        }
    }

    public override void OnModUnload()
    {
        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly)
            .OrderByDescending(k => k.GetLoadPriority(LoadMethodType.OnModUnload)))
            loader.OnModUnload();
    }

    public override void OnWorldLoad()
    {
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>()
            .OrderByDescending(k => k.instance.GetLoadPriority(LoadMethodType.OnWorldLoad)))
        {
            if (!type.MustHaveRealMethodWith("OnWorldLoad", "OnWorldUnload", TOMain.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement OnWorldUnload with OnWorldLoad implemented.");
            else
                loader.OnWorldLoad();
        }
    }

    public override void OnWorldUnload()
    {
        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly)
            .OrderByDescending(k => k.GetLoadPriority(LoadMethodType.OnWorldUnload)))
            loader.OnWorldUnload();
    }
}
