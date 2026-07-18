using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public sealed class PublishedContentUrlRenderer(
    IPublishedContentUrlBuilder urlBuilder,
    IPublishedContentUrlCultureLinkRenderer cultureLinkRenderer) : IPublishedContentUrlRenderer
{
    public XmlSiteMapUrl Render(IPublishedContent content, XmlSitemapUrlRenderContext context)
    {
        return new XmlSiteMapUrl
        {
            Location = urlBuilder.BuildContentUrl(content, context.DefaultLanguageCode, context.Hostname),
            LastModified = content.UpdateDate,
            CultureLinks = cultureLinkRenderer.Render(content, context)
        };
    }
}