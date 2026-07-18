using System.ComponentModel.DataAnnotations;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Common.Providers.Examine.Urls;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Indexes;
using Microsoft.Extensions.Options;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine;

public sealed class ExamineXmlSitemapProvider(
    IOptions<XmlSitemapsOptions> xmlSitemapOptions,
    IPublishedContentService publishedContentService,
    ICmsUrlService cmsUrlService,
    IExamineXmlSitemapRenderer sitemapRenderer,
    IXmlSitemapIndexRenderer sitemapIndexRenderer,
    IEnumerable<IXmlSitemapCustomProvider> customProviders) : IXmlSitemapSourceProvider
{
    /// <inheritdoc />
    public IXmlSiteMapModel GetByRootKey(Guid rootKey)
    {
        return GetByRootKeyAsync(rootKey).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<IXmlSiteMapModel> GetByRootKeyAsync(Guid rootKey)
    {
        return await RenderXmlSiteMapAsync(rootKey, hostname: null, culture: null, sitemapOptions: null);
    }

    /// <inheritdoc />
    public IXmlSiteMapModel GetByPath(string path, string? culture = null, string? hostname = null)
    {
        return GetByPathAsync(path, culture, hostname).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<IXmlSiteMapModel> GetByPathAsync(string path, string? culture = null, string? hostname = null)
    {
        var rootContent = publishedContentService.GetContentByPath(path, hostname, culture);
        if (rootContent is null)
        {
            throw new RootContentNotFoundException();
        }

        return await RenderXmlSiteMapAsync(rootContent.Key, hostname, culture, sitemapOptions: null);
    }

    /// <inheritdoc />
    public IXmlSiteMapModel GetConfigured(string key)
    {
        return GetConfiguredAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<IXmlSiteMapModel> GetConfiguredAsync(string key)
    {
        var configuredSitemaps = xmlSitemapOptions.Value;
        if (!configuredSitemaps.Sitemaps.TryGetValue(key, out var sitemapOptions))
        {
            return await GetCustomConfiguredAsync(key);
        }

        var rootContent = publishedContentService.GetContentByPath(
            sitemapOptions.Path ?? "/",
            sitemapOptions.HostName,
            sitemapOptions.Culture);

        if (rootContent is null)
        {
            throw new RootContentNotFoundException();
        }

        return await RenderXmlSiteMapAsync(rootContent.Key, sitemapOptions.HostName, sitemapOptions.Culture, sitemapOptions);
    }

    /// <inheritdoc />
    public IXmlSiteMapModel GetIndex(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ValidationException("A sitemap index key is required.");
        }

        var configuredSitemaps = xmlSitemapOptions.Value;
        if (!configuredSitemaps.Indexes.TryGetValue(key, out var sitemapIndexOptions))
        {
            throw new InvalidOperationException("Invalid key.");
        }

        return sitemapIndexRenderer.Render(new XmlSitemapIndexRenderContext(
            sitemapIndexOptions.Sitemaps,
            sitemapIndexOptions.HostName,
            XmlSitemapIndexLocationMode.LegacyXmlFile,
            ResolvePublicSitemapAliases(sitemapIndexOptions.Sitemaps, configuredSitemaps)));
    }

    /// <inheritdoc />
    public Task<IXmlSiteMapModel> GetIndexAsync(string key)
    {
        return Task.FromResult(GetIndex(key));
    }

    private async Task<XmlSiteMap> GetCustomConfiguredAsync(string sitemapKey)
    {
        var configuredSitemaps = xmlSitemapOptions.Value;
        if (!configuredSitemaps.CustomSitemaps.TryGetValue(sitemapKey, out var sitemapOptions))
        {
            throw new InvalidOperationException("Invalid key.");
        }

        if (string.IsNullOrWhiteSpace(sitemapOptions.ProviderAlias))
        {
            throw new InvalidOperationException("A custom sitemap provider alias is required.");
        }

        var provider = customProviders.FirstOrDefault(candidate =>
            string.Equals(candidate.Alias, sitemapOptions.ProviderAlias, StringComparison.OrdinalIgnoreCase));
        if (provider is null)
        {
            throw new InvalidOperationException($"Custom sitemap provider '{sitemapOptions.ProviderAlias}' was not found.");
        }

        return await provider.GetSitemapAsync(new XmlSitemapCustomProviderContext(
            sitemapKey,
            sitemapOptions.HostName,
            sitemapOptions.Settings));
    }

    private async Task<XmlSiteMap> RenderXmlSiteMapAsync(
        Guid rootKey,
        string? hostname,
        string? culture,
        SitemapOptions? sitemapOptions)
    {
        var urls = (await cmsUrlService.GetUrlsByKeyAsync(rootKey)).ToList();
        var allLanguageCodes = urls
            .Select(url => url.Culture)
            .Where(cultureCode => string.IsNullOrWhiteSpace(cultureCode) is false)
            .Select(cultureCode => cultureCode!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var defaultLanguageCode = culture ?? allLanguageCodes.FirstOrDefault() ?? "en";
        var cultureSelection = SitemapCultureSelection.Resolve(
            allLanguageCodes,
            xmlSitemapOptions.Value,
            sitemapOptions);

        return sitemapRenderer.Render(new ExamineXmlSitemapRenderContext(
            urls,
            defaultLanguageCode,
            cultureSelection.Cultures,
            hostname,
            cultureSelection.RenderAlternateLinks));
    }

    private static IReadOnlyDictionary<string, string> ResolvePublicSitemapAliases(
        IEnumerable<string> sitemapKeys,
        XmlSitemapsOptions configuredSitemaps)
    {
        return sitemapKeys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                key => key,
                key => ResolvePublicSitemapAlias(key, configuredSitemaps),
                StringComparer.OrdinalIgnoreCase);
    }

    private static string ResolvePublicSitemapAlias(string key, XmlSitemapsOptions configuredSitemaps)
    {
        if (configuredSitemaps.Sitemaps.TryGetValue(key, out var sitemapOptions))
        {
            return SitemapPublicName.Resolve(key, sitemapOptions.PublicName);
        }

        if (configuredSitemaps.CustomSitemaps.TryGetValue(key, out var customSitemapOptions))
        {
            return SitemapPublicName.Resolve(key, customSitemapOptions.PublicName);
        }

        return key;
    }
}
