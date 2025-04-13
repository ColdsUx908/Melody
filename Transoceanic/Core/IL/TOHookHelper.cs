using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonoMod.RuntimeDetour;

namespace Transoceanic.Core;

public class TOHookHelper : ITOLoader
{
    private static List<Hook> detours;

    /// <summary>
    /// 使用Hook修改指定方法。
    /// </summary>
    /// <param name="methodToModify"></param>
    /// <param name="detourMethod"></param>
    public static void ModifyMethodWithDetour(MethodBase methodToModify, Delegate detourMethod)
    {
        Hook hook = new(methodToModify, detourMethod);
        hook.Apply();
        detours.Add(hook);
    }

    void ITOLoader.PostSetupContent()
    {
        detours = [];
        foreach (ITODetourProvider instance in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITODetourProvider>())
            instance.ModifyMethods();
    }

    void ITOLoader.OnModUnload()
    {
        foreach (Hook hook in detours)
            hook.Undo();
        detours.Clear();
        detours = null;
    }
}
