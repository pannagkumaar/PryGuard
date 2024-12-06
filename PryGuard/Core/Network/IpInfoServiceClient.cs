using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PryGuard.DataModels;

namespace PryGuard.Core.Network
{
    /// <summary>
    /// Provides functionality to fetch IP information such as geolocation and time zone.
    /// </summary>
    public class IpInfoServiceClient
    {
        /// <summary>
        /// Checks the proxy by making a request to an IP information service.
        /// </summary>
        /// <param name="proxySettings">The proxy settings to use for the request.</param>
        /// <returns>An <see cref="IpInformation"/> containing IP information, or <c>null</c> if the request fails.</returns>
        public static async Task<IpInformation> FetchProxyInfoAsync(ProxySettings proxySettings)
        {
            if (proxySettings == null)
                throw new ArgumentNullException(nameof(proxySettings));

            var proxy = new WebProxy
            {
                Address = new Uri($"{proxySettings.ProxyType}://{proxySettings.ProxyAddress}:{proxySettings.ProxyPort}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false
            };

            if (proxySettings.IsProxyAuth)
            {
                proxy.Credentials = new NetworkCredential(
                    userName: proxySettings.ProxyLogin,
                    password: proxySettings.ProxyPassword);
            }

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(httpClientHandler, disposeHandler: true);

            try
            {
                var jsonResponse = await client.GetStringAsync("http://ipinfo.io/json?token=dc60639f7aa116");
                var ipInfoResult = JsonSerializer.Deserialize<IpInformation>(jsonResponse);

                if (ipInfoResult?.IpAddress == proxySettings.ProxyAddress)
                {
                    return ipInfoResult;
                }

                throw new HttpRequestException("The proxy IP does not match the IP information from the service.");
            }
            catch (HttpRequestException ex)
            {
                // Log the exception or handle it as needed (e.g., telemetry)
                return null;
            }
        }
    }
}
