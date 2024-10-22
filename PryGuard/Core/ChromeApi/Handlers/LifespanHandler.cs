using CefSharp;
using System;

namespace PryGuard.Core.ChromeApi.Handlers
{
    /// <summary>
    /// Handler for managing browser lifespan events (like popup handling).
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

            // Set the window as a popup and pass the required parameters.
            IntPtr parentHandle = browserControl.GetBrowserHost().GetWindowHandle();
            windowInfo.SetAsPopup(parentHandle, targetUrl); // Pass parentHandle and targetUrl

            // Optionally set the popup window size and position here
            windowInfo.Width = popupFeatures.Width.GetValueOrDefault(800);
            windowInfo.Height = popupFeatures.Height.GetValueOrDefault(600);
            windowInfo.X = popupFeatures.X.GetValueOrDefault(100);
            windowInfo.Y = popupFeatures.Y.GetValueOrDefault(100);

            // Allow the popup to open (return false)
            return false;
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
