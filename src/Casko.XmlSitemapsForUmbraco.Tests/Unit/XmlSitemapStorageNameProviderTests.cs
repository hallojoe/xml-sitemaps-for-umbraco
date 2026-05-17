using System.ComponentModel.DataAnnotations;
using Casko.XmlSitemapsForUmbraco.Storage;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class XmlSitemapStorageNameProviderTests
{
    private readonly XmlSitemapStorageNameProvider _sut = new();

    [Test]
    public void GetFileName_WhenSitemapHasHostAndAlias_UsesHostAndAlias()
    {
        var result = _sut.GetFileName(new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.Sitemap,
            "Products Sitemap",
            "https://www.example.com/da"));

        Assert.That(result, Is.EqualTo("sitemap--www-example-com--products-sitemap.xml"));
    }

    [Test]
    public void GetFileName_WhenSitemapIndexHasHostAndAlias_UsesIndexPrefix()
    {
        var result = _sut.GetFileName(new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.SitemapIndex,
            "Main Index",
            "www.example.com"));

        Assert.That(result, Is.EqualTo("sitemap-index--www-example-com--main-index.xml"));
    }

    [Test]
    public void GetFileName_WhenSameAliasUsesDifferentHosts_ReturnsDifferentFileNames()
    {
        var first = _sut.GetFileName(new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.Sitemap,
            "products",
            "www.example.com"));
        var second = _sut.GetFileName(new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.Sitemap,
            "products",
            "shop.example.com"));

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void GetFileName_WhenHostIsMissing_UsesDefaultHostSegment()
    {
        var result = _sut.GetFileName(new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.Sitemap,
            "products",
            null));

        Assert.That(result, Is.EqualTo("sitemap--default--products.xml"));
    }

    [Test]
    public void GetFileName_WhenAliasIsBlank_ThrowsValidationException()
    {
        TestDelegate action = () => _sut.GetFileName(new XmlSitemapStorageKey(
            XmlSitemapDocumentKind.Sitemap,
            " ",
            "www.example.com"));

        Assert.That(action, Throws.TypeOf<ValidationException>());
    }
}
