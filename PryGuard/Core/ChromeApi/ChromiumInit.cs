using System;
using System.IO;
using CefSharp;
using CefSharp.Wpf;
using PryGuard.Core.ChromeApi.Model.Configs;
using PryGuard.Core.ChromeApi.Settings;
using PryGuard.Model;

namespace PryGuard.Core.ChromeApi
{
    public static class ChromiumInit
    {
        public static void Init(PryGuardProfile PryGuardProfileToStart)
        {
            // Ensuring subprocess is closed if the parent process is terminated
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            CefSharpSettings.ShutdownOnExit = true;

            var cefSettings = new CefSettings
            {
                Locale = PryGuardProfileToStart.FakeProfile.CurrentChromeLanguage.ToLocal(),
                UserAgent = PryGuardProfileToStart.FakeProfile.UserAgent,
                PersistSessionCookies = true,
                LogSeverity = LogSeverity.Error
            };


            // Disable GPU Acceleration for stability
            cefSettings.DisableGpuAcceleration();

            // Ensure there are no conflicting command-line arguments
            if (!cefSettings.CefCommandLineArgs.ContainsKey("disable-gpu"))
            {
                cefSettings.CefCommandLineArgs.Add("disable-gpu", "1");
            }

            cefSettings.CefCommandLineArgs.Add("disable-gpu-vsync", "1");
            cefSettings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");

            // WebGL and media stream settings
            cefSettings.CefCommandLineArgs.Add("enable-webgl-draft-extensions", "1");
            cefSettings.CefCommandLineArgs.Add("enable-webgl", "1");
            cefSettings.CefCommandLineArgs.Add("enable-media-stream", "0");

            // Memory and audio settings
            cefSettings.CefCommandLineArgs.Add("mute-audio", "1");
            cefSettings.CefCommandLineArgs.Add("js-flags", "--max_old_space_size=5000");

            // Handle ignoring certificate errors for smoother browsing
            cefSettings.CefCommandLineArgs.Add("ignore-certificate-errors", "1");

            // Initialize Cef if not already initialized
            if (!Cef.IsInitialized.GetValueOrDefault())
            {
                // Perform dependency check and initialize browser context
                if (!Cef.Initialize(cefSettings, performDependencyCheck: true, browserProcessHandler: null))
                {
                    throw new ArgumentException("Chrome is not initialized");
                }
            }

        }
    }
}
