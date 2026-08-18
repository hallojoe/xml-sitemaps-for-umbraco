namespace Casko.XmlSitemapsForUmbraco.Storage.Configuration;

/// <summary>
/// Configuration for stored XML sitemap delivery.
/// </summary>
public sealed class XmlSitemapStorageOptions
{
    /// <summary>
    /// Configuration key for XML sitemap storage.
    /// </summary>
    public const string Key = "XmlSitemaps:Storage";

    /// <summary>
    /// Gets or sets the number of seconds after which a stored sitemap is considered stale.
    /// </summary>
    public int RefreshStaleAfterSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets or sets background job settings for stored sitemap refreshes.
    /// When null, background refresh jobs are disabled.
    /// </summary>
    public XmlSitemapStorageBackgroundJobOptions? BackgroundJob { get; set; }
}

/// <summary>
/// Configuration for the stored XML sitemap refresh background job.
/// </summary>
public sealed class XmlSitemapStorageBackgroundJobOptions
{
    /// <summary>
    /// Gets or sets the number of seconds between background refresh job runs.
    /// </summary>
    public int IntervalSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets or sets the number of seconds to delay the background refresh job.
    /// </summary>
    public int RefreshJobDelayInSeconds { get; set; } = 10;
}
