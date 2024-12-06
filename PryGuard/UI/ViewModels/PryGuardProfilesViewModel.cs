using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PryGuard.Resources.Commands;
using PryGuard.Resources.Settings;
using PryGuard.DataModels;
using PryGuard.Resources.Helpers;

namespace PryGuard.UI.ViewModels
{
    public class PryGuardProfilesViewModel : BaseViewModel, IDisposable
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

        private SettingsManager _setting;
        public SettingsManager Setting
        {
            get => _setting;
            set => Set(ref _setting, value);
        }

        // Action to request window close
        public Action RequestClose { get; set; }
        #endregion

        #region Ctor
        public PryGuardProfilesViewModel()
        {
            // Initialize commands
            CreateProfileCommand = new RelayCommand(CreateProfile);
            ChangeWindowStateCommand = new RelayCommand(CloseWindowState);
            MaximizeWindowStateCommand = new RelayCommand(MaximizeWindowState);
            ToggleWindowStateCommand = new RelayCommand(ToggleWindowState);
            CloseAppCommand = new RelayCommand(CloseApp);
            StartProfileCommand = new RelayCommand(StartProfile);
            EditProfileCommand = new RelayCommand(EditProfile);
            DeleteProfileCommand = new RelayCommand(DeleteProfile);
            RefreshProfilesCommand = new RelayCommand(RefreshProfiles);

            // Initialize collections
            ProfileTabs = new ObservableCollection<ProfileTab>();
            SavedProxies = new ObservableCollection<string>();

            Setting = new SettingsManager();

            // Start asynchronous initialization
            InitializeAsync();
        }
        #endregion

        #region Initialization
        private async void InitializeAsync()
        {
            await Task.Run(() => { LoadTabs(); });
        }
        #endregion

        #region Profile Work
        private void StartProfile(object arg)
        {
            if (arg is not int profileId)
            {
                MessageBox.Show("Invalid profile identifier.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedProfile = Setting.PryGuardProfiles.FirstOrDefault(p => p.Id == profileId);
            if (selectedProfile != null)
            {
                PryGuardBrowserViewModelVM = new PryGuardBrowserViewModel(selectedProfile);
                ViewManager.Show(PryGuardBrowserViewModelVM);
                Setting.SaveSettings();
            }
            else
            {
                // Handle profile not found
                MessageBox.Show("Profile not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateProfile(object arg)
        {
            var newProfile = PryGuardProfile.GenerateNewProfile("Profile");
            Setting.PryGuardProfiles.Add(newProfile);
            PryGuardProfileSettingsVM = new PryGuardProfileSettingsViewModel(newProfile)
            {
                IsEdit = false // Set IsEdit to false for new profiles
            };
            NextStep(PryGuardProfileSettingsVM);
            PryGuardProfileSettingsVM.PryGuardProfilesVM = this;
        }

        private void DeleteProfile(object arg)
        {
            LoadSavedProxies();
            if (arg is not int profileIdToDelete)
            {
                MessageBox.Show("Invalid profile identifier.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Find the profile with the exact matching ID
            var profileToRemove = Setting.PryGuardProfiles.FirstOrDefault(profile => profile.Id == profileIdToDelete);

            string proxyToRemove = null;

            if (profileToRemove != null)
            {
                // Get the proxy associated with the profile
                proxyToRemove = ConstructProxyString(profileToRemove.Proxy);

                if (Directory.Exists(profileToRemove.CachePath))
                {
                    try
                    {
                        Directory.Delete(profileToRemove.CachePath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete cache directory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                Setting.PryGuardProfiles.Remove(profileToRemove);
            }
            else
            {
                // Handle the case where the profile isn't found
                MessageBox.Show("Profile not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
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
                        SavedProxies.Remove(proxyToRemove);

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
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProfileTabs.Clear();
                LoadTabs();
            });
        }

        private void EditProfile(object arg)
        {
            if (arg is not int profileId)
            {
                MessageBox.Show("Invalid profile identifier.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var profileToEdit = Setting.PryGuardProfiles.FirstOrDefault(x => x.Id == profileId);
            if (profileToEdit != null)
            {
                PryGuardProfileSettingsVM = new PryGuardProfileSettingsViewModel(profileToEdit)
                {
                    SaveProfileButtonContent = "Save",
                    IsEdit = true // Set the IsEdit flag to true
                };
                NextStep(PryGuardProfileSettingsVM);
                PryGuardProfileSettingsVM.PryGuardProfilesVM = this;
            }
            else
            {
                // Handle the case where the profile is not found
                MessageBox.Show("Profile not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Window Work & Actions
        private void FilterProfiles()
        {
            var filteredProfiles = Setting.PryGuardProfiles
                .Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ProfileTabs.Clear();
                foreach (var profile in filteredProfiles)
                {
                    ProfileTabs.Add(new ProfileTab(this)
                    {
                        Name = profile.Name,
                        Id = profile.Id,
                        Status = profile.Status,
                        Tags = profile.Tags,
                        ProxyHostPort = string.IsNullOrEmpty(profile.Proxy.ProxyAddress) && profile.Proxy.ProxyPort == 8080
                            ? ""
                            : $"{profile.Proxy.ProxyAddress}:{profile.Proxy.ProxyPort}",
                        ProxyLoginPass = string.IsNullOrEmpty(profile.Proxy.ProxyLogin) && string.IsNullOrEmpty(profile.Proxy.ProxyPassword)
                            ? ""
                            : $"{profile.Proxy.ProxyLogin}:{profile.Proxy.ProxyPassword}"
                    });
                }
            });
        }

        public void MoveProfile(ProfileTab sourceProfile, ProfileTab targetProfile)
        {
            var sourceIndex = ProfileTabs.IndexOf(sourceProfile);
            var targetIndex = ProfileTabs.IndexOf(targetProfile);

            if (sourceIndex != -1 && targetIndex != -1 && sourceIndex != targetIndex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProfileTabs.Move(sourceIndex, targetIndex);
                });

                // Save the new order to your settings if needed
                Setting.SaveSettings();
            }
        }

        public void LoadSavedProxies()
        {
            var path = Path.Combine(ClientConfig.ChromeDataPath, "SavedProxies.txt");
            Application.Current.Dispatcher.Invoke(() =>
            {
                SavedProxies.Clear();
                if (File.Exists(path))
                {
                    var lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        SavedProxies.Add(line);
                    }
                }
            });
        }

        public void SaveSavedProxies()
        {
            var path = Path.Combine(ClientConfig.ChromeDataPath, "SavedProxies.txt");
            try
            {
                File.WriteAllLines(path, SavedProxies);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save proxies: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadTabs()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in Setting.PryGuardProfiles)
                {
                    ProfileTabs.Add(new ProfileTab(this)
                    {
                        Name = item.Name,
                        Id = item.Id,
                        Status = item.Status,
                        Tags = item.Tags,
                        ProxyHostPort = string.IsNullOrEmpty(item.Proxy.ProxyAddress) && item.Proxy.ProxyPort == 8080
                            ? ""
                            : $"{item.Proxy.ProxyAddress}:{item.Proxy.ProxyPort}",
                        ProxyLoginPass = string.IsNullOrEmpty(item.Proxy.ProxyLogin) && string.IsNullOrEmpty(item.Proxy.ProxyPassword)
                            ? ""
                            : $"{item.Proxy.ProxyLogin}:{item.Proxy.ProxyPassword}"
                    });
                }
            });
        }

        private void CloseWindowState(object arg)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseApp(object arg)
        {
            // Instead of Environment.Exit(0), request the window to close
            RequestClose?.Invoke();
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Log disposal
                    Debug.WriteLine("Disposing PryGuardProfilesViewModel.");

                    // Clear collections to release references
                    ProfileTabs?.Clear();
                    SavedProxies?.Clear();

                    // Dispose child ViewModels if they implement IDisposable


                    // Unsubscribe from events if any (example)
                    // someService.SomeEvent -= OnSomeEvent;
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
