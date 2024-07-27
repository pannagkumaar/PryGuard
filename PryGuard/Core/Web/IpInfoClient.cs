using System;
using System.Net;
using PryGuard.Model;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PryGuard.Core.Web;

/// <summary>
/// Makes a request to a site that returns ip information (geolocation, time zone, etc.)
/// </summary>
public class IpInfoClient
{
    public static async Task<IpInfoResult> CheckClientProxy(ProxySettings proxySetting)
    {
        var proxy = new WebProxy
        {
            Address = new Uri($"{proxySetting.ProxyType}://{proxySetting.ProxyAddress}:{proxySetting.ProxyPort}"),
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false,

            // Proxy credentials
            Credentials = new NetworkCredential(
                userName: proxySetting.ProxyLogin,
                password: proxySetting.ProxyPassword)
        };

        // Create a client handler that uses the proxy
        var httpClientHandler = new HttpClientHandler
        {
            //Proxy = proxy,
        };

        // Disable SSL verification
        httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        // Finally, create the HTTP client object
        var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
        try
        {
            var json = await client.GetStringAsync($"http://ipinfo.io/{proxy.Address.Host}/json?token=ceade680c77b34"); // use your own token
            var result = JsonSerializer.Deserialize<IpInfoResult>(json);
            if (result.Ip == proxy.Address.Host) { return result; }
            else { throw new HttpRequestException(); }
        }
        catch (HttpRequestException) { return null; }
    }
}