namespace Casko.XmlSitemapsForUmbraco.Storage;

/// <summary>
/// Represents a raw XML sitemap document loaded from or written to a data source.
/// </summary>
public sealed record XmlSitemapStoredDocument(
    XmlSitemapStorageKey Key,
    Guid? MediaKey,
    int? MediaId,
    string FileName,
    string? MediaPath,
    string Xml,
    DateTimeOffset? RefreshedUtc = null);
