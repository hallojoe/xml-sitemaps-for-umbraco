using Casko.XmlSitemapsForUmbraco.Providers;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class HostUrlProviderTests
{
    private IDomainService _domainService = null!;
    private ILanguageService _languageService = null!;
    private IContentService _contentService = null!;

    [SetUp]
    public void SetUp()
    {
        _domainService = Substitute.For<IDomainService>();
        _languageService = Substitute.For<ILanguageService>();
        _contentService = Substitute.For<IContentService>();
        _languageService.GetDefaultLanguageAsync().Returns(CreateLanguage("da"));
    }

    [Test]
    public async Task GetHostUrlsAsync_NormalizesDomainNamesAndIncludesRootContent()
    {
        var rootKey = Guid.NewGuid();
        var rootContent = Substitute.For<IContent>();
        rootContent.Id.Returns(1062);
        rootContent.Key.Returns(rootKey);
        _contentService.GetById(1062).Returns(rootContent);
        _domainService.GetAllAsync(false).Returns([
            CreateDomain("https://example.com/da/", "da", rootContentId: 1062, sortOrder: 20),
            CreateDomain("example.com/en/", "en", rootContentId: 1062, sortOrder: 10),
            CreateDomain("/pl/", "pl", rootContentId: 1062, sortOrder: 30),
            CreateDomain("", null, rootContentId: 1062, sortOrder: 40)
        ]);
        var sut = CreateProvider("https://localhost:56317/root/");

        var result = (await sut.GetHostUrlsAsync()).ToList();

        Assert.That(result.Select(host => host.Uri.ToString()), Is.EqualTo(new[]
        {
            "https://example.com/en/",
            "https://example.com/da/",
            "https://localhost:56317/pl/",
            "https://localhost:56317/root/"
        }));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Culture, Is.EqualTo("en"));
            Assert.That(result[0].Id, Is.EqualTo(1062));
            Assert.That(result[0].Key, Is.EqualTo(rootKey));
            Assert.That(result[0].IsDefaultCulture, Is.False);
            Assert.That(result[1].IsDefaultCulture, Is.True);
            Assert.That(result[2].Culture, Is.EqualTo("pl"));
            Assert.That(result[3].Culture, Is.EqualTo("da"));
            Assert.That(result[3].IsDefaultCulture, Is.True);
        });
    }

    [Test]
    public async Task GetHostUrlsAsync_WhenDomainCannotCreateCompleteHostUrl_FallsBackToFirstRootContentNode()
    {
        var rootKey = Guid.NewGuid();
        var rootContent = Substitute.For<IContent>();
        rootContent.Id.Returns(1234);
        rootContent.Key.Returns(rootKey);
        var totalChildren = 1L;
        _domainService.GetAllAsync(false).Returns([
            CreateDomain("/en/", "en", rootContentId: null)
        ]);
        _contentService
            .GetPagedChildren(-1, 0, 1, out totalChildren, null, null, null, true)
            .Returns([rootContent]);
        var sut = CreateProvider("https://localhost:56317/");

        var result = (await sut.GetHostUrlsAsync()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.Uri.ToString(), Is.EqualTo("https://localhost:56317/"));
            Assert.That(result.Culture, Is.EqualTo("da"));
            Assert.That(result.Id, Is.EqualTo(1234));
            Assert.That(result.Key, Is.EqualTo(rootKey));
            Assert.That(result.IsDefaultCulture, Is.True);
        });
    }

    [Test]
    public async Task GetHostUrlsAsync_WhenNoDomainsExist_UsesFirstRootContentNode()
    {
        var rootKey = Guid.NewGuid();
        var rootContent = Substitute.For<IContent>();
        rootContent.Id.Returns(1234);
        rootContent.Key.Returns(rootKey);
        var totalChildren = 1L;
        _domainService.GetAllAsync(false).Returns([]);
        _contentService
            .GetPagedChildren(-1, 0, 1, out totalChildren, null, null, null, true)
            .Returns([rootContent]);
        var sut = CreateProvider("https://localhost:56317/");

        var result = (await sut.GetHostUrlsAsync()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.Uri.ToString(), Is.EqualTo("https://localhost:56317/"));
            Assert.That(result.Culture, Is.EqualTo("da"));
            Assert.That(result.Id, Is.EqualTo(1234));
            Assert.That(result.Key, Is.EqualTo(rootKey));
            Assert.That(result.IsDefaultCulture, Is.True);
        });
    }

    private HostUrlProvider CreateProvider(string? fallbackApplicationUrl)
    {
        return new HostUrlProvider(
            Options.Create(new WebRoutingSettings { UmbracoApplicationUrl = fallbackApplicationUrl }),
            _domainService,
            _languageService,
            _contentService);
    }

    private static IDomain CreateDomain(string domainName, string? culture, int? rootContentId, int sortOrder = 0)
    {
        var domain = Substitute.For<IDomain>();
        domain.DomainName.Returns(domainName);
        domain.LanguageIsoCode.Returns(culture);
        domain.RootContentId.Returns(rootContentId);
        domain.SortOrder.Returns(sortOrder);
        return domain;
    }

    private static ILanguage CreateLanguage(string isoCode)
    {
        var language = Substitute.For<ILanguage>();
        language.IsoCode.Returns(isoCode);
        return language;
    }
}
