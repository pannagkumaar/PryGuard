using System;
using CefSharp;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PryGuard.Core.Browser;
public class JsWorker
{
    public Action<string> WriteLogDelegate;
    Action<string> _changeBkViewName;
    Action<bool> _reloadBrowserDelegate;
    Action<string> _loadBrowserDelegate;

    IJavascriptCallback _fastLoadCallback;
    IJavascriptCallback _consoleLog;
    Func<List<byte>> _saveScreenDelegate;

    public JsWorker(RequestContext requestContext)
    {
        RequestContext = requestContext;
    }

    #region Properties 
    [JavascriptIgnore] public bool? HasResult { get; set; }
    [JavascriptIgnore] public string EventMeta { get; set; }
    [JavascriptIgnore] public string MarketMeta { get; set; }
    [JavascriptIgnore] public string EventUrl { get; set; }
    [JavascriptIgnore] public string BetName { get; set; }
    [JavascriptIgnore] public string BkViewName { get; set; }
    [JavascriptIgnore] public string TeamOne { get; set; }
    [JavascriptIgnore] public string TeamTwo { get; set; }
    [JavascriptIgnore] public int SportId { get; set; }
    [JavascriptIgnore] public bool CheckRestriction { get; set; }
    [JavascriptIgnore] public bool UseBonus { get; set; }
    [JavascriptIgnore] public int LoginTry { get; set; }
    [JavascriptIgnore] public bool IsReOpeningCoupon { get; set; }
    [JavascriptIgnore] public bool IsNotCheckWaitingStake { get; set; }
    [JavascriptIgnore] public int ShoulderNumber { get; set; }
    [JavascriptIgnore] public string CoefAcceptChange { get; set; }
    [JavascriptIgnore] public bool IsShowStake { get; set; }
    [JavascriptIgnore] public string Login { get; set; }
    [JavascriptIgnore] public string Password { get; set; }
    [JavascriptIgnore] public decimal ProfitCommission { get; set; }
    [JavascriptIgnore] public bool EnableVipBetcity { get; set; }
    [JavascriptIgnore] public bool IsLive { get; set; }
    [JavascriptIgnore] public bool NotifyIfTrader { get; set; }
    [JavascriptIgnore] public RequestContext RequestContext { get; }
    public bool IsNewLoad { get; set; }

    #endregion 

    #region Public
    public void SetCallBacks(
        IJavascriptCallback consoleLog)
    {
        _consoleLog = consoleLog;

    }

    public void SetFastCallback(IJavascriptCallback callback)
    {
        _fastLoadCallback = callback;
    }

    [JavascriptIgnore]
    public async Task<bool> ExecuteCallbackAsync(
        IJavascriptCallback callback,
        string methodName,
        params object[] parms)
    {
        HasResult = null;
        if (callback == null)
            throw new ArgumentNullException(methodName + " == null");
        if (!callback.CanExecute)
            WriteLine(methodName + " Can Not Execute");
        JavascriptResponse jr = await callback.ExecuteAsync(parms);
        int i = 0;
        while (!HasResult.HasValue)
        {
            if (jr.Success || jr.Message == null)
            {
                if (i < 1200)
                {
                    await Task.Delay(50);
                    i++;
                }
                else
                {
                    jr.Success = false;
                    jr.Message = "Lagging";
                    return false;
                }
            }
            else
            {
                WriteLine(methodName + " Not Success. " + jr.Message);
                return false;
            }
        }

        return true;
    }



    public void WriteLine(string msg) => WriteLogDelegate?.Invoke(msg);

    [JavascriptIgnore]
    public void SetChangeViewDelegate(Action<string> changeBkViewName) => _changeBkViewName = changeBkViewName;

    public void ChangeViewName(string name) => _changeBkViewName?.Invoke(name);

    public void JSReturn(bool res) => HasResult = res;

    [JavascriptIgnore]
    public void Reload(bool force) => _reloadBrowserDelegate?.Invoke(force);

    public void LoadEventUrl() => Load(EventUrl);
    public void Load(string url) => _loadBrowserDelegate?.Invoke(url);
    //public void SendInformedMessage(string msg) => InformedManager?.SendMessage(msg);
    public void Reload() => Reload(true);

    public void ClearCache()
    {
        RequestContext.GetCookieManager(new TaskCompletionCallback())
            .DeleteCookies();
        Reload();
    }

    [JavascriptIgnore]
    public void SetSaveScreenDelegate(Func<List<byte>> saveScreenDelegate)
    {
        _saveScreenDelegate = saveScreenDelegate;
    }

    [JavascriptIgnore]
    public Func<List<byte>> GetScreenSaveDelegate()
    {
        return _saveScreenDelegate;
    }

    [JavascriptIgnore]
    public void SetBrowserDelegates(Action<bool> reload, Action<string> load)
    {
        _reloadBrowserDelegate = reload;
        _loadBrowserDelegate = load;
    }

    //[JavascriptIgnore]
    //public void SetInformed(IInformedManager informed)
    //{
    //    InformedManager = informed;
    //}

    public bool FastLoad()
    {
        if (_fastLoadCallback == null || _fastLoadCallback.IsDisposed ||
            !_fastLoadCallback.CanExecute)
            return false;
        _fastLoadCallback.ExecuteAsync();
        return true;
    }

    #endregion
}