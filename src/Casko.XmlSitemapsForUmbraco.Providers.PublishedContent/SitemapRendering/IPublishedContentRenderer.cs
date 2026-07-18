using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

/// <summary>
/// Builds XML sitemap models from published content.
/// </summary>
public interface IPublishedContentRenderer
{
    /// <summary>
    /// Builds a sitemap from the supplied render context.
    /// </summary>
    public XmlSitemap Render(PublishedContentRenderContext context);
}