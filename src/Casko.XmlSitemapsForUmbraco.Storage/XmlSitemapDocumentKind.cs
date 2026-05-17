namespace Casko.XmlSitemapsForUmbraco.Storage;

/// <summary>
/// Identifies the kind of XML sitemap document stored by a data source.
/// </summary>
public enum XmlSitemapDocumentKind
{
    /// <summary>
    /// A regular XML sitemap document.
    /// </summary>
    Sitemap,

    /// <summary>
    /// An XML sitemap index document.
    /// </summary>
    SitemapIndex
}
