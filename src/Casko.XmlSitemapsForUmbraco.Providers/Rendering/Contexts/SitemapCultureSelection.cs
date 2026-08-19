using Casko.XmlSitemapsForUmbraco.Common.Configuration;

namespace Casko.XmlSitemapsForUmbraco.Providers.Rendering.Contexts;

public static class SitemapCultureSelection
{
    private const string AllCultures = "*";

    public static List<string> Normalize(IEnumerable<string>? includedCultures)
    {
        var cultures = (includedCultures ?? [])
            .Where(culture => string.IsNullOrWhiteSpace(culture) is false)
            .Select(culture => culture.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cultures.Contains(AllCultures, StringComparer.OrdinalIgnoreCase))
        {
            return [];
        }

        return cultures;
    }

    public static IReadOnlyCollection<string> FilterAlternativeCultures(
        IEnumerable<string> availableCultures,
        IEnumerable<string>? includedCultures)
    {
        var normalizedCultures = Normalize(includedCultures);
        var available = availableCultures.ToList();

        if (normalizedCultures.Count == 0)
        {
            return available;
        }

        return available
            .Where(culture => normalizedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    public static SitemapCultureSelectionResult Resolve(
        IEnumerable<string> availableCultures,
        XmlSitemapsOptions rootOptions,
        SitemapOptions? sitemapOptions = null)
    {
        var available = availableCultures
            .Where(culture => string.IsNullOrWhiteSpace(culture) is false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rootIncludedCultures = Normalize(rootOptions.IncludedCultures);
        var sitemapIncludedCultures = Normalize(sitemapOptions?.IncludedCultures);
        var rootExcludedCultures = Normalize(rootOptions.ExcludedCultures);
        var sitemapExcludedCultures = Normalize(sitemapOptions?.ExcludedCultures);

        var resolvedCultures = rootIncludedCultures.Count == 0 && sitemapIncludedCultures.Count == 0
            ? available
            : available
                .Where(culture =>
                    rootIncludedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase) ||
                    sitemapIncludedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase))
                .ToList();

        resolvedCultures = resolvedCultures
            .Where(culture =>
                rootExcludedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase) is false ||
                sitemapIncludedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase))
            .Where(culture => sitemapExcludedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase) is false)
            .ToList();

        var renderAlternateLinks = resolvedCultures.Count != 1 ||
            rootOptions.RenderAlternateLinksForSingleCultureSitemaps;

        return new SitemapCultureSelectionResult(
            resolvedCultures,
            renderAlternateLinks);
    }
}

public sealed record SitemapCultureSelectionResult(
    IReadOnlyCollection<string> Cultures,
    bool RenderAlternateLinks);
