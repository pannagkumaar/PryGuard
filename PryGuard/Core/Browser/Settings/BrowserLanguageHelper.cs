using System;
using System.Collections.Generic;
using System.Linq;

namespace PryGuard.Core.Browser.Settings
{
    /// <summary>
    /// Helper class for managing browser languages.
    /// </summary>
    public static class BrowserLanguageHelper
    {
        private static readonly Dictionary<BrowserLanguage, BrowserLanguageInfo> Languages =
            new()
            {
                {
                    BrowserLanguage.Russian,
                    new BrowserLanguageInfo()
                    {
                        AcceptLanguageList = "ru-RU,ru", Locale = "ru-RU", DisplayName = "Russian",
                        Language = BrowserLanguage.Russian
                    }
                },
                {
                    BrowserLanguage.EnglishUS,
                    new BrowserLanguageInfo()
                    {
                        AcceptLanguageList = "en-US,en", Locale = "en-US", DisplayName = "English (USA)",
                        Language = BrowserLanguage.EnglishUS
                    }
                },
                {
                    BrowserLanguage.EnglishUK,
                    new BrowserLanguageInfo()
                    {
                        AcceptLanguageList = "en-GB,en", Locale = "en-GB", DisplayName = "English (UK)",
                        Language = BrowserLanguage.EnglishUK
                    }
                },
                {
                    BrowserLanguage.Swedish,
                    new BrowserLanguageInfo()
                    {
                        AcceptLanguageList = "sv-SE,sv", Locale = "sv-SE", DisplayName = "Swedish",
                        Language = BrowserLanguage.Swedish
                    }
                },
                {
                    BrowserLanguage.German,
                    new BrowserLanguageInfo()
                    {
                        AcceptLanguageList = "de-DE,de", Locale = "de-DE", DisplayName = "German",
                        Language = BrowserLanguage.German
                    }
                },
                {
                    BrowserLanguage.French,
                    new BrowserLanguageInfo()
                    {
                        AcceptLanguageList = "fr-FR,fr", Locale = "fr-FR", DisplayName = "French",
                        Language = BrowserLanguage.French
                    }
                },
                {
                    BrowserLanguage.Italian,
                    new BrowserLanguageInfo()
                    {
                        AcceptLanguageList = "it-IT,it", Locale = "it-IT", DisplayName = "Italian",
                        Language = BrowserLanguage.Italian
                    }
                },
                {
                    BrowserLanguage.Kazakh,
                    new BrowserLanguageInfo()
                    {
                        AcceptLanguageList = "kk-KZ,kk", Locale = "kk-KZ", DisplayName = "Kazakh",
                        Language = BrowserLanguage.Kazakh
                    }
                }
            };

        static BrowserLanguageHelper()
        {
            Array values = Enum.GetValues(typeof(BrowserLanguage));
            if (Languages.Count != values.Length)
                throw new ArgumentException("Not all languages are accounted for!");
        }

        public static BrowserLanguageInfo GetFullInfo(BrowserLanguage language)
        {
            if (!Languages.ContainsKey(language))
                throw new ArgumentOutOfRangeException($"This language {language} is not supported");
            return Languages[language];
        }

        public static string ToAcceptList(this BrowserLanguage lang)
        {
            List<BrowserLanguage> browserLanguageList = new List<BrowserLanguage>() { lang };
            if (lang != BrowserLanguage.EnglishUS)
                browserLanguageList.Add(BrowserLanguage.EnglishUS);
            return GetAcceptList(browserLanguageList);
        }

        private static string GetAcceptList(IEnumerable<BrowserLanguage> langs)
        {
            return langs.Aggregate(string.Empty, (str, lang) =>
                string.IsNullOrWhiteSpace(str)
                    ? Languages[lang].AcceptLanguageList
                    : $"{str},{Languages[lang].AcceptLanguageList}");
        }

        public static string ToDisplayName(this BrowserLanguage lang)
        {
            return Languages[lang].DisplayName;
        }

        public static BrowserLanguage FindLanguage(string languageString)
        {
            foreach (var language in Languages)
            {
                if (language.Value.DisplayName.Equals(languageString.Trim(), StringComparison.OrdinalIgnoreCase))
                    return language.Key;
            }

            return BrowserLanguage.EnglishUS;
        }

        public static string ToLocale(this BrowserLanguage lang)
        {
            return Languages[lang].Locale;
        }

        public static List<BrowserLanguage> GetAllLanguages()
        {
            return Languages.Keys.ToList();
        }

        public static List<BrowserLanguageInfo> GetAllLanguageInfos()
        {
            return Languages.Values.ToList();
        }
    }
}
