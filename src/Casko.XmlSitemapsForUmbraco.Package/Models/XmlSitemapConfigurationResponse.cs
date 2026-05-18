using Casko.XmlSitemapsForUmbraco.Common.Configuration;

namespace Casko.XmlSitemapsForUmbraco.Package.Models;

/// <summary>
/// Read-only XML sitemap configuration summary for the backoffice dashboard.
/// </summary>
public sealed record XmlSitemapConfigurationResponse
{
    /// <summary>
    /// Gets a value indicating whether XML sitemaps are enabled.
    /// </summary>
    public required bool Enabled { get; init; }

    /// <summary>
    /// Gets a value indicating whether friendly XML rewrite paths are enabled.
    /// </summary>
    public required bool RewritesEnabled { get; init; }

    /// <summary>
    /// Gets a value indicating whether alternate links are rendered for single-culture sitemaps.
    /// </summary>
    public required bool RenderAlternateLinksForSingleCultureSitemaps { get; init; }

    /// <summary>
    /// Gets the number of configured content sitemaps.
    /// </summary>
    public required int SitemapCount { get; init; }

    /// <summary>
    /// Gets the number of configured custom sitemaps.
    /// </summary>
    public required int CustomSitemapCount { get; init; }

    /// <summary>
    /// Gets the number of configured sitemap indexes.
    /// </summary>
    public required int IndexCount { get; init; }

    /// <summary>
    /// Gets root-level filter settings.
    /// </summary>
    public required XmlSitemapGlobalFiltersResponse GlobalFilters { get; init; }

    /// <summary>
    /// Gets storage refresh settings.
    /// </summary>
    public required XmlSitemapStorageConfigurationResponse Storage { get; init; }

    /// <summary>
    /// Gets configured content sitemap rows.
    /// </summary>
    public required IReadOnlyList<XmlSitemapConfigurationRowResponse> Sitemaps { get; init; }

    /// <summary>
    /// Gets configured custom sitemap rows.
    /// </summary>
    public required IReadOnlyList<XmlSitemapCustomConfigurationRowResponse> CustomSitemaps { get; init; }

    /// <summary>
    /// Gets configured sitemap index rows.
    /// </summary>
    public required IReadOnlyList<XmlSitemapIndexConfigurationRowResponse> Indexes { get; init; }

    /// <summary>
    /// Creates a response from configured XML sitemap options.
    /// </summary>
    public static XmlSitemapConfigurationResponse FromOptions(XmlSitemapsOptions options)
    {
        return new XmlSitemapConfigurationResponse
        {
            Enabled = options.Enabled,
            RewritesEnabled = options.RewritesEnabled,
            RenderAlternateLinksForSingleCultureSitemaps =
                options.RenderAlternateLinksForSingleCultureSitemaps,
            SitemapCount = options.Sitemaps.Count,
            CustomSitemapCount = options.CustomSitemaps.Count,
            IndexCount = options.Indexes.Count,
            GlobalFilters = new XmlSitemapGlobalFiltersResponse
            {
                IncludedContentTypeAliases = options.IncludedContentTypeAliases,
                ExcludedContentTypeAliases = options.ExcludedContentTypeAliases,
                IncludedCultures = options.IncludedCultures,
                ExcludedCultures = options.ExcludedCultures
            },
            Storage = new XmlSitemapStorageConfigurationResponse
            {
                RefreshStaleAfterSeconds = options.Storage.RefreshStaleAfterSeconds,
                BackgroundJobEnabled = options.Storage.BackgroundJob.Enabled,
                BackgroundJobIntervalSeconds = options.Storage.BackgroundJob.IntervalSeconds
            },
            Sitemaps = options.Sitemaps
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => XmlSitemapConfigurationRowResponse.FromOptions(pair.Key, pair.Value))
                .ToArray(),
            CustomSitemaps = options.CustomSitemaps
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => XmlSitemapCustomConfigurationRowResponse.FromOptions(pair.Key, pair.Value))
                .ToArray(),
            Indexes = options.Indexes
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => XmlSitemapIndexConfigurationRowResponse.FromOptions(pair.Key, pair.Value))
                .ToArray()
        };
    }
}

/// <summary>
/// Root-level XML sitemap filter settings.
/// </summary>
public sealed record XmlSitemapGlobalFiltersResponse
{
    /// <summary>
    /// Gets root-level included content type aliases.
    /// </summary>
    public required IReadOnlyList<string> IncludedContentTypeAliases { get; init; }

    /// <summary>
    /// Gets root-level excluded content type aliases.
    /// </summary>
    public required IReadOnlyList<string> ExcludedContentTypeAliases { get; init; }

    /// <summary>
    /// Gets root-level included cultures.
    /// </summary>
    public required IReadOnlyList<string> IncludedCultures { get; init; }

    /// <summary>
    /// Gets root-level excluded cultures.
    /// </summary>
    public required IReadOnlyList<string> ExcludedCultures { get; init; }
}

/// <summary>
/// XML sitemap storage refresh settings.
/// </summary>
public sealed record XmlSitemapStorageConfigurationResponse
{
    /// <summary>
    /// Gets the number of seconds after which stored sitemap XML is considered stale.
    /// </summary>
    public required int RefreshStaleAfterSeconds { get; init; }

    /// <summary>
    /// Gets a value indicating whether the background refresh job is enabled.
    /// </summary>
    public required bool BackgroundJobEnabled { get; init; }

    /// <summary>
    /// Gets the number of seconds between background refresh job runs.
    /// </summary>
    public required int BackgroundJobIntervalSeconds { get; init; }
}

/// <summary>
/// Configured content sitemap row.
/// </summary>
public sealed record XmlSitemapConfigurationRowResponse
{
    /// <summary>
    /// Gets the configured sitemap key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the configured content path.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Gets the configured host name.
    /// </summary>
    public string? HostName { get; init; }

    /// <summary>
    /// Gets the configured primary culture.
    /// </summary>
    public string? Culture { get; init; }

    /// <summary>
    /// Gets configured included cultures.
    /// </summary>
    public required IReadOnlyList<string> IncludedCultures { get; init; }

    /// <summary>
    /// Gets configured excluded cultures.
    /// </summary>
    public required IReadOnlyList<string> ExcludedCultures { get; init; }

    /// <summary>
    /// Gets configured included document type aliases.
    /// </summary>
    public required IReadOnlyList<string> IncludedDocumentTypeAliases { get; init; }

    /// <summary>
    /// Gets configured excluded document type aliases.
    /// </summary>
    public required IReadOnlyList<string> ExcludedDocumentTypeAliases { get; init; }

    internal static XmlSitemapConfigurationRowResponse FromOptions(string key, SitemapOptions options)
    {
        return new XmlSitemapConfigurationRowResponse
        {
            Key = key,
            Path = options.Path,
            HostName = options.HostName,
            Culture = options.Culture,
            IncludedCultures = options.IncludedCultures,
            ExcludedCultures = options.ExcludedCultures,
            IncludedDocumentTypeAliases = options.IncludedDocumentTypeAliases,
            ExcludedDocumentTypeAliases = options.ExcludedDocumentTypeAliases
        };
    }
}

/// <summary>
/// Configured custom sitemap row.
/// </summary>
public sealed record XmlSitemapCustomConfigurationRowResponse
{
    /// <summary>
    /// Gets the configured sitemap key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the configured custom provider alias.
    /// </summary>
    public string? ProviderAlias { get; init; }

    /// <summary>
    /// Gets the configured host name.
    /// </summary>
    public string? HostName { get; init; }

    /// <summary>
    /// Gets the number of custom provider settings.
    /// </summary>
    public required int SettingCount { get; init; }

    /// <summary>
    /// Gets the custom provider setting keys without their values.
    /// </summary>
    public required IReadOnlyList<string> SettingKeys { get; init; }

    internal static XmlSitemapCustomConfigurationRowResponse FromOptions(
        string key,
        CustomSitemapOptions options)
    {
        return new XmlSitemapCustomConfigurationRowResponse
        {
            Key = key,
            ProviderAlias = options.ProviderAlias,
            HostName = options.HostName,
            SettingCount = options.Settings.Count,
            SettingKeys = options.Settings.Keys.Order(StringComparer.Ordinal).ToArray()
        };
    }
}

/// <summary>
/// Configured sitemap index row.
/// </summary>
public sealed record XmlSitemapIndexConfigurationRowResponse
{
    /// <summary>
    /// Gets the configured index key.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the configured host name.
    /// </summary>
    public string? HostName { get; init; }

    /// <summary>
    /// Gets the configured sitemap keys included in this index.
    /// </summary>
    public required IReadOnlyList<string> Sitemaps { get; init; }

    internal static XmlSitemapIndexConfigurationRowResponse FromOptions(
        string key,
        SitemapIndexOptions options)
    {
        return new XmlSitemapIndexConfigurationRowResponse
        {
            Key = key,
            HostName = options.HostName,
            Sitemaps = options.Sitemaps
        };
    }
}
