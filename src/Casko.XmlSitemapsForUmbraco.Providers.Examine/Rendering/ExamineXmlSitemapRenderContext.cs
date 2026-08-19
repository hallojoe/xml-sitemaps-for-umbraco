using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;

public sealed record ExamineXmlSitemapRenderContext(
    IReadOnlyCollection<CmsUrl> Urls,
    string DefaultLanguageCode,
    IReadOnlyCollection<string> AlternativeLanguageCodes,
    string? Hostname,
    bool RenderAlternateLinks = true,
    bool UseHostnameForCultureLinks = false);
