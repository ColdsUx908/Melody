using Terraria.ModLoader;
using Transoceanic.Core.Localization;

namespace Transoceanic.Commands;

public class BroadCastCommand : ITOCommand
{
    public CommandType Type => CommandType.Chat;

    public string Command => "say";

    public void Action(CommandCaller caller, string[] args) => TOLocalizationUtils.ChatLiteralText(string.Join(' ', args));
}
