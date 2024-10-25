using CefSharp;
using System;

namespace PryGuard.Core.ChromeApi.Handlers
{
    /// <summary>
    /// Handler for managing browser lifespan events (like popup handling).
    /// </summary>
    public class LifespanHandler : ILifeSpanHandler
    {
        public event Action<string> PopupRequested;

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

            // Cancel the popup
            // Notify the main application to open the URL in a new tab
            PopupRequested?.Invoke(targetUrl);

            // Return true to cancel the popup
            return true;
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
