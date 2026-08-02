namespace Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;

public class XmlSitemapUrlBuilder : IXmlSitemapUrlBuilder
{
    public string BuildSitemapApiUrl(string sitemapAlias, string? hostname)
    {
        var relativeUrl = $"/{XmlSitemapApiConstants.ApiRoute}?name={Uri.EscapeDataString(sitemapAlias)}";
        return CombineWithHostname(relativeUrl, hostname);
    }

    public string BuildLegacySitemapFileUrl(string sitemapAlias, string? hostname)
    {
        return CombineWithHostname($"/{sitemapAlias}.xml", hostname);
    }

    public string CombineWithHostname(string relativeUrl, string? hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname))
        {
            return relativeUrl;
        }

        return $"{hostname.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
    }
}
