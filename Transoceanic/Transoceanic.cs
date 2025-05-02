using System;
using System.Linq;
using Terraria.ModLoader;
using Transoceanic.Core;
using Transoceanic.Core.IL;
using Transoceanic.GlobalInstances.GlobalNPCs;

namespace Transoceanic;

public class Transoceanic : Mod
{
    internal static Transoceanic Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
        TOMain.GeneralTimer = 0;
        TOGlobalNPC._identifierAllocator = 0ul;
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>(TOMain.Assembly)
            .OrderByDescending(k => k.instance.GetLoadPriority(LoadMethodType.Load)))
        {
            if (!type.MustHaveRealMethodWith("Load", "Unload", TOMain.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement Unload with Load implemented.");
            else
                loader.Load();
        }
    }

    public override void PostSetupContent()
    {
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>()
            .OrderByDescending(k => k.instance.GetLoadPriority(LoadMethodType.PostSetupContent)))
        {
            if (!type.MustHaveRealMethodWith("PostSetUpContents", "OnModUnload", TOMain.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement OnModUnload with PostSetupContent implemented.");
            else
                loader.PostSetupContent();
        }
    }

    public override void Unload()
    {
        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly)
            .OrderByDescending(k => k.GetLoadPriority(LoadMethodType.UnLoad)))
            loader.UnLoad();

        TOGlobalNPC._identifierAllocator = 0ul;
        TOMain.GeneralTimer = 0;
        Instance = null;
    }
}
