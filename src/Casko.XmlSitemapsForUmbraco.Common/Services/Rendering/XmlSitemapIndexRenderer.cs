using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed class XmlSitemapIndexRenderer(ISitemapUrlBuilder urlBuilder) : IXmlSitemapIndexRenderer
{
    public XmlSiteMapIndex Render(XmlSitemapIndexRenderContext context)
    {
        return new XmlSiteMapIndex
        {
            Locations = context.SitemapAliases
                .Select(alias => ResolvePublicAlias(alias, context.PublicSitemapAliases))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(publicAlias => new XmlSiteMapIndexLocation
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
