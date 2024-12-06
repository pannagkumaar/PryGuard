using System;
using CefSharp;
using System.Security.Cryptography.X509Certificates;
using PryGuard.Core.Browser.Proxy;

namespace PryGuard.Core.Browser.Handlers;

/// <summary>
/// Work with requests
/// intercept them, block some of them, if authorization for proxy is needed, we perform it
/// </summary>
public class RequestHandler : IRequestHandler
{
    public ProxyAuthCredentials _authCredentials;
    public static readonly CookieHandler CookieHandler = new CookieHandler();
    private readonly BlockManager _blockManager;


    public RequestHandler(BlockManager blockManager)
    {
        _blockManager = blockManager;
    }

    public void SetAuthCredentials(ProxyAuthCredentials proxyAuthCredentials)
    {
        _authCredentials = proxyAuthCredentials;
    }

    bool IRequestHandler.OnBeforeBrowse(
      IWebBrowser browserControl,
      IBrowser browser,
      IFrame frame,
      IRequest request,
      bool userGesture,
      bool isRedirect)
    {
        return false;
    }

    public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
    }

    bool IRequestHandler.OnOpenUrlFromTab(
      IWebBrowser browserControl,
      IBrowser browser,
      IFrame frame,
      string targetUrl,
      WindowOpenDisposition targetDisposition,
      bool userGesture)
    {
        return false;
    }

    public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
      IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
    {
        return new ResourceRequestHandler(_blockManager);
    }

    bool IRequestHandler.OnCertificateError(
      IWebBrowser browserControl,
      IBrowser browser,
      CefErrorCode errorCode,
      string requestUrl,
      ISslInfo sslInfo,
      IRequestCallback callback)
    {
        if (sslInfo.CertStatus.HasFlag(CertStatus.AuthorityInvalid))
        {
            Uri result;
            if (Uri.TryCreate(requestUrl, UriKind.Absolute, out result) && !result.IsDefaultPort)
            {
                using (callback)
                {
                    callback.Continue(true);
                    return true;
                }
            }

            browserControl.LoadHtml("<h1>It looks like your ISP is blocking access to the site. Use proxy and Dns from google</h1>", browser.MainFrame.Url);
            using (callback)
            {
                callback.Continue(false);
                return false;
            }
        }

        if (callback.IsDisposed)
            return false;
        using (callback)
        {
            callback.Continue(true);
            return true;
        }
    }

    //void IRequestHandler.OnPluginCrashed(
    //  IWebBrowser browserControl,
    //  IBrowser browser,
    //  string pluginPath)
    //{
    //}


    bool IRequestHandler.GetAuthCredentials(
      IWebBrowser browserControl,
      IBrowser browser,
      string originUrl,
      bool isProxy,
      string host,
      int port,
      string realm,
      string scheme,
      IAuthCallback callback)
    {

        if (_authCredentials == null)
        {
            if (isProxy)
                browserControl.GetMainFrame().LoadUnicodeHtml("There is no authorization data for the proxy");
            callback.Dispose();
            return false;
        }

        if (isProxy)
        {
            using (callback)
            {
                callback.Continue(_authCredentials.Login, _authCredentials.Password);
                return true;
            }
        }

        callback.Dispose();
        return false;
    }

    bool IRequestHandler.OnSelectClientCertificate(
      IWebBrowser browserControl,
      IBrowser browser,
      bool isProxy,
      string host,
      int port,
      X509Certificate2Collection certificates,
      ISelectClientCertificateCallback callback)
    {
        callback.Dispose();
        return false;
    }

    void IRequestHandler.OnRenderProcessTerminated(
      IWebBrowser browserControl,
      IBrowser browser,
      CefTerminationStatus status)
    {
        browserControl.Reload();
    }

    public void OnRenderViewReady(
        IWebBrowser chromiumWebBrowser,
        IBrowser browser)
    {
    }
}
