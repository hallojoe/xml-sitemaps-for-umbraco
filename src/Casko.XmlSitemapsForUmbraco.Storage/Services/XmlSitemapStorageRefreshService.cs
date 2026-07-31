using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Serialization;
using Microsoft.Extensions.Options;

namespace Casko.XmlSitemapsForUmbraco.Storage.Services;

public sealed class XmlSitemapStorageRefreshService(
    IXmlSitemapSourceProvider sourceProvider,
    IXmlSitemapDataSource xmlSitemapDataSource,
    IXmlSitemapXmlSerializer xmlSitemapXmlSerializer,
    IOptions<XmlSitemapsOptions> xmlSitemapOptions) : IXmlSitemapStorageRefreshService
{
    /// <inheritdoc />
    public async Task RefreshAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var key in xmlSitemapOptions.Value.Sitemaps.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RefreshConfiguredAsync(key, cancellationToken);
        }

        foreach (var key in xmlSitemapOptions.Value.CustomSitemaps.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RefreshCustomAsync(key, cancellationToken);
        }

        foreach (var key in xmlSitemapOptions.Value.Indexes.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RefreshIndexAsync(key, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<IXmlSitemapModel> RefreshConfiguredAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!xmlSitemapOptions.Value.Sitemaps.TryGetValue(key, out var sitemapOptions))
        {
            throw new InvalidOperationException("Invalid key.");
        }

        var xmlSiteMap = await sourceProvider.GetConfiguredAsync(key);
        if (xmlSiteMap is XmlSitemap sitemap)
        {
            var storageKey = new XmlSitemapStorageKey(
                XmlSitemapDocumentKind.Sitemap,
                key,
                sitemapOptions.HostName);
            var xml = xmlSitemapXmlSerializer.Serialize(sitemap);
            await xmlSitemapDataSource.WriteAsync(storageKey, xml, cancellationToken);
        }

        return xmlSiteMap;
    }

    /// <inheritdoc />
    public async Task<IXmlSitemapModel> RefreshCustomAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!xmlSitemapOptions.Value.CustomSitemaps.TryGetValue(key, out var sitemapOptions))
        {
            throw new InvalidOperationException("Invalid key.");
        }

        var xmlSiteMap = await sourceProvider.GetConfiguredAsync(key);
        if (xmlSiteMap is XmlSitemap sitemap)
        {
            var storageKey = new XmlSitemapStorageKey(
                XmlSitemapDocumentKind.Sitemap,
                key,
                sitemapOptions.HostName);
            var xml = xmlSitemapXmlSerializer.Serialize(sitemap);
            await xmlSitemapDataSource.WriteAsync(storageKey, xml, cancellationToken);
        }

        return xmlSiteMap;
    }

    /// <inheritdoc />
    public async Task<IXmlSitemapModel> RefreshIndexAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!xmlSitemapOptions.Value.Indexes.TryGetValue(key, out var sitemapIndexOptions))
        {
            throw new InvalidOperationException("Invalid key.");
        }

        var xmlSiteMap = await sourceProvider.GetIndexAsync(key);
        if (xmlSiteMap is XmlSitemapIndex sitemapIndex)
        {
            var storageKey = new XmlSitemapStorageKey(
                XmlSitemapDocumentKind.SitemapIndex,
                key,
                sitemapIndexOptions.HostName);
            var xml = xmlSitemapXmlSerializer.Serialize(sitemapIndex);
            await xmlSitemapDataSource.WriteAsync(storageKey, xml, cancellationToken);
        }

        return xmlSiteMap;
    }
}
