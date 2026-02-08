namespace Transoceanic.Framework.Helpers.AbstractionHelpers;

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