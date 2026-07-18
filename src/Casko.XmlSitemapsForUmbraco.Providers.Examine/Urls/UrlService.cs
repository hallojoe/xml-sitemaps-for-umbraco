using Examine;
using Examine.Search;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace Casko.XmlSitemapsForUmbraco.Common.Providers.Examine.Urls;

public class UrlResolverSettings
{
    public const string Key = "Casko:Search:Url";
    public ushort PageSize { get; set; } = 1000;
}

public record CmsUrl(
    string UrlPath, 
    DateTime LastUpdate, 
    string? Hostname, 
    string? Culture, 
    int? Id = null,
    int? Key = null);

public interface ICmsUrlService
{
    /// <summary>
    /// Gets all root items in the content tree.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<CmsUrl>> GetUrlsByKeyAsync(Guid key, CancellationToken cancellationToken = default);
}

public sealed class ContentUrlService(
    IOptions<UrlResolverSettings> urlResolverSettings,
    IContentService contentService,
    IDocumentUrlService documentUrlService,
    IExamineManager examineManager) : ICmsUrlService
{
    /// <inheritdoc />
    public Task<IEnumerable<CmsUrl>> GetUrlsByKeyAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var indexName = "ExternalIndex";
        if(!examineManager.TryGetIndex(indexName, out var index))
        {
            return Task.FromResult(Enumerable.Empty<CmsUrl>());
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

        var languages = new List<string> {"da", "en", "pl"};
        foreach (var searchResult in searchResultList)
        {
            if (!Guid.TryParse(searchResult.Values["__Key"], out var contentKey))
            {
                continue;
            }

            if (!int.TryParse(searchResult.Values["__NodeId"], out var contentId))
            {
                continue;
            }

            if (!long.TryParse(searchResult.Values["updateDate"], out var updateDateAsLong))
            {
                continue;
            }

            var updatedDate = new DateTime(updateDateAsLong);
            
            foreach (var language in languages)
            {
                var updatedDateForCulture = updatedDate;

                if (!long.TryParse(searchResult.Values["updateDate_" + language], out var updatedDateAsLongForCulture))
                {
                    updatedDateForCulture = new DateTime(updatedDateAsLongForCulture);
                }

                var url = documentUrlService.GetLegacyRouteFormat(contentKey, language, false);

                if (url.Equals("#"))
                {
                    continue;
                }

                cmsUrls.Add(new CmsUrl(url, updatedDateForCulture, "", language, contentId));
            }            
        }

        return Task.FromResult<IEnumerable<CmsUrl>>(cmsUrls);
    }
}