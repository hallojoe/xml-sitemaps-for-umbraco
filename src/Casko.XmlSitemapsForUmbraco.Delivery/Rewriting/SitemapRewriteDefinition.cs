namespace Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;

public sealed record SitemapRewriteDefinition(
    string Path,
    string TargetPath,
    string Key,
    string PublicName,
    SitemapRewriteKind Kind,
    string? HostName);
