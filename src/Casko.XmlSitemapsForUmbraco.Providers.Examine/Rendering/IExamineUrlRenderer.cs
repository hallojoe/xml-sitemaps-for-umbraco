using Casko.XmlSitemapsForUmbraco.Common.Providers.Examine.Urls;
using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;

/// <summary>
/// Builds sitemap URLs from CMS URLs supplied by Examine-backed services.
/// </summary>
public interface IExamineUrlRenderer
{
    /// <summary>
    /// Builds sitemap URLs from the supplied CMS URLs.
    /// </summary>
    public IEnumerable<XmlSitemapUrl> Render(
        IEnumerable<CmsUrl> urls,
        string defaultLanguageCode,
        IReadOnlyCollection<string> alternativeLanguageCodes,
        string? hostname,
        bool renderAlternateLinks);
}
