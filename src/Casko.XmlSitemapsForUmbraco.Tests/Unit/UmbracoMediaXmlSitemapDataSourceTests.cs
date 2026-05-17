using System.Text;
using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;
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
    private UmbracoMediaXmlSitemapDataSource _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mediaService = Substitute.For<IMediaService>();
        _mediaFileAccessor = Substitute.For<IUmbracoMediaFileAccessor>();
        _sut = new UmbracoMediaXmlSitemapDataSource(
            _mediaService,
            new XmlSitemapStorageNameProvider(),
            _mediaFileAccessor);
    }

    [Test]
    public async Task ReadAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        _mediaService.GetRootMedia().Returns([]);

        var result = await _sut.ReadAsync(CreateKey());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ReadAsync_WhenFileExists_ReturnsStoredXml()
    {
        var refreshedUtc = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);
        var folder = CreateMedia(10, "Xml Sitemaps");
        var file = CreateMedia(20, "sitemap--www-example-com--products.xml", refreshedUtc);
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [file]);
        _mediaFileAccessor.GetFilePath(file).Returns("/media/sitemaps/products.xml");
        _mediaFileAccessor.OpenRead("/media/sitemaps/products.xml")
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes("<urlset />")));

        var result = await _sut.ReadAsync(CreateKey());

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Xml, Is.EqualTo("<urlset />"));
            Assert.That(result.FileName, Is.EqualTo("sitemap--www-example-com--products.xml"));
            Assert.That(result.MediaId, Is.EqualTo(20));
            Assert.That(result.MediaPath, Is.EqualTo("/media/sitemaps/products.xml"));
            Assert.That(result.RefreshedUtc, Is.EqualTo(new DateTimeOffset(refreshedUtc)));
        });
    }

    [Test]
    public async Task ReadAsync_WhenFileHasNoPath_ReturnsNull()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var file = CreateMedia(20, "sitemap--www-example-com--products.xml");
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [file]);
        _mediaFileAccessor.GetFilePath(file).Returns((string?)null);

        var result = await _sut.ReadAsync(CreateKey());

        Assert.That(result, Is.Null);
        _mediaFileAccessor.DidNotReceive().OpenRead(Arg.Any<string>());
    }

    [Test]
    public async Task ReadAsync_WhenFileStreamIsNull_ReturnsNull()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var file = CreateMedia(20, "sitemap--www-example-com--products.xml");
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [file]);
        _mediaFileAccessor.GetFilePath(file).Returns("/media/sitemaps/products.xml");
        _mediaFileAccessor.OpenRead("/media/sitemaps/products.xml").Returns(Stream.Null);

        var result = await _sut.ReadAsync(CreateKey());

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ReadAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        AsyncTestDelegate action = async () => await _sut.ReadAsync(CreateKey(), cancellationTokenSource.Token);

        Assert.That(action, Throws.TypeOf<OperationCanceledException>());
        _mediaService.DidNotReceive().GetRootMedia();
    }

    [Test]
    public async Task WriteAsync_WhenRootFolderDoesNotExist_CreatesRootFolderAndFile()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var file = CreateMedia(20, "sitemap--www-example-com--products.xml");
        _mediaService.GetRootMedia().Returns([]);
        _mediaService
            .CreateMedia(UmbracoMediaXmlSitemapDataSource.RootFolderName, UmbracoConstants.System.Root, UmbracoConstants.Conventions.MediaTypes.Folder)
            .Returns(folder);
        ConfigureChildren(folder, []);
        _mediaService
            .CreateMedia("sitemap--www-example-com--products.xml", folder, UmbracoConstants.Conventions.MediaTypes.File)
            .Returns(file);
        _mediaFileAccessor.GetFilePath(file).Returns("/media/sitemaps/products.xml");

        var result = await _sut.WriteAsync(CreateKey(), "<urlset />");

        Assert.That(result.MediaId, Is.EqualTo(20));
        _mediaService.Received(1).Save(folder);
        _mediaService.Received(1).Save(file);
        _mediaFileAccessor.Received(1).SetInitialFile(
            file,
            "sitemap--www-example-com--products.xml",
            Arg.Any<Stream>());
    }

    [Test]
    public async Task WriteAsync_WhenFileExists_UpdatesExistingFileContent()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var file = CreateMedia(20, "sitemap--www-example-com--products.xml");
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [file]);
        _mediaFileAccessor.GetFilePath(file).Returns("/media/sitemaps/products.xml");

        var result = await _sut.WriteAsync(CreateKey(), "<urlset />");

        Assert.That(result.MediaId, Is.EqualTo(20));
        _mediaFileAccessor.Received(1).UpdateFileContent("/media/sitemaps/products.xml", Arg.Any<Stream>());
        _mediaFileAccessor.DidNotReceive().SetInitialFile(Arg.Any<IMedia>(), Arg.Any<string>(), Arg.Any<Stream>());
        _mediaService.Received(1).Save(file);
    }

    [Test]
    public async Task WriteAsync_WhenFileExistsWithoutPath_SetsInitialFileOnExistingMedia()
    {
        var folder = CreateMedia(10, "Xml Sitemaps");
        var file = CreateMedia(20, "sitemap--www-example-com--products.xml");
        ConfigureRootFolder(folder);
        ConfigureChildren(folder, [file]);
        _mediaFileAccessor.GetFilePath(file).Returns((string?)null, "/media/sitemaps/products.xml");

        var result = await _sut.WriteAsync(CreateKey(), "<urlset />");

        Assert.Multiple(() =>
        {
            Assert.That(result.MediaId, Is.EqualTo(20));
            Assert.That(result.MediaPath, Is.EqualTo("/media/sitemaps/products.xml"));
        });
        _mediaService.DidNotReceive().CreateMedia(
            "sitemap--www-example-com--products.xml",
            folder,
            UmbracoConstants.Conventions.MediaTypes.File);
        _mediaFileAccessor.Received(1).SetInitialFile(
            file,
            "sitemap--www-example-com--products.xml",
            Arg.Any<Stream>());
        _mediaService.Received(1).Save(file);
    }

    [Test]
    public void WriteAsync_WhenXmlIsNull_ThrowsArgumentNullException()
    {
        AsyncTestDelegate action = async () => await _sut.WriteAsync(CreateKey(), null!);

        Assert.That(action, Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void WriteAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        AsyncTestDelegate action = async () => await _sut.WriteAsync(
            CreateKey(),
            "<urlset />",
            cancellationTokenSource.Token);

        Assert.That(action, Throws.TypeOf<OperationCanceledException>());
        _mediaService.DidNotReceive().GetRootMedia();
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
        long total;
        _mediaService.GetPagedChildren(folder.Id, 0, 100, out total)
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
