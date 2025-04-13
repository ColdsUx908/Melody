using CalamityMod.Balancing;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Transoceanic.Core;
using Transoceanic.Commands;

namespace TransoceanicCalamity.Commands;

/// <summary>
/// 获取客户端的潜伏攻击伤害倍率。
/// </summary>
public class StealthStrikeFactorCommand_TO : ITOCommand
{
    CommandType ITOCommand.Type => CommandType.Chat;

    string ITOCommand.Command => "stealthfactor";

    private const string commandPrefix = TOCMain.ModLocalizationPrefix + "Commands.StealthStrikeFactor.";

    string ITOCommand.Usage => "/stealthfactor <get/set> [new value]";

    string ITOCommand.Description => Language.GetTextValue(commandPrefix + "Description");

    void ITOCommand.Action(CommandCaller caller, string[] args)
    {
        if (caller.Player != Main.LocalPlayer)
            return;

        try
        {
            switch (args[0])
            {
                case "get":
                case "g":
                case "0":
                    caller.ReplyLocalizedTextWith(
                        commandPrefix + "GetReply",
                        Color.White,
                        BalancingConstants.UniversalStealthStrikeDamageFactor);
                    break;
                case "set":
                case "s":
                case "1":
                    caller.ReplyLocalizedTextWith(
                        commandPrefix + "GetReply",
                        Color.White,
                        BalancingConstants.UniversalStealthStrikeDamageFactor = float.Parse(args[1]));
                    break;
                default:
                    break;
            }
        }
        catch
        {
            caller.ReplyLocalizedText(TOCMain.ModLocalizationPrefix + "Commands.StealthStrikeFactor.InvalidReply");
        }
    }
}
