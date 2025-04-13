namespace Transoceanic.Core;

public enum LoadMethodType
{
    Load,
    PostSetupContent,
    OnModUnload,
    UnLoad
}

/// <summary>
/// 资源加载器接口。
/// </summary>
public interface ITOLoader
{
    /// <summary>
    /// 在本Mod加载时调用。
    /// <br/>不支持其他程序集实现。
    /// </summary>
    internal virtual void Load() { }

    /// <summary>
    /// 内容加载完成后调用。
    /// <br/>其他程序集类型只应覆写此方法。
    /// </summary>
    public virtual void PostSetupContent() { }

    /// <summary>
    /// 在Mod卸载前调用。
    /// </summary>
    public virtual void OnModUnload() { }

    /// <summary>
    /// 在Mod卸载时调用。
    /// <br/>不支持其他程序集实现。
    /// </summary>
    internal virtual void UnLoad() { }

    /// <summary>
    /// 加载优先级，越大越先加载。
    /// <br/>如对加载顺序不敏感，不要覆写此属性。
    /// </summary>
    /// <param name="loadMethodType">加载方法类型</param>
    public virtual double LoadPriority(LoadMethodType loadMethodType) => 0.0;
}
