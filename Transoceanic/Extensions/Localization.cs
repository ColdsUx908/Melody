namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(CommandCaller commandCaller)
    {
        public void ReplyLocalizedText(string key, Color? textColor = null) => commandCaller.Reply(Language.GetTextValue(key), textColor ?? Color.White);

        public void ReplyLocalizedTextWith(string key, Color? textColor = null, params object[] args) => commandCaller.Reply(TOLocalizationUtils.GetTextFormat(key, args), textColor ?? Color.White);

        public void ReplyStringBuilder(StringBuilder builder, Color? textColor = null) => commandCaller.Reply(builder.ToString(), textColor ?? Color.White);
    }

    extension(StringBuilder builder)
    {
        public void AppendLocalizedLine(string key) => builder.AppendLine(Language.GetTextValue(key));

        public void AppendLocalizedLineWith(string key, params object[] args) => builder.AppendLine(TOLocalizationUtils.GetTextFormat(key, args));

        public void AppendTODebugErrorMessage()
        {
            builder.Append(Environment.NewLine);
            builder.AppendLocalizedLine(TOMain.DebugErrorMessageKey);
        }
    }

    extension(CommandCaller caller)
    {
        public void ReplyDebugErrorMessage(string key, object[] args)
        {
            caller.Reply("[Transoceanic ERROR", TOMain.TODebugErrorColor);
            caller.ReplyLocalizedTextWith(key, TOMain.TODebugErrorColor, args);
            caller.ReplyLocalizedText(TOMain.DebugErrorMessageKey, TOMain.TODebugErrorColor);
        }
    }
}
