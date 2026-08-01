using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public sealed class PublishedContentUrlRenderer(
    IPublishedContentUrlBuilder urlBuilder,
    IPublishedContentUrlCultureLinkRenderer cultureLinkRenderer) : IPublishedContentUrlRenderer
{
    public XmlSitemapUrl Render(IPublishedContent content, XmlSitemapUrlRenderContext context)
    {
        return new XmlSitemapUrl
        {
            Location = urlBuilder.BuildContentUrl(content, context.DefaultLanguageCode, context.Hostname),
            LastModified = content.UpdateDate,
            CultureLinks = cultureLinkRenderer.Render(content, context)
        };
    }
}