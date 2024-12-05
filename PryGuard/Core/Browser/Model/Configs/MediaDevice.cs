namespace PryGuard.Core.ChromeApi.Model.Configs;
public class MediaDevice
{
    public string DeviceId { get; set; }
    public string Kind { get; set; }
    public string Label { get; set; }
    public string GroupId { get; set; }

    public MediaDevice(string deviceId, string kind, string label, string groupId)
    {
        this.DeviceId = deviceId;
        this.Kind = kind;
        this.Label = label;
        this.GroupId = groupId;
    }

    public static MediaDevice AudioInputDevice(string id, string groupId)
    {
        return new MediaDevice(id, "audioinput", "", groupId);
    }

    public static MediaDevice AudioOutputDevice(string id, string groupId)
    {
        return new MediaDevice(id, "audiooutput", "", groupId);
    }

    public static MediaDevice VideoInputDevice(string id, string groupId)
    {
        return new MediaDevice(id, "videoinput", "", groupId);
    }
}