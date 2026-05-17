using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Services.Cms;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class SitemapCultureSelectionTests
{
    [Test]
    public void Normalize_WhenCulturesMissing_DefaultsToAllCultures()
    {
        var result = SitemapCultureSelection.Normalize(null);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Normalize_WhenWildcardIncluded_KeepsOnlyWildcard()
    {
        var result = SitemapCultureSelection.Normalize(["da-DK", "*", "en-US"]);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Normalize_WhenCulturesEmpty_DefaultsToAllCultures()
    {
        var result = SitemapCultureSelection.Normalize([]);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void FilterAlternativeCultures_WhenWildcardIncluded_ReturnsAllCultures()
    {
        var result = SitemapCultureSelection.FilterAlternativeCultures(
            ["en-US", "da-DK"],
            ["*"]);

        Assert.That(result, Is.EqualTo(new[] { "en-US", "da-DK" }));
    }

    [Test]
    public void FilterAlternativeCultures_WhenCulturesMissing_ReturnsAllCultures()
    {
        var result = SitemapCultureSelection.FilterAlternativeCultures(
            ["en-US", "da-DK"],
            []);

        Assert.That(result, Is.EqualTo(new[] { "en-US", "da-DK" }));
    }

    [Test]
    public void FilterAlternativeCultures_WhenSpecificCulturesIncluded_FiltersToSelection()
    {
        var result = SitemapCultureSelection.FilterAlternativeCultures(
            ["en-US", "da-DK", "de-DE"],
            ["da-DK", "de-DE"]);

        Assert.That(result, Is.EqualTo(new[] { "da-DK", "de-DE" }));
    }

    [Test]
    public void Resolve_WhenRootIncludesCultures_AppliesToAllSitemaps()
    {
        var result = SitemapCultureSelection.Resolve(
            ["en", "da", "pl"],
            new XmlSitemapsOptions { IncludedCultures = ["en", "da"] });

        Assert.That(result.Cultures, Is.EqualTo(new[] { "en", "da" }));
    }

    [Test]
    public void Resolve_WhenSitemapIncludesCultures_AddsToRootIncludes()
    {
        var result = SitemapCultureSelection.Resolve(
            ["en", "da", "pl"],
            new XmlSitemapsOptions { IncludedCultures = ["en"] },
            new SitemapOptions { IncludedCultures = ["da"] });

        Assert.That(result.Cultures, Is.EqualTo(new[] { "en", "da" }));
    }

    [Test]
    public void Resolve_WhenRootExcludesCulture_RemovesItUnlessSitemapIncludesIt()
    {
        var result = SitemapCultureSelection.Resolve(
            ["en", "da", "pl"],
            new XmlSitemapsOptions
            {
                IncludedCultures = ["en", "da"],
                ExcludedCultures = ["da"]
            },
            new SitemapOptions { IncludedCultures = ["da"] });

        Assert.That(result.Cultures, Is.EqualTo(new[] { "en", "da" }));
    }

    [Test]
    public void Resolve_WhenSitemapExcludesCulture_RemovesItLast()
    {
        var result = SitemapCultureSelection.Resolve(
            ["en", "da", "pl"],
            new XmlSitemapsOptions { IncludedCultures = ["en"] },
            new SitemapOptions
            {
                IncludedCultures = ["da"],
                ExcludedCultures = ["da"]
            });

        Assert.That(result.Cultures, Is.EqualTo(new[] { "en" }));
    }

    [Test]
    public void Resolve_WhenCulturesNotIncluded_ReturnsAllAvailableExceptExcluded()
    {
        var result = SitemapCultureSelection.Resolve(
            ["en", "da", "pl"],
            new XmlSitemapsOptions { ExcludedCultures = ["pl"] });

        Assert.That(result.Cultures, Is.EqualTo(new[] { "en", "da" }));
    }

    [Test]
    public void Resolve_WhenSingleCultureAndRootSettingDisabled_DisablesAlternateLinks()
    {
        var result = SitemapCultureSelection.Resolve(
            ["en", "da"],
            new XmlSitemapsOptions { IncludedCultures = ["en"] });

        Assert.That(result.RenderAlternateLinks, Is.False);
    }

    [Test]
    public void Resolve_WhenSingleCultureAndRootSettingEnabled_EnablesAlternateLinks()
    {
        var result = SitemapCultureSelection.Resolve(
            ["en", "da"],
            new XmlSitemapsOptions
            {
                IncludedCultures = ["en"],
                RenderAlternateLinksForSingleCultureSitemaps = true
            });

        Assert.That(result.RenderAlternateLinks, Is.True);
    }
}
