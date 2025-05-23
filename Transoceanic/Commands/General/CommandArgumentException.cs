using System;
using System.Runtime.Serialization;
using System.Text;
using Terraria.ModLoader;

namespace Transoceanic.Commands;

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
