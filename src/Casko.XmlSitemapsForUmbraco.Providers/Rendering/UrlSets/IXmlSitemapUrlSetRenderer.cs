using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Providers.Rendering.UrlSets;

/// <summary>
/// Builds XML sitemap models from rendered sitemap URLs.
/// </summary>
public interface IXmlSitemapUrlSetRenderer
{
    /// <summary>
    /// Builds a sitemap from the supplied URLs.
    /// </summary>
    public XmlSitemap Render(IEnumerable<XmlSitemapUrl> urls);
}
