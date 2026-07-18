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

        var lastIndexOfSlash = hostname.LastIndexOf('/');
        if ((hostname.StartsWith("http", StringComparison.OrdinalIgnoreCase) && lastIndexOfSlash > 7) ||
            (!hostname.StartsWith("http", StringComparison.OrdinalIgnoreCase) && lastIndexOfSlash > 0))
        {
            hostname = hostname[..lastIndexOfSlash];
        }

        return $"{hostname.TrimEnd('/')}{relativeUrl}";
    }
}
