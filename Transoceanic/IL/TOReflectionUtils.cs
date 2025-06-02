using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReLogic.Utilities;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using ZLinq;
using ZLinq.Linq;

namespace Transoceanic.IL;

public static class TOReflectionUtils
{
    /// <summary>
    /// 包含所有所需Flag。
    /// </summary>
    public const BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 包含静态成员所需Flag。
    /// </summary>
    public const BindingFlags StaticBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// 检查指定类型是否声明了对应方法，
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <param name="flags"></param>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    public static bool HasRealMethod(this Type type, string methodName, BindingFlags flags, out MethodInfo methodInfo)
    {
        methodInfo = type.GetMethod(methodName, flags);
        return methodInfo is not null && methodInfo.DeclaringType == type;
    }

    /// <summary>
    /// 检查指定类型是否声明了对应方法，
    /// </summary>
    /// <param name="type"></param>
    /// <param name="methodName"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static bool HasRealMethod(this Type type, string methodName, BindingFlags flags) =>
        type.HasRealMethod(methodName, flags, out _);

    /// <summary>
    /// 获取指定类型中所有由该类型声明的方法。
    /// </summary>
    /// <param name="type"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static IEnumerable<MethodInfo> GetRealMethods(this Type type, BindingFlags flags) => type.GetMethods(flags).Where(k => k.DeclaringType == type);

    /// <summary>
    /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="otherMethod"/> 的情况，判定是否有正确的方法关系。
    /// </summary>
    /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="otherMethod"/> 不存在时返回 <see langword="false"/>。</returns>
    public static bool MustHaveRealMethodWith(this Type type, string methodNameA, string methodNameB, BindingFlags flags, out MethodInfo mainMethod, out MethodInfo otherMethod) =>
        (type.HasRealMethod(methodNameA, flags, out mainMethod), type.HasRealMethod(methodNameB, flags, out otherMethod)) is not (true, false);

    /// <summary>
    /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="otherMethod"/> 的情况，判定是否有正确的方法关系。
    /// </summary>
    /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="otherMethod"/> 不存在时返回 <see langword="false"/>。</returns>
    public static bool MustHaveRealMethodWith(this Type type, string mainMethod, string otherMethod, BindingFlags flags) =>
        type.MustHaveRealMethodWith(mainMethod, otherMethod, flags, out _, out _);

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

    /// <summary>
    /// 判定指定方法是否为指定基类型的重写方法。
    /// </summary>
    /// <param name="method"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public static bool IsOverrideOf(this MethodInfo method, Type baseType)
    {
        MethodInfo baseDefinition = method.GetBaseDefinition();
        return baseDefinition.DeclaringType == baseType &&
               !baseDefinition.DeclaringType.IsInterface;
    }

    /// <summary>
    /// 判定指定方法是否为指定基类型的重写方法。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool IsOverrideOf<T>(this MethodInfo method) => method.IsOverrideOf(typeof(T));

    /// <summary>
    /// 判定指定方法是否为某个基类型的重写方法。
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool IsOverride(this MethodInfo method)
    {
        MethodInfo baseDefinition = method.GetBaseDefinition();
        return baseDefinition.DeclaringType != method.DeclaringType &&
               !baseDefinition.DeclaringType.IsInterface;
    }

    /// <summary>
    /// 判定指定方法是否实现了指定接口类型。
    /// </summary>
    /// <param name="method"></param>
    /// <param name="interfaceType"></param>
    /// <returns></returns>
    public static bool IsInterfaceImplementationOf(this MethodInfo method, Type interfaceType)
    {
        InterfaceMapping map;
        try
        {
            map = method.DeclaringType.GetInterfaceMap(interfaceType);
        }
        catch (NotSupportedException)
        {
            return false;
        }

        for (int i = 0; i < map.InterfaceMethods.Length; i++)
        {
            if (map.TargetMethods[i] == method)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 判定指定方法是否实现了指定接口类型。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool IsInterfaceImplementationOf<T>(this MethodInfo method) => method.IsInterfaceImplementationOf(typeof(T));

    /// <summary>
    /// 判定指定方法是否实现了某个接口类型。
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static bool IsInterfaceImplementation(this MethodInfo method) => method.DeclaringType.GetInterfaces().Any(method.IsInterfaceImplementationOf);

    /// <summary>
    /// 获取指定类型中所有重写方法。
    /// </summary>
    /// <param name="type"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static IEnumerable<MethodInfo> GetOverrideMethods(this Type type, BindingFlags flags) => type.GetRealMethods(flags).Where(k => k.IsOverride());

    /// <summary>
    /// 获取指定类型中所有重写方法的名称。
    /// </summary>
    /// <param name="type"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetOverrideMethodNames(this Type type, BindingFlags flags) =>
        from MethodInfo method in type.GetOverrideMethods(flags)
        select method.IsGenericMethod
                 ? method.Name + "<" + string.Join(",", method.GetGenericArguments().Select(k => k.Name)) + ">"
                 : method.Name;
}
