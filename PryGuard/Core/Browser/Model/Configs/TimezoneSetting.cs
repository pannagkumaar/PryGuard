using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PryGuard.Core.Browser.Model.Configs;
public class TimezoneSetting : INotifyPropertyChanged
{
    private bool _hide = true;

    public bool HideTimezone
    {
        get => _hide;
        set
        {
            _hide = value;
            OnPropertyChanged(nameof(HideTimezone));
            OnPropertyChanged(nameof(StringPresent));
        }
    }

    private string _customTz = "Europe";

    public string CustomTimezone
    {
        get => _customTz;
        set
        {
            _customTz = value;
            OnPropertyChanged(nameof(StringPresent));
        }
    }

    public string StringPresent
    {
        get
        {
            if (HideTimezone)
            {
                return "According to the IP";
            }

            return CustomTimezone;
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