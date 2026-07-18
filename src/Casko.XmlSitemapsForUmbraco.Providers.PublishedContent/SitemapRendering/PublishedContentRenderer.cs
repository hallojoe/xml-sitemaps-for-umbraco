using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.UrlSets;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public sealed class PublishedContentRenderer(
    IPublishedContentCollector contentCollector,
    IPublishedContentUrlRenderer urlRenderer,
    IXmlSitemapUrlSetRenderer urlSetRenderer) : IPublishedContentRenderer
{
    public PublishedContentRenderer(
        IPublishedContentCollector contentCollector,
        IPublishedContentUrlRenderer urlRenderer)
        : this(contentCollector, urlRenderer, new XmlSitemapUrlSetRenderer())
    {
    }

    public XmlSitemap Render(PublishedContentRenderContext context)
    {
        var includedContentItems = contentCollector
            .Collect(context)
            .Where(content => context.ShouldIncludeContent?.Invoke(content) is not false)
            .ToList();

        var urlContext = new XmlSitemapUrlRenderContext(
            context.DefaultLanguageCode,
            context.AlternativeLanguageCodes,
            context.Hostname,
            context.RenderAlternateLinks);

        return urlSetRenderer.Render(includedContentItems.Select(content => urlRenderer.Render(content, urlContext)));
    }
}
