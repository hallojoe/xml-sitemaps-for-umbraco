namespace Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;

public record XmlSitemapRenderContext<T>(
    IReadOnlyCollection<T> RootContents,
    string DefaultLanguageCode,
    IReadOnlyCollection<string> AlternativeLanguageCodes,
    string? Hostname,
    bool RenderAlternateLinks = true,
    Func<T, bool>? ShouldIncludeContent = null);
