using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Providers;

/// <summary>
/// Builds XML sitemaps from a custom user implementation.
/// </summary>
public interface IXmlSitemapCustomProvider
{
    /// <summary>
    /// Gets the provider alias used by configuration.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Gets a custom XML sitemap.
    /// </summary>
    public Task<XmlSiteMap> GetSitemapAsync(
        XmlSitemapCustomProviderContext context,
        CancellationToken cancellationToken = default);
}
