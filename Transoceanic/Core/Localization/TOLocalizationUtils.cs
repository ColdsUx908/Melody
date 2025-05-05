using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Transoceanic.Core.Localization;

public static partial class TOLocalizationUtils
{
    public static string GetTextFormat(string key, object[] args) => Language.GetText(key).Format(args);

    public static void ChatLiteralText(string text, Color? textColor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(text, textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), textColor ?? Color.White);
    }

    public static void ChatLiteralText(string text, Color? textColor = null, params Player[] receivers)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(text, textColor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(text), textColor ?? Color.White, player.whoAmI);
        }
    }

    public static void ChatLocalizedText(string key, Color? textColor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), textColor ?? Color.White);
    }

    public static void ChatLocalizedText(string key, Color? textColor = null, params Player[] receivers)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromKey(key), textColor ?? Color.White, player.whoAmI);
        }
    }

    public static void ChatLocalizedTextWith(string key, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(GetTextFormat(key, args), textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(GetTextFormat(key, args)), textColor ?? Color.White);
    }

    public static void ChatLocalizedTextWith(string key, Player receiver, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(GetTextFormat(key, args), textColor ?? Color.White);
        else
            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(GetTextFormat(key, args)), textColor ?? Color.White, receiver.whoAmI);
    }

    public static void ChatLocalizedTextWith(string key, Player[] receivers, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(GetTextFormat(key, args), textColor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(GetTextFormat(key, args)), textColor ?? Color.White, player.whoAmI);
        }
    }

    public static void ChatStringBuilder(StringBuilder stringBuilder, Color? textcolor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(stringBuilder.ToString(), textcolor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(stringBuilder.ToString()), textcolor ?? Color.White);
    }

    public static void ReplyLocalizedText(this CommandCaller commandCaller, string key, Color? textColor = null) => commandCaller.Reply(Language.GetTextValue(key), textColor ?? Color.White);

    public static void ReplyLocalizedTextWith(this CommandCaller commandCaller, string key, Color? textColor = null, params object[] args) => commandCaller.Reply(GetTextFormat(key, args), textColor ?? Color.White);

    public static void ReplyStringBuilder(this CommandCaller commandCaller, StringBuilder stringBuilder, Color? textColor = null) => commandCaller.Reply(stringBuilder.ToString(), textColor ?? Color.White);

    public static void AppendLocalized(this StringBuilder stringBuilder, string key) => stringBuilder.Append(Language.GetTextValue(key));

    public static void AppendLocalizedWith(this StringBuilder stringBuilder, string key, params object[] args) => stringBuilder.Append(GetTextFormat(key, args));

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

    public static void ChatDebugErrorMessage(string key, Player receiver, params object[] args)
    {
        ChatLiteralText("[Transoceanic ERROR]", TOMain.TODebugErrorColor, receiver);
        ChatLocalizedTextWith(TOMain.DebugPrefix + key, receiver, TOMain.TODebugErrorColor, args);
        ChatLocalizedText(TOMain.DebugErrorMessageKey, TOMain.TODebugErrorColor, receiver);
    }

    public static void ChatDebugErrorMessage(string key, Player[] receivers, params object[] args)
    {
        ChatLiteralText("[Transoceanic ERROR]", TOMain.TODebugErrorColor, receivers);
        ChatLocalizedTextWith(TOMain.DebugPrefix + key, receivers, TOMain.TODebugErrorColor, args);
        ChatLocalizedText(TOMain.DebugErrorMessageKey, TOMain.TODebugErrorColor, receivers);
    }

    public static void ReplyDebugErrorMessage(this CommandCaller caller, string key, object[] args)
    {
        caller.Reply("[Transoceanic ERROR", TOMain.TODebugErrorColor);
        caller.ReplyLocalizedTextWith(key, TOMain.TODebugErrorColor, args);
        caller.ReplyLocalizedText(TOMain.DebugErrorMessageKey, TOMain.TODebugErrorColor);
    }
}