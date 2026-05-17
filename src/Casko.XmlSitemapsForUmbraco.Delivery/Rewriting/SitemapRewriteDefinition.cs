namespace Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;

public sealed record SitemapRewriteDefinition(
    string Path,
    string TargetPath,
    string Key,
    SitemapRewriteKind Kind,
    string? HostName);