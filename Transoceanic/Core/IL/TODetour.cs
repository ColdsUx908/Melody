using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Core.IL;

[AttributeUsage(AttributeTargets.Class)]
public class TODetourAttribute(Type targetType) : Attribute
{
    public Type TargetType { get; } = targetType;
}

public interface ITODetourProvider
{
    public abstract Dictionary<MethodInfo, Delegate> DetoursToApply { get; }

    /// <summary>
    /// 加载优先级，越大越早加载。
    /// </summary>
    public virtual decimal LoadPriority => 0m;
}

public class TODetourHelper : ITOLoader
{
    private static readonly Regex _detourRegex = new(@"^Detour_(?<methodName>.*)$");
    internal static readonly List<Hook> _detours = [];

    void ITOLoader.PostSetupContent()
    {
        _detours.Clear();

        foreach ((Type type, TODetourAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<TODetourAttribute>())
        {
            Type targetType = attribute.TargetType;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.UniversalBindingFlags).Where(k => k.IsStatic))
            {
                Match match = _detourRegex.Match(detour.Name);
                if (match.Success)
                    TODetourUtils.ModifyMethodWithDetour(targetType.GetMethod(match.Groups["methodName"].Value, global::Transoceanic.Core.IL.TOReflectionUtils.UniversalBindingFlags), detour);
            }
        }

        foreach (ITODetourProvider detourProvider in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITODetourProvider>().OrderByDescending(k => k.LoadPriority))
        {
            foreach ((MethodInfo target, Delegate detour) in detourProvider.DetoursToApply)
                TODetourUtils.ModifyMethodWithDetour(target, detour);
        }
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

    public static void ModifyMethodWithDetour(MethodInfo target, MethodInfo detour)
    {
        if (target is not null && detour is not null)
        {
            Hook hook = new(target, detour);
            hook.Apply();
            TODetourHelper._detours.Add(hook);
        }
    }

    public static void ModifyMethodWithDetour(MethodInfo target, Delegate detour)
    {
        if (target is not null && detour is not null)
        {
            Hook hook = new(target, detour);
            hook.Apply();
            TODetourHelper._detours.Add(hook);
        }
    }
}