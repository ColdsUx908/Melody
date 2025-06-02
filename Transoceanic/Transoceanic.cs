using System;
using System.Linq;
using Terraria.ModLoader;
using Transoceanic.GlobalInstances;
using Transoceanic.IL;
using ZLinq;

namespace Transoceanic;

public class Transoceanic : Mod
{
    internal static Transoceanic Instance { get; private set; }
    /// <summary>
    /// 是否已加载 <see cref="Instance"/>，亦即 <see cref="Load"/> 方法已被调用。
    /// </summary>
    internal static bool InstanceLoaded => Instance is not null;

    public override void Load()
    {
        Instance = this;
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).AsValueEnumerable()
            .OrderByDescending(k => k.instance.GetPriority(LoaderMethodType.Load)))
        {
            if (!type.MustHaveRealMethodWith("Load", "Unload", TOReflectionUtils.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement Unload with Load implemented.");
            else
                loader.Load();
        }
    }

    public override void PostSetupContent()
    {
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>().AsValueEnumerable()
            .OrderByDescending(k => k.instance.GetPriority(LoaderMethodType.PostSetupContent)))
        {
            if (!type.MustHaveRealMethodWith("PostSetUpContents", "OnModUnload", TOReflectionUtils.UniversalBindingFlags))
                throw new Exception($"[{type.Name}] must implement OnModUnload with PostSetupContent implemented.");
            else
                loader.PostSetupContent();
        }
    }

    public override void Unload()
    {
        if (InstanceLoaded)
        {
            foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly)
                .OrderByDescending(k => k.GetPriority(LoaderMethodType.UnLoad)))
            {
                loader.UnLoad();
            }

            TOGlobalNPC._identifierAllocator = 0ul;
            TOMain.GameTimer = 0;
            Instance = null;
        }
    }
}
