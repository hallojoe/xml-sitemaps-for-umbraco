using System.Text;
using Casko.XmlSitemapsForUmbraco.Storage.Configuration;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;

public sealed class UmbracoMediaXmlSitemapDataSource(
    IMediaService mediaService,
    IXmlSitemapStorageNameProvider nameProvider,
    IUmbracoMediaFileAccessor mediaFileAccessor,
    HybridCache cache,
    IOptions<XmlSitemapStorageOptions> storageOptions,
    TimeProvider timeProvider) : IXmlSitemapDataSource
{
    public const string RootFolderName = "Xml Sitemaps";
    private const int PageSize = 100;
    private const int RetainedVersionCount = 2;
    private static readonly HybridCacheEntryOptions CacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(1),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };

    /// <inheritdoc />
    public async Task<XmlSitemapStoredDocument?> ReadAsync(
        XmlSitemapStorageKey key,
        CancellationToken cancellationToken = default)
    {
        key.Validate();
        cancellationToken.ThrowIfCancellationRequested();

        var fileName = nameProvider.GetFileName(key);
        var cacheKey = GetCacheKey(key);
        var version = await cache.GetOrCreateAsync(
            cacheKey,
            _ => ValueTask.FromResult(ResolveLatestVersion(fileName)),
            CacheEntryOptions,
            cancellationToken: cancellationToken);

        var document = version is null ? null : ReadVersion(key, version);
        if (document is not null || version is null)
        {
            return document;
        }

        await cache.RemoveAsync(cacheKey, cancellationToken);
        var fallback = ResolveLatestVersion(fileName, version.MediaKey);
        if (fallback is null)
        {
            return null;
        }

        await cache.SetAsync(cacheKey, fallback, CacheEntryOptions, cancellationToken: cancellationToken);
        return ReadVersion(key, fallback);
    }

    /// <inheritdoc />
    public async Task<XmlSitemapStoredDocument> WriteAsync(
        XmlSitemapStorageKey key,
        string xml,
        CancellationToken cancellationToken = default)
    {
        key.Validate();
        ArgumentNullException.ThrowIfNull(xml);
        cancellationToken.ThrowIfCancellationRequested();

        var logicalFileName = nameProvider.GetFileName(key);
        var folder = EnsureRootFolder();
        var versionedFileName = CreateVersionedFileName(logicalFileName, timeProvider.GetUtcNow());
        var media = mediaService.CreateMedia(versionedFileName, folder, Constants.Conventions.MediaTypes.File);

        using var createStream = CreateStream(xml);
        mediaFileAccessor.SetInitialFile(media, versionedFileName, createStream);
        mediaService.Save(media);

        var mediaPath = mediaFileAccessor.GetFilePath(media);
        var version = CreateVersion(media, versionedFileName, mediaPath);
        await cache.SetAsync(GetCacheKey(key), version, CacheEntryOptions, cancellationToken: cancellationToken);
        CleanupObsoleteVersions(folder.Id, logicalFileName);

        return CreateDocument(key, media, versionedFileName, mediaPath, xml);
    }

    private XmlSitemapStoredDocument? ReadVersion(
        XmlSitemapStorageKey key,
        StoredSitemapMediaVersion version)
    {
        using var stream = mediaFileAccessor.OpenRead(version.MediaPath);
        if (stream == Stream.Null)
        {
            return null;
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var xml = reader.ReadToEnd();

        return new XmlSitemapStoredDocument(
            key,
            version.MediaKey,
            version.MediaId,
            version.FileName,
            version.MediaPath,
            xml,
            version.PublishedUtc);
    }

    private StoredSitemapMediaVersion? ResolveLatestVersion(string logicalFileName, Guid? excludedMediaKey = null)
    {
        var folder = FindRootFolder();
        if (folder is null)
        {
            return null;
        }

        var children = GetChildren(folder.Id)
            .Where(media => media.Key != excludedMediaKey)
            .ToArray();
        var candidates = children
            .Where(media => IsVersionOf(media.Name ?? string.Empty, logicalFileName))
            .OrderByDescending(media => media.Name, StringComparer.Ordinal)
            .ToArray();

        if (candidates.Length == 0)
        {
            candidates = children
                .Where(media => string.Equals(media.Name, logicalFileName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(GetRefreshedUtc)
                .ToArray();
        }

        foreach (var candidate in candidates)
        {
            var path = mediaFileAccessor.GetFilePath(candidate);
            if (!string.IsNullOrWhiteSpace(path))
            {
                return CreateVersion(candidate, candidate.Name ?? logicalFileName, path);
            }
        }

        return null;
    }

    private void CleanupObsoleteVersions(int folderId, string logicalFileName)
    {
        var cleanupAfterSeconds = storageOptions.Value.VersionCleanupAfterSeconds;
        if (cleanupAfterSeconds <= 0)
        {
            return;
        }

        var cutoff = timeProvider.GetUtcNow().AddSeconds(-cleanupAfterSeconds);
        var obsoleteVersions = GetChildren(folderId)
            .Where(media => IsVersionOf(media.Name ?? string.Empty, logicalFileName))
            .OrderByDescending(media => media.Name, StringComparer.Ordinal)
            .Skip(RetainedVersionCount)
            .Where(media => GetRefreshedUtc(media) is { } refreshedUtc && refreshedUtc <= cutoff);

        foreach (var media in obsoleteVersions)
        {
            mediaService.Delete(media);
        }
    }

    private IMedia EnsureRootFolder()
    {
        var existing = FindRootFolder();
        if (existing is not null)
        {
            return existing;
        }

        var folder = mediaService.CreateMedia(
            RootFolderName,
            Constants.System.Root,
            Constants.Conventions.MediaTypes.Folder);
        mediaService.Save(folder);

        return folder;
    }

    private IMedia? FindRootFolder()
    {
        return mediaService.GetRootMedia()
            .FirstOrDefault(media => string.Equals(media.Name, RootFolderName, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<IMedia> GetChildren(int parentId)
    {
        long total;
        var pageIndex = 0;
        do
        {
            var children = mediaService.GetPagedChildren(parentId, pageIndex, PageSize, out total);
            foreach (var child in children)
            {
                yield return child;
            }

            pageIndex++;
        }
        while (pageIndex * PageSize < total);
    }

    private static bool IsVersionOf(string candidateFileName, string logicalFileName)
    {
        var extension = Path.GetExtension(logicalFileName);
        var stem = Path.GetFileNameWithoutExtension(logicalFileName);
        return candidateFileName.StartsWith($"{stem}--", StringComparison.OrdinalIgnoreCase) &&
               candidateFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateVersionedFileName(string logicalFileName, DateTimeOffset publishedUtc)
    {
        var extension = Path.GetExtension(logicalFileName);
        var stem = Path.GetFileNameWithoutExtension(logicalFileName);
        return $"{stem}--{publishedUtc:yyyyMMddHHmmssfffffff}Z--{Guid.NewGuid():N}{extension}";
    }

    private static string GetCacheKey(XmlSitemapStorageKey key)
    {
        return $"xml-sitemaps:media-version:{key.Kind}:{key.HostName ?? "default"}:{key.Alias}";
    }

    private static StoredSitemapMediaVersion CreateVersion(IMedia media, string fileName, string? mediaPath)
    {
        return new StoredSitemapMediaVersion(
            media.Key,
            media.Id,
            fileName,
            mediaPath ?? string.Empty,
            GetRefreshedUtc(media));
    }

    private static XmlSitemapStoredDocument CreateDocument(
        XmlSitemapStorageKey key,
        IMedia media,
        string fileName,
        string? mediaPath,
        string xml)
    {
        return new XmlSitemapStoredDocument(
            key,
            media.Key,
            media.Id,
            fileName,
            mediaPath,
            xml,
            GetRefreshedUtc(media));
    }

    private static MemoryStream CreateStream(string xml)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(xml));
    }

    private static DateTimeOffset? GetRefreshedUtc(IMedia media)
    {
        if (media.UpdateDate == default)
        {
            return null;
        }

        return media.UpdateDate.Kind switch
        {
            DateTimeKind.Local => new DateTimeOffset(media.UpdateDate).ToUniversalTime(),
            DateTimeKind.Utc => new DateTimeOffset(media.UpdateDate),
            _ => new DateTimeOffset(DateTime.SpecifyKind(media.UpdateDate, DateTimeKind.Utc))
        };
    }

    private sealed record StoredSitemapMediaVersion(
        Guid MediaKey,
        int MediaId,
        string FileName,
        string MediaPath,
        DateTimeOffset? PublishedUtc);
}
