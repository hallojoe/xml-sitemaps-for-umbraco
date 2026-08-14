using Umbraco.Cms.Core.Services;

namespace Casko.XmlSitemapsForUmbraco.Providers.Routing;

/// <summary>
/// Resolves an XML sitemap request path to the HostUrl seed for URL lookup.
/// </summary>
public interface IHostUrlResolver
{
    /// <summary>
    /// Resolves the host url.
    /// </summary>
    /// <param name="path">The route path to resolve.</param>
    /// <param name="hostname">Optional hostname used to select a host URL.</param>
    /// <param name="culture">Optional culture used for host and route resolution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved sitemap root, or <c>null</c> when no host or route exists.</returns>
    public Task<HostUrl?> ResolveAsync(
        string path,
        string? hostname = null,
        string? culture = null,
        CancellationToken cancellationToken = default);
}

