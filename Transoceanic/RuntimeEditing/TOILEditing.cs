using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Transoceanic.RuntimeEditing;

public static class TOILUtils
{
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ILHook CreateILHook(MethodBase source, ILContext.Manipulator manip, MethodInfo manipMethod)
    {
        DetourConfig detourConfig = manipMethod.Attribute<CustomDetourConfigAttribute>()?.DetourConfig;
        return detourConfig is not null ? new ILHook(source, manip, detourConfig, true) : new ILHook(source, manip, true);
    }
}