using System;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Transoceanic.Core.Localization;

namespace Transoceanic.Commands;

/// <summary>
/// 开启或关闭调试模式的命令。
/// </summary>
public class DebugModeCommand : ITOCommand
{
    CommandType ITOCommand.Type => CommandType.Chat;

    string ITOCommand.Command => "debug";

    private const string commandPrefix = TOMain.ModLocalizationPrefix + "Commands.DebugMode.";

    void ITOCommand.Action(CommandCaller caller, string[] args)
    {
        try
        {
            if (args.Length == 0)
                caller.ReplyLocalizedTextWith(commandPrefix + "Status", Color.White, TOMain.DEBUG);
            else
            {
                switch (args[0])
                {
                    case "on":
                    case "1":
                        TOMain.DEBUG = true;
                        caller.ReplyLocalizedText(commandPrefix + "On");
                        break;
                    case "off":
                    case "0":
                        TOMain.DEBUG = false;
                        caller.ReplyLocalizedText(commandPrefix + "Off");
                        break;
                    default:
                        throw new ArgumentException("InvalidArgument");
                }
            }
        }
        catch
        {
            caller.ReplyLocalizedText(commandPrefix + "Invalid");
        }
    }
}
