using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Serialization;
using Microsoft.Extensions.Options;
using CommonXmlSitemapApiConstants = Casko.XmlSitemapsForUmbraco.Common.XmlSitemapApiConstants;

namespace Casko.XmlSitemapsForUmbraco.Storage.Services;

public sealed class StoredXmlSitemapProvider(
    IXmlSitemapSourceProvider sourceProvider,
    IXmlSitemapDataSource xmlSitemapDataSource,
    IXmlSitemapXmlDeserializer xmlSitemapXmlDeserializer,
    IXmlSitemapStorageRefreshService xmlSitemapStorageRefreshService,
    IOptions<XmlSitemapsOptions> xmlSitemapOptions,
    TimeProvider timeProvider) : IXmlSitemapProvider
{
    /// <inheritdoc />
    public IXmlSitemapModel GetByRootKey(Guid rootKey)
    {
        return sourceProvider.GetByRootKey(rootKey);
    }

    /// <inheritdoc />
    public Task<IXmlSitemapModel> GetByRootKeyAsync(Guid rootKey)
    {
        return sourceProvider.GetByRootKeyAsync(rootKey);
    }

    /// <inheritdoc />
    public IXmlSitemapModel GetByPath(string path, string? culture = null, string? hostname = null)
    {
        return sourceProvider.GetByPath(path, culture, hostname);
    }

    /// <inheritdoc />
    public Task<IXmlSitemapModel> GetByPathAsync(string path, string? culture = null, string? hostname = null)
    {
        return sourceProvider.GetByPathAsync(path, culture, hostname);
    }

    /// <inheritdoc />
    public IXmlSitemapModel GetConfigured(string key)
    {
        return GetConfiguredAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<IXmlSitemapModel> GetConfiguredAsync(string key)
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

        if (IsImplicitSingleSitemapKey(key))
        {
            return await GetStoredSitemapAsync(
                key,
                hostName: null,
                () => xmlSitemapStorageRefreshService.RefreshConfiguredAsync(key));
        }

        throw new InvalidOperationException("Invalid key.");
    }

    private async Task<IXmlSitemapModel> GetStoredSitemapAsync(
        string key,
        string? hostName,
        Func<Task<IXmlSitemapModel>> refresh)
    {
        var storageKey = new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.Sitemap,
            key,
            hostName);

        var storedDocument = await xmlSitemapDataSource.ReadAsync(storageKey);
        if (storedDocument is not null && !IsStale(storedDocument))
        {
            return xmlSitemapXmlDeserializer.Deserialize<XmlSitemap>(storedDocument.Xml);
        }

        return await refresh();
    }

    /// <inheritdoc />
    public IXmlSitemapModel GetIndex(string key)
    {
        return GetIndexAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<IXmlSitemapModel> GetIndexAsync(string key)
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
            return xmlSitemapXmlDeserializer.Deserialize<XmlSitemapIndex>(storedDocument.Xml);
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

    private bool IsImplicitSingleSitemapKey(string key)
    {
        return xmlSitemapOptions.Value.Mode == XmlSitemapsMode.Single &&
               string.Equals(key, CommonXmlSitemapApiConstants.DefaultSitemapKey, StringComparison.OrdinalIgnoreCase);
    }
}
