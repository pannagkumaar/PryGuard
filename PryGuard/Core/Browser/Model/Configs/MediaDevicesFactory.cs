using System;
using System.Linq;
using System.Collections.Generic;

namespace PryGuard.Core.Browser.Model.Configs;
public class MediaDevicesFactory
{
    private static Random random = new();

    public static List<MediaDevice> generateDevices(int audioinputs, int audiooutputs, int videoinputs)
    {
        string audioGroup = generateRandomId();
        string videoGroup = generateRandomId();
        List<MediaDevice> devices = new List<MediaDevice>();
        if (audioinputs > 0)
        {
            devices.Add(MediaDevice.AudioInputDevice("default", audioGroup));
            devices.Add(MediaDevice.AudioInputDevice("communications", audioGroup));
            for (int i = 0; i < audioinputs; i++)
            {
                devices.Add(MediaDevice.AudioInputDevice(generateRandomId(), audioGroup));
            }
        }

        for (int i = 0; i < videoinputs; i++)
        {
            devices.Add(MediaDevice.VideoInputDevice(generateRandomId(), videoGroup));
        }

        if (audiooutputs > 0)
        {
            devices.Add(MediaDevice.AudioOutputDevice("default", audioGroup));
            devices.Add(MediaDevice.AudioOutputDevice("communications", audioGroup));
            for (int i = 0; i < audiooutputs; i++)
            {
                devices.Add(MediaDevice.AudioOutputDevice(generateRandomId(), audioGroup));
            }
        }

        return devices;
    }

    private static string generateRandomId()
    {
        return generateRandomString(64);
    }

    private static string generateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars.ToLower(), length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}