using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.Examine;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Indexes;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.UrlSets;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using CommonXmlSitemapApiConstants = Casko.XmlSitemapsForUmbraco.Common.XmlSitemapApiConstants;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class ExamineXmlSitemapProviderTests
{
    private IPublishedContentService _publishedContentService = null!;
    private ICmsUrlService _cmsUrlService = null!;
    private IXmlSitemapCustomProvider _customProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _publishedContentService = Substitute.For<IPublishedContentService>();
        _cmsUrlService = Substitute.For<ICmsUrlService>();
        _customProvider = Substitute.For<IXmlSitemapCustomProvider>();
        _customProvider.Alias.Returns("external-products-provider");
    }

    [Test]
    public async Task GetByRootKeyAsync_WhenUrlsExist_RendersSitemapFromCmsUrls()
    {
        var rootKey = Guid.NewGuid();
        var lastModified = new DateTime(2026, 5, 14, 12, 0, 0, DateTimeKind.Utc);
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/en/products", lastModified, "https://example.com", "en", Id: 10)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions());

        var result = await sut.GetByRootKeyAsync(rootKey) as XmlSitemap;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Urls, Has.Count.EqualTo(1));
            Assert.That(result.Urls[0].Location, Is.EqualTo("https://example.com/en/products"));
            Assert.That(result.Urls[0].LastModified, Is.EqualTo(lastModified));
        });
    }

    [Test]
    public void GetByRootKeyAsync_WhenUrlsAreEmpty_ThrowsRootContentHasNoContentException()
    {
        var rootKey = Guid.NewGuid();
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([]);
        var sut = CreateProvider(new XmlSitemapsOptions());

        AsyncTestDelegate action = async () => await sut.GetByRootKeyAsync(rootKey);

        Assert.That(action, Throws.TypeOf<RootContentHasNoContentException>());
    }

    [Test]
    public async Task GetByPathAsync_ResolvesRootContentThenQueriesUrlsByRootKey()
    {
        var rootKey = Guid.NewGuid();
        var root = CreateContent(rootKey);
        _publishedContentService.GetContentByPath("/products", "https://example.com", "da").Returns(root);
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/da/produkter", new DateTime(2026, 5, 14), null, "da", Id: 10)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions());

        var result = await sut.GetByPathAsync("/products", "da", "https://example.com") as XmlSitemap;

        Assert.That(result!.Urls[0].Location, Is.EqualTo("https://example.com/da/produkter"));
        await _cmsUrlService.Received(1).GetUrlsByKeyAsync(rootKey);
    }

    [Test]
    public async Task GetConfiguredAsync_WhenKeyIsConfigured_ResolvesConfiguredRootAndHostname()
    {
        var rootKey = Guid.NewGuid();
        var root = CreateContent(rootKey);
        _publishedContentService.GetContentByPath("/products", "https://example.com", "da").Returns(root);
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/da/produkter", new DateTime(2026, 5, 14), null, "da", Id: 10)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions
                {
                    Path = "/products",
                    HostName = "https://example.com",
                    Culture = "da"
                }
            }
        });

        var result = await sut.GetConfiguredAsync("products") as XmlSitemap;

        Assert.That(result!.Urls[0].Location, Is.EqualTo("https://example.com/da/produkter"));
        _publishedContentService.Received(1).GetContentByPath("/products", "https://example.com", "da");
    }

    [Test]
    public void GetConfiguredAsync_WhenKeyIsMissing_ThrowsInvalidOperationException()
    {
        var sut = CreateProvider(new XmlSitemapsOptions());

        AsyncTestDelegate action = async () => await sut.GetConfiguredAsync("missing");

        Assert.That(action, Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task GetConfiguredAsync_WhenSingleModeUsesImplicitSitemapKey_ResolvesRootPath()
    {
        var rootKey = Guid.NewGuid();
        var root = CreateContent(rootKey);
        _publishedContentService.GetContentByPath("/").Returns(root);
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/", new DateTime(2026, 5, 14), "https://ignored.example.com", "en", Id: 10)
        ]);
        var sut = CreateProvider(
            new XmlSitemapsOptions(),
            webRoutingSettings: new WebRoutingSettings
            {
                UmbracoApplicationUrl = "https://example.com"
            });

        var result = await sut.GetConfiguredAsync(CommonXmlSitemapApiConstants.DefaultSitemapKey) as XmlSitemap;

        Assert.That(result!.Urls[0].Location, Is.EqualTo("https://example.com/"));
        _publishedContentService.Received(1).GetContentByPath("/");
    }

    [Test]
    public async Task GetConfiguredAsync_WhenKeyIsCustomSitemap_CallsConfiguredProviderWithContext()
    {
        var sitemap = new XmlSitemap();
        XmlSitemapCustomProviderContext? context = null;
        _customProvider
            .GetSitemapAsync(Arg.Do<XmlSitemapCustomProviderContext>(value => context = value))
            .Returns(sitemap);
        var sut = CreateProvider(new XmlSitemapsOptions
        {
            CustomSitemaps =
            {
                ["external-products"] = new CustomSitemapOptions
                {
                    ProviderAlias = "external-products-provider",
                    HostName = "custom.example.com",
                    Settings =
                    {
                        ["FeedId"] = "products"
                    }
                }
            }
        }, [_customProvider]);

        var result = await sut.GetConfiguredAsync("external-products");

        Assert.That(result, Is.SameAs(sitemap));
        Assert.That(context, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(context!.Key, Is.EqualTo("external-products"));
            Assert.That(context.HostName, Is.EqualTo("custom.example.com"));
            Assert.That(context.Settings["FeedId"], Is.EqualTo("products"));
        });
    }

    [Test]
    public void GetIndex_WhenPublicAliasesAreConfigured_UsesPublicAliasesAndDeduplicatesLocations()
    {
        var sut = CreateProvider(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["host1-main"] = new SitemapOptions { PublicName = "xmlsitemap" },
                ["host2-main"] = new SitemapOptions { PublicName = "xmlsitemap" },
                ["news"] = new SitemapOptions { PublicName = "news-public" }
            },
            Indexes =
            {
                ["main"] = new SitemapIndexOptions
                {
                    HostName = "https://example.com",
                    Sitemaps = ["host1-main", "host2-main", "news"]
                }
            }
        });

        var result = sut.GetIndex("main") as XmlSitemapIndex;

        Assert.That(result!.Locations.Select(location => location.Location), Is.EqualTo(new[]
        {
            "https://example.com/xmlsitemap.xml",
            "https://example.com/news-public.xml"
        }));
    }

    [Test]
    public void RenderUrls_UsesHostnameOverrideAndRendersAlternateLinksById()
    {
        var renderer = new ExamineUrlRenderer(new XmlSitemapUrlBuilder());
        var lastModified = new DateTime(2026, 5, 14);
        var urls = new[]
        {
            new CmsUrl("/da/produkter", lastModified, "https://ignored.example.com", "da", Id: 10),
            new CmsUrl("https://ignored.example.com/en/products", lastModified, "https://ignored.example.com", "en", Id: 10)
        };

        var result = renderer.Render(urls, "da", ["da", "en"], "https://example.com", renderAlternateLinks: true).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Location, Is.EqualTo("https://example.com/da/produkter"));
            Assert.That(result[0].CultureLinks!.Select(link => link.HrefLang), Is.EqualTo(new[] { "da", "en" }));
            Assert.That(result[0].CultureLinks!.Select(link => link.Href), Is.EqualTo(new[]
            {
                "https://example.com/da/produkter",
                "https://example.com/en/products"
            }));
        });
    }

    [Test]
    public async Task GetByRootKeyAsync_WhenSingleCultureAlternateLinksAreDisabled_RendersNoCultureLinks()
    {
        var rootKey = Guid.NewGuid();
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/da/produkter", new DateTime(2026, 5, 14), "https://example.com", "da", Id: 10)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions
        {
            IncludedCultures = ["da"]
        });

        var result = await sut.GetByRootKeyAsync(rootKey) as XmlSitemap;

        Assert.That(result!.Urls[0].CultureLinks, Is.Empty);
    }

    [Test]
    public async Task GetByRootKeyAsync_WhenUrlsHaveSameLocation_DeduplicatesCaseInsensitively()
    {
        var rootKey = Guid.NewGuid();
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/same", new DateTime(2026, 5, 14), null, "en", Id: 10),
            new CmsUrl("/SAME", new DateTime(2026, 5, 15), null, "en", Id: 20)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions());

        var result = await sut.GetByRootKeyAsync(rootKey) as XmlSitemap;

        Assert.That(result!.Urls, Has.Count.EqualTo(1));
        Assert.That(result.Urls[0].Location, Is.EqualTo("/SAME"));
    }

    private ExamineXmlSitemapProvider CreateProvider(
        XmlSitemapsOptions options,
        IEnumerable<IXmlSitemapCustomProvider>? customProviders = null,
        WebRoutingSettings? webRoutingSettings = null)
    {
        var urlBuilder = new XmlSitemapUrlBuilder();
        var urlSetRenderer = new XmlSitemapUrlSetRenderer();

        return new ExamineXmlSitemapProvider(
            Options.Create(webRoutingSettings ?? new WebRoutingSettings()),
            Options.Create(options),
            _publishedContentService,
            _cmsUrlService,
            new ExamineXmlSitemapRenderer(new ExamineUrlRenderer(urlBuilder), urlSetRenderer),
            new XmlSitemapIndexRenderer(urlBuilder),
            customProviders ?? []);
    }

    private static IPublishedContent CreateContent(Guid key)
    {
        var content = Substitute.For<IPublishedContent>();
        content.Key.Returns(key);
        return content;
    }
}
