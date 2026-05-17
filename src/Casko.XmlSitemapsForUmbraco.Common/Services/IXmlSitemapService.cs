using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Common.Services;

/// <summary>
/// Provides XML sitemap and XML sitemap index models.
/// </summary>
public interface IXmlSitemapService
{
    /// <summary>
    /// Gets an XML sitemap by its root content key.
    /// </summary>
    public IXmlSiteMapModel GetByRootKey(Guid rootKey);

    /// <summary>
    /// Gets an XML sitemap by its root content key.
    /// </summary>
    public Task<IXmlSiteMapModel> GetByRootKeyAsync(Guid rootKey);

    /// <summary>
    /// Gets an XML sitemap by resolving a content path, optionally scoped by culture and host name.
    /// </summary>
    public IXmlSiteMapModel GetByPath(string path, string? culture = null, string? hostname = null);

    /// <summary>
    /// Gets an XML sitemap by resolving a content path, optionally scoped by culture and host name.
    /// </summary>
    public Task<IXmlSiteMapModel> GetByPathAsync(string path, string? culture = null, string? hostname = null);

    /// <summary>
    /// Gets a configured XML sitemap by its configuration key.
    /// </summary>
    public IXmlSiteMapModel GetConfigured(string key);

    /// <summary>
    /// Gets a configured XML sitemap by its configuration key.
    /// </summary>
    public Task<IXmlSiteMapModel> GetConfiguredAsync(string key);

    /// <summary>
    /// Gets a configured XML sitemap index by its configuration key.
    /// </summary>
    public IXmlSiteMapModel GetIndex(string key);

    /// <summary>
    /// Gets a configured XML sitemap index by its configuration key.
    /// </summary>
    public Task<IXmlSiteMapModel> GetIndexAsync(string key);

}
