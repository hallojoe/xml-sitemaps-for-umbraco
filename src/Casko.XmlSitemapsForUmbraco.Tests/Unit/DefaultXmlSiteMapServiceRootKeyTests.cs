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
public class DefaultXmlSiteMapServiceRootKeyTests
{
    private ICmsContentService _cmsContentService = null!;
    private IXmlSitemapRenderer _sitemapRenderer = null!;
    private IXmlSitemapIndexRenderer _sitemapIndexRenderer = null!;
    private IPublishedUrlProvider _publishedUrlProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _cmsContentService = Substitute.For<ICmsContentService>();
        _sitemapRenderer = Substitute.For<IXmlSitemapRenderer>();
        _sitemapIndexRenderer = Substitute.For<IXmlSitemapIndexRenderer>();
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
        var sitemap = new XmlSiteMap();
        XmlSitemapRenderContext? context = null;
        _cmsContentService.GetContent(rootKey).Returns(root);
        _cmsContentService.GetLanguagesAsync().Returns(["en-US", "da-DK"]);
        _sitemapRenderer
            .Render(Arg.Do<XmlSitemapRenderContext>(value => context = value))
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
        _cmsContentService.GetContent(rootKey).Returns((IPublishedContent?)null);
        var sut = CreateService(new XmlSitemapsOptions());

        AsyncTestDelegate action = async () => await sut.GetByRootKeyAsync(rootKey);

        Assert.That(action, Throws.TypeOf<RootContentNotFoundException>());
    }

    private DefaultXmlSiteMapService CreateService(XmlSitemapsOptions options)
    {
        return new DefaultXmlSiteMapService(
            Options.Create(options),
            _cmsContentService,
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
