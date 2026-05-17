using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Models.Serialization;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class XmlSitemapXmlSerializerTests
{
    private readonly XmlSitemapXmlSerializer _sut = new();

    [Test]
    public void Serialize_WhenSitemapModelIsProvided_ReturnsUrlSetXml()
    {
        var result = _sut.Serialize(new XmlSiteMap
        {
            Urls =
            [
                new XmlSiteMapUrl
                {
                    Location = "https://www.example.com/"
                }
            ]
        });

        Assert.That(result, Does.Contain("<urlset"));
        Assert.That(result, Does.Contain("<loc>https://www.example.com/</loc>"));
    }

    [Test]
    public void Serialize_WhenSitemapIndexModelIsProvided_ReturnsSitemapIndexXml()
    {
        var result = _sut.Serialize(new XmlSiteMapIndex
        {
            Locations =
            [
                new XmlSiteMapIndexLocation
                {
                    Location = "https://www.example.com/sitemap.xml"
                }
            ]
        });

        Assert.That(result, Does.Contain("<sitemapindex"));
        Assert.That(result, Does.Contain("<loc>https://www.example.com/sitemap.xml</loc>"));
    }
}
