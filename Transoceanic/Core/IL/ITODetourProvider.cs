using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Core.IL;

public interface ITODetourProvider
{
    public abstract Dictionary<MethodInfo, Delegate> DetoursToApply { get; }

    /// <summary>
    /// 加载优先级，越大越早加载。。
    /// </summary>
    public virtual decimal LoadPriority => 0m;
}

public class TODetourHelper : ITOLoader
{
    private static List<Hook> _detours = [];

    void ITOLoader.PostSetupContent()
    {
        _detours = [];
        foreach (ITODetourProvider detourProvider in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITODetourProvider>().OrderByDescending(k => k.LoadPriority))
        {
            foreach ((MethodInfo methodToModify, Delegate detourMethod) in detourProvider.DetoursToApply)
            {
                Hook hook = new(methodToModify, detourMethod);
                hook.Apply();
                _detours.Add(hook);
            }
        }
    }

    void ITOLoader.OnModUnload()
    {
        foreach (Hook hook in _detours)
            hook.Undo();
        _detours.Clear();
        _detours = null;
    }
}
