using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Providers;

/// <summary>
/// Provides XML sitemap and XML sitemap index models.
/// </summary>
public interface IXmlSitemapProvider
{
    /// <summary>
    /// Gets an XML sitemap by its root content key. 
    /// </summary>
    public IXmlSitemapModel GetByRootKey(Guid rootKey);

    /// <summary>
    /// Gets an XML sitemap by its root content key.
    /// </summary>
    public Task<IXmlSitemapModel> GetByRootKeyAsync(Guid rootKey);

    /// <summary>
    /// Gets an XML sitemap by resolving a content path, optionally scoped by culture and host name.
    /// </summary>
    public IXmlSitemapModel GetByPath(string path, string? culture = null, string? hostname = null);

    /// <summary>
    /// Gets an XML sitemap by resolving a content path, optionally scoped by culture and host name.
    /// </summary>
    public Task<IXmlSitemapModel> GetByPathAsync(string path, string? culture = null, string? hostname = null);

    /// <summary>
    /// Gets a configured XML sitemap by its configuration key.
    /// </summary>
    public IXmlSitemapModel GetConfigured(string key);

    /// <summary>
    /// Gets a configured XML sitemap by its configuration key.
    /// </summary>
    public Task<IXmlSitemapModel> GetConfiguredAsync(string key);

    /// <summary>
    /// Gets a configured XML sitemap index by its configuration key.
    /// </summary>
    public IXmlSitemapModel GetIndex(string key);

    /// <summary>
    /// Gets a configured XML sitemap index by its configuration key.
    /// </summary>
    public Task<IXmlSitemapModel> GetIndexAsync(string key);

}
