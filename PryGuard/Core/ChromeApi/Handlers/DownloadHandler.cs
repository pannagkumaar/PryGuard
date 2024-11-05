// DownloadHandler.cs
using CefSharp;
using System;

public class DownloadHandler : IDownloadHandler
{
    public event Action<DownloadItem> DownloadStarted;
    public event Action<DownloadItem> DownloadUpdated;

    public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
    {
        // Allow all downloads
        return true;
    }

    public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
    {
        DownloadStarted?.Invoke(downloadItem);

        if (!callback.IsDisposed)
        {
            using (callback)
            {
                // Initiate the download without showing a dialog
                callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
            }
        }
    }

    public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
    {
        DownloadUpdated?.Invoke(downloadItem);

        // Check if the download has been canceled
        if (DownloadManager.Instance.IsDownloadCancelled(downloadItem.Id))
        {
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Cancel();
                }
            }
        }
    }
}
