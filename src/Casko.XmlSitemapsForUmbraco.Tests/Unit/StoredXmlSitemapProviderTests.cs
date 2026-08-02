using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Common.Serialization;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using CommonXmlSitemapApiConstants = Casko.XmlSitemapsForUmbraco.Common.XmlSitemapApiConstants;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class StoredXmlSitemapProviderTests
{
    private IXmlSitemapSourceProvider _sourceProvider = null!;
    private IXmlSitemapDataSource _xmlSitemapDataSource = null!;
    private IXmlSitemapXmlDeserializer _xmlSitemapXmlDeserializer = null!;
    private IXmlSitemapStorageRefreshService _xmlSitemapStorageRefreshService = null!;
    private StoredXmlSitemapProvider _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sourceProvider = Substitute.For<IXmlSitemapSourceProvider>();
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
    public async Task GetByRootKeyAsync_DelegatesToDefaultServiceWithoutUsingStorage()
    {
        var rootKey = Guid.NewGuid();
        var sitemap = new XmlSitemap();
        _sourceProvider.GetByRootKeyAsync(rootKey).Returns(sitemap);

        var result = await _sut.GetByRootKeyAsync(rootKey);

        Assert.That(result, Is.SameAs(sitemap));
        await _sourceProvider.Received(1).GetByRootKeyAsync(rootKey);
        await _xmlSitemapDataSource.DidNotReceiveWithAnyArgs().ReadAsync(default!);
        await _xmlSitemapStorageRefreshService.DidNotReceiveWithAnyArgs().RefreshConfiguredAsync(default!);
        await _xmlSitemapStorageRefreshService.DidNotReceiveWithAnyArgs().RefreshCustomAsync(default!);
        await _xmlSitemapStorageRefreshService.DidNotReceiveWithAnyArgs().RefreshIndexAsync(default!);
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
        var storedSitemap = new XmlSitemap();
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
        _xmlSitemapXmlDeserializer.Deserialize<XmlSitemap>("<urlset />").Returns(storedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(storedSitemap));
        await _sourceProvider.DidNotReceive().GetConfiguredAsync(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.DidNotReceive().RefreshConfiguredAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredSitemapIsMissing_RefreshesSitemap()
    {
        var generatedSitemap = new XmlSitemap();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(Task.FromResult<XmlSitemapStoredDocument?>(null));
        _xmlSitemapStorageRefreshService.RefreshConfiguredAsync("products").Returns(generatedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(generatedSitemap));
        await _xmlSitemapStorageRefreshService.Received(1).RefreshConfiguredAsync("products");
    }

    [Test]
    public async Task GetConfiguredAsync_WhenSingleModeUsesImplicitSitemapKey_RefreshesImplicitSitemap()
    {
        _sut = CreateService(new XmlSitemapsOptions());
        var generatedSitemap = new XmlSitemap();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(Task.FromResult<XmlSitemapStoredDocument?>(null));
        _xmlSitemapStorageRefreshService
            .RefreshConfiguredAsync(CommonXmlSitemapApiConstants.DefaultSitemapKey)
            .Returns(generatedSitemap);

        var result = await _sut.GetConfiguredAsync(CommonXmlSitemapApiConstants.DefaultSitemapKey);

        Assert.That(result, Is.SameAs(generatedSitemap));
        await _xmlSitemapDataSource.Received(1).ReadAsync(
            Arg.Is<XmlSitemapStorageKey>(key =>
                key.Kind == XmlSitemapDocumentKind.Sitemap &&
                key.Alias == CommonXmlSitemapApiConstants.DefaultSitemapKey &&
                key.HostName == null));
        await _xmlSitemapStorageRefreshService.Received(1)
            .RefreshConfiguredAsync(CommonXmlSitemapApiConstants.DefaultSitemapKey);
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredSitemapIsStale_RefreshesSitemap()
    {
        var generatedSitemap = new XmlSitemap();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(CreateStoredDocument(CreateSitemapStorageKey(), "<old-urlset />", DateTimeOffset.UtcNow.AddHours(-2)));
        _xmlSitemapStorageRefreshService.RefreshConfiguredAsync("products").Returns(generatedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(generatedSitemap));
        _xmlSitemapXmlDeserializer.DidNotReceive().Deserialize<XmlSitemap>(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.Received(1).RefreshConfiguredAsync("products");
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStaleCheckIsDisabled_ReturnsStoredSitemap()
    {
        var storedSitemap = new XmlSitemap();
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
        _xmlSitemapXmlDeserializer.Deserialize<XmlSitemap>("<urlset />").Returns(storedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(storedSitemap));
        await _xmlSitemapStorageRefreshService.DidNotReceive().RefreshConfiguredAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredCustomSitemapExists_ReturnsDeserializedSitemap()
    {
        var storedSitemap = new XmlSitemap();
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
        _xmlSitemapXmlDeserializer.Deserialize<XmlSitemap>("<urlset />").Returns(storedSitemap);

        var result = await _sut.GetConfiguredAsync("external-products");

        Assert.That(result, Is.SameAs(storedSitemap));
        await _xmlSitemapStorageRefreshService.DidNotReceive().RefreshCustomAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredCustomSitemapIsMissing_RefreshesCustomSitemap()
    {
        var generatedSitemap = new XmlSitemap();
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
        var generatedSitemap = new XmlSitemap();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(CreateStoredDocument(CreateCustomSitemapStorageKey(), "<old-urlset />", DateTimeOffset.UtcNow.AddHours(-2)));
        _xmlSitemapStorageRefreshService.RefreshCustomAsync("external-products").Returns(generatedSitemap);

        var result = await _sut.GetConfiguredAsync("external-products");

        Assert.That(result, Is.SameAs(generatedSitemap));
        _xmlSitemapXmlDeserializer.DidNotReceive().Deserialize<XmlSitemap>(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.Received(1).RefreshCustomAsync("external-products");
    }

    [Test]
    public async Task GetIndexAsync_WhenStoredIndexExists_ReturnsDeserializedIndex()
    {
        var storedIndex = new XmlSitemapIndex();
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
        _xmlSitemapXmlDeserializer.Deserialize<XmlSitemapIndex>("<sitemapindex />").Returns(storedIndex);

        var result = await _sut.GetIndexAsync("main");

        Assert.That(result, Is.SameAs(storedIndex));
        await _sourceProvider.DidNotReceive().GetIndexAsync(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.DidNotReceive().RefreshIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetIndexAsync_WhenStoredIndexIsMissing_RefreshesIndex()
    {
        var generatedIndex = new XmlSitemapIndex();
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
        var generatedIndex = new XmlSitemapIndex();
        _xmlSitemapDataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>())
            .Returns(CreateStoredDocument(CreateIndexStorageKey(), "<old-index />", DateTimeOffset.UtcNow.AddHours(-2)));
        _xmlSitemapStorageRefreshService.RefreshIndexAsync("main").Returns(generatedIndex);

        var result = await _sut.GetIndexAsync("main");

        Assert.That(result, Is.SameAs(generatedIndex));
        _xmlSitemapXmlDeserializer.DidNotReceive().Deserialize<XmlSitemapIndex>(Arg.Any<string>());
        await _xmlSitemapStorageRefreshService.Received(1).RefreshIndexAsync("main");
    }

    private StoredXmlSitemapProvider CreateService(XmlSitemapsOptions options)
    {
        return new StoredXmlSitemapProvider(
            _sourceProvider,
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
