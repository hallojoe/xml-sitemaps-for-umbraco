using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class SitemapRewriteDefinitionServiceTests
{
    [Test]
    public void GetDefinitions_CreatesDefinitionsForConfiguredSitemapsAndIndexes()
    {
        var sut = CreateService(new XmlSitemapsOptions
        {
            Indexes =
            {
                ["xmlsitemap"] = new SitemapIndexOptions { HostName = "https://host.dk" }
            },
            Sitemaps =
            {
                ["xmlsitemap-host-dk-en"] = new SitemapOptions { HostName = "host.dk" }
            },
            CustomSitemaps =
            {
                ["external-products"] = new CustomSitemapOptions
                {
                    ProviderAlias = "external-products-provider",
                    HostName = "custom.dk"
                }
            }
        });

        var result = sut.GetDefinitions();

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result, Has.Some.Matches<SitemapRewriteDefinition>(definition =>
            definition.Path == "/xmlsitemap.xml" &&
            definition.TargetPath == "/api/sitemap/index/key?key=xmlsitemap" &&
            definition.Kind == SitemapRewriteKind.SitemapIndex &&
            definition.HostName == "host.dk"));
        Assert.That(result, Has.Some.Matches<SitemapRewriteDefinition>(definition =>
            definition.Path == "/xmlsitemap-host-dk-en.xml" &&
            definition.TargetPath == "/api/sitemap/key?key=xmlsitemap-host-dk-en" &&
            definition.Kind == SitemapRewriteKind.Sitemap &&
            definition.HostName == "host.dk"));
        Assert.That(result, Has.Some.Matches<SitemapRewriteDefinition>(definition =>
            definition.Path == "/external-products.xml" &&
            definition.TargetPath == "/api/sitemap/key?key=external-products" &&
            definition.Kind == SitemapRewriteKind.Sitemap &&
            definition.HostName == "custom.dk"));
    }

    [Test]
    public void GetDefinitions_WhenIndexAndSitemapKeysCollide_PrefersIndexDefinition()
    {
        var sut = CreateService(new XmlSitemapsOptions
        {
            Indexes =
            {
                ["xmlsitemap"] = new SitemapIndexOptions()
            },
            Sitemaps =
            {
                ["xmlsitemap"] = new SitemapOptions()
            }
        });

        var result = sut.GetDefinitions();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().Kind, Is.EqualTo(SitemapRewriteKind.SitemapIndex));
    }

    [Test]
    public void GetDefinitions_WhenRegularAndCustomSitemapKeysCollide_PrefersRegularSitemapDefinition()
    {
        var sut = CreateService(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["xmlsitemap"] = new SitemapOptions { HostName = "regular.dk" }
            },
            CustomSitemaps =
            {
                ["xmlsitemap"] = new CustomSitemapOptions
                {
                    ProviderAlias = "custom-provider",
                    HostName = "custom.dk"
                }
            }
        });

        var result = sut.GetDefinitions();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().HostName, Is.EqualTo("regular.dk"));
    }

    [Test]
    public void TryMatch_WhenHostMatches_ReturnsDefinition()
    {
        var sut = CreateService(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["xmlsitemap-host-dk-en"] = new SitemapOptions { HostName = "host.dk" }
            }
        });

        var matched = sut.TryMatch(
            new PathString("/XMLSITEMAP-HOST-DK-EN.xml"),
            new HostString("host.dk"),
            out var definition);

        Assert.That(matched, Is.True);
        Assert.That(definition?.Key, Is.EqualTo("xmlsitemap-host-dk-en"));
    }

    [Test]
    public void TryMatch_WhenHostDoesNotMatch_ReturnsFalse()
    {
        var sut = CreateService(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["xmlsitemap-host-dk-en"] = new SitemapOptions { HostName = "host.dk" }
            }
        });

        var matched = sut.TryMatch(
            new PathString("/xmlsitemap-host-dk-en.xml"),
            new HostString("other.dk"),
            out var definition);

        Assert.That(matched, Is.False);
        Assert.That(definition, Is.Null);
    }

    [Test]
    public void TryMatch_WhenDefinitionHasNoHost_MatchesAnyHost()
    {
        var sut = CreateService(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["xmlsitemap"] = new SitemapOptions()
            }
        });

        var matched = sut.TryMatch(
            new PathString("/xmlsitemap.xml"),
            new HostString("anything.dk"),
            out var definition);

        Assert.That(matched, Is.True);
        Assert.That(definition?.Key, Is.EqualTo("xmlsitemap"));
    }

    [Test]
    public void NormalizeHostName_WhenHostHasSchemeAndPort_KeepsHostAndPort()
    {
        var result = SitemapRewriteDefinitionService.NormalizeHostName("https://host.dk:1234/");

        Assert.That(result, Is.EqualTo("host.dk:1234"));
    }

    private static SitemapRewriteDefinitionService CreateService(XmlSitemapsOptions options)
    {
        return new SitemapRewriteDefinitionService(Options.Create(options));
    }
}
