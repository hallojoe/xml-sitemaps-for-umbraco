using Examine;
using Examine.Search;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;

public sealed class DeliveryApiContentIndexUrlService(
    IOptions<UrlResolverSettings> urlResolverSettings,
    IOptions<WebRoutingSettings> webRoutingSettings,
    IOptions<RequestHandlerSettings> requestHandlerSettings,
    IOptions<XmlSitemapsOptions> xmlSitemapsOptions,
    ILanguageService languageService,
    IDomainService domainService,
    IHostUrlProvider hostUrlProvider,
    IDocumentUrlService documentUrlService,
    IExamineManager examineManager) : ICmsUrlService
{
    /// <inheritdoc />
    public async Task<IEnumerable<CmsUrl>> GetUrlsByKeyAsync(Guid key, CancellationToken cancellationToken = default)
    {
        if(!examineManager.TryGetIndex(Umbraco.Cms.Core.Constants.UmbracoIndexes.DeliveryApiContentIndexName, out var index))
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
                .NativeQuery("+ancestorIds:" + key.ToString("D"))
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

                var url = documentUrlService.GetLegacyRouteFormat(contentKey, language, false);

                if (url.Equals("#"))
                {
                    continue;
                }

                var resolvedUrl = ExternalIndexUrlService.ResolveUrl(
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
}

