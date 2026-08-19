using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Examine;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class ExamineSitemapSearchResultFilterTests
{
    [Test]
    public void IsIncluded_WhenNoContentTypeFiltersAreConfigured_IncludesResult()
    {
        var sut = CreateSut();

        Assert.That(sut.IsIncluded(CreateSearchResult()), Is.True);
    }

    [Test]
    public void IsIncluded_WhenAliasIsInIncludeList_IgnoresCase()
    {
        var sut = CreateSut(new XmlSitemapsOptions
        {
            IncludedContentTypeAliases = ["articlePage"]
        });

        Assert.That(sut.IsIncluded(CreateSearchResult(("__NodeTypeAlias", "ARTICLEPAGE"))), Is.True);
    }

    [Test]
    public void IsIncluded_WhenAliasIsNotInIncludeList_ExcludesResult()
    {
        var sut = CreateSut(new XmlSitemapsOptions
        {
            IncludedContentTypeAliases = ["articlePage"]
        });

        Assert.That(sut.IsIncluded(CreateSearchResult(("__NodeTypeAlias", "loginPage"))), Is.False);
    }

    [Test]
    public void IsIncluded_WhenAliasIsInExcludeList_IgnoresCase()
    {
        var sut = CreateSut(new XmlSitemapsOptions
        {
            ExcludedContentTypeAliases = ["loginPage"]
        });

        Assert.That(sut.IsIncluded(CreateSearchResult(("__NodeTypeAlias", "LOGINPAGE"))), Is.False);
    }

    [Test]
    public void IsIncluded_WhenAliasIsIncludedAndExcluded_ExclusionWins()
    {
        var sut = CreateSut(new XmlSitemapsOptions
        {
            IncludedContentTypeAliases = ["articlePage"],
            ExcludedContentTypeAliases = ["articlePage"]
        });

        Assert.That(sut.IsIncluded(CreateSearchResult(("__NodeTypeAlias", "articlePage"))), Is.False);
    }

    [Test]
    public void IsIncluded_WhenAliasIsMissing_RejectsOnlyWhenIncludeListIsConfigured()
    {
        var permissiveSut = CreateSut();
        var restrictiveSut = CreateSut(new XmlSitemapsOptions
        {
            IncludedContentTypeAliases = ["articlePage"]
        });
        var searchResult = CreateSearchResult();

        Assert.Multiple(() =>
        {
            Assert.That(permissiveSut.IsIncluded(searchResult), Is.True);
            Assert.That(restrictiveSut.IsIncluded(searchResult), Is.False);
        });
    }

    [Test]
    public void IsIncluded_WhenConfiguredPropertyMatches_ExcludesResultRegardlessOfContentType()
    {
        var sut = CreateSut(new XmlSitemapsOptions
        {
            IncludedContentTypeAliases = ["articlePage"],
            ExcludingUrlPropertyAlias = "metaRobots",
            ExcludingUrlPropertyValue = "noindex"
        });

        Assert.That(sut.IsIncluded(CreateSearchResult(
            ("__NodeTypeAlias", "articlePage"),
            ("metaRobots", "NOINDEX"))), Is.False);
    }

    private static ExamineSitemapSearchResultFilter CreateSut(XmlSitemapsOptions? options = null)
    {
        return new ExamineSitemapSearchResultFilter(Options.Create(options ?? new XmlSitemapsOptions()));
    }

    private static ISearchResult CreateSearchResult(params (string Key, string Value)[] values)
    {
        var searchResult = Substitute.For<ISearchResult>();
        searchResult.Values.Returns(values.ToDictionary(value => value.Key, value => value.Value));
        return searchResult;
    }
}
