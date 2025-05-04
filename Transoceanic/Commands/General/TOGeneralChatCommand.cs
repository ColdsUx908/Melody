using System;
using Terraria.ModLoader;
using Transoceanic.Core.Localization;
using Transoceanic.GlobalInstances;

namespace Transoceanic.Commands;

public class TOGeneralChatCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "//1"; //需要使用的指令是"///1"（三个斜杠）

    private const string commandPrefix = TOMain.ModLocalizationPrefix + "Commands.GeneralCommand.";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            caller.ReplyLocalizedText(commandPrefix + "Helper");
        switch (args[0].ToLower())
        {
            case "help":
            case "h":
            case "?":
                caller.ReplyLocalizedText(commandPrefix + "Helper");
                break;
            case "redo":
                CommandCallInfo commandCallInfo = caller.Player.Ocean().CommandCallInfo;
                TryExecute(caller, commandCallInfo.Command, commandCallInfo.Args);
                break;
            default:
                TryExecute(caller, args[0], args[1..]);
                break;
        }
    }

    private static void TryExecute(CommandCaller caller, string command, string[] args)
    {
        if (TOCommandHelper.CommandSet.TryGetValue(command, out (CommandType Type, Action<CommandCaller, string[]> Action) value) && value.Type.HasFlag(CommandType.Chat))
        {
            caller.Player.Ocean().CommandCallInfo = new CommandCallInfo(caller, CommandType.Chat, command, args);
            value.Action?.Invoke(caller, args);
        }
        else
            caller.ReplyLocalizedText(TOMain.ModLocalizationPrefix + "Commands.GeneralCommand.Helper2");
    }
}
