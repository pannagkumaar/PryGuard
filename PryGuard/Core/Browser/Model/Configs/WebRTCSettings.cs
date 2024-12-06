using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using System.Collections.Generic;


namespace PryGuard.Core.Browser.Model.Configs
{
    public class WebRTCSettings : INotifyPropertyChanged
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum WebRTCStatus
        {
            OFF,
            ALLOW_ALL,
            ALLOW_STUN,
            ALLOW_TURN,
            MANUAL,
            REAL // Re-added 'REAL' status
        }

        private readonly object _lock = new object();

        private string _localIp = "192.168.1.48";
        public string LocalIp
        {
            get
            {
                lock (_lock)
                {
                    return _localIp;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_localIp == value)
                        return;
                    _localIp = value;
                    OnPropertyChanged(nameof(LocalIp));
                }
            }
        }

        private string _publicIp;
        public string PublicIp
        {
            get
            {
                lock (_lock)
                {
                    return _publicIp;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_publicIp == value)
                        return;
                    _publicIp = value;
                    OnPropertyChanged(nameof(PublicIp));
                }
            }
        }

        private WebRTCStatus _rtcStatus;
        public WebRTCStatus WebRtcStatus
        {
            get
            {
                lock (_lock)
                {
                    return _rtcStatus;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_rtcStatus == value)
                        return;
                    _rtcStatus = value;
                    OnPropertyChanged(nameof(WebRtcStatus));
                }
            }
        }

        private List<string> _stunServers = new List<string>
        {
            "stun:stun.l.google.com:19302"
        };
        public List<string> StunServers
        {
            get
            {
                lock (_lock)
                {
                    return _stunServers;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_stunServers == value)
                        return;
                    _stunServers = value;
                    OnPropertyChanged(nameof(StunServers));
                }
            }
        }

        private List<string> _turnServers = new List<string>
        {
            // Add TURN server URLs here
        };
        public List<string> TurnServers
        {
            get
            {
                lock (_lock)
                {
                    return _turnServers;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_turnServers == value)
                        return;
                    _turnServers = value;
                    OnPropertyChanged(nameof(TurnServers));
                }
            }
        }

        private bool _useMdns = true;
        public bool UseMdns
        {
            get
            {
                lock (_lock)
                {
                    return _useMdns;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_useMdns == value)
                        return;
                    _useMdns = value;
                    OnPropertyChanged(nameof(UseMdns));
                }
            }
        }

        public WebRTCSettings()
        {
            WebRtcStatus = WebRTCStatus.OFF;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
