using Examine;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;

/// <summary>
/// Filters Examine search results before they are converted to sitemap URLs.
/// </summary>
public interface IExamineSitemapSearchResultFilter
{
    /// <summary>
    /// Gets whether the search result should be included in a sitemap.
    /// </summary>
    bool IsIncluded(ISearchResult searchResult);
}
