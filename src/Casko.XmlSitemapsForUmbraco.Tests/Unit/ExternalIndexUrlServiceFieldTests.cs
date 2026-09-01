using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Examine;
using NSubstitute;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class ExternalIndexUrlServiceFieldTests
{
    [Test]
    public void IsPublishedForCulture_WhenResultHasOnlyInvariantPublicationField_IncludesPublishedContent()
    {
        var searchResult = CreateSearchResult(("__Published", "y"));

        var isPublished = ExternalIndexUrlService.IsPublishedForCulture(searchResult, "da");

        Assert.That(isPublished, Is.True);
    }

    [Test]
    public void IsPublishedForCulture_WhenCultureSpecificFieldExists_UsesThatField()
    {
        var searchResult = CreateSearchResult(
            ("__Published", "y"),
            ("__Published_da", "n"));

        var isPublished = ExternalIndexUrlService.IsPublishedForCulture(searchResult, "da");

        Assert.That(isPublished, Is.False);
    }

    private static ISearchResult CreateSearchResult(params (string Key, string Value)[] values)
    {
        var searchResult = Substitute.For<ISearchResult>();
        searchResult.Values.Returns(values.ToDictionary(value => value.Key, value => value.Value));
        return searchResult;
    }
}
