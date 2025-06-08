using MonoMod.RuntimeDetour;

namespace Transoceanic.IL;

/// <summary>
/// 用于标记包含Detour方法的类。
/// <br/>所有逻辑由反射实现。
/// <br/>若目标类不是静态类，则建议使用 <see cref="DetourClassToAttribute{T}"/>。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassToAttribute : Attribute
{
    [DisallowNull]
    public Type TargetType { get; }

    /// <summary>
    /// 用于标识Detour方法的前缀。
    /// <br/>若要标记的Detour方法为 <c>Detour_{methodName}</c> 的形式，则前缀为 <c>"Detour_"</c>。
    /// </summary>
    [DisallowNull]
    public string DetourPrefix { get; }

    public DetourClassToAttribute([DisallowNull] Type targetType, [DisallowNull] string detourPrefix = "Detour_")
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentException.ThrowIfNullOrWhiteSpace(detourPrefix);
        TargetType = targetType;
        DetourPrefix = detourPrefix;
    }
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// <br/>所有逻辑由反射实现。
/// </summary>
/// <typeparam name="T"></typeparam>
public class DetourClassToAttribute<T> : DetourClassToAttribute where T : class
{
    public DetourClassToAttribute([DisallowNull] string detourPrefix = "Detour_") : base(typeof(T), detourPrefix) { }
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// <br/>所有逻辑由反射实现。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MultiDetourClassToAttribute : Attribute
{
    [DisallowNull]
    public Type[] TargetTypes { get; }

    /// <summary>
    /// 用于标识Detour方法的前缀。
    /// <br/>若要标记的Detour方法为 <c>{Detour_}{typeName}_{methodName}</c> 的形式，则前缀为 <c>"Detour_"</c>。
    /// </summary>
    [DisallowNull]
    public string DetourPrefix { get; }

    public MultiDetourClassToAttribute([DisallowNull] Type[] targetTypes, [DisallowNull] string detourPrefix)
    {
        ArgumentException.ThrowIfNullOrEmptyOrAnyNull(targetTypes);
        ArgumentException.ThrowIfNullOrEmpty(detourPrefix);
        TargetTypes = targetTypes;
        DetourPrefix = detourPrefix;
    }

    public MultiDetourClassToAttribute([DisallowNull] params Type[] targetTypes) : this(targetTypes, "Detour_") { }
}

[AttributeUsage(AttributeTargets.Method)]
public class DetourMethodToAttribute : Attribute
{
    [DisallowNull]
    public Type TargetType { get; }

    /// <summary>
    /// 用于标识Detour方法的前缀。
    /// <br/>若要标记的Detour方法为 <c>Detour_{methodName}</c> 的形式，则前缀为 <c>"Detour_"</c>。
    /// </summary>
    [DisallowNull]
    public string DetourPrefix { get; }

    public DetourMethodToAttribute([DisallowNull] Type targetType, [DisallowNull] string detourPrefix = "Detour_")
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentException.ThrowIfNullOrWhiteSpace(detourPrefix);
        TargetType = targetType;
        DetourPrefix = detourPrefix;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class DetourMethodToAttribute<T> : DetourMethodToAttribute
{
    public DetourMethodToAttribute() : base(typeof(T)) { }
}

/// <summary>
/// 用于实现自定义的Detour逻辑。
/// </summary>
public interface ITODetourProvider
{
    /// <summary>
    /// 应用Detour逻辑。
    /// <br/>使用 <see cref="TODetourUtils.ModifyMethodWithDetour(MethodBase, Delegate, DetourConfig)"/> 或 <see cref="TODetourUtils.ModifyMethodWithDetour(MethodBase, MethodInfo, DetourConfig)"/> 来实现Detour逻辑，
    /// <br/>以便在 <see cref="TODetourHelper.Detours"/> 中注册Detour，并自动加载和卸载。
    /// </summary>
    public abstract void ApplyDetour();

    /// <summary>
    /// 加载优先级，越大越早加载。
    /// </summary>
    public virtual decimal LoadPriority => 0m;
}

public class TODetourHelper : ITOLoader
{
    internal static List<Hook> Detours { get; } = [];

    void ITOLoader.PostSetupContent()
    {
        Detours.Clear();

        foreach ((Type type, DetourClassToAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<DetourClassToAttribute>())
        {
            Regex regex = new(attribute.DetourPrefix + @"(?<methodName>.*)$");
            Type targetType = attribute.TargetType;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
            {
                Match match = regex.Match(detour.Name);
                if (match.Success)
                    TODetourUtils.ModifyMethodWithDetour(targetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
            }
        }

        foreach ((Type type, MultiDetourClassToAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<MultiDetourClassToAttribute>())
        {
            Regex regex = new(attribute.DetourPrefix + @"(?<typeName>[^_]+)_(?<methodName>.*)$");
            Type[] targetTypes = attribute.TargetTypes;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
            {
                Match match = regex.Match(detour.Name);
                if (match.Success)
                {
                    Type targetType = targetTypes.AsValueEnumerable().FirstOrDefault(k => k.Name == match.Groups["typeName"].Value);
                    if (targetType is not null)
                        TODetourUtils.ModifyMethodWithDetour(targetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
                }
            }
        }

        foreach ((MethodInfo detour, DetourMethodToAttribute attribute) in TOReflectionUtils.GetMethodsWithAttribute<DetourMethodToAttribute>())
        {
            Regex regex = new(attribute.DetourPrefix + @"(?<methodName>.*)$");
            Match match = regex.Match(detour.Name);
            if (match.Success)
                TODetourUtils.ModifyMethodWithDetour(attribute.TargetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
        }

        foreach (ITODetourProvider detourProvider in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITODetourProvider>().OrderByDescending(k => k.LoadPriority))
            detourProvider.ApplyDetour();
    }

    void ITOLoader.OnModUnload()
    {
        foreach (Hook hook in Detours)
            hook.Undo();
        Detours.Clear();
    }
}

public static class TODetourUtils
{
    public static void ModifyMethodWithDetour(MethodBase target, MethodInfo detour, DetourConfig detourConfig = null)
    {
        if (target is not null && detour is not null)
        {
            Hook hook = detourConfig is not null ? new(target, detour, detourConfig, true) : new(target, detour, true);
            TODetourHelper.Detours.Add(hook);
        }
    }

    public static void ModifyMethodWithDetour(MethodBase target, Delegate detour, DetourConfig detourConfig = null)
    {
        if (target is not null && detour is not null)
        {
            Hook hook = detourConfig is not null ? new(target, detour, detourConfig, true) : new(target, detour, true);
            TODetourHelper.Detours.Add(hook);
        }
    }

    public static void ModifyMethodWithDetour(Type targetType, string methodName, MethodInfo detour, DetourConfig detourConfig = null) => ModifyMethodWithDetour(targetType.GetMethod(methodName, TOReflectionUtils.UniversalBindingFlags), detour, detourConfig);

    public static void ModifyMethodWithDetour(Type targetType, string methodName, Delegate detour, DetourConfig detourConfig = null) => ModifyMethodWithDetour(targetType.GetMethod(methodName, TOReflectionUtils.UniversalBindingFlags), detour, detourConfig);

    public static void ModifyMethodWithDetour<T>(string methodName, MethodInfo detour, DetourConfig detourConfig = null) => ModifyMethodWithDetour(typeof(T), methodName, detour, detourConfig);

    public static void ModifyMethodWithDetour<T>(string methodName, Delegate detour, DetourConfig detourConfig = null) => ModifyMethodWithDetour(typeof(T), methodName, detour, detourConfig);
}