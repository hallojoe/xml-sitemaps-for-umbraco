using Casko.XmlSitemapsForUmbraco.Common.Configuration;
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
    private IXmlSitemapDataSource _dataSource = null!;
    private IXmlSitemapXmlDeserializer _deserializer = null!;
    private StoredXmlSitemapProvider _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sourceProvider = Substitute.For<IXmlSitemapSourceProvider>();
        _dataSource = Substitute.For<IXmlSitemapDataSource>();
        _deserializer = Substitute.For<IXmlSitemapXmlDeserializer>();
        _sut = CreateService(CreateOptions());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredSitemapExists_ReturnsDeserializedSitemap()
    {
        var storedSitemap = new XmlSitemap();
        _dataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>()).Returns(CreateDocument(CreateSitemapKey(), "<urlset />"));
        _deserializer.Deserialize<XmlSitemap>("<urlset />").Returns(storedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(storedSitemap));
        await _sourceProvider.DidNotReceive().GetConfiguredAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredSitemapIsMissing_ReturnsEmptySitemapWithoutRendering()
    {
        _dataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>()).Returns((XmlSitemapStoredDocument?)null);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.TypeOf<XmlSitemap>());
        Assert.That(((XmlSitemap)result).Urls, Is.Empty);
        await _sourceProvider.DidNotReceive().GetConfiguredAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenStoredSitemapIsOld_ReturnsStoredSitemapWithoutRendering()
    {
        var storedSitemap = new XmlSitemap();
        _dataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>()).Returns(CreateDocument(CreateSitemapKey(), "<urlset />", DateTimeOffset.UtcNow.AddDays(-1)));
        _deserializer.Deserialize<XmlSitemap>("<urlset />").Returns(storedSitemap);

        var result = await _sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(storedSitemap));
        await _sourceProvider.DidNotReceive().GetConfiguredAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenSingleModeSitemapIsMissing_ReturnsEmptySitemap()
    {
        _sut = CreateService(new XmlSitemapsOptions());
        _dataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>()).Returns((XmlSitemapStoredDocument?)null);

        var result = await _sut.GetConfiguredAsync(CommonXmlSitemapApiConstants.DefaultSitemapKey);

        Assert.That(result, Is.TypeOf<XmlSitemap>());
        await _dataSource.Received(1).ReadAsync(Arg.Is<XmlSitemapStorageKey>(key =>
            key.Kind == XmlSitemapDocumentKind.Sitemap &&
            key.Alias == CommonXmlSitemapApiConstants.DefaultSitemapKey));
    }

    [Test]
    public async Task GetIndexAsync_WhenStoredIndexExists_ReturnsDeserializedIndex()
    {
        var storedIndex = new XmlSitemapIndex { Locations = [] };
        _dataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>()).Returns(CreateDocument(CreateIndexKey(), "<sitemapindex />"));
        _deserializer.Deserialize<XmlSitemapIndex>("<sitemapindex />").Returns(storedIndex);

        var result = await _sut.GetIndexAsync("main");

        Assert.That(result, Is.SameAs(storedIndex));
        await _sourceProvider.DidNotReceive().GetIndexAsync(Arg.Any<string>());
    }

    [Test]
    public async Task GetIndexAsync_WhenStoredIndexIsMissing_ReturnsEmptyIndexWithoutRendering()
    {
        _dataSource.ReadAsync(Arg.Any<XmlSitemapStorageKey>()).Returns((XmlSitemapStoredDocument?)null);

        var result = await _sut.GetIndexAsync("main");

        Assert.That(result, Is.TypeOf<XmlSitemapIndex>());
        Assert.That(((XmlSitemapIndex)result).Locations, Is.Empty);
        await _sourceProvider.DidNotReceive().GetIndexAsync(Arg.Any<string>());
    }

    private StoredXmlSitemapProvider CreateService(XmlSitemapsOptions options)
    {
        return new StoredXmlSitemapProvider(
            _sourceProvider,
            _dataSource,
            _deserializer,
            Options.Create(options));
    }

    private static XmlSitemapsOptions CreateOptions()
    {
        return new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions { HostName = "www.example.com" }
            },
            Indexes =
            {
                ["main"] = new SitemapIndexOptions { HostName = "index.example.com", Sitemaps = ["products"] }
            }
        };
    }

    private static XmlSitemapStorageKey CreateSitemapKey()
    {
        return new XmlSitemapStorageKey(XmlSitemapDocumentKind.Sitemap, "products", "www.example.com");
    }

    private static XmlSitemapStorageKey CreateIndexKey()
    {
        return new XmlSitemapStorageKey(XmlSitemapDocumentKind.SitemapIndex, "main", "index.example.com");
    }

    private static XmlSitemapStoredDocument CreateDocument(
        XmlSitemapStorageKey key,
        string xml,
        DateTimeOffset? refreshedUtc = null)
    {
        return new XmlSitemapStoredDocument(key, Guid.NewGuid(), 42, "sitemap.xml", "/media/sitemap.xml", xml, refreshedUtc);
    }
}
