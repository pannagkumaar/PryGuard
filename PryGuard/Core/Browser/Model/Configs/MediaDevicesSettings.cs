using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PryGuard.Core.Browser.Model.Configs;
public sealed class MediaDevicesSettings : INotifyPropertyChanged
{
    private bool _hide = true;

    public ControlMode _status;
    public ControlMode Status
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
    public bool HideDevices
    {
        get => _hide;
        set
        {
            _hide = value;
            OnPropertyChanged(nameof(HideDevices));
        }
    }

    private int _vi = 1;

    public int VideoInputs
    {
        get => _vi;
        set
        {
            _vi = value;
            OnPropertyChanged(nameof(VideoInputs));
        }
    }

    private int _ai = 1;

    public int AudioInputs
    {
        get => _ai;
        set
        {
            _ai = value;
            OnPropertyChanged(nameof(AudioInputs));
        }
    }

    private int _ao = 2;

    public int AudioOutputs
    {
        get => _ao;
        set
        {
            _ao = value;
            OnPropertyChanged(nameof(AudioOutputs));
        }
    }


    public int SavedVideoInputs { get; set; } = 1;
    public int SavedAudioInputs { get; set; } = 1;
    public int SavedAudioOutputs { get; set; } = 2;

    public bool IsChanged()
    {
        return VideoInputs != SavedVideoInputs || AudioInputs != SavedAudioInputs ||
               AudioOutputs != SavedAudioOutputs;
    }

    public void Update()
    {
        if (!IsChanged())
        {
            return;
        }

        SavedAudioInputs = AudioInputs;
        SavedVideoInputs = VideoInputs;
        SavedAudioOutputs = AudioOutputs;
    }

    public MediaDevicesSettings(int vi, int ai, int ao)
    {
        AudioInputs = ai;
        VideoInputs = vi;
        AudioOutputs = ao;
    }

    public MediaDevicesSettings()
    {

    }

    public string StringPresent => $"({VideoInputs},{AudioInputs},{AudioOutputs})";

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChangedEventHandler propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}