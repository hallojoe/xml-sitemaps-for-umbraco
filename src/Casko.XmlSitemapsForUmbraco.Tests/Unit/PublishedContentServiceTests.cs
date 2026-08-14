using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class PublishedContentServiceTests
{
    private IUmbracoContextFactory _umbracoContextFactory = null!;
    private IDocumentUrlService _documentUrlService = null!;
    private IDocumentNavigationQueryService _documentNavigationQueryService = null!;
    private ILanguageService _languageService = null!;
    private IHostUrlProvider _hostUrlProvider = null!;
    private IPublishedContentCache _publishedContentCache = null!;

    [SetUp]
    public void SetUp()
    {
        _umbracoContextFactory = Substitute.For<IUmbracoContextFactory>();
        _documentUrlService = Substitute.For<IDocumentUrlService>();
        _documentNavigationQueryService = Substitute.For<IDocumentNavigationQueryService>();
        _languageService = Substitute.For<ILanguageService>();
        _hostUrlProvider = Substitute.For<IHostUrlProvider>();
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>([]));
        _publishedContentCache = Substitute.For<IPublishedContentCache>();

        var umbracoContext = Substitute.For<IUmbracoContext>();
        umbracoContext.Content.Returns(_publishedContentCache);

        var contextAccessor = Substitute.For<IUmbracoContextAccessor>();
        var contextReference = new UmbracoContextReference(umbracoContext, false, contextAccessor);
        _umbracoContextFactory.EnsureUmbracoContext().Returns(contextReference);
    }

    [Test]
    public void GetRootContents_WhenRootNodeSearchLevelIsZero_ReturnsDirectRoots()
    {
        var root = CreateContent(100, "home");
        ConfigureNavigationRoots(root);
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });

        var result = sut.GetRootContents().ToArray();

        Assert.That(result, Is.EqualTo(new[] { root }));
    }

    [Test]
    public void GetRootContents_WhenRootNodeSearchLevelIsZeroAndContentTypeAliasIsSet_FiltersDirectRoots()
    {
        var includedRoot = CreateContent(100, "home");
        var excludedRoot = CreateContent(101, "landingPage");
        ConfigureNavigationRoots(includedRoot, excludedRoot);
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });

        var result = sut.GetRootContents("home").ToArray();

        Assert.That(result, Is.EqualTo(new[] { includedRoot }));
    }

    [Test]
    public void GetRootContents_WhenRootNodeSearchLevelIsOne_ReturnsFirstLevelChildrenOfNavigationRoots()
    {
        var childOne = CreateContent(200, "home");
        var childTwo = CreateContent(201, "home");
        var rootContainer = CreateContent(100, "container");
        ConfigureNavigationRoots(rootContainer);
        ConfigureChildRoots(rootContainer, childOne, childTwo);
        var sut = CreateService(new XmlSitemapsOptions
        {
            Mode = XmlSitemapsMode.Configuration,
            RootNodeSearchLevel = 1
        });

        var result = sut.GetRootContents().ToArray();

        Assert.That(result, Is.EqualTo(new[] { childOne, childTwo }));
    }

    [Test]
    public void GetRootContents_WhenRootNodeSearchLevelIsOneAndChildrenAreMissing_ReturnsNoRoots()
    {
        var rootContainer = CreateContent(100, "container");
        ConfigureNavigationRoots(rootContainer);
        var sut = CreateService(new XmlSitemapsOptions
        {
            Mode = XmlSitemapsMode.Configuration,
            RootNodeSearchLevel = 1
        });

        var result = sut.GetRootContents().ToArray();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetRootContents_WhenRootNodeSearchLevelIsAboveOne_ThrowsClearException()
    {
        var root = CreateContent(100, "home");
        ConfigureNavigationRoots(root);
        var sut = CreateService(new XmlSitemapsOptions
        {
            Mode = XmlSitemapsMode.Configuration,
            RootNodeSearchLevel = 2
        });

        TestDelegate action = () => _ = sut.GetRootContents().ToArray();

        Assert.That(
            action,
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.EqualTo(
                    "The default ICmsContentService implementation only supports RootNodeSearchLevel values 0 and 1. Configure a custom ICmsContentService for deeper root structures."));
    }

    [Test]
    public void GetRootContents_WhenSingleModeUsesDefaultOptions_ReturnsFirstDiscoveredRoot()
    {
        var firstRoot = CreateContent(100, "landingPage");
        var secondRoot = CreateContent(101, "home");
        ConfigureNavigationRoots(firstRoot, secondRoot);
        var sut = CreateService(new XmlSitemapsOptions());

        var result = sut.GetRootContents().ToArray();

        Assert.That(result, Is.EqualTo(new[] { firstRoot }));
    }

    [Test]
    public void GetRootContents_WhenSingleModeHasRootContentTypeAliases_ReturnsFirstMatchingRoot()
    {
        var firstRoot = CreateContent(100, "landingPage");
        var matchingRoot = CreateContent(101, "home");
        var laterMatchingRoot = CreateContent(102, "home");
        ConfigureNavigationRoots(firstRoot, matchingRoot, laterMatchingRoot);
        var sut = CreateService(new XmlSitemapsOptions
        {
            RootContentTypeAliases = ["home"]
        });

        var result = sut.GetRootContents().ToArray();

        Assert.That(result, Is.EqualTo(new[] { matchingRoot }));
    }

    [Test]
    public void GetRootContents_WhenSingleModeSearchLevelIsOne_UsesFirstMatchingChildRoot()
    {
        var container = CreateContent(100, "container");
        var firstChild = CreateContent(200, "landingPage");
        var matchingChild = CreateContent(201, "home");
        ConfigureNavigationRoots(container);
        ConfigureChildRoots(container, firstChild, matchingChild);
        var sut = CreateService(new XmlSitemapsOptions
        {
            RootNodeSearchLevel = 1,
            RootContentTypeAliases = ["home"]
        });

        var result = sut.GetRootContents().ToArray();

        Assert.That(result, Is.EqualTo(new[] { matchingChild }));
    }

    [Test]
    public void GetRootContents_WhenSingleModeHasNoMatchingAlias_ReturnsNoRoots()
    {
        var root = CreateContent(100, "landingPage");
        ConfigureNavigationRoots(root);
        var sut = CreateService(new XmlSitemapsOptions
        {
            RootContentTypeAliases = ["home"]
        });

        var result = sut.GetRootContents().ToArray();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetContentByPath_WhenHostnameIsEmpty_UsesTheFirstResolvedRoot()
    {
        var root = CreateContent(100, "home");
        ConfigureHostUrls(new HostUrl(new Uri("https://example.com/"), "en", root.Id, root.Key, true));
        _publishedContentCache.GetById(root.Key).Returns(root);
        var expectedContentKey = Guid.NewGuid();
        _documentUrlService
            .GetDocumentKeyByRoute("/", null, root.Id, false)
            .Returns(expectedContentKey);
        _publishedContentCache.GetById(false, expectedContentKey).Returns(root);
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });

        var result = sut.GetContentByPath("/", hostname: null);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(root));
            _documentUrlService.Received(1).GetDocumentKeyByRoute("/", null, root.Id, false);
        });
    }

    [Test]
    public void GetContentByPath_WhenMultipleHostUrlsMatchHostname_UsesTheMatchingRoot()
    {
        var firstRoot = CreateContent(100, "home");
        var matchingRoot = CreateContent(101, "home");
        ConfigureHostUrls(
            new HostUrl(new Uri("https://first.example.com/"), "en", firstRoot.Id, firstRoot.Key, true),
            new HostUrl(new Uri("https://match.example.com/"), "en", matchingRoot.Id, matchingRoot.Key, true));
        _publishedContentCache.GetById(firstRoot.Key).Returns(firstRoot);
        _publishedContentCache.GetById(matchingRoot.Key).Returns(matchingRoot);
        var expectedContentKey = Guid.NewGuid();
        _documentUrlService
            .GetDocumentKeyByRoute("/", null, matchingRoot.Id, false)
            .Returns(expectedContentKey);
        _publishedContentCache.GetById(false, expectedContentKey).Returns(matchingRoot);
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });

        var result = sut.GetContentByPath("/", hostname: "match.example.com");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(matchingRoot));
            _documentUrlService.Received(1).GetDocumentKeyByRoute("/", null, matchingRoot.Id, false);
        });
    }

    [Test]
    public void GetContentByPath_WhenHostnameContainsAPath_MatchesPathfulHostUrl()
    {
        var matchingRoot = CreateContent(201, "home");
        ConfigureHostUrls(new HostUrl(new Uri("https://match.example.com/en/"), "en", matchingRoot.Id, matchingRoot.Key, true));
        _publishedContentCache.GetById(matchingRoot.Key).Returns(matchingRoot);
        var expectedContentKey = Guid.NewGuid();
        _documentUrlService
            .GetDocumentKeyByRoute("/", null, matchingRoot.Id, false)
            .Returns(expectedContentKey);
        _publishedContentCache.GetById(false, expectedContentKey).Returns(matchingRoot);
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });

        var result = sut.GetContentByPath("/", hostname: "https://match.example.com/en");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(matchingRoot));
            _documentUrlService.Received(1).GetDocumentKeyByRoute("/", null, matchingRoot.Id, false);
        });
    }

    [Test]
    public void GetContent_WhenPublishedContentCacheIsSupplied_DoesNotEnsureUmbracoContext()
    {
        var key = Guid.NewGuid();
        var content = CreateContent(100, "home");
        _publishedContentCache.GetById(key).Returns(content);
        var sut = CreateService(new XmlSitemapsOptions());
        _umbracoContextFactory.ClearReceivedCalls();

        var result = sut.GetContent(key, _publishedContentCache);

        Assert.That(result, Is.SameAs(content));
        _umbracoContextFactory.DidNotReceive().EnsureUmbracoContext();
    }

    [Test]
    public void GetContent_WhenPublishedContentCacheIsNotSupplied_EnsuresUmbracoContext()
    {
        var key = Guid.NewGuid();
        var content = CreateContent(100, "home");
        _publishedContentCache.GetById(key).Returns(content);
        var sut = CreateService(new XmlSitemapsOptions());
        _umbracoContextFactory.ClearReceivedCalls();

        var result = sut.GetContent(key);

        Assert.That(result, Is.SameAs(content));
        _umbracoContextFactory.Received(1).EnsureUmbracoContext();
    }

    [Test]
    public void GetContentByPath_WhenPublishedContentCacheIsSupplied_DoesNotEnsureUmbracoContext()
    {
        var root = CreateContent(100, "home");
        ConfigureHostUrls(new HostUrl(new Uri("https://example.com/"), "en", root.Id, root.Key, true));
        _publishedContentCache.GetById(root.Key).Returns(root);
        var expectedContentKey = Guid.NewGuid();
        _documentUrlService
            .GetDocumentKeyByRoute("/", null, root.Id, false)
            .Returns(expectedContentKey);
        _publishedContentCache.GetById(false, expectedContentKey).Returns(root);
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });
        _umbracoContextFactory.ClearReceivedCalls();

        var result = sut.GetContentByPath("/", publishedContentCache: _publishedContentCache);

        Assert.That(result, Is.SameAs(root));
        _umbracoContextFactory.DidNotReceive().EnsureUmbracoContext();
    }

    [Test]
    public void GetContentByPath_WhenCultureIsSpecified_PrefersMatchingHostUrlCulture()
    {
        var defaultRoot = CreateContent(200, "home");
        var englishRoot = CreateContent(201, "home");
        ConfigureHostUrls(
            new HostUrl(new Uri("https://example.com/"), "da", defaultRoot.Id, defaultRoot.Key, true),
            new HostUrl(new Uri("https://example.com/en/"), "en", englishRoot.Id, englishRoot.Key, false));
        _publishedContentCache.GetById(defaultRoot.Key).Returns(defaultRoot);
        _publishedContentCache.GetById(englishRoot.Key).Returns(englishRoot);
        var expectedContentKey = Guid.NewGuid();
        _documentUrlService
            .GetDocumentKeyByRoute("/", "en", englishRoot.Id, false)
            .Returns(expectedContentKey);
        _publishedContentCache.GetById(false, expectedContentKey).Returns(englishRoot);
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });

        var result = sut.GetContentByPath("/", hostname: "https://example.com/en", culture: "en");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(englishRoot));
            _documentUrlService.Received(1).GetDocumentKeyByRoute("/", "en", englishRoot.Id, false);
        });
    }

    [Test]
    public void GetContentByPath_WhenHostnameDoesNotMatchAHostUrl_ThrowsNoRootContentException()
    {
        var root = CreateContent(200, "home");
        ConfigureHostUrls(new HostUrl(new Uri("https://first.example.com/"), "en", root.Id, root.Key, true));
        _publishedContentCache.GetById(root.Key).Returns(root);
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });

        TestDelegate action = () => sut.GetContentByPath("/", hostname: "unknown.example.com");

        Assert.That(action, Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("No content found at root."));
    }

    [Test]
    public void GetContentByPath_WhenNoHostUrlsExist_ThrowsNoRootContentException()
    {
        var sut = CreateService(new XmlSitemapsOptions { Mode = XmlSitemapsMode.Configuration });

        TestDelegate action = () => sut.GetContentByPath("/", hostname: "missing.example.com");

        Assert.That(action, Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("No content found at root."));
    }

    private PublishedContentService CreateService(XmlSitemapsOptions options)
    {
        return new PublishedContentService(
            Options.Create(options),
            _umbracoContextFactory,
            _documentUrlService,
            _documentNavigationQueryService,
            _languageService,
            _hostUrlProvider);
    }

    private void ConfigureHostUrls(params HostUrl[] hostUrls)
    {
        _hostUrlProvider.GetHostUrlsAsync().Returns(Task.FromResult<IEnumerable<HostUrl>>(hostUrls));
    }

    private void ConfigureNavigationRoots(params IPublishedContent[] roots)
    {
        _documentNavigationQueryService.TryGetRootKeys(out Arg.Any<IEnumerable<Guid>>())
            .Returns(callInfo =>
            {
                callInfo[0] = roots.Select(root => root.Key).ToArray();
                return true;
            });

        foreach (var root in roots)
        {
            _publishedContentCache.GetById(root.Key).Returns(root);
        }
    }

    private void ConfigureChildRoots(IPublishedContent parent, params IPublishedContent[] children)
    {
        _documentNavigationQueryService.TryGetChildrenKeys(parent.Key, out Arg.Any<IEnumerable<Guid>>())
            .Returns(callInfo =>
            {
                callInfo[1] = children.Select(child => child.Key).ToArray();
                return true;
            });

        foreach (var child in children)
        {
            _publishedContentCache.GetById(child.Key).Returns(child);
        }
    }

    private static IPublishedContent CreateContent(int id, string contentTypeAlias)
    {
        var content = Substitute.For<IPublishedContent>();
        var contentType = Substitute.For<IPublishedContentType>();
        var contentKey = Guid.NewGuid();
        content.Id.Returns(id);
        content.Key.Returns(contentKey);
        contentType.Alias.Returns(contentTypeAlias);
        content.ContentType.Returns(contentType);
        return content;
    }
}
