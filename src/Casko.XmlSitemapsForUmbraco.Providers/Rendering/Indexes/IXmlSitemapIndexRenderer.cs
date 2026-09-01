using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.Rendering.Contexts;

namespace Casko.XmlSitemapsForUmbraco.Providers.Rendering.Indexes;

/// <summary>
/// Builds XML sitemap index models from configured sitemap aliases.
/// </summary>
public interface IXmlSitemapIndexRenderer
{
    /// <summary>
    /// Builds a sitemap index from the supplied render context.
    /// </summary>
    public XmlSitemapIndex Render(XmlSitemapIndexRenderContext context);
}
