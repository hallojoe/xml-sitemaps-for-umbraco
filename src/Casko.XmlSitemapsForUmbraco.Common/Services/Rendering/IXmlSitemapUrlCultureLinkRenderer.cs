using Casko.XmlSitemapsForUmbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

/// <summary>
/// Builds alternate culture links for sitemap URL entries.
/// </summary>
public interface IXmlSitemapUrlCultureLinkRenderer
{
    /// <summary>
    /// Builds alternate culture links for one content item.
    /// </summary>
    public List<XHtmlLink> Render(IPublishedContent content, XmlSitemapUrlRenderContext context);
}