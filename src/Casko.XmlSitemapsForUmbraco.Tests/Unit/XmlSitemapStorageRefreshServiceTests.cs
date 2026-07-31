using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Serialization;
using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class XmlSitemapStorageRefreshServiceTests
{
    private IXmlSitemapSourceProvider _sourceProvider = null!;
    private IXmlSitemapDataSource _xmlSitemapDataSource = null!;
    private IXmlSitemapXmlSerializer _xmlSitemapXmlSerializer = null!;
    private XmlSitemapStorageRefreshService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sourceProvider = Substitute.For<IXmlSitemapSourceProvider>();
        _xmlSitemapDataSource = Substitute.For<IXmlSitemapDataSource>();
        _xmlSitemapXmlSerializer = Substitute.For<IXmlSitemapXmlSerializer>();
        _sut = CreateService(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions { PublicName = "xmlsitemap", HostName = "www.example.com" },
                ["articles"] = new SitemapOptions { HostName = "articles.example.com" }
            },
            CustomSitemaps =
            {
                ["external-products"] = new CustomSitemapOptions
                {
                    ProviderAlias = "external-products-provider",
                    HostName = "custom.example.com"
                }
            },
            Indexes =
            {
                ["main"] = new SitemapIndexOptions
                {
                    HostName = "index.example.com",
                    Sitemaps = ["products", "articles"]
                }
            }
        });
    }

    [Test]
    public async Task RefreshConfiguredAsync_RebuildsConfiguredSitemapAndWritesStorageKey()
    {
        var sitemap = new XmlSitemap();
        _sourceProvider.GetConfiguredAsync("products").Returns(sitemap);
        _xmlSitemapXmlSerializer.Serialize(sitemap).Returns("<urlset />");

        var result = await _sut.RefreshConfiguredAsync("products");

        Assert.That(result, Is.SameAs(sitemap));
        await _xmlSitemapDataSource.Received(1).WriteAsync(
            Arg.Is<XmlSitemapStorageKey>(key =>
                key.Kind == XmlSitemapDocumentKind.Sitemap &&
                key.Alias == "products" &&
                key.HostName == "www.example.com"),
            "<urlset />");
    }

    [Test]
    public async Task RefreshIndexAsync_RebuildsConfiguredIndexAndWritesStorageKey()
    {
        var index = new XmlSitemapIndex();
        _sourceProvider.GetIndexAsync("main").Returns(index);
        _xmlSitemapXmlSerializer.Serialize(index).Returns("<sitemapindex />");

        var result = await _sut.RefreshIndexAsync("main");

        Assert.That(result, Is.SameAs(index));
        await _xmlSitemapDataSource.Received(1).WriteAsync(
            Arg.Is<XmlSitemapStorageKey>(key =>
                key.Kind == XmlSitemapDocumentKind.SitemapIndex &&
                key.Alias == "main" &&
                key.HostName == "index.example.com"),
            "<sitemapindex />");
    }

    [Test]
    public async Task RefreshCustomAsync_RebuildsConfiguredCustomSitemapAndWritesStorageKey()
    {
        var sitemap = new XmlSitemap();
        _sourceProvider.GetConfiguredAsync("external-products").Returns(sitemap);
        _xmlSitemapXmlSerializer.Serialize(sitemap).Returns("<custom-urlset />");

        var result = await _sut.RefreshCustomAsync("external-products");

        Assert.That(result, Is.SameAs(sitemap));
        await _xmlSitemapDataSource.Received(1).WriteAsync(
            Arg.Is<XmlSitemapStorageKey>(key =>
                key.Kind == XmlSitemapDocumentKind.Sitemap &&
                key.Alias == "external-products" &&
                key.HostName == "custom.example.com"),
            "<custom-urlset />");
    }

    [Test]
    public async Task RefreshAllAsync_RefreshesSitemapsBeforeCustomSitemapsBeforeIndexes()
    {
        var products = new XmlSitemap();
        var articles = new XmlSitemap();
        var externalProducts = new XmlSitemap();
        var index = new XmlSitemapIndex();
        _sourceProvider.GetConfiguredAsync("products").Returns(products);
        _sourceProvider.GetConfiguredAsync("articles").Returns(articles);
        _sourceProvider.GetConfiguredAsync("external-products").Returns(externalProducts);
        _sourceProvider.GetIndexAsync("main").Returns(index);
        _xmlSitemapXmlSerializer.Serialize(products).Returns("<products />");
        _xmlSitemapXmlSerializer.Serialize(articles).Returns("<articles />");
        _xmlSitemapXmlSerializer.Serialize(externalProducts).Returns("<external-products />");
        _xmlSitemapXmlSerializer.Serialize(index).Returns("<index />");

        await _sut.RefreshAllAsync();

        Received.InOrder(() =>
        {
            _xmlSitemapDataSource.WriteAsync(
                Arg.Is<XmlSitemapStorageKey>(key => key.Kind == XmlSitemapDocumentKind.Sitemap && key.Alias == "products"),
                "<products />");
            _xmlSitemapDataSource.WriteAsync(
                Arg.Is<XmlSitemapStorageKey>(key => key.Kind == XmlSitemapDocumentKind.Sitemap && key.Alias == "articles"),
                "<articles />");
            _xmlSitemapDataSource.WriteAsync(
                Arg.Is<XmlSitemapStorageKey>(key => key.Kind == XmlSitemapDocumentKind.Sitemap && key.Alias == "external-products"),
                "<external-products />");
            _xmlSitemapDataSource.WriteAsync(
                Arg.Is<XmlSitemapStorageKey>(key => key.Kind == XmlSitemapDocumentKind.SitemapIndex && key.Alias == "main"),
                "<index />");
        });
    }

    private XmlSitemapStorageRefreshService CreateService(XmlSitemapsOptions options)
    {
        return new XmlSitemapStorageRefreshService(
            _sourceProvider,
            _xmlSitemapDataSource,
            _xmlSitemapXmlSerializer,
            Options.Create(options));
    }
}
