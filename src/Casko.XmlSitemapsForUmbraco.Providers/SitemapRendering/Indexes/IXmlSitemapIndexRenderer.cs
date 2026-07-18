using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;

namespace Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Indexes;

/// <summary>
/// Builds XML sitemap index models from configured sitemap aliases.
/// </summary>
public interface IXmlSitemapIndexRenderer
{
    /// <summary>
    /// Builds a sitemap index from the supplied render context.
    /// </summary>
    public XmlSiteMapIndex Render(XmlSitemapIndexRenderContext context);
}
