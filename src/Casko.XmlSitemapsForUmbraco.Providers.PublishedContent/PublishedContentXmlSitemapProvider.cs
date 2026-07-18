using System.ComponentModel.DataAnnotations;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent;

public class PublishedContentXmlSitemapProvider(
    IOptions<XmlSitemapsOptions> legacySitemapOptions,
    IPublishedContentService publishedContentService,
    IPublishedContentRenderer sitemapRenderer,
    IPublishedContentIndexRenderer sitemapIndexRenderer,
    IPublishedUrlProvider publishedUrlProvider,
    IEnumerable<IXmlSitemapCustomProvider> customProviders) : IXmlSitemapSourceProvider
{
    /// <inheritdoc />
    public virtual IXmlSitemapModel GetIndex(string sitemapIndexKey)
    {
        if (string.IsNullOrWhiteSpace(sitemapIndexKey))
        {
            throw new ValidationException("A sitemap index key is required.");
        }

        var configuredSitemaps = legacySitemapOptions.Value;

        if (!configuredSitemaps.Indexes.TryGetValue(sitemapIndexKey, out var sitemapIndexOptions))
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
    public virtual Task<IXmlSitemapModel> GetIndexAsync(string key)
    {
        return Task.FromResult(GetIndex(key));
    }

    /// <inheritdoc />
    public virtual IXmlSitemapModel GetByRootKey(Guid rootKey)
    {
        return GetByRootKeyAsync(rootKey).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public virtual async Task<IXmlSitemapModel> GetByRootKeyAsync(Guid rootKey)
    {
        var rootContent = publishedContentService.GetContent(rootKey);
        if (rootContent is null)
        {
            throw new RootContentNotFoundException();
        }

        return await RenderXmlSiteMapAsync(rootContent, hostname: null, culture: null, sitemapOptions: null);
    }

    /// <inheritdoc />
    public virtual IXmlSitemapModel GetByPath(string path, string? culture = null, string? hostname = null)
    {
        return GetByPathAsync(path, culture, hostname).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public virtual async Task<IXmlSitemapModel> GetByPathAsync(string path, string? culture = null, string? hostname = null)
    {
        return await GetXmlSiteMapAsync(path, hostname, culture, sitemapOptions: null);
    }

    /// <inheritdoc />
    public virtual IXmlSitemapModel GetConfigured(string sitemapKey)
    {
        return GetConfiguredAsync(sitemapKey).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public virtual async Task<IXmlSitemapModel> GetConfiguredAsync(string sitemapKey)
    {
        var configuredSitemaps = legacySitemapOptions.Value;

        if (!configuredSitemaps.Sitemaps.TryGetValue(sitemapKey, out var sitemapOptions))
        {
            return await GetCustomConfiguredAsync(sitemapKey);
        }

        return await GetXmlSiteMapAsync(sitemapOptions.Path ?? "/", sitemapOptions.HostName, sitemapOptions.Culture, sitemapOptions);
    }

    private async Task<XmlSitemap> GetCustomConfiguredAsync(string sitemapKey)
    {
        var configuredSitemaps = legacySitemapOptions.Value;
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

    private async Task<XmlSitemap> GetXmlSiteMapAsync(
        string path,
        string? hostname,
        string? culture,
        SitemapOptions? sitemapOptions)
    {
        var rootContent = publishedContentService.GetContentByPath(path, hostname, culture);
        if (rootContent is null)
        {
            throw new RootContentNotFoundException();
        }

        return await RenderXmlSiteMapAsync(rootContent, hostname, culture, sitemapOptions);
    }

    private async Task<XmlSitemap> RenderXmlSiteMapAsync(
        IPublishedContent rootContent,
        string? hostname,
        string? culture,
        SitemapOptions? sitemapOptions)
    {
        var rootOptions = legacySitemapOptions.Value;

        var allLanguageCodes = await publishedContentService.GetLanguagesAsync();

        var defaultLanguageCode = culture ?? allLanguageCodes.FirstOrDefault() ?? "en";

        var cultureSelection = SitemapCultureSelection.Resolve(
            allLanguageCodes,
            rootOptions,
            sitemapOptions);

        var contentTypeSelection = SitemapContentTypeSelection.Resolve(rootOptions, sitemapOptions);
        var propertyExclusionSelection = SitemapPropertyExclusionSelection.Resolve(rootOptions);

        if (string.IsNullOrWhiteSpace(hostname))
        {
            hostname = ResolveHostname(rootContent, culture);
        }

        return sitemapRenderer.Render(new PublishedContentRenderContext(
            [rootContent],
            defaultLanguageCode,
            cultureSelection.Cultures,
            hostname,
            cultureSelection.RenderAlternateLinks,
            content => contentTypeSelection.ShouldInclude(content) &&
                       propertyExclusionSelection.ShouldInclude(content, defaultLanguageCode)));
    }

    private string? ResolveHostname(IPublishedContent rootContent, string? culture)
    {
        var absoluteUrl = publishedUrlProvider.GetUrl(rootContent, UrlMode.Absolute, culture, current: null);
        if (string.IsNullOrWhiteSpace(absoluteUrl) || absoluteUrl == "#")
        {
            return null;
        }

        if (Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.Authority;
        }

        var schemeSeparatorIndex = absoluteUrl.IndexOf("://", StringComparison.Ordinal);
        if (schemeSeparatorIndex >= 0)
        {
            absoluteUrl = absoluteUrl[(schemeSeparatorIndex + 3)..];
        }

        return absoluteUrl.Split('/').FirstOrDefault();
    }
}
