namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed record XmlSitemapUrlRenderContext(
    string DefaultLanguageCode,
    IReadOnlyCollection<string> AlternativeLanguageCodes,
    string? Hostname,
    bool RenderAlternateLinks = true);