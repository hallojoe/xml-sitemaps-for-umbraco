using Casko.XmlSitemapsForUmbraco.Common.Services;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Models.Enums;

namespace Casko.XmlSitemapsForUmbraco.TestSite;

public sealed class DummyCustomSitemapProvider : IXmlSitemapCustomProvider
{
    public string Alias => "dummy-custom-sitemap-provider";

    public Task<XmlSiteMap> GetSitemapAsync(
        XmlSitemapCustomProviderContext context,
        CancellationToken cancellationToken = default)
    {
        var lastModified = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var sitemap = new XmlSiteMap
        {
            Urls =
            [
                CreateUrl("https://example.com/dummy/one", lastModified, ChangeFrequency.Weekly, 0.8),
                CreateUrl("https://example.com/dummy/two", lastModified.AddDays(-7), ChangeFrequency.Monthly, 0.6),
                CreateUrl("https://example.com/dummy/three", lastModified.AddDays(-14), ChangeFrequency.Yearly, 0.4)
            ]
        };

        return Task.FromResult(sitemap);
    }

    private static XmlSiteMapUrl CreateUrl(
        string location,
        DateTime lastModified,
        ChangeFrequency changeFrequency,
        double priority)
    {
        return new XmlSiteMapUrl
        {
            Location = location,
            LastModified = lastModified,
            ChangeFrequency = changeFrequency,
            Priority = priority
        };
    }
}
