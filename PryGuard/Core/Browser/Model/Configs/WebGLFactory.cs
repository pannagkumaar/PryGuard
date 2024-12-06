using PryGuard.DataModels;
using PryGuard.Resources.Helpers;
using System.Collections.Generic;

namespace PryGuard.Core.Browser.Model.Configs;
public class WebGLFactory
{
    public static List<string> Vendors = new List<string>()
{
    "Google inc.",
    //"AMD",                     
    //"NVIDIA",                   
    //"Intel",                    
    //"Apple",                  
    //"Qualcomm",                 
    
};//param 37445
    public static List<string> Renderers = new List<string>() { "ANGLE (NVIDIA GeForce GTX 1050 ti Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE (AMD Radeon R5 340 (0x00006611) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE (NVIDIA GeForce RTX 2070 with Max-Q Design (0x00001F10) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE (Intel Inc., Intel(R) Iris(TM) Plus Graphics 645)",
    "ANGLE (Intel(R) Iris(R) Xe Graphics (0x0000A7A1) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE (AMD Radeon(TM) Graphics (0x00001681) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE ( NVIDIA GeForce GTX 1660 SUPER (0x000021C4) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE ( NVIDIA GeForce RTX 3060 Ti (0x00002486) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE (Intel, Intel(R) Iris(R) Xe Graphics (0x000046A6) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE (Intel, Intel(R) UHD Graphics 630 (0x00003E92) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE ( Intel(R) UHD Graphics (0x0000A78B) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE ( NVIDIA GeForce GTX 1650 (0x00001F91) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE ( Intel(R) UHD Graphics (0x00009B41) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE (AMD Radeon (TM) Graphics (0x000015E7) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE ( NVIDIA GeForce RTX 3060 Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE ( NVIDIA GeForce RTX 4060 Ti (0x00002803) Direct3D11 vs_5_0 ps_5_0)",
    "ANGLE ( NVIDIA GeForce RTX 3080 Ti (0x00002208) Direct3D11 vs_5_0 ps_5_0)",

    };//param 37446

    public static WebGLSetting Generate()
    {
        WebGLSetting.WebGlNoise noise = new WebGLSetting.WebGlNoise();
        noise.Index = (int)(FakeProfileFactory.GenerateRandomDouble() % 10);
        noise.Difference = FakeProfileFactory.GenerateRandomDouble() * 0.00001;
        WebGLSetting glSetting = new(noise);
        glSetting.Status = WebGLSetting.WebGlStatus.NOISE;
        glSetting.Params.Add(WebGLSetting.UNMASKED_VENDOR, new WebGLParam(WebGLSetting.UNMASKED_VENDOR, Vendors.GetRandomValue()));
        glSetting.Params.Add(WebGLSetting.UNMASKED_RENDERER, new WebGLParam(WebGLSetting.UNMASKED_RENDERER, Renderers.GetRandomValue()));
        return glSetting;
    }

    public static WebGLSetting Generate(Fingerprint fingerprint)
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