using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MonoMod.RuntimeDetour;

namespace Transoceanic.IL;

/// <summary>
/// 用于标记包含Detour方法的类。
/// <br/>所有Detour方法必须以 <c>Detour_[methodName]</c> 的形式声明。
/// <br/>所有逻辑由反射实现。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DetourClassToAttribute : Attribute
{
    [NotNull]
    public Type TargetType { get; }

    public DetourClassToAttribute([NotNull] Type targetType) => TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// <br/>所有Detour方法必须以 <c>Detour_[typeName]_[methodName]</c> 的形式声明。
/// <br/>所有逻辑由反射实现。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class MultiDetourClassToAttribute : Attribute
{
    [NotNull]
    public Type[] TargetTypes { get; }

    public MultiDetourClassToAttribute([NotNull] params Type[] targetTypes) => TargetTypes = targetTypes ?? throw new ArgumentNullException(nameof(targetTypes));
}

public sealed class DetourMethodAttribute : Attribute
{
    /// <summary>
    /// 目标方法名称。
    /// </summary>
    [NotNull]
    public MethodBase TargetMethod { get; }

    public DetourMethodAttribute([NotNull] MethodBase targetMethod) => TargetMethod = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));
}

/// <summary>
/// 用于实现自定义的Detour逻辑。
/// </summary>
public interface ITODetourProvider
{
    /// <summary>
    /// 应用Detour逻辑。
    /// <br/>使用 <see cref="TODetourUtils.ModifyMethodWithDetour(MethodInfo, Delegate)"/> 或 <see cref="TODetourUtils.ModifyMethodWithDetour(MethodInfo, MethodInfo)"/> 来实现Detour逻辑，
    /// <br/>以便在 <see cref="TODetourHelper._detours"/> 中注册Detour，并自动加载和卸载。
    /// </summary>
    public abstract void ApplyDetour();

    /// <summary>
    /// 加载优先级，越大越早加载。
    /// </summary>
    public virtual decimal LoadPriority => 0m;
}

public sealed class TODetourHelper : ITOLoader
{
    private static readonly Regex _detourRegex = new(@"^Detour_(?<methodName>.*)$");
    private static readonly Regex _multiDetourRegex = new(@"^Detour_(?<typeName>[^_]+)_(?<methodName>.*)$");
    internal static readonly List<Hook> _detours = [];

    void ITOLoader.PostSetupContent()
    {
        _detours.Clear();

        foreach ((Type type, DetourClassToAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<DetourClassToAttribute>())
        {
            Type targetType = attribute.TargetType;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
            {
                Match match = _detourRegex.Match(detour.Name);
                if (match.Success)
                    TODetourUtils.ModifyMethodWithDetour(targetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
            }
        }

        foreach ((Type type, MultiDetourClassToAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<MultiDetourClassToAttribute>())
        {
            Type[] targetTypes = attribute.TargetTypes;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
            {
                Match match = _multiDetourRegex.Match(detour.Name);
                if (match.Success)
                {
                    Type targetType = targetTypes.FirstOrDefault(k => k.Name == match.Groups["typeName"].Value);
                    if (targetType is not null)
                        TODetourUtils.ModifyMethodWithDetour(targetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
                }
            }
        }

        foreach (ITODetourProvider detourProvider in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITODetourProvider>().OrderByDescending(k => k.LoadPriority))
            detourProvider.ApplyDetour();
    }

    void ITOLoader.OnModUnload()
    {
        foreach (Hook hook in _detours)
            hook.Undo();
        _detours.Clear();
    }
}

public static class TODetourUtils
{
    public static void ModifyMethodWithDetour(MethodBase target, MethodInfo detour)
    {
        if (target is not null && detour is not null)
        {
            Hook hook = new(target, detour);
            hook.Apply();
            TODetourHelper._detours.Add(hook);
        }
    }

    public static void ModifyMethodWithDetour(MethodBase target, Delegate detour)
    {
        if (target is not null && detour is not null)
        {
            Hook hook = new(target, detour);
            hook.Apply();
            TODetourHelper._detours.Add(hook);
        }
    }

    public static void ModifyMethodWithDetour(Type targetType, string methodName, MethodInfo detour) => ModifyMethodWithDetour(targetType.GetMethod(methodName, TOReflectionUtils.UniversalBindingFlags), detour);

    public static void ModifyMethodWithDetour(Type targetType, string methodName, Delegate detour) => ModifyMethodWithDetour(targetType.GetMethod(methodName, TOReflectionUtils.UniversalBindingFlags), detour);

    public static void ModifyMethodWithDetour<T>(string methodName, MethodInfo detour) => ModifyMethodWithDetour(typeof(T), methodName, detour);

    public static void ModifyMethodWithDetour<T>(string methodName, Delegate detour) => ModifyMethodWithDetour(typeof(T), methodName, detour);
}