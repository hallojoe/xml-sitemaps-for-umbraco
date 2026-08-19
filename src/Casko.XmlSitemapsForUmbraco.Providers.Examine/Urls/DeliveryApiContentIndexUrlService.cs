using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Examine;
using Examine.Search;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;

/// <summary>
/// Resolves sitemap URLs from Umbraco's culture-variant Delivery API content index.
/// </summary>
public sealed class DeliveryApiContentIndexUrlService(
    IOptions<UrlResolverSettings> urlResolverSettings,
    IOptions<WebRoutingSettings> webRoutingSettings,
    IOptions<RequestHandlerSettings> requestHandlerSettings,
    IOptions<XmlSitemapsOptions> xmlSitemapsOptions,
    IExamineSitemapSearchResultFilter searchResultFilter,
    IDomainService domainService,
    IDocumentUrlService documentUrlService,
    IExamineManager examineManager) : ICmsUrlService
{
    private const string AncestorIdsField = "ancestorIds";
    private const string ContentIdField = "id";
    private const string ContentKeyField = "itemId";
    private const string CultureField = "culture";
    private const string UpdateDateField = "updateDate";

    /// <inheritdoc />
    public async Task<IEnumerable<CmsUrl>> GetUrlsByKeyAsync(Guid key, CancellationToken cancellationToken = default)
    {
        if (!examineManager.TryGetIndex(
                Umbraco.Cms.Core.Constants.UmbracoIndexes.DeliveryApiContentIndexName,
                out var index))
        {
            return [];
        }

        var searchResults = GetSearchResults(index.Searcher, key);
        var assignedDomains = (await domainService.GetAssignedDomainsAsync(key, false)).ToArray();
        var cmsUrls = new List<CmsUrl>();

        foreach (var searchResult in searchResults)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!searchResultFilter.IsIncluded(searchResult) ||
                !TryCreateCmsUrl(searchResult, assignedDomains, out var cmsUrl))
            {
                continue;
            }

            cmsUrls.Add(cmsUrl);
        }

        return cmsUrls;
    }

    private IEnumerable<ISearchResult> GetSearchResults(ISearcher searcher, Guid rootKey)
    {
        var searchResults = new List<ISearchResult>();
        var skip = 0;
        long total;
        var rootKeyValue = rootKey.ToString("D");

        do
        {
            var results = searcher
                .CreateQuery(IndexTypes.Content)
                .NativeQuery($"+({ContentKeyField}:{rootKeyValue} {AncestorIdsField}:{rootKeyValue})")
                .Execute(new QueryOptions(skip, urlResolverSettings.Value.PageSize));

            total = results.TotalItemCount;
            searchResults.AddRange(results);
            skip += urlResolverSettings.Value.PageSize;
        }
        while (skip < total);

        return searchResults;
    }

    private bool TryCreateCmsUrl(
        ISearchResult searchResult,
        IReadOnlyCollection<Umbraco.Cms.Core.Models.IDomain> assignedDomains,
        out CmsUrl cmsUrl)
    {
        cmsUrl = null!;

        if (!TryGetValue(searchResult, ContentKeyField, out var contentKeyValue) ||
            !Guid.TryParse(contentKeyValue, out var contentKey) ||
            !TryGetValue(searchResult, ContentIdField, out var contentIdValue) ||
            !int.TryParse(contentIdValue, out var contentId) ||
            !TryGetValue(searchResult, CultureField, out var culture) ||
            !IsIncludedCulture(culture) ||
            !TryGetValue(searchResult, UpdateDateField, out var updateDateValue) ||
            !long.TryParse(updateDateValue, out var updateDateTicks))
        {
            return false;
        }

        var url = documentUrlService.GetLegacyRouteFormat(contentKey, culture, false);
        if (string.Equals(url, "#", StringComparison.Ordinal))
        {
            return false;
        }

        var resolvedUrl = ExternalIndexUrlService.ResolveUrl(
            url,
            culture,
            assignedDomains,
            webRoutingSettings.Value.UmbracoApplicationUrl,
            requestHandlerSettings.Value.AddTrailingSlash);

        cmsUrl = new CmsUrl(
            resolvedUrl.UrlPath,
            new DateTime(updateDateTicks),
            resolvedUrl.Hostname,
            culture,
            contentId,
            contentKey);
        return true;
    }

    private bool IsIncludedCulture(string culture)
    {
        var options = xmlSitemapsOptions.Value;
        return (options.IncludedCultures.Count == 0 ||
                options.IncludedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase)) &&
               !options.ExcludedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryGetValue(ISearchResult searchResult, string fieldName, out string value)
    {
        return searchResult.Values.TryGetValue(fieldName, out value!) &&
               !string.IsNullOrWhiteSpace(value);
    }
}
