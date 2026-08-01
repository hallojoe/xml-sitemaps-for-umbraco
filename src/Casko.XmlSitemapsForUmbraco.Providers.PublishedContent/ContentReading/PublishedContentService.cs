using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;

public class PublishedContentService(
    IOptions<XmlSitemapsOptions> xmlSitemapsOptions,
    IUmbracoContextFactory umbracoContextFactory,
    IDocumentUrlService documentUrlService,
    IDocumentNavigationQueryService documentNavigationQueryService,
    ILanguageService languageService,
    IPublishedUrlProvider publishedUrlProvider)
    : IPublishedContentService
{
    private IPublishedContent? GetRootContentByHostname(string? hostname = null, string? culture = null)
    {

        if (documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys2))
        {
            var rootKeys2Test = rootKeys2.ToArray();
    
            
        }


        using UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
        
        var rootContents = GetRootContents(null, umbracoContextReference.UmbracoContext.Content).ToArray();

        if (rootContents.Length == 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(hostname))
        {
            return rootContents.FirstOrDefault();
        }

        foreach (var rootContent in rootContents)
        {
            var rootContentUrl = publishedUrlProvider.GetUrl(rootContent, UrlMode.Absolute, culture, current: null);
            if (IsHostnameMatch(hostname, rootContentUrl))
            {
                return rootContent;
            }
        }

        return GetConfiguredRootNodeSearchLevel() == 1
            ? rootContents.FirstOrDefault()
            : null;
    }

    public IPublishedContent? GetContentByPath(
        string path,
        string? hostname = null,
        string? culture = null,
        bool preview = false)
    {
        var normalizedPath = NormalizePath(path);

        var rootContent = GetRootContentByHostname(hostname, culture);

        if (rootContent is null)
        {
            throw new InvalidOperationException("No content found at root.");
        }

        var documentStartNodeId = rootContent.Id;

        var contentKey = documentUrlService.GetDocumentKeyByRoute(normalizedPath, culture, documentStartNodeId, preview);

        if (contentKey is null)
        {
            throw new InvalidOperationException("No content key found at path.");
        }

        using UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();

        var publishedContentCache = umbracoContextReference.UmbracoContext.Content;

        var publishedContent = publishedContentCache.GetById(preview, contentKey.Value);

        return publishedContent;
    }
    
    public async Task<string[]> GetLanguagesAsync()
    {
        var defaultLanguageCode = (await languageService.GetDefaultLanguageAsync())?.IsoCode;

        if (string.IsNullOrWhiteSpace(defaultLanguageCode))
        {
            return [];
        }

        var allLanguageCodes = (await languageService.GetAllAsync())
            .Select(language => language.IsoCode)
            .ToList();

        allLanguageCodes.Remove(defaultLanguageCode);
        allLanguageCodes.Insert(0, defaultLanguageCode);

        return allLanguageCodes.ToArray();
    }

    public IPublishedContent? GetContent(Guid key)
    {
        using UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();

        return umbracoContextReference.UmbracoContext.Content.GetById(key);
    }

    public IPublishedContent? GetRootContent(
        string? contentTypeAlias = null,
        IPublishedContentCache? publishedContentCache = null)
    {
        return InternalGetRootContents(contentTypeAlias, publishedContentCache).FirstOrDefault();
    }

    public IEnumerable<IPublishedContent> GetRootContents(
        string? contentTypeAlias = null,
        IPublishedContentCache? publishedContentCache = null)
    {
        return InternalGetRootContents(contentTypeAlias, publishedContentCache);
    }

    private IEnumerable<IPublishedContent> InternalGetRootContents(
        string? contentTypeAlias,
        IPublishedContentCache? publishedContentCache)
    {
        if (publishedContentCache is null)
        {
            using UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();
            return InternalGetRootContents(contentTypeAlias, umbracoContextReference.UmbracoContext.Content).ToArray();
        }

        if (documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys) is false)
        {
            return [];
        }

        var navigationRoots = rootKeys
            .Select(key => publishedContentCache.GetById(key))
            .WhereNotNull()
            .ToArray();

        var siteRoots = GetConfiguredRootNodeSearchLevel() switch
        {
            0 => navigationRoots,
            1 => navigationRoots.SelectMany(root => GetChildContents(root.Key, publishedContentCache)).ToArray(),
            _ => throw new InvalidOperationException(
                "The default ICmsContentService implementation only supports RootNodeSearchLevel values 0 and 1. Configure a custom ICmsContentService for deeper root structures.")
        };

        var filteredSiteRoots = FilterByContentTypeAlias(siteRoots, contentTypeAlias);
        if (xmlSitemapsOptions.Value.Mode == XmlSitemapsMode.Configuration)
        {
            return filteredSiteRoots;
        }

        var rootContentTypeAliases = xmlSitemapsOptions.Value.RootContentTypeAliases;
        if (rootContentTypeAliases.Length > 0)
        {
            filteredSiteRoots = filteredSiteRoots
                .Where(content => rootContentTypeAliases.Contains(
                    content.ContentType.Alias,
                    StringComparer.OrdinalIgnoreCase));
        }

        return filteredSiteRoots.Take(1).ToArray();
    }

    private int GetConfiguredRootNodeSearchLevel() => xmlSitemapsOptions.Value.RootNodeSearchLevel;

    private static IEnumerable<IPublishedContent> FilterByContentTypeAlias(
        IEnumerable<IPublishedContent> contents,
        string? contentTypeAlias)
    {
        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            return contents;
        }

        return contents.Where(content =>
            string.Equals(content.ContentType.Alias, contentTypeAlias, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<IPublishedContent> GetChildContents(Guid parentKey, IPublishedContentCache publishedContentCache)
    {
        if (documentNavigationQueryService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> childKeys) is false)
        {
            return [];
        }

        return childKeys
            .Select(key => publishedContentCache.GetById(key))
            .WhereNotNull();
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        path = path.Split('?', '#')[0];

        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return path;
    }

    private static bool IsHostnameMatch(string hostname, string? absoluteContentUrl)
    {
        if (string.IsNullOrWhiteSpace(absoluteContentUrl) || absoluteContentUrl.EndsWith('#'))
        {
            return false;
        }

        if (absoluteContentUrl.StartsWith(hostname, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!Uri.TryCreate(absoluteContentUrl, UriKind.Absolute, out var contentUri))
        {
            return false;
        }

        var hostnameWithoutScheme = RemoveScheme(hostname).Trim('/');
        return contentUri.Authority.StartsWith(hostnameWithoutScheme, StringComparison.OrdinalIgnoreCase);
    }

    private static string RemoveScheme(string hostname)
    {
        var schemeSeparatorIndex = hostname.IndexOf("://", StringComparison.Ordinal);
        return schemeSeparatorIndex < 0
            ? hostname
            : hostname[(schemeSeparatorIndex + 3)..];
    }
}
