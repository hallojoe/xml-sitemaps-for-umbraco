using System.Text;
using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.Configuration;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using UmbracoConstants = Umbraco.Cms.Core.Constants;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class UmbracoMediaXmlSitemapDataSourceTests
{
    private IMediaService _mediaService = null!;
    private IUmbracoMediaFileAccessor _mediaFileAccessor = null!;
    private ServiceProvider _serviceProvider = null!;
    private UmbracoMediaXmlSitemapDataSource _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mediaService = Substitute.For<IMediaService>();
        _mediaFileAccessor = Substitute.For<IUmbracoMediaFileAccessor>();
        var services = new ServiceCollection();
        services.AddHybridCache();
        _serviceProvider = services.BuildServiceProvider();
        _sut = new UmbracoMediaXmlSitemapDataSource(
            _mediaService,
            new XmlSitemapStorageNameProvider(),
            _mediaFileAccessor,
            _serviceProvider.GetRequiredService<HybridCache>(),
            Options.Create(new XmlSitemapStorageOptions()),
            TimeProvider.System);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public async Task ReadAsync_WhenNoStoredFileExists_ReturnsNull()
    {
        _mediaService.GetRootMedia().Returns([]);

        var result = await _sut.ReadAsync(CreateKey());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ReadAsync_WhenLegacyFileExists_ReturnsStoredXml()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var legacyFile = CreateMedia(20, "sitemap--www-example-com--products.xml");
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [legacyFile]);
        _mediaFileAccessor.GetFilePath(legacyFile).Returns("/media/sitemaps/products.xml");
        _mediaFileAccessor.OpenRead("/media/sitemaps/products.xml")
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes("<urlset />")));

        var result = await _sut.ReadAsync(CreateKey());

        Assert.That(result?.Xml, Is.EqualTo("<urlset />"));
        Assert.That(result?.MediaId, Is.EqualTo(20));
    }

    [Test]
    public async Task ReadAsync_WhenVersionedFilesExist_ReturnsLatestVersion()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var older = CreateMedia(20, "sitemap--www-example-com--products--202608181000000000000Z--a.xml");
        var latest = CreateMedia(21, "sitemap--www-example-com--products--202608181100000000000Z--b.xml");
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [older, latest]);
        _mediaFileAccessor.GetFilePath(older).Returns("/media/sitemaps/older.xml");
        _mediaFileAccessor.GetFilePath(latest).Returns("/media/sitemaps/latest.xml");
        _mediaFileAccessor.OpenRead("/media/sitemaps/latest.xml")
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes("<latest />")));

        var result = await _sut.ReadAsync(CreateKey());

        Assert.Multiple(() =>
        {
            Assert.That(result?.Xml, Is.EqualTo("<latest />"));
            Assert.That(result?.MediaId, Is.EqualTo(21));
        });
    }

    [Test]
    public async Task ReadAsync_CachesResolvedVersionLocator()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var latest = CreateMedia(21, "sitemap--www-example-com--products--202608181100000000000Z--b.xml");
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [latest]);
        _mediaFileAccessor.GetFilePath(latest).Returns("/media/sitemaps/latest.xml");
        _mediaFileAccessor.OpenRead("/media/sitemaps/latest.xml")
            .Returns(_ => new MemoryStream(Encoding.UTF8.GetBytes("<latest />")));

        await _sut.ReadAsync(CreateKey());
        await _sut.ReadAsync(CreateKey());

        _mediaService.Received(1).GetPagedChildren(folder.Id, 0, 100, out Arg.Any<long>());
    }

    [Test]
    public async Task ReadAsync_WhenCachedVersionCannotBeRead_FallsBackToPreviousVersion()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var older = CreateMedia(20, "sitemap--www-example-com--products--202608181000000000000Z--a.xml");
        var latest = CreateMedia(21, "sitemap--www-example-com--products--202608181100000000000Z--b.xml");
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [older, latest]);
        _mediaFileAccessor.GetFilePath(older).Returns("/media/sitemaps/older.xml");
        _mediaFileAccessor.GetFilePath(latest).Returns("/media/sitemaps/latest.xml");
        _mediaFileAccessor.OpenRead("/media/sitemaps/latest.xml").Returns(Stream.Null);
        _mediaFileAccessor.OpenRead("/media/sitemaps/older.xml")
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes("<older />")));

        var result = await _sut.ReadAsync(CreateKey());

        Assert.That(result?.Xml, Is.EqualTo("<older />"));
        Assert.That(result?.MediaId, Is.EqualTo(20));
    }

    [Test]
    public async Task WriteAsync_CreatesImmutableVersionWithoutUpdatingExistingFile()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var createdFile = CreateMedia(20, "versioned-file.xml");
        _mediaService.GetRootMedia().Returns([folder]);
        ConfigureChildren(folder, []);
        _mediaService.CreateMedia(
                Arg.Is<string>(name => name.StartsWith("sitemap--www-example-com--products--", StringComparison.Ordinal)),
                folder,
                UmbracoConstants.Conventions.MediaTypes.File)
            .Returns(createdFile);
        _mediaFileAccessor.GetFilePath(createdFile).Returns("/media/sitemaps/products.xml");

        var result = await _sut.WriteAsync(CreateKey(), "<urlset />");

        Assert.Multiple(() =>
        {
            Assert.That(result.MediaId, Is.EqualTo(20));
            Assert.That(result.FileName, Does.StartWith("sitemap--www-example-com--products--"));
        });
        _mediaFileAccessor.Received(1).SetInitialFile(createdFile, Arg.Any<string>(), Arg.Any<Stream>());
        _mediaService.Received(1).Save(createdFile);
    }

    [Test]
    public async Task WriteAsync_CleansUpExpiredVersionsBeyondTheTwoNewest()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var first = CreateMedia(20, "sitemap--www-example-com--products--202608181200000000000Z--a.xml", DateTime.UtcNow);
        var second = CreateMedia(21, "sitemap--www-example-com--products--202608181100000000000Z--b.xml", DateTime.UtcNow);
        var expired = CreateMedia(22, "sitemap--www-example-com--products--202608180900000000000Z--c.xml", DateTime.UtcNow.AddHours(-1));
        var createdFile = CreateMedia(23, "sitemap--www-example-com--products--999912312359599999999Z--d.xml");
        _mediaService.GetRootMedia().Returns([folder]);
        ConfigureChildren(folder, [createdFile, first, second, expired]);
        _mediaService.CreateMedia(Arg.Any<string>(), folder, UmbracoConstants.Conventions.MediaTypes.File).Returns(createdFile);
        _mediaFileAccessor.GetFilePath(createdFile).Returns("/media/sitemaps/products.xml");
        _sut = CreateSut(new XmlSitemapStorageOptions { VersionCleanupAfterSeconds = 600 });

        await _sut.WriteAsync(CreateKey(), "<urlset />");

        _mediaService.Received(1).Delete(expired);
        _mediaService.DidNotReceive().Delete(first);
        _mediaService.DidNotReceive().Delete(second);
    }

    [Test]
    public void WriteAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        AsyncTestDelegate action = async () => await _sut.WriteAsync(CreateKey(), "<urlset />", cancellationTokenSource.Token);

        Assert.That(action, Throws.TypeOf<OperationCanceledException>());
        _mediaService.DidNotReceive().GetRootMedia();
    }

    private UmbracoMediaXmlSitemapDataSource CreateSut(XmlSitemapStorageOptions options)
    {
        return new UmbracoMediaXmlSitemapDataSource(
            _mediaService,
            new XmlSitemapStorageNameProvider(),
            _mediaFileAccessor,
            _serviceProvider.GetRequiredService<HybridCache>(),
            Options.Create(options),
            TimeProvider.System);
    }

    private static XmlSitemapStorageKey CreateKey()
    {
        return new XmlSitemapStorageKey(XmlSitemapDocumentKind.Sitemap, "products", "www.example.com");
    }

    private void ConfigureRootFolder(IMedia folder)
    {
        _mediaService.GetRootMedia().Returns([folder]);
    }

    private void ConfigureChildren(IMedia folder, IEnumerable<IMedia> children)
    {
        _mediaService.GetPagedChildren(folder.Id, 0, 100, out Arg.Any<long>())
            .Returns(callInfo =>
            {
                var childList = children.ToList();
                callInfo[3] = (long)childList.Count;
                return childList;
            });
    }

    private static IMedia CreateMedia(int id, string name, DateTime? updateDate = null)
    {
        var media = Substitute.For<IMedia>();
        media.Id.Returns(id);
        media.Key.Returns(Guid.NewGuid());
        media.Name.Returns(name);
        media.UpdateDate.Returns(updateDate ?? DateTime.UtcNow);
        return media;
    }
}
