using PryGuard.UI.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PryGuard.DataModels;

public class ProfileTab : INotifyPropertyChanged
{
    public ProfileTab() { }
    public ProfileTab(PryGuardProfilesViewModel vm) { ViewModel = vm; }

    private PryGuardProfilesViewModel _viewModel;
    public PryGuardProfilesViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel == value)
                return;
            _viewModel = value;
            OnPropertyChanged(nameof(ViewModel));

        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;
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

    private string _status;
    public string Status
    {
        get => _status;
        set
        {
            if (_status == value)
                return;
            _status = value;
            OnPropertyChanged(nameof(Status));

        }
    }

    private string _proxyHostPort;
    public string ProxyHostPort
    {
        get => _proxyHostPort;
        set
        {
            if (_proxyHostPort == value)
                return;
            _proxyHostPort = value;
            OnPropertyChanged(nameof(ProxyHostPort));

        }
    }

    private string _proxyLoginPass;
    public string ProxyLoginPass
    {
        get => _proxyLoginPass;
        set
        {
            if (_proxyLoginPass == value)
                return;
            _proxyLoginPass = value;
            OnPropertyChanged(nameof(ProxyLoginPass));

        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}