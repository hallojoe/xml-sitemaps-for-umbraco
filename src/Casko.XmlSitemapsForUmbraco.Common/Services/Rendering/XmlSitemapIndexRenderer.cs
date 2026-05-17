using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed class XmlSitemapIndexRenderer(ISitemapUrlBuilder urlBuilder) : IXmlSitemapIndexRenderer
{
    public XmlSiteMapIndex Render(XmlSitemapIndexRenderContext context)
    {
        return new XmlSiteMapIndex
        {
            Locations = context.SitemapAliases
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(alias => new XmlSiteMapIndexLocation
                {
                    Location = context.LocationMode == XmlSitemapIndexLocationMode.LegacyXmlFile
                        ? urlBuilder.BuildLegacySitemapFileUrl(alias, context.Hostname)
                        : urlBuilder.BuildSitemapApiUrl(alias, context.Hostname)
                })
                .ToList()
        };
    }
}