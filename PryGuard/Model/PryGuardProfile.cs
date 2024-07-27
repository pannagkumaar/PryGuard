using System;
using System.IO;
using System.ComponentModel;
using PryGuard.Services.Helpers;
using System.Runtime.CompilerServices;
using PryGuard.Core.ChromeApi.Model.Configs;

namespace PryGuard.Model;
public class PryGuardProfile : INotifyPropertyChanged
{
    private ProxySettings _proxy;
    public ProxySettings Proxy
    {
        get => _proxy;
        set
        {
            if (_proxy == value)
                return;
            _proxy = value;
            OnPropertyChanged(nameof(Proxy));
        }
    }

    /// <summary>
    /// Profile Name
    /// </summary>
    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;
            // Rename folder at once
            if (_name != null && Directory.Exists(ClientConfig.ChromeDataPath + "\\" + _name))
            {
                Directory.Move(ClientConfig.ChromeDataPath + "\\" + _name, ClientConfig.ChromeDataPath + "\\" + value);
            }
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private int _id;
    public int Id
    {
        get => _id;
        set
        {
            if (_id == value)
                return;
            _id = value;
            OnPropertyChanged(nameof(Id));

        }
    }

    private string _status;
    public string Status
    {
        get => _status;
        set
        {
            if (_status == value)
                return;
            _status = value;
            _status = _status.Replace("System.Windows.Controls.ComboBoxItem: ", "");
            OnPropertyChanged(nameof(Status));
        }
    }

    private string _tags;
    public string Tags
    {
        get => _tags;
        set
        {
            if (_tags == value)
                return;
            _tags = value;
            OnPropertyChanged(nameof(Tags));
        }
    }

    /// <summary>
    /// On/Off
    /// </summary>
    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;
            _isEnabled = value;
            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    /// <summary>
    /// Browser footprint
    /// </summary>
    private FakeProfile _fakeProfile;
    public FakeProfile FakeProfile
    {
        get => _fakeProfile;
        set
        {
            if (_fakeProfile == value)
                return;
            _fakeProfile = value;
            OnPropertyChanged(nameof(FakeProfile));
        }
    }

    private bool _isLoadImage;
    public bool IsLoadImage
    {
        get => _isLoadImage;
        set
        {
            if (_isLoadImage == value)
                return;
            _isLoadImage = value;
            OnPropertyChanged(nameof(IsLoadImage));
        }
    }

    private bool _isLoadCacheInMemory;
    public bool IsLoadCacheInMemory
    {
        get => _isLoadCacheInMemory;
        set
        {
            if (_isLoadCacheInMemory == value)
                return;
            _isLoadCacheInMemory = value;
            OnPropertyChanged(nameof(IsLoadCacheInMemory));
        }
    }

    /// <summary>
    /// Block ads and scripts
    /// </summary>
    private bool _isAdBlock;
    public bool IsAdBlock
    {
        get => _isAdBlock;
        set
        {
            if (_isAdBlock == value)
                return;
            _isAdBlock = value;
            OnPropertyChanged(nameof(IsAdBlock));
        }
    }

    private string _cachePath;
    public string CachePath
    {
        get => _cachePath;       
        set
        {
            if (_cachePath == value)
                return;
            _cachePath = value;
            OnPropertyChanged(nameof(CachePath));
        }
    }

    public static PryGuardProfile GenerateNewProfile(string name)
    {
        var profileId = new Random().Next(666, 1337);
        return new PryGuardProfile()
        {
            Name = name,
            Id = profileId,
            Status = "NEW",
            FakeProfile = FakeProfileFactory.Generate(),
            IsEnabled = false,
            IsAdBlock = true,
            IsLoadImage = true,
            IsLoadCacheInMemory = true,
            CachePath = Path.Combine(ClientConfig.ChromeDataPath, name + "_Cache_" + profileId),
            Proxy = new ProxySettings()
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == Name)
        {

        }
    }
}