using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;

public sealed class SitemapContentTypeSelection
{
    private readonly HashSet<string> _includedAliases;
    private readonly HashSet<string> _excludedAliases;

    private SitemapContentTypeSelection(IEnumerable<string> includedAliases, IEnumerable<string> excludedAliases)
    {
        _includedAliases = new HashSet<string>(includedAliases, StringComparer.OrdinalIgnoreCase);
        _excludedAliases = new HashSet<string>(excludedAliases, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> IncludedAliases => _includedAliases;
    public IReadOnlyCollection<string> ExcludedAliases => _excludedAliases;

    public static SitemapContentTypeSelection Resolve(
        XmlSitemapsOptions rootOptions,
        SitemapOptions? sitemapOptions = null)
    {
        var includedAliases = NormalizeAliases(rootOptions.IncludedContentTypeAliases)
            .Concat(NormalizeAliases(sitemapOptions?.IncludedDocumentTypeAliases))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var excludedAliases = NormalizeAliases(rootOptions.ExcludedContentTypeAliases)
            .Concat(NormalizeAliases(sitemapOptions?.ExcludedDocumentTypeAliases))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return new SitemapContentTypeSelection(includedAliases, excludedAliases);
    }

    public bool ShouldInclude(IPublishedContent content)
    {
        var contentTypeAlias = content.ContentType.Alias;
        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            return false;
        }

        if (_includedAliases.Count > 0 && _includedAliases.Contains(contentTypeAlias) is false)
        {
            return false;
        }

        return _excludedAliases.Contains(contentTypeAlias) is false;
    }

    private static List<string> NormalizeAliases(IEnumerable<string>? aliases) =>
        (aliases ?? [])
            .Where(alias => string.IsNullOrWhiteSpace(alias) is false)
            .Select(alias => alias.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
