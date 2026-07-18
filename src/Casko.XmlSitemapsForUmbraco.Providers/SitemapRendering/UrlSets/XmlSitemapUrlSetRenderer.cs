using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.UrlSets;

public sealed class XmlSitemapUrlSetRenderer : IXmlSitemapUrlSetRenderer
{
    public XmlSiteMap Render(IEnumerable<XmlSiteMapUrl> urls)
    {
        var urlsByLocation = new Dictionary<string, XmlSiteMapUrl>(StringComparer.OrdinalIgnoreCase);
        foreach (var sitemapUrl in urls.Where(url => string.IsNullOrWhiteSpace(url.Location) is false))
        {
            urlsByLocation[sitemapUrl.Location] = sitemapUrl;
        }

        if (urlsByLocation.Count == 0)
        {
            throw new InvalidDataException("No valid URLs found in the input collection.");
        }

        return new XmlSiteMap
        {
            Urls = urlsByLocation.Values.ToList()
        };
    }
}
