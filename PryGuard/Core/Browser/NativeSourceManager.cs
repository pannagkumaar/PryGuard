using System;
using System.IO;
using System.Windows;
using System.Text.Json;
using System.Windows.Resources;
using System.Collections.Generic;
using PryGuard.Core.Browser.Model.Configs;

namespace PryGuard.Core.Browser;

public class NativeSourceManager
{
    private Dictionary<string, string> _codes = new();

    public NativeSourceManager()
    {
        LoadFakeCode("fakeinject.js");
    }

    private void LoadFakeCode(string filename)
    {
        string resource =
            LoadResource(new Uri("pack://application:,,,/Resources/JavaScript/Fake/" + filename, UriKind.Absolute));
        _codes[filename] = resource;
    }

    private void LoadJsCode(string filename)
    {
        string resource =
            LoadResource(new Uri("pack://application:,,,/Resources/JavaScript/" + filename, UriKind.Absolute));
        _codes[filename] = resource;
    }

    private string LoadResource(Uri urlResorse)
    {
        StreamResourceInfo resourceStream = Application.GetResourceStream(urlResorse);
        if (resourceStream == null)
            return null;
        string end;
        using (StreamReader streamReader = new StreamReader(resourceStream.Stream))
            end = streamReader.ReadToEnd();
        return end;
    }

    public string this[string fileName] => _codes[fileName + ".js"];

    public bool Contains(string fileName)
    {
        return _codes.ContainsKey(fileName + ".js");
    }

    public string GetCodeForFakeProfile(string fileName, FakeProfile fakeProfile) => this[fileName].Replace(
        "let fakeProfile = {}",
        "let fakeProfile = JSON.parse('" + JsonSerializer.Serialize(fakeProfile) + "')");

}