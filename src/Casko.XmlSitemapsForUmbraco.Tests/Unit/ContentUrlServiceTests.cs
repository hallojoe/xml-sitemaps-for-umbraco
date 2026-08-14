using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class ExternalIndexUrlServiceTests
{
    [Test]
    public void ResolveUrl_WhenLegacyPathHasNumericPrefix_RemovesPrefixAndUsesFallbackHostname()
    {
        var result = ExternalIndexUrlService.ResolveUrl(
            "/1062/some-path",
            "da",
            [],
            "https://localhost:44317/");

        Assert.Multiple(() =>
        {
            Assert.That(result.UrlPath, Is.EqualTo("/some-path"));
            Assert.That(result.Hostname, Is.EqualTo("https://localhost:44317"));
        });
    }

    [Test]
    public void ResolveUrl_WhenCultureDomainsExist_UsesMatchingCultureDomain()
    {
        var domains = new[]
        {
            CreateDomain("da.example.com", "da"),
            CreateDomain("https://en.example.com", "en")
        };

        var daResult = ExternalIndexUrlService.ResolveUrl("/1062/tekst-side", "da", domains, "https://fallback.example.com");
        var enResult = ExternalIndexUrlService.ResolveUrl("/1062/text-page", "en", domains, "https://fallback.example.com");

        Assert.Multiple(() =>
        {
            Assert.That(daResult.UrlPath, Is.EqualTo("/tekst-side"));
            Assert.That(daResult.Hostname, Is.EqualTo("https://da.example.com"));
            Assert.That(enResult.UrlPath, Is.EqualTo("/text-page"));
            Assert.That(enResult.Hostname, Is.EqualTo("https://en.example.com"));
        });
    }

    [Test]
    public void ResolveUrl_WhenCultureDomainsContainPaths_PreservesPathfulHostnames()
    {
        var domains = new[]
        {
            CreateDomain("localhost:56317/en/", "en"),
            CreateDomain("https://localhost:56317/pl/", "pl")
        };

        var enResult = ExternalIndexUrlService.ResolveUrl("/1062/text-page", "en", domains, "https://localhost:56317");
        var plResult = ExternalIndexUrlService.ResolveUrl("/1062/pl-page-1", "pl", domains, "https://localhost:56317");

        Assert.Multiple(() =>
        {
            Assert.That(enResult.UrlPath, Is.EqualTo("/text-page"));
            Assert.That(enResult.Hostname, Is.EqualTo("https://localhost:56317/en"));
            Assert.That(plResult.UrlPath, Is.EqualTo("/pl-page-1"));
            Assert.That(plResult.Hostname, Is.EqualTo("https://localhost:56317/pl"));
        });
    }

    [Test]
    public void ResolveUrl_WhenCultureDomainsArePathOnly_CombinesWithFallbackApplicationOrigin()
    {
        var domains = new[]
        {
            CreateDomain("/", "da"),
            CreateDomain("/en/", "en"),
            CreateDomain("/pl/", "pl")
        };

        var daResult = ExternalIndexUrlService.ResolveUrl("/1062/tekst-side", "da", domains, "https://localhost:56317");
        var enResult = ExternalIndexUrlService.ResolveUrl("/1062/text-page", "en", domains, "https://localhost:56317");
        var plResult = ExternalIndexUrlService.ResolveUrl("/1062/pl-page-1", "pl", domains, "https://localhost:56317");

        Assert.Multiple(() =>
        {
            Assert.That(daResult.Hostname, Is.EqualTo("https://localhost:56317"));
            Assert.That(enResult.Hostname, Is.EqualTo("https://localhost:56317/en"));
            Assert.That(plResult.Hostname, Is.EqualTo("https://localhost:56317/pl"));
        });
    }

    [Test]
    public void ResolveUrl_WhenFirstPathSegmentIsNotNumeric_LeavesPathAndUsesFallbackHostname()
    {
        var result = ExternalIndexUrlService.ResolveUrl(
            "/some-path",
            "da",
            [],
            "https://example.com/");

        Assert.Multiple(() =>
        {
            Assert.That(result.UrlPath, Is.EqualTo("/some-path"));
            Assert.That(result.Hostname, Is.EqualTo("https://example.com"));
        });
    }

    [Test]
    public void ResolveUrl_WhenLegacyPathIsRootOnlyNumeric_ResolvesToRootPath()
    {
        var result = ExternalIndexUrlService.ResolveUrl(
            "/1062",
            "da",
            [],
            "https://example.com/");

        Assert.Multiple(() =>
        {
            Assert.That(result.UrlPath, Is.EqualTo("/"));
            Assert.That(result.Hostname, Is.EqualTo("https://example.com"));
        });
    }

    [Test]
    public void ResolveUrl_WhenUrlIsAbsolute_RemovesLegacyPrefixAndPreservesItsHostname()
    {
        var result = ExternalIndexUrlService.ResolveUrl(
            "https://existing.example.com/1062/some-path",
            "da",
            [],
            "https://fallback.example.com/");

        Assert.Multiple(() =>
        {
            Assert.That(result.UrlPath, Is.EqualTo("/some-path"));
            Assert.That(result.Hostname, Is.EqualTo("https://existing.example.com"));
        });
    }

    private static IDomain CreateDomain(string domainName, string culture, int sortOrder = 0)
    {
        var domain = Substitute.For<IDomain>();
        domain.DomainName.Returns(domainName);
        domain.LanguageIsoCode.Returns(culture);
        domain.SortOrder.Returns(sortOrder);
        return domain;
    }
}
