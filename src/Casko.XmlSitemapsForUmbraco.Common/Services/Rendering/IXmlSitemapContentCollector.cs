using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

/// <summary>
/// Collects content items that should be considered for sitemap rendering.
/// </summary>
public interface IXmlSitemapContentCollector
{
    /// <summary>
    /// Returns the root content and descendants for sitemap rendering.
    /// </summary>
    public IEnumerable<IPublishedContent> Collect(XmlSitemapRenderContext context);
}