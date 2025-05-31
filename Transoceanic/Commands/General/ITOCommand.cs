using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Transoceanic.IL;

namespace Transoceanic.Commands;

public interface ITOCommand
{
    public abstract CommandType Type { get; }

    public abstract string Command { get; }

    /// <summary>
    /// 执行命令的函数。
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="args">参数数组。</param>
    public abstract void Action(CommandCaller caller, string[] args);

    public virtual void Help(CommandCaller caller, string[] args) { }
}

public class TOCommandHelper : ITOLoader
{
    internal static Dictionary<string, ITOCommand> CommandSet { get; } = [];

    void ITOLoader.PostSetupContent()
    {
        foreach (ITOCommand commandContainer in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOCommand>())
        {
            if (!commandContainer.Command.Equals("help", StringComparison.CurrentCultureIgnoreCase)) //不加载关键字为"help"的命令
                CommandSet[commandContainer.Command.ToLower()] = commandContainer;
        }
    }

    void ITOLoader.OnModUnload()
    {
        CommandSet.Clear();
    }
}