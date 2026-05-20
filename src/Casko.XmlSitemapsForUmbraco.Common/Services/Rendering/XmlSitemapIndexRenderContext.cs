namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed record XmlSitemapIndexRenderContext(
    IReadOnlyCollection<string> SitemapAliases,
    string? Hostname,
    XmlSitemapIndexLocationMode LocationMode,
    IReadOnlyDictionary<string, string>? PublicSitemapAliases = null);
