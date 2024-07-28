using System;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PryGuard.Services.Helpers;

public static class ClientConfig
{

    /// <summary>
    /// Where is the application
    /// </summary>
    public static string AppDataPath { get; set; }

    /// <summary>
    /// Where browser data is stored
    /// </summary>
    public static string ChromeDataPath { get; }

    /// <summary>
    /// Where the music is kept
    /// </summary>
    public static string AudioDataPath { get; }

    /// <summary>
    /// Where caches from all browsers are stored
    /// </summary>

    public static string ChromeCachePath { get; }

    static ClientConfig()
    {
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        AppDataPath = Path.Combine(Environment.CurrentDirectory, "PryGuard");
        ChromeDataPath = Path.Combine(AppDataPath, "chromiumData");
        ChromeCachePath = Path.Combine(ChromeDataPath, "chromiumCache");
    }

    public static DirectorySecurity updateDirSecurity(SecurityIdentifier id, DirectorySecurity sec)
    {
        // Using this instead of the "Everyone" string means we work on non-English systems.

        sec.AddAccessRule(new FileSystemAccessRule(id, FileSystemRights.FullControl,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
            AccessControlType.Allow));
        return sec;
    }
}
