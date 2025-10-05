namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(CommandCaller caller)
    {
        public void ReplyLocalizedText(string key, Color? textColor = null) => caller.Reply(Language.GetTextValue(key), textColor ?? Color.White);

        public void ReplyLocalizedTextFormat(string key, Color? textColor = null, params object[] args) => caller.Reply(Language.GetTextFormat(key, args), textColor ?? Color.White);

        public void ReplyLocalizedText(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null) => caller.ReplyLocalizedText(localizationPrefixProvider.GetKey(suffix), textColor);

        public void ReplyLocalizedTextFormat(ILocalizationPrefix localizationPrefixProvider, string suffix, Color? textColor = null, params object[] args) => caller.ReplyLocalizedTextFormat(localizationPrefixProvider.GetKey(suffix), textColor, args);

        public void ReplyStringBuilder(StringBuilder builder, Color? textColor = null) => caller.Reply(builder.ToString(), textColor ?? Color.White);

        public void ReplyDebugErrorMessage(string key, params object[] args)
        {
            caller.Reply("[Transoceanic ERROR", TOMain.TODebugErrorColor);
            caller.ReplyLocalizedTextFormat(key, TOMain.TODebugErrorColor, args);
            caller.ReplyLocalizedText(TOMain.DebugErrorMessageKey, TOMain.TODebugErrorColor);
        }
    }
}