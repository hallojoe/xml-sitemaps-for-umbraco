using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.Examine;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Routing;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.Rendering.Indexes;
using Casko.XmlSitemapsForUmbraco.Providers.Rendering.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.Rendering.UrlSets;
using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using CommonXmlSitemapApiConstants = Casko.XmlSitemapsForUmbraco.Common.XmlSitemapApiConstants;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class ExamineXmlSitemapProviderTests
{
    private IExamineSitemapRootResolver _sitemapRootResolver = null!;
    private IHostUrlProvider _hostUrlProvider = null!;
    private ICmsUrlService _cmsUrlService = null!;
    private IDocumentUrlService _documentUrlService = null!;
    private IXmlSitemapCustomProvider _customProvider = null!;
    private CapturingLogger<ExamineXmlSitemapProvider> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _sitemapRootResolver = Substitute.For<IExamineSitemapRootResolver>();
        _hostUrlProvider = Substitute.For<IHostUrlProvider>();
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([]));
        _cmsUrlService = Substitute.For<ICmsUrlService>();
        _documentUrlService = Substitute.For<IDocumentUrlService>();
        _customProvider = Substitute.For<IXmlSitemapCustomProvider>();
        _customProvider.Alias.Returns("external-products-provider");
        _logger = new CapturingLogger<ExamineXmlSitemapProvider>();
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
    public async Task GetByRootKeyAsync_WhenHostUrlMatchesRoot_UsesHostUrlHostnameAndDefaultCulture()
    {
        var rootKey = Guid.NewGuid();
        var lastModified = new DateTime(2026, 8, 2, 12, 0, 0, DateTimeKind.Utc);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([
            new HostUrl(new Uri("https://localhost:56317/en/"), "en", 10, rootKey, true)
        ]));
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/text-page", lastModified, null, "en", Id: 10)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions());

        var result = await sut.GetByRootKeyAsync(rootKey) as XmlSitemap;

        Assert.That(result!.Urls[0].Location, Is.EqualTo("https://localhost:56317/en/text-page"));
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
        _sitemapRootResolver.ResolveAsync("/products", "https://example.com", "da").Returns(
            new ExamineSitemapRoot(rootKey, new HostUrl(new Uri("https://example.com"), "da", 10, rootKey, true)));
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
        _sitemapRootResolver.ResolveAsync("/products", "https://example.com", "da").Returns(
            new ExamineSitemapRoot(rootKey, new HostUrl(new Uri("https://example.com"), "da", 10, rootKey, true)));
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/da/produkter", new DateTime(2026, 5, 14), null, "da", Id: 10)
        ]);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([
            new HostUrl(new Uri("https://ignored.example.com"), "en", 10, rootKey, true)
        ]));
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
        await _sitemapRootResolver.Received(1).ResolveAsync("/products", "https://example.com", "da");
    }

    [Test]
    public async Task GetConfiguredAsync_WhenOnlyEnglishIsSelected_ExcludesDanishOnlyDescendants()
    {
        var rootKey = Guid.NewGuid();
        _sitemapRootResolver.ResolveAsync("/area/section-2/", "https://website1.dev.localhost/en/", "en")
            .Returns(new ExamineSitemapRoot(rootKey, new HostUrl(
                new Uri("https://website1.dev.localhost/en/"), "en", 10, rootKey, true)));
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/area/section-2/", new DateTime(2026, 8, 17), "https://website1.dev.localhost/en", "en", Id: 1),
            new CmsUrl("/omraade/sektion-2/", new DateTime(2026, 8, 17), "https://website1.dev.localhost", "da", Id: 1),
            new CmsUrl("/omraade/sektion-2/tekst-sub-side-21/test-side-0/", new DateTime(2026, 8, 17), "https://website1.dev.localhost", "da", Id: 2)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions
        {
            IncludedCultures = ["en", "da", "pl"],
            RenderAlternateLinksForSingleCultureSitemaps = true,
            Sitemaps =
            {
                ["area-2-en"] = new SitemapOptions
                {
                    Path = "/area/section-2/",
                    HostName = "https://website1.dev.localhost/en/",
                    Culture = "en",
                    IncludedCultures = ["en"],
                    ExcludedCultures = ["da", "pl"]
                }
            }
        });

        var result = await sut.GetConfiguredAsync("area-2-en") as XmlSitemap;

        Assert.That(result!.Urls, Has.Count.EqualTo(1));
        Assert.That(result.Urls[0].Location, Is.EqualTo("https://website1.dev.localhost/en/area/section-2/"));
        Assert.That(result.Urls[0].CultureLinks!.Single().HrefLang, Is.EqualTo("en"));
    }

    [Test]
    public async Task GetConfiguredAsync_WhenMultipleCulturesAreSelected_RendersSelectedCultureVariantsAndAlternateLinks()
    {
        var rootKey = Guid.NewGuid();
        _sitemapRootResolver.ResolveAsync("/products", "https://example.com/en", "en")
            .Returns(new ExamineSitemapRoot(rootKey, new HostUrl(
                new Uri("https://example.com/en"), "en", 10, rootKey, true)));
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/produkter", new DateTime(2026, 8, 17), "https://example.com", "da", Id: 1),
            new CmsUrl("/products", new DateTime(2026, 8, 17), "https://example.com/en", "en", Id: 1),
            new CmsUrl("/produkty", new DateTime(2026, 8, 17), "https://example.com/pl", "pl", Id: 1)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions
                {
                    Path = "/products",
                    HostName = "https://example.com/en",
                    Culture = "en",
                    IncludedCultures = ["en", "da"]
                }
            }
        });

        var result = await sut.GetConfiguredAsync("products") as XmlSitemap;

        Assert.That(result!.Urls, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result.Urls[0].Location, Is.EqualTo("https://example.com/en/products"));
            Assert.That(result.Urls[0].CultureLinks!.Select(link => link.HrefLang), Is.EqualTo(new[] { "en", "da" }));
            Assert.That(result.Urls[0].CultureLinks!.Select(link => link.Href), Is.EqualTo(new[]
            {
                "https://example.com/en/products",
                "https://example.com/produkter"
            }));
        });
    }

    [Test]
    public async Task GetConfiguredAsync_WhenNoUrlsMatchSelectedCultures_ReturnsEmptySitemap()
    {
        var rootKey = Guid.NewGuid();
        _sitemapRootResolver.ResolveAsync("/area/section-2/", "https://website1.dev.localhost/en/", "en")
            .Returns(new ExamineSitemapRoot(rootKey, new HostUrl(
                new Uri("https://website1.dev.localhost/en/"), "en", 10, rootKey, true)));
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/omraade/sektion-2/", new DateTime(2026, 8, 17), "https://website1.dev.localhost", "da", Id: 1)
        ]);
        var sut = CreateProvider(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["area-2-en"] = new SitemapOptions
                {
                    Path = "/area/section-2/",
                    HostName = "https://website1.dev.localhost/en/",
                    Culture = "en",
                    IncludedCultures = ["en"]
                }
            }
        });

        var result = await sut.GetConfiguredAsync("area-2-en") as XmlSitemap;

        Assert.That(result!.Urls, Is.Empty);
    }

    [Test]
    public void GetConfiguredAsync_WhenKeyIsMissing_ThrowsInvalidOperationException()
    {
        var sut = CreateProvider(new XmlSitemapsOptions());

        AsyncTestDelegate action = async () => await sut.GetConfiguredAsync("missing");

        Assert.That(action, Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void GetConfiguredAsync_WhenConfiguredRootCannotResolve_LogsContextAndThrowsRootContentNotFoundException()
    {
        _sitemapRootResolver.ResolveAsync("/missing", "https://example.com", "da")
            .Returns((ExamineSitemapRoot?)null);
        var sut = CreateProvider(new XmlSitemapsOptions
        {
            Sitemaps =
            {
                ["products"] = new SitemapOptions
                {
                    Path = "/missing",
                    HostName = "https://example.com",
                    Culture = "da"
                }
            }
        });

        AsyncTestDelegate action = async () => await sut.GetConfiguredAsync("products");

        Assert.That(action, Throws.TypeOf<RootContentNotFoundException>());
        Assert.That(_logger.Entries, Has.Some.Matches<CapturedLogEntry>(entry =>
            entry.LogLevel == LogLevel.Debug &&
            entry.Properties["SitemapKey"].Equals("products") &&
            entry.Properties["Path"].Equals("/missing") &&
            entry.Properties["HostName"].Equals("https://example.com") &&
            entry.Properties["Culture"].Equals("da")));
        Assert.That(_logger.Entries, Has.Some.Matches<CapturedLogEntry>(entry =>
            entry.LogLevel == LogLevel.Information &&
            entry.Properties["SitemapKey"].Equals("products") &&
            entry.Properties["Path"].Equals("/missing") &&
            entry.Properties["HostName"].Equals("https://example.com") &&
            entry.Properties["Culture"].Equals("da")));
    }

    [Test]
    public async Task GetConfiguredAsync_WhenSingleModeUsesImplicitSitemapKey_ResolvesRootPath()
    {
        var rootKey = Guid.NewGuid();
        _sitemapRootResolver.ResolveAsync("/").Returns(
            new ExamineSitemapRoot(rootKey, new HostUrl(new Uri("https://example.com"), "en", 10, rootKey, true)));
        _cmsUrlService.GetUrlsByKeyAsync(rootKey).Returns([
            new CmsUrl("/", new DateTime(2026, 5, 14), "https://ignored.example.com", "en", Id: 10)
        ]);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([
            new HostUrl(new Uri("https://example.com"), "en", 10, rootKey, true)
        ]));
        var sut = CreateProvider(new XmlSitemapsOptions());

        var result = await sut.GetConfiguredAsync(CommonXmlSitemapApiConstants.DefaultSitemapKey) as XmlSitemap;

        Assert.That(result!.Urls[0].Location, Is.EqualTo("https://example.com/"));
        await _sitemapRootResolver.Received(1).ResolveAsync("/");
    }

    [Test]
    public void GetByPathAsync_WhenPathCannotResolve_ThrowsRootContentNotFoundException()
    {
        _sitemapRootResolver.ResolveAsync("/missing", "https://example.com", "da").Returns((ExamineSitemapRoot?)null);
        var sut = CreateProvider(new XmlSitemapsOptions());

        AsyncTestDelegate action = async () => await sut.GetByPathAsync("/missing", "da", "https://example.com");

        Assert.That(action, Throws.TypeOf<RootContentNotFoundException>());
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
    public void RenderUrls_UsesHostnameOverrideForLocationAndCmsHostnamesForAlternateLinks()
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
                "https://ignored.example.com/da/produkter",
                "https://ignored.example.com/en/products"
            }));
        });
    }

    [Test]
    public void RenderUrls_WhenCultureHostnamesContainPaths_PreservesCultureHostnamePathsInAlternateLinks()
    {
        var renderer = new ExamineUrlRenderer(new XmlSitemapUrlBuilder());
        var lastModified = new DateTime(2026, 8, 1);
        var urls = new[]
        {
            new CmsUrl("/", lastModified, "https://localhost:56317", "da", Id: 10),
            new CmsUrl("/", lastModified, "https://localhost:56317/en", "en", Id: 10),
            new CmsUrl("/", lastModified, "https://localhost:56317/pl", "pl", Id: 10),
            new CmsUrl("/tekst-side", lastModified, "https://localhost:56317", "da", Id: 20),
            new CmsUrl("/text-page", lastModified, "https://localhost:56317/en", "en", Id: 20),
            new CmsUrl("/pl-page-1", lastModified, "https://localhost:56317/pl", "pl", Id: 20)
        };

        var result = renderer.Render(urls, "da", ["da", "en", "pl"], "https://localhost:56317", renderAlternateLinks: true).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Location, Is.EqualTo("https://localhost:56317/"));
            Assert.That(result[0].CultureLinks!.Select(link => link.Href), Is.EqualTo(new[]
            {
                "https://localhost:56317/",
                "https://localhost:56317/en/",
                "https://localhost:56317/pl/"
            }));
            Assert.That(result[1].Location, Is.EqualTo("https://localhost:56317/tekst-side"));
            Assert.That(result[1].CultureLinks!.Select(link => link.Href), Is.EqualTo(new[]
            {
                "https://localhost:56317/tekst-side",
                "https://localhost:56317/en/text-page",
                "https://localhost:56317/pl/pl-page-1"
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

    [Test]
    public async Task RootResolver_WhenPathIsRoot_ReturnsSelectedHostUrlKey()
    {
        var rootKey = Guid.NewGuid();
        var hostUrl = new HostUrl(new Uri("https://example.com"), "da", 10, rootKey, true);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([hostUrl]));
        var sut = new ExamineSitemapRootResolver(_hostUrlProvider, _documentUrlService);

        var result = await sut.ResolveAsync("/", "https://example.com", "da");

        Assert.That(result, Is.EqualTo(new ExamineSitemapRoot(rootKey, hostUrl)));
        _documentUrlService.DidNotReceiveWithAnyArgs().GetDocumentKeyByRoute(default!, default, default, default);
    }

    [Test]
    public async Task RootResolver_WhenPathIsBelowRoot_UsesDocumentUrlServiceWithSelectedHostId()
    {
        var rootKey = Guid.NewGuid();
        var sectionKey = Guid.NewGuid();
        var hostUrl = new HostUrl(new Uri("https://example.com"), "da", 10, rootKey, true);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([hostUrl]));
        _documentUrlService.GetDocumentKeyByRoute("/om-os", "da", 10, false).Returns(sectionKey);
        var sut = new ExamineSitemapRootResolver(_hostUrlProvider, _documentUrlService);

        var result = await sut.ResolveAsync("/om-os", "https://example.com", "da");

        Assert.That(result, Is.EqualTo(new ExamineSitemapRoot(sectionKey, hostUrl)));
        _documentUrlService.Received(1).GetDocumentKeyByRoute("/om-os", "da", 10, false);
    }

    [Test]
    public async Task RootResolver_WhenHostnamesContainPaths_SelectsMatchingPathfulHost()
    {
        var rootKey = Guid.NewGuid();
        var sectionKey = Guid.NewGuid();
        var defaultHost = new HostUrl(new Uri("https://example.com"), "da", 10, Guid.NewGuid(), true);
        var pathHost = new HostUrl(new Uri("https://example.com/en/"), "en", 20, rootKey, false);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([defaultHost, pathHost]));
        _documentUrlService.GetDocumentKeyByRoute("/news", "en", 20, false).Returns(sectionKey);
        var sut = new ExamineSitemapRootResolver(_hostUrlProvider, _documentUrlService);

        var result = await sut.ResolveAsync("/news", "https://example.com/en", "en");

        Assert.That(result, Is.EqualTo(new ExamineSitemapRoot(sectionKey, pathHost)));
    }

    [Test]
    public async Task RootResolver_WhenMultipleHostUrlsMatchHostname_PrefersMatchingCulture()
    {
        var englishKey = Guid.NewGuid();
        var defaultHost = new HostUrl(new Uri("https://example.com"), "da", 10, Guid.NewGuid(), true);
        var englishHost = new HostUrl(new Uri("https://example.com"), "en", 20, englishKey, false);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([defaultHost, englishHost]));
        var sut = new ExamineSitemapRootResolver(_hostUrlProvider, _documentUrlService);

        var result = await sut.ResolveAsync("/", "https://example.com", "en");

        Assert.That(result, Is.EqualTo(new ExamineSitemapRoot(englishKey, englishHost)));
    }

    [Test]
    public async Task RootResolver_WhenHostnameDoesNotMatch_ReturnsNull()
    {
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([
            new HostUrl(new Uri("https://example.com"), "da", 10, Guid.NewGuid(), true)
        ]));
        var sut = new ExamineSitemapRootResolver(_hostUrlProvider, _documentUrlService);

        var result = await sut.ResolveAsync("/", "https://unknown.example.com", "da");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task RootResolver_WhenDocumentUrlServiceCannotResolvePath_ReturnsNull()
    {
        var hostUrl = new HostUrl(new Uri("https://example.com"), "da", 10, Guid.NewGuid(), true);
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([hostUrl]));
        _documentUrlService.GetDocumentKeyByRoute("/missing", "da", 10, false).Returns((Guid?)null);
        var sut = new ExamineSitemapRootResolver(_hostUrlProvider, _documentUrlService);

        var result = await sut.ResolveAsync("/missing", "https://example.com", "da");

        Assert.That(result, Is.Null);
    }

    private ExamineXmlSitemapProvider CreateProvider(
        XmlSitemapsOptions options,
        IEnumerable<IXmlSitemapCustomProvider>? customProviders = null)
    {
        var urlBuilder = new XmlSitemapUrlBuilder();
        var urlSetRenderer = new XmlSitemapUrlSetRenderer();

        return new ExamineXmlSitemapProvider(
            Options.Create(options),
            _sitemapRootResolver,
            _hostUrlProvider,
            _cmsUrlService,
            new ExamineXmlSitemapRenderer(new ExamineUrlRenderer(urlBuilder), urlSetRenderer),
            new XmlSitemapIndexRenderer(urlBuilder),
            customProviders ?? [],
            _logger);
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<CapturedLogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state is IReadOnlyList<KeyValuePair<string, object?>> values
                ? values.ToDictionary(value => value.Key, value => value.Value)
                : new Dictionary<string, object?>();
            Entries.Add(new CapturedLogEntry(logLevel, properties));
        }
    }

    private sealed record CapturedLogEntry(
        LogLevel LogLevel,
        IReadOnlyDictionary<string, object?> Properties);
}
