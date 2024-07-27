using CefSharp;

namespace PryGuard.Core.ChromeApi.Handlers;

/// <summary>
/// View documentation for ILifespanhandler
/// </summary>
public class LifespanHandler : ILifeSpanHandler
{
    bool ILifeSpanHandler.OnBeforePopup(
      IWebBrowser browserControl,
      IBrowser browser,
      IFrame frame,
      string targetUrl,
      string targetFrameName,
      WindowOpenDisposition targetDisposition,
      bool userGesture,
      IPopupFeatures popupFeatures,
      IWindowInfo windowInfo,
      IBrowserSettings browserSettings,
      ref bool noJavascriptAccess,
      out IWebBrowser newBrowser)
    {
        newBrowser = null;
        return false;
    }

    void ILifeSpanHandler.OnAfterCreated(
      IWebBrowser browserControl,
      IBrowser browser)
    {
    }

    bool ILifeSpanHandler.DoClose(IWebBrowser browserControl, IBrowser browser)
    {
        return false;
    }

    void ILifeSpanHandler.OnBeforeClose(
      IWebBrowser browserControl,
      IBrowser browser)
    {
        browser.Dispose();
    }
}
