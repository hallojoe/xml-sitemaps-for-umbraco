namespace Casko.XmlSitemapsForUmbraco.Common.Configuration;

/// <summary>
/// XML sitemaps settings.
/// </summary>
public sealed class XmlSitemapsOptions
{
    /// <summary>
    /// Key.
    /// </summary>
    public const string Key = "XmlSiteMaps";

    /// <summary>
    /// Gets or sets a value indicating whether XML sitemaps are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether configured sitemaps and sitemap indexes should be exposed as XML rewrite paths.
    /// </summary>
    public bool RewritesEnabled { get; set; }

    /// <summary>
    /// Gets or sets the path for the sitemap.
    /// </summary>
    public List<string> IncludedContentTypeAliases { get; set; } = [];

    /// <summary>
    /// Gets or sets the host name for the sitemap.
    /// </summary>
    public List<string> ExcludedContentTypeAliases { get; set; } = [];

    /// <summary>
    /// Gets or sets the culture for the sitemap.
    /// </summary>
    public List<string> IncludedCultures { get; set; } = [];

    /// <summary>
    /// Gets or sets the culture for the sitemap.
    /// </summary>
    public List<string> ExcludedCultures { get; set; } = [];

    /// <summary>
    /// Gets or sets the property alias whose value can exclude a content URL from generated sitemaps.
    /// </summary>
    public string? ExcludingUrlPropertyAlias { get; set; }

    /// <summary>
    /// Gets or sets the property value that excludes a content URL when found in the configured property.
    /// </summary>
    public string? ExcludingUrlPropertyValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render alternate links for single culture sitemaps.
    /// </summary>
    public bool RenderAlternateLinksForSingleCultureSitemaps { get; set; }

    /// <summary>
    /// Dictionary of sitemap configurations keyed by sitemap name.
    /// </summary>
    public Dictionary<string, SitemapOptions> Sitemaps { get; set; } = [];

    /// <summary>
    /// Dictionary of custom sitemap configurations keyed by sitemap name.
    /// </summary>
    public Dictionary<string, CustomSitemapOptions> CustomSitemaps { get; set; } = [];

    /// <summary>
    /// Dictionary of sitemap index configurations keyed by index name.
    /// </summary>
    public Dictionary<string, SitemapIndexOptions> Indexes { get; set; } = [];

    /// <summary>
    /// Gets or sets storage refresh settings.
    /// </summary>
    public XmlSitemapStorageOptions Storage { get; set; } = new();
}

/// <summary>
/// XML sitemap storage refresh settings.
/// </summary>
public sealed class XmlSitemapStorageOptions
{
    /// <summary>
    /// Gets or sets the number of seconds after which a stored sitemap is considered stale.
    /// </summary>
    public int RefreshStaleAfterSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets or sets background job settings for stored sitemap refreshes.
    /// </summary>
    public XmlSitemapStorageBackgroundJobOptions BackgroundJob { get; set; } = new();
}

/// <summary>
/// Stored XML sitemap background job settings.
/// </summary>
public sealed class XmlSitemapStorageBackgroundJobOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the background refresh job is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of seconds between background refresh job runs.
    /// </summary>
    public int IntervalSeconds { get; set; } = 3600;
}

/// <summary>
/// Individual sitemap configuration.
/// </summary>
public sealed class SitemapOptions
{
    /// <summary>
    /// Gets or sets the public XML file name for this sitemap, without the .xml extension.
    /// </summary>
    public string? PublicName { get; set; }

    /// <summary>
    /// Gets or sets the path of content to render.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the host name from where path should be resolved..
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// Primary culture of content to render.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// List of cultures to render.
    /// </summary>
    public List<string> IncludedCultures { get; set; } = [];

    /// <summary>
    /// List of cultures to exclude.
    /// </summary>
    public List<string> ExcludedCultures { get; set; } = [];

    /// <summary>
    /// List of document types to render.
    /// </summary>
    public List<string> IncludedDocumentTypeAliases { get; set; } = [];

    /// <summary>
    /// List of document types to exclude.
    /// </summary>
    public List<string> ExcludedDocumentTypeAliases { get; set; } = [];
}

/// <summary>
/// Custom sitemap configuration.
/// </summary>
public sealed class CustomSitemapOptions
{
    /// <summary>
    /// Gets or sets the public XML file name for this custom sitemap, without the .xml extension.
    /// </summary>
    public string? PublicName { get; set; }

    /// <summary>
    /// Gets or sets the alias of the custom sitemap provider.
    /// </summary>
    public string? ProviderAlias { get; set; }

    /// <summary>
    /// Gets or sets the host name for this custom XML sitemap.
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// Gets or sets provider-specific settings.
    /// </summary>
    public Dictionary<string, string?> Settings { get; set; } = [];
}

/// <summary>
/// Sitemap index configuration.
/// </summary>
public sealed class SitemapIndexOptions
{
    /// <summary>
    /// Gets or sets the public XML file name for this sitemap index, without the .xml extension.
    /// </summary>
    public string? PublicName { get; set; }

    /// <summary>
    /// Gets or sets the host name for this XML sitemap index.
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// Gets or sets the list of XML sitemap keys to include in the index.
    /// </summary>
    public List<string> Sitemaps { get; set; } = [];
}
