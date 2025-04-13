using System;
using System.Reflection;

namespace Transoceanic.Core;

public interface ITODetourProvider
{
    /// <summary>
    /// 在方法中调用 <see cref="TOHookHelper.ModifyMethodWithDetour(MethodBase, Delegate)"/> 以修改指定方法。
    /// </summary>
    public abstract void ModifyMethods();
}
