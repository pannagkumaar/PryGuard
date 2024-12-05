namespace PryGuard.Core.Browser.Model.Configs;
public class WebGLParam
{
    public int Key { get; set; }
    public string Value { get; set; }

    public WebGLParam(int key, string value)
    {
        Key = key;
        Value = value;
    }
}