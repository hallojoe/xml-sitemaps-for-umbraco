using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;

public sealed class ExamineUrlRenderer(IXmlSitemapUrlBuilder urlBuilder) : IExamineUrlRenderer
{
    public IEnumerable<XmlSitemapUrl> Render(
        IEnumerable<CmsUrl> urls,
        string defaultLanguageCode,
        IReadOnlyCollection<string> alternativeLanguageCodes,
        string? hostname,
        bool renderAlternateLinks)
    {
        var urlGroups = urls
            .Where(url => string.IsNullOrWhiteSpace(url.UrlPath) is false)
            .GroupBy(GetGroupKey);

        foreach (var urlGroup in urlGroups)
        {
            var groupUrls = urlGroup.ToList();
            var primaryUrl = SelectPrimaryUrl(groupUrls, defaultLanguageCode, alternativeLanguageCodes);
            if (primaryUrl is null)
            {
                continue;
            }

            yield return new XmlSitemapUrl
            {
                Location = BuildUrl(primaryUrl, hostname),
                LastModified = primaryUrl.LastUpdate,
                CultureLinks = RenderCultureLinks(groupUrls, defaultLanguageCode, alternativeLanguageCodes, hostname, renderAlternateLinks)
            };
        }
    }

    private static string GetGroupKey(CmsUrl url)
    {
        if (url.Id is not null)
        {
            return $"id:{url.Id.Value}";
        }

        return "url:" + NormalizeUrlPath(url.UrlPath);
    }

    private static CmsUrl? SelectPrimaryUrl(
        IReadOnlyCollection<CmsUrl> urls,
        string defaultLanguageCode,
        IReadOnlyCollection<string> alternativeLanguageCodes)
    {
        return urls.FirstOrDefault(url => IsCulture(url, defaultLanguageCode)) ??
            alternativeLanguageCodes
                .Select(culture => urls.FirstOrDefault(url => IsCulture(url, culture)))
                .FirstOrDefault(url => url is not null) ??
            urls.FirstOrDefault();
    }

    private List<XHtmlLink> RenderCultureLinks(
        IReadOnlyCollection<CmsUrl> urls,
        string defaultLanguageCode,
        IReadOnlyCollection<string> alternativeLanguageCodes,
        string? hostname,
        bool renderAlternateLinks)
    {
        if (renderAlternateLinks is false)
        {
            return [];
        }

        var orderedLanguageCodes = alternativeLanguageCodes.Except([defaultLanguageCode]).ToList();
        orderedLanguageCodes.Insert(0, defaultLanguageCode);

        return orderedLanguageCodes
            .Select(culture => urls.FirstOrDefault(url => IsCulture(url, culture)))
            .Where(url => url is not null)
            .Select(url => new XHtmlLink
            {
                Href = BuildUrl(url!, hostname),
                HrefLang = url!.Culture!
            })
            .Where(cultureLink => !cultureLink.Href.Contains('#'))
            .ToList();
    }

    private string BuildUrl(CmsUrl url, string? hostname)
    {
        var resolvedHostname = string.IsNullOrWhiteSpace(hostname)
            ? url.Hostname
            : hostname;
        var urlPath = NormalizeUrlPath(url.UrlPath);

        if (string.IsNullOrWhiteSpace(resolvedHostname) is false &&
            Uri.TryCreate(urlPath, UriKind.Absolute, out var absoluteUrl))
        {
            urlPath = absoluteUrl.PathAndQuery;
        }

        return urlBuilder.CombineWithHostname(urlPath, resolvedHostname);
    }

    private static bool IsCulture(CmsUrl url, string culture)
    {
        return string.Equals(url.Culture, culture, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeUrlPath(string urlPath)
    {
        if (urlPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            urlPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            urlPath.StartsWith('/'))
        {
            return urlPath;
        }

        return "/" + urlPath;
    }
}
