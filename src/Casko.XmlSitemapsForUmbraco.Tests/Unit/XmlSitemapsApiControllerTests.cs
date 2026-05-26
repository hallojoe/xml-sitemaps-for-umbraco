using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Package.Controllers;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class XmlSitemapsApiControllerTests
{
    [Test]
    public void GetConfiguration_ReturnsConfiguredOptions()
    {
        var controller = new XmlSitemapsApiController(Options.Create(new XmlSitemapsOptions
        {
            Enabled = false,
            Sitemaps =
            {
                ["xmlsitemap"] = new SitemapOptions()
            }
        }));

        var result = controller.GetConfiguration();

        Assert.Multiple(() =>
        {
            Assert.That(result.Value?.Enabled, Is.False);
            Assert.That(result.Value?.SitemapCount, Is.EqualTo(1));
        });
    }
}
