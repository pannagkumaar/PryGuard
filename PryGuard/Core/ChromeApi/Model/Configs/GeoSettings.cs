using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PryGuard.Core.ChromeApi.Model.Configs;
public class GeoSettings : INotifyPropertyChanged
{

    private AutoManualEnum _status;
    public AutoManualEnum Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public string _latitude;
    public string Latitude
    {
        get => _latitude;
        set
        {
            if (_latitude == value)
                return;
            _latitude = value;
            OnPropertyChanged(nameof(Latitude));
        }
    }

    public string _longitude;
    public string Longitude
    {
        get => _longitude;
        set
        {
            if (_longitude == value)
                return;
            _longitude = value;
            OnPropertyChanged(nameof(Longitude));
        }
    }
    public GeoSettings() { Status = AutoManualEnum.AUTO; }


    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}