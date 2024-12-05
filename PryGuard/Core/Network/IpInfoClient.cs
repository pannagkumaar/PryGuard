using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PryGuard.DataModels;

namespace PryGuard.Core.Network
{
    /// <summary>
    /// Makes a request to a site that returns IP information (geolocation, time zone, etc.)
    /// </summary>
    public class IpInfoClient
    {
        public static async Task<IpInfoResult> CheckClientProxy(ProxySettings proxySetting)
        {
            var proxy = new WebProxy
            {
                Address = new Uri($"{proxySetting.ProxyType}://{proxySetting.ProxyAddress}:{proxySetting.ProxyPort}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false
            };

            if (proxySetting.IsProxyAuth)
            {
                // Proxy credentials
                proxy.Credentials = new NetworkCredential(
                    userName: proxySetting.ProxyLogin,
                    password: proxySetting.ProxyPassword);
            }

            // Create a client handler that uses the proxy
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            // Finally, create the HTTP client object
            var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            try
            {
                var json = await client.GetStringAsync($"http://ipinfo.io/json?token=dc60639f7aa116");
                var result = JsonSerializer.Deserialize<IpInfoResult>(json);
                if (result.Ip == proxySetting.ProxyAddress)
                {
                    return result;
                }
                else
                {
                    throw new HttpRequestException();
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
