using PryGuard.Model;
using System.Windows;
using PryGuard.Core.Web;
using System.Windows.Media;
using System.Threading.Tasks;
using PryGuard.Services.Commands;
using PryGuard.Core.ChromeApi.Model.Configs;
using System.Windows.Input;
using PryGuard.Core.ChromeApi.Settings;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PryGuard.View;
using CefSharp.DevTools.Profiler;
using PryGuard.Services.Helpers;
using System.Xml.Linq;
using PryGuard.Model;
using System.Linq;

namespace PryGuard.ViewModel;
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

    public EChromeLanguage SelectedLanguage
    {
        get => PryGuardProf?.FakeProfile?.ChromeLanguageInfo?.Language ?? EChromeLanguage.EnUsa;
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
        var result = await IpInfoClient.CheckClientProxy(PryGuardProf.Proxy);
        if (result == null)
        {
            TbProxyBrush = Brushes.Red;
            await Task.Delay(2000);
            TbProxyBrush = Brushes.White;
            return;
        }
        if (result.Ip == PryGuardProf.Proxy.ProxyAddress)
        {
            TbProxyBrush = Brushes.Green;
            await Task.Delay(2000);
            TbProxyBrush = Brushes.White;
        }
    }

    private void CloseProfileSettings(object arg)
    {
        ViewManager.Close(this);
    }
    private void CloseWindowState(object arg)
    {
        WindowState = WindowState.Minimized;
    }
    #endregion
}