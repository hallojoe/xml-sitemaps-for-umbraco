using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

/// <summary>
/// Collects content items that should be considered for sitemap rendering.
/// </summary>
public interface IPublishedContentCollector
{
    /// <summary>
    /// Returns the root content and descendants for sitemap rendering.
    /// </summary>
    public IEnumerable<IPublishedContent> Collect(PublishedContentRenderContext context);
}