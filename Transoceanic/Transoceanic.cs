using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Transoceanic.Commands;
using Transoceanic.Core;
using Transoceanic.GlobalEntity.GlobalNPCs;

namespace Transoceanic;

public class Transoceanic : Mod
{
    internal static Transoceanic Instance;

    public override void Load()
    {
        Instance = this;
        TOMain.GeneralTimer = 0;
        TOGlobalNPC._identifierAllocator = 0;
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).
            OrderByDescending(k => k.instance.LoadPriority(LoadMethodType.Load)))
        {
            if (!type.MustHaveRealMethodTogether("Load", "Unload", TOMain.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must have a Load and Unload method together or neither.");
            else
                loader.Load();
        }
    }

    public override void PostSetupContent()
    {
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>().
            OrderByDescending(k => k.instance.LoadPriority(LoadMethodType.PostSetupContent)))
        {
            if (!type.MustHaveRealMethodTogether("PostSetUpContents", "OnModUnload", TOMain.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must have a PostSetUpContents and OnModUnload method together or neither.");
            else
                loader.Load();
        }

        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>().
            OrderByDescending(k => k.LoadPriority(LoadMethodType.PostSetupContent)))
            loader.PostSetupContent();
    }

    public override void Unload()
    {
        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).
            OrderByDescending(k => k.LoadPriority(LoadMethodType.Load)))
            loader.UnLoad();
        TOGlobalNPC._identifierAllocator = 0;
        TOMain.GeneralTimer = 0;
        Instance = null;
    }
}
