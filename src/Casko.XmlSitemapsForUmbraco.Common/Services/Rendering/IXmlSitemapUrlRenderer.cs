using Casko.XmlSitemapsForUmbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

/// <summary>
/// Builds individual URL entries for XML sitemaps.
/// </summary>
public interface IXmlSitemapUrlRenderer
{
    /// <summary>
    /// Builds a sitemap URL entry for one content item.
    /// </summary>
    public XmlSiteMapUrl Render(IPublishedContent content, XmlSitemapUrlRenderContext context);
}