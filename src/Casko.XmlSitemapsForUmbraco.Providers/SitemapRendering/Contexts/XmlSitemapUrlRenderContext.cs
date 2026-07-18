namespace Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;

public sealed record XmlSitemapUrlRenderContext(
    string DefaultLanguageCode,
    IReadOnlyCollection<string> AlternativeLanguageCodes,
    string? Hostname,
    bool RenderAlternateLinks = true);