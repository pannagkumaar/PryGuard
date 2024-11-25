using System;
using System.IO;
using System.Linq;
using PryGuard.Model;
using System.Windows;
using System.Threading.Tasks;
using PryGuard.Services.Commands;
using PryGuard.Services.Settings;
using System.Collections.ObjectModel;

using PryGuard.Services.Helpers;


namespace PryGuard.ViewModel;
public class PryGuardProfilesViewModel : BaseViewModel
{
    private bool _isWindowMaximized = false;

    #region Commands
    public RelayCommand CloseAppCommand { get; private set; }
    public RelayCommand CreateProfileCommand { get; private set; }
    public RelayCommand ToggleWindowStateCommand { get; private set; }
    public RelayCommand StartProfileCommand { get; private set; }
    public RelayCommand EditProfileCommand { get; private set; }
    public RelayCommand DeleteProfileCommand { get; private set; }
    public RelayCommand RefreshProfilesCommand { get; private set; }
    public RelayCommand ChangeWindowStateCommand { get; private set; }
    public RelayCommand MaximizeWindowStateCommand { get; private set; }
    #endregion

    #region Properties
    public ObservableCollection<string> SavedProxies { get; set; }
    private ObservableCollection<ProfileTab> _profileTabs;
    public ObservableCollection<ProfileTab> ProfileTabs
    {
        get => _profileTabs;
        set => Set(ref _profileTabs, value);
    }

    private WindowState _windowState;
    public WindowState WindowState
    {
        get => _windowState;
        set => Set(ref _windowState, value);
    }

    public bool IsWindowMaximized
    {
        get => _isWindowMaximized;
        set => Set(ref _isWindowMaximized, value);
    }

    private string _searchTerm;
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm != value)
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm)); 
                FilterProfiles();
            }
        }
    }



    private PryGuardProfileSettingsViewModel _PryGuardProfileSettingsVM;
    public PryGuardProfileSettingsViewModel PryGuardProfileSettingsVM
    {
        get => _PryGuardProfileSettingsVM;
        set => Set(ref _PryGuardProfileSettingsVM, value);
    }

    private PryGuardBrowserViewModel _PryGuardBrowserViewModelVM;
    public PryGuardBrowserViewModel PryGuardBrowserViewModelVM
    {
        get => _PryGuardBrowserViewModelVM;
        set => Set(ref _PryGuardBrowserViewModelVM, value);
    }

    private Setting _setting;
    public Setting Setting
    {
        get => _setting;
        set => Set(ref _setting, value);
    }
    #endregion

    #region Ctor
    public PryGuardProfilesViewModel()
    {
        CreateProfileCommand = new RelayCommand(CreateProfile);
        ChangeWindowStateCommand = new RelayCommand(CloseWindowState);
        MaximizeWindowStateCommand = new RelayCommand(MaximizeWindowState);
        ToggleWindowStateCommand = new RelayCommand(ToggleWindowState);
        CloseAppCommand = new RelayCommand(CloseApp);
        StartProfileCommand = new RelayCommand(StartProfile);
        EditProfileCommand = new RelayCommand(EditProfile);
        DeleteProfileCommand = new RelayCommand(DeleteProfile);
        RefreshProfilesCommand = new RelayCommand(RefreshProfiles);
        ProfileTabs = new();
        Setting = new();
        Task.Run(() => { LoadTabs(); });
        
    }
    #endregion

    #region Profile Work
    private void StartProfile(object arg)
    {
        PryGuardBrowserViewModelVM = new(Setting.PryGuardProfiles.Where(x => x.Id == (int)arg).First());
        ViewManager.Show(PryGuardBrowserViewModelVM);
        Setting.SaveSettings();
    }
    private void CreateProfile(object arg)
    {
        Setting.PryGuardProfiles.Add(PryGuardProfile.GenerateNewProfile("Profile"));
        PryGuardProfileSettingsVM = new PryGuardProfileSettingsViewModel(Setting.PryGuardProfiles.Last())
        {
            IsEdit = false // Set IsEdit to false for new profiles
        };
        this.NextStep(PryGuardProfileSettingsVM);
        PryGuardProfileSettingsVM.PryGuardProfilesVM = this;
    }

    private void DeleteProfile(object arg)
    {
        LoadSavedProxies(); 
        int profileIdToDelete = (int)arg;

        // Find the profile with the exact matching ID
        var profileToRemove = Setting.PryGuardProfiles.FirstOrDefault(profile => profile.Id == profileIdToDelete);

        string proxyToRemove = null;

        if (profileToRemove != null)
        {
            // Get the proxy associated with the profile
            proxyToRemove = ConstructProxyString(profileToRemove.Proxy);

            if (Directory.Exists(profileToRemove.CachePath))
            {
                Directory.Delete(profileToRemove.CachePath, true);
            }
            Setting.PryGuardProfiles.Remove(profileToRemove);
        }
        else
        {
            // Handle the case where the profile isn't found
            // Perhaps log a warning or notify the user
        }

        var tabToRemove = ProfileTabs.FirstOrDefault(item => item.Id == profileIdToDelete);
        if (tabToRemove != null)
        {
            ProfileTabs.Remove(tabToRemove);
        }

        // Now, remove the proxy if it's not used by any other profiles
        if (!string.IsNullOrEmpty(proxyToRemove))
        {
            bool isProxyUsedElsewhere = IsProxyUsedElsewhere(proxyToRemove, profileIdToDelete);

            if (!isProxyUsedElsewhere)
            {
                
                // Remove the proxy from SavedProxies
                if (SavedProxies.Contains(proxyToRemove))
                {
                    int length = SavedProxies.Count;
                    SavedProxies.Remove(proxyToRemove);

                     length = SavedProxies.Count;
                    // Save the updated list of saved proxies
                    SaveSavedProxies();
                }
            }
        }

        Setting.SaveSettings();
    }
    // Method to construct the proxy string from ProxySettings
private string ConstructProxyString(ProxySettings proxy)
{
    if (proxy == null || string.IsNullOrEmpty(proxy.ProxyAddress))
        return null;

    if (!string.IsNullOrEmpty(proxy.ProxyLogin) && !string.IsNullOrEmpty(proxy.ProxyPassword))
    {
        return $"{proxy.ProxyAddress}:{proxy.ProxyPort}:{proxy.ProxyLogin}:{proxy.ProxyPassword}";
    }
    else
    {
        return $"{proxy.ProxyAddress}:{proxy.ProxyPort}";
    }
}

// Method to check if the proxy is used by other profiles
private bool IsProxyUsedElsewhere(string proxyString, int excludingProfileId)
{
    return Setting.PryGuardProfiles.Any(profile =>
    {
        if (profile.Id == excludingProfileId)
            return false;

        string profileProxyString = ConstructProxyString(profile.Proxy);
        return proxyString == profileProxyString;
    });
}


    private void RefreshProfiles(object arg)
    {
        ProfileTabs.Clear();
        LoadTabs();
    }
    private void EditProfile(object arg)
    {
        var profileToEdit = Setting.PryGuardProfiles.FirstOrDefault(x => x.Id == (int)arg);
        if (profileToEdit != null)
        {
            PryGuardProfileSettingsVM = new PryGuardProfileSettingsViewModel(profileToEdit)
            {
                SaveProfileButtonContent = "Save",
                IsEdit = true // Set the IsEdit flag to true
            };
            this.NextStep(PryGuardProfileSettingsVM);
            PryGuardProfileSettingsVM.PryGuardProfilesVM = this;
        }
        else
        {
            // Handle the case where the profile is not found
        }
    }

    #endregion

    #region Window Work & Actions
    private void FilterProfiles()
    {
        var filteredProfiles = Setting.PryGuardProfiles
            .Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();

        ProfileTabs.Clear();
        foreach (var profile in filteredProfiles)
        {
            ProfileTabs.Add(new ProfileTab(this)
            {
                Name = profile.Name,
                Id = profile.Id,
                Status = profile.Status,
                Tags = profile.Tags,
                ProxyHostPort = profile.Proxy.ProxyAddress == "" && profile.Proxy.ProxyPort == 8080 ? "" : profile.Proxy.ProxyAddress + ":" + profile.Proxy.ProxyPort,
                ProxyLoginPass = profile.Proxy.ProxyLogin == "" && profile.Proxy.ProxyPassword == "" ? "" : profile.Proxy.ProxyLogin + ":" + profile.Proxy.ProxyPassword
            });
        }
    }
    public void MoveProfile(ProfileTab sourceProfile, ProfileTab targetProfile)
    {
        var sourceIndex = ProfileTabs.IndexOf(sourceProfile);
        var targetIndex = ProfileTabs.IndexOf(targetProfile);

        if (sourceIndex != -1 && targetIndex != -1 && sourceIndex != targetIndex)
        {
            ProfileTabs.Move(sourceIndex, targetIndex);
            // Save the new order to your settings if needed
            Setting.SaveSettings();
        }
    }
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
    public void LoadTabs()
    {
        foreach (var item in Setting.PryGuardProfiles)
        {

            ProfileTabs.Add(new ProfileTab(this)
            {
                Name = item.Name,
                Id = item.Id,
                Status = item.Status,
                Tags = item.Tags,
                //ProxyHostPort = item.Proxy.ProxyAddress + ":" + item.Proxy.ProxyPort,
                //ProxyLoginPass = item.Proxy.ProxyLogin + ":" + item.Proxy.ProxyPassword
                ProxyHostPort = item.Proxy.ProxyAddress == "" && item.Proxy.ProxyPort == 8080 ? "" : item.Proxy.ProxyAddress + ":" + item.Proxy.ProxyPort,
                ProxyLoginPass = item.Proxy.ProxyLogin == "" && item.Proxy.ProxyPassword == "" ? "" : item.Proxy.ProxyLogin + ":" + item.Proxy.ProxyPassword
            });
        }
    }
    private void CloseWindowState(object arg)
    {
        WindowState = WindowState.Minimized;
    }
    private void CloseApp(object arg)
    {
        Environment.Exit(0);
    }

    private void MaximizeWindowState()
    {
        WindowState = WindowState.Maximized;
        IsWindowMaximized = true;
    }

    private void RestoreWindowState()
    {
        WindowState = WindowState.Normal;
        IsWindowMaximized = false;
    }

    private void ToggleWindowState()
    {
        if (IsWindowMaximized)
        {
            RestoreWindowState();
        }
        else
        {
            MaximizeWindowState();
        }
    }

    #endregion
}