using System;
using CefSharp;
using PryGuard.Core.Browser;

namespace PryGuard.Core.Browser.Handlers;

/// <summary>
/// Swap resources in requests (e.g. user-agent or entire request/response body)
/// </summary>
public class ResourceRequestHandler : IResourceRequestHandler
{
    private readonly BlockManager _blockManager;

    public ResourceRequestHandler(BlockManager blockManager)
    {
        _blockManager = blockManager;
    }

    ICookieAccessFilter IResourceRequestHandler.GetCookieAccessFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request)
    {
        return null;
    }

    private string GetSafeMainFrameUrl(IBrowser browser)
    {
        try
        {
            return browser.MainFrame.Url.ToLower();
        }
        catch
        {
            return string.Empty;
        }
    }

    public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request,
        IRequestCallback callback)
    {
        if (request.Url.Contains("chrome-devtools://"))
            return CefReturnValue.Continue;

        if (!_blockManager.IsBlocked(request.Url, GetSafeMainFrameUrl(browser)))
        {
            // var ua = ((BotBrowser) chromiumWebBrowser).JsWorker.Bookmaker.FakeProfile.UserAgent;
            // request.SetHeaderByName("User-Agent", ua, true);
            using (callback)
                return CefReturnValue.Continue;
        }

        try
        {
            if (!browser.IsDisposed)
            {
                if (browser.MainFrame.IsValid && !browser.MainFrame.IsDisposed)
                    browser.MainFrame.ExecuteJavaScriptAsync(
                        "console.error('" + request.Method + " " + request.Url +
                        " net::ERR_BLOCKED_BY_CLIENT');", request.Url);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        using (callback)
        {
            callback.Continue(false);
            return CefReturnValue.Cancel;
        }
    }

    public IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request)
    {
        return null;
    }

    public void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request,
        IResponse response, ref string newUrl)
    {
    }

    public bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request,
        IResponse response)
    {
        return false;
    }

    public IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request, IResponse response)
    {
        if (request.Url.StartsWith("chrome-devtools://", StringComparison.InvariantCultureIgnoreCase))
            return null;
        var lowerRequestUrl = request.Url.ToLower();
        if (lowerRequestUrl.Contains("true/app") && lowerRequestUrl.Contains("kooboo-resource"))
            return new StringReplaceResponseFilter("$compileProvider.debugInfoEnabled(false);",
                "$compileProvider.debugInfoEnabled(true);");
        if (lowerRequestUrl.Contains("static/js/olimp") && lowerRequestUrl.Contains(".js"))
            return new StringReplaceResponseFilter("={sports:{getSports:",
                "=window.zapi={sports:{getSports:");
        if (request.Url.ToLower().Contains("favbet") && request.Url.ToLower().Contains("main") && request.Url.ToLower().Contains(".js"))
            return new StringReplaceResponseFilter("this.send=function(e)",
                "(window.wss?window.wss.push(this):window.wss=[],window.wss.push(this)),this.send=function(e)");
        return null;
    }

    public void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request,
        IResponse response, UrlRequestStatus status, long receivedContentLength)
    {
    }

    public bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
        IRequest request)
    {
        return request.Url.StartsWith("mailto");
    }

    public void Dispose()
    {
    }
}
