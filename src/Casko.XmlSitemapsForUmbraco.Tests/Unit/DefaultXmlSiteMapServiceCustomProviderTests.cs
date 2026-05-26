using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Common.Services;
using Casko.XmlSitemapsForUmbraco.Common.Services.Cms;
using Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;
using Casko.XmlSitemapsForUmbraco.Models;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class DefaultXmlSiteMapServiceCustomProviderTests
{
    private ICmsContentService _cmsContentService = null!;
    private IXmlSitemapRenderer _sitemapRenderer = null!;
    private IXmlSitemapIndexRenderer _sitemapIndexRenderer = null!;
    private IXmlSitemapCustomProvider _customProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _cmsContentService = Substitute.For<ICmsContentService>();
        _sitemapRenderer = Substitute.For<IXmlSitemapRenderer>();
        _sitemapIndexRenderer = Substitute.For<IXmlSitemapIndexRenderer>();
        _customProvider = Substitute.For<IXmlSitemapCustomProvider>();
        _customProvider.Alias.Returns("external-products-provider");
    }

    [Test]
    public async Task GetConfiguredAsync_WhenKeyIsCustomSitemap_CallsConfiguredProviderWithContext()
    {
        var sitemap = new XmlSiteMap();
        XmlSitemapCustomProviderContext? context = null;
        _customProvider
            .GetSitemapAsync(Arg.Do<XmlSitemapCustomProviderContext>(value => context = value))
            .Returns(sitemap);
        var sut = CreateService(new XmlSitemapsOptions
        {
            CustomSitemaps =
            {
                ["external-products"] = new CustomSitemapOptions
                {
                    ProviderAlias = "external-products-provider",
                    HostName = "custom.example.com",
                    Settings =
                    {
                        ["FeedId"] = "products"
                    }
                }
            }
        }, [_customProvider]);

        var result = await sut.GetConfiguredAsync("external-products");

        Assert.That(result, Is.SameAs(sitemap));
        Assert.That(context, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(context!.Key, Is.EqualTo("external-products"));
            Assert.That(context.HostName, Is.EqualTo("custom.example.com"));
            Assert.That(context.Settings["FeedId"], Is.EqualTo("products"));
        });
    }

    [Test]
    public void GetConfiguredAsync_WhenCustomProviderIsMissing_ThrowsInvalidOperationException()
    {
        var sut = CreateService(new XmlSitemapsOptions
        {
            CustomSitemaps =
            {
                ["external-products"] = new CustomSitemapOptions
                {
                    ProviderAlias = "missing-provider"
                }
            }
        }, [_customProvider]);

        AsyncTestDelegate action = async () => await sut.GetConfiguredAsync("external-products");

        Assert.That(action, Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void GetConfiguredAsync_WhenRegularAndCustomKeysCollide_UsesRegularSitemap()
    {
        _cmsContentService.GetLanguagesAsync().Returns([]);
        _cmsContentService
            .GetContentByPath(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>())
            .Returns((IPublishedContent?)null);
        var sut = CreateService(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions { Path = "/products" }
            },
            CustomSitemaps =
            {
                ["products"] = new CustomSitemapOptions
                {
                    ProviderAlias = "external-products-provider"
                }
            }
        }, [_customProvider]);

        AsyncTestDelegate action = async () => await sut.GetConfiguredAsync("products");

        Assert.That(action, Throws.TypeOf<RootContentNotFoundException>());
        _customProvider.DidNotReceive().GetSitemapAsync(Arg.Any<XmlSitemapCustomProviderContext>());
    }

    private DefaultXmlSiteMapService CreateService(
        XmlSitemapsOptions options,
        IEnumerable<IXmlSitemapCustomProvider> customProviders)
    {
        return new DefaultXmlSiteMapService(
            Options.Create(options),
            _cmsContentService,
            _sitemapRenderer,
            _sitemapIndexRenderer,
            Substitute.For<IPublishedUrlProvider>(),
            customProviders);
    }
}
