using System;
using CefSharp;
using PryGuard.Core.Browser;

namespace PryGuard.Core.Browser.Handlers;

/// <summary>
/// Catches frame loading events
/// </summary>
public class LoadHandler : ILoadHandler
{

    public event Action<bool> IsSecureConnectionChanged;
    private Action _onBrowserFailLoadListener;
    private readonly string _jsCode;
    private readonly string _codeForFakeProfile;

    public LoadHandler(string jsCode, string fakeProfileCode, Action OnBrowserFailLoadListener)
    {
        _jsCode = jsCode;
        _codeForFakeProfile = fakeProfileCode;
        _onBrowserFailLoadListener = OnBrowserFailLoadListener;
    }

    public void OnLoadingStateChange(IWebBrowser chromiumWebBrowser,
        LoadingStateChangedEventArgs loadingStateChangedArgs)
    {
    }

    public void OnFrameLoadStart(IWebBrowser chromiumWebBrowser, FrameLoadStartEventArgs frameLoadStartArgs)
    {
        var frame = frameLoadStartArgs.Frame;
        var lower = frame.Url.ToLower();
        if (frame.IsMain)
            IsSecureConnectionChanged?.Invoke(false);
        // if (lower.StartsWith("chrome://") || lower.StartsWith("chrome-devtools://") ||
        // frameLoadStartArgs.Frame.IsMain)
        // return;
        if (frame.IsMain && frame.IsValid && frame.Url != "about:blank" && !frame.Browser.IsPopup)
        {
            frame.ExecuteJavaScriptAsync(_jsCode, "about:blank/inject");
        }

        if (frame.IsValid && !lower.Contains("ohio8"))
        {
            frame.ExecuteJavaScriptAsync(_codeForFakeProfile,
                "ext::fakeinjection", 1);
        }
    }

    public void OnFrameLoadEnd(IWebBrowser chromiumWebBrowser, FrameLoadEndEventArgs frameLoadEndArgs)
    {
        if (!frameLoadEndArgs.Frame.IsValid || !frameLoadEndArgs.Frame.IsMain || !frameLoadEndArgs.Url.ToLower().StartsWith("https://"))
        {
            return;
        }

        IsSecureConnectionChanged?.Invoke(true);
    }

    public void OnLoadError(IWebBrowser chromiumWebBrowser, LoadErrorEventArgs loadErrorArgs)
    {
        if (!loadErrorArgs.Frame.IsValid || !loadErrorArgs.Frame.IsMain ||
            loadErrorArgs.ErrorCode == CefErrorCode.Aborted)
        {
            return;
        }
        _onBrowserFailLoadListener?.Invoke();
        string html = string.Format("<h3>The loading of page {0} ended with an error: {1}({2})</h3>",
            loadErrorArgs.FailedUrl, loadErrorArgs.ErrorText, loadErrorArgs.ErrorCode.ToString());
        if (loadErrorArgs.ErrorCode == CefErrorCode.ProxyConnectionFailed)
            html +=
                "<h1>No connection to proxy! Make sure your proxies are still working<h1><p>ProxyConnectionFailed</p>";
        if (loadErrorArgs.ErrorCode == CefErrorCode.ProxyAuthRequested)
            html += "<h1>Check your proxy login and password<h1><p>ProxyAuthRequested</p> ";
        if (loadErrorArgs.ErrorCode == CefErrorCode.SocksConnectionFailed)
            html += "<h1>Check your proxy login and password<h1> <p>SocksConnectionFailed</p> ";
        if (loadErrorArgs.ErrorCode == CefErrorCode.TunnelConnectionFailed)
            html +=
                "<h1>Proxy connection error!  Make sure your proxies are still working!<h1><p>TunnelConnectionFailed</p> ";
        loadErrorArgs.Frame.LoadUnicodeHtml(html);
    }
}
