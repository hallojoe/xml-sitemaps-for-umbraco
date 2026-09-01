namespace Casko.XmlSitemapsForUmbraco.Providers.Rendering.Urls;

/// <summary>
/// Builds URLs used by sitemap and sitemap index models.
/// </summary>
public interface IXmlSitemapUrlBuilder
{
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
