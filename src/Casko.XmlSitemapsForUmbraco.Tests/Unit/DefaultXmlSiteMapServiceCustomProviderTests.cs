using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;
using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class DefaultXmlSiteMapServiceCustomProviderTests
{
    private IPublishedContentService _publishedContentService = null!;
    private IPublishedContentRenderer _sitemapRenderer = null!;
    private IPublishedContentIndexRenderer _sitemapIndexRenderer = null!;
    private IHostUrlProvider _hostUrlProvider = null!;
    private IXmlSitemapCustomProvider _customProvider = null!;
    private IUmbracoContextFactory _umbracoContextFactory = null!;
    private IPublishedContentCache _publishedContentCache = null!;

    [SetUp]
    public void SetUp()
    {
        _publishedContentService = Substitute.For<IPublishedContentService>();
        _sitemapRenderer = Substitute.For<IPublishedContentRenderer>();
        _sitemapIndexRenderer = Substitute.For<IPublishedContentIndexRenderer>();
        _hostUrlProvider = Substitute.For<IHostUrlProvider>();
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([]));
        _customProvider = Substitute.For<IXmlSitemapCustomProvider>();
        _customProvider.Alias.Returns("external-products-provider");
        _umbracoContextFactory = Substitute.For<IUmbracoContextFactory>();
        _publishedContentCache = Substitute.For<IPublishedContentCache>();

        var umbracoContext = Substitute.For<IUmbracoContext>();
        umbracoContext.Content.Returns(_publishedContentCache);
        _umbracoContextFactory
            .EnsureUmbracoContext()
            .Returns(new UmbracoContextReference(
                umbracoContext,
                true,
                Substitute.For<IUmbracoContextAccessor>()));
    }

    [Test]
    public async Task GetConfiguredAsync_WhenKeyIsCustomSitemap_CallsConfiguredProviderWithContext()
    {
        var sitemap = new XmlSitemap();
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
        _umbracoContextFactory.DidNotReceive().EnsureUmbracoContext();
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
        _publishedContentService.GetLanguagesAsync().Returns([]);
        _publishedContentService
            .GetContentByPath(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                publishedContentCache: _publishedContentCache)
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

    private PublishedContentXmlSitemapProvider CreateService(
        XmlSitemapsOptions options,
        IEnumerable<IXmlSitemapCustomProvider> customProviders)
    {
        return new PublishedContentXmlSitemapProvider(
            Options.Create(options),
            _publishedContentService,
            _sitemapRenderer,
            _sitemapIndexRenderer,
            _hostUrlProvider,
            _umbracoContextFactory,
            customProviders);
    }
}
