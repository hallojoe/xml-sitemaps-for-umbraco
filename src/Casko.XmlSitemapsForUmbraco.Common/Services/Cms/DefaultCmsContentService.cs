using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Cms;

public class DefaultCmsContentService(
    IUmbracoContextFactory umbracoContextFactory,
    IDocumentUrlService documentUrlService,
    IDocumentNavigationQueryService documentNavigationQueryService,
    ILanguageService languageService)
    : ICmsContentService
{
    private IPublishedContent? GetRootContentByHostname(string? hostname = null, string? culture = null)
    {
        var rootContents = GetRootContents();

        if (string.IsNullOrWhiteSpace(hostname))
        {
            return rootContents.FirstOrDefault();
        }

        foreach (var rootContent in rootContents)
        {
            var rootContentUrl = rootContent.Url(culture, UrlMode.Absolute);
            if (IsHostnameMatch(hostname, rootContentUrl))
            {
                return rootContent;
            }
        }

        return null;
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

        return publishedContentCache.GetById(preview, contentKey.Value);
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

    public IPublishedContent? GetRootContent(string? contentTypeAlias = null)
    {
        return InternalGetRootContents(contentTypeAlias).FirstOrDefault();
    }

    public IEnumerable<IPublishedContent> GetRootContents(string? contentTypeAlias = null)
    {
        return InternalGetRootContents(contentTypeAlias);
    }

    private IEnumerable<IPublishedContent> InternalGetRootContents(string? contentTypeAlias = null)
    {
        using UmbracoContextReference umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();

        var publishedContentCache = umbracoContextReference.UmbracoContext.Content;

        if (documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys) is false)
        {
            return [];
        }

        var siteRoots = rootKeys
            .Select(key => publishedContentCache.GetById(key))
            .WhereNotNull();

        return string.IsNullOrWhiteSpace(contentTypeAlias)
            ? siteRoots
            : siteRoots.Where(content => content.ContentType.Alias == contentTypeAlias);
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

    private static bool IsHostnameMatch(string hostname, string absoluteContentUrl)
    {
        if (absoluteContentUrl.EndsWith('#'))
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
