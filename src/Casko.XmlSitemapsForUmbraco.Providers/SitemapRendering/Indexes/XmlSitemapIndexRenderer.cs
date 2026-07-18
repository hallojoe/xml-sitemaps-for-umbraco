using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;

namespace Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Indexes;

public sealed class XmlSitemapIndexRenderer(IXmlSitemapUrlBuilder urlBuilder) : IXmlSitemapIndexRenderer
{
    public XmlSitemapIndex Render(XmlSitemapIndexRenderContext context)
    {
        return new XmlSitemapIndex
        {
            Locations = context.SitemapAliases
                .Select(alias => ResolvePublicAlias(alias, context.PublicSitemapAliases))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(publicAlias => new XmlSitemapIndexLocation
                {
                    Location = context.LocationMode == XmlSitemapIndexLocationMode.LegacyXmlFile
                        ? urlBuilder.BuildLegacySitemapFileUrl(publicAlias, context.Hostname)
                        : urlBuilder.BuildSitemapApiUrl(publicAlias, context.Hostname)
                })
                .ToList()
        };
    }

    private static string ResolvePublicAlias(string alias, IReadOnlyDictionary<string, string>? publicAliases)
    {
        return publicAliases is not null && publicAliases.TryGetValue(alias, out var publicAlias)
            ? publicAlias
            : alias;
    }
}
