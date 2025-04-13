using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Transoceanic.Core;

public static partial class TOLocalizationUtils
{
    public static void ChatLocalizedText(string key, Color? textColor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
        else if (Main.netMode is NetmodeID.Server or NetmodeID.MultiplayerClient)
            ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), textColor ?? Color.White);
    }

    public static void ChatLocalizedText(string key, Color? textColor = null, params Player[] receivers)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
        else if (Main.netMode is NetmodeID.Server or NetmodeID.MultiplayerClient)
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromKey(key), textColor ?? Color.White, player.whoAmI);
    }

    public static void ChatLocalizedTextWith(string key, Color? textColor = null, params object[] args)
    {
        Color textColorValue = textColor ?? Color.White;
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValueWith(key, args), textColorValue);
        else if (Main.netMode is NetmodeID.Server or NetmodeID.MultiplayerClient)
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Language.GetText(key).Format(args)), textColorValue);
    }

    public static void ChatLocalizedTextWith(string key, Player receiver, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValueWith(key, args), textColor ?? Color.White);
        else if (Main.netMode is NetmodeID.Server or NetmodeID.MultiplayerClient)
            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(Language.GetText(key).Format(args)), textColor ?? Color.White, receiver.whoAmI);
    }

    public static void ChatLocalizedTextWith(string key, Player[] receivers, Color? textColor = null, params object[] args)
    {
        Color textColorValue = textColor ?? Color.White;
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValueWith(key, args), textColorValue);
        else if (Main.netMode is NetmodeID.Server or NetmodeID.MultiplayerClient)
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(Language.GetText(key).Format(args)), textColorValue, player.whoAmI);
        }
    }

    public static void ChatStringBuilder(StringBuilder stringBuilder, Color? textcolor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(stringBuilder.ToString(), textcolor ?? Color.White);
        else if (Main.netMode is NetmodeID.Server or NetmodeID.MultiplayerClient)
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(stringBuilder.ToString()), textcolor ?? Color.White);
    }

    public static void ReplyLocalizedText(this CommandCaller commandCaller, string key, Color? textColor = null) => commandCaller.Reply(Language.GetTextValue(key), textColor ?? Color.White);

    public static void ReplyLocalizedTextWith(this CommandCaller commandCaller, string key, Color? textColor = null, params object[] args) => commandCaller.Reply(Language.GetText(key).Format(args), textColor ?? Color.White);

    public static void ReplyStringBuilder(this CommandCaller commandCaller, StringBuilder stringBuilder, Color? textColor = null) => commandCaller.Reply(stringBuilder.ToString(), textColor ?? Color.White);

    public static void AppendLocalized(this StringBuilder stringBuilder, string key) => stringBuilder.Append(Language.GetTextValue(key));

    public static void AppendLocalizedTO(this StringBuilder stringBuilder, string key) => stringBuilder.Append(Language.GetTextValue(TOMain.ModLocalizationPrefix + key));

    public static StringBuilder CreateWithDebugHeader()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append("[Transoceanic ERROR]\n");
        return stringBuilder;
    }

    public static void AppendTODebugErrorMessage(this StringBuilder stringBuilder)
    {
        stringBuilder.Append('\n');
        stringBuilder.AppendLocalized(TOMain.DebugErrorMessageKey);
    }
}