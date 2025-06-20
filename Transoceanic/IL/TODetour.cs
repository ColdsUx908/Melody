using System.Collections;
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
    /// <br/>使用 <see cref="TODetourUtils.Modify{TDelegate}(MethodBase, TDelegate)"/> 或 <see cref="TODetourUtils.Modify(MethodBase, MethodInfo)"/> 来实现Detour逻辑，
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
    public class DetourSet : IEnumerable<Hook>
    {
        private readonly Dictionary<Type, Dictionary<MethodBase, List<Hook>>> _data = [];

        public void Add(Hook hook)
        {
            ArgumentNullException.ThrowIfNull(hook);
            Type targetType = hook.Source.DeclaringType;
            if (!_data.TryGetValue(targetType, out Dictionary<MethodBase, List<Hook>> methodHooks))
                _data[targetType] = methodHooks = [];
            if (!methodHooks.TryGetValue(hook.Source, out List<Hook> hooks))
                methodHooks[hook.Source] = hooks = [];
            hooks.Add(hook);
        }

        public bool RemoveFirst(Hook hook)
        {
            ArgumentNullException.ThrowIfNull(hook);
            Type targetType = hook.Source.DeclaringType;
            if (!_data.TryGetValue(targetType, out Dictionary<MethodBase, List<Hook>> methodHooks))
                return false;
            foreach (List<Hook> hooks in methodHooks.Values)
            {
                if (hooks.Remove(hook))
                    return true;
            }
            return false;
        }

        public void Clear()
        {
            foreach (Dictionary<MethodBase, List<Hook>> methodHooks in _data.Values)
            {
                foreach (List<Hook> hooks in methodHooks.Values)
                {
                    foreach (Hook hook in hooks)
                        hook.Undo();
                    hooks.Clear();
                }
                methodHooks.Clear();
            }
            _data.Clear();
        }

        public bool TryGetHooks(MethodBase targetMethod, out List<Hook> hooks)
        {
            ArgumentNullException.ThrowIfNull(targetMethod);
            foreach (Dictionary<MethodBase, List<Hook>> methodHooks in _data.Values)
            {
                if (methodHooks.TryGetValue(targetMethod, out hooks) && hooks.Count > 0)
                    return true;
            }
            hooks = null;
            return false;
        }

        public IEnumerator<Hook> GetEnumerator()
        {
            foreach (Dictionary<MethodBase, List<Hook>> methodHooks in _data.Values)
            {
                foreach (List<Hook> hooks in methodHooks.Values)
                {
                    foreach (Hook hook in hooks)
                        yield return hook;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal static DetourSet Detours { get; } = [];

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
    void ITOLoader.OnModUnload() => Detours.Clear();
}

public static class TODetourUtils
{
    public static Hook Modify(MethodBase target, MethodInfo detour)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(detour);
        DetourConfig detourConfig = detour.GetAttribute<CustomDetourConfigAttribute>()?.DetourConfig;
        Hook hook = detourConfig is not null ? new(target, detour, detourConfig, true) : new(target, detour, true);
        TODetourHelper.Detours.Add(hook);
        return hook;
    }

    public static Hook Modify<TDelegate>(MethodBase target, TDelegate detour) where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(detour);
        DetourConfig detourConfig = detour.Method.GetAttribute<CustomDetourConfigAttribute>()?.DetourConfig;
        Hook hook = detourConfig is not null ? new(target, detour, detourConfig, true) : new(target, detour, true);
        TODetourHelper.Detours.Add(hook);
        return hook;
    }

    public static Hook Modify(Type targetType, string methodName, MethodInfo detour) => Modify(targetType.GetMethod(methodName, TOReflectionUtils.UniversalBindingFlags), detour);

    public static Hook Modify<TDelegate>(Type targetType, string methodName, TDelegate detour) where TDelegate : Delegate => Modify(targetType.GetMethod(methodName, TOReflectionUtils.UniversalBindingFlags), detour);

    public static Hook Modify<T>(string methodName, MethodInfo detour) => Modify(typeof(T), methodName, detour);

    public static Hook Modify<T, TDelegate>(string methodName, TDelegate detour) where TDelegate : Delegate => Modify(typeof(T), methodName, detour);

    private const string DefaultPrefix = "Detour_";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = @"(?<methodName>.*)$";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern2 = @"(?<typeName>[^_]+)_(?<methodName>.*)$";

    public static Hook ApplyStaticMethodDetour(MethodInfo detour, Type targetType)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return null;
        string prefix = detour.GetAttribute<CustomDetourPrefixAttribute>()?.Prefix ?? DefaultPrefix;
        Match match = Regex.Match(detour.Name, prefix + Pattern);
        if (match.Success)
            return Modify(detour.GetAttribute<CustomDetourTargetAttribute>()?.TargetMethod ?? targetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
        return null;
    }

    public static Hook ApplyTypedStaticMethodDetour(MethodInfo detour, Type[] targetTypes)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return null;
        string prefix = detour.GetAttribute<CustomDetourPrefixAttribute>()?.Prefix ?? DefaultPrefix;
        Match match = Regex.Match(detour.Name, prefix + Pattern2);
        if (match.Success)
        {
            Type targetType = targetTypes.AsValueEnumerable().FirstOrDefault(k => k.Name == match.Groups["typeName"].Value);
            if (targetType is not null)
                return Modify(detour.GetAttribute<CustomDetourTargetAttribute>()?.TargetMethod ?? targetType.GetMethod(match.Groups["methodName"].Value, TOReflectionUtils.UniversalBindingFlags), detour);
        }
        return null;
    }
}