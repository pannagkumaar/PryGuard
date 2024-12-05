using CefSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;   
using System.Windows.Threading;

public class DownloadManager : INotifyPropertyChanged
{
    // Singleton instance
    private static readonly Lazy<DownloadManager> _instance = new Lazy<DownloadManager>(() => new DownloadManager());

    public static DownloadManager Instance => _instance.Value;

    public ObservableCollection<DownloadItemViewModel> Downloads { get; set; } = new ObservableCollection<DownloadItemViewModel>();
    private ConcurrentDictionary<int, bool> _cancelledDownloads = new ConcurrentDictionary<int, bool>();

    private DownloadManager() { }

    public void AddDownload(DownloadItem downloadItem)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existingDownload = Downloads.FirstOrDefault(d => d.Id == downloadItem.Id);
            if (existingDownload != null)
            {
                // Update the existing item with new information
                existingDownload.Update(downloadItem);
            }
            else
            {
                // Add new item to the collection
                Downloads.Add(new DownloadItemViewModel(downloadItem));
                OnPropertyChanged(nameof(Downloads));
            }
        });
    }

    public void CancelDownload(int downloadId)
    {
        _cancelledDownloads[downloadId] = true;
    }

    public bool IsDownloadCancelled(int downloadId)
    {
        return _cancelledDownloads.ContainsKey(downloadId);
    }
    public void UpdateDownload(DownloadItem downloadItem)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existingDownload = Downloads.FirstOrDefault(d => d.Id == downloadItem.Id);
            if (existingDownload != null)
            {
                // Update existing item
                existingDownload.Update(downloadItem);
            }
        });
    }



    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
