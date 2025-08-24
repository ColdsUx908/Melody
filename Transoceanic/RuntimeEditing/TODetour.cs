using System.Collections;
using System.ComponentModel;
using MonoMod.RuntimeDetour;

namespace Transoceanic.RuntimeEditing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourSourceAttribute : Attribute
{
    public readonly Type SourceType;

    public readonly string Name;

    public readonly BindingFlags BindingAttr;

    public readonly Type[] ParameterTypes;

    public CustomDetourSourceAttribute(Type sourceType, string name, BindingFlags bindingAttr, Type[] parameterTypes = null)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentException.ThrowIfNullOrEmpty(name);
        SourceType = sourceType;
        Name = name;
        BindingAttr = bindingAttr;
        ParameterTypes = parameterTypes;
    }

    public MethodInfo Source =>
        ParameterTypes is not null ? SourceType.GetMethod(Name, BindingAttr, ParameterTypes) : SourceType.GetMethod(Name, BindingAttr);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourPrefixAttribute : Attribute
{
    public readonly string Prefix;

    public CustomDetourPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourConfigAttribute : Attribute
{
    public readonly string Id;

    public readonly int? Priority;

    public readonly string[] Before;

    public readonly string[] After;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly int SubPriority;

    public DetourConfig DetourConfig => new(Id, Priority, Before, After, SubPriority);

    public CustomDetourConfigAttribute(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// <br/>若目标类不是静态类，则建议使用 <see cref="DetourClassToAttribute{T}"/>。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassToAttribute : Attribute
{
    public readonly Type SourceType;

    public DetourClassToAttribute(Type sourceType) => SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// </summary>
/// <typeparam name="T"></typeparam>
public class DetourClassToAttribute<T> : DetourClassToAttribute where T : class
{
    public DetourClassToAttribute() : base(typeof(T)) { }
}

/// <summary>
/// 用于标记包含Detour方法的类。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassTo_MultiSourceAttribute : Attribute
{
    public readonly Type[] SourceTypes;

    public DetourClassTo_MultiSourceAttribute(params Type[] sourceTypes)
    {
        ArgumentException.ThrowIfNullOrEmptyOrAnyNull(sourceTypes);
        SourceTypes = sourceTypes;
    }
}

/// <summary>
/// 用于标记不在Detour类中的方法是Detour方法。
/// <br/>若目标类不是静态类，则建议使用 <see cref="DetourMethodToAttribute{T}"/>。
/// <remarks>注意：在Detour类特性修饰的类中应用该特性会使Detour重复应用。</remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DetourMethodToAttribute : Attribute
{
    public readonly Type SourceType;

    /// <summary>
    /// 参数偏移量，设为负数以禁用参数偏移机制。
    /// </summary>
    public int ParamOffset { get; init; } = -1;

    public DetourMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

/// <summary>
/// 用于标记不在Detour类中的方法是Detour方法。
/// <remarks>注意：在Detour类特性修饰的类中应用该特性会使Detour重复应用。</remarks>
/// </summary>
public class DetourMethodToAttribute<T> : DetourMethodToAttribute
{
    public DetourMethodToAttribute() : base(typeof(T)) { }
}

/// <summary>
/// 用于标记由Detour类特性修饰的类中的某个方法，使自动应用逻辑不涉及该方法。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class NotDetourMethodAttribute : Attribute;

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

public sealed class TODetourHelper : IResourceLoader
{
    public sealed class DetourSet : IEnumerable<Hook>
    {
        private readonly Dictionary<Type, Dictionary<MethodBase, List<Hook>>> _data = [];

        public void Add(Hook hook)
        {
            ArgumentNullException.ThrowIfNull(hook);
            Type targetType = hook.Source.DeclaringType;
            if (!_data.ContainsKey(targetType))
                _data[targetType] = [];
            if (_data[targetType].TryGetValue(hook.Source, out List<Hook> value))
                value.Add(hook);
            else
                _data[targetType][hook.Source] = [hook];
        }

        public bool Remove(Hook hook)
        {
            ArgumentNullException.ThrowIfNull(hook);
            Type targetType = hook.Source.DeclaringType;
            if (!_data.TryGetValue(targetType, out Dictionary<MethodBase, List<Hook>> methodHooks))
                return false;
            foreach ((MethodBase source, List<Hook> hooks) in methodHooks)
            {
                int index = hooks.IndexOf(hook);
                if (index >= 0)
                {
                    hooks[index].Undo();
                    hooks.RemoveAt(index);
                    if (hooks.Count == 0)
                        methodHooks.Remove(source);
                    if (methodHooks.Count == 0)
                        _data.Remove(targetType);
                    return true;
                }
            }
            return false;
        }

        public void RemoveAll(Predicate<Hook> match)
        {
            ArgumentNullException.ThrowIfNull(match);
            foreach ((Type sourceType, Dictionary<MethodBase, List<Hook>> methodHooks) in _data)
            {
                foreach ((MethodBase source, List<Hook> hooks) in methodHooks)
                {
                    foreach (Hook hook in hooks)
                    {
                        if (match(hook))
                        {
                            hook.Undo();
                            hooks.Remove(hook);
                        }
                    }
                    if (hooks.Count == 0)
                        methodHooks.Remove(source);
                    if (methodHooks.Count == 0)
                        _data.Remove(sourceType);
                }
            }
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

    internal static readonly DetourSet Detours = [];

    void IResourceLoader.PostSetupContent()
    {
        Detours.Clear();

        foreach ((Type type, DetourClassToAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<DetourClassToAttribute>())
            TODetourUtils.ApplyAllStaticMethodDetoursOfType(type, attribute.SourceType);

        foreach ((Type type, DetourClassTo_MultiSourceAttribute attribute) in TOReflectionUtils.GetTypesWithAttribute<DetourClassTo_MultiSourceAttribute>())
        {
            Type[] sourceTypes = attribute.SourceTypes;
            foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
                TODetourUtils.ApplyTypedStaticMethodDetour(detour, sourceTypes);
        }

        foreach ((MethodInfo detour, DetourMethodToAttribute attribute) in TOReflectionUtils.GetMethodsWithAttribute<DetourMethodToAttribute>())
            TODetourUtils.ApplyStaticMethodDetour(detour, attribute.SourceType, attribute.ParamOffset < 0 ? null : attribute.ParamOffset);

        foreach (ITODetourProvider detourProvider in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITODetourProvider>().OrderByDescending(d => d.LoadPriority))
            detourProvider.ApplyDetour();
    }

    void IResourceLoader.OnModUnload() => Detours.Clear();
}

public static partial class TODetourUtils
{
    private const string DefaultPrefix = "Detour_";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = """^{0}(?<methodName>[\S]*?)(?:__[\S]*)?$""";
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern2 = """^{0}(?<typeName>[\S]*?)__(?<methodName>[\S]*?)(?:__[\S]*)?$""";

    private static readonly Regex _defaultDetourNameRegex = GetDefaultDetourNameRegex();
    private static readonly Regex _defaultDetourNameRegex2 = GetDefaultDetourNameRegex2();

    [GeneratedRegex("""^Detour_(?<methodName>[\S]*?)(?:__[\S]*)?$""")]
    private static partial Regex GetDefaultDetourNameRegex();

    [GeneratedRegex("""^Detour_(?<typeName>[\S]*?)__(?<methodName>[\S]*?)(?:__[\S]*)?$""")]
    private static partial Regex GetDefaultDetourNameRegex2();

    /// <summary>
    /// 尝试解析提供的Detour方法名，获取其应用的源方法名。
    /// <para/>逻辑：
    /// <br/> 1. 尝试获取detour参数的 <see cref="CustomDetourPrefixAttribute"/> 特性，如果存在，使用其提供的前缀，否则使用 <see cref="DefaultPrefix"/> 作为前缀，命名为prefix。
    /// <br/> 2. 尝试将方法名与 <c>{prefix}{methodName}[__{paramName}]</c> 进行匹配。
    /// <br/> 3. 如果匹配成功，输出methodName。
    /// </summary>
    /// <param name="detour">Detour方法，必须是具名方法。</param>
    /// <param name="sourceName">输出的源方法名。</param>
    /// <returns>解析是否成功。</returns>
    public static bool EvaluateDetourName(MethodInfo detour, [NotNullWhen(true)] out string sourceName)
    {
        string prefix = detour.Attribute<CustomDetourPrefixAttribute>()?.Prefix;
        Match match = prefix is null ? _defaultDetourNameRegex.Match(detour.Name) : Regex.Match(detour.Name, string.Format(Pattern, prefix));
        if (match.Success)
        {
            sourceName = match.Groups["methodName"].Value;
            return true;
        }
        sourceName = null;
        return false;
    }

    /// <summary>
    /// 尝试解析提供的Detour方法名，获取其应用的源类型名和源方法名。
    /// <para/>逻辑：
    /// <br/> 1. 尝试获取detour参数的 <see cref="CustomDetourPrefixAttribute"/> 特性，如果存在，使用其提供的前缀，否则使用 <see cref="DefaultPrefix"/> 作为前缀，命名为prefix。
    /// <br/> 2. 尝试将方法名与 <c>{prefix}{typeName}__{methodName}[__{paramName}]</c> 进行匹配。
    /// <br/> 3. 如果匹配成功，输出typeName和methodName。
    /// </summary>
    /// <param name="detour">Detour方法，必须是具名方法。</param>
    /// <param name="sourceTypeName">输出的源类型名</param>
    /// <param name="sourceMethodName">输出的源方法名。</param>
    /// <returns>解析是否成功。</returns>
    public static bool EvaluateTypedDetourName(MethodInfo detour, [NotNullWhen(true)] out string sourceTypeName, [NotNullWhen(true)] out string sourceMethodName)
    {
        string prefix = detour.Attribute<CustomDetourPrefixAttribute>()?.Prefix ?? DefaultPrefix;
        Match match = prefix is null ? _defaultDetourNameRegex2.Match(detour.Name) : Regex.Match(detour.Name, string.Format(Pattern2, prefix));
        if (match.Success)
        {
            sourceTypeName = match.Groups["typeName"].Value;
            sourceMethodName = match.Groups["methodName"].Value;
            return true;
        }
        sourceTypeName = null;
        sourceMethodName = null;
        return false;
    }

    /// <summary>
    /// 尝试将Detour应用到指定源方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify(MethodBase source, MethodInfo detour)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(detour);
        DetourConfig detourConfig = detour.Attribute<CustomDetourConfigAttribute>()?.DetourConfig;
        Hook hook = detourConfig is not null ? new(source, detour, detourConfig, true) : new(source, detour, true);
        TODetourHelper.Detours.Add(hook);
        return hook;
    }

    /// <summary>
    /// 尝试将Detour应用到指定源方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TDelegate>(MethodBase source, TDelegate detour) where TDelegate : Delegate
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(detour);
        DetourConfig detourConfig = detour.Method.Attribute<CustomDetourConfigAttribute>()?.DetourConfig;
        Hook hook = detourConfig is not null ? new(source, detour, detourConfig, true) : new(source, detour, true);
        TODetourHelper.Detours.Add(hook);
        return hook;
    }

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TSource, TTarget>(TSource source, TTarget detour) where TSource : Delegate where TTarget : Delegate => Modify(source.Method, detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify(Type sourceType, string sourceMethodName, MethodInfo detour) => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, TDelegate detour) where TDelegate : Delegate => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<T>(string sourceMethodName, MethodInfo detour) => Modify(typeof(T), sourceMethodName, detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="paramOffset">Detour方法参数偏移量。
    /// <br/>应设置为目标方法第一个参数在Detour方法中的索引。
    /// <br/>例如，若Detour方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。
    /// <br/>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify(Type sourceType, string sourceMethodName, int paramOffset, MethodInfo detour) => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags, detour.ParameterTypes[paramOffset..]), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="paramOffset">Detour方法参数偏移量。
    /// <br/>应设置为目标方法第一个参数在Detour方法中的索引。
    /// <br/>例如，若Detour方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。
    /// <br/>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, int paramOffset, TDelegate detour) where TDelegate : Delegate => Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags, detour.Method.ParameterTypes[paramOffset..]), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="paramOffset">Detour方法参数偏移量。
    /// <br/>应设置为目标方法第一个参数在Detour方法中的索引。
    /// <br/>例如，若Detour方法有 <c>orig</c> 和 <c>self</c> 参数，则应设置为 <c>2</c>；若都没有，则应设置为 <c>0</c>。
    /// <br/>只应设置为 <c>0</c>、<c>1</c> 或 <c>2</c>。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<T>(string sourceMethodName, int paramOffset, MethodInfo detour) => Modify(typeof(T), sourceMethodName, paramOffset, detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="hasThis">目标方法是否为实例方法（有 <see langword="this"/> 指针）。
    /// <br/>会影响获取方法时的参数偏移量（<see langword="true"/> 为2，反之为1）和 <c>bindingAttr</c> 实参。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify(Type sourceType, string sourceMethodName, bool hasThis, MethodInfo detour) =>
        Modify(hasThis ? sourceType.GetMethod(sourceMethodName, TOReflectionUtils.InstanceBindingFlags, detour.ParameterTypes[2..])
            : sourceType.GetMethod(sourceMethodName, TOReflectionUtils.StaticBindingFlags, detour.ParameterTypes[1..]), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="hasThis">目标方法是否为实例方法（有 <see langword="this"/> 指针）。
    /// <br/>会影响获取方法时的参数偏移量（<see langword="true"/> 为2，反之为1）和 <c>bindingAttr</c> 实参。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<TDelegate>(Type sourceType, string sourceMethodName, bool hasThis, TDelegate detour) where TDelegate : Delegate =>
        Modify(hasThis ? sourceType.GetMethod(sourceMethodName, TOReflectionUtils.InstanceBindingFlags, detour.Method.ParameterTypes[2..])
            : sourceType.GetMethod(sourceMethodName, TOReflectionUtils.StaticBindingFlags, detour.Method.ParameterTypes[1..]), detour);

    /// <summary>
    /// 尝试在指定类中获取对应名称方法，并将Detour应用到获取的方法上。
    /// </summary>
    /// <param name="hasThis">目标方法是否为实例方法（有 <see langword="this"/> 指针）。
    /// <br/>会影响获取方法时的参数偏移量（<see langword="true"/> 为2，反之为1）和 <c>bindingAttr</c> 实参。
    /// </param>
    /// <returns>创建的Hook对象。</returns>
    public static Hook Modify<T>(string sourceMethodName, bool hasThis, MethodInfo detour) => Modify(typeof(T), sourceMethodName, hasThis, detour);

    public static Hook ApplyStaticMethodDetour(MethodInfo detour, Type sourceType, int? paramOffset = null)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return null;
        if (detour.TryGetAttribute(out CustomDetourSourceAttribute attribute))
            return Modify(attribute.Source, detour);
        if (EvaluateDetourName(detour, out string sourceName))
            return paramOffset is null ? Modify(sourceType, sourceName, detour)
                : Modify(sourceType, sourceName, paramOffset.Value, detour);
        return null;
    }

    public static Hook ApplyTypedStaticMethodDetour(MethodInfo detour, Type[] sourceTypes)
    {
        if (detour.HasAttribute<NotDetourMethodAttribute>())
            return null;
        if (detour.TryGetAttribute(out CustomDetourSourceAttribute attribute))
            return Modify(attribute.Source, detour);
        if (EvaluateTypedDetourName(detour, out string sourceTypeName, out string sourceMethodName))
        {
            Type sourceType = sourceTypes.AsValueEnumerable().FirstOrDefault(t => t.Name == sourceTypeName);
            if (sourceType is not null)
                return Modify(sourceType.GetMethod(sourceMethodName, TOReflectionUtils.UniversalBindingFlags), detour);
        }
        return null;
    }

    public static void ApplyAllStaticMethodDetoursOfType(Type type, Type sourceType)
    {
        foreach (MethodInfo detour in type.GetRealMethods(TOReflectionUtils.StaticBindingFlags))
            ApplyStaticMethodDetour(detour, sourceType);
    }
}