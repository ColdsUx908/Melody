namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(ILocalizationPrefix localizationPrefixProvider)
    {
        public string LocalizationPrefix => localizationPrefixProvider.LocalizationPrefix;

        public string GetKey(string suffix) => localizationPrefixProvider.LocalizationPrefix + "." + suffix;

        public LocalizedText GetText(string suffix) => Language.GetText(localizationPrefixProvider.GetKey(suffix));

        public string GetTextValue(string suffix) => Language.GetTextValue(localizationPrefixProvider.GetKey(suffix));

        public string GetTextFormat(string suffix, params object[] args) => Language.GetTextFormat(localizationPrefixProvider.GetKey(suffix), args);
    }
}