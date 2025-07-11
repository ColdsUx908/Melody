namespace Transoceanic.Core.Extensions;

public static partial class TOExtensions
{
    extension(ILocalizationPrefix localizationPrefixProvider)
    {
        public string GetKeyWithPrefix(string key) => localizationPrefixProvider.LocalizationPrefix + key;

        public LocalizedText GetTextWithPrefix(string key) => Language.GetText(localizationPrefixProvider.GetKeyWithPrefix(key));

        public string GetTextValueWithPrefix(string key) => Language.GetTextValue(localizationPrefixProvider.GetKeyWithPrefix(key));

        public string GetTextFormatWithPrefix(string key, params object[] args) => Language.GetTextFormat(localizationPrefixProvider.GetKeyWithPrefix(key), args);
    }
}
