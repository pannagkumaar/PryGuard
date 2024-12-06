using CefSharp;
using System.Threading;
using System.Threading.Tasks;
using PryGuard.Core.Browser.Proxy;

namespace PryGuard.Core.Browser;

public static class RequestContextExtentions
{
    // Set proxy in requestcontext
    public static Task SetProxy(
        this IRequestContext context,
        ChromeProxy chromeProxy)
    {
        return Cef.UIThreadTaskFactory.StartNew(() =>
        {
            Thread.Sleep(100);
            return context.SetPreference("proxy", chromeProxy.GetContextPreference(), out _);
        });
    }

    public static async Task<bool> DisableWebRtc(
        this IRequestContext context)
    {
        return await Cef.UIThreadTaskFactory.StartNew(() => context.SetPreference("webrtc.ip_handling_policy", "disable_non_proxied_udp", out _));
    }
}
