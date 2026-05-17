using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Services;
using Casko.XmlSitemapsForUmbraco.Common.Services.Cms;
using Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Models.Serialization;
using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class StoredXmlSitemapServiceTests
{
    private DefaultXmlSiteMapService _defaultXmlSiteMapService = null!;
    private IXmlSitemapDataSource _xmlSitemapDataSource = null!;
    private IXmlSitemapXmlDeserializer _xmlSitemapXmlDeserializer = null!;
    private IXmlSitemapStorageRefreshService _xmlSitemapStorageRefreshService = null!;
    private StoredXmlSitemapService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _defaultXmlSiteMapService = Substitute.For<DefaultXmlSiteMapService>(
            Options.Create(new XmlSitemapsOptions()),
            Substitute.For<ICmsContentService>(),
            Substitute.For<IXmlSitemapRenderer>(),
            Substitute.For<IXmlSitemapIndexRenderer>(),
            Array.Empty<IXmlSitemapCustomProvider>());
        _xmlSitemapDataSource = Substitute.For<IXmlSitemapDataSource>();
        _xmlSitemapXmlDeserializer = Substitute.For<IXmlSitemapXmlDeserializer>();
        _xmlSitemapStorageRefreshService = Substitute.For<IXmlSitemapStorageRefreshService>();
        _sut = CreateService(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions
                {
                    HostName = "www.example.com"
                }
            },
            Indexes =
            {
                ["main"] = new SitemapIndexOptions
                {
                    HostName = "index.example.com",
                    Sitemaps = ["products"]
                }
            },
            CustomSitemaps =
            {
                ["external-products"] = new CustomSitemapOptions
                {
                    ProviderAlias = "external-products-provider",
                    HostName = "custom.example.com"
                }
            }
        });
    }

    [Test]
    public void GetConfiguredAsync_WhenKeyIsMissing_ThrowsInvalidOperationException()
    {
        AsyncTestDelegate action = async () => await _sut.GetConfiguredAsync("missing");

        Assert.That(action, Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredSitemapExists_ReturnsDeserializedSitemap()
    {
        var storedSitemap = new XmlSiteMap();
        _xmlSitemapDataSource
            .ReadAsync(Arg.Is<XmlSitemapStorageKey>(key =>
                key.Kind == XmlSitemapDocumentKind.Sitemap &&
                key.Alias == "products" &&
                key.HostName == "www.example.com"))
            .Returns(new XmlSitemapStoredDocument(
                CreateSitemapStorageKey(),
                Guid.NewGuid(),
                42,
                "sitemap--www-example-com--products.xml",
                "/media/sitemaps/products.xml",
                "<urlset />",
                DateTimeOffset.UtcNow));
        _xmlSitemapXmlDeserializer.Deserialize<XmlSiteMap>("<urlset />").Returns(storedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(storedSitemap));
        await _defaultXmlSiteMapService.DidNotReceive().GetConfiguredAsync(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.DidNotReceive().RefreshConfiguredAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredSitemapIsMissing_RefreshesSitemap()
    {
        var generatedSitemap = new XmlSiteMap();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(Task.FromResult<XmlSitemapStoredDocument?>(null));
        _xmlSitemapStorageRefreshService.RefreshConfiguredAsync("products").Returns(generatedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(generatedSitemap));
        await _xmlSitemapStorageRefreshService.Received(1).RefreshConfiguredAsync("products");
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredSitemapIsStale_RefreshesSitemap()
    {
        var generatedSitemap = new XmlSiteMap();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(CreateStoredDocument(CreateSitemapStorageKey(), "<old-urlset />", DateTimeOffset.UtcNow.AddHours(-2)));
        _xmlSitemapStorageRefreshService.RefreshConfiguredAsync("products").Returns(generatedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(generatedSitemap));
        _xmlSitemapXmlDeserializer.DidNotReceive().Deserialize<XmlSiteMap>(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.Received(1).RefreshConfiguredAsync("products");
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStaleCheckIsDisabled_ReturnsStoredSitemap()
    {
        var storedSitemap = new XmlSiteMap();
        _sut = CreateService(new XmlSitemapsOptions
        {
            Storage = new XmlSitemapStorageOptions
            {
                RefreshStaleAfterSeconds = 0
            },
            Sitemaps =
            {
                ["products"] = new SitemapOptions
                {
                    HostName = "www.example.com"
                }
            }
        });
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(CreateStoredDocument(CreateSitemapStorageKey(), "<urlset />", DateTimeOffset.UtcNow.AddYears(-1)));
        _xmlSitemapXmlDeserializer.Deserialize<XmlSiteMap>("<urlset />").Returns(storedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(storedSitemap));
        await _xmlSitemapStorageRefreshService.DidNotReceive().RefreshConfiguredAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredCustomSitemapExists_ReturnsDeserializedSitemap()
    {
        var storedSitemap = new XmlSiteMap();
        _xmlSitemapDataSource
            .ReadAsync(Arg.Is<XmlSitemapStorageKey>(key =>
                key.Kind == XmlSitemapDocumentKind.Sitemap &&
                key.Alias == "external-products" &&
                key.HostName == "custom.example.com"))
            .Returns(new XmlSitemapStoredDocument(
                CreateCustomSitemapStorageKey(),
                Guid.NewGuid(),
                44,
                "sitemap--custom-example-com--external-products.xml",
                "/media/sitemaps/external-products.xml",
                "<urlset />",
                DateTimeOffset.UtcNow));
        _xmlSitemapXmlDeserializer.Deserialize<XmlSiteMap>("<urlset />").Returns(storedSitemap);

        var result = await _sut.GetConfiguredAsync("external-products");

        Assert.That(result, Is.SameAs(storedSitemap));
        await _xmlSitemapStorageRefreshService.DidNotReceive().RefreshCustomAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredCustomSitemapIsMissing_RefreshesCustomSitemap()
    {
        var generatedSitemap = new XmlSiteMap();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(Task.FromResult<XmlSitemapStoredDocument?>(null));
        _xmlSitemapStorageRefreshService.RefreshCustomAsync("external-products").Returns(generatedSitemap);

        var result = await _sut.GetConfiguredAsync("external-products");

        Assert.That(result, Is.SameAs(generatedSitemap));
        await _xmlSitemapStorageRefreshService.Received(1).RefreshCustomAsync("external-products");
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredCustomSitemapIsStale_RefreshesCustomSitemap()
    {
        var generatedSitemap = new XmlSiteMap();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(CreateStoredDocument(CreateCustomSitemapStorageKey(), "<old-urlset />", DateTimeOffset.UtcNow.AddHours(-2)));
        _xmlSitemapStorageRefreshService.RefreshCustomAsync("external-products").Returns(generatedSitemap);

        var result = await _sut.GetConfiguredAsync("external-products");

        Assert.That(result, Is.SameAs(generatedSitemap));
        _xmlSitemapXmlDeserializer.DidNotReceive().Deserialize<XmlSiteMap>(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.Received(1).RefreshCustomAsync("external-products");
    }

    [Test]
    public async Task GetIndexAsync_WhenStoredIndexExists_ReturnsDeserializedIndex()
    {
        var storedIndex = new XmlSiteMapIndex();
        _xmlSitemapDataSource
            .ReadAsync(Arg.Is<XmlSitemapStorageKey>(key =>
                key.Kind == XmlSitemapDocumentKind.SitemapIndex &&
                key.Alias == "main" &&
                key.HostName == "index.example.com"))
            .Returns(new XmlSitemapStoredDocument(
                CreateIndexStorageKey(),
                Guid.NewGuid(),
                43,
                "sitemap-index--index-example-com--main.xml",
                "/media/sitemaps/main.xml",
                "<sitemapindex />",
                DateTimeOffset.UtcNow));
        _xmlSitemapXmlDeserializer.Deserialize<XmlSiteMapIndex>("<sitemapindex />").Returns(storedIndex);

        var result = await _sut.GetIndexAsync("main");

        Assert.That(result, Is.SameAs(storedIndex));
        await _defaultXmlSiteMapService.DidNotReceive().GetIndexAsync(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.DidNotReceive().RefreshIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetIndexAsync_WhenStoredIndexIsMissing_RefreshesIndex()
    {
        var generatedIndex = new XmlSiteMapIndex();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(Task.FromResult<XmlSitemapStoredDocument?>(null));
        _xmlSitemapStorageRefreshService.RefreshIndexAsync("main").Returns(generatedIndex);

        var result = await _sut.GetIndexAsync("main");

        Assert.That(result, Is.SameAs(generatedIndex));
        await _xmlSitemapStorageRefreshService.Received(1).RefreshIndexAsync("main");
    }

    [Test]
    public async Task GetIndexAsync_WhenStoredIndexIsStale_RefreshesIndex()
    {
        var generatedIndex = new XmlSiteMapIndex();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(CreateStoredDocument(CreateIndexStorageKey(), "<old-index />", DateTimeOffset.UtcNow.AddHours(-2)));
        _xmlSitemapStorageRefreshService.RefreshIndexAsync("main").Returns(generatedIndex);

        var result = await _sut.GetIndexAsync("main");

        Assert.That(result, Is.SameAs(generatedIndex));
        _xmlSitemapXmlDeserializer.DidNotReceive().Deserialize<XmlSiteMapIndex>(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.Received(1).RefreshIndexAsync("main");
    }

    private StoredXmlSitemapService CreateService(XmlSitemapsOptions options)
    {
        return new StoredXmlSitemapService(
            _defaultXmlSiteMapService,
            _xmlSitemapDataSource,
            _xmlSitemapXmlDeserializer,
            _xmlSitemapStorageRefreshService,
            Options.Create(options),
            TimeProvider.System);
    }

    private static XmlSitemapStorageKey CreateSitemapStorageKey()
    {
        return new XmlSitemapStorageKey(XmlSitemapDocumentKind.Sitemap, "products", "www.example.com");
    }

    private static XmlSitemapStorageKey CreateIndexStorageKey()
    {
        return new XmlSitemapStorageKey(XmlSitemapDocumentKind.SitemapIndex, "main", "index.example.com");
    }

    private static XmlSitemapStorageKey CreateCustomSitemapStorageKey()
    {
        return new XmlSitemapStorageKey(XmlSitemapDocumentKind.Sitemap, "external-products", "custom.example.com");
    }

    private static XmlSitemapStoredDocument CreateStoredDocument(
        XmlSitemapStorageKey key,
        string xml,
        DateTimeOffset refreshedUtc)
    {
        return new XmlSitemapStoredDocument(
            key,
            Guid.NewGuid(),
            42,
            "sitemap.xml",
            "/media/sitemaps/sitemap.xml",
            xml,
            refreshedUtc);
    }
}
