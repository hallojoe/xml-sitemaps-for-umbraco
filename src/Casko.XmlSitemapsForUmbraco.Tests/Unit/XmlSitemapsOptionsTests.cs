using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class XmlSitemapsOptionsTests
{
    [Test]
    public void Defaults_UseSingleMode()
    {
        var options = new XmlSitemapsOptions();

        Assert.Multiple(() =>
        {
            Assert.That(options.Mode, Is.EqualTo(XmlSitemapsMode.Single));
            Assert.That(options.RootNodeMode, Is.EqualTo(XmlSitemapsMode.Single));
        });
    }

    [Test]
    public void Bind_WhenModeIsConfigured_BindsCanonicalModeProperty()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["XmlSitemaps:Mode"] = "Configuration"
            })
            .Build();
        var options = new XmlSitemapsOptions();

        configuration.GetSection(XmlSitemapsOptions.Key).Bind(options);

        Assert.Multiple(() =>
        {
            Assert.That(options.Mode, Is.EqualTo(XmlSitemapsMode.Configuration));
            Assert.That(options.RootNodeMode, Is.EqualTo(XmlSitemapsMode.Configuration));
        });
    }

    [Test]
    public void Bind_WhenRootNodeModeIsConfigured_BindsCompatibilityAlias()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["XmlSitemaps:RootNodeMode"] = "Configuration"
            })
            .Build();
        var options = new XmlSitemapsOptions();

        configuration.GetSection(XmlSitemapsOptions.Key).Bind(options);

        Assert.Multiple(() =>
        {
            Assert.That(options.Mode, Is.EqualTo(XmlSitemapsMode.Configuration));
            Assert.That(options.RootNodeMode, Is.EqualTo(XmlSitemapsMode.Configuration));
        });
    }
}
