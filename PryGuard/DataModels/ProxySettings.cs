using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PryGuard.Core.Browser.Proxy;

namespace PryGuard.DataModels
{
    /// <summary>
    /// Represents the settings for a proxy configuration.
    /// </summary>
    public class ProxySettings : INotifyPropertyChanged
    {
        private bool _isCustomProxy;
        private EProxyType _proxyType;
        private bool _isProxyAuth;
        private string _proxyAddress;
        private string _proxyLogin;
        private int _proxyPort;
        private string _proxyPassword;
        private string _proxyLine;
        private bool _isHTTP;
        private bool _isSOCKS4;
        private bool _isSOCKS5;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxySettings"/> class with default values.
        /// </summary>
        public ProxySettings()
        {
            _proxyType = EProxyType.HTTP;
            ProxyAddress = string.Empty;
            ProxyLogin = string.Empty;
            ProxyPassword = string.Empty;
            ProxyPort = 8080;
            IsProxyAuth = false;
        }

        /// <summary>
        /// Gets or sets the proxy line in the format "address:port" or "address:port:login:password".
        /// </summary>
        public string ProxyLine
        {
            get => _proxyLine;
            set
            {
                if (_proxyLine == value) return;
                _proxyLine = value;

                var parts = _proxyLine.Split(':');
                if (parts.Length == 2)
                {
                    // Standard proxy without credentials
                    IsCustomProxy = true;
                    ProxyAddress = parts[0];
                    ProxyPort = Convert.ToInt32(parts[1]);
                    ProxyLogin = null;
                    ProxyPassword = null;
                    IsProxyAuth = false;
                }
                else if (parts.Length == 4)
                {
                    // Proxy with credentials
                    IsCustomProxy = true;
                    ProxyAddress = parts[0];
                    ProxyPort = Convert.ToInt32(parts[1]);
                    ProxyLogin = parts[2];
                    ProxyPassword = parts[3];
                    IsProxyAuth = true;
                }
                OnPropertyChanged(nameof(ProxyLine));
            }
        }

        public bool IsHTTP
        {
            get => _isHTTP;
            set
            {
                if (_isHTTP == value) return;
                _isHTTP = value;
                if (_isHTTP) ProxyType = EProxyType.HTTP;
                OnPropertyChanged(nameof(IsHTTP));
            }
        }

        public bool IsSOCKS4
        {
            get => _isSOCKS4;
            set
            {
                if (_isSOCKS4 == value) return;
                _isSOCKS4 = value;
                if (_isSOCKS4) ProxyType = EProxyType.SOCKS4;
                OnPropertyChanged(nameof(IsSOCKS4));
            }
        }

        public bool IsSOCKS5
        {
            get => _isSOCKS5;
            set
            {
                if (_isSOCKS5 == value) return;
                _isSOCKS5 = value;
                if (_isSOCKS5) ProxyType = EProxyType.SOCKS5;
                OnPropertyChanged(nameof(IsSOCKS5));
            }
        }

        public bool IsCustomProxy
        {
            get => _isCustomProxy;
            set
            {
                if (_isCustomProxy == value) return;
                _isCustomProxy = value;
                OnPropertyChanged(nameof(IsCustomProxy));
                OnPropertyChanged(nameof(ProxySummary));
            }
        }

        public EProxyType ProxyType
        {
            get => _proxyType;
            set
            {
                if (_proxyType == value) return;
                _proxyType = value;
                OnPropertyChanged(nameof(ProxyType));
            }
        }

        public string ProxyAddress
        {
            get => _proxyAddress;
            set
            {
                if (_proxyAddress == value) return;
                _proxyAddress = value;
                OnPropertyChanged(nameof(ProxyAddress));
                OnPropertyChanged(nameof(ProxySummary));
            }
        }

        public int ProxyPort
        {
            get => _proxyPort;
            set
            {
                if (_proxyPort == value) return;
                _proxyPort = value;
                OnPropertyChanged(nameof(ProxyPort));
            }
        }

        public bool IsProxyAuth
        {
            get => _isProxyAuth;
            set
            {
                if (_isProxyAuth == value) return;
                _isProxyAuth = ProxyLogin != string.Empty;
                OnPropertyChanged(nameof(IsProxyAuth));
            }
        }

        public string ProxyLogin
        {
            get => _proxyLogin;
            set
            {
                if (_proxyLogin == value) return;
                _proxyLogin = value;
                if (!string.IsNullOrEmpty(value)) IsProxyAuth = true;
                OnPropertyChanged(nameof(ProxyLogin));
            }
        }

        public string ProxyPassword
        {
            get => _proxyPassword;
            set
            {
                if (_proxyPassword == value) return;
                _proxyPassword = value;
                OnPropertyChanged(nameof(ProxyPassword));
            }
        }

        public string ProxySummary
        {
            get => IsCustomProxy ? ProxyAddress : "Not in use";
        }

        public string ProxyLines => $"{ProxyAddress}:{ProxyPort}";

        public string GetProxyString()
        {
            return $"{ProxyType}://{ProxyAddress}:{ProxyPort}";
        }

        public ChromeProxy ToChromeProxy()
        {
            return ProxyType switch
            {
                EProxyType.Direct => new DirectProxy(),
                EProxyType.HTTP or EProxyType.HTTPS or EProxyType.SOCKS4 or EProxyType.SOCKS5 =>
                    new ChromeProxy(ProxyType, ProxyAddress, ProxyPort),
                _ => throw new NotImplementedException("Unsupported proxy type."),
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
