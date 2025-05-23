using Terraria.ModLoader;

namespace Transoceanic.Commands;

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