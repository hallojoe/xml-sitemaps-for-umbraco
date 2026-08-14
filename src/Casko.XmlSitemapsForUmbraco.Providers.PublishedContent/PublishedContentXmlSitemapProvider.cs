using System.ComponentModel.DataAnnotations;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;
using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using CommonXmlSitemapApiConstants = Casko.XmlSitemapsForUmbraco.Common.XmlSitemapApiConstants;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent;

// [Obsolete("Use the Examine XmlSitemapProvider instead.", true)]
public class PublishedContentXmlSitemapProvider(
    IOptions<XmlSitemapsOptions> legacySitemapOptions,
    IPublishedContentService publishedContentService,
    IPublishedContentRenderer sitemapRenderer,
    IPublishedContentIndexRenderer sitemapIndexRenderer,
    IHostUrlProvider hostUrlProvider,
    IUmbracoContextFactory umbracoContextFactory,
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

        using var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
        
        var publishedContentCache = umbracoContextReference.UmbracoContext.Content;
        
        var rootContent = publishedContentService.GetContent(rootKey, publishedContentCache);
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
            if (IsImplicitSingleSitemapKey(configuredSitemaps, sitemapKey))
            {
                return await GetXmlSiteMapAsync("/", hostname: null, culture: null, sitemapOptions: null);
            }

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
        var hostUrls = await hostUrlProvider.GetHostUrlsAsync();
        
        var hostUrl = hostname is null 
            ? hostUrls?.FirstOrDefault(hostUrl => hostUrl.IsDefaultCulture) 
            : hostUrls.FirstOrDefault(hostUrl => hostUrl.Uri.ToString().Contains(hostname.Replace("http://", "").Replace("https://", "")));

        if (hostUrl is null)
        {
            throw new RootContentNotFoundException();
        }

        using var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();

        var rootContent = await umbracoContextReference.UmbracoContext.Content.GetByIdAsync(hostUrl.Key);
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

        var hostUrl = await ResolveHostUrlAsync(rootContent.Key, culture);

        var defaultLanguageCode = culture ?? hostUrl?.Culture ?? allLanguageCodes.FirstOrDefault() ?? string.Empty;
        
        var availableCultures = ResolveAvailableCultures(allLanguageCodes, culture, hostUrl?.Culture);

        var cultureSelection = SitemapCultureSelection.Resolve(
            availableCultures,
            rootOptions,
            sitemapOptions);

        var contentTypeSelection = SitemapContentTypeSelection.Resolve(rootOptions, sitemapOptions);
        var propertyExclusionSelection = SitemapPropertyExclusionSelection.Resolve(rootOptions);
        var resolvedHostname = string.IsNullOrWhiteSpace(hostname)
            ? ResolveHostname(hostUrl)
            : hostname;

        return sitemapRenderer.Render(new PublishedContentRenderContext(
            [rootContent],
            defaultLanguageCode,
            cultureSelection.Cultures,
            resolvedHostname,
            cultureSelection.RenderAlternateLinks,
            content => contentTypeSelection.ShouldInclude(content) && propertyExclusionSelection.ShouldInclude(content, defaultLanguageCode)));
    }

    private async Task<HostUrl?> ResolveHostUrlAsync(Guid rootKey, string? culture)
    {
        var hostUrls = (await hostUrlProvider.GetHostUrlsAsync())
            .Where(hostUrl => hostUrl.Key == rootKey)
            .ToList();

        if (hostUrls.Count == 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(culture) is false)
        {
            var cultureHostUrl = hostUrls.FirstOrDefault(hostUrl =>
                string.Equals(hostUrl.Culture, culture, StringComparison.OrdinalIgnoreCase));
            if (cultureHostUrl is not null)
            {
                return cultureHostUrl;
            }
        }

        return hostUrls.FirstOrDefault(hostUrl => hostUrl.IsDefaultCulture) ?? hostUrls.First();
    }

    private static string? ResolveHostname(HostUrl? hostUrl)
    {
        return hostUrl?.Uri.ToString().TrimEnd('/');
    }

    private static IReadOnlyCollection<string> ResolveAvailableCultures(
        IEnumerable<string> languageCodes,
        string? culture,
        string? hostCulture)
    {
        return languageCodes
            .Concat([culture, hostCulture])
            .Where(languageCode => string.IsNullOrWhiteSpace(languageCode) is false)
            .Select(languageCode => languageCode!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsImplicitSingleSitemapKey(XmlSitemapsOptions options, string sitemapKey)
    {
        return options.Mode == XmlSitemapsMode.Single &&
               string.Equals(sitemapKey, CommonXmlSitemapApiConstants.DefaultSitemapKey, StringComparison.OrdinalIgnoreCase);
    }
}
