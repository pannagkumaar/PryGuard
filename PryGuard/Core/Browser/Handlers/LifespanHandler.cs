using CefSharp;
using System;

namespace PryGuard.Core.Browser.Handlers
{
    /// <summary>
    /// Handler for managing browser lifespan events (like popup handling).
    /// </summary>
    public class LifespanHandler : ILifeSpanHandler
    {
        public event Action<string, bool> PopupRequested;

        public bool OnBeforePopup(
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

            // Cast the browser control to PryGuardBrowser to access IsIncognito
            var pryGuardBrowser = browserControl as PryGuardBrowser;
            bool isIncognito = pryGuardBrowser?.IsIncognito ?? false;

            // Invoke the event with both parameters
            PopupRequested?.Invoke(targetUrl, isIncognito);

            return true; // Cancel the popup
        }


        void ILifeSpanHandler.OnAfterCreated(
            IWebBrowser browserControl,
            IBrowser browser)
        {
            // You can add any behavior here that you want to occur after a popup is created
        }

        bool ILifeSpanHandler.DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            // Return false to let the browser handle the close
            return false;
        }

        void ILifeSpanHandler.OnBeforeClose(
            IWebBrowser browserControl,
            IBrowser browser)
        {
            // Cleanup resources if necessary when a browser window is closed
            browser.Dispose();
        }
    }
}
