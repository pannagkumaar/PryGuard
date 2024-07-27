using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;

namespace PryGuard.Core.ChromeApi.Model.Configs;
public class WebRTCSettings : INotifyPropertyChanged
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WebRTCStatus
    {
        OFF,
        MANUAL,
        REAL
    }

    private string _localIp = "192.168.1.48";
    public string LocalIp
    {
        get => _localIp;
        set
        {
            if (_localIp == value)
                return;
            _localIp = value;
            OnPropertyChanged(nameof(LocalIp));
        }
    }
    private string _publicIp;
    public string PublicIp
    {
        get => _publicIp;
        set
        {
            if (_publicIp == value)
                return;
            _publicIp = value;
            OnPropertyChanged(nameof(PublicIp));
        }
    }

    private WebRTCStatus _rtcStatus;
    public WebRTCStatus WebRtcStatus
    {
        get => _rtcStatus;
        set
        {
            _rtcStatus = value;
            OnPropertyChanged(nameof(WebRtcStatus));
        }
    }

    public WebRTCSettings()
    {
        WebRtcStatus = WebRTCStatus.OFF;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}