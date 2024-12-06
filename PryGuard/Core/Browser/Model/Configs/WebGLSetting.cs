using System.ComponentModel;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;

using System.Diagnostics;

namespace PryGuard.Core.Browser.Model.Configs;
public class WebGLSetting
{
    public static int UNMASKED_VENDOR = 37445;
    public static int UNMASKED_RENDERER = 37446;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WebGlStatus
    {
        OFF,
        NOISE
    }

    public class WebGlNoise
    {
        public int Index; //0...9
        public double Difference; //0...1
    }
    public WebGLSetting() { }
    public WebGLSetting(WebGlNoise noise)
    {
        Noise = noise;
    }

    public WebGlNoise Noise = new();

    private WebGlStatus _status = WebGlStatus.OFF;
    public WebGlStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public bool HideWebGL
    {
        get => Status == WebGlStatus.NOISE;
        set
        {
            Status = value ? WebGlStatus.NOISE : WebGlStatus.OFF;
            OnPropertyChanged(nameof(HideWebGL));
        }
    }
    public string Renderer
    {
        get
        {
            Debug.WriteLine("Getting Renderer value...");
            if (Params.ContainsKey(UNMASKED_RENDERER))
            {
                Debug.WriteLine($"Renderer found: {Params[UNMASKED_RENDERER].Value}");
                LogParams();
                return Params[UNMASKED_RENDERER].Value;
            }
            Debug.WriteLine("Renderer not found in Params.");
            LogParams();
            return "";
        }
        set
        {
            Debug.WriteLine($"Setting Renderer value to: {value}");
            if (!Params.ContainsKey(UNMASKED_RENDERER))
            {
                Debug.WriteLine("Renderer not found in Params. Adding new entry.");
                Params[UNMASKED_RENDERER] = new WebGLParam(UNMASKED_RENDERER, value);
            }
            else
            {
                Debug.WriteLine("Renderer found in Params. Updating value.");
                Params[UNMASKED_RENDERER].Value = value;
            }
            LogParams();
            OnPropertyChanged(nameof(HideWebGL));
        }
    }

    private void LogParams()
    {
        Debug.WriteLine("Listing all key-value pairs in Params:");
        foreach (var param in Params)
        {
            Debug.WriteLine($"Key: {param.Key}, Value: {param.Value.Value}");
        }
    }

    public string Vendor
    {
        get
        {
            if (Params.ContainsKey(UNMASKED_VENDOR))
                return Params[UNMASKED_VENDOR].Value;
            return "";
        }
        set
        {
            if (!Params.ContainsKey(UNMASKED_VENDOR))
                Params[UNMASKED_VENDOR] = new WebGLParam(UNMASKED_VENDOR, value);
            else
                Params[UNMASKED_VENDOR].Value = value;
            OnPropertyChanged(nameof(HideWebGL));
        }
    }

    public Dictionary<int, WebGLParam> Params = new Dictionary<int, WebGLParam>();

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}