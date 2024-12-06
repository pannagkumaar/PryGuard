using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PryGuard.DataModels;

/// <summary>
/// Fingerprint
/// </summary>
public class Fingerprint
{
    [JsonPropertyName("webgl_params")]
    public Dictionary<int, string> Params { get; set; }
    [JsonPropertyName("webgl_vendor")]
    public string Vendor { get; set; }
    [JsonPropertyName("webgl_renderer")]
    public string Renderer { get; set; }
    [JsonPropertyName("useragent")]
    public string UserAgent { get; set; }
    [JsonPropertyName("fonts")]
    public List<string> Fonts { get; set; }
}
