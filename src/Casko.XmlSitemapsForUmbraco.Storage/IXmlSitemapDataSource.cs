namespace Casko.XmlSitemapsForUmbraco.Storage;

/// <summary>
/// Reads and writes raw XML sitemap documents from a backing data source.
/// </summary>
public interface IXmlSitemapDataSource
{
    /// <summary>
    /// Reads a stored XML sitemap document, or returns <c>null</c> when it does not exist.
    /// </summary>
    public Task<XmlSitemapStoredDocument?> ReadAsync(XmlSitemapStorageKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a raw XML sitemap document and returns the stored representation.
    /// </summary>
    public Task<XmlSitemapStoredDocument> WriteAsync(
        XmlSitemapStorageKey key,
        string xml,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether a stored XML sitemap document exists.
    /// </summary>
    public async Task<bool> ExistsAsync(XmlSitemapStorageKey key, CancellationToken cancellationToken = default)
    {
        return await ReadAsync(key, cancellationToken) is not null;
    }
}
