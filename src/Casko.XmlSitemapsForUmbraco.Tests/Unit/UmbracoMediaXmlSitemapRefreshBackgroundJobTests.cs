using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class UmbracoMediaXmlSitemapRefreshBackgroundJobTests
{
    [Test]
    public void Delay_IsTenSeconds()
    {
        var sut = CreateJob(new XmlSitemapsOptions(), Substitute.For<IXmlSitemapStorageRefreshService>());

        Assert.That(sut.Delay, Is.EqualTo(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void Period_UsesConfiguredIntervalSeconds()
    {
        var sut = CreateJob(new XmlSitemapsOptions
        {
            Storage = new XmlSitemapStorageOptions
            {
                BackgroundJob = new XmlSitemapStorageBackgroundJobOptions
                {
                    IntervalSeconds = 120
                }
            }
        }, Substitute.For<IXmlSitemapStorageRefreshService>());

        Assert.That(sut.Period, Is.EqualTo(TimeSpan.FromSeconds(120)));
    }

    [Test]
    public void ServerRoles_UseRecurringBackgroundJobDefaults()
    {
        var sut = CreateJob(new XmlSitemapsOptions(), Substitute.For<IXmlSitemapStorageRefreshService>());

        Assert.That(sut.ServerRoles, Is.EqualTo(IRecurringBackgroundJob.DefaultServerRoles));
    }

    [Test]
    public async Task RunJobAsync_WhenDisabled_DoesNotRefresh()
    {
        var refreshService = Substitute.For<IXmlSitemapStorageRefreshService>();
        var sut = CreateJob(new XmlSitemapsOptions
        {
            Storage = new XmlSitemapStorageOptions
            {
                BackgroundJob = new XmlSitemapStorageBackgroundJobOptions
                {
                    Enabled = false
                }
            }
        }, refreshService);

        await sut.RunJobAsync();

        await refreshService.DidNotReceive().RefreshAllAsync();
    }

    [Test]
    public async Task RunJobAsync_WhenEnabled_RefreshesAllConfiguredDocuments()
    {
        var refreshService = Substitute.For<IXmlSitemapStorageRefreshService>();
        var sut = CreateJob(new XmlSitemapsOptions(), refreshService);

        await sut.RunJobAsync();

        await refreshService.Received(1).RefreshAllAsync();
    }

    private static UmbracoMediaXmlSitemapRefreshBackgroundJob CreateJob(
        XmlSitemapsOptions options,
        IXmlSitemapStorageRefreshService refreshService)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => refreshService);
        var serviceProvider = services.BuildServiceProvider();

        return new UmbracoMediaXmlSitemapRefreshBackgroundJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(options),
            Substitute.For<ILogger<UmbracoMediaXmlSitemapRefreshBackgroundJob>>());
    }
}
