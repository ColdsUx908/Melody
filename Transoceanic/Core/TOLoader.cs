using Transoceanic.RuntimeEditing;

namespace Transoceanic.Core;

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

[AttributeUsage(AttributeTargets.Method)]
public sealed class LoadPriorityAttribute : Attribute
{
    /// <summary>
    /// 加载优先级。
    /// <remarks>值越大，加载越早。</remarks>
    /// </summary>
    public double Priority { get; }

    public LoadPriorityAttribute(double priority) => Priority = priority;
}

/// <summary>
/// 资源加载器接口。
/// </summary>
public interface IResourceLoader
{
    /// <summary>
    /// 内容加载完成后调用。
    /// </summary>
    public virtual void PostSetupContent() { }

    /// <summary>
    /// 添加配方完成后调用。
    /// </summary>
    public virtual void PostAddRecipes() { }

    /// <summary>
    /// 在Mod卸载前调用。
    /// </summary>
    public virtual void OnModUnload() { }

    /// <summary>
    /// 在世界加载时调用。
    /// </summary>
    public virtual void OnWorldLoad() { }

    /// <summary>
    /// 在世界卸载时调用。
    /// </summary>
    public virtual void OnWorldUnload() { }
}

internal interface ITOLoader
{
    /// <summary>
    /// 在本Mod加载时调用。
    /// </summary>
    internal abstract void Load();

    /// <summary>
    /// 在Mod卸载时调用。
    /// </summary>
    internal virtual void Unload() { }
}

public sealed class TOLoaderSystem : ModSystem
{
    public override void PostAddRecipes()
    {
        foreach (IResourceLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IResourceLoader>().AsValueEnumerable()
            orderby pair.type.GetMethod(nameof(IResourceLoader.PostAddRecipes), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0
            select pair.instance)
        {
            loader.PostAddRecipes();
        }
    }

    public override void OnModUnload()
    {
        foreach (IResourceLoader loader in (
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IResourceLoader>().AsValueEnumerable()
            orderby pair.type.GetMethod(nameof(IResourceLoader.PostSetupContent), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
            select pair.instance).Reverse())
        {
            loader.OnModUnload();
        }
    }

    public override void OnWorldLoad()
    {
        foreach (IResourceLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IResourceLoader>().AsValueEnumerable()
            orderby pair.type.GetMethod(nameof(IResourceLoader.OnWorldLoad), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
            select pair.instance)
        {
            loader.OnWorldLoad();
        }
    }

    public override void OnWorldUnload()
    {
        if (TOMain.Loaded)
        {
            foreach (IResourceLoader loader in (
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IResourceLoader>().AsValueEnumerable()
                orderby pair.type.GetMethod(nameof(IResourceLoader.OnWorldLoad), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                select pair.instance).Reverse())
            {
                loader.OnWorldUnload();
            }
        }
    }
}