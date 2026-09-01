namespace Casko.XmlSitemapsForUmbraco.Providers.Rendering.Contexts;

public sealed record XmlSitemapUrlRenderContext(
    string DefaultLanguageCode,
    IReadOnlyCollection<string> AlternativeLanguageCodes,
    string? Hostname,
    bool RenderAlternateLinks = true);