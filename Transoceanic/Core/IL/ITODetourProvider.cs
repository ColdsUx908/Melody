using System;
using System.Collections.Generic;
using System.Reflection;

namespace Transoceanic.Core.IL;

public interface ITODetourProvider
{
    public abstract Dictionary<MethodInfo, Delegate> DetoursToApply { get; }
}
