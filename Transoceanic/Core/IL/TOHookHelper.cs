using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Core.IL;

public class TOHookHelper : ITOLoader
{
    private static List<Hook> _detours = [];

    void ITOLoader.PostSetupContent()
    {
        _detours = [];
        foreach (ITODetourProvider detourProvider in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITODetourProvider>())
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
