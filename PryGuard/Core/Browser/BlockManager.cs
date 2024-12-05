using System;

using System.Collections.Generic;

using System.Threading;
using CefSharp;
using PryGuard.Services.Helpers.Annotations;

namespace PryGuard.Core.ChromeApi
{
    /// <summary>
    /// Used to block requests to specific domains.
    /// </summary>
    public class BlockManager : IDisposable
    {
        private readonly HashSet<string> _domains;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockManager"/> class.
        /// </summary>
        public BlockManager(IEnumerable<string> initialDomains = null)
        {
            _domains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Tracking and Analytics Services
                 "mc.yandex",
                "yabs.yandex",
                "newrelic.com",
                "datadome.co",
                "google-analytics.com",
                "analytics.js",
                "dpm.demdex.net",
                "omtrdc.net",
                "fbevents.js",
                "maxymiser.net",
                "userzoom.com",
                "usabilla.com",
                "zopim.com",
                "ensighten.com",
                "fastviewdata.com",
                "pixel.gif?",
                "matomo.org",
                "mixpanel.com",
                "hotjar.com",
                "segment.io",
                "clicky.com",
                "heap.io",

                // Advertising and Marketing Tools
                "googletagmanager.com",
                "gtm.js?id=GTM",
                "openstat.net",
                "cnt.js",
                "top-fwz1.mail.ru",
                "connect.facebook.net",
                "brightcove.com",
                "bing.com/bat.js",
                "ads.doubleclick.net",
                "adroll.com",
                "adservice.google.com",
                "ads.pubmatic.com",
                "criteo.com",
                "serving-sys.com",

                // Security and Anti-Bot Services
                "connextra.com",
                "iesnare.com",
                "dmpcounter.com",
                "regstat.betfair.com",
                "cloudfront.loggly.com",
                "counter.rambler.ru",
                "static.hotjar.com",
                "cedexis.com",
                "cloudflare.com",
                "reblaze.com",
                "akismet.com",
                "honeypot.com",

                // Miscellaneous
                "whdn.williamhill.com/core/ob/static/cust/images/en/stats.gif?ver=",
                "static.hotjar.com/c/",
                "pixel.ads",
                "tracking.pixel",
                "analytics.pixel",
                "logstash",
                "error.log"
            };

            if (initialDomains != null)
            {
                foreach (var domain in initialDomains)
                {
                    _domains.Add(domain);
                }
            }
        }

        /// <summary>
        /// Indicates whether the BlockManager is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Determines whether the specified URL should be blocked based on the block list.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <param name="mainFrameUrl">The main frame URL, if applicable.</param>
        /// <returns><c>true</c> if the URL should be blocked; otherwise, <c>false</c>.</returns>
        public bool IsBlock([CanBeNull] string url, [CanBeNull] string mainFrameUrl)
        {
            if (!IsActive || string.IsNullOrWhiteSpace(url))
                return false;

            if (!string.IsNullOrWhiteSpace(mainFrameUrl))
            {
                var mainDomain = ExtractDomain(mainFrameUrl);
                if (mainDomain == null)
                    return false;

                // Example condition: Do not block if the main frame is a trusted domain
                // Modify this condition based on your requirements
                if (IsTrustedDomain(mainDomain))
                    return false;
            }

            var domain = ExtractDomain(url);
            if (domain == null)
                return false;

            _lock.EnterReadLock();
            try
            {
                return _domains.Contains(domain);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Adds a domain to the block list.
        /// </summary>
        /// <param name="domainName">The domain name to add.</param>
        public void AddDomain(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
                throw new ArgumentException("Domain name cannot be null or whitespace.", nameof(domainName));

            _lock.EnterWriteLock();
            try
            {
                _domains.Add(domainName);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a domain from the block list.
        /// </summary>
        /// <param name="domainName">The domain name to remove.</param>
        /// <returns><c>true</c> if the domain was removed; otherwise, <c>false</c>.</returns>
        public bool RemoveDomain(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
                return false;

            _lock.EnterWriteLock();
            try
            {
                return _domains.Remove(domainName);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether the specified domain is trusted.
        /// Modify this method based on your criteria for trusted domains.
        /// </summary>
        /// <param name="domain">The domain to check.</param>
        /// <returns><c>true</c> if the domain is trusted; otherwise, <c>false</c>.</returns>
        private bool IsTrustedDomain(string domain)
        {
            // Example trusted domains
            var trustedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "example.com",
                "trustedsite.com",
                "mysite.org"
                // Add more trusted domains as needed
            };

            return trustedDomains.Contains(domain);
        }

        /// <summary>
        /// Extracts the domain from a given URL.
        /// </summary>
        /// <param name="url">The URL from which to extract the domain.</param>
        /// <returns>The domain if extraction is successful; otherwise, <c>null</c>.</returns>
        private string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch (UriFormatException)
            {
                // Handle invalid URL formats if necessary
                return null;
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="BlockManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="BlockManager"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _lock?.Dispose();
                // Dispose other managed resources here if necessary
            }

            // Free unmanaged resources here if necessary

            _disposed = true;
        }

        // Override methods if necessary
        [JavascriptIgnore]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [JavascriptIgnore]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [JavascriptIgnore]
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
