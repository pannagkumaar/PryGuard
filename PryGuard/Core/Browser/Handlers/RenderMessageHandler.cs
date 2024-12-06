using CefSharp;

namespace PryGuard.Core.Browser.Handlers;

/// <summary>
/// Chrome rendering events
/// </summary>
public class RenderMessageHandler : IRenderProcessMessageHandler
{
    private string ExecuteCode { get; }

    public RenderMessageHandler(string code)
    {
        ExecuteCode = code;
    }

    public void OnContextCreated(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
    {
        if (ExecuteCode == null)
        {
            return;
        }

        if (frame.IsValid)
        {
            frame.ExecuteJavaScriptAsync(ExecuteCode, "ext::fakeinjectionmain");
        }
    }

    public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
    {
    }

    public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode node)
    {
    }

    public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
      JavascriptException exception)
    {
    }
}
