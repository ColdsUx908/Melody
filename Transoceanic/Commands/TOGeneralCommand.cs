using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Transoceanic.GlobalInstances;
using Transoceanic.IL;
using Transoceanic.Localization;

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

public class CommandCallInfo
{
    public CommandCaller Caller { get; }

    public CommandType CommandType { get; }

    public string Command { get; }

    public string[] Args { get; }

    public CommandCallInfo(CommandType commandType, string command, CommandCaller caller, string[] args)
    {
        CommandType = commandType;
        Command = command;
        Caller = caller;
        Args = args;
    }

    public CommandCallInfo(ITOCommand commandInstance, CommandCaller caller, string[] args)
    {
        CommandType = commandInstance.Type;
        Command = commandInstance.Command;
        Caller = caller;
        Args = args;
    }
}

public class CommandArgumentException : Exception
{
    public CommandCallInfo CallInfo { get; }

    public CommandArgumentException(CommandCallInfo callInfo) : base() => CallInfo = callInfo;

    public CommandArgumentException(CommandCallInfo callInfo, string message = "") : base(message) => CallInfo = callInfo;

    public CommandArgumentException(CommandCallInfo callInfo, string message, Exception innerException) : base(message, innerException) => CallInfo = callInfo;

    public CommandArgumentException(ITOCommand commandInstance, CommandCaller caller, string[] args) : this(new CommandCallInfo(commandInstance, caller, args)) { }

    public CommandArgumentException(ITOCommand commandInstance, CommandCaller caller, string[] args, string message) : this(new CommandCallInfo(commandInstance, caller, args), message) { }

    public CommandArgumentException(ITOCommand commandInstance, CommandCaller caller, string[] args, string message, Exception innerException) : this(new CommandCallInfo(commandInstance, caller, args), message, innerException) { }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"Command: {CallInfo.Command}");
        builder.AppendLine($"Command Type: {CallInfo.CommandType}");
        builder.AppendLine($"Calling Player Index and Name: {CallInfo.Caller.Player.whoAmI}, {CallInfo.Caller.Player.name}");
        builder.AppendLine($"Arguments: {string.Join(' ', CallInfo.Args)}");
        builder.AppendLine($"Message: {Message}");
        return builder.ToString();
    }
}

public class TOGeneralChatCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "/chat"; //需要使用的指令是"//chat"（两个斜杠）

    private const string localizationPrefix = TOMain.ModLocalizationPrefix + "Commands.GeneralCommand.";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            caller.ReplyLocalizedText(localizationPrefix + "Helper");
        switch (args[0].ToLower())
        {
            case "help":
            case "h":
            case "?":
                caller.ReplyLocalizedText(localizationPrefix + "Helper");
                break;
            case "redo":
                CommandCallInfo commandCallInfo = caller.Player.Ocean().CommandCallInfo;
                if (commandCallInfo is not null && commandCallInfo.CommandType == CommandType.Chat)
                    TOCommandHelper.TryExecute(caller, commandCallInfo.Command, CommandType.Chat, commandCallInfo.Args);
                break;
            default:
                TOCommandHelper.TryExecute(caller, args[0], CommandType.Chat, args[1..]);
                break;
        }
    }
}

public class TOCommandHelper : ITOLoader
{
    internal static Dictionary<string, ITOCommand> CommandSet { get; } = [];

    private const string localizationPrefix = TOMain.ModLocalizationPrefix + "Commands.GeneralCommand.";

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

    public static void TryExecute(CommandCaller caller, string command, CommandType commandType, string[] args)
    {
        if (CommandSet.TryGetValue(command, out ITOCommand value) && value.Type.HasFlag(commandType))
        {
            try
            {
                caller.Player.Ocean().CommandCallInfo = new(commandType, command, caller, args);
                value.Action(caller, args);
            }
            catch (CommandArgumentException e)
            {
                caller.ReplyLocalizedTextWith(localizationPrefix + "InvalidArguments", Color.Red, e);
                value.Help(caller, args);
            }
        }
        else
            caller.ReplyLocalizedText(localizationPrefix + "Helper2");
    }
}
