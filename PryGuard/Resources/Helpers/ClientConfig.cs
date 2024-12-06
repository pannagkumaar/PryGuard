using System;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace PryGuard.Resources.Helpers
{
    /// <summary>
    /// Provides configuration paths and utilities for the PryGuard client.
    /// </summary>
    public static class ClientConfig
    {
        /// <summary>
        /// The root application data directory for PryGuard.
        /// </summary>
        public static string AppDataPath { get; private set; }

        /// <summary>
        /// The directory where browser data is stored.
        /// </summary>
        public static string ChromeDataPath { get; private set; }

        /// <summary>
        /// The directory where cached browser data is stored.
        /// </summary>
        public static string ChromeCachePath { get; private set; }

        /// <summary>
        /// The directory where audio data is stored.
        /// </summary>
        public static string AudioDataPath { get; private set; }

        /// <summary>
        /// Initializes static members of the <see cref="ClientConfig"/> class.
        /// </summary>
        static ClientConfig()
        {
            InitializePaths();
        }

        /// <summary>
        /// Sets up the directory paths for the application.
        /// </summary>
        private static void InitializePaths()
        {
            string currentDir = Environment.CurrentDirectory;
            AppDataPath = Path.Combine(currentDir, "PryGuard");
            ChromeDataPath = Path.Combine(AppDataPath, "ChromiumData");
            ChromeCachePath = Path.Combine(ChromeDataPath, "Cache");
            AudioDataPath = Path.Combine(AppDataPath, "Audio");

            EnsureDirectoryExists(AppDataPath);
            EnsureDirectoryExists(ChromeDataPath);
            EnsureDirectoryExists(ChromeCachePath);
            EnsureDirectoryExists(AudioDataPath);
        }

        /// <summary>
        /// Ensures that a directory exists. If it does not, it is created.
        /// </summary>
        /// <param name="path">The directory path to check or create.</param>
        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Updates the directory security settings to grant full control to the specified security identifier.
        /// </summary>
        /// <param name="securityIdentifier">The security identifier to grant access.</param>
        /// <param name="directorySecurity">The directory security object to update.</param>
        /// <returns>The updated <see cref="DirectorySecurity"/> object.</returns>
        public static DirectorySecurity UpdateDirectorySecurity(SecurityIdentifier securityIdentifier, DirectorySecurity directorySecurity)
        {
            if (securityIdentifier == null)
                throw new ArgumentNullException(nameof(securityIdentifier));

            if (directorySecurity == null)
                throw new ArgumentNullException(nameof(directorySecurity));

            directorySecurity.AddAccessRule(new FileSystemAccessRule(
                securityIdentifier,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));

            return directorySecurity;
        }
    }
}
