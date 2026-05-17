using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

/// <summary>
/// Builds XML sitemap models from published content.
/// </summary>
public interface IXmlSitemapRenderer
{
    /// <summary>
    /// Builds a sitemap from the supplied render context.
    /// </summary>
    public XmlSiteMap Render(XmlSitemapRenderContext context);
}