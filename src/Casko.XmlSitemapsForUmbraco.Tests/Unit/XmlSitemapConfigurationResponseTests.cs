using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Package.Models;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class XmlSitemapConfigurationResponseTests
{
    [Test]
    public void FromOptions_WhenUsingDefaults_ReturnsDefaultConfigurationSummary()
    {
        var result = XmlSitemapConfigurationResponse.FromOptions(new XmlSitemapsOptions());

        Assert.Multiple(() =>
        {
            Assert.That(result.Enabled, Is.True);
            Assert.That(result.RewritesEnabled, Is.False);
            Assert.That(result.Mode, Is.EqualTo(XmlSitemapsMode.Single));
            Assert.That(result.RenderAlternateLinksForSingleCultureSitemaps, Is.False);
            Assert.That(result.RootNodeSearchLevel, Is.Zero);
            Assert.That(result.RootContentTypeAliases, Is.Empty);
            Assert.That(result.SitemapCount, Is.EqualTo(1));
            Assert.That(result.CustomSitemapCount, Is.Zero);
            Assert.That(result.IndexCount, Is.Zero);
            Assert.That(result.Storage.RefreshStaleAfterSeconds, Is.EqualTo(3600));
            Assert.That(result.Storage.BackgroundJobEnabled, Is.True);
            Assert.That(result.Storage.BackgroundJobIntervalSeconds, Is.EqualTo(3600));
            Assert.That(result.Sitemaps.Single().Key, Is.EqualTo("xmlsitemap"));
            Assert.That(result.Sitemaps.Single().PublicName, Is.EqualTo("xmlsitemap"));
            Assert.That(result.Sitemaps.Single().Path, Is.EqualTo("/"));
            Assert.That(result.CustomSitemaps, Is.Empty);
            Assert.That(result.Indexes, Is.Empty);
        });
    }

    [Test]
    public void FromOptions_MapsRootFiltersAndStorageSettings()
    {
        var result = XmlSitemapConfigurationResponse.FromOptions(new XmlSitemapsOptions
        {
            Enabled = false,
            RewritesEnabled = true,
            Mode = XmlSitemapsMode.Single,
            RenderAlternateLinksForSingleCultureSitemaps = true,
            RootNodeSearchLevel = 1,
            RootContentTypeAliases = ["homePage"],
            IncludedContentTypeAliases = ["homePage"],
            ExcludedContentTypeAliases = ["hiddenPage"],
            IncludedCultures = ["en", "da"],
            ExcludedCultures = ["pl"],
            ExcludingUrlPropertyAlias = "metaRobots",
            ExcludingUrlPropertyValue = "noindex",
            Storage =
            {
                RefreshStaleAfterSeconds = 120,
                BackgroundJob =
                {
                    Enabled = false,
                    IntervalSeconds = 600
                }
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Enabled, Is.False);
            Assert.That(result.RewritesEnabled, Is.True);
            Assert.That(result.Mode, Is.EqualTo(XmlSitemapsMode.Single));
            Assert.That(result.RenderAlternateLinksForSingleCultureSitemaps, Is.True);
            Assert.That(result.RootNodeSearchLevel, Is.EqualTo(1));
            Assert.That(result.RootContentTypeAliases, Is.EqualTo(new[] { "homePage" }));
            Assert.That(result.GlobalFilters.IncludedContentTypeAliases, Is.EqualTo(new[] { "homePage" }));
            Assert.That(result.GlobalFilters.ExcludedContentTypeAliases, Is.EqualTo(new[] { "hiddenPage" }));
            Assert.That(result.GlobalFilters.IncludedCultures, Is.EqualTo(new[] { "en", "da" }));
            Assert.That(result.GlobalFilters.ExcludedCultures, Is.EqualTo(new[] { "pl" }));
            Assert.That(result.GlobalFilters.ExcludingUrlPropertyAlias, Is.EqualTo("metaRobots"));
            Assert.That(result.GlobalFilters.ExcludingUrlPropertyValue, Is.EqualTo("noindex"));
            Assert.That(result.Storage.RefreshStaleAfterSeconds, Is.EqualTo(120));
            Assert.That(result.Storage.BackgroundJobEnabled, Is.False);
            Assert.That(result.Storage.BackgroundJobIntervalSeconds, Is.EqualTo(600));
        });
    }

    [Test]
    public void FromOptions_MapsConfiguredSitemapsCustomSitemapsAndIndexes()
    {
        var result = XmlSitemapConfigurationResponse.FromOptions(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions
                {
                    PublicName = "products-public",
                    Path = "/products",
                    HostName = "https://example.com",
                    Culture = "en",
                    IncludedCultures = ["en"],
                    ExcludedCultures = ["da"],
                    IncludedDocumentTypeAliases = ["productPage"],
                    ExcludedDocumentTypeAliases = ["archivedProduct"]
                }
            },
            CustomSitemaps =
            {
                ["external-products"] = new CustomSitemapOptions
                {
                    PublicName = "external-products-public",
                    ProviderAlias = "externalProducts",
                    HostName = "https://external.example.com",
                    Settings =
                    {
                        ["apiKey"] = "super-secret",
                        ["endpoint"] = "https://api.example.com"
                    }
                }
            },
            Indexes =
            {
                ["xmlsitemap"] = new SitemapIndexOptions
                {
                    PublicName = "xmlsitemap-public",
                    HostName = "https://example.com",
                    Sitemaps = ["products", "external-products"]
                }
            }
        });

        var sitemap = result.Sitemaps.Single();
        var customSitemap = result.CustomSitemaps.Single();
        var index = result.Indexes.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.SitemapCount, Is.EqualTo(1));
            Assert.That(result.CustomSitemapCount, Is.EqualTo(1));
            Assert.That(result.IndexCount, Is.EqualTo(1));

            Assert.That(sitemap.Key, Is.EqualTo("products"));
            Assert.That(sitemap.PublicName, Is.EqualTo("products-public"));
            Assert.That(sitemap.Path, Is.EqualTo("/products"));
            Assert.That(sitemap.HostName, Is.EqualTo("https://example.com"));
            Assert.That(sitemap.Culture, Is.EqualTo("en"));
            Assert.That(sitemap.IncludedCultures, Is.EqualTo(new[] { "en" }));
            Assert.That(sitemap.ExcludedCultures, Is.EqualTo(new[] { "da" }));
            Assert.That(sitemap.IncludedDocumentTypeAliases, Is.EqualTo(new[] { "productPage" }));
            Assert.That(sitemap.ExcludedDocumentTypeAliases, Is.EqualTo(new[] { "archivedProduct" }));

            Assert.That(customSitemap.Key, Is.EqualTo("external-products"));
            Assert.That(customSitemap.PublicName, Is.EqualTo("external-products-public"));
            Assert.That(customSitemap.ProviderAlias, Is.EqualTo("externalProducts"));
            Assert.That(customSitemap.HostName, Is.EqualTo("https://external.example.com"));
            Assert.That(customSitemap.SettingCount, Is.EqualTo(2));
            Assert.That(customSitemap.SettingKeys, Is.EqualTo(new[] { "apiKey", "endpoint" }));
            Assert.That(customSitemap.SettingKeys, Does.Not.Contain("super-secret"));
            Assert.That(customSitemap.SettingKeys, Does.Not.Contain("https://api.example.com"));

            Assert.That(index.Key, Is.EqualTo("xmlsitemap"));
            Assert.That(index.PublicName, Is.EqualTo("xmlsitemap-public"));
            Assert.That(index.HostName, Is.EqualTo("https://example.com"));
            Assert.That(index.Sitemaps, Is.EqualTo(new[] { "products", "external-products" }));
            Assert.That(index.PublicSitemaps, Is.EqualTo(new[] { "products-public", "external-products-public" }));
        });
    }

    [Test]
    public void FromOptions_WhenPublicNamesAreNotConfigured_UsesKeysAsPublicNames()
    {
        var result = XmlSitemapConfigurationResponse.FromOptions(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions()
            },
            CustomSitemaps =
            {
                ["external-products"] = new CustomSitemapOptions()
            },
            Indexes =
            {
                ["xmlsitemap"] = new SitemapIndexOptions
                {
                    Sitemaps = ["products", "external-products", "missing"]
                }
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Sitemaps.Single().PublicName, Is.EqualTo("products"));
            Assert.That(result.CustomSitemaps.Single().PublicName, Is.EqualTo("external-products"));
            Assert.That(result.Indexes.Single().PublicName, Is.EqualTo("xmlsitemap"));
            Assert.That(result.Indexes.Single().PublicSitemaps, Is.EqualTo(new[]
            {
                "products",
                "external-products",
                "missing"
            }));
        });
    }

    [Test]
    public void FromOptions_OrdersConfiguredRowsByKey()
    {
        var result = XmlSitemapConfigurationResponse.FromOptions(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["z-sitemap"] = new SitemapOptions(),
                ["a-sitemap"] = new SitemapOptions()
            },
            CustomSitemaps =
            {
                ["z-custom"] = new CustomSitemapOptions(),
                ["a-custom"] = new CustomSitemapOptions()
            },
            Indexes =
            {
                ["z-index"] = new SitemapIndexOptions(),
                ["a-index"] = new SitemapIndexOptions()
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Sitemaps.Select(row => row.Key), Is.EqualTo(new[] { "a-sitemap", "z-sitemap" }));
            Assert.That(result.CustomSitemaps.Select(row => row.Key), Is.EqualTo(new[] { "a-custom", "z-custom" }));
            Assert.That(result.Indexes.Select(row => row.Key), Is.EqualTo(new[] { "a-index", "z-index" }));
        });
    }
}
