namespace Transoceanic;

public enum LoaderMethodType
{
    Load,
    PostSetupContent,
    PostAddRecipes,
    OnModUnload,
    UnLoad,

    OnWorldLoad,
    OnWorldUnload
}

/// <summary>
/// 资源加载器接口。
/// </summary>
public interface ITOLoader
{
    /// <summary>
    /// 在本Mod加载时调用。
    /// <br/>必须与 <see cref="UnLoad"/> 一起实现。
    /// <br/>不支持其他程序集实现。
    /// </summary>
    internal virtual void Load() { }

    /// <summary>
    /// 内容加载完成后调用。
    /// <br/>必须与 <see cref="OnModUnload"/> 一起实现。
    /// <br/>其他程序集类型只应覆写此方法。
    /// </summary>
    public virtual void PostSetupContent() { }

    /// <summary>
    /// 添加配方完成后调用。
    /// <br/>必须与 <see cref="OnModUnload"/> 一起实现。
    /// <br/>其他程序集类型只应覆写此方法。
    /// </summary>
    public virtual void PostAddRecipes() { }

    /// <summary>
    /// 在Mod卸载前调用。
    /// </summary>
    public virtual void OnModUnload() { }

    /// <summary>
    /// 在Mod卸载时调用。
    /// <br/>不支持其他程序集实现。
    /// </summary>
    internal virtual void UnLoad() { }

    /// <summary>
    /// 在世界加载时调用。
    /// </summary>
    public virtual void OnWorldLoad() { }

    /// <summary>
    /// 在世界卸载时调用。
    /// </summary>
    public virtual void OnWorldUnload() { }

    /// <summary>
    /// 优先级，越大越早执行。
    /// <br/>如对加载顺序不敏感，不要重写此方法。
    /// <remarks>请确保同时处理加载和卸载优先级，以避免出现问题。一般建议设置卸载优先级为对应加载优先级的相反数。</remarks>
    /// </summary>
    /// <param name="type">加载方法类型。</param>
    public virtual decimal GetPriority(LoaderMethodType type) => 0m;
}

public class TOLoaderSystem : ModSystem
{
    public override void PostAddRecipes()
    {
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>().AsValueEnumerable()
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
        foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).AsValueEnumerable()
            .OrderByDescending(k => k.GetPriority(LoaderMethodType.OnModUnload)))
        {
            loader.OnModUnload();
        }
    }

    public override void OnWorldLoad()
    {
        foreach ((Type type, ITOLoader loader) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>().AsValueEnumerable()
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
        if (Transoceanic.Loaded)
        {
            foreach (ITOLoader loader in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOLoader>(TOMain.Assembly).AsValueEnumerable()
                .OrderByDescending(k => k.GetPriority(LoaderMethodType.OnWorldUnload)))
            {
                loader.OnWorldUnload();
            }
        }
    }
}