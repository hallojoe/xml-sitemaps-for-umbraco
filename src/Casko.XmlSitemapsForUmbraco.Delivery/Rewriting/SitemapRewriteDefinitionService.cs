using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;

public sealed class SitemapRewriteDefinitionService(IOptions<XmlSitemapsOptions> options) : ISitemapRewriteDefinitionService
{
    public IReadOnlyCollection<SitemapRewriteDefinition> GetDefinitions()
    {
        var settings = options.Value;
        var definitions = new List<SitemapRewriteDefinition>();
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (indexKey, indexOptions) in settings.Indexes)
        {
            var definition = CreateDefinition(
                indexKey,
                indexOptions.HostName,
                SitemapRewriteKind.SitemapIndex);

            if (paths.Add(definition.Path))
            {
                definitions.Add(definition);
            }
        }

        foreach (var (sitemapKey, sitemapOptions) in settings.Sitemaps)
        {
            var definition = CreateDefinition(
                sitemapKey,
                sitemapOptions.HostName,
                SitemapRewriteKind.Sitemap);

            if (paths.Add(definition.Path))
            {
                definitions.Add(definition);
            }
        }

        foreach (var (sitemapKey, sitemapOptions) in settings.CustomSitemaps)
        {
            var definition = CreateDefinition(
                sitemapKey,
                sitemapOptions.HostName,
                SitemapRewriteKind.Sitemap);

            if (paths.Add(definition.Path))
            {
                definitions.Add(definition);
            }
        }

        return definitions;
    }

    public bool TryMatch(PathString requestPath, HostString requestHost, out SitemapRewriteDefinition? definition)
    {
        definition = GetDefinitions()
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Path, requestPath.Value, StringComparison.OrdinalIgnoreCase) &&
                IsHostMatch(candidate.HostName, requestHost));

        return definition is not null;
    }

    private static SitemapRewriteDefinition CreateDefinition(string key, string? hostName, SitemapRewriteKind kind)
    {
        return new SitemapRewriteDefinition(
            Path: $"/{key}.xml",
            TargetPath: CreateTargetPath(key, kind),
            Key: key,
            Kind: kind,
            HostName: NormalizeHostName(hostName));
    }

    private static string CreateTargetPath(string key, SitemapRewriteKind kind)
    {
        var escapedKey = Uri.EscapeDataString(key);
        var route = kind == SitemapRewriteKind.SitemapIndex
            ? $"/{XmlSitemapApiConstants.ApiRoute}/index/key"
            : $"/{XmlSitemapApiConstants.ApiRoute}/key";

        return $"{route}?key={escapedKey}";
    }

    private static bool IsHostMatch(string? configuredHostName, HostString requestHost)
    {
        if (string.IsNullOrWhiteSpace(configuredHostName))
        {
            return true;
        }

        var normalizedRequestHost = NormalizeHostName(requestHost.Value);
        return string.Equals(configuredHostName, normalizedRequestHost, StringComparison.OrdinalIgnoreCase);
    }

    public static string? NormalizeHostName(string? hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            return null;
        }

        var value = hostName.Trim().TrimEnd('/');
        if (value.Contains("://") is false)
        {
            value = "https://" + value;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) is false ||
            string.IsNullOrWhiteSpace(uri.Host))
        {
            return hostName.Trim().TrimEnd('/');
        }

        return uri.IsDefaultPort
            ? uri.Host
            : $"{uri.Host}:{uri.Port}";
    }
}
