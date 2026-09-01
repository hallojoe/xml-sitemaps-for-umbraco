using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Umbraco.Cms.Core.Services;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Routing;

/// <inheritdoc />
public sealed class ExamineSitemapRootResolver(
    IHostUrlProvider hostUrlProvider,
    IDocumentUrlService documentUrlService) : IExamineSitemapRootResolver
{
    /// <inheritdoc />
    public async Task<ExamineSitemapRoot?> ResolveAsync(
        string path,
        string? hostname = null,
        string? culture = null,
        CancellationToken cancellationToken = default)
    {
        var hostUrl = await ResolveHostUrlAsync(hostname, culture, cancellationToken);
        if (hostUrl is null)
        {
            return null;
        }

        var normalizedPath = NormalizePath(path);
        if (normalizedPath == "/")
        {
            return new ExamineSitemapRoot(hostUrl.Key, hostUrl);
        }

        var contentKey = documentUrlService.GetDocumentKeyByRoute(
            normalizedPath,
            culture,
            hostUrl.Id,
            false);

        return contentKey is null
            ? null
            : new ExamineSitemapRoot(contentKey.Value, hostUrl);
    }

    private async Task<HostUrl?> ResolveHostUrlAsync(
        string? hostname,
        string? culture,
        CancellationToken cancellationToken)
    {
        var hostUrls = (await hostUrlProvider.GetHostUrlsAsync(cancellationToken)).ToList();
        if (hostUrls.Count == 0)
        {
            return null;
        }

        var candidates = string.IsNullOrWhiteSpace(hostname)
            ? hostUrls
            : hostUrls.Where(hostUrl => IsHostnameMatch(hostname, hostUrl.Uri)).ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(culture) is false)
        {
            var cultureHostUrl = candidates.FirstOrDefault(hostUrl =>
                string.Equals(hostUrl.Culture, culture, StringComparison.OrdinalIgnoreCase));
            if (cultureHostUrl is not null)
            {
                return cultureHostUrl;
            }
        }

        return candidates.FirstOrDefault(hostUrl => hostUrl.IsDefaultCulture) ?? candidates.First();
    }

    private static bool IsHostnameMatch(string hostname, Uri hostUrl)
    {
        var normalizedHostname = NormalizeHostname(hostname);
        var normalizedHostUrl = NormalizeHostname(hostUrl.ToString());

        return string.Equals(normalizedHostname, normalizedHostUrl, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeHostname(string hostname)
    {
        if (!Uri.TryCreate(hostname, UriKind.Absolute, out var uri))
        {
            hostname = "https://" + hostname.Trim('/');
            Uri.TryCreate(hostname, UriKind.Absolute, out uri);
        }

        return uri is null
            ? hostname.TrimEnd('/')
            : uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        path = path.Split('?', '#')[0];

        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        return path;
    }
}
