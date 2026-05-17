using Casko.XmlSitemapsForUmbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed class XmlSitemapUrlRenderer(
    ISitemapUrlBuilder urlBuilder,
    IXmlSitemapUrlCultureLinkRenderer cultureLinkRenderer) : IXmlSitemapUrlRenderer
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