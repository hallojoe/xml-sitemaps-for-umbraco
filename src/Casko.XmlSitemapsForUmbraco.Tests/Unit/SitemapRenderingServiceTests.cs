using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Indexes;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class SitemapRenderingServiceTests
{
    [Test]
    public void Render_WhenRootHasDescendant_IncludesRootAndDescendant()
    {
        var child = CreateContent("child");
        var root = CreateContent("root");
        var contentCollector = Substitute.For<IPublishedContentCollector>();
        contentCollector.Collect(Arg.Any<PublishedContentRenderContext>()).Returns([root, child]);
        var urlRenderer = Substitute.For<IPublishedContentUrlRenderer>();
        urlRenderer.Render(root, Arg.Any<XmlSitemapUrlRenderContext>())
            .Returns(new XmlSitemapUrl { Location = "/root" });
        urlRenderer.Render(child, Arg.Any<XmlSitemapUrlRenderContext>())
            .Returns(new XmlSitemapUrl { Location = "/child" });
        var sut = new PublishedContentRenderer(contentCollector, urlRenderer);

        var result = sut.Render(new PublishedContentRenderContext([root], "en", ["en"], "example.com"));

        Assert.That(result.Urls.Select(url => url.Location), Is.EqualTo(new[] { "/root", "/child" }));
    }

    [Test]
    public void Render_WhenDocumentTypeFilterExcludesContent_ExcludesDisallowedContent()
    {
        var child = CreateContent("hidden");
        var root = CreateContent("visible");
        var contentCollector = Substitute.For<IPublishedContentCollector>();
        contentCollector.Collect(Arg.Any<PublishedContentRenderContext>()).Returns([root, child]);
        var urlRenderer = Substitute.For<IPublishedContentUrlRenderer>();
        urlRenderer.Render(root, Arg.Any<XmlSitemapUrlRenderContext>())
            .Returns(new XmlSitemapUrl { Location = "/root" });
        var sut = new PublishedContentRenderer(contentCollector, urlRenderer);

        var result = sut.Render(new PublishedContentRenderContext(
            [root],
            "en",
            ["en"],
            Hostname: null,
            RenderAlternateLinks: true,
            content => content.ContentType.Alias != "hidden"));

        Assert.That(result.Urls, Has.Count.EqualTo(1));
        Assert.That(result.Urls[0].Location, Is.EqualTo("/root"));
        urlRenderer.DidNotReceive().Render(child, Arg.Any<XmlSitemapUrlRenderContext>());
    }

    [Test]
    public void Render_WhenFilterExcludesEveryContentItem_ThrowsRootContentHasNoContentException()
    {
        var root = CreateContent("hidden");
        var contentCollector = Substitute.For<IPublishedContentCollector>();
        contentCollector.Collect(Arg.Any<PublishedContentRenderContext>()).Returns([root]);
        var sut = new PublishedContentRenderer(contentCollector, Substitute.For<IPublishedContentUrlRenderer>());

        TestDelegate action = () => sut.Render(new PublishedContentRenderContext(
            [root],
            "en",
            ["en"],
            Hostname: null,
            RenderAlternateLinks: true,
            _ => false));

        Assert.That(action, Throws.TypeOf<RootContentHasNoContentException>());
    }

    [Test]
    public void Render_WhenUrlsHaveSameLocation_DeduplicatesCaseInsensitively()
    {
        var child = CreateContent("child");
        var root = CreateContent("root");
        var contentCollector = Substitute.For<IPublishedContentCollector>();
        contentCollector.Collect(Arg.Any<PublishedContentRenderContext>()).Returns([root, child]);
        var urlRenderer = Substitute.For<IPublishedContentUrlRenderer>();
        urlRenderer.Render(root, Arg.Any<XmlSitemapUrlRenderContext>())
            .Returns(new XmlSitemapUrl { Location = "/same" });
        urlRenderer.Render(child, Arg.Any<XmlSitemapUrlRenderContext>())
            .Returns(new XmlSitemapUrl { Location = "/SAME" });
        var sut = new PublishedContentRenderer(contentCollector, urlRenderer);

        var result = sut.Render(new PublishedContentRenderContext([root], "en", ["en"], Hostname: null));

        Assert.That(result.Urls, Has.Count.EqualTo(1));
        Assert.That(result.Urls[0].Location, Is.EqualTo("/SAME"));
    }

    [Test]
    public void RenderUrl_SetsLocationLastModifiedAndCultureLinks()
    {
        var content = CreateContent("root");
        var lastModified = new DateTime(2026, 5, 14, 12, 0, 0, DateTimeKind.Utc);
        content.UpdateDate.Returns(lastModified);
        var urlBuilder = Substitute.For<IPublishedContentUrlBuilder>();
        urlBuilder.BuildContentUrl(content, "en", "example.com").Returns("https://example.com/en");
        var cultureLinkRenderer = Substitute.For<IPublishedContentUrlCultureLinkRenderer>();
        var cultureLinks = new List<XHtmlLink> { new() { Href = "https://example.com/en", HrefLang = "en" } };
        cultureLinkRenderer.Render(content, Arg.Any<XmlSitemapUrlRenderContext>()).Returns(cultureLinks);
        var sut = new PublishedContentUrlRenderer(urlBuilder, cultureLinkRenderer);

        var result = sut.Render(content, new XmlSitemapUrlRenderContext("en", ["en"], "example.com"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Location, Is.EqualTo("https://example.com/en"));
            Assert.That(result.LastModified, Is.EqualTo(lastModified));
            Assert.That(result.CultureLinks, Is.EqualTo(cultureLinks));
        });
    }

    [Test]
    public void RenderCultureLinks_PutsCurrentCultureFirstAndSkipsFragmentUrls()
    {
        var content = CreateContent("root");
        var urlBuilder = Substitute.For<IPublishedContentUrlBuilder>();
        urlBuilder.BuildContentUrl(content, "da", "example.com").Returns("https://example.com/da");
        urlBuilder.BuildContentUrl(content, "en", "example.com").Returns("https://example.com/en");
        urlBuilder.BuildContentUrl(content, "pl", "example.com").Returns("#");
        var sut = new PublishedContentUrlCultureLinkRenderer(urlBuilder);

        var result = sut.Render(content, new XmlSitemapUrlRenderContext("da", ["en", "pl", "da"], "example.com"));

        Assert.That(result.Select(link => link.HrefLang), Is.EqualTo(new[] { "da", "en" }));
        Assert.That(result.Select(link => link.Href), Is.EqualTo(new[] { "https://example.com/da", "https://example.com/en" }));
    }

    [Test]
    public void RenderCultureLinks_WhenDisabled_ReturnsNoLinks()
    {
        var content = CreateContent("root");
        var urlBuilder = Substitute.For<IPublishedContentUrlBuilder>();
        var sut = new PublishedContentUrlCultureLinkRenderer(urlBuilder);

        var result = sut.Render(content, new XmlSitemapUrlRenderContext(
            "da",
            ["da"],
            "example.com",
            RenderAlternateLinks: false));

        Assert.That(result, Is.Empty);
        urlBuilder.DidNotReceiveWithAnyArgs().BuildContentUrl(default!, default!, default);
    }

    [Test]
    public void RenderCultureLinks_WhenEnabledForSingleCulture_ReturnsCurrentCultureLink()
    {
        var content = CreateContent("root");
        var urlBuilder = Substitute.For<IPublishedContentUrlBuilder>();
        urlBuilder.BuildContentUrl(content, "da", "example.com").Returns("https://example.com/da");
        var sut = new PublishedContentUrlCultureLinkRenderer(urlBuilder);

        var result = sut.Render(content, new XmlSitemapUrlRenderContext(
            "da",
            ["da"],
            "example.com",
            RenderAlternateLinks: true));

        Assert.That(result.Select(link => link.HrefLang), Is.EqualTo(new[] { "da" }));
        Assert.That(result.Select(link => link.Href), Is.EqualTo(new[] { "https://example.com/da" }));
    }

    [Test]
    public void RenderIndex_CreatesDistinctLocationsWithHostname()
    {
        var sut = new PublishedContentIndexRenderer(new XmlSitemapIndexRenderer(new PublishedContentUrlBuilder()));

        var result = sut.Render(new XmlSitemapIndexRenderContext(
            ["products", "products", "news"],
            "https://example.com",
            XmlSitemapIndexLocationMode.ApiRoute));

        Assert.That(result.Locations.Select(location => location.Location), Is.EqualTo(new[]
        {
            $"https://example.com/{XmlSitemapApiConstants.ApiRoute}?name=products",
            $"https://example.com/{XmlSitemapApiConstants.ApiRoute}?name=news"
        }));
    }

    [Test]
    public void RenderIndex_WhenPublicAliasesAreConfigured_UsesPublicAliasesForLocations()
    {
        var sut = new PublishedContentIndexRenderer(new XmlSitemapIndexRenderer(new PublishedContentUrlBuilder()));

        var result = sut.Render(new XmlSitemapIndexRenderContext(
            ["host1-main", "host2-main", "news"],
            "https://example.com",
            XmlSitemapIndexLocationMode.LegacyXmlFile,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["host1-main"] = "xmlsitemap",
                ["host2-main"] = "xmlsitemap",
                ["news"] = "news-public"
            }));

        Assert.That(result.Locations.Select(location => location.Location), Is.EqualTo(new[]
        {
            "https://example.com/xmlsitemap.xml",
            "https://example.com/news-public.xml"
        }));
    }

    [Test]
    public void BuildLegacySitemapFileUrl_CombinesHostnameAndAlias()
    {
        var sut = new PublishedContentUrlBuilder();

        var result = sut.BuildLegacySitemapFileUrl("products", "https://example.com/");

        Assert.That(result, Is.EqualTo("https://example.com/products.xml"));
    }

    private static IPublishedContent CreateContent(string contentTypeAlias)
    {
        var content = Substitute.For<IPublishedContent>();
        var contentType = Substitute.For<IPublishedContentType>();
        contentType.Alias.Returns(contentTypeAlias);
        content.ContentType.Returns(contentType);

        return content;
    }
}
