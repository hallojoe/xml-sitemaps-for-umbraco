using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

/// <summary>
/// Builds XML sitemap index models from configured sitemap aliases.
/// </summary>
public interface IPublishedContentIndexRenderer
{
    /// <summary>
    /// Builds a sitemap index from the supplied render context.
    /// </summary>
    public XmlSitemapIndex Render(XmlSitemapIndexRenderContext context);
}