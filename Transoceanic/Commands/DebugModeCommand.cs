namespace Transoceanic.Commands;

/// <summary>
/// 开启或关闭调试模式的命令。
/// </summary>
public class DebugModeCommand : ITOCommand
{
    public CommandType Type => CommandType.Chat;

    public string Command => "debug";

    private const string commandPrefix = TOMain.ModLocalizationPrefix + "Commands.DebugMode.";

    public void Action(CommandCaller caller, string[] args)
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
                    throw new CommandArgumentException(this, caller, args);
            }
        }
    }
}