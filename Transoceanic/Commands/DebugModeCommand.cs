namespace Transoceanic.Commands;

/// <summary>
/// 开启或关闭调试模式的命令。
/// </summary>
public sealed class DebugModeCommand : TOCommand, ILocalizationPrefix
{
    public override CommandType Type => CommandType.Chat;

    public override string Command => "debug";

    public string LocalizationPrefix => TOMain.ModLocalizationPrefix + "Commands.DebugMode";

    public override void Action(CommandCaller caller, string[] args)
    {
        if (args.Length == 0)
            caller.ReplyLocalizedTextFormat(this, "Status", Color.White, TOMain.DEBUG);
        else
        {
            switch (args[0])
            {
                case "on":
                case "true":
                case "1":
                    TOMain.DEBUG = true;
                    caller.ReplyLocalizedText(this, "On");
                    break;
                case "off":
                case "false":
                case "0":
                    TOMain.DEBUG = false;
                    caller.ReplyLocalizedText(this, "Off");
                    break;
                default:
                    throw new CommandArgumentException(this, caller, args);
            }
        }
    }
}