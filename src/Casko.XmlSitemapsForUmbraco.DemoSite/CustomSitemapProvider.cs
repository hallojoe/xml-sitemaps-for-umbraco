using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Models.Enums;
using Casko.XmlSitemapsForUmbraco.Providers;

namespace Casko.XmlSitemapsForUmbraco.DemoSite;

public sealed class CustomSitemapProvider : IXmlSitemapCustomProvider
{
    public string Alias => "custom-sitemap-provider";

    public Task<XmlSitemap> GetSitemapAsync(
        XmlSitemapCustomProviderContext context,
        CancellationToken cancellationToken = default)
    {
        var lastModified = DateTime.UtcNow;
        var sitemap = new XmlSitemap
        {
            Urls =
            [
                CreateUrl($"{context.HostName}/dummy/one", lastModified, ChangeFrequency.Weekly, 0.8),
                CreateUrl($"{context.HostName}/dummy/two", lastModified.AddDays(-7), ChangeFrequency.Monthly, 0.6),
                CreateUrl($"{context.HostName}/dummy/three", lastModified.AddDays(-14), ChangeFrequency.Yearly, 0.4)
            ]
        };

        return Task.FromResult(sitemap);
    }

    private static XmlSitemapUrl CreateUrl(
        string location,
        DateTime lastModified,
        ChangeFrequency changeFrequency,
        double priority)
    {
        return new XmlSitemapUrl
        {
            Location = location,
            LastModified = lastModified,
            ChangeFrequency = changeFrequency,
            Priority = priority
        };
    }
}
