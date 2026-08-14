using Casko.XmlSitemapsForUmbraco.Providers.Routing;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Routing;

/// <summary>
/// Resolves an Examine sitemap request path to the content key that should seed URL lookup.
/// </summary>
public interface IExamineSitemapRootResolver
{
    /// <summary>
    /// Resolves the content key for a path under a selected host URL.
    /// </summary>
    /// <param name="path">The route path to resolve.</param>
    /// <param name="hostname">Optional hostname used to select a host URL.</param>
    /// <param name="culture">Optional culture used for host and route resolution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved sitemap root, or <c>null</c> when no host or route exists.</returns>
    public Task<ExamineSitemapRoot?> ResolveAsync(
        string path,
        string? hostname = null,
        string? culture = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolved Examine sitemap root information.
/// </summary>
/// <param name="Key">The content key that should seed indexed URL lookup.</param>
/// <param name="HostUrl">The host URL selected for route resolution.</param>
public sealed record ExamineSitemapRoot(Guid Key, HostUrl HostUrl);
