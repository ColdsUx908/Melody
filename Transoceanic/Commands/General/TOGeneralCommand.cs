using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Transoceanic.Core.Localization;
using Transoceanic.GlobalInstances;

namespace Transoceanic.Commands;

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
                TryExecute(caller, commandCallInfo.Command, commandCallInfo.Args);
                break;
            default:
                TryExecute(caller, args[0], args[1..]);
                break;
        }
    }

    private static void TryExecute(CommandCaller caller, string command, string[] args)
    {
        if (TOCommandHelper.CommandSet.TryGetValue(command, out ITOCommand value) && value.Type.HasFlag(CommandType.Chat))
        {
            try
            {
                caller.Player.Ocean().CommandCallInfo = new CommandCallInfo(CommandType.Chat, command, caller, args);
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
