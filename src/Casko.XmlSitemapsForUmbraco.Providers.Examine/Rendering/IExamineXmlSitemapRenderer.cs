using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;

/// <summary>
/// Builds XML sitemap models from Examine-backed CMS URLs.
/// </summary>
public interface IExamineXmlSitemapRenderer
{
    /// <summary>
    /// Builds a sitemap from the supplied render context.
    /// </summary>
    public XmlSitemap Render(ExamineXmlSitemapRenderContext context);
}
