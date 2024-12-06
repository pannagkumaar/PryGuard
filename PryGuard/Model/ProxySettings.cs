using System;
using System.ComponentModel;
using PryGuard.Core.ChromeApi.Proxy;
using System.Runtime.CompilerServices;

namespace PryGuard.Model;

public class ProxySettings : INotifyPropertyChanged
{
    private bool _isCustomProxy;
    private EProxyType _proxyType;
    private bool _isProxyAuth;
    private string _proxy_addr;

    private string _proxy_login;
    private int _proxy_port;
    private string _proxy_password;

    public ProxySettings()
    {
        _proxyType = EProxyType.HTTP;
        ProxyAddress = "";
        _isProxyAuth = false;
        ProxyLogin = "";
        ProxyPassword = "";
        ProxyPort = 8080;
    }
    public string ProxyLines
    {
        get
        {
            return $"{ProxyAddress}:{ProxyPort}";
        }
    }
    private string _proxyLine;
    public string ProxyLine
    {
        get => _proxyLine;
        set
        {
            if (_proxyLine == value)
                return;
            _proxyLine = value;
            var splitted = _proxyLine.Split(':');
            if (splitted.Length == 2)
            {
                // Standard proxy without credentials
                IsCustomProxy = true;
                ProxyAddress = splitted[0];
                ProxyPort = System.Convert.ToInt32(splitted[1]);
                ProxyLogin = null;
                ProxyPassword = null;
                IsProxyAuth = false;
            }
            else if (splitted.Length == 4)
            {
                // Custom proxy with credentials
                IsCustomProxy = true;
                ProxyAddress = splitted[0];
                ProxyPort = System.Convert.ToInt32(splitted[1]);
                ProxyLogin = splitted[2];
                ProxyPassword = splitted[3];
                IsProxyAuth = true;
            }
            OnPropertyChanged(nameof(ProxyLine));
        }
    }

    private bool _isHTTP;
    public bool IsHTTP
    {
        get => _isHTTP;
        set
        {
            if (_isHTTP == value)
                return;
            _isHTTP = value;
            if (_isHTTP) { ProxyType = EProxyType.HTTP; }
            OnPropertyChanged(nameof(IsHTTP));
        }
    }

    private bool _isSOCKS4;
    public bool IsSOCKS4
    {
        get => _isSOCKS4;
        set
        {
            if (_isSOCKS4 == value)
                return;
            _isSOCKS4 = value;
            if (_isSOCKS4) { ProxyType = EProxyType.SOCKS4; }
            OnPropertyChanged(nameof(IsSOCKS4));
        }
    }

    private bool _isSOCKS5;
    public bool IsSOCKS5
    {
        get => _isSOCKS5;
        set
        {
            if (_isSOCKS5 == value)
                return;
            _isSOCKS5 = value;
            if (_isSOCKS5) { ProxyType = EProxyType.SOCKS5; }
            OnPropertyChanged(nameof(IsSOCKS5));
        }
    }

    public bool IsCustomProxy
    {
        get => _isCustomProxy;
        set
        {
            if (_isCustomProxy == value)
                return;
            _isCustomProxy = value;
            OnPropertyChanged(nameof(IsCustomProxy));
            OnPropertyChanged(nameof(StringPresent));

        }
    }

    public EProxyType ProxyType
    {
        get => _proxyType;
        set
        {
            if (_proxyType == value)
                return;
            //if (value == EProxyType.SOCKS5)
            //    IsProxyAuth = false;
            //if (value == EProxyType.SOCKS4)
            //    IsProxyAuth = false;
            //if (value == EProxyType.Direct)
            //    IsProxyAuth = false;
            _proxyType = value;
            OnPropertyChanged(nameof(ProxyType));

        }
    }

    public string ProxyAddress
    {
        get => _proxy_addr;
        set
        {
            _proxy_addr = value;
            OnPropertyChanged(nameof(ProxyAddress));
            OnPropertyChanged(nameof(StringPresent));

        }
    }

    public string ProxyString()
    {
        return _proxyType + "://" + ProxyAddress + ":" + ProxyPort;
    }
    public int ProxyPort
    {
        get { return _proxy_port; }
        set
        {
            _proxy_port = value;
            OnPropertyChanged(nameof(ProxyPort));
        }
    }

    public bool IsProxyAuth
    {
        get => _isProxyAuth;
        set
        {
            if (_isProxyAuth == value)
                return;
            _isProxyAuth = ProxyLogin != "";
           // _isProxyAuth = ProxyType != EProxyType.SOCKS5 && (ProxyType != EProxyType.SOCKS4 && (ProxyType != EProxyType.Direct && value));
            OnPropertyChanged(nameof(IsProxyAuth));

        }
    }

    public string ProxyLogin
    {
        get => _proxy_login;
        set
        {
            if (_proxy_login == value)
                return;

            _proxy_login = value;
            if (value != "") { IsProxyAuth = true; }
            OnPropertyChanged(nameof(ProxyLogin));
        }
    }

    public string ProxyPassword
    {
        get => _proxy_password;
        set
        {
            if (_proxy_password == value)
                return;

            _proxy_password = value;
            OnPropertyChanged(nameof(ProxyPassword));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    public string StringPresent
    {
        get
        {
            if (!_isCustomProxy)
            {
                return "Не используется";
            }

            return $"{ProxyAddress}";
        }
    }
    public ChromeProxy ToChromeProxy()
    {
        switch (ProxyType)
        {
            case EProxyType.Direct:
                return new DirectProxy();
            case EProxyType.HTTP:
            case EProxyType.HTTPS:
            case EProxyType.SOCKS4:
            case EProxyType.SOCKS5:
                return new ChromeProxy(ProxyType, ProxyAddress, ProxyPort);
        }

        throw new NotImplementedException();
    }
}
