namespace Casko.XmlSitemapsForUmbraco.Providers.Rendering.Contexts;

public sealed record XmlSitemapIndexRenderContext(
    IReadOnlyCollection<string> SitemapAliases,
    string? Hostname,
    XmlSitemapIndexLocationMode LocationMode,
    IReadOnlyDictionary<string, string>? PublicSitemapAliases = null);
