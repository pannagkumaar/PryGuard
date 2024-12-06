using System;
using System.Collections.Generic;

namespace PryGuard.Core.Browser.Settings
{
    public enum BrowserLanguage
    {
        Russian,
        EnglishUS,
        EnglishUK,
        Swedish,
        German,
        French,
        Italian,
        Kazakh,
    }

    public static class BrowserLanguageExtensions
    {
        private static readonly Random RandomGenerator = new Random();

        // A mapping of BrowserLanguage to locales
        private static readonly Dictionary<BrowserLanguage, string> LanguageToLocaleMap = new()
        {
            { BrowserLanguage.Russian, "ru-RU" },
            { BrowserLanguage.EnglishUS, "en-US" },
            { BrowserLanguage.EnglishUK, "en-GB" },
            { BrowserLanguage.Swedish, "sv-SE" },
            { BrowserLanguage.German, "de-DE" },
            { BrowserLanguage.French, "fr-FR" },
            { BrowserLanguage.Italian, "it-IT" },
            { BrowserLanguage.Kazakh, "kk-KZ" }
        };

        // Method to get a random language
        public static BrowserLanguage GetRandomLanguage()
        {
            Array values = Enum.GetValues(typeof(BrowserLanguage));
            int randomIndex = RandomGenerator.Next(values.Length);
            return (BrowserLanguage)values.GetValue(randomIndex);
        }

        // Method to get a specific enum value by index
        public static BrowserLanguage GetValue(int index)
        {
            Array values = Enum.GetValues(typeof(BrowserLanguage));
            if (index < 0 || index >= values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for the enum values.");
            }
            return (BrowserLanguage)values.GetValue(index);
        }

        // Extension method to get the locale for a BrowserLanguage
        public static string ToLocal(this BrowserLanguage lang)
        {
            if (LanguageToLocaleMap.TryGetValue(lang, out var locale))
            {
                return locale;
            }

            throw new ArgumentException($"Locale for language {lang} is not defined.");
        }
    }
}
