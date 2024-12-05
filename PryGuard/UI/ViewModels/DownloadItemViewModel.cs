using CefSharp;
using PryGuard.Resources.Commands;
using System;
using System.ComponentModel;
using System.Windows.Input;

public class DownloadItemViewModel : INotifyPropertyChanged
{
    #region Properties
    public int Id { get; private set; }

    private string _suggestedFileName;
    public string SuggestedFileName
    {
        get => _suggestedFileName;
        set
        {
            if (_suggestedFileName != value)
            {
                _suggestedFileName = value;
                OnPropertyChanged(nameof(SuggestedFileName));
            }
        }
    }

    private int _percentComplete;
    public int PercentComplete
    {
        get => _percentComplete;
        set
        {
            if (_percentComplete != value)
            {
                _percentComplete = value;
                OnPropertyChanged(nameof(PercentComplete));
            }
        }
    }

    private bool _isComplete;
    public bool IsComplete
    {
        get => _isComplete;
        set
        {
            if (_isComplete != value)
            {
                _isComplete = value;
                OnPropertyChanged(nameof(IsComplete));
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    private bool _isCancelled;
    public bool IsCancelled
    {
        get => _isCancelled;
        set
        {
            if (_isCancelled != value)
            {
                _isCancelled = value;
                OnPropertyChanged(nameof(IsCancelled));
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    public string Status => IsComplete ? "Complete" : (IsCancelled ? "Cancelled" : "Downloading");

    private string _fullPath;
    public string FullPath
    {
        get => _fullPath;
        set
        {
            if (_fullPath != value)
            {
                _fullPath = value;
                OnPropertyChanged(nameof(FullPath));
            }
        }
    }
    #endregion

    #region Commands
    // Command to open the file
    public ICommand OpenFileCommand { get; }
    public ICommand CancelCommand { get; }
    #endregion

    #region Ctor
    public DownloadItemViewModel(DownloadItem downloadItem)
    {
        Id = downloadItem.Id;
        OpenFileCommand = new RelayCommand(OpenFile, CanOpenFile);
        CancelCommand = new RelayCommand(CancelDownload, CanCancelDownload);
        Update(downloadItem);
    }
    #endregion

    #region DownloadWork
    private void CancelDownload(object parameter)
    {
        DownloadManager.Instance.CancelDownload(Id);
    }

    private bool CanCancelDownload(object parameter)
    {
        return !IsComplete && !IsCancelled;
    }
    private void RaiseCommandsCanExecuteChanged()
    {
        (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (OpenFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }
    public void Update(DownloadItem downloadItem)
    {
        // Only update SuggestedFileName if it's not null or empty
        if (!string.IsNullOrEmpty(downloadItem.SuggestedFileName))
        {
            SuggestedFileName = downloadItem.SuggestedFileName;
        }

        PercentComplete = downloadItem.PercentComplete;
        IsComplete = downloadItem.IsComplete;
        IsCancelled = downloadItem.IsCancelled;
        FullPath = downloadItem.FullPath;
        
    }



    private void OpenFile(object parameter)
    {
        try
        {
            if (System.IO.File.Exists(FullPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = FullPath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log error)
        }
    }

    private bool CanOpenFile(object parameter)
    {
        return IsComplete && System.IO.File.Exists(FullPath);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
#endregion