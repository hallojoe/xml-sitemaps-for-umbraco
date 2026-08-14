using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
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
    IHostUrlProvider hostUrlProvider)
    : IPublishedContentService
{
    private IPublishedContent? GetRootContentByHostname(
        IPublishedContentCache publishedContentCache,
        string? hostname = null,
        string? culture = null)
    {
        var hostUrl = ResolveHostUrl(hostname, culture);

        if (hostUrl is null)
        {
            return null;
        }

        return publishedContentCache.GetById(hostUrl.Key);
    }

    public IPublishedContent? GetContentByPath(
        string path,
        string? hostname = null,
        string? culture = null,
        bool preview = false,
        IPublishedContentCache? publishedContentCache = null)
    {
        if (publishedContentCache is not null)
        {
            return InternalGetContentByPath(path, hostname, culture, preview, publishedContentCache);
        }

        using UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();

        return InternalGetContentByPath(path, hostname, culture, preview, umbracoContextReference.UmbracoContext.Content);
    }

    private IPublishedContent? InternalGetContentByPath(
        string path,
        string? hostname,
        string? culture,
        bool preview,
        IPublishedContentCache publishedContentCache)
    {
        var normalizedPath = NormalizePath(path);
        var rootContent = GetRootContentByHostname(publishedContentCache, hostname, culture);

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

    public IPublishedContent? GetContent(Guid key, IPublishedContentCache? publishedContentCache = null)
    {
        if (publishedContentCache is not null)
        {
            return publishedContentCache.GetById(key);
        }

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

    private HostUrl? ResolveHostUrl(string? hostname, string? culture)
    {
        var hostUrls = hostUrlProvider.GetHostUrlsAsync().GetAwaiter().GetResult().ToList();
        if (hostUrls.Count == 0)
        {
            return null;
        }

        var candidates = string.IsNullOrWhiteSpace(hostname)
            ? hostUrls
            : hostUrls.Where(hostUrl => IsHostnameMatch(hostname, hostUrl.Uri)).ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(culture) is false)
        {
            var cultureHostUrl = candidates.FirstOrDefault(hostUrl =>
                string.Equals(hostUrl.Culture, culture, StringComparison.OrdinalIgnoreCase));
            if (cultureHostUrl is not null)
            {
                return cultureHostUrl;
            }
        }

        return candidates.FirstOrDefault(hostUrl => hostUrl.IsDefaultCulture) ?? candidates.First();
    }

    private static bool IsHostnameMatch(string hostname, Uri hostUrl)
    {
        var normalizedHostname = NormalizeHostname(hostname);
        var normalizedHostUrl = NormalizeHostname(hostUrl.ToString());

        return string.Equals(normalizedHostname, normalizedHostUrl, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeHostname(string hostname)
    {
        if (!Uri.TryCreate(hostname, UriKind.Absolute, out var uri))
        {
            hostname = "https://" + hostname.Trim('/');
            Uri.TryCreate(hostname, UriKind.Absolute, out uri);
        }

        return uri is null
            ? hostname.TrimEnd('/')
            : uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
    }
}
