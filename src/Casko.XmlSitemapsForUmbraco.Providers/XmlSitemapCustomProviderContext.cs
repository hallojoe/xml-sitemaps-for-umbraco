namespace Casko.XmlSitemapsForUmbraco.Providers;

/// <summary>
/// Provides configuration context to a custom XML sitemap provider.
/// </summary>
public sealed record XmlSitemapCustomProviderContext(
    string Key,
    string? HostName,
    IReadOnlyDictionary<string, string?> Settings);
