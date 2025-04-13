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

    public abstract void Action(CommandCaller caller, string[] args);

    public virtual string Usage => "";

    public virtual string Description => "";



    public static Dictionary<(string Command, CommandType Type), Action<CommandCaller, string[]>> CommandSet = [];

    void ITOLoader.PostSetupContent()
    {
        foreach (ITOCommand instance in TOReflectionUtils.GetTypeInstancesDerivedFrom<ITOCommand>())
        {
            if (!instance.Command.Equals("help", StringComparison.CurrentCultureIgnoreCase)) //不加载关键字为"help"的命令
                CommandSet.TryAdd((instance.Command, instance.Type), instance.Action);
        }
    }

    void ITOLoader.OnModUnload()
    {
        CommandSet.Clear();
        CommandSet = null;
    }
}
