using System;
using Terraria.ModLoader;
using Transoceanic.Core.Localization;

namespace Transoceanic.Commands;

public class TOGeneralChatCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "'1";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            caller.ReplyLocalizedText(TOMain.ModLocalizationPrefix + "Commands.GeneralCommand.Helper");
        else if (args[0].Equals("help", StringComparison.CurrentCultureIgnoreCase))
        {
        }
        else if (TOCommandHelper.CommandSet.TryGetValue((args[0].ToLower(), CommandType.Chat), out Action<CommandCaller, string[]> action))
            action?.Invoke(caller, args[1..]);
        else
            caller.ReplyLocalizedText(TOMain.ModLocalizationPrefix + "Commands.GeneralCommand.Helper2");
    }
}
