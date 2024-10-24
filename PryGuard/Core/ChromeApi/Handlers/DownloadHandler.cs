using CefSharp;
using PryGuard.View;
using System;
using System.Windows;

public class DownloadHandler : IDownloadHandler
{
    

    // Constructor that takes a DownloadManagerView reference
    public DownloadHandler()
    {
        
    }

    public event EventHandler<DownloadItem> DownloadUpdated;
    public event Action<DownloadItem> DownloadStarted;

    public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
    {
        return true;
    }

    public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
    {
        if (!callback.IsDisposed)
        {
            using (callback)
            {
                DownloadStarted?.Invoke(downloadItem);
                callback.Continue(downloadItem.SuggestedFileName, showDialog: true);

                // Use the passed-in reference to add the download
                
            }
        }
    }

    public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
    {
        DownloadUpdated?.Invoke(this, downloadItem);
    }
}
