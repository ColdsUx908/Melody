using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Transoceanic.Core.Localization;

namespace Transoceanic.Commands;

public class BroadCastCommand : ITOCommand
{
    CommandType ITOCommand.Type => CommandType.Chat;

    string ITOCommand.Command => "say";

    void ITOCommand.Action(CommandCaller caller, string[] args) => TOLocalizationUtils.ChatLiteralText(string.Join(' ', args));
}
