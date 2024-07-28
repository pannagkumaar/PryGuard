using CefSharp;
using CefSharp.Wpf;
using PryGuard.Core.ChromeApi.Handlers;
using System;

namespace PryGuard.Core.ChromeApi;
public class PryGuardBrowser : ChromiumWebBrowser
{
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

    internal void LoadUrl(Uri uri)
    {
        throw new NotImplementedException();
    }
}