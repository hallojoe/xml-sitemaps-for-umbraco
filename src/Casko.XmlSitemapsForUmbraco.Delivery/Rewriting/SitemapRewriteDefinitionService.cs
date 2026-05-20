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
                SitemapPublicName.Resolve(indexKey, indexOptions.PublicName),
                indexOptions.HostName,
                SitemapRewriteKind.SitemapIndex);

            if (paths.Add(GetDefinitionConflictKey(definition)))
            {
                definitions.Add(definition);
            }
        }

        foreach (var (sitemapKey, sitemapOptions) in settings.Sitemaps)
        {
            var definition = CreateDefinition(
                sitemapKey,
                SitemapPublicName.Resolve(sitemapKey, sitemapOptions.PublicName),
                sitemapOptions.HostName,
                SitemapRewriteKind.Sitemap);

            if (paths.Add(GetDefinitionConflictKey(definition)))
            {
                definitions.Add(definition);
            }
        }

        foreach (var (sitemapKey, sitemapOptions) in settings.CustomSitemaps)
        {
            var definition = CreateDefinition(
                sitemapKey,
                SitemapPublicName.Resolve(sitemapKey, sitemapOptions.PublicName),
                sitemapOptions.HostName,
                SitemapRewriteKind.Sitemap);

            if (paths.Add(GetDefinitionConflictKey(definition)))
            {
                definitions.Add(definition);
            }
        }

        return definitions;
    }

    public bool TryMatch(PathString requestPath, HostString requestHost, out SitemapRewriteDefinition? definition)
    {
        var normalizedRequestHost = NormalizeHostName(requestHost.Value);

        definition = GetDefinitions()
            .Where(candidate => string.Equals(candidate.Path, requestPath.Value, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(candidate =>
                candidate.HostName is not null &&
                string.Equals(candidate.HostName, normalizedRequestHost, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(candidate => IsHostMatch(candidate.HostName, normalizedRequestHost));

        return definition is not null;
    }

    private static SitemapRewriteDefinition CreateDefinition(string key, string publicName, string? hostName, SitemapRewriteKind kind)
    {
        return new SitemapRewriteDefinition(
            Path: $"/{publicName}.xml",
            TargetPath: CreateTargetPath(key, kind),
            Key: key,
            PublicName: publicName,
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

    private static string GetDefinitionConflictKey(SitemapRewriteDefinition definition)
    {
        return $"{definition.Path}|{definition.HostName}";
    }

    private static bool IsHostMatch(string? configuredHostName, string? normalizedRequestHost)
    {
        if (string.IsNullOrWhiteSpace(configuredHostName))
        {
            return true;
        }

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
