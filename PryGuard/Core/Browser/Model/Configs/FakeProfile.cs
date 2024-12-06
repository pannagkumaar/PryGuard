using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PryGuard.Core.Browser.Settings;

namespace PryGuard.Core.Browser.Model.Configs
{
    /// <summary>
    /// A fake profile for the browser, with data substituted via JavaScript in the browser itself.
    /// </summary>
    public class FakeProfile : INotifyPropertyChanged
    {
        public BrowserType BrowserTypeType { get; set; }

        private BrowserLanguageInfo _chromeLanguageInfo;
        public BrowserLanguageInfo ChromeLanguageInfo
        {
            get => _chromeLanguageInfo;
            set => SetProperty(ref _chromeLanguageInfo, value);
        }

        public BrowserLanguage CurrentChromeLanguage
        {
            get => ChromeLanguageInfo.Language;
            set => ChromeLanguageInfo = BrowserLanguageHelper.GetFullInfo(value);
        }

        private OSVersion _osVersion;
        public OSVersion OsVersion
        {
            get => _osVersion;
            set => SetProperty(ref _osVersion, value);
        }

        public bool IsX64 { get; set; } = true;

        public string Platform { get; set; } = "Win32";

        private ControlMode _cpuStatus;
        public ControlMode CpuStatus
        {
            get => _cpuStatus;
            set => SetProperty(ref _cpuStatus, value);
        }

        private int _cpuConcurrency;
        public int CpuConcurrency
        {
            get => _cpuConcurrency;
            set => SetProperty(ref _cpuConcurrency, value);
        }

        private ControlMode _memStatus;
        public ControlMode MemStatus
        {
            get => _memStatus;
            set => SetProperty(ref _memStatus, value);
        }

        private int _memoryAvailable;
        public int MemoryAvailable
        {
            get => _memoryAvailable;
            set => SetProperty(ref _memoryAvailable, value);
        }

        private bool _isSendDoNotTrack;
        public bool IsSendDoNotTrack
        {
            get => _isSendDoNotTrack;
            set => SetProperty(ref _isSendDoNotTrack, value);
        }

        public bool IsMac { get; set; }

        private string _userAgent;
        public string UserAgent
        {
            get => _userAgent;
            set => SetProperty(ref _userAgent, value);
        }

        public string AppVersion => !string.IsNullOrWhiteSpace(UserAgent) && UserAgent.Length >= 8
            ? UserAgent.Substring(8)
            : string.Empty;

        private bool _hideCanvas;
        public bool HideCanvas
        {
            get => _hideCanvas;
            set => SetProperty(ref _hideCanvas, value);
        }

        public string CanvasFingerPrintHash { get; set; }

        public double BaseLatency { get; set; }
        public double ChannelDataDelta { get; set; }
        public double ChannelDataIndexDelta { get; set; }
        public double FloatFrequencyDataDelta { get; set; }
        public double FloatFrequencyDataIndexDelta { get; set; }

        private ScreenSize _screenSize;
        public ScreenSize ScreenSize
        {
            get => _screenSize;
            set => SetProperty(ref _screenSize, value);
        }

        private bool _hideFonts = true;
        public bool HideFonts
        {
            get => _hideFonts;
            set => SetProperty(ref _hideFonts, value);
        }

        public List<string> Fonts { get; set; } = new List<string>();
        public int FontsCount => Fonts.Count;

        private WebGLSetting _webGL;
        public WebGLSetting WebGL
        {
            get => _webGL;
            set => SetProperty(ref _webGL, value);
        }

        private MediaDevicesSettings _mediaDevicesSettings;
        public MediaDevicesSettings MediaDevicesSettings
        {
            get => _mediaDevicesSettings;
            set => SetProperty(ref _mediaDevicesSettings, value);
        }

        private WebRTCSettings _webRTCSettings;
        public WebRTCSettings WebRtcSettings
        {
            get => _webRTCSettings;
            set => SetProperty(ref _webRTCSettings, value);
        }

        public GeoSettings GeoSettings { get; set; }
        public TimezoneSetting TimezoneSetting { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
