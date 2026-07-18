using System.Text;
using Casko.XmlSitemapsForUmbraco.Http;
using Casko.XmlSitemapsForUmbraco.Models;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class XmlResultTests
{
    [Test]
    public async Task ExecuteAsync_WhenXmlSiteMapContainsCultureLinks_RendersXhtmlPrefix()
    {
        var httpContext = new DefaultHttpContext();
        await using var responseStream = new MemoryStream();
        httpContext.Response.Body = responseStream;

        var result = new XmlResult<XmlSiteMap>(new XmlSiteMap
        {
            Urls =
            [
                new XmlSiteMapUrl
                {
                    Location = "https://www.example.com/",
                    CultureLinks =
                    [
                        new XHtmlLink
                        {
                            Href = "https://www.example.com/da/",
                            HrefLang = "da"
                        }
                    ]
                }
            ]
        });

        await result.ExecuteAsync(httpContext);

        var xml = Encoding.UTF8.GetString(responseStream.ToArray());

        Assert.That(httpContext.Response.ContentType, Is.EqualTo(Constants.XmlMimeType));
        Assert.That(xml, Does.Contain($"xmlns:{Constants.XhtmlPrefix}=\"{Constants.XhtmlNamespace}\""));
        Assert.That(xml, Does.Not.Contain($"xmlns:{Constants.ImagePrefix}="));
        Assert.That(xml, Does.Not.Contain($"xmlns:{Constants.VideoPrefix}="));
        Assert.That(xml, Does.Not.Contain($"xmlns:{Constants.NewsPrefix}="));
        Assert.That(xml, Does.Contain("<xhtml:link"));
        Assert.That(xml, Does.Not.Contain($"<link rel=\"alternate\" hreflang=\"da\" href=\"https://www.example.com/da/\" xmlns=\"{Constants.XhtmlNamespace}\""));
    }
}
