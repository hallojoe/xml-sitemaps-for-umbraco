using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.UrlSets;

public sealed class XmlSitemapUrlSetRenderer : IXmlSitemapUrlSetRenderer
{
    public XmlSitemap Render(IEnumerable<XmlSitemapUrl> urls)
    {
        var urlsByLocation = new Dictionary<string, XmlSitemapUrl>(StringComparer.OrdinalIgnoreCase);
        foreach (var sitemapUrl in urls.Where(url => string.IsNullOrWhiteSpace(url.Location) is false))
        {
            urlsByLocation[sitemapUrl.Location] = sitemapUrl;
        }

        if (urlsByLocation.Count == 0)
        {
            throw new InvalidDataException("No valid URLs found in the input collection.");
        }

        return new XmlSitemap
        {
            Urls = urlsByLocation.Values.ToList()
        };
    }
}
