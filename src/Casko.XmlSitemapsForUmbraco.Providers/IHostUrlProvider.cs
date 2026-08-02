using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Casko.XmlSitemapsForUmbraco.Providers;

/// <summary>
/// A record representing a host URL with optional culture, ID, and key.
/// Used to store host URL nodes from Umbraco.
/// </summary>
/// <param name="Uri"></param>
/// <param name="Culture"></param>
/// <param name="Id"></param>
/// <param name="Key"></param>
/// <param name="IsDefaultCulture"></param>
public record HostUrl(Uri Uri, string Culture, int Id, Guid Key, bool IsDefaultCulture);

/// <summary>
/// IHostUrlProvider is responsible for providing the host URL nodes from Umbraco.
/// </summary>
public interface IHostUrlProvider
{
    /// <summary>
    /// Resolve the host urls from Umbraco and return them as a list of HostUrl records.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<HostUrl>> GetHostUrlsAsync(CancellationToken cancellationToken = default);    
}

/// <summary>
/// Default implementation of IHostUrlProvider. Uses <see cref="IDomainService"/> to resolve the host URLs from Umbraco.
/// </summary>
public class HostUrlProvider(
    IOptions<WebRoutingSettings> webRoutingSettings,
    IDomainService domainService, 
    ILanguageService languageService,
    IContentService contentService) : IHostUrlProvider
{
    /// <inheritdoc/>
    public async Task<IEnumerable<HostUrl>> GetHostUrlsAsync(CancellationToken cancellationToken = default)
    {
        var defaultCulture = (await languageService.GetDefaultLanguageAsync())?.IsoCode;
        var domains = (await domainService.GetAllAsync(includeWildcards: false)).ToList();
        if (domains.Count == 0)
        {
            return CreateFallbackHostUrls(defaultCulture);
        }

        var hostUrls = domains
            .OrderBy(domain => domain.SortOrder)
            .Select(domain => CreateHostUrl(domain, defaultCulture))
            .Where(hostUrl => hostUrl is not null)
            .Select(hostUrl => hostUrl!)
            .ToList();

        return hostUrls.Count > 0
            ? hostUrls
            : CreateFallbackHostUrls(defaultCulture);
    }

    private IEnumerable<HostUrl> CreateFallbackHostUrls(string? defaultCulture)
    {
        var content = contentService
            .GetPagedChildren(-1, 0, 1, out var totalChildren, null, null, null, true)
            .FirstOrDefault();

        var hostUrl = content is null
            ? null
            : CreateHostUrl(content, defaultCulture);

        return hostUrl is null
            ? []
            : [hostUrl];
    }

    private HostUrl? CreateHostUrl(IDomain domain, string? defaultCulture)
    {
        var rootContent = domain.RootContentId is null
            ? null
            : contentService.GetById(domain.RootContentId.Value);
        var uri = ResolveUri(domain.DomainName, webRoutingSettings.Value.UmbracoApplicationUrl);
        var culture = ResolveCulture(domain.LanguageIsoCode, defaultCulture);

        if (rootContent is null || uri is null || culture is null)
        {
            return null;
        }

        return new HostUrl(
            uri,
            culture,
            rootContent.Id,
            rootContent.Key,
            IsDefaultCulture(culture, defaultCulture));
    }

    private HostUrl? CreateHostUrl(IContent content, string? defaultCulture)
    {
        var uri = ResolveUri(domainName: null, webRoutingSettings.Value.UmbracoApplicationUrl);
        var culture = ResolveCulture(culture: null, defaultCulture);

        if (uri is null || culture is null)
        {
            return null;
        }

        return new HostUrl(
            uri,
            culture,
            content.Id,
            content.Key,
            IsDefaultCulture(culture, defaultCulture));
    }

    internal static Uri? ResolveUri(string? domainName, string? fallbackApplicationUrl)
    {
        if (Uri.TryCreate(domainName, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        if (string.IsNullOrWhiteSpace(domainName) || domainName.StartsWith('/'))
        {
            return ResolveUriFromApplicationUrl(domainName, fallbackApplicationUrl);
        }

        if (domainName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Uri.TryCreate($"https://{domainName.Trim()}", UriKind.Absolute, out var hostUri)
            ? hostUri
            : null;
    }

    private static Uri? ResolveUriFromApplicationUrl(string? domainName, string? fallbackApplicationUrl)
    {
        if (!Uri.TryCreate(fallbackApplicationUrl, UriKind.Absolute, out var fallbackUri))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(domainName))
        {
            return fallbackUri;
        }

        return Uri.TryCreate(fallbackUri.GetLeftPart(UriPartial.Authority) + domainName, UriKind.Absolute, out var pathUri)
            ? pathUri
            : null;
    }

    private static string? ResolveCulture(string? culture, string? defaultCulture)
    {
        return string.IsNullOrWhiteSpace(culture)
            ? defaultCulture
            : culture;
    }

    private static bool IsDefaultCulture(string culture, string? defaultCulture)
    {
        return string.Equals(culture, defaultCulture, StringComparison.OrdinalIgnoreCase);
    }
}
