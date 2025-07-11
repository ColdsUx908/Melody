using Terraria.Chat;

namespace Transoceanic.Localization;

public static class TOLocalizationUtils
{
    public static void ChatLiteralText(string text, Color? textColor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(text, textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), textColor ?? Color.White);
    }

    public static void ChatLiteralText(string text, Color? textColor = null, params ReadOnlySpan<Player> receivers)
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

    public static void ChatLocalizedText(string key, Color? textColor = null, params ReadOnlySpan<Player> receivers)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextValue(key), textColor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromKey(key), textColor ?? Color.White, player.whoAmI);
        }
    }

    public static void ChatLocalizedTextFormat(string key, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextFormat(key, args), textColor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(Language.GetTextFormat(key, args)), textColor ?? Color.White);
    }

    public static void ChatLocalizedTextFormat(string key, Player receiver, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextFormat(key, args), textColor ?? Color.White);
        else
            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(Language.GetTextFormat(key, args)), textColor ?? Color.White, receiver.whoAmI);
    }

    public static void ChatLocalizedTextFormat(string key, Player[] receivers, Color? textColor = null, params object[] args)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(Language.GetTextFormat(key, args), textColor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(Language.GetTextFormat(key, args)), textColor ?? Color.White, player.whoAmI);
        }
    }

    public static void ChatStringBuilder(StringBuilder builder, Color? textcolor = null)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(builder.ToString(), textcolor ?? Color.White);
        else
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(builder.ToString()), textcolor ?? Color.White);
    }

    public static void ChatStringBuilder(StringBuilder builder, Color? textcolor = null, params ReadOnlySpan<Player> receivers)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(builder.ToString(), textcolor ?? Color.White);
        else
        {
            foreach (Player player in receivers)
                ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(builder.ToString()), textcolor ?? Color.White, player.whoAmI);
        }
    }

    public static void ChatEmptyLine() => ChatLiteralText("");

    public static void ChatEmptyLine(params ReadOnlySpan<Player> receivers) => ChatLiteralText("", null, receivers);

    public static StringBuilder CreateWithDebugHeader()
    {
        StringBuilder builder = new();
        builder.AppendLine("[Transoceanic ERROR]");
        return builder;
    }

    public static void ChatDebugErrorMessage(string key, Player receiver, params object[] args)
    {
        ChatLiteralText("[Transoceanic ERROR]", TOMain.TODebugErrorColor, receiver);
        ChatLocalizedTextFormat(TOMain.DebugPrefix + key, receiver, TOMain.TODebugErrorColor, args);
        ChatLocalizedText(TOMain.DebugErrorMessageKey, TOMain.TODebugErrorColor, receiver);
    }

    public static void ChatDebugErrorMessage(string key, Player[] receivers, params object[] args)
    {
        ChatLiteralText("[Transoceanic ERROR]", TOMain.TODebugErrorColor, receivers);
        ChatLocalizedTextFormat(TOMain.DebugPrefix + key, receivers, TOMain.TODebugErrorColor, args);
        ChatLocalizedText(TOMain.DebugErrorMessageKey, TOMain.TODebugErrorColor, receivers);
    }

    /*
    public static string Translation(string Chinese = null, string English = null, string Spanish = null, string Russian = null) => Language.ActiveCulture.LegacyId switch
    {
        7 => Chinese,
        6 => Russian,
        5 => Spanish,
        1 => English ?? "Invalid String.",
        _ => English ?? "Invalid String."
    };
    */
}