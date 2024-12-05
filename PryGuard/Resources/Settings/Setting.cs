using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using PryGuard.DataModels;
using PryGuard.UI.ViewModels;

namespace PryGuard.Resources.Settings;
public class Setting : BaseViewModel
{
    private string _settingsJsonPath = Directory.GetCurrentDirectory() + @"\settings.json";

    private List<PryGuardProfile> _PryGuardProfiles;
    public List<PryGuardProfile> PryGuardProfiles
    {
        get => _PryGuardProfiles;
        set => Set(ref _PryGuardProfiles, value);
    }

    public Setting()
    {
        if (!LoadSettings()) { SetDefaultSettings(); }
    }

    private void SetDefaultSettings() { PryGuardProfiles = new(); }
    public void ParseJson(JsonNode json)
    {
        if (json[nameof(PryGuardProfiles)] != null)
            PryGuardProfiles = json[nameof(PryGuardProfiles)].Deserialize<List<PryGuardProfile>>();
        else
            PryGuardProfiles = new List<PryGuardProfile>();
    }

    public void SaveSettings()
    {
        using StreamWriter writer = new(_settingsJsonPath);
        var doc = JsonSerializer.Serialize(this);
        writer.Write(doc);
        writer.Close();
    }

    private bool LoadSettings()
    {
        try
        {
            using StreamReader reader = new(_settingsJsonPath);
            var json = reader.ReadToEnd();
            ParseJson(JsonNode.Parse(json));
            reader.Close();
            return true;
        }
        catch (Exception) { return false; }
    }
}
