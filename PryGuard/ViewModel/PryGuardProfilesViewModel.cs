using System;
using System.IO;
using System.Linq;
using PryGuard.Model;
using System.Windows;
using System.Threading.Tasks;
using PryGuard.Services.Commands;
using PryGuard.Services.Settings;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PryGuard.ViewModel;
public class PryGuardProfilesViewModel : BaseViewModel
{
    #region Commands
    public RelayCommand CloseAppCommand { get; private set; }
    public RelayCommand CreateProfileCommand { get; private set; }
    public RelayCommand StartProfileCommand { get; private set; }
    public RelayCommand EditProfileCommand { get; private set; }
    public RelayCommand DeleteProfileCommand { get; private set; }
    public RelayCommand RefreshProfilesCommand { get; private set; }
    public RelayCommand ChangeWindowStateCommand { get; private set; }
    public RelayCommand MaximizeWindowStateCommand { get; private set; }
    #endregion

    #region Properties
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
        PryGuardProfileSettingsVM = new PryGuardProfileSettingsViewModel(Setting.PryGuardProfiles.Last());
        this.NextStep(PryGuardProfileSettingsVM);
        PryGuardProfileSettingsVM.PryGuardProfilesVM = this;
    }
    private void DeleteProfile(object arg)
    {
        var profilesToRemove = Setting.PryGuardProfiles.Where(profile => profile.Id == (int)arg).ToList();
        foreach (var profile in profilesToRemove)
        {
            if (Directory.Exists(profile.CachePath))
            {
                Directory.Delete(profile.CachePath, true);
            }
            Setting.PryGuardProfiles.Remove(profile);
        }

        var tabsToRemove = ProfileTabs.Where(item => item.Id == (int)arg).ToList();
        foreach (var item in tabsToRemove)
        {
            ProfileTabs.Remove(item);
        }

        Setting.SaveSettings();
    }
    private void RefreshProfiles(object arg)
    {
        ProfileTabs.Clear();
        LoadTabs();
    }
    private void EditProfile(object arg)
    {
        PryGuardProfileSettingsVM = new PryGuardProfileSettingsViewModel(
            Setting.PryGuardProfiles.Where(x => x.Id == (int)arg).First())
        {
            SaveProfileButtonContent = "Save"
        };
        this.NextStep(PryGuardProfileSettingsVM);
        PryGuardProfileSettingsVM.PryGuardProfilesVM = this;
    }
    #endregion

    #region Window Work & Actions
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
    private void MaximizeWindowState(object arg)
    {
        WindowState = WindowState.Maximized;
    }
    private void CloseApp(object arg)
    {
        Environment.Exit(0);
    }
    #endregion
}