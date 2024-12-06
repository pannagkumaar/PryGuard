using PryGuard.DataModels;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Input;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using PryGuard.UI.Views;

using System.Linq;

using System.IO;
using PryGuard.Resources.Commands;
using PryGuard.Core.Browser.Settings;
using PryGuard.Core.Network;
using PryGuard.Resources.Helpers;

using PryGuard.Core.Browser.Model.Configs;
using PryGuard.UI.Views;

namespace PryGuard.UI.ViewModels;
public class PryGuardProfileSettingsViewModel : BaseViewModel
{
    #region Commands
    public RelayCommand CloseProfileSettingsCommand { get; private set; }
    public RelayCommand ChangeWindowStateCommand { get; private set; }
    public RelayCommand CheckProxyCommand { get; private set; }
    public RelayCommand ImportProfileCommand { get; private set; }
    public RelayCommand SaveProfileCommand { get; private set; }
    public ICommand NewFingerprintCommand { get; }
    public ObservableCollection<string> Renderers { get; }
    public ObservableCollection<string> SavedProxies { get; set; }

    #endregion

    #region Properties
    private PryGuardProfile _pryGuardProf;

    public PryGuardProfile PryGuardProf
    {
        get => _pryGuardProf;
        set
        {
            _pryGuardProf = value;
            OnPropertyChanged(nameof(PryGuardProf));
            OnPropertyChanged(nameof(SelectedLanguage));
        }
    }

    public BrowserLanguage SelectedLanguage
    {
        get => PryGuardProf?.FakeProfile?.ChromeLanguageInfo?.Language ?? BrowserLanguage.EnglishUS;
        set
        {
            if (PryGuardProf?.FakeProfile?.ChromeLanguageInfo != null && PryGuardProf.FakeProfile.ChromeLanguageInfo.Language != value)
            {
                PryGuardProf.FakeProfile.ChromeLanguageInfo.Language = value;
                OnPropertyChanged(nameof(SelectedLanguage));
            }
        }
    }
    public List<ScreenSize> ScreenSizes { get; } = new List<ScreenSize>()
        {
            new ScreenSize(1920, 1080),
            new ScreenSize(1366, 768),
            new ScreenSize(1280, 1024),
            new ScreenSize(1440, 900),
            new ScreenSize(1600, 900),
            new ScreenSize(1280, 800),
            new ScreenSize(1024, 768),
            new ScreenSize(2560, 1440),
            new ScreenSize(3840, 2160)
        };
    private WindowState _windowState;
    public WindowState WindowState
    {
        get => _windowState;
        set => Set(ref _windowState, value);
    }

    private PryGuardProfilesViewModel _PryGuardProfilesVM;
    public PryGuardProfilesViewModel PryGuardProfilesVM

    {
        get => _PryGuardProfilesVM;
        set => Set(ref _PryGuardProfilesVM, value);

    }


    private string _saveProfileButtonContent = "Create";
    public string SaveProfileButtonContent
    {
        get => _saveProfileButtonContent;
        set => Set(ref _saveProfileButtonContent, value);
    }

    public bool IsEdit { get; set; }

    private Brush _tbProxyBrush = Brushes.White;
    public Brush TbProxyBrush
    {
        get => _tbProxyBrush;
        set => Set(ref _tbProxyBrush, value);
    }
    private string _selectedSavedProxy;
    public string SelectedSavedProxy
    {
        get => _selectedSavedProxy;
        set
        {
            _selectedSavedProxy = value;
            OnPropertyChanged(nameof(SelectedSavedProxy));

            if (!string.IsNullOrEmpty(_selectedSavedProxy))
            {
                // Parse the selected proxy string and update PryGuardProf.Proxy
                ParseAndSetProxy(_selectedSavedProxy);
            }
        }
    }
    //private PryGuardProfile _PryGuardProf;
    //public PryGuardProfile PryGuardProf
    //{
    //    get => _PryGuardProf;
    //    set => Set(ref _PryGuardProf, value);
    //}
    #endregion

    #region Ctor
    public PryGuardProfileSettingsViewModel() { }

    public PryGuardProfileSettingsViewModel(PryGuardProfile PryGuardProfile)
    {
        CloseProfileSettingsCommand = new RelayCommand(CloseProfileSettings);
        SaveProfileCommand = new RelayCommand(SaveProfile);
        ChangeWindowStateCommand = new RelayCommand(CloseWindowState);
        CheckProxyCommand = new RelayCommand(CheckProxy);
        NewFingerprintCommand = new RelayCommand(GenerateNewFingerprint);
        ImportProfileCommand = new RelayCommand(ImportProfile);
        PryGuardProf = PryGuardProfile;
        Renderers = new ObservableCollection<string>(WebGLFactory.Renderers);
        LoadSavedProxies();
    }
    #endregion

    #region Window Work & Actions
    private void GenerateNewFingerprint(object parameter)
    {
        if (PryGuardProf != null)
        {
            var newFakeProfile = FakeProfileFactory.Generate();

            PryGuardProf.FakeProfile = newFakeProfile;



            PryGuardProfilesVM.Setting.SaveSettings();
        }
        else
        {
            // Handle the case where PryGuardProf is null
            // Notify UI of the issue
        }
    }
    private void SaveProfile(object arg)
    {
        // Find the profile by Id (if it exists) using FirstOrDefault in all cases
        var existingProfileTab = PryGuardProfilesVM.ProfileTabs.FirstOrDefault(tab => tab.Id == PryGuardProf.Id);

        if (!string.IsNullOrEmpty(PryGuardProf.Proxy.ProxyAddress))
        {
            // Construct the proxy string conditionally based on authentication
            string proxyString;
            if (!string.IsNullOrEmpty(PryGuardProf.Proxy.ProxyLogin) && !string.IsNullOrEmpty(PryGuardProf.Proxy.ProxyPassword))
            {
                // Include authentication details
                proxyString = $"{PryGuardProf.Proxy.ProxyAddress}:{PryGuardProf.Proxy.ProxyPort}:{PryGuardProf.Proxy.ProxyLogin}:{PryGuardProf.Proxy.ProxyPassword}";
            }
            else
            {
                // No authentication, exclude login and password
                proxyString = $"{PryGuardProf.Proxy.ProxyAddress}:{PryGuardProf.Proxy.ProxyPort}";
            }

            // Add the proxy string to the saved proxies if it's not already present
            if (!SavedProxies.Contains(proxyString))
            {
                SavedProxies.Add(proxyString);
                SaveSavedProxies();
            }
        }
        if (SaveProfileButtonContent == "Create" || SaveProfileButtonContent == "Import")
        {
            // Close the view and add a new profile tab
            ViewManager.Close(this);

            // If profile doesn't exist, add it as new
            if (existingProfileTab == null)
            {
                PryGuardProfilesVM.ProfileTabs.Add(new ProfileTab(PryGuardProfilesVM)
                {
                    Name = PryGuardProf.Name,
                    Id = PryGuardProf.Id,
                    Status = PryGuardProf.Status,
                    Tags = PryGuardProf.Tags,
                    ProxyHostPort = PryGuardProf.Proxy.ProxyAddress == "" && PryGuardProf.Proxy.ProxyPort == 8080 ? "" : PryGuardProf.Proxy.ProxyAddress + ":" + PryGuardProf.Proxy.ProxyPort,
                    ProxyLoginPass = PryGuardProf.Proxy.ProxyLogin == "" && PryGuardProf.Proxy.ProxyPassword == "" ? "" : PryGuardProf.Proxy.ProxyLogin + ":" + PryGuardProf.Proxy.ProxyPassword
                });
            }

            PryGuardProfilesVM.Setting.SaveSettings();
            PryGuardProfilesVM.ProfileTabs.Clear();
            PryGuardProfilesVM.LoadTabs();
        }
        else if (SaveProfileButtonContent == "Save" && PryGuardProf.Status == "NEW")
        {
            ViewManager.Close(this);

            if (existingProfileTab != null)
            {
                existingProfileTab.Status = "NEW";
                existingProfileTab.Name = PryGuardProf.Name;
                existingProfileTab.Tags = PryGuardProf.Tags;
                existingProfileTab.ProxyHostPort = PryGuardProf.Proxy.ProxyAddress == "" && PryGuardProf.Proxy.ProxyPort == 8080 ? "" : PryGuardProf.Proxy.ProxyAddress + ":" + PryGuardProf.Proxy.ProxyPort;
                existingProfileTab.ProxyLoginPass = PryGuardProf.Proxy.ProxyLogin == "" && PryGuardProf.Proxy.ProxyPassword == "" ? "" : PryGuardProf.Proxy.ProxyLogin + ":" + PryGuardProf.Proxy.ProxyPassword;
            }

            PryGuardProfilesVM.Setting.SaveSettings();
            PryGuardProfilesVM.ProfileTabs.Clear();
            PryGuardProfilesVM.LoadTabs();
        }
        else if (SaveProfileButtonContent == "Save" && PryGuardProf.Status == "BAN")
        {
            ViewManager.Close(this);

            if (existingProfileTab != null)
            {
                existingProfileTab.Status = "BAN";
                existingProfileTab.Name = PryGuardProf.Name;
                existingProfileTab.Tags = PryGuardProf.Tags;
                existingProfileTab.ProxyHostPort = PryGuardProf.Proxy.ProxyAddress == "" && PryGuardProf.Proxy.ProxyPort == 8080 ? "" : PryGuardProf.Proxy.ProxyAddress + ":" + PryGuardProf.Proxy.ProxyPort;
                existingProfileTab.ProxyLoginPass = PryGuardProf.Proxy.ProxyLogin == "" && PryGuardProf.Proxy.ProxyPassword == "" ? "" : PryGuardProf.Proxy.ProxyLogin + ":" + PryGuardProf.Proxy.ProxyPassword;
            }

            PryGuardProfilesVM.Setting.SaveSettings();
            PryGuardProfilesVM.ProfileTabs.Clear();
            PryGuardProfilesVM.LoadTabs();
        }
        else if (SaveProfileButtonContent == "Save" && PryGuardProf.Status == "READY")
        {
            ViewManager.Close(this);

            if (existingProfileTab != null)
            {
                existingProfileTab.Status = "READY";
                existingProfileTab.Name = PryGuardProf.Name;
                existingProfileTab.Tags = PryGuardProf.Tags;
                existingProfileTab.ProxyHostPort = PryGuardProf.Proxy.ProxyAddress == "" && PryGuardProf.Proxy.ProxyPort == 8080 ? "" : PryGuardProf.Proxy.ProxyAddress + ":" + PryGuardProf.Proxy.ProxyPort;
                existingProfileTab.ProxyLoginPass = PryGuardProf.Proxy.ProxyLogin == "" && PryGuardProf.Proxy.ProxyPassword == "" ? "" : PryGuardProf.Proxy.ProxyLogin + ":" + PryGuardProf.Proxy.ProxyPassword;
            }

            PryGuardProfilesVM.Setting.SaveSettings();
            PryGuardProfilesVM.ProfileTabs.Clear();
            PryGuardProfilesVM.LoadTabs();
        }
        else
        {
            PryGuardProf.Status = "UPDATED";
            ViewManager.Close(this);

            if (existingProfileTab != null)
            {
                existingProfileTab.Status = "UPDATED";
                existingProfileTab.Name = PryGuardProf.Name;
                existingProfileTab.Tags = PryGuardProf.Tags;
                existingProfileTab.ProxyHostPort = PryGuardProf.Proxy.ProxyAddress == "" && PryGuardProf.Proxy.ProxyPort == 8080 ? "" : PryGuardProf.Proxy.ProxyAddress + ":" + PryGuardProf.Proxy.ProxyPort;
                existingProfileTab.ProxyLoginPass = PryGuardProf.Proxy.ProxyLogin == "" && PryGuardProf.Proxy.ProxyPassword == "" ? "" : PryGuardProf.Proxy.ProxyLogin + ":" + PryGuardProf.Proxy.ProxyPassword;
            }

            PryGuardProfilesVM.Setting.SaveSettings();
            PryGuardProfilesVM.ProfileTabs.Clear();
            PryGuardProfilesVM.LoadTabs();
        }

        PryGuardProf.IsSaved = true;
    }
    private void ParseAndSetProxy(string proxyString)
    {
        var parts = proxyString.Split(':');

        if (parts.Length == 2 || parts.Length == 4)
        {
            // Create a new ProxySettings instance
            var proxySettings = new ProxySettings
            {
                ProxyAddress = parts[0],
                ProxyPort = int.TryParse(parts[1], out int port) ? port : 8080,
                IsCustomProxy = true
            };

            if (parts.Length == 4)
            {
                // Proxy with authentication
                proxySettings.ProxyLogin = parts[2];
                proxySettings.ProxyPassword = parts[3];
                proxySettings.IsProxyAuth = true;
            }
            else
            {
                // Proxy without authentication
                proxySettings.ProxyLogin = null;
                proxySettings.ProxyPassword = null;
                proxySettings.IsProxyAuth = false;
            }

            // Set the proxy type (adjust as needed)
            proxySettings.IsHTTP = true; // Assuming HTTP for simplicity
            proxySettings.IsSOCKS4 = false;
            proxySettings.IsSOCKS5 = false;

            // Update the profile's proxy settings
            PryGuardProf.Proxy = proxySettings;
        }
    }

    private void ImportProfile(object arg)
    {
        var savedProfiles = PryGuardProfilesVM.Setting.PryGuardProfiles.Where(p => p.IsSaved).ToList();
        var profileSelectionWindow = new ProfileSelectionWindow(savedProfiles);
        if (profileSelectionWindow.ShowDialog() == true)
        {

            if (PryGuardProfilesVM.Setting.PryGuardProfiles.Any())
            {
                var lastProfile = PryGuardProfilesVM.Setting.PryGuardProfiles.Last();
                PryGuardProfilesVM.Setting.PryGuardProfiles.Remove(lastProfile);


                var tabToRemove = PryGuardProfilesVM.ProfileTabs.FirstOrDefault(tab => tab.Id == lastProfile.Id);
                if (tabToRemove != null)
                {
                    PryGuardProfilesVM.ProfileTabs.Remove(tabToRemove);
                }
            }


            var importedProfile = PryGuardProfile.ImportFromProfile(profileSelectionWindow.SelectedProfile);
            PryGuardProfilesVM.Setting.PryGuardProfiles.Add(importedProfile);


            PryGuardProf = importedProfile;


            SaveProfileButtonContent = "Import";
        }
    }

    private async void CheckProxy()
    {
        var a = PryGuardProf.Proxy.ProxyAddress;
        if (PryGuardProf.Proxy.ProxyAddress == "") return;
        var result = await IpInfoServiceClient.FetchProxyInfoAsync(PryGuardProf.Proxy);
        if (result == null)
        {
            TbProxyBrush = Brushes.Red;
            await Task.Delay(2000);
            TbProxyBrush = Brushes.White;
            return;
        }
        if (result.IpAddress == PryGuardProf.Proxy.ProxyAddress)
        {
            TbProxyBrush = Brushes.Green;
            await Task.Delay(2000);
            TbProxyBrush = Brushes.White;
        }
    }

    private void CloseProfileSettings(object arg)
    {
        if (!PryGuardProf.IsSaved && !IsEdit)
        {
            // Remove the unsaved profile from the list
            PryGuardProfilesVM.Setting.PryGuardProfiles.Remove(PryGuardProf);
        }
        ViewManager.Close(this);
    }

    private void CloseWindowState(object arg)
    {
        WindowState = WindowState.Minimized;
    }
    #endregion

    #region proxywork
    public void LoadSavedProxies()
    {
        var path = Path.Combine(ClientConfig.ChromeDataPath, "SavedProxies.txt");
        if (File.Exists(path))
        {
            var lines = File.ReadAllLines(path);
            SavedProxies = new ObservableCollection<string>(lines);
        }
        else
        {
            SavedProxies = new ObservableCollection<string>();
        }
    }


    public void SaveSavedProxies()
    {
        var path = Path.Combine(ClientConfig.ChromeDataPath, "SavedProxies.txt");
        File.WriteAllLines(path, SavedProxies);
    }

    #endregion
}