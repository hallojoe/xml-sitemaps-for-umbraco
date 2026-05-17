using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed class SitemapUrlBuilder : ISitemapUrlBuilder
{
    public string BuildContentUrl(IPublishedContent content, string languageCode, string? hostname)
    {
        if (!string.IsNullOrWhiteSpace(hostname) && !hostname.StartsWith("https://"))
        {
            hostname = "https://" + hostname.Trim('/');
        }

        return CombineWithHostname(
            content.Url(mode: UrlMode.Relative, culture: languageCode),
            hostname);
    }

    public string BuildSitemapApiUrl(string sitemapAlias, string? hostname)
    {
        // TODO: Get api route from elsewhere...
        var apiRoute = "api/sitemap";
        var relativeUrl = $"/{apiRoute}?name={Uri.EscapeDataString(sitemapAlias)}";
        return CombineWithHostname(relativeUrl, hostname);
    }

    public string BuildLegacySitemapFileUrl(string sitemapAlias, string? hostname)
    {
        return CombineWithHostname($"/{sitemapAlias}.xml", hostname);
    }

    public string CombineWithHostname(string relativeUrl, string? hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname))
        {
            return relativeUrl;
        }

        var lastIndexOfSlash = hostname.LastIndexOf('/');
        if ((hostname.StartsWith("http") && lastIndexOfSlash > 7) || (!hostname.StartsWith("http") && lastIndexOfSlash > 0))
        {
            hostname = hostname[..lastIndexOfSlash];
        }

        return $"{hostname.TrimEnd('/')}{relativeUrl}";
    }
}