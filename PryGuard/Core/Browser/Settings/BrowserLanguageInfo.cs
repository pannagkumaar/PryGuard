using System.Text.Json.Serialization;

namespace PryGuard.Core.Browser.Settings
{
    public class BrowserLanguageInfo
    {
        [JsonPropertyName("Language")]
        public BrowserLanguage Language { get; internal set; }

        [JsonPropertyName("Locale")]
        public string Locale { get; internal set; }

        [JsonPropertyName("AcceptList")]
        public string AcceptLanguageList { get; internal set; }

        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; internal set; }
    }
}
