using System;

using CefSharp;
using CefSharp.Wpf;
using PryGuard.Core.Browser.Settings;

using PryGuard.DataModels;

namespace PryGuard.Core.Browser;
public static class ChromiumInit
{
    public static void Init(PryGuardProfile PryGuardProfileToStart)
    {
        // CefSharpSettings.LegacyJavascriptBindingEnabled = true;
        CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
        CefSharpSettings.ShutdownOnExit = true;
        // CefSharpSettings.ConcurrentTaskExecution = true;
        var cefSettings = new CefSettings();
        // cefSettings.LocalesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "locales");
        cefSettings.Locale = PryGuardProfileToStart.FakeProfile.CurrentChromeLanguage.ToLocal();

        cefSettings.UserAgent = PryGuardProfileToStart.FakeProfile.UserAgent;
        // if (!cefSettings.CefCommandLineArgs.ContainsKey("disable-gpu"))
        //cefSettings.CefCommandLineArgs.Add("disable-gpu", "1");
        ///cefSettings.RootCachePath = ClientConfig.ChromeDataPath;
        if (cefSettings.CefCommandLineArgs.ContainsKey("enable-media-stream"))
            cefSettings.CefCommandLineArgs.Remove("enable-media-stream");
        cefSettings.DisableGpuAcceleration();
        cefSettings.CommandLineArgsDisabled = true;
        cefSettings.PersistSessionCookies = true;
        cefSettings.PersistUserPreferences = true;
        ///if (!appsettings.IsLoadImage)
        ///    cefSettings.CefCommandLineArgs.Add("disable-image-loading", "1");
        cefSettings.CefCommandLineArgs.Remove("disable-cache-settings=0");
        cefSettings.CefCommandLineArgs.Add("disable-gpu=1");
        cefSettings.CefCommandLineArgs.Add("disable-gpu-vsync=1");
        cefSettings.CefCommandLineArgs.Add("disable-gpu-compositing=1");
        // test
        cefSettings.CefCommandLineArgs.Add("renderer-process-limit", "1");
        cefSettings.CefCommandLineArgs.Add("enable-webgl-draft-extensions=1");
        cefSettings.CefCommandLineArgs.Add("enable-webgl=1");
        cefSettings.CefCommandLineArgs.Add("enable-media-stream=0");
        // cefSettings.CefCommandLineArgs.Add("shared-texture-enabled=0");
        // cefSettings.CefCommandLineArgs.Add("force-device-scale-factor=1");
        // cefSettings.CefCommandLineArgs.Add("disable-pinch=1");
        // cefSettings.CefCommandLineArgs.Add("disable-notifications");
        cefSettings.CefCommandLineArgs.Add("mute-audio=1");
        cefSettings.CefCommandLineArgs.Add("ignore-certificate-errors=1");
        cefSettings.CefCommandLineArgs.Add("js-flags=--max_old_space_size=5000");
        // cefSettings.CefCommandLineArgs.Add("disable-features=sparerendererforsiteperprocess");
        // cefSettings.CefCommandLineArgs.Add("enable-features=CastMediaRouteProvider");
        // cefSettings.CefCommandLineArgs.Add("disable-features=CalculateNativeWinOcclusion");
        // cefSettings.CefCommandLineArgs.Add("uncaught-exception-stack-size=10");
        ///cefSettings.LogFile = Path.Combine(ClientConfig.ChromeDataPath, "Log.txt");
        cefSettings.LogSeverity = LogSeverity.Error;
        cefSettings.IgnoreCertificateErrors = true;
        if (!Cef.IsInitialized && !Cef.Initialize(cefSettings))
            throw new ArgumentException("Chrome is not initialized");
    }
}
