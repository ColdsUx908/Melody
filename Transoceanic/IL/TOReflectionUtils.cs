namespace Transoceanic.IL;

public static class TOReflectionUtils
{
    /// <summary>
    /// 包含所有所需Flag。
    /// </summary>
    public const BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 包含实例成员所需Flag。
    /// </summary>
    public const BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 包含静态成员所需Flag。
    /// </summary>
    public const BindingFlags StaticBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 获取能被tML加载的所有类型。
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Type> GetAllTypes() =>
        from mod in ModLoader.Mods
        from type in AssemblyManager.GetLoadableTypes(mod.Code)
        select type;

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类。
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType, Assembly assemblyToSearch) =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        where baseType.IsAssignableTo(type) && !type.IsAbstract
        select type;

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType) =>
        from type in GetAllTypes()
        where baseType.IsAssignableTo(type) && !type.IsAbstract
        select type;

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>(Assembly assemblyToSearch) => GetTypesDerivedFrom(typeof(T), assemblyToSearch);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>() => GetTypesDerivedFrom(typeof(T));

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）的对应实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetTypeInstancesDerivedFrom<T>(Assembly assemblyToSearch) =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
        select (T)Activator.CreateInstance(type);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）的对应实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetTypeInstancesDerivedFrom<T>() =>
        from type in GetAllTypes()
        where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
        select (T)Activator.CreateInstance(type);

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类（非抽象）及其对应实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<(Type type, T instance)> GetTypesAndInstancesDerivedFrom<T>(Assembly assemblyToSearch) =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
        select (type, (T)Activator.CreateInstance(type));

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类（非抽象）及其对应实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<(Type type, T instance)> GetTypesAndInstancesDerivedFrom<T>() =>
        from type in GetAllTypes()
        where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
        select (type, (T)Activator.CreateInstance(type));

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的类型及对应特性实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<(Type type, T attribute)> GetTypesWithAttribute<T>(Assembly assemblyToSearch) where T : Attribute =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        let attribute = type.GetAttribute<T>()
        where attribute is not null
        select (type, attribute);

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的类型及对应特性实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<(Type type, T attribute)> GetTypesWithAttribute<T>() where T : Attribute =>
        from type in GetAllTypes()
        let attribute = type.GetAttribute<T>()
        where attribute is not null
        select (type, attribute);

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的方法及对应特性实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<(MethodInfo method, T attribute)> GetMethodsWithAttribute<T>(Assembly assemblyToSearch) where T : Attribute =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        from method in type.GetRealMethods(UniversalBindingFlags)
        let attribute = method.GetAttribute<T>()
        where attribute is not null
        select (method, attribute);

    /// <summary>
    /// 获取指定程序集中所有指定特性修饰的方法及对应特性实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<(MethodInfo method, T attribute)> GetMethodsWithAttribute<T>() where T : Attribute =>
        from type in GetAllTypes()
        from method in type.GetRealMethods(UniversalBindingFlags)
        let attribute = method.GetAttribute<T>()
        where attribute is not null
        select (method, attribute);
}
