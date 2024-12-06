using System.Collections.Generic;

namespace PryGuard.Core.Browser.Proxy;
public class ChromeProxy
{
    private readonly EProxyType _proxyType;
    private readonly string ip;
    private readonly int port;

    public ChromeProxy(EProxyType proxyType, string ip, int port)
    {
        _proxyType = proxyType;
        this.ip = ip;
        this.port = port;
    }
    public virtual Dictionary<string, object> GetContextPreference()
    {
        return new Dictionary<string, object>() { { "mode", "fixed_servers" }, { "server", GetProxyString() } };
    }

    public virtual string GetProxyString()
    {
        return $"{_proxyType}://{ip}:{port}";
    }
}