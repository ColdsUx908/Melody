﻿using System;
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
            .OrderByDescending(k => k.instance.GetPriority(LoaderMethodType.PostAddRecipes)))
        {
            if (!type.MustHaveRealMethodWith("PostAddRecipes", "OnModUnload", TOReflectionUtils.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement OnModUnload with PostAddRecipes implemented.");
            else
                loader.PostAddRecipes();
        }
    }

    public override void OnModUnload()
    {
        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly)
            .OrderByDescending(k => k.GetPriority(LoaderMethodType.OnModUnload)))
            loader.OnModUnload();
    }

    public override void OnWorldLoad()
    {
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>()
            .OrderByDescending(k => k.instance.GetPriority(LoaderMethodType.OnWorldLoad)))
        {
            if (!type.MustHaveRealMethodWith("OnWorldLoad", "OnWorldUnload", TOReflectionUtils.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement OnWorldUnload with OnWorldLoad implemented.");
            else
                loader.OnWorldLoad();
        }
    }

    public override void OnWorldUnload()
    {
        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly)
            .OrderByDescending(k => k.GetPriority(LoaderMethodType.OnWorldUnload)))
            loader.OnWorldUnload();
    }
}
