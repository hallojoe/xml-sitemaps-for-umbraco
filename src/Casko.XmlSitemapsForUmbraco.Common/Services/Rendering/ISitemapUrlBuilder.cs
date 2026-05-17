using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

/// <summary>
/// Builds URLs used by sitemap and sitemap index models.
/// </summary>
public interface ISitemapUrlBuilder
{
    /// <summary>
    /// Builds the public URL for a content item and culture.
    /// </summary>
    public string BuildContentUrl(IPublishedContent content, string languageCode, string? hostname);

    /// <summary>
    /// Builds the package API URL for a configured sitemap alias.
    /// </summary>
    public string BuildSitemapApiUrl(string sitemapAlias, string? hostname);

    /// <summary>
    /// Builds the legacy XML file URL for a configured sitemap alias.
    /// </summary>
    public string BuildLegacySitemapFileUrl(string sitemapAlias, string? hostname);

    /// <summary>
    /// Combines a relative URL with an optional hostname.
    /// </summary>
    public string CombineWithHostname(string relativeUrl, string? hostname);
}