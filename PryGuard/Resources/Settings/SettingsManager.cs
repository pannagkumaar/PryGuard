using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using PryGuard.DataModels;
using PryGuard.UI.ViewModels;

namespace PryGuard.Resources.Settings
{
    /// <summary>
    /// Manages application settings, including saving and loading profiles.
    /// </summary>
    public class SettingsManager : BaseViewModel
    {
        private readonly string _settingsFilePath;

        private List<PryGuardProfile> _pryGuardProfiles;
        /// <summary>
        /// The list of PryGuard profiles.
        /// </summary>
        public List<PryGuardProfile> PryGuardProfiles
        {
            get => _pryGuardProfiles;
            set => Set(ref _pryGuardProfiles, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsManager"/> class.
        /// </summary>
        /// <param name="settingsFilePath">The path to the settings file.</param>
        public SettingsManager(string settingsFilePath = null)
        {
            _settingsFilePath = settingsFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
            if (!LoadSettings())
            {
                SetDefaultSettings();
            }
        }

        /// <summary>
        /// Sets default settings for the application.
        /// </summary>
        private void SetDefaultSettings()
        {
            PryGuardProfiles = new List<PryGuardProfile>();
        }

        /// <summary>
        /// Parses a JSON node and populates the settings.
        /// </summary>
        /// <param name="json">The JSON node to parse.</param>
        private void ParseJson(JsonNode json)
        {
            if (json?[nameof(PryGuardProfiles)] != null)
            {
                PryGuardProfiles = json[nameof(PryGuardProfiles)].Deserialize<List<PryGuardProfile>>() ?? new List<PryGuardProfile>();
            }
            else
            {
                PryGuardProfiles = new List<PryGuardProfile>();
            }
        }

        /// <summary>
        /// Saves the settings to a file.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var serializedData = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_settingsFilePath, serializedData);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the settings from a file.
        /// </summary>
        /// <returns><c>true</c> if settings were loaded successfully; otherwise, <c>false</c>.</returns>
        private bool LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                ParseJson(JsonNode.Parse(json));
                return true;
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return false;
            }
        }
    }
}
