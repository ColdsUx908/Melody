using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Transoceanic.Core.Utilities;

namespace Transoceanic.RuntimeEditing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomManipulatorPrefixAttribute : Attribute
{
    public readonly string Prefix;

    public CustomManipulatorPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

/// <summary>
/// 用于标记不在IL编辑类中的方法是IL编辑方法。
/// <br/>若目标类不是静态类，则建议使用 <see cref="DetourMethodToAttribute{T}"/>。
/// <remarks>注意：在IL编辑类中应用该特性会使IL编辑方法重复应用。</remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ILEditingMethodToAttribute : Attribute
{
    public readonly Type SourceType;

    public ILEditingMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

/// <summary>
/// 用于标记不在IL编辑中的方法是IL方法。
/// <remarks>注意：在IL编辑类中应用该特性会使IL编辑方法重复应用，很可能会导致问题。</remarks>
/// </summary>
public class ILEditingMethodToAttribute<T> : DetourMethodToAttribute
{
    public ILEditingMethodToAttribute() : base(typeof(T)) { }
}

public sealed class TOILEditingHelper : IResourceLoader
{

    internal static readonly List<ILHook> Manipulators = [];

    void IResourceLoader.PostSetupContent()
    {
        Manipulators.Clear();

        foreach ((MethodInfo manipulator, ILEditingMethodToAttribute attribute) in TOReflectionUtils.GetMethodsWithAttribute<ILEditingMethodToAttribute>())
        {
            Type sourceType = attribute.SourceType;
            if (TOILEditingUtils.EvaluateManipulatorName(manipulator, out string sourceName))
                TOILEditingUtils.Modify(sourceType, sourceName, manipulator);
        }
    }

    void IResourceLoader.OnModUnload() => Manipulators.Clear();
}

public static partial class TOILEditingUtils
{
    [StringSyntax(StringSyntaxAttribute.Regex)] private const string Pattern = """^{0}(?<methodName>[\S]*?)(?:__[\S]*)?$""";

    private static readonly Regex _defaultManipulatorNameRegex = GetDefaultManipulatorNameRegex();

    [GeneratedRegex("""^IL_(?<methodName>[\S]*)$""")]
    private static partial Regex GetDefaultManipulatorNameRegex();

    public static bool EvaluateManipulatorName(MethodInfo detour, [NotNullWhen(true)] out string sourceName)
    {
        string prefix = detour.Attribute<CustomManipulatorPrefixAttribute>()?.Prefix;
        Match match = prefix is null ? _defaultManipulatorNameRegex.Match(detour.Name) : Regex.Match(detour.Name, string.Format(Pattern, prefix));
        if (match.Success)
        {
            sourceName = match.Groups["methodName"].Value;
            return true;
        }
        sourceName = null;
        return false;
    }

    public static ILHook Modify(MethodBase source, ILContext.Manipulator manip)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manip);
        return CreateILHook(source, manip, manip.Method);
    }

    public static ILHook Modify(MethodBase source, MethodInfo manipMethod)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manipMethod);
        try
        {
            return CreateILHook(source, manipMethod.CreateDelegate<ILContext.Manipulator>(), manipMethod);
        }
        catch (ArgumentException e)
        {
            throw new ArgumentException("The provided method's signature is not compatible with the delegate type ILContext.Manipulator.", nameof(manipMethod), e);
        }
    }

    public static ILHook Modify(Type sourceType, string sourceName, ILContext.Manipulator manip) => Modify(sourceType.GetMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), manip);

    public static ILHook Modify(Type sourceType, string sourceName, MethodInfo manip) => Modify(sourceType.GetMethod(sourceName, TOReflectionUtils.UniversalBindingFlags), manip);

    public static ILHook Modify<T>(string sourceName, ILContext.Manipulator manip) => Modify(typeof(T), sourceName, manip);

    public static ILHook Modify<T>(string sourceName, MethodInfo manip) => Modify(typeof(T), sourceName, manip);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ILHook CreateILHook(MethodBase source, ILContext.Manipulator manip, MethodInfo manipMethod)
    {
        DetourConfig detourConfig = manipMethod.Attribute<CustomDetourConfigAttribute>()?.DetourConfig;
        ILHook hook = detourConfig is not null ? new ILHook(source, manip, detourConfig, true) : new ILHook(source, manip, true);
        TOILEditingHelper.Manipulators.Add(hook);
        return hook;
    }
}