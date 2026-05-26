using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Services.Cms;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class SitemapPropertyExclusionSelectionTests
{
    [TestCase(null, "noindex")]
    [TestCase("metaRobots", null)]
    [TestCase("", "noindex")]
    [TestCase("metaRobots", "")]
    public void ShouldInclude_WhenRuleIsNotConfigured_IncludesContent(string? alias, string? value)
    {
        var content = Substitute.For<IPublishedContent>();
        var sut = SitemapPropertyExclusionSelection.Resolve(new XmlSitemapsOptions
        {
            ExcludingUrlPropertyAlias = alias,
            ExcludingUrlPropertyValue = value
        });

        var result = sut.ShouldInclude(content, "en-US");

        Assert.That(result, Is.True);
        content.DidNotReceiveWithAnyArgs().GetProperty(default!);
    }

    [Test]
    public void ShouldInclude_WhenPropertyIsMissing_IncludesContent()
    {
        var content = Substitute.For<IPublishedContent>();
        content.GetProperty("metaRobots").Returns((IPublishedProperty?)null);
        var sut = CreateSelection("metaRobots", "noindex");

        var result = sut.ShouldInclude(content, "en-US");

        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldInclude_WhenPropertyContainsConfiguredValue_ExcludesContent()
    {
        var content = CreateContentWithProperty("metaRobots", "noindex,nofollow");
        var sut = CreateSelection("metaRobots", "noindex");

        var result = sut.ShouldInclude(content, "en-US");

        Assert.That(result, Is.False);
    }

    [Test]
    public void ShouldInclude_WhenPropertyContainsConfiguredValueWithDifferentCasing_ExcludesContent()
    {
        var content = CreateContentWithProperty("metaRobots", "NoIndex,NoFollow");
        var sut = CreateSelection("metaRobots", "noindex");

        var result = sut.ShouldInclude(content, "en-US");

        Assert.That(result, Is.False);
    }

    [TestCase(true, "true", false)]
    [TestCase(false, "false", false)]
    [TestCase(false, "true", true)]
    public void ShouldInclude_WhenPropertyIsBoolean_MatchesBooleanText(bool propertyValue, string configuredValue, bool expected)
    {
        var content = CreateContentWithProperty("excludeFromSitemap", propertyValue);
        var sut = CreateSelection("excludeFromSitemap", configuredValue);

        var result = sut.ShouldInclude(content, "en-US");

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldInclude_PassesCultureToPropertyValueLookup()
    {
        var property = Substitute.For<IPublishedProperty>();
        property.GetValue("da-DK", null).Returns("noindex");
        var content = Substitute.For<IPublishedContent>();
        content.GetProperty("metaRobots").Returns(property);
        var sut = CreateSelection("metaRobots", "noindex");

        sut.ShouldInclude(content, "da-DK");

        property.Received(1).GetValue("da-DK", null);
    }

    private static SitemapPropertyExclusionSelection CreateSelection(string alias, string value)
    {
        return SitemapPropertyExclusionSelection.Resolve(new XmlSitemapsOptions
        {
            ExcludingUrlPropertyAlias = alias,
            ExcludingUrlPropertyValue = value
        });
    }

    private static IPublishedContent CreateContentWithProperty(string alias, object? value)
    {
        var property = Substitute.For<IPublishedProperty>();
        property.GetValue(Arg.Any<string?>(), null).Returns(value);
        var content = Substitute.For<IPublishedContent>();
        content.GetProperty(alias).Returns(property);

        return content;
    }
}
