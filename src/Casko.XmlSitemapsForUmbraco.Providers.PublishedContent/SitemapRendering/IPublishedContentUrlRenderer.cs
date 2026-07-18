using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

/// <summary>
/// Builds individual URL entries for XML sitemaps.
/// </summary>
public interface IPublishedContentUrlRenderer
{
    /// <summary>
    /// Builds a sitemap URL entry for one content item.
    /// </summary>
    public XmlSiteMapUrl Render(IPublishedContent content, XmlSitemapUrlRenderContext context);
}