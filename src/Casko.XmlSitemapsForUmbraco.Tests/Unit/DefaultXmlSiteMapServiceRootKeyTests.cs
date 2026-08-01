using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using CommonXmlSitemapApiConstants = Casko.XmlSitemapsForUmbraco.Common.XmlSitemapApiConstants;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class DefaultXmlSiteMapServiceRootKeyTests
{
    private IPublishedContentService _publishedContentService = null!;
    private IPublishedContentRenderer _sitemapRenderer = null!;
    private IPublishedContentIndexRenderer _sitemapIndexRenderer = null!;
    private IPublishedUrlProvider _publishedUrlProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _publishedContentService = Substitute.For<IPublishedContentService>();
        _sitemapRenderer = Substitute.For<IPublishedContentRenderer>();
        _sitemapIndexRenderer = Substitute.For<IPublishedContentIndexRenderer>();
        _publishedUrlProvider = Substitute.For<IPublishedUrlProvider>();
        _publishedUrlProvider
            .GetUrl(Arg.Any<IPublishedContent>(), UrlMode.Absolute, Arg.Any<string?>(), Arg.Any<Uri?>())
            .Returns("https://example.com/");
    }

    [Test]
    public async Task GetByRootKeyAsync_WhenContentExists_RendersSitemapFromRootContent()
    {
        var rootKey = Guid.NewGuid();
        var root = CreateContent("root");
        var hidden = CreateContent("hidden");
        var noIndex = CreateContent("root", "metaRobots", "noindex,nofollow");
        var sitemap = new XmlSitemap();
        PublishedContentRenderContext? context = null;
        _publishedContentService.GetContent(rootKey).Returns(root);
        _publishedContentService.GetLanguagesAsync().Returns(["en-US", "da-DK"]);
        _sitemapRenderer
            .Render(Arg.Do<PublishedContentRenderContext>(value => context = value))
            .Returns(sitemap);
        var sut = CreateService(new XmlSitemapsOptions
        {
            IncludedContentTypeAliases = ["root"],
            ExcludingUrlPropertyAlias = "metaRobots",
            ExcludingUrlPropertyValue = "noindex"
        });

        var result = await sut.GetByRootKeyAsync(rootKey);

        Assert.That(result, Is.SameAs(sitemap));
        Assert.That(context, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(context!.RootContents, Is.EqualTo(new[] { root }));
            Assert.That(context.DefaultLanguageCode, Is.EqualTo("en-US"));
            Assert.That(context.AlternativeLanguageCodes, Is.EqualTo(new[] { "en-US", "da-DK" }));
            Assert.That(context.Hostname, Is.EqualTo("example.com"));
            Assert.That(context.ShouldIncludeContent!(root), Is.True);
            Assert.That(context.ShouldIncludeContent!(hidden), Is.False);
            Assert.That(context.ShouldIncludeContent!(noIndex), Is.False);
        });
        _publishedUrlProvider.Received(1).GetUrl(root, UrlMode.Absolute, null, null);
    }

    [Test]
    public void GetByRootKeyAsync_WhenContentDoesNotExist_ThrowsRootContentNotFoundException()
    {
        var rootKey = Guid.NewGuid();
        _publishedContentService.GetContent(rootKey).Returns((IPublishedContent?)null);
        var sut = CreateService(new XmlSitemapsOptions());

        AsyncTestDelegate action = async () => await sut.GetByRootKeyAsync(rootKey);

        Assert.That(action, Throws.TypeOf<RootContentNotFoundException>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenSingleModeUsesImplicitSitemapKey_RendersSitemapFromRootPath()
    {
        var root = CreateContent("home");
        var sitemap = new XmlSitemap();
        PublishedContentRenderContext? context = null;
        _publishedContentService.GetContentByPath("/", null, null).Returns(root);
        _publishedContentService.GetLanguagesAsync().Returns(["en-US"]);
        _sitemapRenderer
            .Render(Arg.Do<PublishedContentRenderContext>(value => context = value))
            .Returns(sitemap);
        var sut = CreateService(new XmlSitemapsOptions());

        var result = await sut.GetConfiguredAsync(CommonXmlSitemapApiConstants.DefaultSitemapKey);

        Assert.That(result, Is.SameAs(sitemap));
        Assert.That(context!.RootContents, Is.EqualTo(new[] { root }));
        _publishedContentService.Received(1).GetContentByPath("/", null, null);
    }

    private PublishedContentXmlSitemapProvider CreateService(XmlSitemapsOptions options)
    {
        return new PublishedContentXmlSitemapProvider(
            Options.Create(options),
            _publishedContentService,
            _sitemapRenderer,
            _sitemapIndexRenderer,
            _publishedUrlProvider,
            Array.Empty<IXmlSitemapCustomProvider>());
    }

    private static IPublishedContent CreateContent(
        string contentTypeAlias,
        string? propertyAlias = null,
        object? propertyValue = null)
    {
        var content = Substitute.For<IPublishedContent>();
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
