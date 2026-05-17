using System.ComponentModel.DataAnnotations;

namespace Casko.XmlSitemapsForUmbraco.Storage;

/// <summary>
/// Identifies a stored XML sitemap document.
/// </summary>
public sealed record XmlSitemapStorageKey(
    XmlSitemapDocumentKind Kind,
    string Alias,
    string? HostName)
{
    /// <summary>
    /// Throws when the storage key cannot identify a document.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            throw new ValidationException("A sitemap alias is required.");
        }
    }
}
