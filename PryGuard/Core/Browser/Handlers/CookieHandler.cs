using CefSharp;

namespace PryGuard.Core.Browser.Handlers;

/// <summary>
/// Handler for cookies
/// </summary>
public class CookieHandler : ICookieAccessFilter
{
    public bool CanSendCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, Cookie cookie)
    {
        return true;
    }

    public bool CanSaveCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response,
        Cookie cookie)
    {
        return true;
    }
}
