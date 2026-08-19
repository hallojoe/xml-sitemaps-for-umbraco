using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Examine;
using Microsoft.Extensions.Options;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;

internal sealed class ExamineSitemapSearchResultFilter(
    IOptions<XmlSitemapsOptions> xmlSitemapsOptions) : IExamineSitemapSearchResultFilter
{
    private const string NodeTypeAliasField = "__NodeTypeAlias";

    public bool IsIncluded(ISearchResult searchResult)
    {
        var options = xmlSitemapsOptions.Value;

        if (IsExcludedByProperty(searchResult, options) ||
            IsExcludedByContentTypeAlias(searchResult, options))
        {
            return false;
        }

        return IsIncludedByContentTypeAlias(searchResult, options);
    }

    private static bool IsExcludedByProperty(ISearchResult searchResult, XmlSitemapsOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.ExcludingUrlPropertyAlias) &&
               !string.IsNullOrWhiteSpace(options.ExcludingUrlPropertyValue) &&
               searchResult.Values.TryGetValue(options.ExcludingUrlPropertyAlias, out var indexedValue) &&
               string.Equals(indexedValue, options.ExcludingUrlPropertyValue, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExcludedByContentTypeAlias(ISearchResult searchResult, XmlSitemapsOptions options)
    {
        return searchResult.Values.TryGetValue(NodeTypeAliasField, out var contentTypeAlias) &&
               options.ExcludedContentTypeAliases.Contains(contentTypeAlias, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsIncludedByContentTypeAlias(ISearchResult searchResult, XmlSitemapsOptions options)
    {
        return options.IncludedContentTypeAliases.Count == 0 ||
               (searchResult.Values.TryGetValue(NodeTypeAliasField, out var contentTypeAlias) &&
                options.IncludedContentTypeAliases.Contains(contentTypeAlias, StringComparer.OrdinalIgnoreCase));
    }
}
