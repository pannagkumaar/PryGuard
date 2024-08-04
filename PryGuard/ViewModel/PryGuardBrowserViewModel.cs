using System;
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
using System.Windows.Data;

namespace PryGuard.ViewModel;
public class PryGuardBrowserViewModel : BaseViewModel
{
    #region Fields
    private int _mainIDCounter;
    private DelegateCommand _closeCommand;
    private Label _tabBtnToDrag;
    private readonly string _profileHistoryPath;
    private ListView _listView;

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
    public RelayCommand LoadHistoryLinkCommand { get; private set; }
    public RelayCommand AddressOnKeyDownCommand { get; private set; }
    public RelayCommand OpenContextMenuSettingsCommand { get; private set; }
    public DelegateCommand CloseCommand =>
          _closeCommand ?? (_closeCommand = new DelegateCommand(obj => CloseWindow(obj)));
    public ICommand AddBookmarkCommand { get; }
    public ICommand RemoveBookmarkCommand { get; }
    public ICommand OpenBookmarkCommand { get; }

    #endregion

    #region Properties
    private CustomTabItem _currentTabItem;
    public CustomTabItem CurrentTabItem
    {
        get => _currentTabItem;
        set => Set(ref _currentTabItem, value);
    }
    private BookmarkManager _bookmarkManager;

    public ObservableCollection<Bookmark> Bookmarks => _bookmarkManager?.Bookmarks;
    private ObservableCollection<PryGuardHistoryItem> _PryGuardHistoryList;
    public ObservableCollection<PryGuardHistoryItem> PryGuardHistoryList
    {
        get => _PryGuardHistoryList;
        set => Set(ref _PryGuardHistoryList, value);
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
        NormalStateWindowCommand = new RelayCommand(NormalStateWindow);
        AddTabCommand = new RelayCommand(AddTab);
        OpenTabCommand = new RelayCommand(OpenTab);
        CloseTabCommand = new RelayCommand(CloseTab);
        
        RefreshCommand = new RelayCommand(Refresh);
        ForwardCommand = new RelayCommand(Forward);
        BackCommand = new RelayCommand(Back);
        AddressOnKeyDownCommand = new RelayCommand(AddressOnKeyDown);
        LoadHistoryLinkCommand = new RelayCommand(LoadHistoryLink);
        OpenHistoryCommand = new RelayCommand(AddTabHistory);
        OpenContextMenuSettingsCommand = new RelayCommand(OpenContextMenuSettings);

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
        PryGuardBrowser.Address = "https://google.com/";
        return PryGuardBrowser;
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
            tabItem.Address = e.NewValue.ToString();

            // Update the Address property of the ViewModel
            Address = e.NewValue.ToString();
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
    private void SaveHistoryJson(string address, string desc)
    {
        if (!File.Exists(_profileHistoryPath))
        {
            // Ensure the file is created if it doesn't exist
            File.Create(_profileHistoryPath).Close();
        }

        Task.Run(() =>
        {
            // Create a history item with the current date including the year
            var hist = new PryGuardHistoryItem(DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                desc, address.Replace("https://", ""));

            // Insert the new history item at the start of the list
            PryGuardHistoryList.Insert(0, hist);

            // Serialize the history list to JSON
            var doc = JsonSerializer.Serialize(PryGuardHistoryList);

            // Write the serialized data to the file
            using StreamWriter writer = new(_profileHistoryPath);
            writer.Write(doc);
            writer.Close();

            // Update the UI on the main thread
            Application.Current.Dispatcher.Invoke(delegate
            {
                var listBoxItem = new ListViewItem();

                // Set properties for the list box item
                ListViewItemProperties.SetTimeHistory(listBoxItem, hist.Time);
                ListViewItemProperties.SetDescHistory(listBoxItem, hist.Description);
                ListViewItemProperties.SetLinkPreview(listBoxItem, hist.Link[..hist.Link.IndexOf('/')]);
                ListViewItemProperties.SetFullLink(listBoxItem, hist.Link);
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

    private void AddListViewItem(PryGuardHistoryItem item)
    {
        Application.Current.Dispatcher.Invoke((Action)delegate
        {
            var listBoxItem = new ListViewItem();

            ListViewItemProperties.SetTimeHistory(listBoxItem, item.Time);
            ListViewItemProperties.SetDescHistory(listBoxItem, item.Description);
            ListViewItemProperties.SetLinkPreview(listBoxItem, item.Link[..item.Link.IndexOf('/')]);
            ListViewItemProperties.SetFullLink(listBoxItem, item.Link);
            _listView.Items.Add(listBoxItem);
        });
    }
    private async void LoadHistoryAsListView()
    {
        _listView.Items.Clear();
        Task.Run(() =>
        {
            foreach (var item in PryGuardHistoryList)
            {
                AddListViewItem(item);
            }
        });
    }
    #endregion

    #region Tab Work
    private async void AddTabHistory()
    {
        if (PryGuardHistoryList.Count == 0) { return; }

        Tabs.Add(new CustomTabItem() { Tag = _mainIDCounter, Content = _listView });
        CurrentTabItem = Tabs.Last();
        Address = "PryGuard://history/";
        var button = new Label
        {
            Content = "History",
            AllowDrop = true,
            Tag = _mainIDCounter
        };
        button.DragEnter += BtnTabDragEnter;
        button.MouseLeftButtonDown += BtnMouseDownForDragAndOpenTab;

        if (_mainIDCounter == 0) { TabBtnsAndAddTabBtn.Insert(0, button); }
        else { TabBtnsAndAddTabBtn.Insert(TabBtnsAndAddTabBtn.Count - 1, button); }

        _mainIDCounter++;
        LoadHistoryAsListView();
    }
    private async void AddTab()
    {
        var browser = await InitBrowser(_mainIDCounter > 0);
        //var browser = await InitBrowser(true);
        browser.TitleChanged += Browser_TitleChanged;
        browser.LoadingStateChanged += Browser_LoadingStateChanged;
        browser.AddressChanged += Browser_AddressChanged;

        var newTabItem = new CustomTabItem()
        {
            Tag = _mainIDCounter,
            Content = browser,
            Title = browser.Title,
            Address = browser.Address,
            CloseTabCommand = CloseTabCommand  // Set the command
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
        if (parameter is not MouseButtonEventArgs mouseArgs || mouseArgs.Source is not TextBlock tb) return;
        var id = (int)tb.Tag;

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
            else { Address = (CurrentTabItem.Content as PryGuardBrowser).Address; }
        }
    }
    #endregion

    #region Window Work
    private void AddressOnKeyDown(object arg)
    {
        if ((arg as KeyEventArgs).Key == Key.Enter)
        {
            (CurrentTabItem.Content as PryGuardBrowser).LoadUrlAsync(Address);
        }
    }
    private void Back(object arg)
    {
        if (CurrentTabItem == null) return;
        (CurrentTabItem.Content as PryGuardBrowser).Back();
    }
    private void Forward(object arg)
    {
        if (CurrentTabItem == null) return;
        (CurrentTabItem.Content as PryGuardBrowser).Forward();
    }
    private void Refresh(object arg)
    {
        if (CurrentTabItem == null) return;

        if (CurrentTabItem.Content.ToString().Contains("ListView"))
        {
            CurrentTabItem.Content = _listView;
        }
        else
        {
            (CurrentTabItem.Content as PryGuardBrowser).Reload();
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
        CurWindowState = WindowState.Maximized;

        //if (CurWindowState == WindowState.Maximized)
        //{
        //    CurWindowState = WindowState.Normal;
        //}
        //else
        //{
        //    CurWindowState = WindowState.Maximized;
        //}
    }
    private void CloseWindow(object obj)
    {
        ViewManager.Close(this);
        
        
        
    }
    public bool CanClose()
    {
        return true;
    }
    #endregion
}