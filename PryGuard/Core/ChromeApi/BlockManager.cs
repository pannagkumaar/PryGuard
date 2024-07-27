using System;
using CefSharp;
using System.Collections.Generic;
using PryGuard.Services.Helpers.Annotations;

namespace PryGuard.Core.ChromeApi;

/// <summary>
/// Used to block requests to specific domains
/// </summary>
public class BlockManager : IDisposable
{
    private HashSet<string> _domains = new HashSet<string>()
        {
            "whdn.williamhill.com/core/ob/static/cust/images/en/stats.gif?ver=",
            "mc.yandex",
            "yabs.yandex", "newrelic", "zopim.com", "connextra.com", "datadome.co", "dmpcounter.com", "iesnare.com",
            "ensighten.com", "fastviewdata.com", "googletagmanager.com/gtm.js?id=GTM", "openstat.net/cnt.js",
            "top-fwz1.mail.ru/js/code.js", "connect.facebook.net/ru_RU/sdk.js", "cedexis.com", "pixel.gif?",
            "cloudfront.loggly.com", "counter.rambler.ru/top100.jcn?", "static.hotjar.com/c/",
            "google-analytics.com/analytics.js", "regstat.betfair.com", "maxymiser.net", "fbevents.js",
            "dpm.demdex.net", "omtrdc.net", "google-analytics.com", "brightcove.com", "userzoom.com", "bing.com/bat.js",
            "usabilla.com", "group-ib.ru"
    };
    public bool IsWork { get; set; } = true;

    public bool IsBlock([CanBeNull] string url, [CanBeNull] string mainFrameUrl)
    {
        //if (!IsWork || string.IsNullOrWhiteSpace(url) ||
        //    (mainFrameUrl.Contains("skrill.com") || mainFrameUrl.Contains("neteller.com")))
        //    return false;
        url = url.Trim().ToLower();
        foreach (string domain in _domains)
        {
            if (url.Contains(domain.ToLower()))
                return true;
        }

        return false;
    }

    public void AddDomain(string domainName)
    {
        _domains.Add(domainName);
    }

    public bool RemoveDomain(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            return false;
        return _domains.Remove(domainName);
    }

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

    public void Dispose()
    {
    }
}
