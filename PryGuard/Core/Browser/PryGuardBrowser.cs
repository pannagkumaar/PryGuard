using CefSharp;
using CefSharp.Wpf;
using PryGuard.Core.Browser.Handlers;
using System;

namespace PryGuard.Core.Browser
{
    public class PryGuardBrowser : ChromiumWebBrowser
    {
        public bool IsIncognito { get; set; }
        public PryGuardBrowser(RequestContext context)
        {
            RequestContext = context;
            MenuHandler = new MenuHandler();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            var context = (RequestContext)RequestContext;

            if (!context.IsDisposed)
                context.Dispose();
        }

        // Method to get the CookieManager for this browser
        public ICookieManager GetCookieManager()
        {
            // Provide a null callback if you don't need to handle it
            return RequestContext.GetCookieManager(null);
        }

        internal void LoadUrl(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
