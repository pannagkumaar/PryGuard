using System;
using System.Collections.Generic;
using System.Linq;

namespace PryGuard.Core.Browser.Settings;
/// <summary>
/// This is where the possible languages for the browser are stored 
/// </summary>
public static class EChromeLanguageHelper
{
    private static readonly Dictionary<EChromeLanguage, ChromeLanguageInfo> Languages =
        new()
        {
                {
                    EChromeLanguage.Ru,
                    new ChromeLanguageInfo()
                    {
                        AcceptLanguageList = "ru-RU,ru", Locale = "ru-RU", Name = "Russian",
                        Language = EChromeLanguage.Ru
                    }
                },
                {
                    EChromeLanguage.EnUsa,
                    new ChromeLanguageInfo()
                    {
                        AcceptLanguageList = "en-US,en", Locale = "en-US", Name = "English(USA)",
                        Language = EChromeLanguage.EnUsa
                    }
                },
                {
                    EChromeLanguage.EnGb,
                    new ChromeLanguageInfo()
                    {
                        AcceptLanguageList = "en-GB,en", Locale = "en-GB", Name = "English(UK)",
                        Language = EChromeLanguage.EnGb
                    }
                },
                {
                    EChromeLanguage.Sw,
                    new ChromeLanguageInfo()
                    {
                        AcceptLanguageList = "sv-SE,sv", Locale = "sv-SE", Name = "Sweden",
                        Language = EChromeLanguage.Sw
                    }
                },
                {
                    EChromeLanguage.De,
                    new ChromeLanguageInfo()
                    {
                        AcceptLanguageList = "de-DE,de", Locale = "de-DE", Name = "Germany",
                        Language = EChromeLanguage.De
                    }
                },
                {
                    EChromeLanguage.Fr,
                    new ChromeLanguageInfo()
                    {
                        AcceptLanguageList = "fr-FR,fr", Locale = "fr-FR", Name = "French",
                        Language = EChromeLanguage.Fr
                    }
                },
                {
                    EChromeLanguage.It,
                    new ChromeLanguageInfo()
                    {
                        AcceptLanguageList = "it-IT,it", Locale = "it-IT", Name = "Italian",
                        Language = EChromeLanguage.It
                    }
                },
                {
                    EChromeLanguage.Kz,
                    new ChromeLanguageInfo()
                    {
                        AcceptLanguageList = "kk-KZ,kk", Locale = "kk-KZ", Name = "Kazakh",
                        Language = EChromeLanguage.Kz
                    }
                }
        };

    static EChromeLanguageHelper()
    {
        Array values = Enum.GetValues(typeof(EChromeLanguage));
        if (Languages.Count != values.Length)
            throw new ArgumentException("Not all languages are accounted for!");
    }

    public static ChromeLanguageInfo GetFullInfo(EChromeLanguage language)
    {
        if (!Languages.ContainsKey(language))
            throw new ArgumentOutOfRangeException(string.Format("This language {0} is not supported", language));
        return Languages[language];
    }

    public static string ToAcceptList(this EChromeLanguage lang)
    {
        List<EChromeLanguage> echromeLanguageList = new List<EChromeLanguage>() { lang };
        if (lang != EChromeLanguage.EnUsa)
            echromeLanguageList.Add(EChromeLanguage.EnUsa);
        return GetAcceptList(echromeLanguageList);
    }

    private static string GetAcceptList(IEnumerable<EChromeLanguage> langs)
    {
        string str = "";
        foreach (EChromeLanguage lang in langs)
            str = !string.IsNullOrWhiteSpace(str)
                ? str + "," + Languages[lang].AcceptLanguageList
                : Languages[lang].AcceptLanguageList;
        return str;
    }

    public static string ToNormalString(this EChromeLanguage lang)
    {
        return Languages[lang].Name;
    }

    public static EChromeLanguage FindLang(string langstr)
    {
        foreach (KeyValuePair<EChromeLanguage, ChromeLanguageInfo> language in Languages)
        {
            if (language.Value.Name.ToLower() == langstr.ToLower().Trim())
                return language.Key;
        }

        return EChromeLanguage.EnUsa;
    }

    public static string ToLocal(this EChromeLanguage lang)
    {
        return Languages[lang].Locale;
    }

    public static List<EChromeLanguage> GetAllLanguages()
    {
        return Languages.Keys.ToList();
    }

    public static List<ChromeLanguageInfo> GetAllLanguagesInfo()
    {
        return Languages.Values.ToList();
    }
}