using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

/// <summary>
/// Builds URLs used by sitemap and sitemap index models.
/// </summary>
public interface IPublishedContentUrlBuilder : IXmlSitemapUrlBuilder
{
    /// <summary>
    /// Builds the public URL for a content item and culture.
    /// </summary>
    public string BuildContentUrl(IPublishedContent content, string languageCode, string? hostname);

}
