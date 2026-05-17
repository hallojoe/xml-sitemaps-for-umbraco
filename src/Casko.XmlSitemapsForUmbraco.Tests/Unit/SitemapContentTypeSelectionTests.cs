using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Services.Cms;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class SitemapContentTypeSelectionTests
{
    [Test]
    public void Resolve_WhenRootIncludesAliases_AppliesGlobally()
    {
        var sut = SitemapContentTypeSelection.Resolve(new XmlSitemapsOptions
        {
            IncludedContentTypeAliases = ["homePage"]
        });

        Assert.Multiple(() =>
        {
            Assert.That(sut.ShouldInclude(CreateContent("homePage")), Is.True);
            Assert.That(sut.ShouldInclude(CreateContent("articlePage")), Is.False);
        });
    }

    [Test]
    public void Resolve_WhenSitemapIncludesAliases_AddsToRootIncludes()
    {
        var sut = SitemapContentTypeSelection.Resolve(
            new XmlSitemapsOptions { IncludedContentTypeAliases = ["homePage"] },
            new SitemapOptions { IncludedDocumentTypeAliases = ["articlePage"] });

        Assert.Multiple(() =>
        {
            Assert.That(sut.ShouldInclude(CreateContent("homePage")), Is.True);
            Assert.That(sut.ShouldInclude(CreateContent("articlePage")), Is.True);
            Assert.That(sut.ShouldInclude(CreateContent("productPage")), Is.False);
        });
    }

    [Test]
    public void Resolve_WhenRootAndSitemapExcludeAliases_RemovesMatchesAfterIncludes()
    {
        var sut = SitemapContentTypeSelection.Resolve(
            new XmlSitemapsOptions
            {
                IncludedContentTypeAliases = ["homePage", "articlePage", "productPage"],
                ExcludedContentTypeAliases = ["articlePage"]
            },
            new SitemapOptions { ExcludedDocumentTypeAliases = ["productPage"] });

        Assert.Multiple(() =>
        {
            Assert.That(sut.ShouldInclude(CreateContent("homePage")), Is.True);
            Assert.That(sut.ShouldInclude(CreateContent("articlePage")), Is.False);
            Assert.That(sut.ShouldInclude(CreateContent("productPage")), Is.False);
        });
    }

    [Test]
    public void Resolve_WhenNoIncludes_AllowsAllNonExcludedAliases()
    {
        var sut = SitemapContentTypeSelection.Resolve(new XmlSitemapsOptions
        {
            ExcludedContentTypeAliases = ["hiddenPage"]
        });

        Assert.Multiple(() =>
        {
            Assert.That(sut.ShouldInclude(CreateContent("homePage")), Is.True);
            Assert.That(sut.ShouldInclude(CreateContent("hiddenPage")), Is.False);
        });
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
