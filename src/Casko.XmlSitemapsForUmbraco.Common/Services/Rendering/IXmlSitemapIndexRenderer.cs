using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

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