using CefSharp;
using CefSharp.ModelBinding;
using CefSharp.Wpf;
using PryGuard.Core.Browser;
using PryGuard.Core.Browser.Handlers;
using PryGuard.Core.Browser.Model.Configs;
using PryGuard.Core.Browser.Proxy;
using PryGuard.Core.Network;
using PryGuard.DataModels;
using PryGuard.Resources.Commands;
using PryGuard.UI.Controls.Buttons;
using PryGuard.UI.Views;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;


namespace PryGuard.UI.ViewModels;

public class PryGuardBrowserViewModel : BaseViewModel
{
    #region Fields
    private int _mainIDCounter;
    private DelegateCommand _closeCommand;
    private Label _tabBtnToDrag;
    private readonly string _profileHistoryPath;
    private ListView _listView;
    private DownloadManager _downloadManager;


    #region BrowserSettings & Other
    private List<PryGuardBrowser> _browsers;
    private PryGuardProfile _PryGuardProfileToStart;
    //private Window _window;
    private PryGuardProfile _PryGuardProfile;
    private IpInformation _proxyInfo;
    private BlockManager _blockManager;
    private NativeSourceManager _nativeManager;
    private RequestHandler _requestHandler;
    private RequestContextSettings _requestContextSettings;
    private RequestContext _context;
    private LifespanHandler _lifespanHandler;
    private RenderMessageHandler _renderMessageHandler;
    private LoadHandler _loadHandler;
    private JsWorker _jsWorker;
    private PryGuardBrowser _previousBrowser;
    private DispatcherTimer _suspensionTimer;
    private readonly TimeSpan TabInactivityTimeout = TimeSpan.FromMinutes(1);

    #endregion
    #endregion

    #region Commands
    public RelayCommand MinimizeWindowCommand { get; private set; }
    public RelayCommand MaximizeWindowCommand { get; private set; }
    public RelayCommand NormalStateWindowCommand { get; private set; }
    public RelayCommand AddTabCommand { get; private set; }
    public RelayCommand AddIncognitoTabCommand { get; private set; }
    public RelayCommand OpenTabCommand { get; private set; }
    public RelayCommand CloseTabCommand { get; private set; }
    public RelayCommand<PryGuardHistoryItem> DeleteHistoryItemCommand { get; set; }
    public RelayCommand<Bookmark> RemoveBookmarkCommand { get; set; }
    public RelayCommand RefreshCommand { get; private set; }
    public RelayCommand ForwardCommand { get; private set; }
    public RelayCommand BackCommand { get; private set; }
    public RelayCommand OpenHistoryCommand { get; private set; }
    public RelayCommand AddBookmarkTabCommand { get; private set; }
    public RelayCommand AddDownloadTabCommand { get; private set; }
    public RelayCommand LoadHistoryLinkCommand { get; private set; }
    public RelayCommand DeleteHistoryCommand { get; private set; }
    public RelayCommand AddressOnKeyDownCommand { get; private set; }
    public RelayCommand OpenContextMenuSettingsCommand { get; private set; }

    public RelayCommand OpenEmailCommand { get; private set; }

    public DelegateCommand CloseCommand =>
          _closeCommand ?? (_closeCommand = new DelegateCommand(obj => CloseWindow(obj)));
    public ICommand AddBookmarkCommand { get; }
    public ICommand OpenBookmarkCommand { get; }
    public ICommand OpenDevToolsCommand { get; }
    public ICommand ToggleIncognitoModeCommand { get; }
    public ICommand ShowFindBarCommand { get; }
    public ICommand CloseFindBarCommand { get; }
    public ICommand FindNextCommand { get; }
    public ICommand FindPreviousCommand { get; }
    public ICommand ToggleAutoSuspendCommand { get; }
    public Action FocusSearchTextBoxAction { get; set; }


    #endregion

    #region Properties
    private bool _isFindBarVisible;
    public bool IsFindBarVisible
    {
        get => _isFindBarVisible;
        set
        {
            _isFindBarVisible = value;
            OnPropertyChanged(nameof(IsFindBarVisible));
        }
    }

    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged(nameof(SearchText));
            PerformSearch(); // Perform search as the user types
        }
    }

    private string _cookies;
    public string Cookies
    {
        get => _cookies;
        set
        {
            if (_cookies == value)
                return;
            _cookies = value;
            OnPropertyChanged(nameof(Cookies));
        }
    }
    private bool _isIncognitoMode;
    public bool IsIncognitoMode
    {
        get => _isIncognitoMode;
        set
        {
            _isIncognitoMode = value;
            OnPropertyChanged(nameof(IsIncognitoMode));
            OnPropertyChanged(nameof(IncognitoModeText)); // Updates text dynamically
        }
    }
    public string IncognitoModeText => IsIncognitoMode ? "Incognito off" : "Incognito on";

    private bool _isAutoSuspendEnabled = false;//default to disabled
    public bool IsAutoSuspendEnabled
    {
        get => _isAutoSuspendEnabled;
        set
        {
            if (_isAutoSuspendEnabled != value)
            {
                _isAutoSuspendEnabled = value;
                OnPropertyChanged(nameof(IsAutoSuspendEnabled));
                OnPropertyChanged(nameof(SmartTabSuspensionText));

                // Optionally, update timer behavior immediately
                if (_isAutoSuspendEnabled)
                {
                    _suspensionTimer.Start();
                }
                else
                {
                    _suspensionTimer.Stop();
                }
            }
        }
    }
    public string SmartTabSuspensionText => IsAutoSuspendEnabled ? "Auto Suspend: On" : "Auto Suspend: Off";

    private Dictionary<int, string> _incognitoCache = new Dictionary<int, string>();

    private CustomTabItem _currentTabItem;
    public CustomTabItem CurrentTabItem
    {
        get => _currentTabItem;
        set
        {
            Set(ref _currentTabItem, value);
            OnCurrentTabItemChanged();
        }
    }




    private BookmarkManager _bookmarkManager;

    public ObservableCollection<Bookmark> Bookmarks => _bookmarkManager?.Bookmarks;


    private ObservableCollection<PryGuardHistoryItem> _pryGuardHistoryList = new ObservableCollection<PryGuardHistoryItem>();

    public ObservableCollection<PryGuardHistoryItem> PryGuardHistoryList
    {
        get => _pryGuardHistoryList;
        set => Set(ref _pryGuardHistoryList, value);
    }




    private string _address;
    public string Address
    {
        get => _address;
        set
        {
            if (_address != value)
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }
    }

    private ObservableCollection<CustomTabItem> tabs;
    public ObservableCollection<CustomTabItem> Tabs
    {
        get => tabs;
        set => Set(ref tabs, value);
    }

    private ObservableCollection<UIElement> _tabBtnsAndAddTabBtn;
    public ObservableCollection<UIElement> TabBtnsAndAddTabBtn
    {
        get => _tabBtnsAndAddTabBtn;
        set => Set(ref _tabBtnsAndAddTabBtn, value);
    }

    public Action Close { get; set; }

    private WindowState _curWindowState;
    public WindowState CurWindowState
    {
        get => _curWindowState;
        set => Set(ref _curWindowState, value);
    }


    #endregion

    #region Ctor
    public PryGuardBrowserViewModel() { }
    public PryGuardBrowserViewModel(PryGuardProfile PryGuardProfileToStart)
    {


        _PryGuardProfileToStart = PryGuardProfileToStart;

        _mainIDCounter = 0;
        Tabs = new();
        _browsers = new();
        _listView = new();
        PryGuardHistoryList = new();
        _profileHistoryPath = _PryGuardProfileToStart.CachePath + "\\History.json";
        _bookmarkManager = new BookmarkManager(_PryGuardProfileToStart.CachePath);
        AddBookmarkCommand = new RelayCommand(AddBookmark);
        RemoveBookmarkCommand = new RelayCommand<Bookmark>(RemoveBookmark);
        OpenBookmarkCommand = new RelayCommand<Bookmark>(OpenBookmark);
        MinimizeWindowCommand = new RelayCommand(MinimizedWindow);
        MaximizeWindowCommand = new RelayCommand(MaximizedWindow);
        DeleteHistoryCommand = new RelayCommand(DeleteHistory);
        NormalStateWindowCommand = new RelayCommand(NormalStateWindow);
        AddTabCommand = new RelayCommand(AddTab);
        AddIncognitoTabCommand = new RelayCommand(AddIncognitoTab);
        OpenTabCommand = new RelayCommand(OpenTab);
        CloseTabCommand = new RelayCommand(CloseTab);
        AddBookmarkTabCommand = new RelayCommand(AddTabBookmark);
        AddDownloadTabCommand = new RelayCommand(AddDownloadTab);
        RefreshCommand = new RelayCommand(Refresh);
        ForwardCommand = new RelayCommand(Forward);
        BackCommand = new RelayCommand(Back);
        AddressOnKeyDownCommand = new RelayCommand(AddressOnKeyDown);
        LoadHistoryLinkCommand = new RelayCommand(LoadHistoryLink);
        DeleteHistoryItemCommand = new RelayCommand<PryGuardHistoryItem>(DeleteHistoryItem);
        OpenHistoryCommand = new RelayCommand(AddTabHistory);
        OpenContextMenuSettingsCommand = new RelayCommand(OpenContextMenuSettings);
        ToggleIncognitoModeCommand = new RelayCommand(ToggleIncognitoMode);
        OpenDevToolsCommand = new RelayCommand(OpenDevTools);
        ShowFindBarCommand = new RelayCommand(ShowFindBar);
        CloseFindBarCommand = new RelayCommand(CloseFindBar);
        FindNextCommand = new RelayCommand(FindNext);
        FindPreviousCommand = new RelayCommand(FindPrevious);
        OpenEmailCommand = new RelayCommand(OpenEmail);
        CurWindowState = WindowState.Maximized;
        ToggleAutoSuspendCommand = new RelayCommand(ToggleAutoSuspend);
        InitializeSuspensionTimer();



        try
        {
            ChromiumInit.Init(PryGuardProfileToStart);
        }
        catch (Exception ex)
        {
            File.WriteAllText("fail.txt", ex.Message);
            throw;
        }

        LoadHistoryJsonAsync();
        TabBtnsAndAddTabBtn = new() { InitAddTabBtn.CreateBtn(AddTab) };
        AddTab();
    }
    #endregion

    #region PryGuardBrowser Work
    private async Task<PryGuardBrowser> InitBrowser(bool isNewPage, int id)
    {
        PryGuardBrowser browser = await CreateBrowser(isNewPage, id, _PryGuardProfileToStart);
        _browsers.Add(browser);
        if (browser.IsBrowserInitialized)
        {
            await ParseAndInsertCookies(_PryGuardProfile.Cookies, _PryGuardProfile, browser);
        }
        else
        {
            browser.IsBrowserInitializedChanged += async (s, e) =>
            {
                if (browser.IsBrowserInitialized)
                {
                    await ParseAndInsertCookies(_PryGuardProfile.Cookies, _PryGuardProfile, browser);
                }
            };
        }

        return browser;
    }
    private async Task<PryGuardBrowser> InitIncognitoBrowser(int id)
    {
        var browser = await CreateBrowser(isNewPage: true, id: id, profile: _PryGuardProfileToStart, incognitoCache: _incognitoCache);
        browser.IsIncognito = true; // Set incognito mode
        _browsers.Add(browser);
        // Do not increment _mainIDCounter here
        return browser;
    }

    private void ToggleAutoSuspend()
    {
        IsAutoSuspendEnabled = !IsAutoSuspendEnabled;
    }
    private void InitializeSuspensionTimer()
    {
        _suspensionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1) // Check every minute
        };
        _suspensionTimer.Tick += SuspensionTimer_Tick;
        _suspensionTimer.Start();
    }


    private void SuspensionTimer_Tick(object sender, EventArgs e)
    {
        foreach (var tab in Tabs.ToList()) // Use ToList to avoid collection modification during iteration
        {
            if (tab.IsSuspended)
                continue; // Skip already suspended tabs

            // Skip the active tab
            if (tab == CurrentTabItem)
                continue;

            // Check inactivity
            if (DateTime.Now - tab.LastUsed > TabInactivityTimeout)
            {
                SuspendTab(tab);
            }
        }
    }

    private void SuspendTab(CustomTabItem tab)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (tab.Content is PryGuardBrowser browser)
            {
                // Unsubscribe event handlers
                browser.TitleChanged -= Browser_TitleChanged;
                browser.LoadingStateChanged -= Browser_LoadingStateChanged;
                browser.AddressChanged -= Browser_AddressChanged;
                browser.PreviewKeyDown -= Browser_PreviewKeyDown;

                // Store the current URL
                tab.SuspendedUrl = browser.Address;

                // Dispose of the browser to free resources
                browser.Dispose();

                // Replace content with a placeholder
                tab.Content = new TextBlock
                {
                    Text = "Tab Suspended. Click to reload.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    /*Foreground = Brushes.Gray */// Optional: Gray out the text
                };

                tab.IsSuspended = true;

                // Update the tab button's content to indicate suspension
                var tabBtn = TabBtnsAndAddTabBtn.OfType<Label>().FirstOrDefault(item => (int)item.Tag == tab.Tag);
                if (tabBtn != null)
                {
                    string suspendedContent = tab.IsIncognito ? $"Suspended Inco {tab.Title}" : $"Suspended {tab.Title}";
                    tabBtn.Content = suspendedContent;
                    /* tabBtn.Foreground = Brushes.Gray;*/ // Optional: Gray out the tab button
                    tabBtn.ToolTip = "Tab Suspended. Click to reload.";
                }
            }
        });
    }
    private async void ResumeTab(CustomTabItem tab)
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            // Recreate the browser instance
            var browser = await InitBrowser(isNewPage: true, id: tab.Tag);

            // Subscribe to event handlers
            browser.TitleChanged += Browser_TitleChanged;
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.AddressChanged += Browser_AddressChanged;
            browser.PreviewKeyDown += Browser_PreviewKeyDown;

            // Load the suspended URL
            browser.LoadUrlAsync(tab.SuspendedUrl);

            // Replace the placeholder with the browser
            tab.Content = browser;

            tab.IsSuspended = false;

            // Update the tab button's content to indicate active state
            var tabBtn = TabBtnsAndAddTabBtn.OfType<Label>().FirstOrDefault(item => (int)item.Tag == tab.Tag);
            if (tabBtn != null)
            {
                string activeContent = tab.IsIncognito ? $"Inco {tab.Title}" : tab.Title;
                tabBtn.Content = activeContent;
                /*tabBtn.Foreground = Brushes.Black;*/ // Optional: Reset tab button color
                tabBtn.ToolTip = "Tab Active.";
            }

            // Add the browser to the list
            _browsers.Add(browser);

            // Update LastUsed timestamp
            tab.LastUsed = DateTime.Now;
        });
    }



    private async Task<PryGuardBrowser> CreateBrowser(bool isNewPage, int id, PryGuardProfile profile, Dictionary<int, string> incognitoCache = null)
    {
        _PryGuardProfile = profile;

        if (!isNewPage)
        {
            _blockManager = new BlockManager();
            _nativeManager = new NativeSourceManager();
            _blockManager.IsActive = _PryGuardProfile.IsAdBlock;
            _requestHandler = new RequestHandler(_blockManager);
            _requestContextSettings = new RequestContextSettings();
            _lifespanHandler = new LifespanHandler();
            _lifespanHandler.PopupRequested += (url, isIncognito) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OpenUrlInNewTab(url, isIncognito);
                });
            };


            // Set up cache path
            if (_PryGuardProfile.IsLoadCacheInMemory && incognitoCache == null)
            {
                _requestContextSettings.CachePath = _PryGuardProfile.CachePath; // Use profile-specific cache
                var cachePath = _PryGuardProfile.CachePath;
                _requestContextSettings.PersistSessionCookies = true;
            }
            else if (incognitoCache != null)
            {
                // Use an in-memory or temporary cache for incognito mode
                _requestContextSettings.CachePath = Path.Combine(Path.GetTempPath(), "PryGuardTempCache_" + _mainIDCounter);
                _requestContextSettings.PersistSessionCookies = false;
            }
            else
            {
                // Set a unique path for non-incognito sessions if cache is not loaded into memory
                _requestContextSettings.CachePath = Path.Combine(Path.GetTempPath(), "PryGuardProfile_" + _PryGuardProfile.Name + "_Cache");
                _requestContextSettings.PersistSessionCookies = true;
            }

            _requestContextSettings = new RequestContextSettings
            {
                CachePath = _PryGuardProfile.IsLoadCacheInMemory ? _PryGuardProfile.CachePath : Path.Combine(Path.GetTempPath(), "PryGuardProfile_" + _PryGuardProfile.Name + "_Cache"),
                PersistSessionCookies = !IsIncognitoMode, // Use IsIncognitoMode from the ViewModel instead
                PersistUserPreferences = !IsIncognitoMode  // Use IsIncognitoMode from the ViewModel instead
            };
            _context = new RequestContext(_requestContextSettings);
            _context.DisableWebRtc();

            _jsWorker = new(_context);

            if (_PryGuardProfile.Proxy.IsCustomProxy)
            {


                if (_PryGuardProfile.Proxy.IsProxyAuth)
                {
                    _proxyInfo = await IpInfoServiceClient.FetchProxyInfoAsync(_PryGuardProfile.Proxy);
                    _requestHandler.SetAuthCredentials(new ProxyAuthCredentials()
                    {
                        Login = _PryGuardProfile.Proxy.ProxyLogin,
                        Password = _PryGuardProfile.Proxy.ProxyPassword
                    });
                }
                else
                {
                    _proxyInfo = await IpInfoServiceClient.FetchProxyInfoAsync(_PryGuardProfile.Proxy);
                }
            }

            isNewPage = true;
            var browser = InitBasicSettingsBrowser(isNewPage, id, profile);
            browser.IsIncognito = incognitoCache != null; // Set based on incognitoCache
            return browser;
        }
        else
        {
            var browser = InitBasicSettingsBrowser(isNewPage, id, profile);
            browser.IsIncognito = incognitoCache != null; // Set based on incognitoCache
            return browser;
        }
    }


    private static readonly string CachedHtmlContent = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>PryGuard Browser</title>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Orbitron:wght@400;700&display=swap');
        body, html {
            margin: 0;
            padding: 0;
            font-family: 'Orbitron', sans-serif;
            background-color: #050505;
            color: #ffffff;
            height: 100vh;
            overflow: hidden;
        }
        .container {
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            height: 100vh;
            position: relative;
            z-index: 1;
        }
        .background {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: linear-gradient(45deg, #ff0000, #800000);
            filter: blur(100px);
            opacity: 0.2;
            z-index: -1;
        }
        .logo {
            font-size: 4rem;
            font-weight: bold;
            color: #ff0000;
            margin-bottom: 2rem;
            text-shadow: 0 0 10px rgba(255, 0, 0, 0.5);
        }
        .search-form {
            display: flex;
            margin-bottom: 2rem;
        }
        .search-input {
            padding: 0.75rem 1.5rem;
            font-size: 1rem;
            border: 2px solid #ff0000;
            border-radius: 25px 0 0 25px;
            outline: none;
            width: 400px;
            background-color: rgba(255, 255, 255, 0.1);
            color: #ffffff;
            transition: all 0.3s ease;
        }
        .search-input:focus {
            background-color: rgba(255, 255, 255, 0.2);
            box-shadow: 0 0 15px rgba(255, 0, 0, 0.5);
        }
        .search-button {
            padding: 0.75rem 1.5rem;
            font-size: 1rem;
            background-color: #ff0000;
            color: #ffffff;
            border: 2px solid #ff0000;
            border-radius: 0 25px 25px 0;
            cursor: pointer;
            transition: all 0.3s ease;
        }
        .search-button:hover {
            background-color: #cc0000;
            box-shadow: 0 0 15px rgba(255, 0, 0, 0.5);
        }
        .clock {
            font-size: 2rem;
            margin-bottom: 2rem;
            text-shadow: 0 0 10px rgba(255, 0, 0, 0.5);
        }
        .quick-links {
            display: flex;
            gap: 1rem;
            margin-top: 2rem;
        }
        .quick-link {
            padding: 0.5rem 1rem;
            background-color: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            text-decoration: none;
            color: #ffffff;
            transition: all 0.3s ease;
        }
        .quick-link:hover {
            background-color: rgba(255, 0, 0, 0.5);
            transform: translateY(-5px);
        }
    </style>
</head>
<body>
    <div class=""background""></div>
    <div class=""container"">
        <h1 class=""logo"">PryGuard</h1>
        <div class=""clock"" id=""clock""></div>
        <form class=""search-form"" action=""https://www.google.com/search"" method=""get"" target=""_self"">
            <input type=""text"" name=""q"" class=""search-input"" placeholder=""Search with PryGuard..."" required>
            <button type=""submit"" class=""search-button"">Search</button>
        </form>
         <div class=""quick-links"">
        <a href=""https://www.github.com"" class=""quick-link"" target=""_blank"">GitHub</a>
        <a href=""https://www.stackoverflow.com"" class=""quick-link"" target=""_blank"">Stack Overflow</a>
        <a href=""https://www.reddit.com"" class=""quick-link"" target=""_blank"">Reddit</a>
        <a href=""https://www.youtube.com"" class=""quick-link"" target=""_blank"">YouTube</a>
        <a href=""https://iphey.com"" class=""quick-link"" target=""_blank"">IP Hey</a>
        <a href=""https://amiunique.org"" class=""quick-link"" target=""_blank"">Am I Unique</a>
    </div>
    </div>
    <script>
        function updateClock() {
            const now = new Date();
            const time = now.toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
            document.getElementById('clock').textContent = time;
        }
        setInterval(updateClock, 1000);
        updateClock();
    </script>
</body>
</html>";

    private static readonly HashSet<int> LocaleOverrideApplied = new HashSet<int>();
    private static readonly object LocaleLock = new object();

    private PryGuardBrowser InitBasicSettingsBrowser(bool isNewPage, object id, PryGuardProfile PryGuardProfile)
    {
        var PryGuardBrowser = new PryGuardBrowser(_context);
        PryGuardBrowser.Tag = id;
        PryGuardBrowser.LifeSpanHandler = _lifespanHandler;

        // Subscribe to IsBrowserInitializedChanged event
        PryGuardBrowser.IsBrowserInitializedChanged += PryGuardBrowser_IsBrowserInitializedChanged;

        // Configure Browser Settings
        PryGuardBrowser.BrowserSettings.ImageLoading = PryGuardProfile.IsLoadImage ? CefState.Enabled : CefState.Disabled;
        PryGuardBrowser.BrowserSettings.RemoteFonts = CefState.Enabled;
        PryGuardBrowser.BrowserSettings.JavascriptCloseWindows = CefState.Disabled;

        // Setup Download Handler
        var downloadHandler = new DownloadHandler();
        PryGuardBrowser.DownloadHandler = downloadHandler;

        // Subscribe to Download Events
        downloadHandler.DownloadStarted += DownloadManager.Instance.AddDownload;
        downloadHandler.DownloadUpdated += DownloadManager.Instance.UpdateDownload;

        // Setup Custom Context Menu Handler
        var menuHandler = new CustomContextMenuHandler();

        // Subscribe to OpenUrlInNewTabRequested event with locale override tracking
        menuHandler.OpenUrlInNewTabRequested += (url) =>
        {
            // Ensure this runs on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                OpenUrlInNewTab(url);
            });
        };

        PryGuardBrowser.MenuHandler = menuHandler;

        // Setup Render and Load Handlers
        if (isNewPage)
        {
            var codeForFakeProfile = _nativeManager.GetCodeForFakeProfile("fakeinject", PryGuardProfile.FakeProfile);
            _renderMessageHandler = new RenderMessageHandler(codeForFakeProfile);
            PryGuardBrowser.RenderProcessMessageHandler = _renderMessageHandler;
            _loadHandler = new LoadHandler("777", codeForFakeProfile, () => { ProfileFail(); });
            PryGuardBrowser.LoadHandler = _loadHandler;
        }
        else
        {
            PryGuardBrowser.RenderProcessMessageHandler = _renderMessageHandler;
            PryGuardBrowser.LoadHandler = _loadHandler;
        }

        // Assign Request Handler
        PryGuardBrowser.RequestHandler = _requestHandler;

        // Configure Javascript Object Repository
        PryGuardBrowser.JavascriptObjectRepository.Settings.JavascriptBindingApiEnabled = false;
        PryGuardBrowser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
        PryGuardBrowser.JavascriptObjectRepository.NameConverter = new MyCamelCaseNameConverter();
        PryGuardBrowser.JavascriptObjectRepository.Register(
            "worker",
            _jsWorker,
            options: new BindingOptions()
            {
                Binder = new DefaultBinder(new MyCamelCaseNameConverter())
            });

        // Execute Geolocation Script
        ExecGeoScript(PryGuardBrowser);

        // Apply HTML Content
        // Load cached HTML content to optimize memory usage
        PryGuardBrowser.LoadHtml(CachedHtmlContent);

        return PryGuardBrowser;
    }




    private async Task ParseAndInsertCookies(string cookiesInput, PryGuardProfile pryGuardProfile, ChromiumWebBrowser browser)
    {
        if (CurrentTabItem is CustomTabItem currentTab && currentTab.IsIncognito) return;
        if (string.IsNullOrWhiteSpace(cookiesInput))
            return;

        // Get the CookieManager from the global context
        var cookieManager = browser.GetCookieManager();

        // Each cookie should be on a new line
        string[] cookiesLines = cookiesInput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        foreach (var cookieLine in cookiesLines)
        {
            if (string.IsNullOrWhiteSpace(cookieLine))
                continue;

            // Parse each cookie format "CookieName=CookieValue; Domain=http://localhost:8000"
            var parts = cookieLine.Split(';');
            if (parts.Length != 2)
            {
                // Invalid format, skip
                continue;
            }

            var cookieParts = parts[0].Split('=');
            if (cookieParts.Length != 2)
            {
                // Invalid format, skip
                continue;
            }

            string cookieName = cookieParts[0].Trim();
            string cookieValue = cookieParts[1].Trim();
            string domainInfo = parts[1].Replace("Domain=", "").Trim();

            // Extract protocol and domain from the domainInfo
            Uri uri;

            try
            {
                // Create a Uri to extract components
                uri = new Uri(domainInfo);
            }
            catch (UriFormatException)
            {
                // Invalid format, skip
                continue;
            }

            string protocol = uri.Scheme;
            string domain = uri.Host;
            int port = uri.Port;


            if (port == -1)
            {
                port = protocol == "https" ? 443 : 80;
            }


            string url = $"{protocol}://{domain}:{port}";


            var cookie = new Cookie
            {
                Name = cookieName,
                Value = cookieValue,
                Domain = domain,
                Path = "/",
                Expires = DateTime.UtcNow.AddYears(1),
                Secure = protocol == "https",
                HttpOnly = false,
            };


            bool success = await cookieManager.SetCookieAsync(url, cookie);


            await cookieManager.FlushStoreAsync();

            if (!success)
            {
                Console.WriteLine($"Failed to set cookie for domain: {domain}");
            }
        }
    }





    private void ExecGeoScript(PryGuardBrowser PryGuardBrowser)
    {
        double latitude;
        double longitude;
        if (_PryGuardProfile.FakeProfile.GeoSettings.Latitude == null
            || _PryGuardProfile.FakeProfile.GeoSettings.Longitude == null)
            return;

        if (_PryGuardProfile.FakeProfile.GeoSettings.Status == ControlMode.Automatic)
        {
            if (!_PryGuardProfile.Proxy.IsProxyAuth)
                return;
            latitude = double.Parse(_proxyInfo.Location.Split(',')[0], CultureInfo.InvariantCulture);
            longitude = double.Parse(_proxyInfo.Location.Split(',')[1], CultureInfo.InvariantCulture);
        }
        else
        {
            latitude = double.Parse(_PryGuardProfile.FakeProfile.GeoSettings.Latitude, CultureInfo.InvariantCulture);
            longitude = double.Parse(_PryGuardProfile.FakeProfile.GeoSettings.Longitude, CultureInfo.InvariantCulture);
        }

        string script = $@"
            navigator.permissions.query = options => {{
              return Promise.resolve({{
                state: 'granted'
              }});
            }};
            navigator.geolocation.getCurrentPosition = (success, error, options) => {{
              success({{
                coords: {{
                  latitude: {latitude.ToString("0.000000", CultureInfo.InvariantCulture)},
                  longitude: {longitude.ToString("0.000000", CultureInfo.InvariantCulture)},
                  accuracy: 10,
                  altitude: null,
                  altitudeAccuracy: null,
                  heading: null,
                  speed: null
                }},
                timestamp: Date.now()
              }});
            }};
            navigator.geolocation.watchPosition = (success, error, options) => {{
              success({{
                coords: {{
                  latitude: {latitude.ToString("0.000000", CultureInfo.InvariantCulture)},
                  longitude: {longitude.ToString("0.000000", CultureInfo.InvariantCulture)},
                  accuracy: 49,
                  altitude: null,
                  altitudeAccuracy: null,
                  heading: null,
                  speed: null
                }},
                timestamp: Date.now()
              }});
              return 0;
            }};
            ";

        PryGuardBrowser.ExecuteScriptAsyncWhenPageLoaded(script, oneTime: false);
    }
    private void ProfileFail()
    {
        // Cleanup or reset actions, like nullifying proxy-specific configurations
        _proxyInfo = null;

    }

    private async void PryGuardBrowser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue)
        {
            return;
        }

        var browser = (PryGuardBrowser)sender;
        int browserId = (int)browser.Tag;

        // Check if locale override has already been applied
        lock (LocaleLock)
        {
            if (LocaleOverrideApplied.Contains(browserId))
            {
                // Locale override already applied, skip to prevent exceptions
                return;
            }
        }

        try
        {
            using (var client = browser.GetDevToolsClient())
            {
                var canEmu = await client.Emulation.CanEmulateAsync();
                if (canEmu.Result)
                {
                    // Apply User Agent Override
                    await client.Emulation.SetUserAgentOverrideAsync(_PryGuardProfile.FakeProfile.UserAgent);

                    // Apply Locale Override
                    await client.Emulation.SetLocaleOverrideAsync(_PryGuardProfile.FakeProfile.ChromeLanguageInfo.Locale);

                    // Apply Timezone Override if Proxy Authentication is enabled
                    if (_PryGuardProfile.Proxy.IsProxyAuth)
                    {
                        if (_proxyInfo == null)
                        {
                            MessageBox.Show("PROXY DONT WORK!");
                            return;
                        }

                        await client.Emulation.SetTimezoneOverrideAsync(_proxyInfo.TimeZone);
                    }
                    if (_PryGuardProfile.Proxy.IsCustomProxy)
                    {
                        if (_proxyInfo != null)
                        {
                            var chromeProxy = _PryGuardProfile.Proxy.ToChromeProxy();
                            _context.SetProxy(chromeProxy);
                        }

                    }

                    // Make the browser focusable and subscribe to focus events
                    browser.Focusable = true;
                    browser.GotFocus -= Browser_GotFocus;
                    browser.GotFocus += Browser_GotFocus;
                    browser.LostFocus -= Browser_LostFocus;
                    browser.LostFocus += Browser_LostFocus;

                    // Mark locale override as applied
                    lock (LocaleLock)
                    {
                        LocaleOverrideApplied.Add(browserId);
                    }

                    Debug.WriteLine($"Locale override applied for browser ID: {browserId}");
                }
            }
        }
        catch (CefSharp.DevTools.DevToolsClientException ex)
        {
            Debug.WriteLine($"DevTools Client Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }
    private void Browser_GotFocus(object sender, RoutedEventArgs e)
    {
        var browser = sender as PryGuardBrowser;
        var tab = Tabs.FirstOrDefault(t => t.Content == browser);
        if (tab != null)
        {
            tab.LastUsed = DateTime.Now;
        }
    }

    private void Browser_LostFocus(object sender, RoutedEventArgs e)
    {
        var browser = sender as PryGuardBrowser;
        var tab = Tabs.FirstOrDefault(t => t.Content == browser);
        if (tab != null)
        {
            tab.LastUsed = DateTime.Now;
        }
    }
    private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var browser = sender as PryGuardBrowser;
        var tabItem = Tabs.FirstOrDefault(tab => tab.Tag == (int)browser.Tag);
        if (tabItem != null)
        {
            string newTitle = e.NewValue.ToString();
            tabItem.Title = newTitle;

            // Update the tab button's content to reflect the new title
            var button = TabBtnsAndAddTabBtn.OfType<Label>().FirstOrDefault(lbl => (int)lbl.Tag == (int)browser.Tag);
            if (button != null)
            {
                button.Content = tabItem.IsIncognito ? $"Inco {newTitle}" : newTitle;
            }

            // **Capture the address on the UI thread**
            string address = browser.Address;

            // **Run the SaveHistoryJsonAsync without accessing UI elements**
            Task.Run(() => SaveHistoryJsonAsync(address, newTitle));
        }
    }



    private void Browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var browser = sender as PryGuardBrowser;
        var tabItem = Tabs.FirstOrDefault(tab => tab.Tag == (int)browser.Tag);

        if (tabItem != null)
        {
            // Check if the new value is a data: URL
            string newAddress = e.NewValue.ToString();
            if (newAddress.StartsWith("data:"))
            {
                // Set to a friendly display name or custom URL
                newAddress = "about:blank"; // or "about:blank" or whatever you prefer
            }

            tabItem.Address = newAddress;

            // Update the Address property of the ViewModel
            Address = newAddress;
        }
    }


    private void AddBookmark()
    {
        var customTabItem = Tabs.OfType<CustomTabItem>().FirstOrDefault(tab => tab == CurrentTabItem);
        if (customTabItem != null)
        {
            var title = customTabItem.Title; // Get the current tab title
            var url = customTabItem.Address; // Get the current tab URL
            _bookmarkManager.AddBookmark(title, url);
        }
        else
        {
            // Handle the case where CurrentTabItem is not of type CustomTabItem
            MessageBox.Show("Current tab is not of type CustomTabItem.");
        }
    }
    private void OpenBookmark(Bookmark bookmark)
    {

        AddTab();
        (CurrentTabItem.Content as PryGuardBrowser).LoadUrlAsync(bookmark.URL);

    }
    private void RemoveBookmark(Bookmark bookmark)
    {
        _bookmarkManager.RemoveBookmark(bookmark);
    }

    private void Browser_LoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
    {
    }
    #endregion

    #region HistoryWork

    public void ToggleIncognitoMode()
    {
        if (IsIncognitoMode)
        {
            CloseCurrentTabIfIncognito();
            IsIncognitoMode = false;
        }
        else
        {
            IsIncognitoMode = true;
            AddIncognitoTab();
        }
        OnPropertyChanged(nameof(IncognitoModeText));
    }


    private void DeleteHistory()
    {
        var result = MessageBox.Show("Are you sure you want to delete all browsing history?",
                                     "Confirm Deletion",
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            PryGuardHistoryList.Clear();

            var doc = JsonSerializer.Serialize(PryGuardHistoryList);

            using (StreamWriter writer = new(_profileHistoryPath))
            {
                writer.Write(doc);
                writer.Close();
            }
            Application.Current.Dispatcher.Invoke(delegate
            {
                _listView.Items.Clear();
            });
        }
        // If the user clicks 'No', do nothing
    }

    private async Task SaveHistoryJsonAsync(string address, string desc)
    {
        if (CurrentTabItem is CustomTabItem currentTab && currentTab.IsIncognito)
            return; // Do not save history for incognito

        string profileHistoryPath = Path.Combine(_PryGuardProfile.CachePath, "History.json");

        if (PryGuardHistoryList == null)
        {
            PryGuardHistoryList = new ObservableCollection<PryGuardHistoryItem>();
        }

        try
        {
            var hist = new PryGuardHistoryItem(DateTime.Now.ToString("yyyy/MM/dd HH:mm"), desc, address.Replace("https://", ""));
            Application.Current.Dispatcher.Invoke(() => PryGuardHistoryList.Insert(0, hist));

            var doc = JsonSerializer.Serialize(PryGuardHistoryList);
            await File.WriteAllTextAsync(profileHistoryPath, doc); // Asynchronous write
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving history: {ex.Message}");
        }
    }



    private void SaveHistoryList()
    {
        try
        {
            var json = JsonSerializer.Serialize(PryGuardHistoryList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_profileHistoryPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void DeleteHistoryItem(PryGuardHistoryItem historyItem)
    {
        if (historyItem == null)
        {
            MessageBox.Show("No history item selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        // Remove the item from the ObservableCollection
        PryGuardHistoryList.Remove(historyItem);

        SaveHistoryList();

    }

    private async Task LoadHistoryJsonAsync()
    {
        if (string.IsNullOrEmpty(_profileHistoryPath) || !File.Exists(_profileHistoryPath))
        {
            return; // Exit if path is not set or file doesn't exist
        }

        if (PryGuardHistoryList == null)
        {
            PryGuardHistoryList = new ObservableCollection<PryGuardHistoryItem>();
        }

        try
        {
            string json = await File.ReadAllTextAsync(_profileHistoryPath);
            var historyList = JsonSerializer.Deserialize<ObservableCollection<PryGuardHistoryItem>>(json);
            if (historyList != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in historyList)
                    {
                        PryGuardHistoryList.Add(item);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading history: {ex.Message}");
        }
    }




    private void LoadHistoryLink(object link)
    {
        AddTab();
        (CurrentTabItem.Content as PryGuardBrowser).LoadUrlAsync(link.ToString());
    }
    private void OpenContextMenuSettings(object arg)
    {
        if (arg is StackPanel button)
        {
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.IsOpen = true;
        }
    }
    #endregion

    #region Tab Work

    private async void AddTabHistory()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {

            // Create and add the tab
            var newTab = new CustomTabItem
            {
                Tag = _mainIDCounter,
                Content = new HistoryView()
                {
                    DataContext = this
                }
            };
            Tabs.Add(newTab);
            CurrentTabItem = newTab;
            Address = "PryGuard://history/";



            // Create and add the button for the tab
            Label button = new Label
            {
                Content = "History",
                AllowDrop = true,
                Tag = _mainIDCounter
            };
            button.DragEnter += BtnTabDragEnter;
            button.MouseLeftButtonDown += BtnMouseDownForDragAndOpenTab;

            // Add the button in the appropriate position
            if (_mainIDCounter == 0)
            {
                TabBtnsAndAddTabBtn.Insert(0, button);
            }
            else
            {
                TabBtnsAndAddTabBtn.Insert(TabBtnsAndAddTabBtn.Count - 1, button);
            }


            _mainIDCounter++;
        });
    }
    private async void AddTabBookmark()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var newTab = new CustomTabItem
            {
                Tag = _mainIDCounter,
                Content = new BookmarkView()
                {
                    DataContext = this
                }
            };
            Tabs.Add(newTab);
            CurrentTabItem = newTab;

            Address = "PryGuard://bookmarks/";



            Label button = new Label
            {
                Content = "Bookmark",
                AllowDrop = true,
                Tag = _mainIDCounter
            };

            button.DragEnter += BtnTabDragEnter;
            button.MouseLeftButtonDown += BtnMouseDownForDragAndOpenTab;

            if (_mainIDCounter == 0)
            {
                TabBtnsAndAddTabBtn.Insert(0, button);
            }
            else
            {
                TabBtnsAndAddTabBtn.Insert(TabBtnsAndAddTabBtn.Count - 1, button);
            }

            _mainIDCounter++;
        });
    }


    private void AddDownloadTab()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var downloadsPage = new DownloadPage();

            var newTab = new CustomTabItem
            {
                Tag = _mainIDCounter,
                Content = downloadsPage
            };
            Tabs.Add(newTab);
            CurrentTabItem = newTab;

            Address = "PryGuard://downloads/";

            Label button = new Label
            {
                Content = "Downloads",
                AllowDrop = true,
                Tag = _mainIDCounter
            };

            button.DragEnter += BtnTabDragEnter;
            button.MouseLeftButtonDown += BtnMouseDownForDragAndOpenTab;

            if (_mainIDCounter == 0)
            {
                TabBtnsAndAddTabBtn.Insert(0, button);
            }
            else
            {
                TabBtnsAndAddTabBtn.Insert(TabBtnsAndAddTabBtn.Count - 1, button);
            }

            _mainIDCounter++;
        });
    }

    // Generalized method for adding a new tab
    private async Task AddNewTabAsync(bool isIncognito, string url = null)
    {
        try
        {
            // Handle mode reset if adding a regular tab while in incognito mode
            if (!isIncognito && IsIncognitoMode)
            {
                IsIncognitoMode = false;
                OnPropertyChanged(nameof(IsIncognitoMode));
                OnPropertyChanged(nameof(IncognitoModeText));
            }

            // Capture the current ID and increment the counter
            int currentId = _mainIDCounter++;

            // Initialize the appropriate browser instance
            PryGuardBrowser browser = isIncognito
                ? await InitIncognitoBrowser(currentId)
                : await InitBrowser(isNewPage: currentId > 0, currentId);

            // Attach common event handlers
            browser.TitleChanged += Browser_TitleChanged;
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.AddressChanged += Browser_AddressChanged;
            browser.PreviewKeyDown += Browser_PreviewKeyDown;

            // Load the specified URL if provided
            if (!string.IsNullOrEmpty(url))
            {
                browser.Load(url);
            }
            else
            {
                // Optionally, initialize with a default page if no URL is provided
                // browser.Load("about:blank"); // Example default page
            }

            browser.Focus();

            // Create the new tab item with conditional properties
            var newTabItem = new CustomTabItem
            {
                Tag = currentId,
                Content = browser,
                Title = isIncognito ? "Incognito" : browser.Title,
                Address = browser.Address,
                CloseTabCommand = CloseTabCommand,
                IsIncognito = isIncognito
            };


            // Bind Address updates (do not save history for incognito)
            browser.AddressChanged += (s, e) => newTabItem.Address = browser.Address;

            // If not incognito, bind Title updates
            if (!isIncognito)
            {
                browser.TitleChanged += (s, e) => newTabItem.Title = browser.Title;
            }

            // Add the new tab to the collection and set it as the current tab
            Tabs.Add(newTabItem);
            CurrentTabItem = newTabItem;
            CurrentTabItem.LastUsed = DateTime.Now;
            // Create and configure the tab button
            var button = new Label
            {
                Content = newTabItem.Title,
                AllowDrop = true,
                Tag = currentId,
                DataContext = newTabItem
            };

            button.SetBinding(ContentControl.ContentProperty, new Binding("Title"));
            button.DragEnter += BtnTabDragEnter;
            button.MouseLeftButtonDown += BtnMouseDownForDragAndOpenTab;

            // Insert the tab button before the "Add Tab" button
            TabBtnsAndAddTabBtn.Insert(TabBtnsAndAddTabBtn.Count - 1, button);
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error, show a message to the user)
            Debug.WriteLine($"Error adding new tab: {ex.Message}");
            // Optionally, display a user-friendly message
            // MessageBox.Show("Failed to open a new tab.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Refactored AddIncognitoTab method
    private async void AddIncognitoTab()
    {
        await AddNewTabAsync(isIncognito: true);
    }

    // Refactored AddTab method
    private async void AddTab()
    {
        await AddNewTabAsync(isIncognito: false);
    }

    // Refactored OpenUrlInNewTab method
    private async void OpenUrlInNewTab(string url, bool isIncognito = false)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            // Optionally, handle invalid URLs
            // MessageBox.Show("The provided URL is invalid.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await AddNewTabAsync(isIncognito: isIncognito, url: url);
    }


    private void CloseCurrentTabIfIncognito()
    {
        if (CurrentTabItem is CustomTabItem incognitoTab && incognitoTab.IsIncognito)
        {
            Tabs.Remove(incognitoTab);
            _incognitoCache.Clear();

            var buttonToRemove = TabBtnsAndAddTabBtn
                .OfType<FrameworkElement>()
                .FirstOrDefault(btn => (int)btn.Tag == incognitoTab.Tag);

            if (buttonToRemove != null)
            {
                TabBtnsAndAddTabBtn.Remove(buttonToRemove);
            }

            // Clear temporary incognito data
            var incognitoPath = Path.Combine(Path.GetTempPath(), "PryGuardTempCache_" + incognitoTab.Tag);
            if (Directory.Exists(incognitoPath))
            {
                Directory.Delete(incognitoPath, true);
            }
        }
    }










    private void OpenDevTools(object obj)
    {
        // Assuming CurrentTabItem is your active browser tab
        var browser = CurrentTabItem.Content as PryGuardBrowser; // Get the browser instance directly

        if (browser != null && browser.IsBrowserInitialized) // Check if the browser is initialized
        {
            browser.ShowDevTools(); // Open the Developer Tools
        }
    }

    private void BtnMouseDownForDragAndOpenTab(object sender, MouseButtonEventArgs e)
    {
        _tabBtnToDrag = sender as Label;
        DragDrop.DoDragDrop(_tabBtnToDrag, _tabBtnToDrag, DragDropEffects.Move);
        e.Handled = true;
        OpenTab(_tabBtnToDrag.Tag);
    }

    private void BtnTabDragEnter(object sender, DragEventArgs e)
    {
        #region Animation to fututur updates
        //System.Windows.Media.Animation.Storyboard fadeInStoryboard = new();
        //System.Windows.Media.Animation.DoubleAnimation fadeInAnimation = new()
        //{
        //    From = 0,
        //    To = 1,
        //    Duration = TimeSpan.FromSeconds(0.5)
        //};
        //System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
        //fadeInStoryboard.Children.Add(fadeInAnimation);
        //fadeInStoryboard.Begin(_tabBtnToDrag);
        #endregion

        if (e.Source.ToString().Contains("Border") || e.Source.ToString().Contains("TextBlock")) { return; }
        Label btn = (Label)e.Source;
        int where_to_drop = TabBtnsAndAddTabBtn.IndexOf(btn);
        TabBtnsAndAddTabBtn.Remove(_tabBtnToDrag);
        TabBtnsAndAddTabBtn.Insert(where_to_drop, _tabBtnToDrag);
    }
    private void CloseTab(object parameter = null)
    {
        int id;

        // Determine the id based on parameter or CurrentTabItem
        if (parameter is MouseButtonEventArgs mouseArgs && mouseArgs.Source is TextBlock tb)
        {
            id = (int)tb.Tag;
        }
        else if (parameter is int parameterId)
        {
            id = parameterId;
        }
        else if (CurrentTabItem != null)
        {
            id = CurrentTabItem.Tag;
        }
        else
        {
            // No tab to close
            return;
        }

        // Find the tab and get its index before removing it
        var itemToRemove = Tabs.FirstOrDefault(item => item.Tag == id);
        if (itemToRemove != null)
        {
            int currentIndex = Tabs.IndexOf(itemToRemove); // Get index before removing

            if (itemToRemove.Content is PryGuardBrowser browser)
            {
                // Unsubscribe event handlers
                browser.TitleChanged -= Browser_TitleChanged;
                browser.LoadingStateChanged -= Browser_LoadingStateChanged;
                browser.AddressChanged -= Browser_AddressChanged;
                browser.PreviewKeyDown -= Browser_PreviewKeyDown;

                // If this browser was the previous one, clear the reference
                if (_previousBrowser == browser)
                {
                    _previousBrowser = null;
                }

                // Dispose of the browser
                browser.Dispose();
            }
            else if (itemToRemove.IsSuspended)
            {
                // No browser instance to dispose
                // Optionally, clear SuspendedUrl
                itemToRemove.SuspendedUrl = null;
            }

            itemToRemove.Content = null;
            Tabs.Remove(itemToRemove);

            // Update CurrentTabItem
            if (Tabs.Count > 0)
            {
                if (currentIndex >= Tabs.Count)
                {
                    currentIndex = Tabs.Count - 1;
                }
                if (currentIndex >= 0)
                {
                    CurrentTabItem = Tabs[currentIndex];
                }
                else
                {
                    CurrentTabItem = Tabs.First();
                }
            }
            else
            {
                CurrentTabItem = null;
            }
        }

        // Remove the tab button
        var tabBtnToRemove = TabBtnsAndAddTabBtn.OfType<Label>().FirstOrDefault(item => (int)item.Tag == id);
        if (tabBtnToRemove != null)
        {
            TabBtnsAndAddTabBtn.Remove(tabBtnToRemove);
        }

        // Remove from _browsers
        _browsers.RemoveAll(b => (int)b.Tag == id);

        // Check if this was the Incognito tab
        var closedTab = itemToRemove as CustomTabItem;
        if (closedTab != null && closedTab.IsIncognito)
        {
            IsIncognitoMode = false;  // Reset IsIncognitoMode
            OnPropertyChanged(nameof(IsIncognitoMode));  // Notify UI of change
        }

        // Optionally force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }





    private void OnCurrentTabItemChanged()
    {
        if (_currentTabItem != null)
        {
            if (_currentTabItem.Content is PryGuardBrowser browser)
            {
                // Unsubscribe from previous browser events
                if (_previousBrowser != null && _previousBrowser != browser)
                {
                    _previousBrowser.AddressChanged -= Browser_AddressChanged;
                }

                // Subscribe to the AddressChanged event of the new browser
                browser.AddressChanged += Browser_AddressChanged;

                // Update the Address property
                Address = browser.Address;

                // Store the current browser
                _previousBrowser = browser;
            }
            else
            {
                // If the content is not a browser, set Address accordingly
                Address = _currentTabItem.Address ?? string.Empty;
            }
        }
        else
        {
            // No CurrentTabItem, clear the Address
            Address = string.Empty;
        }
    }

    private async void OpenTab(object arg)
    {
        var tabToSelect = Tabs.FirstOrDefault(item => item.Tag == (int)arg);
        if (tabToSelect != null)
        {
            CurrentTabItem = tabToSelect;

            if (tabToSelect.IsSuspended)
            {
                ResumeTab(tabToSelect);
            }
            else
            {
                if (CurrentTabItem.Content is PryGuardBrowser browser)
                {
                    Address = browser.Address;
                }
                else
                {
                    Address = "PryGuard://default/";
                }
            }
        }
    }


    #endregion

    #region find work
    private void ShowFindBar()
    {
        IsFindBarVisible = true;
        //FocusSearchTextBoxAction?.Invoke();

    }

    private void CloseFindBar()
    {
        IsFindBarVisible = false;
        // Stop finding
        var browser = CurrentTabItem?.Content as PryGuardBrowser;
        browser?.GetBrowserHost()?.StopFinding(true);
        SearchText = string.Empty;
    }

    private void FindNext()
    {
        var browser = CurrentTabItem?.Content as PryGuardBrowser;
        var browserHost = browser?.GetBrowserHost();

        if (!string.IsNullOrEmpty(SearchText) && browserHost != null)
        {
            // Call Find with the "resume" parameter set to true to move to the next match
            browserHost.Find(SearchText, forward: true, findNext: true, matchCase: false);
        }
    }

    private void FindPrevious()
    {
        var browser = CurrentTabItem?.Content as PryGuardBrowser;
        var browserHost = browser?.GetBrowserHost();

        if (!string.IsNullOrEmpty(SearchText) && browserHost != null)
        {
            // Call Find with the "resume" parameter set to true to move to the previous match
            browserHost.Find(SearchText, forward: false, findNext: true, matchCase: false);
        }
    }


    private void PerformSearch(bool forward = true)
    {
        var browser = CurrentTabItem?.Content as PryGuardBrowser;
        var browserHost = browser?.GetBrowserHost();

        if (string.IsNullOrEmpty(SearchText))
        {
            // Stop finding when the search text is empty to clear previous results
            browserHost?.StopFinding(true);
            return;
        }

        if (browserHost != null)
        {
            browserHost.Find(SearchText, forward, false, false);
        }
    }


    #endregion

    #region Window Work
    private void Browser_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var browser = sender as IWebBrowser;

        if (browser == null)
            return;

        // Check for Ctrl+P (Print)
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.P)
        {
            browser.GetBrowserHost().Print();
            e.Handled = true;
        }
        // Check for F12 (Developer Tools)
        else if (e.Key == Key.F12)
        {
            browser.ShowDevTools();
            e.Handled = true;
        }
        // Check for Ctrl+Shift+I (Developer Tools)
        else if (e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.I)
        {
            browser.ShowDevTools();
            e.Handled = true;
        }
        // Check for Ctrl+R (Reload)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.R)
        {
            browser.Reload();
            e.Handled = true;
        }
        // Check for Ctrl+T (New Tab)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.T)
        {
            AddTab();
            e.Handled = true;
        }

        // Check for Ctrl+W (Close Tab)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
        {
            CloseTab(null);
            e.Handled = true;
        }

        // Ctrl + '+' (Zoom In)
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && (e.Key == Key.OemPlus || e.Key == Key.Add))
        {
            browser.GetZoomLevelAsync().ContinueWith(task =>
            {
                var currentZoomLevel = task.Result;
                var newZoomLevel = currentZoomLevel + 0.1;
                browser.SetZoomLevel(newZoomLevel);
            }, TaskScheduler.FromCurrentSynchronizationContext());
            e.Handled = true;
        }
        // Ctrl + '-' (Zoom Out)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && (e.Key == Key.OemMinus || e.Key == Key.Subtract))
        {
            browser.GetZoomLevelAsync().ContinueWith(task =>
            {
                var currentZoomLevel = task.Result;
                var newZoomLevel = currentZoomLevel - 0.1;
                browser.SetZoomLevel(newZoomLevel);
            }, TaskScheduler.FromCurrentSynchronizationContext());
            e.Handled = true;
        }
        // Ctrl + '0' (Reset Zoom)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.D0)
        {
            browser.SetZoomLevel(0);
            e.Handled = true;
        }
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            SavePage(browser);
            e.Handled = true;
        }
        // Check for Ctrl+Z (Undo)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
        {
            browser.GetFocusedFrame()?.Undo();
            e.Handled = true;
        }

    }







    private void SavePage(IWebBrowser browser)
    {
        // Get the main (focused) frame
        var frame = browser.GetFocusedFrame();

        if (frame == null)
            return;

        // Get the HTML source asynchronously
        frame.GetSourceAsync().ContinueWith(taskHtml =>
        {
            var html = taskHtml.Result;

            // Open a "Save As" dialog on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "HTML Files (*.html)|*.html|All Files (*.*)|*.*",
                    FileName = "Untitled.html"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Save the HTML content to the selected file
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, html);
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (e.g., display an error message)
                        MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });
        });
    }


    private void AddressOnKeyDown(object arg)
    {
        if ((arg as KeyEventArgs).Key == Key.Enter)
        {
            var input = Address.Trim();

            // Regex to check if input looks like a domain (with optional subdomains)
            string domainPattern = @"^([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$";

            if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
            {
                // If the input is a well-formed URL, load it directly
                (CurrentTabItem.Content as PryGuardBrowser).LoadUrlAsync(input);
            }
            else if (Regex.IsMatch(input, domainPattern))
            {
                // If it matches the domain pattern, prepend "https://" and treat it as a URL
                var url = $"https://{input}";
                (CurrentTabItem.Content as PryGuardBrowser).LoadUrlAsync(url);
            }
            else
            {
                // Otherwise, perform a search using a search engine
                var searchQuery = $"https://www.google.com/search?q={Uri.EscapeDataString(input)}";
                (CurrentTabItem.Content as PryGuardBrowser).LoadUrlAsync(searchQuery);
            }

            // Shift focus away from the search bar after executing the search
            Keyboard.ClearFocus();  // Clear keyboard focus from the search bar
            (CurrentTabItem.Content as PryGuardBrowser)?.Focus();  // Set focus to the browser or any other element
        }
    }


    private void Back(object arg)
    {
        if (CurrentTabItem == null) return;

        if (CurrentTabItem.Content is PryGuardBrowser browser)
        {
            browser.Back();
        }
        else
        {
            // Optionally, handle the case for history/bookmark or do nothing.
            // MessageBox.Show("Back operation not supported on this tab.");
        }
    }
    private void OpenEmail()
    {
        OpenUrlInNewTab("pryguard.org");
    }
    private void Forward(object arg)
    {
        if (CurrentTabItem == null) return;

        if (CurrentTabItem.Content is PryGuardBrowser browser)
        {
            browser.Forward();
        }
        else
        {
            // Optionally, handle the case for history/bookmark or do nothing.
            // MessageBox.Show("Forward operation not supported on this tab.");
        }
    }

    private void Refresh(object arg)
    {
        if (CurrentTabItem == null) return;

        if (CurrentTabItem.Content is PryGuardBrowser browser)
        {
            browser.Reload();
        }
        else if (CurrentTabItem.Content is ListView listView)
        {
            // Assuming _listView is your ListView instance
            CurrentTabItem.Content = _listView;
        }
        else
        {
            // Optionally, handle the case for history/bookmark or do nothing.
            // MessageBox.Show("Refresh operation not supported on this tab.");
        }
    }

    private void NormalStateWindow(object arg)
    {
        //if ((arg as MouseEventArgs).LeftButton == MouseButtonState.Pressed)
        //{
        //    CurWindowState = WindowState.Normal;
        //}
    }
    private void MinimizedWindow(object arg)
    {
        CurWindowState = WindowState.Minimized;
    }
    private void MaximizedWindow(object arg)
    {
        // Toggle between maximized and normal window state
        if (CurWindowState == WindowState.Maximized)
        {
            CurWindowState = WindowState.Normal; // Set to normal if it's currently maximized
        }
        else
        {
            CurWindowState = WindowState.Maximized; // Maximize if it's not already maximized
        }
    }

    private async void CloseWindow(object obj)
    {
        //if (CurrentTabItem != null)
        //{
        //    var currentBrowser = CurrentTabItem.Content as ChromiumWebBrowser;
        //    if (currentBrowser != null)
        //    {
        //        await CaptureScreenshotAsync(currentBrowser);
        //    }
        //}
        ViewManager.Close(this);
    }
    public bool CanClose()
    {
        return true;
    }
    #endregion
}