using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Indexes;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public sealed class PublishedContentIndexRenderer(IXmlSitemapIndexRenderer sitemapIndexRenderer) : IPublishedContentIndexRenderer
{
    public XmlSitemapIndex Render(XmlSitemapIndexRenderContext context)
    {
        return sitemapIndexRenderer.Render(context);
    }
}
