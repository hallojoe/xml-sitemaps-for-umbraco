using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public sealed class PublishedContentUrlBuilder(IXmlSitemapUrlBuilder xmlSitemapUrlBuilder) : IPublishedContentUrlBuilder
{
    public PublishedContentUrlBuilder()
        : this(new XmlSitemapUrlBuilder())
    {
    }

    public string BuildContentUrl(IPublishedContent content, string languageCode, string? hostname)
    {
        if (!string.IsNullOrWhiteSpace(hostname) && !hostname.StartsWith("https://"))
        {
            hostname = "https://" + hostname.Trim('/');
        }

        return xmlSitemapUrlBuilder.CombineWithHostname(
            content.Url(mode: UrlMode.Relative, culture: languageCode),
            hostname);
    }

    public string BuildSitemapApiUrl(string sitemapAlias, string? hostname) =>
        xmlSitemapUrlBuilder.BuildSitemapApiUrl(sitemapAlias, hostname);

    public string BuildLegacySitemapFileUrl(string sitemapAlias, string? hostname) =>
        xmlSitemapUrlBuilder.BuildLegacySitemapFileUrl(sitemapAlias, hostname);

    public string CombineWithHostname(string relativeUrl, string? hostname) =>
        xmlSitemapUrlBuilder.CombineWithHostname(relativeUrl, hostname);
}
