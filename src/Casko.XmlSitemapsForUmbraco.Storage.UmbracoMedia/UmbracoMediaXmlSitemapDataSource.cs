using System.Text;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;

public sealed class UmbracoMediaXmlSitemapDataSource(
    IMediaService mediaService,
    IXmlSitemapStorageNameProvider nameProvider,
    IUmbracoMediaFileAccessor mediaFileAccessor) : IXmlSitemapDataSource
{
    public const string RootFolderName = "Xml Sitemaps";
    private const int PageSize = 100;

    /// <inheritdoc />
    public Task<XmlSitemapStoredDocument?> ReadAsync(
        XmlSitemapStorageKey key,
        CancellationToken cancellationToken = default)
    {
        key.Validate();
        cancellationToken.ThrowIfCancellationRequested();

        var fileName = nameProvider.GetFileName(key);
        var media = FindMedia(fileName);
        if (media is null)
        {
            return Task.FromResult<XmlSitemapStoredDocument?>(null);
        }

        var filePath = mediaFileAccessor.GetFilePath(media);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.FromResult<XmlSitemapStoredDocument?>(null);
        }

        using var stream = mediaFileAccessor.OpenRead(filePath);
        if (stream == Stream.Null)
        {
            return Task.FromResult<XmlSitemapStoredDocument?>(null);
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var xml = reader.ReadToEnd();

        return Task.FromResult<XmlSitemapStoredDocument?>(CreateDocument(key, media, fileName, filePath, xml));
    }

    /// <inheritdoc />
    public Task<XmlSitemapStoredDocument> WriteAsync(
        XmlSitemapStorageKey key,
        string xml,
        CancellationToken cancellationToken = default)
    {
        key.Validate();
        ArgumentNullException.ThrowIfNull(xml);
        cancellationToken.ThrowIfCancellationRequested();

        var fileName = nameProvider.GetFileName(key);
        var folder = EnsureRootFolder();
        var media = FindMedia(fileName, folder.Id);

        if (media is not null)
        {
            var existingPath = mediaFileAccessor.GetFilePath(media);
            if (!string.IsNullOrWhiteSpace(existingPath))
            {
                using var updateStream = CreateStream(xml);
                mediaFileAccessor.UpdateFileContent(existingPath, updateStream);
                mediaService.Save(media);
                return Task.FromResult(CreateDocument(key, media, fileName, existingPath, xml));
            }
        }

        media ??= mediaService.CreateMedia(fileName, folder, Constants.Conventions.MediaTypes.File);

        using var createStream = CreateStream(xml);
        mediaFileAccessor.SetInitialFile(media, fileName, createStream);
        mediaService.Save(media);

        return Task.FromResult(CreateDocument(
            key,
            media,
            fileName,
            mediaFileAccessor.GetFilePath(media),
            xml));
    }

    private IMedia EnsureRootFolder()
    {
        var existing = mediaService.GetRootMedia()
            .FirstOrDefault(media => string.Equals(media.Name, RootFolderName, StringComparison.OrdinalIgnoreCase));

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

    private IMedia? FindMedia(string fileName, int? parentId = null)
    {
        var parent = parentId ?? mediaService.GetRootMedia()
            .FirstOrDefault(media => string.Equals(media.Name, RootFolderName, StringComparison.OrdinalIgnoreCase))
            ?.Id;

        if (parent is null)
        {
            return null;
        }

        long total;
        var pageIndex = 0;
        do
        {
            var children = mediaService.GetPagedChildren(parent.Value, pageIndex, PageSize, out total);
            var match = children.FirstOrDefault(media =>
                string.Equals(media.Name, fileName, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
            {
                return match;
            }

            pageIndex++;
        }
        while (pageIndex * PageSize < total);

        return null;
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
}
