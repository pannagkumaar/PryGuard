using System.Collections.Generic;

namespace PryGuard.Core.Browser.Proxy;
public class DirectProxy : ChromeProxy
{
    public override Dictionary<string, object> GetContextPreference()
    {
        return new Dictionary<string, object>() { { "mode", "direct" } };
    }

    public override string GetProxyString()
    {
        return "";
    }

    public DirectProxy() : base(EProxyType.Direct, "", 0)
    {
    }
}