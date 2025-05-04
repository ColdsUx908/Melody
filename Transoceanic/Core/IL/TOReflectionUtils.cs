using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Transoceanic.Core.ExtraData.Maths;
using Transoceanic.Core.MathHelp;

namespace Transoceanic.Core.IL;

public static class TOReflectionUtils
{
    public static bool HasRealMethod(this Type type, string methodName, BindingFlags bindingAttr, out MethodInfo methodInfo)
    {
        methodInfo = type.GetMethod(methodName, bindingAttr);
        return methodInfo is not null && methodInfo.DeclaringType == type;
    }

    public static bool HasRealMethod(this Type type, string methodName, BindingFlags bindingAttr) =>
        type.HasRealMethod(methodName, bindingAttr, out _);

    public static bool MustHaveRealMethodWith(this Type type, string methodNameA, string methodNameB, BindingFlags bindingAttr, out MethodInfo mainMethod, out MethodInfo otherMethod) =>
        TOMathHelper.GetTwoBooleanStatus(type.HasRealMethod(methodNameA, bindingAttr, out mainMethod),
                                         type.HasRealMethod(methodNameB, bindingAttr, out otherMethod))
            is not TwoBooleanStatus.ATrue;

    public static bool MustHaveRealMethodWith(this Type type, string mainMethod, string otherMethod, BindingFlags bindingAttr) =>
        type.MustHaveRealMethodWith(mainMethod, otherMethod, bindingAttr, out _, out _);

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类。
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom(Type baseType, Assembly assemblyToSearch) =>
        from Type type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
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
        from Mod mod in ModLoader.Mods
        from Type type in
            from Type type in AssemblyManager.GetLoadableTypes(mod.Code)
            where baseType.IsAssignableTo(type) && !type.IsAbstract
            select type
        select type;

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>(Assembly assemblyToSearch) =>
        GetTypesDerivedFrom(typeof(T), assemblyToSearch);

    /// <summary>
    /// 获取指定基类型的所有派生类或实现类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<Type> GetTypesDerivedFrom<T>() =>
        GetTypesDerivedFrom(typeof(T));

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类的对应实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<T> GetTypeInstancesDerivedFrom<T>(Assembly assemblyToSearch) =>
        from Type type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
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
        from Mod mod in ModLoader.Mods
        from Type type in
            from Type type in AssemblyManager.GetLoadableTypes(mod.Code)
            where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
            select type
        select (T)Activator.CreateInstance(type);

    /// <summary>
    /// 获取指定基类型在指定程序集中的所有派生类或实现类及其对应实例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assemblyToSearch"></param>
    /// <returns></returns>
    public static IEnumerable<(Type type, T instance)> GetTypesAndInstancesDerivedFrom<T>(Assembly assemblyToSearch) =>
        from Type type in AssemblyManager.GetLoadableTypes(assemblyToSearch)
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
        from Mod mod in ModLoader.Mods
        from Type type in
            from Type type in AssemblyManager.GetLoadableTypes(mod.Code)
            where type.IsAssignableTo(typeof(T)) && !type.IsAbstract
            select type
        select (type, (T)Activator.CreateInstance(type));
}
