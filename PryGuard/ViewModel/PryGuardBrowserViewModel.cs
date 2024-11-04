﻿using System;
using CefSharp;
using System.IO;
using System.Linq;
using PryGuard.Model;
using System.Windows;
using PryGuard.Core.Web;
using System.Text.Json;
using System.Globalization;
using System.Windows.Input;
using PryGuard.Core.ChromeApi;
using CefSharp.ModelBinding;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Controls;
using PryGuard.Services.Commands;
using PryGuard.Services.UI.Button;
using System.Collections.Generic;
using PryGuard.Core.ChromeApi.Proxy;
using System.Collections.ObjectModel;
using PryGuard.Core.ChromeApi.Handlers;
using PryGuard.Services.UI.ListView.ListViewItem;
using PryGuard.View;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PryGuard.View;
using CefSharp.Wpf;
using PryGuard.View;


namespace PryGuard.ViewModel;

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
    private IpInfoResult _proxyInfo;
    private BlockManager _blockManager;
    private NativeSourceManager _nativeManager;
    private RequestHandler _requestHandler;
    private RequestContextSettings _requestContextSettings;
    private RequestContext _context;
    private LifespanHandler _lifespanHandler;
    private RenderMessageHandler _renderMessageHandler;
    private LoadHandler _loadHandler;
    private JsWorker _jsWorker;
    #endregion
    #endregion

    #region Commands
    public RelayCommand MinimizeWindowCommand { get; private set; }
    public RelayCommand MaximizeWindowCommand { get; private set; }
    public RelayCommand NormalStateWindowCommand { get; private set; }
    public RelayCommand AddTabCommand { get; private set; }
    public RelayCommand OpenTabCommand { get; private set; }
    public RelayCommand CloseTabCommand { get; private set; }
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

    public DelegateCommand CloseCommand =>
          _closeCommand ?? (_closeCommand = new DelegateCommand(obj => CloseWindow(obj)));
    public ICommand AddBookmarkCommand { get; }
    public ICommand RemoveBookmarkCommand { get; }
    public ICommand OpenBookmarkCommand { get; }
    public ICommand OpenDevToolsCommand { get; }

    #endregion

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
    #region Properties
    private CustomTabItem _currentTabItem;
    public CustomTabItem CurrentTabItem
    {
        get => _currentTabItem;
        set => Set(ref _currentTabItem, value);
    }
    private BookmarkManager _bookmarkManager;

    public ObservableCollection<Bookmark> Bookmarks => _bookmarkManager?.Bookmarks;


    private ObservableCollection<PryGuardHistoryItem> _pryGuardHistoryList;
   
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
    public PryGuardBrowserViewModel( PryGuardProfile PryGuardProfileToStart)
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
        OpenTabCommand = new RelayCommand(OpenTab);
        CloseTabCommand = new RelayCommand(CloseTab);
        AddBookmarkTabCommand = new RelayCommand(AddTabBookmark);
        AddDownloadTabCommand = new RelayCommand(AddDownloadTab);
        RefreshCommand = new RelayCommand(Refresh);
        ForwardCommand = new RelayCommand(Forward);
        BackCommand = new RelayCommand(Back);
        AddressOnKeyDownCommand = new RelayCommand(AddressOnKeyDown);
        LoadHistoryLinkCommand = new RelayCommand(LoadHistoryLink);
        OpenHistoryCommand = new RelayCommand(AddTabHistory);
        OpenContextMenuSettingsCommand = new RelayCommand(OpenContextMenuSettings);
        OpenDevToolsCommand = new RelayCommand(OpenDevTools);
        CurWindowState = WindowState.Maximized;



        try
        {
            ChromiumInit.Init(PryGuardProfileToStart);
        }
        catch (Exception ex)
        {
            File.WriteAllText("fail.txt", ex.Message);
            throw;
        }

        LoadHistoryJson();
        TabBtnsAndAddTabBtn = new() { InitAddTabBtn.CreateBtn(AddTab) };
        AddTab();
    }
    #endregion

    #region PryGuardBrowser Work
    private async Task<PryGuardBrowser> InitBrowser(bool isNewPage)
    {
        PryGuardBrowser browser = await CreateBrowser(isNewPage, _mainIDCounter, _PryGuardProfileToStart);
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

    private async Task<PryGuardBrowser> CreateBrowser(bool isNewPage, object id, PryGuardProfile PryGuardProfile)
    {
        if (!isNewPage)
        {
            _PryGuardProfile = PryGuardProfile;
            _blockManager = new BlockManager();
            _nativeManager = new NativeSourceManager();
            _blockManager.IsWork = _PryGuardProfile.IsAdBlock;
            _requestHandler = new RequestHandler(_blockManager);
            _requestContextSettings = new RequestContextSettings();
            _lifespanHandler = new LifespanHandler();
            _lifespanHandler.PopupRequested += (url) =>
            {
                // Ensure this is executed on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OpenUrlInNewTab(url);
                });
            };
            if (_PryGuardProfile.IsLoadCacheInMemory)
            {
                _requestContextSettings.CachePath = _PryGuardProfile.CachePath;
                _requestContextSettings.PersistSessionCookies = true;
            }
            else
            {
                DateTime now = DateTime.Now;
                Random random = new((int)now.TimeOfDay.TotalMilliseconds);
                string tempPath = Path.GetTempPath();
                string path2 = "PryGuard" + random.Next();
                string normalStringUpper = _PryGuardProfile.Name;
                now = DateTime.Now;
                string str1 = new Random((int)now.TimeOfDay.TotalMilliseconds + random.Next()).Next().ToString();
                string path3 = normalStringUpper + str1;
                string str2 = Path.Combine(tempPath, path2, path3, "cache");
                _requestContextSettings.CachePath = str2;
                _PryGuardProfile.CachePath = Path.Combine(tempPath, path2);
                _requestContextSettings.PersistSessionCookies = false;
                _requestContextSettings.PersistUserPreferences = false;
            }

            _context = new RequestContext(_requestContextSettings);
            _context.DisableWebRtc();

            _jsWorker = new(_context);

            if (_PryGuardProfile.Proxy.IsCustomProxy)
            {
                var chromeProxy = _PryGuardProfile.Proxy.ToChromeProxy();
                _context.SetProxy(chromeProxy);

                if (_PryGuardProfile.Proxy.IsProxyAuth)
                {
                    // Check the client proxy and set authentication credentials
                    _proxyInfo = await IpInfoClient.CheckClientProxy(_PryGuardProfile.Proxy);
                    _requestHandler.SetAuthCredentials(new ProxyAuthCredentials()
                    {
                        Login = _PryGuardProfile.Proxy.ProxyLogin,
                        Password = _PryGuardProfile.Proxy.ProxyPassword
                    });
                }
                else
                {
                    // If the proxy does not require authentication, just check the client proxy
                    _proxyInfo = await IpInfoClient.CheckClientProxy(_PryGuardProfile.Proxy);
                }
            }

            isNewPage = true;
            return InitBasicSettingsBrowser(isNewPage, id, PryGuardProfile);
        }
        else { return InitBasicSettingsBrowser(isNewPage, id, PryGuardProfile); }
    }
    private PryGuardBrowser InitBasicSettingsBrowser(bool isNewPage, object id, PryGuardProfile PryGuardProfile)
    {
        var PryGuardBrowser = new PryGuardBrowser(_context);
        PryGuardBrowser.LifeSpanHandler = _lifespanHandler;
        PryGuardBrowser.IsBrowserInitializedChanged += PryGuardBrowser_IsBrowserInitializedChanged;
        PryGuardBrowser.BrowserSettings.ImageLoading = PryGuardProfile.IsLoadImage ? CefState.Enabled : CefState.Disabled;
        PryGuardBrowser.BrowserSettings.RemoteFonts = CefState.Enabled;
        PryGuardBrowser.BrowserSettings.JavascriptCloseWindows = CefState.Disabled;

        var downloadHandler = new DownloadHandler();
        PryGuardBrowser.DownloadHandler = downloadHandler;

        // Ensure DownloadManager.Instance is accessible
        downloadHandler.DownloadStarted += DownloadManager.Instance.AddDownload;
        downloadHandler.DownloadUpdated += DownloadManager.Instance.UpdateDownload;
        // Create an instance of your custom context menu handler
        var menuHandler = new CustomContextMenuHandler();

        // Subscribe to the OpenUrlInNewTabRequested event
        menuHandler.OpenUrlInNewTabRequested += (url) =>
        {
            // Ensure this runs on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                OpenUrlInNewTab(url);
            });
        };

        PryGuardBrowser.MenuHandler = menuHandler;




        if (isNewPage)
        {
            var codeForFakeProfile = _nativeManager.GetCodeForFakeProfile("" +
                "fakeinject", PryGuardProfile.FakeProfile);
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

        PryGuardBrowser.RequestHandler = _requestHandler;
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

        ExecGeoScript(PryGuardBrowser);

        PryGuardBrowser.Tag = id;

        string htmlContent = @"
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
            <a href=""https://www.github.com"" class=""quick-link"" target=""_self"">GitHub</a>
            <a href=""https://www.stackoverflow.com"" class=""quick-link"" target=""_self"">Stack Overflow</a>
            <a href=""https://www.reddit.com"" class=""quick-link"" target=""_self"">Reddit</a>
            <a href=""https://www.youtube.com"" class=""quick-link"" target=""_blank"">YouTube</a>
            <a href=""https://www.iphey.com"" class=""quick-link"" target=""_self"">IP Hey</a>
            <a href=""https://amiunique.org"" class=""quick-link"" target=""_self"">Am I Unique</a>
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

        // Load HTML content directly into the browser
        PryGuardBrowser.LoadHtml(htmlContent);

        return PryGuardBrowser;
    }

    private async Task ParseAndInsertCookies(string cookiesInput, PryGuardProfile pryGuardProfile, ChromiumWebBrowser browser)
    {
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
                Secure = (protocol == "https"), 
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

    


    public async Task CaptureScreenshotAsync(ChromiumWebBrowser browser)
    {
        if (browser.IsBrowserInitialized)
        {
            var bitmap = new Bitmap((int)browser.ActualWidth, (int)browser.ActualHeight);
            var browserHost = browser.GetBrowser().GetHost();

            // Render the browser into a bitmap
            var viewRect = new CefSharp.Structs.Rect(0, 0, (int)browser.ActualWidth, (int)browser.ActualHeight);

            // Take screenshot (you need to implement CaptureBrowserAsBitmap manually or capture from the view if possible)
            var screenshotBytes = CaptureBrowserAsBitmap(browser);

            // Save screenshot to file
            var screenshotPath = Path.Combine(_PryGuardProfileToStart.CachePath, "last_visited_site_screenshot.png");
            using (var fileStream = new FileStream(screenshotPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.WriteAsync(screenshotBytes, 0, screenshotBytes.Length);
            }

            // Alert message instead of console output
            MessageBox.Show("Screenshot captured and saved successfully at " + screenshotPath, "Screenshot Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            // Alert message instead of console output
            MessageBox.Show("Browser is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private byte[] CaptureBrowserAsBitmap(ChromiumWebBrowser browser)
    {
        var viewWidth = (int)browser.ActualWidth;
        var viewHeight = (int)browser.ActualHeight;

        // Create a new bitmap of the browser size
        var bitmap = new Bitmap(viewWidth, viewHeight);

        // Render the browser into the bitmap
        using (var graphics = Graphics.FromImage(bitmap))
        {
            // Use System.Drawing.Point instead of System.Windows.Point
            graphics.CopyFromScreen(new System.Drawing.Point(0, 0), System.Drawing.Point.Empty, new System.Drawing.Size(viewWidth, viewHeight));
        }

        // Convert the Bitmap to a byte array
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }



    private void ExecGeoScript(PryGuardBrowser PryGuardBrowser)
    {
        double latitude;
        double longitude;
        if (_PryGuardProfile.FakeProfile.GeoSettings.Latitude == null
            || _PryGuardProfile.FakeProfile.GeoSettings.Longitude == null)
            return;

        if (_PryGuardProfile.FakeProfile.GeoSettings.Status == Core.ChromeApi.Model.Configs.AutoManualEnum.AUTO)
        {
            if (!_PryGuardProfile.Proxy.IsProxyAuth)
                return;
            latitude = double.Parse(_proxyInfo.Loc.Split(',')[0], CultureInfo.InvariantCulture);
            longitude = double.Parse(_proxyInfo.Loc.Split(',')[1], CultureInfo.InvariantCulture);
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
    private void ProfileFail() { }
    private async void PryGuardBrowser_IsBrowserInitializedChanged(
        object sender,
        DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue)
        {
            return;
        }

        var browser = (PryGuardBrowser)sender;

        using (var client = browser.GetDevToolsClient())
        {
            var canEmu = await client.Emulation.CanEmulateAsync();
            if (canEmu.Result)
            {

                //await client.Emulation.SetDeviceMetricsOverrideAsync(_PryGuardProfile.FakeProfile.ScreenSize.Width, _PryGuardProfile.FakeProfile.ScreenSize.Height, 1, false);
                await client.Emulation.SetUserAgentOverrideAsync(_PryGuardProfile.FakeProfile.UserAgent);
                await client.Emulation.SetLocaleOverrideAsync(_PryGuardProfile.FakeProfile.ChromeLanguageInfo.Locale);
                if (_PryGuardProfile.Proxy.IsProxyAuth)
                {
                    if (_proxyInfo == null)
                    {
                        MessageBox.Show("PROXY DONT WORK!");
                    }

                    await client.Emulation.SetTimezoneOverrideAsync(_proxyInfo.Timezone);
                }
            }
        }
    }

    private void Browser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var browser = sender as PryGuardBrowser;
        var tabItem = Tabs.FirstOrDefault(tab => (int)tab.Tag == (int)browser.Tag);
        if (tabItem != null)
        {
            tabItem.Title = e.NewValue.ToString();

            // Update the Label's Content to reflect the new title
            var button = TabBtnsAndAddTabBtn.OfType<Label>().FirstOrDefault(lbl => (int)lbl.Tag == (int)browser.Tag);
            if (button != null)
            {
                button.Content = e.NewValue.ToString();
            }

            // Save the history
            SaveHistoryJson(browser.Address, e.NewValue.ToString());
        }
    }

    private void Browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var browser = sender as PryGuardBrowser;
        var tabItem = Tabs.FirstOrDefault(tab => (int)tab.Tag == (int)browser.Tag);

        if (tabItem != null)
        {
            // Check if the new value is a `data:` URL
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

    private void SaveHistoryJson(string address, string desc)
    {
        if (!File.Exists(_profileHistoryPath))
        {
            File.Create(_profileHistoryPath).Close();
        }

        Task.Run(() =>
        {
            var hist = new PryGuardHistoryItem(DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                desc, address.Replace("https://", ""));

            // Write to the JSON file in the background thread
            var doc = JsonSerializer.Serialize(PryGuardHistoryList);
            using StreamWriter writer = new(_profileHistoryPath);
            writer.Write(doc);
            writer.Close();

            // Update the UI on the main thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Insert history item into the ObservableCollection on the UI thread
                PryGuardHistoryList.Insert(0, hist);

                var listBoxItem = new ListViewItem();

                // Set properties for the list box item
                ListViewItemProperties.SetTimeHistory(listBoxItem, hist.Time);
                ListViewItemProperties.SetDescHistory(listBoxItem, hist.Description);
                ListViewItemProperties.SetLinkPreview(listBoxItem, hist.Link[..hist.Link.IndexOf('/')]);
                ListViewItemProperties.SetFullLink(listBoxItem, hist.Link);

                // Insert into the ListView on the UI thread
                _listView.Items.Insert(0, listBoxItem);
            });
        });
    }


    private void LoadHistoryJson()
    {
        if (!File.Exists(_profileHistoryPath)) { return; }

        using StreamReader reader = new(_profileHistoryPath);
        var json = reader.ReadToEnd();
        reader.Close();
        PryGuardHistoryList = JsonSerializer.Deserialize<ObservableCollection<PryGuardHistoryItem>>(JsonNode.Parse(json).ToString());
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
            PryGuardHistoryList.Add(new PryGuardHistoryItem("2024/09/21 10:30", "Test Description", "www.google.com"));
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
            
            // Load history when the tab is created
            LoadHistoryJson(); // Ensure history is loaded

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



    private async void AddTab()
    {
        var browser = await InitBrowser(_mainIDCounter > 0);
        browser.TitleChanged += Browser_TitleChanged;
        browser.LoadingStateChanged += Browser_LoadingStateChanged;
        browser.AddressChanged += Browser_AddressChanged;
        browser.PreviewKeyDown += Browser_PreviewKeyDown;

        browser.Focus();
        var newTabItem = new CustomTabItem()
        {
            Tag = _mainIDCounter,
            Content = browser,
            Title = browser.Title,
            Address = browser.Address,
            CloseTabCommand = CloseTabCommand // Set the command
        };

        // Set bindings to keep Title and Address updated
        browser.TitleChanged += (s, e) => newTabItem.Title = browser.Title;
        browser.AddressChanged += (s, e) => newTabItem.Address = browser.Address;

        Tabs.Add(newTabItem);
        CurrentTabItem = Tabs.Last();

        var button = new Label
        {
            Content = newTabItem.Title,
            AllowDrop = true,
            Tag = _mainIDCounter,
            DataContext = newTabItem
        };

        button.SetBinding(Label.ContentProperty, new Binding("Title")); // Bind button content to Title

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
    }
    private async void OpenUrlInNewTab(string url)
    {
        var browser = await InitBrowser(_mainIDCounter > 0);
        browser.TitleChanged += Browser_TitleChanged;
        browser.LoadingStateChanged += Browser_LoadingStateChanged;
        browser.AddressChanged += Browser_AddressChanged;
        browser.PreviewKeyDown += Browser_PreviewKeyDown;
        // Navigate the browser to the URL

        browser.Load(url);
        browser.Focus();
        var newTabItem = new CustomTabItem()
        {
            Tag = _mainIDCounter,
            Content = browser,
            Title = browser.Title,
            Address = browser.Address,
            CloseTabCommand = CloseTabCommand // Set the command
        };

        // Set bindings to keep Title and Address updated
        browser.TitleChanged += (s, e) => newTabItem.Title = browser.Title;
        browser.AddressChanged += (s, e) => newTabItem.Address = browser.Address;

        Tabs.Add(newTabItem);
        CurrentTabItem = Tabs.Last();

        var button = new Label
        {
            Content = newTabItem.Title,
            AllowDrop = true,
            Tag = _mainIDCounter,
            DataContext = newTabItem
        };

        button.SetBinding(Label.ContentProperty, new Binding("Title")); // Bind button content to Title

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
        _tabBtnToDrag = (sender as Label);
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

    private void CloseTab(object parameter)
    {
        int id;

        // Check if parameter is MouseButtonEventArgs and get id from there
        if (parameter is MouseButtonEventArgs mouseArgs && mouseArgs.Source is TextBlock tb)
        {
            id = (int)tb.Tag;
        }
        else if (CurrentTabItem != null)
        {
            // If parameter is not MouseButtonEventArgs, get id from CurrentTabItem
            id = (int)CurrentTabItem.Tag;
        }
        else
        {
            // No current tab to close
            return;
        }

        // Delete from Tabs
        var itemToRemove = Tabs.FirstOrDefault(item => (int)item.Tag == id);
        if (itemToRemove != null)
        {
            itemToRemove.Content = null;
            Tabs.Remove(itemToRemove);

            if (Tabs.Count > 0)
            {
                int currentIndex = Tabs.IndexOf(itemToRemove);
                CurrentTabItem = currentIndex > 0 ? Tabs[currentIndex - 1] : Tabs.First();
            }
            else
            {
                CurrentTabItem = null;
            }
        }

        // Delete from TabBtns
        var tabBtnToRemove = TabBtnsAndAddTabBtn.OfType<Label>().FirstOrDefault(item => (int)item.Tag == id);
        if (tabBtnToRemove != null)
        {
            TabBtnsAndAddTabBtn.Remove(tabBtnToRemove);
        }

        // Delete from Browsers
        _browsers = _browsers.Where(item => (int)item.Tag != id).ToList();
    }



    private void OpenTab(object arg)
    {
        var tabToSelect = tabs.FirstOrDefault(item => (int)item.Tag == (int)arg);
        if (tabToSelect != null)
        {
            CurrentTabItem = tabToSelect;

            if (CurrentTabItem.Content.ToString().Contains("ListView"))
            {
                Address = "PryGuard://history/";
            }
            else if (CurrentTabItem.Content is PryGuardBrowser browser)
            {
                // Safely cast and access the Address property
                Address = browser.Address;
            }
            else
            {
                // Fallback in case it's not a PryGuardBrowser or a ListView (if needed)
                Address = "PryGuard://default/";
            }
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
        //check for Ctrl+H (History)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.H)
        {
            AddTabHistory();
            e.Handled = true;
        }   
        //check for Ctrl+B (Bookmark)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.B)
        {
            AddTabBookmark();
            e.Handled = true;
        }   
        //check for Ctrl+J (Downloads)
        else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.J)
        {
            AddDownloadTab();
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
        if (CurrentTabItem != null)
        {
            var currentBrowser = CurrentTabItem.Content as ChromiumWebBrowser;
            if (currentBrowser != null)
            {
                await CaptureScreenshotAsync(currentBrowser);
            }
        }
        ViewManager.Close(this);
    }
    public bool CanClose()
    {
        return true;
    }
    #endregion
}