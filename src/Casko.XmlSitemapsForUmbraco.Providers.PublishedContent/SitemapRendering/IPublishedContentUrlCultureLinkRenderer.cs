using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

/// <summary>
/// Builds alternate culture links for sitemap URL entries.
/// </summary>
public interface IPublishedContentUrlCultureLinkRenderer
{
    /// <summary>
    /// Builds alternate culture links for one content item.
    /// </summary>
    public List<XHtmlLink> Render(IPublishedContent content, XmlSitemapUrlRenderContext context);
}