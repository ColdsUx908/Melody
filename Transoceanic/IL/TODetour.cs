using System.ComponentModel;
using MonoMod.RuntimeDetour;

namespace Transoceanic.IL;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourTargetAttribute : Attribute
{
    [DisallowNull]
    public Type TargetType { get; }

    [DisallowNull]
    public string Name { get; }

    [DisallowNull]
    public BindingFlags BindingAttr { get; }

    public CustomDetourTargetAttribute(Type targetType, string name, BindingFlags bindingAttr)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        Name = name;
        BindingAttr = bindingAttr;
    }

    public MethodInfo TargetMethod => TargetType.GetMethod(Name, BindingAttr);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourPrefixAttribute : Attribute
{
    [DisallowNull]
    public string Prefix { get; set; }

    public CustomDetourPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourConfigAttribute : Attribute
{
    [DisallowNull]
    public string Id { get; }

    public int? Priority { get; }

    [AllowNull]
    public IEnumerable<string> Before { get; }

    [AllowNull]
    public IEnumerable<string> After { get; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public int SubPriority { get; }

    public DetourConfig DetourConfig => new(Id, Priority, Before, After, SubPriority);

    public CustomDetourConfigAttribute(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
}

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

    public DetourClassToAttribute(Type targetType) => TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// <br/>所有逻辑由反射实现。
/// </summary>
/// <typeparam name="T"></typeparam>
public class DetourClassToAttribute<T> : DetourClassToAttribute where T : class
{
    public DetourClassToAttribute() : base(typeof(T)) { }
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

    public MultiDetourClassToAttribute(params Type[] targetTypes)
    {
        ArgumentException.ThrowIfNullOrEmptyOrAnyNull(targetTypes);
        TargetTypes = targetTypes;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class DetourMethodToAttribute : Attribute
{
    [DisallowNull]
    public Type TargetType { get; }

    public DetourMethodToAttribute(Type targetType) => TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

[AttributeUsage(AttributeTargets.Method)]
public class NotDetourMethodAttribute : Attribute { }

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
    /// <br/>使用 <see cref="TODetourUtils.ModifyMethodWithDetour(MethodBase, Delegate)"/> 或 <see cref="TODetourUtils.ModifyMethodWithDetour(MethodBase, MethodInfo)"/> 来实现Detour逻辑，
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
            Type targetType = attribute.TargetType;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
                TODetourUtils.ApplyStaticMethodDetour(detour, targetType);
        }

        foreach ((Type type, MultiDetourClassToAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<MultiDetourClassToAttribute>())
        {
            Type[] targetTypes = attribute.TargetTypes;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
                TODetourUtils.ApplyTypedStaticMethodDetour(detour, targetTypes);
        }

        foreach ((MethodInfo detour, DetourMethodToAttribute attribute) in TOReflectionUtils.GetMethodsWithAttribute<DetourMethodToAttribute>())
            TODetourUtils.ApplyStaticMethodDetour(detour, attribute.TargetType);

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
    public static void ModifyMethodWithDetour(MethodBase target, MethodInfo detour)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(detour);
        DetourConfig detourConfig = detour.GetAttribute<CustomDetourConfigAttribute>()?.DetourConfig;
        TODetourHelper.Detours.Add(detourConfig is not null ? new(target, detour, detourConfig, true) : new(target, detour, true));
    }

    public static void ModifyMethodWithDetour(MethodBase target, Delegate detour)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(detour);
        DetourConfig detourConfig = detour.Method.GetAttribute<CustomDetourConfigAttribute>()?.DetourConfig;
        TODetourHelper.Detours.Add(detourConfig is not null ? new(target, detour, detourConfig, true) : new(target, detour, true));
    }

    public static void ModifyMethodWithDetour(Type targetType, string methodName, MethodInfo detour) => ModifyMethodWithDetour(targetType.GetMethod(methodName, TOReflectionUtils.UniversalBindingFlags), detour);

    public static void ModifyMethodWithDetour(Type targetType, string methodName, Delegate detour) => ModifyMethodWithDetour(targetType.GetMethod(methodName, TOReflectionUtils.UniversalBindingFlags), detour);

    public static void ModifyMethodWithDetour<T>(string methodName, MethodInfo detour) => ModifyMethodWithDetour(typeof(T), methodName, detour);

    public static void ModifyMethodWithDetour<T>(string methodName, Delegate detour) => ModifyMethodWithDetour(typeof(T), methodName, detour);

    private const string DefaultPrefix = "Detour_";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = @"(?<methodName>.*)$";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern2 = @"(?<typeName>[^_]+)_(?<methodName>.*)$";

    public static void ApplyStaticMethodDetour(MethodInfo detour, Type targetType)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return;
        string prefix = detour.GetAttribute<CustomDetourPrefixAttribute>()?.Prefix ?? DefaultPrefix;
        Match match = Regex.Match(detour.Name, prefix + Pattern);
        if (match.Success)
            ModifyMethodWithDetour(detour.GetAttribute<CustomDetourTargetAttribute>()?.TargetMethod ?? targetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
    }

    public static void ApplyTypedStaticMethodDetour(MethodInfo detour, Type[] targetTypes)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return;
        string prefix = detour.GetAttribute<CustomDetourPrefixAttribute>()?.Prefix ?? DefaultPrefix;
        Match match = Regex.Match(detour.Name, prefix + Pattern2);
        if (match.Success)
        {
            Type targetType = targetTypes.AsValueEnumerable().FirstOrDefault(k => k.Name == match.Groups["typeName"].Value);
            if (targetType is not null)
                ModifyMethodWithDetour(detour.GetAttribute<CustomDetourTargetAttribute>()?.TargetMethod ?? targetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
        }
    }
}