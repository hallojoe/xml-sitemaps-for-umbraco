using System.ComponentModel.DataAnnotations;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Common.Services.Cms;
using Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;
using Casko.XmlSitemapsForUmbraco.Models;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Common.Services;

public class DefaultXmlSiteMapService(
    IOptions<XmlSitemapsOptions> legacySitemapOptions,
    ICmsContentService cmsContentService,
    IXmlSitemapRenderer sitemapRenderer,
    IXmlSitemapIndexRenderer sitemapIndexRenderer,
    IEnumerable<IXmlSitemapCustomProvider> customProviders) : IXmlSitemapService
{
    /// <inheritdoc />
    public virtual IXmlSiteMapModel GetIndex(string sitemapIndexKey)
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
            XmlSitemapIndexLocationMode.LegacyXmlFile));
    }

    /// <inheritdoc />
    public virtual Task<IXmlSiteMapModel> GetIndexAsync(string key)
    {
        return Task.FromResult(GetIndex(key));
    }

    /// <inheritdoc />
    public virtual IXmlSiteMapModel GetByRootKey(Guid rootKey)
    {
        return GetByRootKeyAsync(rootKey).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public virtual Task<IXmlSiteMapModel> GetByRootKeyAsync(Guid rootKey)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public virtual IXmlSiteMapModel GetByPath(string path, string? culture = null, string? hostname = null)
    {
        return GetByPathAsync(path, culture, hostname).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public virtual async Task<IXmlSiteMapModel> GetByPathAsync(string path, string? culture = null, string? hostname = null)
    {
        return await GetXmlSiteMapAsync(path, hostname, culture, sitemapOptions: null);
    }

    /// <inheritdoc />
    public virtual IXmlSiteMapModel GetConfigured(string sitemapKey)
    {
        return GetConfiguredAsync(sitemapKey).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public virtual async Task<IXmlSiteMapModel> GetConfiguredAsync(string sitemapKey)
    {
        var configuredSitemaps = legacySitemapOptions.Value;

        if (!configuredSitemaps.Sitemaps.TryGetValue(sitemapKey, out var sitemapOptions))
        {
            return await GetCustomConfiguredAsync(sitemapKey);
        }

        return await GetXmlSiteMapAsync(sitemapOptions.Path ?? "/", sitemapOptions.HostName, sitemapOptions.Culture, sitemapOptions);
    }

    private async Task<XmlSiteMap> GetCustomConfiguredAsync(string sitemapKey)
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

    private async Task<XmlSiteMap> GetXmlSiteMapAsync(
        string path,
        string? hostname,
        string? culture,
        SitemapOptions? sitemapOptions)
    {
        var rootOptions = legacySitemapOptions.Value;

        var allLanguageCodes = await cmsContentService.GetLanguagesAsync();

        var defaultLanguageCode = culture ?? allLanguageCodes.FirstOrDefault() ?? "en";

        var cultureSelection = SitemapCultureSelection.Resolve(
            allLanguageCodes,
            rootOptions,
            sitemapOptions);

        var contentTypeSelection = SitemapContentTypeSelection.Resolve(rootOptions, sitemapOptions);

        var rootContent = cmsContentService.GetContentByPath(path, hostname, culture);
        if (rootContent is null)
        {
            throw new RootContentNotFoundException();
        }

        if (string.IsNullOrWhiteSpace(hostname))
        {
            hostname = rootContent.Url(mode: UrlMode.Absolute).Replace("https://", "").Split('/').First();
        }

        return sitemapRenderer.Render(new XmlSitemapRenderContext(
            [rootContent],
            defaultLanguageCode,
            cultureSelection.Cultures,
            hostname,
            cultureSelection.RenderAlternateLinks,
            contentTypeSelection.ShouldInclude));
    }


}
