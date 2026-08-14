using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common;
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
using CommonXmlSitemapApiConstants = Casko.XmlSitemapsForUmbraco.Common.XmlSitemapApiConstants;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class DefaultXmlSiteMapServiceRootKeyTests
{
    private IPublishedContentService _publishedContentService = null!;
    private IPublishedContentRenderer _sitemapRenderer = null!;
    private IPublishedContentIndexRenderer _sitemapIndexRenderer = null!;
    private IHostUrlProvider _hostUrlProvider = null!;
    private IUmbracoContextFactory _umbracoContextFactory = null!;
    private IPublishedContentCache _publishedContentCache = null!;
    private IUmbracoContextAccessor _umbracoContextAccessor = null!;

    [SetUp]
    public void SetUp()
    {
        _publishedContentService = Substitute.For<IPublishedContentService>();
        _sitemapRenderer = Substitute.For<IPublishedContentRenderer>();
        _sitemapIndexRenderer = Substitute.For<IPublishedContentIndexRenderer>();
        _hostUrlProvider = Substitute.For<IHostUrlProvider>();
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([]));
        _umbracoContextFactory = Substitute.For<IUmbracoContextFactory>();
        _publishedContentCache = Substitute.For<IPublishedContentCache>();
        _umbracoContextAccessor = Substitute.For<IUmbracoContextAccessor>();

        var umbracoContext = Substitute.For<IUmbracoContext>();
        umbracoContext.Content.Returns(_publishedContentCache);
        _umbracoContextFactory
            .EnsureUmbracoContext()
            .Returns(new UmbracoContextReference(umbracoContext, true, _umbracoContextAccessor));
    }

    [Test]
    public async Task GetByRootKeyAsync_WhenContentExists_RendersSitemapFromRootContent()
    {
        var rootKey = Guid.NewGuid();
        var root = CreateContent("root", key: rootKey);
        var hidden = CreateContent("hidden");
        var noIndex = CreateContent("root", "metaRobots", "noindex,nofollow");
        var sitemap = new XmlSitemap();
        PublishedContentRenderContext? context = null;
        _publishedContentService.GetContent(rootKey, _publishedContentCache).Returns(root);
        _publishedContentService.GetLanguagesAsync().Returns(["en-US", "da-DK"]);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([
            new HostUrl(new Uri("https://example.com/en-US/"), "en-US", 10, rootKey, true)
        ]));
        _sitemapRenderer
            .Render(Arg.Do<PublishedContentRenderContext>(value =>
            {
                context = value;
                _umbracoContextAccessor.DidNotReceive().Clear();
            }))
            .Returns(sitemap);
        var sut = CreateService(new XmlSitemapsOptions
        {
            IncludedContentTypeAliases = ["root"],
            ExcludingUrlPropertyAlias = "metaRobots",
            ExcludingUrlPropertyValue = "noindex"
        });

        var result = await sut.GetByRootKeyAsync(rootKey);

        Assert.That(result, Is.SameAs(sitemap));
        _umbracoContextFactory.Received(1).EnsureUmbracoContext();
        _umbracoContextAccessor.Received(1).Clear();
        Assert.That(context, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(context!.RootContents, Is.EqualTo(new[] { root }));
            Assert.That(context.DefaultLanguageCode, Is.EqualTo("en-US"));
            Assert.That(context.AlternativeLanguageCodes, Is.EqualTo(new[] { "en-US", "da-DK" }));
            Assert.That(context.Hostname, Is.EqualTo("https://example.com/en-US"));
            Assert.That(context.ShouldIncludeContent!(root), Is.True);
            Assert.That(context.ShouldIncludeContent!(hidden), Is.False);
            Assert.That(context.ShouldIncludeContent!(noIndex), Is.False);
        });
    }

    [Test]
    public async Task GetConfiguredAsync_WhenExplicitHostnameAndCultureAreConfigured_UsesExplicitValues()
    {
        var rootKey = Guid.NewGuid();
        var root = CreateContent("home", key: rootKey);
        var sitemap = new XmlSitemap();
        PublishedContentRenderContext? context = null;
        _publishedContentService
            .GetContentByPath(
                "/products",
                "https://configured.example.com",
                "da-DK",
                publishedContentCache: _publishedContentCache)
            .Returns(root);
        _publishedContentService.GetLanguagesAsync().Returns(["en-US", "da-DK"]);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([
            new HostUrl(new Uri("https://ignored.example.com/en-US/"), "en-US", 10, rootKey, true)
        ]));
        _sitemapRenderer
            .Render(Arg.Do<PublishedContentRenderContext>(value => context = value))
            .Returns(sitemap);
        var sut = CreateService(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions
                {
                    Path = "/products",
                    HostName = "https://configured.example.com",
                    Culture = "da-DK"
                }
            }
        });

        var result = await sut.GetConfiguredAsync("products");

        Assert.That(result, Is.SameAs(sitemap));
        _umbracoContextFactory.Received(1).EnsureUmbracoContext();
        Assert.Multiple(() =>
        {
            Assert.That(context!.DefaultLanguageCode, Is.EqualTo("da-DK"));
            Assert.That(context.Hostname, Is.EqualTo("https://configured.example.com"));
        });
    }

    [Test]
    public void GetByRootKeyAsync_WhenContentDoesNotExist_ThrowsRootContentNotFoundException()
    {
        var rootKey = Guid.NewGuid();
        _publishedContentService.GetContent(rootKey, _publishedContentCache).Returns((IPublishedContent?)null);
        var sut = CreateService(new XmlSitemapsOptions());

        AsyncTestDelegate action = async () => await sut.GetByRootKeyAsync(rootKey);

        Assert.That(action, Throws.TypeOf<RootContentNotFoundException>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenSingleModeUsesImplicitSitemapKey_RendersSitemapFromRootPath()
    {
        var rootKey = Guid.NewGuid();
        var root = CreateContent("home", key: rootKey);
        var sitemap = new XmlSitemap();
        PublishedContentRenderContext? context = null;
        _publishedContentService
            .GetContentByPath("/", null, null, publishedContentCache: _publishedContentCache)
            .Returns(root);
        _publishedContentService.GetLanguagesAsync().Returns(["en-US"]);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([
            new HostUrl(new Uri("https://example.com"), "en-US", 10, rootKey, true)
        ]));
        _sitemapRenderer
            .Render(Arg.Do<PublishedContentRenderContext>(value => context = value))
            .Returns(sitemap);
        var sut = CreateService(new XmlSitemapsOptions());

        var result = await sut.GetConfiguredAsync(CommonXmlSitemapApiConstants.DefaultSitemapKey);

        Assert.That(result, Is.SameAs(sitemap));
        Assert.That(context!.RootContents, Is.EqualTo(new[] { root }));
        _umbracoContextFactory.Received(1).EnsureUmbracoContext();
        _publishedContentService.Received(1).GetContentByPath("/", null, null, publishedContentCache: _publishedContentCache);
    }

    private PublishedContentXmlSitemapProvider CreateService(XmlSitemapsOptions options)
    {
        return new PublishedContentXmlSitemapProvider(
            Options.Create(options),
            _publishedContentService,
            _sitemapRenderer,
            _sitemapIndexRenderer,
            _hostUrlProvider,
            _umbracoContextFactory,
            Array.Empty<IXmlSitemapCustomProvider>());
    }

    private static IPublishedContent CreateContent(
        string contentTypeAlias,
        string? propertyAlias = null,
        object? propertyValue = null,
        Guid? key = null)
    {
        var content = Substitute.For<IPublishedContent>();
        content.Key.Returns(key ?? Guid.NewGuid());
        var contentType = Substitute.For<IPublishedContentType>();
        contentType.Alias.Returns(contentTypeAlias);
        content.ContentType.Returns(contentType);

        if (propertyAlias is not null)
        {
            var property = Substitute.For<IPublishedProperty>();
            property.GetValue(Arg.Any<string?>(), null).Returns(propertyValue);
            content.GetProperty(propertyAlias).Returns(property);
        }

        return content;
    }
}
