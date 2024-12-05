using CefSharp;


namespace PryGuard.Core.Browser.Handlers;
public class MenuHandler : IContextMenuHandler
{
    public void OnBeforeContextMenu(
      IWebBrowser browserControl,
      IBrowser browser,
      IFrame frame,
      IContextMenuParams parameters,
      IMenuModel model)
    {
    }

    public bool OnContextMenuCommand(
      IWebBrowser browserControl,
      IBrowser browser,
      IFrame frame,
      IContextMenuParams parameters,
      CefMenuCommand commandId,
      CefEventFlags eventFlags)
    {
        return false;
    }

    public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame) { }

    public bool RunContextMenu(
      IWebBrowser browserControl,
      IBrowser browser,
      IFrame frame,
      IContextMenuParams parameters,
      IMenuModel model,
      IRunContextMenuCallback callback)
    {
        return true;
    }
}
