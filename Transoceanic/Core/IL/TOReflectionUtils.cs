using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReLogic.Utilities;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Transoceanic.Core.MathHelp;
using Transoceanic.Core.ExtraMathData;

namespace Transoceanic.Core.IL;

public static class TOReflectionUtils
{
    /// <summary>
    /// 包含所有所需Flag。
    /// </summary>
    public const BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

    public static bool HasRealMethod(this Type type, string methodName, BindingFlags bindingAttr, out MethodInfo methodInfo)
    {
        methodInfo = type.GetMethod(methodName, bindingAttr);
        return methodInfo is not null && methodInfo.DeclaringType == type;
    }

    public static bool HasRealMethod(this Type type, string methodName, BindingFlags bindingAttr) =>
        type.HasRealMethod(methodName, bindingAttr, out _);

    /// <summary>
    /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="otherMethod"/> 的情况，判定是否有正确的方法关系。
    /// </summary>
    /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="otherMethod"/> 不存在时返回 <see langword="false"/>。</returns>
    public static bool MustHaveRealMethodWith(this Type type, string methodNameA, string methodNameB, BindingFlags bindingAttr, out MethodInfo mainMethod, out MethodInfo otherMethod) =>
        TOMathHelper.GetTwoBooleanStatus(type.HasRealMethod(methodNameA, bindingAttr, out mainMethod),
                                         type.HasRealMethod(methodNameB, bindingAttr, out otherMethod))
            is not TwoBooleanStatus.ATrue;

    public static bool MustHaveRealMethodWith(this Type type, string mainMethod, string otherMethod, BindingFlags bindingAttr) =>
        type.MustHaveRealMethodWith(mainMethod, otherMethod, bindingAttr, out _, out _);

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
    /// 获取指定基类型的所有派生类或实现类。
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
    /// 获取指定基类型在指定程序集中的所有派生类或实现类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>(Assembly assemblyToSearch) => GetTypesDerivedFrom(typeof(T), assemblyToSearch);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>() => GetTypesDerivedFrom(typeof(T));

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类的对应实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetTypeInstancesDerivedFrom<T>(Assembly assemblyToSearch) =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
        select (T)Activator.CreateInstance(type);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类的对应实例。
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
    /// 获取指定基类型在指定程序集中的所有派生类或实现类及其对应实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<(Type type, T instance)> GetTypesAndInstancesDerivedFrom<T>(Assembly assemblyToSearch) =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
        select (type, (T)Activator.CreateInstance(type));

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类及其对应实例。
    /// <br/>检测范围为所有已加载的Mod。
    /// <para/>使用该方法的加载器应在 <see cref="Mod.PostSetupContent"/> 中调用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<(Type type, T instance)> GetTypesAndInstancesDerivedFrom<T>() =>
        from type in GetAllTypes()
        where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
        select (type, (T)Activator.CreateInstance(type));

    public static IEnumerable<(Type type, T attribute)> GetTypesWithAttribute<T>(Assembly assemblyToSearch) where T : Attribute =>
        from type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
        let attribute = type.GetAttribute<T>()
        where attribute is not null
        select (type, attribute);

    public static IEnumerable<(Type type, T attribute)> GetTypesWithAttribute<T>() where T : Attribute =>
        from type in GetAllTypes()
        let attribute = type.GetAttribute<T>()
        where attribute is not null
        select (type, attribute);

    public static IEnumerable<MethodInfo> GetRealMethods(this Type type, BindingFlags flags) => type.GetMethods(flags).Where(k => k.DeclaringType == type);
}
