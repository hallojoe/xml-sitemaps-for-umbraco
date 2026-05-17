using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Services;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Models.Serialization;
using Microsoft.Extensions.Options;

namespace Casko.XmlSitemapsForUmbraco.Storage.Services;

public sealed class StoredXmlSitemapService(
    DefaultXmlSiteMapService defaultXmlSiteMapService,
    IXmlSitemapDataSource xmlSitemapDataSource,
    IXmlSitemapXmlDeserializer xmlSitemapXmlDeserializer,
    IXmlSitemapStorageRefreshService xmlSitemapStorageRefreshService,
    IOptions<XmlSitemapsOptions> xmlSitemapOptions,
    TimeProvider timeProvider) : IXmlSitemapService
{
    /// <inheritdoc />
    public IXmlSiteMapModel GetByRootKey(Guid rootKey)
    {
        return defaultXmlSiteMapService.GetByRootKey(rootKey);
    }

    /// <inheritdoc />
    public Task<IXmlSiteMapModel> GetByRootKeyAsync(Guid rootKey)
    {
        return defaultXmlSiteMapService.GetByRootKeyAsync(rootKey);
    }

    /// <inheritdoc />
    public IXmlSiteMapModel GetByPath(string path, string? culture = null, string? hostname = null)
    {
        return defaultXmlSiteMapService.GetByPath(path, culture, hostname);
    }

    /// <inheritdoc />
    public Task<IXmlSiteMapModel> GetByPathAsync(string path, string? culture = null, string? hostname = null)
    {
        return defaultXmlSiteMapService.GetByPathAsync(path, culture, hostname);
    }

    /// <inheritdoc />
    public IXmlSiteMapModel GetConfigured(string key)
    {
        return GetConfiguredAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<IXmlSiteMapModel> GetConfiguredAsync(string key)
    {
        if (xmlSitemapOptions.Value.Sitemaps.TryGetValue(key, out var sitemapOptions))
        {
            return await GetStoredSitemapAsync(
                key,
                sitemapOptions.HostName,
                () => xmlSitemapStorageRefreshService.RefreshConfiguredAsync(key));
        }

        if (xmlSitemapOptions.Value.CustomSitemaps.TryGetValue(key, out var customSitemapOptions))
        {
            return await GetStoredSitemapAsync(
                key,
                customSitemapOptions.HostName,
                () => xmlSitemapStorageRefreshService.RefreshCustomAsync(key));
        }

        throw new InvalidOperationException("Invalid key.");
    }

    private async Task<IXmlSiteMapModel> GetStoredSitemapAsync(
        string key,
        string? hostName,
        Func<Task<IXmlSiteMapModel>> refresh)
    {
        var storageKey = new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.Sitemap,
            key,
            hostName);

        var storedDocument = await xmlSitemapDataSource.ReadAsync(storageKey);
        if (storedDocument is not null && !IsStale(storedDocument))
        {
            return xmlSitemapXmlDeserializer.Deserialize<XmlSiteMap>(storedDocument.Xml);
        }

        return await refresh();
    }

    /// <inheritdoc />
    public IXmlSiteMapModel GetIndex(string key)
    {
        return GetIndexAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<IXmlSiteMapModel> GetIndexAsync(string key)
    {
        if (!xmlSitemapOptions.Value.Indexes.TryGetValue(key, out var sitemapIndexOptions))
        {
            throw new InvalidOperationException("Invalid key.");
        }

        var storageKey = new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.SitemapIndex,
            key,
            sitemapIndexOptions.HostName);

        var storedDocument = await xmlSitemapDataSource.ReadAsync(storageKey);
        if (storedDocument is not null && !IsStale(storedDocument))
        {
            return xmlSitemapXmlDeserializer.Deserialize<XmlSiteMapIndex>(storedDocument.Xml);
        }

        return await xmlSitemapStorageRefreshService.RefreshIndexAsync(key);
    }

    private bool IsStale(XmlSitemapStoredDocument storedDocument)
    {
        var staleAfterSeconds = xmlSitemapOptions.Value.Storage.RefreshStaleAfterSeconds;
        if (staleAfterSeconds <= 0)
        {
            return false;
        }

        if (storedDocument.RefreshedUtc is null)
        {
            return true;
        }

        return storedDocument.RefreshedUtc.Value.AddSeconds(staleAfterSeconds) <= timeProvider.GetUtcNow();
    }
}
