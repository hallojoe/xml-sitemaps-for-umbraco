namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.Configuration;

/// <summary>
/// Resolves public XML sitemap names from configured options.
/// </summary>
public static class SitemapPublicName
{
    /// <summary>
    /// Resolves the public sitemap name, falling back to the internal configuration key.
    /// </summary>
    public static string Resolve(string key, string? publicName)
    {
        return string.IsNullOrWhiteSpace(publicName)
            ? key
            : publicName.Trim();
    }
}
