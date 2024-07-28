using PryGuard.Services.Helpers;
using System.Collections.Generic;

namespace PryGuard.Core.ChromeApi.Model.Configs;
public class WebGLFactory
{
    public static List<string> Vendors = new List<string>() { "Google inc." };//param 37445
    public static List<string> Renderers = new List<string>() { "ANGLE (NVIDIA GeForce GTX 1050 ti Direct3D11 vs_5_0 ps_5_0)" };//param 37446

    public static WebGLSetting Generate()
    {
        WebGLSetting.WebGlNoise noise = new WebGLSetting.WebGlNoise();
        noise.Index = (int)(FakeProfileFactory.GenerateRandomDouble() % 10);
        noise.Difference = FakeProfileFactory.GenerateRandomDouble() * 0.00001;
        WebGLSetting glSetting = new(noise);
        glSetting.Status = WebGLSetting.WebGlStatus.NOISE;
        glSetting.Params.Add(WebGLSetting.UNMASKED_VENDOR, new WebGLParam(WebGLSetting.UNMASKED_VENDOR, Vendors.GetRandValue()));
        glSetting.Params.Add(WebGLSetting.UNMASKED_RENDERER, new WebGLParam(WebGLSetting.UNMASKED_RENDERER, Renderers.GetRandValue()));
        return glSetting;
    }

    public static WebGLSetting Generate(PryGuard.Model.Fingerprint fingerprint)
    {
        WebGLSetting.WebGlNoise noise = new WebGLSetting.WebGlNoise();
        noise.Index = (int)(FakeProfileFactory.GenerateRandomDouble() % 10);
        noise.Difference = FakeProfileFactory.GenerateRandomDouble() * 0.00001;
        WebGLSetting glSetting = new WebGLSetting(noise);
        glSetting.Status = WebGLSetting.WebGlStatus.NOISE;
        glSetting.Params.Add(WebGLSetting.UNMASKED_VENDOR, new WebGLParam(WebGLSetting.UNMASKED_VENDOR, fingerprint.Vendor));
        glSetting.Params.Add(WebGLSetting.UNMASKED_RENDERER, new WebGLParam(WebGLSetting.UNMASKED_RENDERER, fingerprint.Renderer));
        foreach (var botFingerprintParam in fingerprint.Params)
        {
            glSetting.Params.Add(botFingerprintParam.Key, new WebGLParam(botFingerprintParam.Key, botFingerprintParam.Value));
        }
        return glSetting;
    }
}