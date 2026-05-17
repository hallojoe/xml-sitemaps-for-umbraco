using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Storage;

/// <summary>
/// Rebuilds configured XML sitemap documents and stores them in the backing data source.
/// </summary>
public interface IXmlSitemapStorageRefreshService
{
    /// <summary>
    /// Rebuilds and stores all configured XML sitemaps, then all configured XML sitemap indexes.
    /// </summary>
    public Task RefreshAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuilds and stores a configured XML sitemap.
    /// </summary>
    public Task<IXmlSiteMapModel> RefreshConfiguredAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuilds and stores a configured custom XML sitemap.
    /// </summary>
    public Task<IXmlSiteMapModel> RefreshCustomAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuilds and stores a configured XML sitemap index.
    /// </summary>
    public Task<IXmlSiteMapModel> RefreshIndexAsync(string key, CancellationToken cancellationToken = default);
}
