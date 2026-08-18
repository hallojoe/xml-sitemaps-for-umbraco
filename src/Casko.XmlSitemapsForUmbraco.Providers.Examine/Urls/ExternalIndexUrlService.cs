using Examine;
using Examine.Search;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;

public class UrlResolverSettings
{
    public const string Key = "XmlSitemaps:Providers:Examine";
    public ushort PageSize { get; set; } = 1000;
}



public sealed class ExternalIndexUrlService(
    IOptions<UrlResolverSettings> urlResolverSettings,
    IOptions<WebRoutingSettings> webRoutingSettings,
    IOptions<RequestHandlerSettings> requestHandlerSettings,
    IOptions<XmlSitemapsOptions> xmlSitemapsOptions,
    ILanguageService languageService,
    IDomainService domainService,
    IDocumentUrlService documentUrlService,
    IExamineManager examineManager) : ICmsUrlService
{
    /// <inheritdoc />
    public async Task<IEnumerable<CmsUrl>> GetUrlsByKeyAsync(Guid key, CancellationToken cancellationToken = default)
    {
        if(!examineManager.TryGetIndex(Umbraco.Cms.Core.Constants.UmbracoIndexes.ExternalIndexName, out var index))
        {
            return [];
        }

        List<ISearchResult> searchResultList = [];
        
        var skip = 0;
        long total;

        do
        {
         
            var searchResults = index.Searcher
                .CreateQuery(IndexTypes.Content)
                .NativeQuery("+pathKeys:" + key.ToString("D"))
                .Execute(new QueryOptions(skip, urlResolverSettings.Value.PageSize));
            
            total = searchResults.TotalItemCount;
            
            searchResultList.AddRange(searchResults);
            
            skip += urlResolverSettings.Value.PageSize;
        }
        while (skip < total);

        var cmsUrls = new List<CmsUrl>();

        var languages = await GetCandidateLanguagesAsync();
        var assignedDomains = (await domainService.GetAssignedDomainsAsync(key, false)).ToArray();
        
        foreach (var searchResult in searchResultList)
        {
            if (searchResult.Values.TryGetValue(
                    xmlSitemapsOptions.Value.ExcludingUrlPropertyAlias ?? "__unknown", 
                    out var excludingPropertyAlias) 
                && 
                    excludingPropertyAlias.Equals(xmlSitemapsOptions.Value.ExcludingUrlPropertyValue, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            if (!Guid.TryParse(searchResult.Values["__Key"], out var contentKey))
            {
                continue;
            }

            if (!int.TryParse(searchResult.Values["__NodeId"], out var contentId))
            {
                continue;
            }

            // if (!searchResult.Values.TryGetValue("__Path", out var path))
            // {
            //     continue;
            // }
            
            if (!long.TryParse(searchResult.Values["updateDate"], out var updateDateAsLong))
            {
                continue;
            }

            var updatedDate = new DateTime(updateDateAsLong);
            
            foreach (var language in languages)
            {
                var updatedDateForCulture = updatedDate;

                if (long.TryParse(searchResult.Values["updateDate_" + language], out var updatedDateAsLongForCulture))
                {
                    updatedDateForCulture = new DateTime(updatedDateAsLongForCulture);
                }

                if (!searchResult.Values.TryGetValue("__Published_" + language, out var publishedValueForCulture))
                {
                    continue;
                }

                if (!publishedValueForCulture.ToLower().Equals("y"))
                {
                    continue;
                }

                var url = documentUrlService.GetLegacyRouteFormat(contentKey, language, false);

                if (url.Equals("#"))
                {
                    continue;
                }

                var resolvedUrl = ResolveUrl(
                    url, 
                    language, 
                    assignedDomains, 
                    webRoutingSettings.Value.UmbracoApplicationUrl, 
                    requestHandlerSettings.Value.AddTrailingSlash);
                
                cmsUrls.Add(new CmsUrl(
                    resolvedUrl.UrlPath,
                    updatedDateForCulture,
                    resolvedUrl.Hostname,
                    language,
                    contentId,
                    contentKey));
            }            
        }

        return cmsUrls;
    }
    
    internal static ResolvedCmsUrl ResolveUrl(
        string url,
        string? culture,
        IReadOnlyCollection<IDomain> assignedDomains,
        string? fallbackApplicationUrl, 
        bool addTrailingSlash = false)
    {
        if (TryCreateHttpUri(url, out var absoluteUrl))
        {
            return new ResolvedCmsUrl(
                RemoveIdFromLegacyRouteFormat(absoluteUrl.PathAndQuery, addTrailingSlash),
                absoluteUrl.GetLeftPart(UriPartial.Authority));
        }

        var sanitizedUrl = RemoveIdFromLegacyRouteFormat(url, addTrailingSlash);
        var hostname = ResolveHostname(culture, assignedDomains, fallbackApplicationUrl);

        return new ResolvedCmsUrl(sanitizedUrl, hostname);
    }

    internal async Task<string[]> GetCandidateLanguagesAsync()
    {
        var defaultLanguageCode = (await languageService.GetDefaultLanguageAsync())?.IsoCode;
        var languageCodes = (await languageService.GetAllAsync())
            .Select(language => language.IsoCode)
            .Where(languageCode => string.IsNullOrWhiteSpace(languageCode) is false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (string.IsNullOrWhiteSpace(defaultLanguageCode) is false)
        {
            languageCodes.RemoveAll(languageCode =>
                string.Equals(languageCode, defaultLanguageCode, StringComparison.OrdinalIgnoreCase));
            languageCodes.Insert(0, defaultLanguageCode);
        }

        var includedCultures = xmlSitemapsOptions.Value.IncludedCultures;
        if (includedCultures.Count > 0)
        {
            languageCodes = languageCodes
                .Where(languageCode => includedCultures.Contains(languageCode, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        var excludedCultures = xmlSitemapsOptions.Value.ExcludedCultures;
        if (excludedCultures.Count > 0)
        {
            languageCodes = languageCodes
                .Where(languageCode => !excludedCultures.Contains(languageCode, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        return languageCodes.ToArray();
    }
    
    internal static string RemoveIdFromLegacyRouteFormat(string url, bool addTrailingSlash = false)
    {
        var trimmedUrl = url.TrimStart('/');
        var separatorIndex = trimmedUrl.IndexOf('/');
        var potentialContentIdPart = separatorIndex < 0
            ? trimmedUrl
            : trimmedUrl[..separatorIndex];

        if (!int.TryParse(potentialContentIdPart, out _))
        {
            return url;
        }

        var sanitizedUrl = separatorIndex < 0
            ? "/"
            : "/" + trimmedUrl[(separatorIndex + 1)..];

        if (string.IsNullOrWhiteSpace(sanitizedUrl))
        {
            return "/";
        }

        if (addTrailingSlash is false)
        {
            return sanitizedUrl == "/" ? sanitizedUrl : sanitizedUrl.TrimEnd('/');
        }

        return sanitizedUrl.EndsWith('/') ? sanitizedUrl : sanitizedUrl + '/';
    }

    internal static string? ResolveHostname(
        string? culture,
        IReadOnlyCollection<IDomain> assignedDomains,
        string? fallbackApplicationUrl)
    {
        var domain = assignedDomains
            .OrderBy(domain => domain.SortOrder)
            .FirstOrDefault(domain =>
                string.Equals(domain.LanguageIsoCode, culture, StringComparison.OrdinalIgnoreCase));

        var domainName = domain?.DomainName;

        if (!string.IsNullOrWhiteSpace(domainName))
        {
            return NormalizeHostname(domainName, fallbackApplicationUrl);
        }

        return string.IsNullOrWhiteSpace(fallbackApplicationUrl)
            ? null
            : NormalizeHostname(fallbackApplicationUrl, fallbackApplicationUrl);
    }

    internal static string NormalizeHostname(string hostname, string? fallbackApplicationUrl)
    {
        if (TryCreateHttpUri(hostname, out _))
        {
            return hostname.TrimEnd('/');
        }

        if (hostname.StartsWith('/'))
        {
            var fallbackOrigin = ResolveFallbackOrigin(fallbackApplicationUrl);
            return $"{fallbackOrigin}{hostname}".TrimEnd('/');
        }

        var scheme = ResolveFallbackScheme(fallbackApplicationUrl);
        return $"{scheme}://{hostname.Trim('/')}";
    }

    internal static string ResolveFallbackOrigin(string? fallbackApplicationUrl)
    {
        if (Uri.TryCreate(fallbackApplicationUrl, UriKind.Absolute, out var applicationUri))
        {
            return applicationUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }

        return string.Empty;
    }

    internal static string ResolveFallbackScheme(string? fallbackApplicationUrl)
    {
        if (Uri.TryCreate(fallbackApplicationUrl, UriKind.Absolute, out var applicationUri))
        {
            return applicationUri.Scheme;
        }

        return Uri.UriSchemeHttps;
    }

    private static bool TryCreateHttpUri(string? value, out Uri uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var parsedUri) &&
            (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
        {
            uri = parsedUri;
            return true;
        }

        uri = null!;
        return false;
    }
    
}

internal sealed record ResolvedCmsUrl(string UrlPath, string? Hostname);
