namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(Language)
    {
        public static string GetTextFormat(string key, params object[] args) => Language.GetText(key).Format(args);
    }
}