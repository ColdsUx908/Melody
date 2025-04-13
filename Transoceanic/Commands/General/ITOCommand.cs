using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ModLoader;
using Transoceanic.Core;

namespace Transoceanic.Commands;

public interface ITOCommand : ITOLoader
{
    public abstract CommandType Type { get; }

    public abstract string Command { get; }

    /// <summary>
    /// 执行命令的函数。
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="args">参数数组。</param>
    public abstract void Action(CommandCaller caller, string[] args);

    public virtual string Usage => "";

    public virtual string Description => "";

    public static Dictionary<(string Command, CommandType Type), Action<CommandCaller, string[]>> CommandSet { get; private set; }

    void ITOLoader.PostSetupContent()
    {
        CommandSet = [];
        foreach (ITOCommand commandContainer in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOCommand>())
        {
            if (!commandContainer.Command.Equals("help", StringComparison.CurrentCultureIgnoreCase)) //不加载关键字为"help"的命令
                CommandSet.TryAdd((commandContainer.Command.ToLower(), commandContainer.Type), commandContainer.Action);
        }
    }

    void ITOLoader.OnModUnload()
    {
        CommandSet.Clear();
        CommandSet = null;
    }
}
